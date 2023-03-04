/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ScriptableProcessor
{
    /// <summary>
    /// 类型相关的实用函数。
    /// </summary>
    public static class TypeExt
    {
        public static string[] GetTypeFullNames(System.Type typeBase, AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<System.Type> typeList = GetTypes(typeBase, assemblyType);
            return typeList.ToArray((type) => { return type.FullName; });
        }

        public static string[] GetTypeFullNames(string typeBaseName, AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<System.Type> typeList = GetTypes(typeBaseName, assemblyType);
            return typeList.ToArray((type) => { return type.FullName; });
        }

        public static string[] GetTypeNames(System.Type typeBase, string withoutExtensions = ".cs", AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<System.Type> typeList = GetTypes(typeBase, assemblyType);
            return typeList.ToArray((type) => { return type.Name + withoutExtensions; });
        }

        public static string[] GetTypeNames(string typeBaseName, string withoutExtensions = ".cs", AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<System.Type> typeList = GetTypes(typeBaseName, assemblyType);
            return typeList.ToArray((type) => { return type.Name + withoutExtensions; });
        }

        public static List<System.Type> GetTypes(string typeBaseName, AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<System.Type> typeList = new List<System.Type>();
            List<System.Type[]> typeArrayList = GetAllTypes(AssemblyExt.GetAssemblies(assemblyType));
            foreach (System.Type[] types in typeArrayList)
            {
                foreach (System.Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeBaseName))
                    {
                        typeList.Add(type);
                    }
                }
            }
            typeList.Sort((a, b) => { return string.Compare(a.FullName, b.FullName); });
            return typeList;
        }

        public static List<System.Type> GetTypes(System.Type typeBase, AssemblyType assemblyType = AssemblyType.Runtime)
        {
            List<System.Type> typeList = new List<System.Type>();
            List<System.Type[]> typeArrayList = GetAllTypes(AssemblyExt.GetAssemblies(assemblyType));
            foreach (System.Type[] types in typeArrayList)
            {
                foreach (System.Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeList.Add(type);
                    }
                }
            }
            typeList.Sort((a, b) => { return string.Compare(a.FullName, b.FullName); });
            return typeList;
        }

        public static List<System.Type[]> GetAllTypes(Assembly[] assemblies)
        {
            List<System.Type[]> typeArrayList = new List<System.Type[]>();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                System.Type[] types = assembly.GetTypes();
                typeArrayList.Add(types);
            }
            return typeArrayList;
        }

        public static T[] ToArray<T>(this List<System.Type> iList, Func<System.Type, T> func)
        {
            T[] array = new T[iList.Count];
            for (int i = 0; i < iList.Count; ++i)
            {
                array[i] = func(iList[i]);
            }
            return array;
        }

        public static bool IsSubclassOf(this System.Type type, string baseFullName)
        {
            if (type.FullName.Equals(baseFullName))
                return true;
            else if (type.BaseType == null)
                return false;
            return IsSubclassOf(type.BaseType, baseFullName);
        }
    }
}
