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
            if (node is ClassDeclarationSyntax interfaceDeclaration)
            {
                if (interfaceDeclaration.BaseList?.Types.Any(x => x.Type.ToString() == "IAugerJobConfiguration") == true)
                {
                    return true;
                }
            }

            return false;
        }, (generatorContext, cancelletionToken) => { return "Found"; });
    }
}
