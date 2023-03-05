/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public interface ISerializedContentBase
    {
        public abstract SerializedProperty PropertyTarget { get; }

        public abstract string ToJson(SerializedProperty prop);

        public abstract void FormJson(SerializedProperty prop, string json);

        public abstract void Dispose();
    }

    public class ScriptableProcessorPacket : ScriptableObject
    {
        [SerializeReference]
        public object m_NativeObject;
    }

    public class BaseObjectContent : ISerializedContentBase
    {
        private readonly object m_NativeObject;
        private readonly string m_OrginJsonData;
        private SerializedObject m_ObjectSerialization;
        private ScriptableProcessorPacket m_ScriptableProcessorPacket;

        private ScriptableProcessorPacket ScriptableProcessorPacket
        {
            get
            {
                if (m_ScriptableProcessorPacket == null)
                    m_ScriptableProcessorPacket = ScriptableObject.CreateInstance<ScriptableProcessorPacket>();
                return m_ScriptableProcessorPacket;
            }
        }

        private SerializedObject ObjectSerialization
        {
            get
            {
                try
                {
                    if (m_ObjectSerialization == null)
                        m_ObjectSerialization = new SerializedObject(ScriptableProcessorPacket);
                    else if (m_ObjectSerialization.targetObject == null)
                    {
                        m_ObjectSerialization.Dispose();
                        m_ObjectSerialization = new SerializedObject(ScriptableProcessorPacket);
                    }
                }
                catch
                {
                    m_ObjectSerialization = new SerializedObject(ScriptableProcessorPacket);
                }
                return m_ObjectSerialization;
            }
        }

        public SerializedProperty PropertyTarget
        {
            get
            {
                ScriptableProcessorPacket.m_NativeObject = m_NativeObject;
                SerializedProperty nativeProp = ObjectSerialization.FindProperty("m_NativeObject");
                ObjectSerialization.Update();
                return nativeProp;
            }
        }

        public BaseObjectContent(Type scriptableType)
        {
            m_NativeObject = Activator.CreateInstance(scriptableType);
            m_OrginJsonData = JsonUtility.ToJson(m_NativeObject);
        }

        public string ToJson(SerializedProperty prop)
        {
            ObjectSerialization.ApplyModifiedProperties();
            return JsonUtility.ToJson(m_NativeObject);
        }

        public void FormJson(SerializedProperty prop, string json)
        {
            json = string.IsNullOrEmpty(json) ? m_OrginJsonData : json;
            JsonUtility.FromJsonOverwrite(json, m_NativeObject);
            ObjectSerialization.Update();
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(m_ScriptableProcessorPacket);
            m_ObjectSerialization.Dispose();
        }
    }

    public class ScriptableObjectContent : ISerializedContentBase
    {
        private readonly Type m_ScriptabType;
        private readonly string m_OrginJsonData;
        private ScriptableObject m_NativeObject;
        private SerializedObject m_ObjectSerialization;

        private ScriptableObject NativeObject
        {
            get
            {
                if (m_NativeObject == null)
                    m_NativeObject = ScriptableObject.CreateInstance(m_ScriptabType);
                return m_NativeObject;
            }
        }

        private SerializedObject ObjectSerialization
        {
            get
            {
                try
                {
                    if (m_ObjectSerialization == null)
                        m_ObjectSerialization = new SerializedObject(NativeObject);
                    else if (m_ObjectSerialization.targetObject == null)
                    {
                        m_ObjectSerialization.Dispose();
                        m_ObjectSerialization = new SerializedObject(NativeObject);
                    }
                }
                catch
                {
                    m_ObjectSerialization = new SerializedObject(NativeObject);
                }
                return m_ObjectSerialization;
            }
        }

        public SerializedProperty PropertyTarget
        {
            get
            {
                return ObjectSerialization.GetIterator();
            }
        }

        public ScriptableObjectContent(Type scriptableType)
        {
            m_ScriptabType = scriptableType;
            m_NativeObject = ScriptableObject.CreateInstance(scriptableType);
            m_ObjectSerialization = new SerializedObject(m_NativeObject);
            m_OrginJsonData = JsonUtility.ToJson(m_NativeObject);
        }

        public string ToJson(SerializedProperty prop)
        {
            ObjectSerialization.ApplyModifiedProperties();
            return JsonUtility.ToJson(NativeObject);
        }

        public void FormJson(SerializedProperty prop, string json)
        {
            json = string.IsNullOrEmpty(json) ? m_OrginJsonData : json;
            JsonUtility.FromJsonOverwrite(json, NativeObject);
            ObjectSerialization.Update();
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(m_NativeObject);
            m_ObjectSerialization.Dispose();
        }
    }

    public class MonoBehaviourContent : ISerializedContentBase
    {
        private readonly Type m_ScriptabType;
        private readonly string m_OrginJsonData;

        private Component m_NativeObject;
        private SerializedObject m_ObjectSerialization;

        private Component NativeObject
        {
            get
            {
                if (m_NativeObject == null)
                {
                    m_NativeObject = ScriptableProcessorManager.ScriptableProcessorPacket.GetOrAddComponent(m_ScriptabType);
                    m_NativeObject.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;
                }
                return m_NativeObject;
            }
        }

        private SerializedObject ObjectSerialization
        {
            get
            {
                try
                {
                    if (m_ObjectSerialization == null)
                        m_ObjectSerialization = new SerializedObject(NativeObject);
                    else if (m_ObjectSerialization.targetObject == null)
                    {
                        m_ObjectSerialization.Dispose();
                        m_ObjectSerialization = new SerializedObject(NativeObject);
                    }
                }
                catch
                {
                    m_ObjectSerialization = new SerializedObject(NativeObject);
                }
                return m_ObjectSerialization;
            }
        }

        public SerializedProperty PropertyTarget
        {
            get
            {
                return ObjectSerialization.GetIterator();
            }
        }

        public MonoBehaviourContent(Type scriptableType)
        {
            m_ScriptabType = scriptableType;
            m_NativeObject = ScriptableProcessorManager.ScriptableProcessorPacket.GetOrAddComponent(m_ScriptabType);
            m_NativeObject.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;
            m_ObjectSerialization = new SerializedObject(m_NativeObject);
            m_OrginJsonData = JsonUtility.ToJson(m_NativeObject);
        }

        public string ToJson(SerializedProperty prop)
        {
            SerializedObject serializedObject = prop.serializedObject;
            serializedObject.ApplyModifiedProperties();
            return JsonUtility.ToJson(serializedObject.targetObject);
        }

        public void FormJson(SerializedProperty prop, string json)
        {
            json = string.IsNullOrEmpty(json) ? m_OrginJsonData : json;
            SerializedObject serializedObject = prop.serializedObject;
            JsonUtility.FromJsonOverwrite(json, serializedObject.targetObject);
            serializedObject.Update();
        }

        public void Dispose()
        {
            JsonUtility.FromJsonOverwrite(m_OrginJsonData, NativeObject);
        }
    }
}
