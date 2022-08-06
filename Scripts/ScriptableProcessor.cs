/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
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
        private int m_ScriptableTypeIndex;
        [SerializeField]
        private string m_ScriptableTypeName;
        [SerializeField]
        private T[] m_CustomScriptables;

        private T m_SelectedScriptable;

        public T this[Transform target]
        {
            get
            {
                if (m_CustomScriptables != null)
                {
                    m_SelectedScriptable = m_CustomScriptables[m_ScriptableTypeIndex];
                    if (m_SelectedScriptable == null && m_ScriptableTypeIndex > 0)
                    {
                        string scriptableAssetName = TypeCreator.GetScriptableAssetName(target.name, m_ScriptableTypeName);
                        m_SelectedScriptable = TypeCreator.Create<T>(m_ScriptableTypeName, scriptableAssetName, target);
                    }
                    m_ScriptableTypeName = null;
                    m_CustomScriptables = null;
                }

                return m_SelectedScriptable;
            }
        }
    }

    [Serializable]
    public sealed class ScriptableProcessor<T> where T : class
    {
        [SerializeField]
        private Transform m_Target;
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

                return m_ScriptableInfos[index][m_Target];
            }
        }

    }
}
