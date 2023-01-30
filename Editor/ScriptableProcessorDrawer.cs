/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableProcessorAttribute))]
    internal sealed class ScriptableProcessorDrawer : PropertyDrawer
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
                }
                scriptableProcessorInspector.Draw(position);
                GUI.changed = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (scriptableProcessorInspector != null)
                return scriptableProcessorInspector.PropertyHeight;
            return base.GetPropertyHeight(property, label);
        }

    }
}
