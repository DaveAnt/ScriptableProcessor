﻿/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEngine;

namespace ScriptableProcessor.Example
{
    [Serializable]
    public class TestObjectHelper2 : TestObjectHelperBase
    {
        public override void PrintTest()
        {
            Debug.Log("TestObjectHelper2");
        }
    }
}
