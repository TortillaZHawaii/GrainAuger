using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace GrainAuger.SourceGenerator;

[Generator]
public class GrainAugerSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter the classes that implement IAugerJobConfiguration
        context.SyntaxProvider
            .CreateSyntaxProvider((node, token) =>
            {
                if (node is not ClassDeclarationSyntax classDeclaration) return false;
                
                return classDeclaration.BaseList?.Types.Any(x =>
                    x.Type.ToString() == "IAugerJobConfiguration") == true;
            }, 
                (generatorContext, _) =>
            {
                var classDeclaration = (ClassDeclarationSyntax)generatorContext.Node;
                var semanticModel = generatorContext.SemanticModel;
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                var namespaceName = classSymbol?.ContainingNamespace.ToDisplayString();

                return (classDeclaration, namespaceName);
            });
    }
}
