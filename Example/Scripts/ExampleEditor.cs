/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEditor;
using ScriptableProcessor;

namespace ScriptableProcessor.Example
{
    [CustomEditor(typeof(ExampleInspectorGUI))]
    public class ExampleEditor : Editor
    {
        ScriptableProcessorInspector scriptableAttrsEditor = new ScriptableProcessorInspector("ScriptabletHelper", typeof(TestScriptableHelperBase));
        ScriptableProcessorInspector scriptableAttrsEditor2 = new ScriptableProcessorInspector("MonoHelper", typeof(TestMonoHelperBase));

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            scriptableAttrsEditor.Draw();
            scriptableAttrsEditor2.Draw();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {

            scriptableAttrsEditor2.Init(serializedObject);
            scriptableAttrsEditor2.Refresh();
            scriptableAttrsEditor.Init(serializedObject);
            scriptableAttrsEditor.Refresh();
        }
    }
}
