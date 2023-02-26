/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
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
        private ScriptableProcessorInspector targetInspector;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (targetInspector == null)
                {
                    Type propertyType = fieldInfo.FieldType.GetProperties()[0].PropertyType;
                    targetInspector = new ScriptableProcessorInspector(fieldInfo.Name, propertyType);
                    targetInspector.Init(property.serializedObject);
                }
                targetInspector.Draw(position);
                GUI.changed = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (targetInspector != null)
                return targetInspector.PropertyHeight;
            return base.GetPropertyHeight(property, label);
        }
    }
}
