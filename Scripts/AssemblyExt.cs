/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ScriptableProcessor
{
    public enum AssemblyType
    {
        Runtime,
        Editor,
        All
    }

    public static class AssemblyExt
    {
        public static readonly string[][] s_AssemblyNames =
        {
            new string[] {
                "Assembly-CSharp",
            },
#if UNITY_EDITOR
            new string[] {
                "Assembly-CSharp-Editor",
            },
#endif
        };

        private static Assembly[] s_AllAssemblies = null;
        private static readonly Assembly[][] s_Assemblies = null;
        private static readonly Dictionary<string, Type> s_CachedTypes = new Dictionary<string, Type>(StringComparer.Ordinal);

        static AssemblyExt()
        {
            s_Assemblies = new Assembly[s_AssemblyNames.Length][];
            for (int index=0; index < s_AssemblyNames.Length; ++index)
            {
                var assemblyNames = s_AssemblyNames[index];
                int noAssemblyIndex = -1, noAssemblyCount = 0;
                s_Assemblies[index] = new Assembly[assemblyNames.Length];
                for(int number=0; number < assemblyNames.Length; ++number)
                {
                    try
                    {
                        if(noAssemblyIndex != -1)
                        {
                            string noAssemblyName = s_AssemblyNames[index][noAssemblyIndex];
                            s_Assemblies[index][noAssemblyIndex] = Assembly.Load(assemblyNames[number]);
                            s_AssemblyNames[index][noAssemblyIndex] = s_AssemblyNames[index][number];
                            s_AssemblyNames[index][number] = noAssemblyName;
                            noAssemblyIndex++;
                        }
                        else
                        {
                            s_Assemblies[index][number] = Assembly.Load(assemblyNames[number]);
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning(ex.Message);
                        noAssemblyIndex = noAssemblyIndex == -1 ? number : noAssemblyIndex;
                        noAssemblyCount++;
                    }
                }
                if (noAssemblyCount != 0)
                {
                    Array.Resize(ref s_Assemblies[index], s_Assemblies[index].Length - noAssemblyCount);
                }
            }
        }

        public static Assembly[] GetAssemblies(AssemblyType assemblyType = AssemblyType.Runtime)
        {
            switch (assemblyType)
            {
                case AssemblyType.Runtime:
                    return s_Assemblies[0];
                case AssemblyType.Editor:
                    return s_Assemblies[1];
                case AssemblyType.All:
                    if (s_AllAssemblies == null)
                    {
                        s_AllAssemblies = new Assembly[s_Assemblies[0].Length + s_Assemblies[1].Length];
                        Array.Copy(s_Assemblies[0], s_AllAssemblies, s_Assemblies[0].Length);
                        Array.Copy(s_Assemblies[1], 0, s_AllAssemblies, s_Assemblies[0].Length, s_Assemblies[1].Length);
                    }
                    return s_AllAssemblies;
            }
            return null;
        }

        public static Type[] GetTypes(AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<Type> results = new List<Type>();
            foreach (System.Reflection.Assembly assembly in GetAssemblies(assemblyType))
            {
                results.AddRange(assembly.GetTypes());
            }

            return results.ToArray();
        }

        public static void GetTypes(List<Type> results, AssemblyType assemblyType = AssemblyType.Runtime)
        {
            if (results == null)
            {
                throw new UnityEngine.UnityException("Results is invalid.");
            }

            results.Clear();
            foreach (System.Reflection.Assembly assembly in GetAssemblies(assemblyType))
            {
                results.AddRange(assembly.GetTypes());
            }
        }

        public static Type GetType(string typeName, AssemblyType assemblyType = AssemblyType.Runtime)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new UnityEngine.UnityException("Type name is invalid.");
            }

            Type type = null;
            if (s_CachedTypes.TryGetValue(typeName, out type))
            {
                return type;
            }

            type = Type.GetType(typeName);
            if (type != null)
            {
                s_CachedTypes.Add(typeName, type);
                return type;
            }

            foreach (Assembly assembly in GetAssemblies(assemblyType))
            {
                type = Type.GetType(string.Format("{0}, {1}", typeName, assembly.FullName));
                if (type != null)
                {
                    s_CachedTypes.Add(typeName, type);
                    return type;
                }
            }

            return null;
        }
    }
}