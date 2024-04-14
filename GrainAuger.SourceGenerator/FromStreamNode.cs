
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GrainAuger.SourceGenerator;

internal abstract record DagNode(
    string StreamNamespace,
    string StreamProvider,
    ITypeSymbol OutputType,
    ITypeSymbol KeyType
);

internal record FromStreamNode(
    string StreamNamespace,
    string StreamProvider,
    ITypeSymbol OutputType,
    ITypeSymbol KeyType
) : DagNode(StreamNamespace, StreamProvider, OutputType, KeyType);

internal record ProcessNode(
    DagNode PreviousNode,
    ImmutableArray<ITypeSymbol> AugerTypes,
    string StreamNamespace
) : DagNode(
    StreamNamespace,
    PreviousNode.StreamProvider,
    AugerTypes.First(),
    AugerTypes.First()
);

internal record Processor(
    ITypeSymbol ProcessorType,
    ITypeSymbol InputType,
    ITypeSymbol OutputType
    );