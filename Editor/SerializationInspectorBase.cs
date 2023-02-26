/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
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

        public virtual SerializationInspectorBase Init(SerializedProperty serializedProperty)
        {
            throw new System.NotImplementedException();
        }

        public virtual SerializationInspectorBase Init(SerializedObject serializedObject)
        {
            throw new System.NotImplementedException();
        }

        public abstract void Refresh();

        public abstract void Dispose();
    }
}
