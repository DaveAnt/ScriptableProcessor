/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableProcessor
{
    [Serializable]
    public class ScriptableAttr<T> where T : class
    {
        [SerializeField]
        private string m_ScriptableTypeName;
        [SerializeField]
        private T m_CustomScriptable, m_OptionScriptable;

        private T m_SelectedScriptable;

        public T GetSelectedScriptable(Transform target,int index)
        {
            if (m_SelectedScriptable == null)
            {
                if (string.IsNullOrEmpty(m_ScriptableTypeName))
                    m_SelectedScriptable = m_CustomScriptable;
                else
                {
                    if (m_OptionScriptable == null)
                    {
                        string scriptableAssetName = TypeCreator.GetScriptableAssetName(target.name, m_ScriptableTypeName, index);
                        m_SelectedScriptable = TypeCreator.Create<T>(m_ScriptableTypeName, scriptableAssetName, target);
                    }
                    else
                    {
                        m_SelectedScriptable = m_OptionScriptable;
                    }
                }
            }

            return m_SelectedScriptable;

        }
    }

    [Serializable]
    public sealed class ScriptableProcessor<T> where T : class
    {
        [SerializeField]
        private Transform m_TargetObject = null; 
        [SerializeField]
        private List<ScriptableAttr<T>> m_ScriptableAttrs = new List<ScriptableAttr<T>>();

        public T this[int index]
        {
            get {
                if (index >= m_ScriptableAttrs.Count)
                {
                    Debug.LogError("ScriptableAttr Index out of range!");
                    return null;
                }

                return m_ScriptableAttrs[index].GetSelectedScriptable(m_TargetObject,index);
            }
        }

    }
}
