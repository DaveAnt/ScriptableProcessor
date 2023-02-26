/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestMonoHelper3 : TestMonoHelperBase
    {
        [SerializeField]
        private bool attr1;

        public override void PrintTest()
        {
            Debug.Log("TestMonotHelper3");
        }
    }
}