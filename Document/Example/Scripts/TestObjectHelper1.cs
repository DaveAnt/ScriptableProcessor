/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEngine;

namespace ScriptableProcessor.Example
{
    [Serializable]
    public class TestObjectHelper1 : TestObjectHelperBase
    {
        [SerializeField]
        private string[] attr1;
        [SerializeField]
        private int attr2;
        [SerializeField]
        private string[] attr3;

        public override void PrintTest()
        {
            Debug.Log("TestObjectHelper1");
        }
    }
}
