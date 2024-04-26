using GrainAuger.Abstractions;
using Orleans.Streams;

namespace GrainAuger.Core
{
    internal class AugerStream : IAugerStream
    {
        internal AugerStream(string providerName, string name, Type outputType)
        {
            ProviderName = providerName;
            Name = name;
            OutputType = outputType;
        }

        public string Name { get; }
        public string ProviderName { get; }
        
        public Type OutputType { get; }

        private readonly List<Type> _processors = new List<Type>();
        public IReadOnlyList<Type> Processors => _processors;
        
        public IAugerStream Process<T1>(string name)
        {
            (Type processorType, Type outputType) = GetProcessorTypes<T1>(OutputType);
            var outputStream = new AugerStream(ProviderName, name, outputType);
            outputStream._processors.Add(processorType);
            
            return outputStream;
        }

        public IAugerStream Process<T1, T2>(string name)
        {
            (Type processorType1, Type outputType1) = GetProcessorTypes<T1>(OutputType);
            (Type processorType2, Type outputType2) = GetProcessorTypes<T2>(outputType1);
            
            var outputStream = new AugerStream(ProviderName, name, outputType2);
            outputStream._processors.Add(processorType1);
            outputStream._processors.Add(processorType2);
            
            return outputStream;
        }

        public IAugerStream Process<T1, T2, T3>(string name)
        {
            (Type processorType1, Type outputType1) = GetProcessorTypes<T1>(OutputType);
            (Type processorType2, Type outputType2) = GetProcessorTypes<T2>(outputType1);
            (Type processorType3, Type outputType3) = GetProcessorTypes<T3>(outputType2);
            
            var outputStream = new AugerStream(ProviderName, name, outputType3);
            outputStream._processors.Add(processorType1);
            outputStream._processors.Add(processorType2);
            outputStream._processors.Add(processorType3);
            
            return outputStream;
        }

        public IAugerStream Process<T1, T2, T3, T4>(string name)
        {
            (Type processorType1, Type outputType1) = GetProcessorTypes<T1>(OutputType);
            (Type processorType2, Type outputType2) = GetProcessorTypes<T2>(outputType1);
            (Type processorType3, Type outputType3) = GetProcessorTypes<T3>(outputType2);
            (Type processorType4, Type outputType4) = GetProcessorTypes<T4>(outputType3);
            
            var outputStream = new AugerStream(ProviderName, name, outputType4);
            outputStream._processors.Add(processorType1);
            outputStream._processors.Add(processorType2);
            outputStream._processors.Add(processorType3);
            outputStream._processors.Add(processorType4);
            
            return outputStream;
        }
        
        public IAugerStream KeyBy(string name, Func<dynamic, Guid> keySelector)
        {
            throw new NotImplementedException();
        }
        
        public IAugerStream KeyBy(string name, Func<dynamic, string> keySelector)
        {
            throw new NotImplementedException();
        }
        
        public IAugerStream KeyBy(string name, Func<dynamic, int> keySelector)
        {
            throw new NotImplementedException();
        }
        
        private (Type processorType, Type outputType) GetProcessorTypes<T>(Type previousOutputType)
        {
            Type type = typeof(T);
            
            // check if type implements IAsyncObserver<T>
            if (!type.IsClass)
            {
                throw new ArgumentException("Processor must be a class");
            }
            
            if (type.IsAbstract)
            {
                throw new ArgumentException("Processor must not be abstract");
            }
            
            var interfaces = type.GetInterfaces();
            
            if (interfaces.Length == 0)
            {
                throw new ArgumentException("Processor must implement IAsyncObserver<TInput>");
            }
            
            Type asyncObserverType = typeof(IAsyncObserver<>);
            if (interfaces.Count(i => i.GetGenericTypeDefinition() == asyncObserverType) != 1)
            {
                throw new ArgumentException("Processor must only implement IAsyncObserver<TInput>");
            }
            
            Type inputType = interfaces.Single(i => i.GetGenericTypeDefinition() == asyncObserverType)
                .GetGenericArguments()[0];

            if (inputType != previousOutputType)
            {
                throw new ArgumentException("Processor must take the same type as the previous processor outputs");
            }
            
            // get output type from constructor
            var constructor = type.GetConstructors().SingleOrDefault();
            if (constructor == null)
            {
                throw new ArgumentException("Processor must have a public constructor");
            }
            
            var parameters = constructor.GetParameters();
            if (parameters.Length != 1)
            {
                throw new ArgumentException("Processor must have a single constructor parameter");
            }

            var outputTypes = parameters.Where(p =>
                p.ParameterType.IsGenericType
                && p.ParameterType.GetGenericTypeDefinition() == typeof(IAsyncObserver<>))
                .ToList();

            if (outputTypes.Count() != 1)
            {
                throw new ArgumentException("Processor must have a single constructor parameter of type IAsyncObserver<TOutput>");
            }
            
            var outputType = outputTypes.Single().ParameterType.GetGenericArguments()[0];
            
            if (outputType == typeof(T))
            {
                throw new ArgumentException("Processor must not output the same type as it takes as input");
            }
            
            if (outputType == typeof(void))
            {
                throw new ArgumentException("Processor must output a type");
            }
            
            if (outputType.IsGenericType)
            {
                throw new ArgumentException("Processor must not output a generic type");
            }
            
            if (outputType.IsInterface)
            {
                throw new ArgumentException("Processor must not output an interface");
            }
            
            if (outputType.IsAbstract)
            {
                throw new ArgumentException("Processor must not output an abstract type");
            }
            
            return (type, outputType);
        }
    }
}
