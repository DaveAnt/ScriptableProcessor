/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
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
        private static readonly string[][] AssemblyNames =
        {
            new string[] {
            "Assembly-CSharp",
            },
            new string[] {
            "Assembly-CSharp",
            "Assembly-CSharp-Editor",
            }
        };


        public static string[] GetTypeFullNames(System.Type typeBase, int index = 0)
        {
            List<System.Type> typeList = GetTypes(typeBase, AssemblyNames[index]);
            return typeList.ToArray((type) => { return type.FullName; });
        }

        public static string[] GetTypeFullNames(string typeBaseName, int index = 0)
        {
            List<System.Type> typeList = GetTypes(typeBaseName, AssemblyNames[index]);
            return typeList.ToArray((type) => { return type.FullName; });
        }

        public static string[] GetTypeNames(System.Type typeBase, string withoutExtensions = ".cs", int index = 0)
        {
            List<System.Type> typeList = GetTypes(typeBase, AssemblyNames[index]);
            return typeList.ToArray((type) => { return type.Name + withoutExtensions; });
        }

        public static string[] GetTypeNames(string typeBaseName, string withoutExtensions = ".cs", int index = 0)
        {
            List<System.Type> typeList = GetTypes(typeBaseName, AssemblyNames[index]);
            return typeList.ToArray((type) => { return type.Name + withoutExtensions; });
        }

        public static List<System.Type> GetTypes(string typeBaseName, string[] assemblyNames)
        {
            List<System.Type> typeList = new List<System.Type>();
            List<System.Type[]> typeArrayList = GetAllTypes(assemblyNames);
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

        public static List<System.Type> GetTypes(System.Type typeBase, string[] assemblyNames)
        {
            List<System.Type> typeList = new List<System.Type>();
            List<System.Type[]> typeArrayList = GetAllTypes(assemblyNames);
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

        public static List<System.Type[]> GetAllTypes(string[] assemblyNames)
        {
            List<System.Type[]> typeArrayList = new List<System.Type[]>();
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    continue;
                }

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
