/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class ExamplePropertyDrawer : MonoBehaviour
    {
        [ScriptableProcessor]
        public ScriptableProcessor<TestScriptableHelperBase> ScriptabletHelper = new ScriptableProcessor<TestScriptableHelperBase>();
        [ScriptableProcessor]
        public ScriptableProcessor<TestMonoHelperBase> MonoHelper = new ScriptableProcessor<TestMonoHelperBase>();

        void Start()
        {
            ScriptabletHelper[0]?.PrintTest();
            MonoHelper[0]?.PrintTest();
        }
    }
}
