using Microsoft.CodeAnalysis;

namespace GrainAuger.SourceGenerator;

public static class GrainAugerDiagnostic
{
    public static Diagnostic MissingMethod(Location location) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA001", 
            "Missing method",
            "Statement must either be 'FromStream<T, TKey>' or 'Process<T...>'.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location
    );
    
    public static Diagnostic InvalidMethod(Location location, string foundMethodName) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA002", 
            "Invalid method",
            "Method '{0}' is not allowed in this context. Use 'FromStream<T, TKey>' or 'Process<T...>' instead.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location, foundMethodName
    );
    
    public static Diagnostic MissingOutputVariable(Location location) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA003", 
            "Missing output variable",
            "Method must assign the output of 'FromStream<T, TKey>' or 'Process<T...>' to a variable.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location
    );

    public static Diagnostic WrongKeyType(Location location, string foundType) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA004", 
            "Invalid key type",
            "Key type '{0}' is not allowed. Use long, string or Guid.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location, foundType
    );
    
    public static Diagnostic AugerCannotInferConstructor(Location location) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA005", 
            "Invalid Auger type",
            "Auger constructor cannot be inferred. Auger types must have exactly one public constructor with IAsyncObserver<T> parameter or be directly derived from one of the provided LoadBalancers.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location
    );
    
    public static Diagnostic AugerOutputInputTypesMismatch(Location location, string outputType, string inputType) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA006", 
            "Output/Input type mismatch",
            "Auger output type '{0}' does not match input type '{1}'.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location, outputType, inputType
    );
    
    public static Diagnostic AugerUsesGeneric(Location location, string foundType) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA007", 
            "Auger uses generic type",
            "Auger type '{0}' cannot be generic. Use a concrete type instead.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location, foundType
    );
    
    public static Diagnostic LoadBalancerUsesUnknownParameter(Location location, string foundType) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA008", 
            "Invalid parameter",
            "Parameter '{0}' is not allowed. Load balancers can use a IStreamProvider and a string for output namespace.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location, foundType
    );
    
    public static Diagnostic ChainedMethodCalls(Location location, string foundMethodName) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "GA009", 
            "Chained method calls",
            "Method '{0}' must be assigned to a variable. Chained method calls are not allowed.",
            "GrainAuger",
            DiagnosticSeverity.Error, true),
        location, foundMethodName
    );
}