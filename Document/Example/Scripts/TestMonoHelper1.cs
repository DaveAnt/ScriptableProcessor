/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEngine;

namespace ScriptableProcessor.Example
{
    public class TestMonoHelper1 : TestMonoHelperBase
    {
        [SerializeField]
        private GameObject[] attr1;
        [SerializeField]
        private float attr2;

        public override void PrintTest()
        {
            Debug.Log("TestMonotHelper1");
        }
    }
}
