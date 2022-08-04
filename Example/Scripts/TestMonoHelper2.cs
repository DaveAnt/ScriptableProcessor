/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.Collections;
using System.Collections.Generic;
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
