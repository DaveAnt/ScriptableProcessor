/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
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

        private Action<GameObject, string[]> m_OnInitProc;
        private T m_ScriptableProcessor;

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
                return m_ScriptableProcessor;
            }
        }

        public ScriptableInfo()
        {
            m_OnInitProc = (target, optionTypeNames) => {
                m_OnInitProc = null;
                if (m_ScriptableTypeIndex == -1)
                    return;

                if (m_ScriptableProcessor == null)
                {
                    m_ScriptableProcessor = TypeCreator.Create<T>(optionTypeNames[m_ScriptableTypeIndex],
                        new CreateParams(target, m_OptionSerializeDatas[m_ScriptableTypeIndex],
                        HideFlags.HideInHierarchy | HideFlags.HideInInspector));
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
            get
            {
                if (index >= m_ScriptableInfos.Length)
                {
                    Debug.LogWarning("ScriptableInfo index out of range!");
                    return null;
                }

                ScriptableInfo<T> scriptableInfo = m_ScriptableInfos[index];
                scriptableInfo.OnInitProc?.Invoke(m_Target, m_OptionTypeNames);
                return scriptableInfo.ScriptableTarget;
            }
        }
    }
}
