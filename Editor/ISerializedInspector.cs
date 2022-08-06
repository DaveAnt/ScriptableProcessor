/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public interface ISerializedInspector
    {
        public float PropertyHeight { get; }

        public abstract void Init(SerializedObject serializedObject);

        public abstract void Draw();

        public abstract void Draw(Rect beginPos);

        public abstract void Refresh();
    }
}
