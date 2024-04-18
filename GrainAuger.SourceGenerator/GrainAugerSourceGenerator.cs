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
            .Select(kvp => GenerateProcessGrainCode(kvp.Key, (ProcessNode)kvp.Value, semanticModel));

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

    private static string GenerateProcessGrainCode(string keyName, ProcessNode node, SemanticModel semanticModel)
    {
        var grainType = "String";

        Dictionary<string, string> insertedVariables = new();
        int variableCounter = 0;
        
        List<string> processorDefinitions = new();
        List<string> processorConstructors = new();
        List<string> processorOnNextCalls = new();
        
        insertedVariables.Add($"global::Microsoft.Extensions.Logging.ILogger<{keyName}>", "logger");

        var inputType = GetGlobalTypeName(node.PreviousNode.OutputType);
        string outputType = "";
        string firstProcessorName = "";
        
        foreach (var augerType in node.AugerTypes)
        {
            var constructors = GetConstructors(augerType, semanticModel);
            if (constructors.Count() != 1)
            {
                continue;
            }
            
            var constructor = constructors.First();
            var parameters = constructor.Parameters;
            var processorVariableName = $"_processor{variableCounter++}";
            var paramStrings = new List<string>();
            if (firstProcessorName == "")
            {
                firstProcessorName = processorVariableName;
            }
            
            foreach (var parameter in parameters)
            {
                var paramKey = GetGlobalTypeName(parameter.Type);
                
                if (parameter.Type.OriginalDefinition.ToDisplayString() == "Orleans.Streams.IAsyncObserver<T>")
                {
                    var outputTypeSymbol = parameter.Type as INamedTypeSymbol;
                    outputType = GetGlobalTypeName(outputTypeSymbol!.TypeArguments.First());
                    paramStrings.Add("_outputStream");
                }
                else
                {
                    if (insertedVariables.TryGetValue(paramKey, out var variableName))
                    {
                        paramStrings.Add(variableName);
                    }
                    else
                    {
                        variableName = $"v{variableCounter++}";
                        insertedVariables.Add(paramKey, variableName);
                        paramStrings.Add(variableName);
                    }
                }
            }

            var globalAuger = GetGlobalTypeName(augerType);
            processorDefinitions.Add($"private readonly {globalAuger} {processorVariableName};");
            processorConstructors.Add($"{processorVariableName} = new {globalAuger}({string.Join(", ", paramStrings)});");
            variableCounter++;
        }

        return $$"""
        [global::Orleans.ImplicitStreamSubscription({{node.PreviousNode.StreamNamespace}})]
        internal class {{keyName}} :
            global::Orleans.Grain,
            global::Orleans.IGrainWith{{grainType}}Key,
            global::Orleans.Streams.IAsyncObserver<{{inputType}}>
        {
            private readonly global::Microsoft.Extensions.Logging.ILogger<{{keyName}}> _logger;
            private global::Orleans.Streams.IAsyncStream<{{outputType}}> _outputStream;
            {{string.Join("\n\t", processorDefinitions)}}
            
            internal {{keyName}}(
                {{string.Join(",\n\t\t", insertedVariables.Select(kvp => $"{kvp.Key} {kvp.Value}"))}}
                )
            {
                _logger = logger;
                {{string.Join("\n\t\t", processorConstructors)}}
            }
            
            public override async Task OnActivateAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Activating...");
                
                await base.OnActivateAsync(cancellationToken);
                
                var inputStreamProvider = this.GetStreamProvider({{node.PreviousNode.StreamProvider}});
                var inputStreamId = global::Orleans.Runtime.StreamId.Create({{node.PreviousNode.StreamNamespace}}, this.GetPrimaryKey{{grainType}}());
                var inputStream = inputStreamProvider.GetStream<{{inputType}}>(inputStreamId);
                
                var outputStreamProvider = this.GetStreamProvider({{node.StreamProvider}});
                var outputStreamId = global::Orleans.Runtime.StreamId.Create({{node.StreamNamespace}}, this.GetPrimaryKey{{grainType}}());
                _outputStream = outputStreamProvider.GetStream<{{outputType}}>(outputStreamId);
                
                await inputStream.SubscribeAsync(this);
                
                _logger.LogInformation("Activated");
            }
            
            public async Task OnNextAsync({{inputType}} item, global::Orleans.Streams.StreamSequenceToken token = null)
            {
                _logger.LogInformation("Processing {item}", item);
                await {{firstProcessorName}}.OnNextAsync(item, token);                
            }
            
            public async Task OnCompletedAsync()
            {
                await {{firstProcessorName}}.OnCompletedAsync();
            }
            
            public async Task OnErrorAsync(Exception ex)
            {
                _logger.LogError(ex, "Error occurred");
                await {{firstProcessorName}}.OnErrorAsync(ex);
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

    private static string GetGlobalTypeName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            ));
    }
}
