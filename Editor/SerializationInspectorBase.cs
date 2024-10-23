/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public abstract class SerializationInspectorBase
    {
        public virtual float PropertyHeight => 0;

        public virtual void Draw()
        {
            throw new System.NotImplementedException();
        }

        public virtual float Draw(Rect beginPos)
        {
            throw new System.NotImplementedException();
        }

        public virtual SerializationInspectorBase Init(SerializedProperty serializedProperty, bool isLock = true, bool isHeader = true)
        {
            throw new System.NotImplementedException();
        }

        public virtual SerializationInspectorBase Init(SerializedObject serializedObject, bool isLock = true, bool isHeader = true)
        {
            throw new System.NotImplementedException();
        }

        public abstract void Refresh();

        public abstract void Dispose();
    }
}
