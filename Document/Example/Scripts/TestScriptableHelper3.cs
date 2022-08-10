/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestScriptableHelper3 : TestScriptableHelperBase
    {
        [SerializeField]
        private GameObject[] attr1;

        public override void PrintTest()
        {
            Debug.Log("TestScriptabletHelper3");
        }
    }
}
