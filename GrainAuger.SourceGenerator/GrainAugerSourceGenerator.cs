using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Threading;
using GrainAuger.Abstractions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

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
                if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
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
        List<SyntaxTree> syntaxTrees = new()
        {
            Capacity = methodDeclarations.Length
        };
        
        foreach (var methodDeclaration in methodDeclarations)
        {
            var tree = GenerateJob(context, compilation, methodDeclaration);
            if (tree is not null)
            {
                syntaxTrees.Add(tree);
            }
        }

        RunOrleansSourceGeneration(context, compilation, "", syntaxTrees.ToArray());
    }

    private SyntaxTree? GenerateJob(SourceProductionContext context, Compilation compilation,
        MethodDeclarationSyntax methodDeclaration)
    {
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
        
        if (ModelExtensions.GetDeclaredSymbol(semanticModel, methodDeclaration) is not IMethodSymbol methodSymbol)
        {
            return null;
        }
        
        var namespaceName = $"GrainAugerCodeGen.{GetNamespaceName(methodSymbol)}";
        var jobName = GetJobNameFromAttribute(methodSymbol);
        
        var grainCodes = new List<string>();
        var statements = GetStatements(methodDeclaration.Body!);

        var dag = new Dictionary<string, DagNode>();
        bool hasErrors = false;

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
            var invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
            if (invocations.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("GA002", "Invalid statement", "Expected to find Process or FromStream method", "GrainAuger",
                        DiagnosticSeverity.Error, true),
                    statement.GetLocation()));
                hasErrors = true;
                continue;
            }

            var methodNames = new HashSet<string>();
            foreach (var invocation in invocations)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    var name = memberAccess.Name.Identifier.ToString();
                    methodNames.Add(name);
                }
            }
            
            
            if (statement is not LocalDeclarationStatementSyntax localDeclarationStatement)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("GA00Y", "Invalid statement", "Expected to find Process or FromStream method", "GrainAuger",
                        DiagnosticSeverity.Error, true),
                    statement.GetLocation()));
                hasErrors = true;
                continue;
            }

            if (methodNames.Contains("FromStream"))
            {
                if (methodNames.Count != 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("GA00Y", "Invalid statement", "Expected to find Process or FromStream method", "GrainAuger",
                            DiagnosticSeverity.Error, true),
                        statement.GetLocation()));
                    hasErrors = true;
                    continue;
                }
                
                var invocation = invocations.First();
                var genericTypes = GetGenericTypes(invocation, semanticModel);
                
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
            else if (methodNames.Contains("Process"))
            {
                var memberAccess = invocations[0].Expression as MemberAccessExpressionSyntax;
                var inputName = memberAccess!.Expression.ToString();
                if (localDeclarationStatement.Declaration.Variables.Count != 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("GA00X", "Invalid statement",
                            "Process should have a variable on the left side.", "GrainAuger",
                            DiagnosticSeverity.Error, true),
                        localDeclarationStatement.GetLocation()));
                    hasErrors = true;
                    continue;
                }

                var outputName = localDeclarationStatement.Declaration.Variables.First().Identifier.ToString();

                var processInvocation = invocations.First();
                var genericTypes = GetGenericTypes(processInvocation, semanticModel);
                grainCodes.Add($"{inputName} -[{string.Join(", ", genericTypes)}]-> {outputName}");

                var constructors = GetPublicConstructors(genericTypes.Last());
                if (constructors.Count() != 1)
                {
                    // put a warning in the analyzer
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("GA003", "Invalid constructors",
                            "Auger should have exactly one public constructor", "GrainAuger",
                            DiagnosticSeverity.Error, true),
                        genericTypes.Last().OriginalDefinition.Locations.First()));
                    hasErrors = true;
                    continue;
                }

                var output = constructors.First().Parameters
                    .Where(p => p.Type.OriginalDefinition.ToDisplayString() == "Orleans.Streams.IAsyncObserver<T>")
                    .Select(p => p.Type as INamedTypeSymbol)
                    .Select(p => p!.TypeArguments.First())
                    .FirstOrDefault();

                if (output is null)
                {
                    // put a warning in the analyzer
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("GA004", "Invalid output",
                            "Auger should have exactly one public constructor with IAsyncObserver<T> parameter",
                            "GrainAuger",
                            DiagnosticSeverity.Error, true),
                        genericTypes.Last().OriginalDefinition.Locations.First()));
                    hasErrors = true;
                    continue;
                }

                dag.Add(outputName, new ProcessNode(
                    dag[inputName],
                    genericTypes,
                    output,
                    $"\"{outputName}\""));
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("GA002", "Invalid statement",
                        "Expected to find Process or FromStream method", "GrainAuger",
                        DiagnosticSeverity.Error, true),
                    localDeclarationStatement.GetLocation()));
                hasErrors = true;
            }
        }
        
        if (hasErrors)
        {
            return null;
        }
        
        var processGrainCodes = dag
            .Where(kvp => kvp.Value is ProcessNode)
            .Select(kvp => GenerateProcessGrainCode(kvp.Key, (ProcessNode)kvp.Value));

        var code = $$"""
        // <auto-generated/>
        #nullable enable
                
        namespace {{namespaceName}};
        
        /*
        Found Dag for job {{jobName}}:
        {{string.Join("\n", grainCodes)}}
        */
        
        {{string.Join("\n\n", processGrainCodes)}}
        """;

        // Normally this should be enough to generate the source code
        // But we need to make sure that the Orleans code generator is run
        // WITH the generated source code
        string hintNamePrefix = $"{namespaceName}.{jobName}";
        string hintName = $"{hintNamePrefix}.auger.g.cs";
        // context.AddSource(hintName, SourceText.From(code, Encoding.UTF8));
        
        // We can't save the file to disk
        // IO operations are not allowed in the source generator
        // RS1035: The symbol 'File' is banned for use by analyzers: Do not do file IO in analyzers
        var path = methodDeclaration.SyntaxTree.FilePath;
