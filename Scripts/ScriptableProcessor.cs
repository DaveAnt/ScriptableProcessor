/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableProcessor
{
    [Serializable]
    public class ScriptableInstance
    {
        [SerializeField]
        private string m_ScriptableData;
        [SerializeField]
        private UnityEngine.Object[] m_ScriptableReferences;

        private string m_ScriptableFormatData;

        public string ScriptableFormatData
        {
            get
            {
                if (string.IsNullOrEmpty(m_ScriptableFormatData))
                {
                    string[] scriptableInstanceIds = new string[m_ScriptableReferences.Length];
                    for (int i = 0; i < m_ScriptableReferences.Length; ++i)
                    {
                        scriptableInstanceIds[i] = string.Format("{{\"instanceID\":{0}}}", m_ScriptableReferences[i]?.GetInstanceID() ?? 0);
                    }

                    m_ScriptableFormatData = string.Format(m_ScriptableData, scriptableInstanceIds);
                }

                return m_ScriptableFormatData;
            }
        }

        public ScriptableInstance New()
        {
            m_ScriptableFormatData = null;
            return this;
        }
    }

    [Serializable]
    public class ScriptableInfo<T> where T : class
    {
        [SerializeField]
        private T m_CustomScriptable;
        [SerializeField]
        private sbyte m_ScriptableTypeIndex;
        [SerializeField]
        private ScriptableInstance[] m_OptionSerializeDatas;

        public Action<GameObject, string[]> OnInit;
        private T m_ScriptableTarget;

        public T ScriptableTarget
        {
            get
            {
                if (m_ScriptableTypeIndex == -1)
                    return m_CustomScriptable;
                return m_ScriptableTarget;
            }
        }

        public ScriptableInfo()
        {
            OnInit = (target, typeNames) =>
            {
                UpdateSerialize(target, typeNames);
                OnInit = null;
            };
        }

        public void UpdateSerialize(GameObject target, string[] typeNames)
        {
            if (m_ScriptableTypeIndex == -1)
                return;

            ScriptableInstance scriptableInstance = m_OptionSerializeDatas[m_ScriptableTypeIndex].New();
            m_ScriptableTarget = TypeCreator.Create<T>(typeNames[m_ScriptableTypeIndex],
                new CreateParams(scriptableInstance.ScriptableFormatData, target));
        }
    }

    [Serializable]
    public class ScriptableProcessor<T> : IEnumerable<T> where T : class
    {
        [SerializeField]
        protected MonoBehaviour m_Target;
        [SerializeField]
        protected string[] m_OptionTypeNames;
        [SerializeField]
        protected ScriptableInfo<T>[] m_ScriptableInfos;
        public int length { get { return m_ScriptableInfos.Length; } }
        public MonoBehaviour target => m_Target;

        public T this[int index]
        {
            get
            {
                if (index < m_ScriptableInfos.Length)
                {
                    ScriptableInfo<T> scriptableInfo = m_ScriptableInfos[index];
                    scriptableInfo.OnInit?.Invoke(m_Target.gameObject, m_OptionTypeNames);
                    return scriptableInfo.ScriptableTarget;
                }
                return null;
            }
        }

        public void UpdateSerialize()
        {
            for (int i = 0; i < m_ScriptableInfos.Length; i++)
            {
                ScriptableInfo<T> scriptableInfo = m_ScriptableInfos[i];
                if (scriptableInfo.OnInit == null)
                {
                    scriptableInfo.UpdateSerialize(m_Target.gameObject, m_OptionTypeNames);
                }
            }
        }

        public byte IndexOf(T item)
        {
            for(byte i =0; i < m_ScriptableInfos.Length;++i)
            {
                if (item == this[i])
                    return i;
            }
            return 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < m_ScriptableInfos.Length;i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
