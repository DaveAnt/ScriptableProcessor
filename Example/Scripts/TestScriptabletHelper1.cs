﻿/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestScriptabletHelper1 : TestScriptableHelperBase
    {
        [SerializeField]
        private string[] attr1;
        [SerializeField]
        private int attr2;

        public override void PrintTest()
        {
            Debug.Log("Test1ScriptabletHelper" + attr2);
        }
    }
}