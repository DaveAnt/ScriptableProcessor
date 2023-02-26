/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class ExampleInspectorGUI : MonoBehaviour
    {
        [SerializeField]
        ScriptableProcessor<TestScriptableHelperBase> ScriptabletHelper = new ScriptableProcessor<TestScriptableHelperBase>();
        [SerializeField]
        ScriptableProcessor<TestMonoHelperBase> MonoHelper = new ScriptableProcessor<TestMonoHelperBase>();
        [SerializeField]
        ScriptableProcessor<TestObjectHelperBase> ObjectHelper = new ScriptableProcessor<TestObjectHelperBase>();

        // Start is called before the first frame update
        void Start()
        {
            ObjectHelper[0]?.PrintTest();
            ScriptabletHelper[0]?.PrintTest();
            MonoHelper[0]?.PrintTest();
        }

    }
}
