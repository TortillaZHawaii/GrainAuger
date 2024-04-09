﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Threading;
using GrainAuger.Abstractions;
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
            (ctx, t) => GenerateGrains(ctx, t.Left, t.Right));
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

    private void GenerateGrains(SourceProductionContext context, Compilation compilation,
        ImmutableArray<MethodDeclarationSyntax> methodDeclarations)
    {
        foreach (var methodDeclaration in methodDeclarations)
        {
            GenerateGrain(context, compilation, methodDeclaration);
        }
    }

    private void GenerateGrain(SourceProductionContext context, Compilation compilation,
        MethodDeclarationSyntax methodDeclaration)
    {
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
        
        if (semanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
        {
            return;
        }
        
        var namespaceName = methodSymbol.ContainingNamespace.ToDisplayString();
        var attributes = methodSymbol.GetAttributes();
        // find one attribute with the name AugerJobConfigurationAttribute
        var attribute = attributes.FirstOrDefault(a => a.AttributeClass?.Name == "AugerJobConfigurationAttribute");
        // get the job name property value from the attribute
        var jobName = attribute?.NamedArguments.FirstOrDefault(a => a.Key == "JobName").Value.Value?.ToString() ?? "UnknownJobName";

        // go over each statement in the method
        DataFlowAnalysis analysis = semanticModel.AnalyzeDataFlow(methodDeclaration.Body);
            
        var outputStreamName = "GrainAuger_ExpiredCardDetector_Output";
        var grainKeyType = "String";
        var inputType = "GrainAuger.Examples.FraudDetection.WebApi.Dtos.CardTransaction";
        
        List<string> privateFields = new();
        List<string> constructorParameters = new();
        List<string> constructorAssignments = new();
        
        var code = $$"""
        // <auto-generated/>
        using Orleans.Runtime;
        using Orleans.Streams;
        using GrainAuger.Core;
                
        namespace {{namespaceName}};
        
        [ImplicitStreamSubscription("{{outputStreamName}}")]
        public class {{outputStreamName}}
            : Grain,
            IGrainWith{{grainKeyType}}Key,
            IAsyncObserver<{{inputType}}>
        {
            // Dependencies
            {{string.Join("/r/n", privateFields)}}
        
            public {{outputStreamName}}(
                {{string.Join(", ", constructorParameters)}}
                )
            {
                GrainContext grainContext = new GrainContext(RegisterTimer);
                {{string.Join("/r/n", constructorAssignments)}}
            }
            
            public override async Task OnActivateAsync(CancellationToken cancellationToken)
            {
                await base.OnActivateAsync(cancellationToken);
                
            }
            
            public async Task OnNextAsync({{inputType}} item, StreamSequenceToken token = null)
            {
                // Process the item
            }
            
            public Task OnCompletedAsync()
            {
                return Task.CompletedTask;
            }
            
            public Task OnErrorAsync(Exception ex)
            {
                return Task.CompletedTask;
            }
        }
        """;
        
        context.AddSource($"{outputStreamName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
