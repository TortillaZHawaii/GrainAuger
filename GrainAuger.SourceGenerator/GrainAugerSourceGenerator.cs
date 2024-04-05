﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace GrainAuger.SourceGenerator;

[Generator]
public class GrainAugerSourceGenerator : IIncrementalGenerator
{
    private const string AttributeSourceCode = $$"""
        // <auto-generated/>
        #nullable enable
        
        namespace GrainAuger.Abstractions;
        
        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class AugerJobConfigurationAttribute : System.Attribute
        {
            public string JobName { get; }
            public string JobDescription { get; }
            public string JobType { get; }
            public string JobVersion { get; }
            
            public AugerJobConfigurationAttribute(
                string jobName,
                string jobDescription,
                string jobType,
                string jobVersion
                )
            {
                JobName = jobName;
                JobDescription = jobDescription;
                JobType = jobType;
                JobVersion = jobVersion;
            }
        }
        """;
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "AugerJobConfigurationAttribute.g.cs",
            SourceText.From(AttributeSourceCode, Encoding.UTF8)));
        
        // Filter the methods that implement IAugerJobConfiguration
        var methods = context.SyntaxProvider
            .CreateSyntaxProvider(
                (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
                GetMethodDeclarationForSourceGen)
            .Where(t => t.jobConfigurationAttributeFound)
            .Select((t, _) => t.syntax);
        
        // context.RegisterSourceOutput(context.CompilationProvider.Combine(methods.Collect()),
        //     (ctx, t) => GenerateGrain(ctx, t.Left, t.Right));
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

    private void GenerateGrain(SourceProductionContext context, Compilation compilation,
        ImmutableArray<MethodDeclarationSyntax> methodDeclarations)
    {
        var outputStreamName = "GrainAuger_ExpiredCardDetector_Output";
        var namespaceName = "GrainAuger_vXXX_EntryPoint_Output";
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
