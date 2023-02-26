/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestScriptableHelper1 : TestScriptableHelperBase
    {
        [SerializeField]
        private string[] attr1;
        [SerializeField]
        private int attr2;

        public override void PrintTest()
        {
            Debug.Log("TestScriptabletHelper1");
        }
    }
}