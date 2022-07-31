/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestScriptabletHelper3 : TestScriptableHelperBase
    {
        [SerializeField]
        private GameObject[] attr1;

        public override void PrintTest()
        {
            Debug.Log("Test3ScriptabletHelper");
        }
    }
}
