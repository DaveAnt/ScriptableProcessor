/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;
using ScriptableProcessor;

namespace ScriptableProcessor.Example
{
    public class ExampleInspectorGUI : MonoBehaviour
    {
        [SerializeField]
        ScriptableProcessor<TestScriptableHelperBase> ScriptabletHelper = new ScriptableProcessor<TestScriptableHelperBase>();
        [SerializeField]
        ScriptableProcessor<TestMonoHelperBase> MonoHelper = new ScriptableProcessor<TestMonoHelperBase>();

        // Start is called before the first frame update
        void Start()
        {
            ScriptabletHelper[0]?.PrintTest();
            MonoHelper[0]?.PrintTest();
            MonoHelper[1]?.PrintTest();
        }
    }
}
