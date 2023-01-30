/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEngine;

namespace ScriptableProcessor
{
    [Serializable]
    public class ScriptableInfo<T> where T : class
    {
        [SerializeField]
        private T m_CustomScriptable;
        [SerializeField]
        private sbyte m_ScriptableTypeIndex;
        [SerializeField]
        private string[] m_OptionSerializeDatas;
        [SerializeField]
        private T[] m_OptionScriptables;

        private Action<GameObject, string[]> m_OnInitProc;

        public Action<GameObject, string[]> OnInitProc
        {
            get
            {
                return m_OnInitProc;
            }
        }

        public T ScriptableTarget
        {
            get
            {
                if (m_ScriptableTypeIndex == -1)
                    return m_CustomScriptable;
                return m_OptionScriptables[m_ScriptableTypeIndex];
            }
        }

        public ScriptableInfo()
        {
            m_OnInitProc = (target, optionTypeNames) => {
                m_OnInitProc = null;
                if (m_ScriptableTypeIndex == -1)
                    return;

                if (m_OptionScriptables == null)
                {
                    m_OptionScriptables = new T[optionTypeNames.Length];
                }

                if (m_OptionScriptables[m_ScriptableTypeIndex] == null)
                {
                    string optionSerializeData = string.Empty;
                    if (m_ScriptableTypeIndex < m_OptionSerializeDatas.Length)
                        optionSerializeData = m_OptionSerializeDatas[m_ScriptableTypeIndex];
                    m_OptionScriptables[m_ScriptableTypeIndex] = TypeCreator.Create<T>(optionTypeNames[m_ScriptableTypeIndex], new CreateParams(target, optionSerializeData));
                }
            };
        }
    }

    [Serializable]
    public sealed class ScriptableProcessor<T> where T : class
    {
        [SerializeField]
        private GameObject m_Target;
        [SerializeField]
        private string[] m_OptionTypeNames;
        [SerializeField]
        private ScriptableInfo<T>[] m_ScriptableInfos;

        public T this[int index]
        {
            get {
                if (index >= m_ScriptableInfos.Length)
                {
                    Debug.LogWarning("ScriptableInfo index out of range!");
                    return null;
                }

                m_ScriptableInfos[index]?.OnInitProc(m_Target, m_OptionTypeNames);
                return m_ScriptableInfos[index].ScriptableTarget;
            }
        }
    }
}
