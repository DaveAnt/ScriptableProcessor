/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestMonoHelper2 : TestMonoHelperBase
    {
        [SerializeField]
        private string[] attr1;
        [SerializeField]
        private int attr2;

        public override void PrintTest()
        {
            Debug.Log("TestMonotHelper2");
        }
    }
}
