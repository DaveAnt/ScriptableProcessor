/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.IO;
using UnityEngine;

namespace ScriptableProcessor
{
    public static class TypeCreator
    {
        public static T Create<T>(string scriptableTypeName = null,params object[] userData) where T : class
        {
            System.Type scriptableType = scriptableTypeName != null ? AssemblyExt.GetType(scriptableTypeName) : typeof(T);
            if (scriptableType.IsSubclassOf(typeof(ScriptableObject)))
                return (T)CreateScriptable(scriptableType, userData);
            else if (scriptableType.IsSubclassOf(typeof(MonoBehaviour)))
                return (T)CreateComponent(scriptableType, userData);
            else
                return (T)CreateObject(scriptableType);
        }

        public static string GetScriptableAssetName(string targetName, string scriptableTypeName)
        {
            string scriptableName = scriptableTypeName.Contains('.') ? Path.GetExtension(scriptableTypeName) : '.' + scriptableTypeName;
            return string.Format("[SP]{0}{1}", targetName, scriptableName);
        }

        private static object CreateScriptable(System.Type scriptableType, params object[] userData)
        {
            ScriptableObject scriptable = null;

            if (scriptableType == null)
            {
                Debug.LogWarning(string.Format("Can not find scriptable type '{0}'.", scriptableType.FullName));
                return null;
            }

            scriptable = ScriptableObject.CreateInstance(scriptableType);
            if (scriptable == null)
            {
                Debug.LogError(string.Format("Can not create scriptable."));
                return null;
            }
            
            scriptable.name = userData.Length > 0 ? userData[0].ToString() : scriptableType.Name;
            return scriptable;
        }

        private static object CreateComponent(System.Type scriptableType, params object[] userData)
        {
            Component component = null;

            if (scriptableType == null)
            {
                Debug.LogWarning(string.Format("Can not find component type '{0}'.", scriptableType));
                return null;
            }

            component = ((Transform)userData[1]).gameObject.AddComponent(scriptableType);
            if (component == null)
            {
                Debug.LogError(string.Format("Can not create component."));
                return null;
            }

            return component;
        }

        private static object CreateObject(System.Type scriptableType, params object[] userData)
        {
            object obj = null;
            if (scriptableType == null)
            {
                Debug.LogWarning(string.Format("Can not find object type '{0}'.", scriptableType.FullName));
                return null;
            }

            obj = System.Activator.CreateInstance(scriptableType);
            if (obj == null)
            {
                Debug.LogWarning("Can not create object.");
                return null;
            }

            return obj;
        }
    }
}
