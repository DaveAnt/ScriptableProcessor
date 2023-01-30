/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEngine;

namespace ScriptableProcessor
{
    public enum ScriptableType : byte
    {
        None,
        MonoBehaviour,
        ScriptableObject,
    }

    public struct CreateParams
    {
        public readonly string serializeData;
        public readonly string scriptableName;
        public readonly GameObject objectTarget;
        public readonly HideFlags hideFlags;

        public CreateParams(GameObject objectTarget, string serializeData = null, HideFlags hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector, string scriptableName = null)
        {
            this.serializeData = serializeData;
            this.scriptableName = scriptableName;
            this.objectTarget = objectTarget;
            this.hideFlags = hideFlags;
        }
    }

    public static class TypeCreator
    {
        public static T Create<T>(string scriptableTypeName, CreateParams createParams, Action<T, ScriptableType> procResultFunc = null) where T : class
        {
            T instance = null;
            System.Type scriptableType = scriptableTypeName != null ? AssemblyExt.GetType(scriptableTypeName) : typeof(T);
            if (scriptableType.IsSubclassOf(typeof(ScriptableObject)))
            {
                instance = (T)CreateScriptable(scriptableType, createParams);
                procResultFunc?.Invoke(instance, ScriptableType.ScriptableObject);
            }
            else if (scriptableType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                instance = (T)CreateComponent(scriptableType, createParams);
                procResultFunc?.Invoke(instance, ScriptableType.MonoBehaviour);
            }
            else
            {
                instance = (T)CreateObject(scriptableType, createParams);
                procResultFunc?.Invoke(instance, ScriptableType.None);
            }

            return instance;
        }

        private static object CreateScriptable(System.Type scriptableType, CreateParams createParams)
        {
            if (scriptableType == null)
            {
                Debug.LogWarning(string.Format("Can not find scriptable type '{0}'.", scriptableType.FullName));
                return null;
            }

            ScriptableObject scriptable = ScriptableObject.CreateInstance(scriptableType);
            if (scriptable == null)
            {
                Debug.LogError(string.Format("Can not create scriptable."));
                return null;
            }

            if (!string.IsNullOrEmpty(createParams.serializeData))
            {
                JsonUtility.FromJsonOverwrite(createParams.serializeData, scriptable);
            }

            scriptable.name = createParams.scriptableName ?? scriptableType.Name;
            scriptable.hideFlags = createParams.hideFlags;
            return scriptable;
        }

        private static object CreateComponent(System.Type scriptableType, CreateParams createParams)
        {
            if (scriptableType == null)
            {
                Debug.LogWarning(string.Format("Can not find component type '{0}'.", scriptableType));
                return null;
            }

            Component component = createParams.objectTarget.AddComponent(scriptableType);

            if (component == null)
            {
                Debug.LogError(string.Format("Can not create component."));
                return null;
            }

            if (!string.IsNullOrEmpty(createParams.serializeData))
            {
                JsonUtility.FromJsonOverwrite(createParams.serializeData, component);
            }

            component.hideFlags = createParams.hideFlags;
            return component;
        }

        private static object CreateObject(System.Type scriptableType, CreateParams createParams)
        {
            if (scriptableType == null)
            {
                Debug.LogWarning(string.Format("Can not find object type '{0}'.", scriptableType.FullName));
                return null;
            }

            object obj;
            if (!string.IsNullOrEmpty(createParams.serializeData))
            {
                obj = JsonUtility.FromJson(createParams.serializeData, scriptableType);
            }
            else
            {
                obj = System.Activator.CreateInstance(scriptableType);
            }
            
            if (obj == null)
            {
                Debug.LogWarning("Can not create object.");
                return null;
            }

            return obj;
        }
    }
}
