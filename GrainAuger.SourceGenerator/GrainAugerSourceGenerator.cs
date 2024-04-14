using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Threading;
using GrainAuger.Abstractions;
using Microsoft.CodeAnalysis.Text;

#nullable enable

namespace GrainAuger.SourceGenerator;

[Generator]
public class GrainAugerSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter the methods that use AugerJobConfigurationAttribute
        var methodProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
                GetMethodDeclarationForSourceGen)
            .Where(t => t is { jobConfigurationAttributeFound: true, syntax: not null })
            .Select((t, _) => t.syntax);
        
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(methodProvider.Collect()),
            (ctx, t) => GenerateJobs(ctx, t.Left, t.Right));
    }

    private (MethodDeclarationSyntax syntax, bool jobConfigurationAttributeFound)
        GetMethodDeclarationForSourceGen(GeneratorSyntaxContext context, CancellationToken token)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in methodDeclarationSyntax.AttributeLists)    
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }
                
                var attributeName = attributeSymbol.ContainingType.ToDisplayString();
                
                if (attributeName == "GrainAuger.Abstractions.AugerJobConfigurationAttribute")
                {
                    return (methodDeclarationSyntax, true);
                }
            }
        }

        return (methodDeclarationSyntax, false);
    }

    private void GenerateJobs(SourceProductionContext context, Compilation compilation,
        ImmutableArray<MethodDeclarationSyntax> methodDeclarations)
    {
        foreach (var methodDeclaration in methodDeclarations)
        {
            GenerateJob(context, compilation, methodDeclaration);
        }
    }

    private void GenerateJob(SourceProductionContext context, Compilation compilation,
        MethodDeclarationSyntax methodDeclaration)
    {
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
        
        if (semanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
        {
            return;
        }
        
        var namespaceName = $"GrainAugerCodeGen.{GetNamespaceName(methodSymbol)}";
        var jobName = GetJobNameFromAttribute(methodSymbol);
        
        var grainCodes = new List<string>();
        var statements = GetStatements(methodDeclaration.Body!);
        var constructors = new List<string>();

        var dag = new Dictionary<string, DagNode>();

        foreach (var statement in statements)
        {
            // we want only statements of form:
            // Using the method Process or FromStream:
            // var inputStream = builder.FromStream<CardTransaction>("AugerStreamProvider", "input");
            // IAugerStream overLimitStream = inputStream.Process<OverLimitDetector, OverLimitDetector>("overLimitStream");
            // var expiredCardStream = inputStream.Process<ExpiredCardDetector>("expiredCardStream");
            // if statement is other than invocation of the Process method or FromStream method
            // put a warning in the analyzer
            
            // Given example statement:
            // IAugerStream overLimitStream = inputStream.Process<OverLimitDetector, OverLimitDetector>("overLimitStream");
            // I want to display the following code:
            // inputStream -[OverLimitDetector, OverLimitDetector]-> overLimitStream
            // so I want to get the generic types of the Process method as well as names of the variables
            var invocation = statement.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation is null)
            {
                continue;
            }
            
            var genericTypes = GetGenericTypes(invocation, semanticModel);
            
            var localDeclarationStatement = (LocalDeclarationStatementSyntax)statement;

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                var name = memberAccessExpression.Name.Identifier.ToString();
                if (name == "Process")
                {
                    var inputName = memberAccessExpression.Expression.ToString();
                    var outputName = localDeclarationStatement.Declaration.Variables.First().Identifier.ToString();
                    var constructorsMerged = genericTypes
                        .Select(genericType => GetConstructors(genericType, semanticModel));
            
                    constructors.AddRange(constructorsMerged.SelectMany(c => c).Select(c => c.ToDisplayString()));
                    grainCodes.Add($"{inputName} -[{string.Join(", ", genericTypes)}]-> {outputName}");
                    
                    var arguments = invocation.ArgumentList.Arguments;
                    
                    var streamNamespace = arguments[0].Expression.ToString();
                    
                    dag.Add(outputName, new ProcessNode(
                        dag[inputName],
                        genericTypes,
                        streamNamespace));
                }
                else if (name == "FromStream")
                {
                    var outputName = localDeclarationStatement.Declaration.Variables.First().Identifier.ToString();
                    grainCodes.Add($"Foreign Source <{genericTypes.First()}> -> {outputName}");
                    // get arguments of the FromStream method

                    var arguments = invocation.ArgumentList.Arguments;
                    
                    var streamProvider = arguments[0].Expression.ToString();
                    var streamNamespace = arguments[1].Expression.ToString();
                    
                    dag.Add(outputName, new FromStreamNode(
                        streamNamespace,
                        streamProvider,
                        genericTypes.First(), 
                        genericTypes.Last()
                        ));
                }
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("GA002", "Invalid statement", "Expected to find Process or FromStream method", "GrainAuger",
                        DiagnosticSeverity.Error, true),
                    statement.GetLocation()));
            }
        }
        
        var processGrainCodes = dag
            .Where(kvp => kvp.Value is ProcessNode)
            .Select(kvp => GenerateProcessGrainCode(kvp.Key, (ProcessNode)kvp.Value));

        var code = $$"""
        // <auto-generated/>
                
        namespace {{namespaceName}};
        
        /*
        Found Dag for job {{jobName}}:
        {{string.Join("\n", grainCodes)}}
        */
        
        /* 
        Found constructors:
        {{string.Join("\n", constructors)}}
        */
        
        {{string.Join("\n\n", processGrainCodes)}}
        """;
        
        context.AddSource($"{jobName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateProcessGrainCode(string keyName, ProcessNode node)
    {
        var grainType = "String";
        
        return $$"""
        [global::Orleans.ImplicitStreamSubscription({{node.PreviousNode.StreamNamespace}})]
        file class {{keyName}} : 
            global::Orleans.Grain,
            global::Orleans.IGrainWith{{grainType}}Key,
            global::Orleans.Streams.IAsyncObserver<{{node.PreviousNode.OutputType}}>
        {
            private readonly global::Microsoft.Extensions.Logging.ILogger<{{keyName}}> _logger;
            private global::Orleans.Streams.IAsyncStream<global::{{node.OutputType}}> _outputStream;
            
            public override async Task OnActivateAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Activating...");
                
                await base.OnActivateAsync(cancellationToken);
                
                var inputStreamProvider = this.GetStreamProvider({{node.PreviousNode.StreamProvider}});
                var inputStreamId = global::Orleans.Runtime.StreamId.Create({{node.PreviousNode.StreamNamespace}}, this.GetPrimaryKey{{grainType}}());
                var inputStream = inputStreamProvider.GetStream<{{node.PreviousNode.OutputType}}>(inputStreamId);
                
                var outputStreamProvider = this.GetStreamProvider({{node.StreamProvider}});
                var outputStreamId = global::Orleans.Runtime.StreamId.Create({{node.StreamNamespace}}, this.GetPrimaryKey{{grainType}}());
                _outputStream = outputStreamProvider.GetStream<global::{{node.OutputType}}>(outputStreamId);
                
                await inputStream.SubscribeAsync(this);
                
                _logger.LogInformation("Activated");
            }
            
            public async Task OnNextAsync({{node.PreviousNode.OutputType}} item, global::Orleans.Streams.StreamSequenceToken token = null)
            {
                _logger.LogInformation("Processing {item}", item);
                // chain the detectors
                // await new {{node.OutputType}}().ProcessAsync(item, 
                //    async alert =>
                //    {
                //        
                //    });
            }
            
            public Task OnCompletedAsync()
            {
                return Task.CompletedTask;
            }
            
            public async Task OnErrorAsync(Exception ex)
            {
                // push the exception to the output stream
                await _outputStream.OnErrorAsync(ex);
            }
        }
        """;
    }
    
    private static string GetNamespaceName(ISymbol methodSymbol)
    {
        var namespaceName = methodSymbol.ContainingNamespace.ToDisplayString();
        return namespaceName;
    }
    
    private static string GetJobNameFromAttribute(ISymbol methodSymbol)
    {
        var attributes = methodSymbol.GetAttributes();
        // check also if the namespace is correct
        var attribute = attributes.FirstOrDefault(a => a.AttributeClass!
            .ToDisplayString() == "GrainAuger.Abstractions.AugerJobConfigurationAttribute");
        // it can either be keyed by the name or by the index (0)
        if (attribute?.NamedArguments.FirstOrDefault(a => a.Key == "JobName").Value.Value is string jobName)
        {
            return jobName;
        }
        
        if (attribute?.ConstructorArguments.FirstOrDefault().Value is string jobNameFromIndex)
        {
            return jobNameFromIndex;
        }
        
        return "UnknownJobName";
    }
    
    // Get statements from the method
    private static IEnumerable<StatementSyntax> GetStatements(SyntaxNode methodDeclaration)
    {
        var statements = methodDeclaration.DescendantNodes().OfType<StatementSyntax>();
        return statements;
    }
    
    private static ImmutableArray<ITypeSymbol> GetGenericTypes(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
        return symbol is not IMethodSymbol method ? [] : method.TypeArguments;
    }
    
    private static ImmutableArray<IMethodSymbol> GetConstructors(ITypeSymbol genericType, SemanticModel semanticModel)
    {
        var constructors = genericType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor)
            .ToImmutableArray();
        return constructors;
    }
}
