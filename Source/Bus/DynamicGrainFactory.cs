using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Orleans.Bus
{
    internal class DynamicGrainFactory
    {
        public static readonly DynamicGrainFactory Instance = new DynamicGrainFactory().Initialize();

        readonly IDictionary<Type, Func<string, object>> grains = 
             new Dictionary<Type, Func<string, object>>();

        DynamicGrainFactory Initialize()
        {
            var factories = LoadAssemblies()
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(IsOrleansCodegenedFactory)
                .ToList();

            foreach (var factory in factories)
            {
                var grain = factory.GetMethod("Cast").ReturnType;

                if (!typeof(IMessageBasedGrain).IsAssignableFrom(grain))
                    continue;

                if (!grain.HasAttribute<ExtendedPrimaryKeyAttribute>())
                    throw new MissingExtendedPrimaryKeyAttributeException(grain);

                grains[grain] = Bind(factory);
            }

            return this;
        }

        static IEnumerable<Assembly> LoadAssemblies()
        {
            var dir = GetAssemblyPath();

            Debug.Assert(dir != null);
            var dlls = Directory.GetFiles(dir, "*.dll");

            return dlls.Where(ContainsOrleansGeneratedCode)
                       .Select(Assembly.LoadFrom);
        }

        static string GetAssemblyPath()
        {
            var builder = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(builder.Path));
        }

        static bool ContainsOrleansGeneratedCode(string dll)
        {
            var info = FileVersionInfo.GetVersionInfo(dll);
            return info.Comments.ToLower() == "contains.orleans.generated.code";
        }

        static bool IsOrleansCodegenedFactory(Type type)
        {
            return type.GetCustomAttributes(typeof(GeneratedCodeAttribute), true)
                       .Cast<GeneratedCodeAttribute>()
                       .Any(x => x.Tool == "Orleans-CodeGenerator")
                   && type.Name.EndsWith("Factory");
        }

        static  Func<string, object> Bind(IReflect factory)
        {
            var method = factory.GetMethod("GetGrain", 
                BindingFlags.Public | BindingFlags.Static, null, 
                new[]{typeof(long), typeof(string)}, null);

            var argument = Expression.Parameter(typeof(string), "ext");
            var call = Expression.Call(method, new Expression[]{Expression.Constant(0L), argument});
            var lambda = Expression.Lambda<Func<string, object>>(call, argument);

            return lambda.Compile();
        }

        public object GetReference(Type type, string id)
        {
            var invoker = grains.Find(type);
            Debug.Assert(invoker != null);
            return invoker.Invoke(id);
        }

        public IEnumerable<Type> RegisteredGrainTypes()
        {
            return grains.Keys;
        }

        [Serializable]
        internal class MissingExtendedPrimaryKeyAttributeException : ApplicationException
        {
            const string description = "The grain interface '{0}' implements IMessageBasedGrain but is missing [ExtendedPrimaryKey] attribute";

            public MissingExtendedPrimaryKeyAttributeException(Type grain)
                : base(string.Format(description, grain))
            {}

            protected MissingExtendedPrimaryKeyAttributeException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }        
    }
}