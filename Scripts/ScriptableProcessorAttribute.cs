/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Diagnostics;
using UnityEngine;

namespace ScriptableProcessor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ScriptableProcessorAttribute : PropertyAttribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ScriptableInspectorAttribute : Attribute
    {
        public System.Type type { get; }

        public ScriptableInspectorAttribute(System.Type type)
        {
            this.type = type;
        }
    }
}