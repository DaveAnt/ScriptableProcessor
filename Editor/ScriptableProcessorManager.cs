using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public static class ScriptableProcessorManager
    {
        private static Dictionary<Type, SerializationInspectorBase> m_TypeInspectorPairs
            = new Dictionary<Type, SerializationInspectorBase>();
        private static Dictionary<string, ISerializedContentBase> m_SerializedContentPairs
            = new Dictionary<string, ISerializedContentBase>();
        private static Dictionary<UnityEngine.Object, SerializedObject> m_CustomSerializedPairs
            = new Dictionary<UnityEngine.Object, SerializedObject>();
        private static Dictionary<string, Type> m_ScriptableTypePairs
            = new Dictionary<string, Type>();

        public static SerializedObject Acquire(UnityEngine.Object custom)
        {
            SerializedObject result;
            if (!m_CustomSerializedPairs.TryGetValue(custom, out result))
            {
                result = new SerializedObject(custom);
                m_CustomSerializedPairs.Add(custom, result);
            }

            return result;
        }

        public static ISerializedContentBase Acquire(GameObject gameObject,string scriptableName)
        {
            Type scriptableType;
            ISerializedContentBase result;
            if (!m_ScriptableTypePairs.TryGetValue(scriptableName, out scriptableType))
            {
                scriptableType = AssemblyExt.GetType(scriptableName);
                m_ScriptableTypePairs.Add(scriptableName,scriptableType);
            }

            if (!m_SerializedContentPairs.TryGetValue(scriptableName, out result))
            {
                if (scriptableType.IsSubclassOf(typeof(MonoBehaviour)))
                    result = new MonoBehaviourContent(scriptableType);
                else if (scriptableType.IsSubclassOf(typeof(ScriptableObject)))
                    result = new ScriptableObjectContent(scriptableType);
                else
                    result = new BaseObjectContent(scriptableType);
                m_SerializedContentPairs.Add(scriptableName, result);
            }

            return result;
        }

        public static SerializationInspectorBase Acquire(Type inspectorType)
        {
            SerializationInspectorBase result;
            if (!m_TypeInspectorPairs.TryGetValue(inspectorType, out result))
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(inspectorType, false);
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is ScriptableInspectorAttribute)
                    {
                        Type type = (attribute as ScriptableInspectorAttribute).type;
                        result = (SerializationInspectorBase)Activator.CreateInstance(type);
                        break;
                    }
                }

                m_TypeInspectorPairs.Add(inspectorType, result);
            }

            return result;
        }

        public static void Dispose(object context)
        {
            foreach (var contextPairs in m_SerializedContentPairs)
                contextPairs.Value.Dispose();
            foreach (var customPairs in m_CustomSerializedPairs)
                customPairs.Value.Dispose();
            m_SerializedContentPairs.Clear();
            m_CustomSerializedPairs.Clear();
            m_ScriptableTypePairs.Clear();
            m_TypeInspectorPairs.Clear();
        }
    }
}