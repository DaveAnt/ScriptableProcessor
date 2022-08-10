﻿/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEditor;
using ScriptableProcessor.Editor;

namespace ScriptableProcessor.Example
{
    [CustomEditor(typeof(ExampleInspectorGUI))]
    public class ExampleInspectorGUIEditor : UnityEditor.Editor
    {
        ScriptableProcessorInspector objectHelperEditor = new ScriptableProcessorInspector("ObjectHelper", typeof(TestObjectHelperBase));
        ScriptableProcessorInspector scriptabletHelperEditor = new ScriptableProcessorInspector("ScriptabletHelper", typeof(TestScriptableHelperBase));
        ScriptableProcessorInspector monoHelperEditor = new ScriptableProcessorInspector("MonoHelper", typeof(TestMonoHelperBase));
        

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            objectHelperEditor.Draw();
            scriptabletHelperEditor.Draw();
            monoHelperEditor.Draw();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            objectHelperEditor.Init(serializedObject);
            objectHelperEditor.Refresh();
            scriptabletHelperEditor.Init(serializedObject);
            scriptabletHelperEditor.Refresh();
            monoHelperEditor.Init(serializedObject);
            monoHelperEditor.Refresh();
        }
    }
}
