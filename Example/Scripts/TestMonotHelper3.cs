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
    public class TestMonotHelper3 : TestMonoHelperBase
    {
        [SerializeField]
        private bool attr1;

        public override void PrintTest()
        {
            Debug.Log("TestMonotHelper3");
        }
    }
}