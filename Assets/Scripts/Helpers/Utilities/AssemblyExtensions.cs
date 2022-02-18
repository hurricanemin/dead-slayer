using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Helpers.Utilities
{
    public static class AssemblyExtensions
    {
        public static List<Type> FindParentClasses(this Type childClass)
        {
            Type parentType = childClass.BaseType;
            List<Type> parentClasses = new List<Type>();

            while (parentType != null && parentType != typeof(MonoBehaviour) && parentType != typeof(Component))
            {
                parentClasses.Add(parentType);
                parentType = parentType.BaseType;
            }

            return parentClasses;
        }

        public static List<Type> FindChildClasses(this Type parentClass)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> foundTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes().Where(x => x.IsSubclassOf(parentClass)
                                                              && !x.IsAbstract).ToArray();
                foreach (var type in types) foundTypes.Add(type);
            }

            return foundTypes;
        }

        public static bool InvokeMethod<T>(this T target, string methodName, object[] parameters = null) where T : class
        {
            parameters ??= Array.Empty<object>();
            Type targetType = target.GetType();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo[] methodInfos = targetType.GetMethods(bindingFlags).Where(x => x.Name == methodName).ToArray();
            bool isInvoked = false;

            foreach (var method in methodInfos)
            {
                ParameterInfo[] parameterInfos = method.GetParameters();
                int parameterCount = parameterInfos.Length;
                if (parameterCount != parameters.Length) continue;
                bool areParametersMatching = true;

                foreach (var parameter in parameters)
                {
                    if (parameter.GetType() == parameterInfos.GetType()) continue;
                    areParametersMatching = false;
                    break;
                }

                if (!areParametersMatching) continue;

                try
                {
                    method.Invoke(target, parameters);
                    isInvoked = true;
                }
                catch
                {
                    //
                }
            }

            return isInvoked;
        }

        public static bool SetField<T>(this T target, string variableName, object value) where T : class
        {
            FieldInfo[] fieldInfos = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name == variableName).ToArray();
            if (fieldInfos.Length <= 0) return false;
            if (fieldInfos[0].FieldType != value.GetType()) return false;
            fieldInfos[0].SetValue(target, value);
            return true;
        }
    }
}