#pragma warning disable RS1035
        File.WriteAllText(path + ".auger.g.cs", code);
#pragma warning restore RS1035
        
        return SyntaxFactory.ParseSyntaxTree(code);
    }
    
    private void RunOrleansSourceGeneration(SourceProductionContext context, Compilation compilation, string hintName,
        params SyntaxTree[] syntaxTrees)
    {
        string assemblyName = "Orleans.CodeGenerator";
        string className = "Orleans.CodeGenerator.OrleansSerializationSourceGenerator";
        var orleansCodeGeneratorType = Type.GetType(
            $"{className}, {assemblyName}"
        );
        
        if (orleansCodeGeneratorType is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("GA001", "Orleans code generator not found", "Orleans code generator not found", "GrainAuger",
                    DiagnosticSeverity.Error, true),
                Location.None));
            return;
        }
        
        var orleansCodeGenerator = ((ISourceGenerator)Activator.CreateInstance(orleansCodeGeneratorType)!);
        var generators = new[] { orleansCodeGenerator };
        
        GeneratorRunner.Run(context, compilation, hintName, generators, syntaxTrees);
    }

    private static string GenerateProcessGrainCode(string keyName, ProcessNode node)
    {
        var grainType = "String";

        Dictionary<string, string> insertedVariables = new();
        int variableCounter = 0;
        
        List<string> processorDefinitions = new();
        List<string> processorConstructors = new();
        
        insertedVariables.Add($"global::Microsoft.Extensions.Logging.ILogger<{keyName}>", "logger");

        var inputType = GetGlobalTypeName(node.PreviousNode.OutputType);
        string outputType = "";
        string firstProcessorName = "";
        // public visibility is required for the Orleans framework to find suitable constructor
        string visibility = "public";
        
        string augerContextVariableName = "augerContext";
        string augerContextDefinition = $"global::GrainAuger.Core.AugerContext {augerContextVariableName} = new (this.RegisterTimer);";
        bool requiresAugerContext = false;
        
        var lastObserver = "_outputStream";
        
        foreach (var augerType in node.AugerTypes.Reverse())
        {
            var constructors = GetPublicConstructors(augerType);
            if (constructors.Count() != 1)
            {
                continue;
            }
            
            var constructor = constructors.First();
            var parameters = constructor.Parameters;
            var processorVariableName = $"_processor{variableCounter++}";
            var paramStrings = new List<string>();
            
            firstProcessorName = processorVariableName;
            
            foreach (var parameter in parameters)
            {
                var attributesString = parameter
                    .GetAttributes()
                    .Select(x => $"global::{x}")
                    .ToList();
                var paramKey = GetGlobalTypeName(parameter.Type);
                if (attributesString.Any())
                {
                    paramKey = $"[{string.Join(", ", attributesString)}] {paramKey}";
                }
                
                if (parameter.Type.OriginalDefinition.ToDisplayString() == "Orleans.Streams.IAsyncObserver<T>")
                {
                    var outputTypeSymbol = parameter.Type as INamedTypeSymbol;
                    outputType = GetGlobalTypeName(outputTypeSymbol!.TypeArguments.First());
                    paramStrings.Add(lastObserver);
                }
                else if (parameter.Type.OriginalDefinition.ToDisplayString() == "GrainAuger.Abstractions.IAugerContext")
                {
                    requiresAugerContext = true;
                    paramStrings.Add(augerContextVariableName);
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
            
            lastObserver = processorVariableName;
            var globalAuger = GetGlobalTypeName(augerType);
            processorDefinitions.Add($"private {globalAuger} {processorVariableName} = null!;");
            processorConstructors.Add($"{processorVariableName} = new {globalAuger}({string.Join(", ", paramStrings)});");
            variableCounter++;
        }

        return $$"""
        [global::Orleans.ImplicitStreamSubscription({{node.PreviousNode.StreamNamespace}})]
        {{visibility}} class {{keyName}}(
            {{string.Join(",\n\t", insertedVariables.Select(kvp => $"{kvp.Key} {kvp.Value}"))}}) :
            global::Orleans.Grain,
            global::Orleans.IGrainWith{{grainType}}Key,
            global::Orleans.Streams.IAsyncObserver<{{inputType}}>
        {
            private global::Orleans.Streams.IAsyncStream<{{outputType}}> _outputStream = null!;
            {{string.Join("\n\t", processorDefinitions)}}
            
            public override async Task OnActivateAsync(CancellationToken cancellationToken)
            {
                if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Information))
                {
                    global::Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(logger, "Activating...");
                }         
                       
                await base.OnActivateAsync(cancellationToken);
                
                var inputStreamProvider = this.GetStreamProvider({{node.PreviousNode.StreamProvider}});
                var inputStreamId = global::Orleans.Runtime.StreamId.Create({{node.PreviousNode.StreamNamespace}}, this.GetPrimaryKey{{grainType}}());
                var inputStream = inputStreamProvider.GetStream<{{inputType}}>(inputStreamId);
                
                var outputStreamProvider = this.GetStreamProvider({{node.StreamProvider}});
                var outputStreamId = global::Orleans.Runtime.StreamId.Create({{node.StreamNamespace}}, this.GetPrimaryKey{{grainType}}());
                _outputStream = outputStreamProvider.GetStream<{{outputType}}>(outputStreamId);
                {{(requiresAugerContext ? $"\n\t\t{augerContextDefinition}" : "")}}
                {{string.Join("\n\t\t", processorConstructors)}}
                
                await inputStream.SubscribeAsync(this);
                
                if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Information))
                {
                    global::Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(logger, "Activated");
                }
            }
            
            public async Task OnNextAsync({{inputType}} item, global::Orleans.Streams.StreamSequenceToken? token = null)
            {
                if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Information))
                {
                    global::Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(logger, "Processing {item}", item);
                }
                await {{firstProcessorName}}.OnNextAsync(item, token);                
            }
            
            public async Task OnCompletedAsync()
            {
                await {{firstProcessorName}}.OnCompletedAsync();
            }
            
            public async Task OnErrorAsync(Exception ex)
            {
                if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Error))
                {
                    global::Microsoft.Extensions.Logging.LoggerExtensions.LogError(logger, "Error occurred");
                }
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
        var symbol = ModelExtensions.GetSymbolInfo(semanticModel, invocation).Symbol;
        return symbol is not IMethodSymbol method ? [] : method.TypeArguments;
    }
    
    private static ImmutableArray<IMethodSymbol> GetPublicConstructors(ITypeSymbol genericType)
    {
        var constructors = genericType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.DeclaredAccessibility == Accessibility.Public)
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
