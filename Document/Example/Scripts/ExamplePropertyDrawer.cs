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
        public ScriptableProcessor<TestObjectHelperBase> ObjectHelper = new ScriptableProcessor<TestObjectHelperBase>();
        [ScriptableProcessor]
        public ScriptableProcessor<TestScriptableHelperBase> ScriptabletHelper = new ScriptableProcessor<TestScriptableHelperBase>();
        [ScriptableProcessor]
        public ScriptableProcessor<TestMonoHelperBase> MonoHelper = new ScriptableProcessor<TestMonoHelperBase>();

        void Start()
        {
            ObjectHelper[0]?.PrintTest();
            ScriptabletHelper[0]?.PrintTest();
            MonoHelper[0]?.PrintTest();
        }
    }
}
