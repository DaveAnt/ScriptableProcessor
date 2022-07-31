/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableProcessor
{
    [CustomPropertyDrawer(typeof(ScriptableProcessorAttribute))]
    public sealed class ScriptableProcessorDrawer : PropertyDrawer
    {
        private ScriptableProcessorInspector scriptableProcessorInspector;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (scriptableProcessorInspector == null)
                {
                    Type propertyType = fieldInfo.FieldType.GetProperties()[0].PropertyType;
                    scriptableProcessorInspector = new ScriptableProcessorInspector(fieldInfo.Name, propertyType);
                    scriptableProcessorInspector.Init(property.serializedObject);
                    scriptableProcessorInspector.Refresh();
                }
                scriptableProcessorInspector.Draw(position);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (scriptableProcessorInspector != null)
                return scriptableProcessorInspector.PropertyHeight;
            return 0;
        }

    }
}
