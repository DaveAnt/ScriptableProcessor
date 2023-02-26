/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public sealed class ScriptableProcessorInspector : SerializationInspectorBase
    {
        private readonly int[] m_ScriptableElementHeight = { 25, 45, 70 };

        private readonly string m_Name;
        private readonly string m_HeaderName;
        private readonly string m_CustomOptionName = "<Custom>";

        private readonly string m_TargetDesc = "m_Target";
        private readonly string m_ScriptableInfosDesc = "m_ScriptableInfos";
        private readonly string m_OptionTypeNamesDesc = "m_OptionTypeNames";

        private readonly string m_CustomScriptableDesc = "m_CustomScriptable";
        private readonly string m_ScriptableTypeIndexDesc = "m_ScriptableTypeIndex";
        private readonly string m_OptionSerializeDatasDesc = "m_OptionSerializeDatas";

        private Component m_TargetComponent;
        private SerializedProperty m_ScriptableInfosProp;
        private SerializedProperty m_OptionTypeNamesProp;
        private SerializedProperty m_ScriptableProcessorProp;
        private ReorderableList m_ScriptableInfosReorderableList;

        private ScriptableType m_OptionScriptableType;
        private string[] m_ScriptableTypeNames;
        private int m_OptionBeginIndex;
        private float m_PropertyHeight;
        private Type m_ScriptableType;

        public bool IsCustomEnable
        {
            get
            {
                return (OptionScriptableType == ScriptableType.MonoBehaviour ||
                    OptionScriptableType == ScriptableType.ScriptableObject);
            }
        }

        public GameObject Target
        {
            get
            {
                return m_TargetComponent.gameObject;
            }
        }

        public ScriptableType OptionScriptableType
        {
            get
            {
                if (m_OptionScriptableType != ScriptableType.None)
                    return m_OptionScriptableType;
                if (!m_ScriptableType.IsSubclassOf(typeof(UnityEngine.Object)))
                    m_OptionScriptableType = ScriptableType.Object;
                else if (m_ScriptableType.IsSubclassOf(typeof(MonoBehaviour)))
                    m_OptionScriptableType = ScriptableType.MonoBehaviour;
                else if (m_ScriptableType.IsSubclassOf(typeof(ScriptableObject)))
                    m_OptionScriptableType = ScriptableType.ScriptableObject;
                else
                    m_OptionScriptableType = ScriptableType.NoSerialize;
                return m_OptionScriptableType;
            }
        }

        public string OptionScriptableData
        {
            get
            {
                int selectedIndex = SelectedIndex;
                if (selectedIndex == -1) return string.Empty;
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(selectedIndex);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
                return optionSerializeDatasProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue).stringValue;
            }
            set
            {
                int selectedIndex = SelectedIndex;
                if (selectedIndex == -1) return;
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(selectedIndex);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
                optionSerializeDatasProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue).stringValue = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (m_ScriptableInfosProp.arraySize < 1)
                    return -1;
                return m_ScriptableInfosReorderableList.index;
            }
        }

        public override float PropertyHeight
        {
            get
            {
                return m_PropertyHeight;
            }
        }

        public ScriptableProcessorInspector(string name, Type scriptableType)
        {
            m_Name = name;
            m_ScriptableType = scriptableType;
            m_HeaderName = "[ScriptableProcessor]-" + m_Name;
            m_OptionBeginIndex = IsCustomEnable ? -1 : 0;
        }

        public override SerializationInspectorBase Init(SerializedObject serializedObject)
        {
            m_TargetComponent = serializedObject.targetObject as Component;
            m_ScriptableProcessorProp = serializedObject.FindProperty(m_Name);
            m_OptionTypeNamesProp = m_ScriptableProcessorProp.FindPropertyRelative(m_OptionTypeNamesDesc);
            m_ScriptableInfosProp = m_ScriptableProcessorProp.FindPropertyRelative(m_ScriptableInfosDesc);
            m_ScriptableInfosReorderableList = new ReorderableList(serializedObject, m_ScriptableInfosProp, true, false, true, true);
            m_ScriptableProcessorProp.FindPropertyRelative(m_TargetDesc).objectReferenceValue = m_TargetComponent.gameObject;

            m_ScriptableInfosReorderableList.drawElementCallback = (Rect position, int index, bool selected, bool focused) =>
            {
                position.height = EditorGUIUtility.singleLineHeight;
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
                using (new EditorGUI.PropertyScope(position, null, scriptableInfoProp))
                {
                    string displayName = ScriptableProcessorUtility.FieldNameForDisplay(m_Name);
                    SerializedProperty customScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                    SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);

                    Rect popupPosition = new Rect(position) { y = position.y + 5 };
                    Rect customPosition = new Rect(position) { y = popupPosition.y + EditorGUIUtility.singleLineHeight + 5 };
                    Rect errorPosition = new Rect(position) { y = customPosition.y + EditorGUIUtility.singleLineHeight + 5 };

                    int scriptableTypeIndex = scriptableTypeIndexProp.intValue - m_OptionBeginIndex;
                    int selectedIndex = EditorGUI.Popup(popupPosition, string.Format("{0}-{1}", displayName, index), scriptableTypeIndex , m_ScriptableTypeNames) + m_OptionBeginIndex;
                    scriptableTypeIndexProp.intValue = selectedIndex;

                    if (selectedIndex < 0)
                    {
                        EditorGUI.PropertyField(customPosition, customScriptablesProp);
                        if (customScriptablesProp.objectReferenceValue == null)
                        {
                            EditorGUI.HelpBox(errorPosition, string.Format("You must set Custom {0} .", displayName), MessageType.Error);
                        }
                    }
                }
                
            };

            m_ScriptableInfosReorderableList.elementHeightCallback = (int index) =>
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty customScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                if (!IsCustomEnable || scriptableTypeIndexProp.intValue >= 0)
                    return m_ScriptableElementHeight[0];
                if (customScriptablesProp.objectReferenceValue != null)
                    return m_ScriptableElementHeight[1];
                return m_ScriptableElementHeight[2];
            };

            m_ScriptableInfosReorderableList.onAddCallback = (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                OnAddReorderableList(m_ScriptableInfosProp.arraySize - 1);
            };

            m_ScriptableInfosReorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                OnRemoveReorderableList(SelectedIndex);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };
            Refresh();

            return this;
        }

        public override void Draw()
        {
            m_ScriptableProcessorProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_ScriptableProcessorProp.isExpanded, m_HeaderName);
            if (m_ScriptableProcessorProp.isExpanded)
            {
                if (OptionScriptableType != ScriptableType.NoSerialize && GUILayout.Button("Clear Trashes"))
                    OnClearTrashesClick();
                m_ScriptableInfosReorderableList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_ScriptableProcessorProp.isExpanded && OptionScriptableType != ScriptableType.NoSerialize && SelectedIndex != -1)
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(SelectedIndex);
                SerializedProperty customScriptableProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);

                if (scriptableTypeIndexProp.intValue >= 0)
                {
                    GameObject target = Target;
                    string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                    ISerializedContentBase serializedContentBase = ScriptableProcessorManager.Acquire(target, currentScriptableTypeName);
                    SerializedProperty prop = serializedContentBase[target];

                    EditorGUI.BeginChangeCheck();
                    ScriptableProcessorUtility.DoLayoutDrawDefaultInspector(prop);
                    if (EditorGUI.EndChangeCheck())
                        OptionScriptableData = serializedContentBase.ToJson(prop);
                    else
                        serializedContentBase.FormJson(prop, OptionScriptableData);
                }
                else if (customScriptableProp.objectReferenceValue != null)
                {
                    SerializedObject serializedObject = ScriptableProcessorManager.Acquire(customScriptableProp.objectReferenceValue);
                    ScriptableProcessorUtility.DoLayoutDrawDefaultInspector(serializedObject);
                }
                else
                {
                    EditorGUILayout.HelpBox("customscriptable is null.", MessageType.Warning);
                }
            }
        }

        public override float Draw(Rect position)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            m_PropertyHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            m_ScriptableProcessorProp.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, m_ScriptableProcessorProp.isExpanded, m_HeaderName);
            Rect buttonPos, lstPos = Rect.zero;
            if (m_ScriptableProcessorProp.isExpanded)
            {
                if (OptionScriptableType != ScriptableType.NoSerialize)
                {
                    buttonPos = new Rect(position) { y = position.y + position.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight };
                    lstPos = new Rect(buttonPos) { y = buttonPos.y + buttonPos.height + EditorGUIUtility.standardVerticalSpacing, height = m_ScriptableInfosReorderableList.GetHeight() };
                    m_PropertyHeight += (buttonPos.height + EditorGUIUtility.standardVerticalSpacing);

                    if (GUI.Button(buttonPos, "Clear Trashes"))
                        OnClearTrashesClick();
                }
                else
                {
                    lstPos = new Rect(position) { y = position.y + position.height + EditorGUIUtility.standardVerticalSpacing, height = m_ScriptableInfosReorderableList.GetHeight() };
                }

                m_PropertyHeight += (lstPos.height + EditorGUIUtility.standardVerticalSpacing);
                m_ScriptableInfosReorderableList.DoList(lstPos);
            }
            EditorGUI.EndFoldoutHeaderGroup();

            if (m_ScriptableProcessorProp.isExpanded && OptionScriptableType != ScriptableType.NoSerialize && SelectedIndex != -1)
            {
                Rect infoPos = new Rect(lstPos) { y = lstPos.y + lstPos.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight * 2 };
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(SelectedIndex);
                SerializedProperty customScriptableProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);

                if (scriptableTypeIndexProp.intValue >= 0)
                {
                    GameObject target = Target;
                    string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                    ISerializedContentBase serializedContentBase = ScriptableProcessorManager.Acquire(target, currentScriptableTypeName);
                    SerializedProperty prop = serializedContentBase[target];

                    EditorGUI.BeginChangeCheck();
                    m_PropertyHeight += ScriptableProcessorUtility.DoRectDrawDefaultInspector(lstPos, prop);
                    if (EditorGUI.EndChangeCheck())
                        OptionScriptableData = serializedContentBase.ToJson(prop);
                    else
                        serializedContentBase.FormJson(prop, OptionScriptableData);
                }
                else if (customScriptableProp.objectReferenceValue != null)
                {
                    SerializedObject serializedObject = ScriptableProcessorManager.Acquire(customScriptableProp.objectReferenceValue);
                    m_PropertyHeight += ScriptableProcessorUtility.DoRectDrawDefaultInspector(lstPos, serializedObject);
                }
                else
                {
                    EditorGUI.HelpBox(infoPos, "customscriptable is null.", MessageType.Warning);
                    m_PropertyHeight += (infoPos.height + EditorGUIUtility.standardVerticalSpacing);
                }
            }
            return m_PropertyHeight;
        }

        public override void Refresh()
        {
            bool isCustomEnable = IsCustomEnable;
            int otionTypeNamesArraySize = m_OptionTypeNamesProp.arraySize;
            List<string> scriptableTypeNameLst = new List<string>(TypeExt.GetTypeFullNames(m_ScriptableType));

            for (int i = 0; i < m_ScriptableInfosProp.arraySize; ++i)
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(i);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
                
                int optionDatasArraySize = optionSerializeDatasProp.arraySize;
                string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                Dictionary<string, string> optionScriptableDict = new Dictionary<string, string>();
                scriptableTypeIndexProp.intValue = m_OptionBeginIndex;

                for (int j = 0; j < optionDatasArraySize; ++j)
                {
                    string scriptableTypeName = m_OptionTypeNamesProp.GetArrayElementAtIndex(j).stringValue;
                    string optionSerializeData = optionSerializeDatasProp.GetArrayElementAtIndex(0).stringValue;
                    optionScriptableDict.Add(scriptableTypeName, optionSerializeData);
                    optionSerializeDatasProp.DeleteArrayElementAtIndex(0);
                }

                for (int j = 0; j < scriptableTypeNameLst.Count; ++j)
                {
                    string optionSerializeData;
                    optionSerializeDatasProp.InsertArrayElementAtIndex(j);
                    optionScriptableDict.TryGetValue(scriptableTypeNameLst[j], out optionSerializeData);
                    optionSerializeDatasProp.GetArrayElementAtIndex(j).stringValue = optionSerializeData;

                    if (scriptableTypeNameLst[j] == currentScriptableTypeName)
                        scriptableTypeIndexProp.intValue = j;
                }
            }

            for (int i = 0; i < otionTypeNamesArraySize; ++i)
                m_OptionTypeNamesProp.DeleteArrayElementAtIndex(0);
            foreach (string scriptableTypeName in scriptableTypeNameLst)
            {
                int index = m_OptionTypeNamesProp.arraySize;
                m_OptionTypeNamesProp.InsertArrayElementAtIndex(index);
                m_OptionTypeNamesProp.GetArrayElementAtIndex(index).stringValue = scriptableTypeName;
            }

            if (isCustomEnable) scriptableTypeNameLst.Insert(0, m_CustomOptionName);
            m_ScriptableTypeNames = scriptableTypeNameLst.ToArray();
        }

        public override void Dispose()
        {
            //m_ScriptableContext?.Dispose();
        }

        private string GetOptionTypeNameByIndex(int index)
        {
            if (index < 0)
                return null;
            else if (index >= m_OptionTypeNamesProp.arraySize)
                return null;
            return m_OptionTypeNamesProp.GetArrayElementAtIndex(index).stringValue;
        }

        private void OnAddReorderableList(int index)
        {
            SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
            SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
            SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
            int optionDatasArraySize = optionSerializeDatasProp.arraySize;
            for (int i = 0; i < m_OptionTypeNamesProp.arraySize; ++i)
            {
                if(i >= optionDatasArraySize)
                    optionSerializeDatasProp.InsertArrayElementAtIndex(i);
                optionSerializeDatasProp.GetArrayElementAtIndex(i).stringValue = string.Empty;
            }

            scriptableTypeIndexProp.intValue = m_OptionBeginIndex;
        }

        private void OnRemoveReorderableList(int index)
        {
            SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
            SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
            int optionDatasArraySize = optionSerializeDatasProp.arraySize;
            for (int i = 0; i < optionDatasArraySize; ++i)
                optionSerializeDatasProp.DeleteArrayElementAtIndex(0);
        }

        private void OnClearTrashesClick()
        {
            for (int i = 0; i < m_ScriptableInfosProp.arraySize; ++i)
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(i);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);

                for (int j = 0; j < optionSerializeDatasProp.arraySize; ++j)
                {
                    if (scriptableTypeIndexProp.intValue != j)
                    {
                        SerializedProperty optionSerializeDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(j);
                        optionSerializeDataProp.stringValue = null;
                    }
                }
            }
        }
    }
}
