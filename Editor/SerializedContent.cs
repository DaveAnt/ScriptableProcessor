/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public class SerializedContent : SerializedObject
    {
        internal class ObjectPacket : ScriptableObject
        {
            [SerializeReference]
            public object m_Native;
        }

        string defaultData;
        public readonly bool custom;
        public readonly object target;
        public readonly Component component;
        static readonly Dictionary<string, SerializedContent> m_SerializedPairs = new Dictionary<string, SerializedContent>();
        public SerializedProperty targetProp
        {
            get
            {
                return (targetObject is ObjectPacket) ? FindProperty("m_Native") : GetIterator();
            }
        }

        public SerializedContent(Object obj, Component component, bool custom = false) : base(obj)
        {
            this.custom = custom;
            this.component = component;
            target = (obj is ObjectPacket) ? (obj as ObjectPacket).m_Native : obj;
            defaultData = JsonUtility.ToJson(target);
        }

        public static SerializedContent Acquire(Object target, Component component)
        {
            SerializedContent result;
            string packetGuid = target.GetInstanceID().ToString();
            if (!m_SerializedPairs.TryGetValue(packetGuid, out result))
            {
                result = new SerializedContent(target, component, true);
                m_SerializedPairs.Add(packetGuid, result);
            }
            else if (result.targetObject == null)
            {
                result.Dispose();
                m_SerializedPairs.Remove(packetGuid);
                result = new SerializedContent(target, component, true);
                m_SerializedPairs.Add(packetGuid, result);
            }

            return result;
        }

        public static SerializedContent Acquire(string typeName,string scriptablePath, string serializedData, Component component)
        {
            SerializedContent result;
            string packetGuid = typeName + scriptablePath + component.GetInstanceID();
            if(!m_SerializedPairs.TryGetValue(packetGuid, out result))
            {
                result = New(typeName, serializedData, component);
                m_SerializedPairs.Add(packetGuid, result);
            }
            else if(result.targetObject == null)
            {
                result.Dispose();
                m_SerializedPairs.Remove(packetGuid);
                result = New(typeName, serializedData, component);
                m_SerializedPairs.Add(packetGuid, result);
            }
            else
            {
                JsonUtility.FromJsonOverwrite(serializedData ?? result.defaultData, result.target);
                result.Update();
            }

            return result;
        }

        private static SerializedContent New(string typeName, string serializedData, Component component)
        {
            object target = TypeCreator.Create(typeName, new CreateParams(serializedData, component.gameObject, null, HideFlags.HideInInspector | HideFlags.DontSave));
            if (!(target is Object))
            {
                ObjectPacket packet = ScriptableObject.CreateInstance<ObjectPacket>();
                packet.m_Native = target;
                target = packet;
            }
            return new SerializedContent((Object)target, component);
        }

        public static void Release(object context)
        {
            foreach(var content in m_SerializedPairs)
            {
                if(content.Value != null && content.Value.targetObject != null)
                {
                    if(!content.Value.custom)
                        Object.DestroyImmediate(content.Value.targetObject, true);
                    content.Value.Dispose();
                }
            }
            m_SerializedPairs.Clear();
        }

        public string ToJson()
        {
            ApplyModifiedProperties();
            return JsonUtility.ToJson(target);
        }
    }
}
