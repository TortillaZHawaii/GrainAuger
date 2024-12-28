using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GrainAuger.SourceGenerator;

internal abstract record DagNode(
    string StreamNamespace,
    string StreamProvider,
    ITypeSymbol OutputType,
    ITypeSymbol OutputKeyType
);

internal record FromStreamNode(
    string StreamNamespace,
    string StreamProvider,
    ITypeSymbol OutputType,
    ITypeSymbol OutputKeyType
) : DagNode(StreamNamespace, StreamProvider, OutputType, OutputKeyType);

internal record ProcessNode(
    DagNode PreviousNode,
    ImmutableArray<ITypeSymbol> AugerTypes,
    ITypeSymbol OutputType,
    ITypeSymbol OutputKeyType,
    string StreamNamespace,
    string StreamProvider = ""
) : DagNode(
    StreamNamespace,
    StreamProvider == "" ? PreviousNode.StreamProvider : StreamProvider,
    OutputType,
    OutputKeyType
);
