﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleans.Bus
{
    class GrainReferenceService
    {
        public static readonly GrainReferenceService Instance = new GrainReferenceService().Initialize();

        readonly IDictionary<Type, Func<string, object>> grains = 
             new Dictionary<Type, Func<string, object>>();

        GrainReferenceService Initialize()
        {
            var factories = LoadAssemblies()
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(IsOrleansCodegenedFactory)
                .ToList();

            foreach (var factory in factories)
            {
                var grain = factory.GetMethod("Cast").ReturnType;
                if (grain.HasAttribute<ExtendedPrimaryKeyAttribute>())
                    grains[grain] = Bind(factory);
            }

            return this;
        }

        static IEnumerable<Assembly> LoadAssemblies()
        {
            var dir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            Debug.Assert(dir != null);
            var dlls = Directory.GetFiles(dir, "*.dll");

            return dlls.Where(ContainsOrleansGeneratedCode)
                       .Select(Assembly.LoadFrom);
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

        public T Get<T>(string id)
        {
            return (T)Get(typeof(T), id);
        }

        public object Get(Type @interface, string id)
        {
            var invoker = grains.Find(@interface);
            Debug.Assert(invoker != null);
            return invoker.Invoke(id);
        }

        public IEnumerable<Type> RegisteredGrainTypes()
        {
            return grains.Keys;
        }
    }
}