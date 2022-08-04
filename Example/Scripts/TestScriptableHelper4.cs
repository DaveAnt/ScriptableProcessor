/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestScriptableHelper4 : TestScriptableHelperBase
    {
        [SerializeField]
        private float attr1;

        public override void PrintTest()
        {
            Debug.Log("Test4ScriptabletHelper");
        }
    }
}
