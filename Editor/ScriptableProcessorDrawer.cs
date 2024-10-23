/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    [CustomPropertyDrawer(typeof(EnumAttribute))]
    internal sealed class EnumDrawer : PropertyDrawer
    {
        Type enumType;
        int enumTotals;
        Enum enumFlags;
        Func<Rect, string, Enum, Enum> EnumFieldFunc;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if(enumType == null)
                {
                    enumType = (attribute as EnumAttribute).enumType;

                    bool isExists = false, isFlags = false;
                    string[] enumNames = Enum.GetNames(enumType);
                    int[] enumValues = new int[enumNames.Length];

                    enumTotals = 0;
                    for (int i = 0; i < enumValues.Length; ++i)
                    {
                        enumValues[i] = Enum.Parse(enumType, enumNames[i]).GetHashCode();
                        enumTotals += enumValues[i];

                        if (property.intValue == enumValues[i])
                            isExists = true;
                    }
                    if(property.intValue == enumTotals) isExists = true;
                    if (!isExists) property.intValue = enumValues[0];
                    object[] customAttributes = enumType.GetCustomAttributes(false);
                    foreach(var custom in customAttributes)
                    {
                        if(custom is FlagsAttribute)
                        {
                            isFlags = true;
                            break;
                        }
                    }
                    EnumFieldFunc = isFlags ? EditorGUI.EnumFlagsField : EditorGUI.EnumPopup;
                    enumFlags = (Enum)Enum.ToObject(enumType, enumTotals);
                }

                Enum enumValue = EnumFieldFunc(position, property.displayName, (Enum)Enum.ToObject(enumType, property.intValue));
                property.intValue = enumValue.HasFlag(enumFlags) ? enumTotals : enumValue.GetHashCode();
            }
        }
    }

    [CustomPropertyDrawer(typeof(ScriptableProcessorAttribute))]
    internal sealed class ScriptableProcessorDrawer : PropertyDrawer
    {
        private float propertyHeight = 18;
        private ScriptableProcessorInspector targetInspector;
        private Color bgColor = new Color(0,0,0,0.2f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                ScriptableProcessorAttribute attr = attribute as ScriptableProcessorAttribute;
                if (targetInspector == null)
                {
                    Type propertyType = ScriptableProcessorUtilities.FetchGenericToTarget(fieldInfo.FieldType, typeof(ScriptableProcessor.ScriptableProcessor<>));
                    targetInspector = new ScriptableProcessorInspector(fieldInfo.Name, propertyType);
                    targetInspector.Init(property, attr.isLock, attr.isHeader);
                }
                if (attr.indentLevel != int.MinValue)
                {
                    EditorGUI.indentLevel = attr.indentLevel;
                }
                int ident = EditorGUI.indentLevel;
                EditorGUI.DrawRect(EditorGUI.IndentedRect(position), bgColor);
                EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                float height = targetInspector.Draw(position);
                EditorGUI.indentLevel = ident;

                if (propertyHeight != height)
                {
                    propertyHeight = height;
                    GUI.changed = true;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            targetInspector?.OnGUIChanged();
            return propertyHeight;
        }
    }
}
