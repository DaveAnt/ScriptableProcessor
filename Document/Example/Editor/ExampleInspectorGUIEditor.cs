/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
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

        public void OnDestroy()
        {
            objectHelperEditor.Dispose();
            scriptabletHelperEditor.Dispose();
            monoHelperEditor.Dispose();
        }

        private void OnEnable()
        {
            objectHelperEditor.Init(serializedObject);
            scriptabletHelperEditor.Init(serializedObject);
            monoHelperEditor.Init(serializedObject);
        }
    }
}
