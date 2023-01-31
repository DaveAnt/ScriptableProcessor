/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public abstract class SerializedInspectorBase : ISerializedInspector
    {
        public virtual float PropertyHeight => 0;

        public virtual void Draw()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Draw(Rect beginPos)
        {
            throw new System.NotImplementedException();
        }

        public abstract void Init(SerializedObject serializedObject);

        public abstract void Refresh();

        public abstract void Dispose();

        protected string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }

        protected float DoRectDrawDefaultInspector(Rect beginPos, SerializedObject obj)
        {
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();

            SerializedProperty property = obj.GetIterator();
            bool expanded = true;
            float height = 0;

            while (property.NextVisible(expanded))
            {
                using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                {
                    beginPos = new Rect(beginPos) { y = beginPos.y + beginPos.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUI.GetPropertyHeight(property) };
                    height += (beginPos.height + EditorGUIUtility.standardVerticalSpacing);
                    EditorGUI.PropertyField(beginPos, property, true);
                }
                
                expanded = false;
            }
            obj.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                GUI.changed = true;
            return height;
        }


        protected bool DoLayoutDrawDefaultInspector(SerializedObject obj)
        {
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();

            SerializedProperty property = obj.GetIterator();
            bool expanded = true;

            while (property.NextVisible(expanded))
            {
                using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                expanded = false;
            }

            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }
    }
}
