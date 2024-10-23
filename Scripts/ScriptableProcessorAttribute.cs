/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Diagnostics;
using UnityEngine;

namespace ScriptableProcessor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class EnumAttribute : PropertyAttribute
    {
        public readonly Type enumType;

        public EnumAttribute(Type enumType)
        {
            this.enumType = enumType;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ScriptableProcessorAttribute : PropertyAttribute
    {
        public readonly bool isLock;
        public readonly bool isHeader;
        public readonly int indentLevel = int.MinValue;

        public ScriptableProcessorAttribute(bool isLock = true,bool isHeader = true, int indentLevel = int.MinValue)
        {
            this.isLock = isLock;
            this.isHeader = isHeader;
            this.indentLevel = indentLevel;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ScriptableInspectorAttribute : Attribute
    {
        public readonly System.Type type;

        public ScriptableInspectorAttribute(System.Type type)
        {
            this.type = type;
        }
    }
}