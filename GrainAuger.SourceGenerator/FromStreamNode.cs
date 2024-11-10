using System.Collections.Immutable;
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
    ITypeSymbol OutputType,
    string StreamNamespace
) : DagNode(
    StreamNamespace,
    PreviousNode.StreamProvider,
    OutputType,
    PreviousNode.KeyType
);






