/*
ScriptableProcessor
Copyright © 2021-2024 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public sealed class ScriptableProcessorInspector : SerializationInspectorBase
    {
        internal class ScriptableInstance
        {
            public readonly string scriptableData;
            public readonly List<UnityEngine.Object> scriptableReferences;

            public ScriptableInstance(SerializedProperty scriptableDataProp, SerializedProperty scriptableReferencesProp)
            {
                scriptableData = scriptableDataProp.stringValue;
                scriptableReferences = new List<UnityEngine.Object>();
                for (int i = 0; i < scriptableReferencesProp.arraySize; ++i)
                {
                    UnityEngine.Object scriptableReference = scriptableReferencesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                    scriptableReferences.Add(scriptableReference);
                }
            }
        }
        private readonly float identWidth = 15;
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

        private readonly string m_ScriptableDataDesc = "m_ScriptableData";
        private readonly string m_ScriptableReferencesDesc = "m_ScriptableReferences";

        private int m_Changed;
        private int m_ComponentNum;
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
        private bool m_Header;
        private bool m_Lock;
        public bool IsCustomEnable
        {
            get
            {
                return (OptionScriptableType == ScriptableType.MonoBehaviour ||
                    OptionScriptableType == ScriptableType.ScriptableObject);
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

                SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue);
                SerializedProperty scriptableDataProp = optionDataProp.FindPropertyRelative(m_ScriptableDataDesc);
                SerializedProperty scriptableReferencesProp = optionDataProp.FindPropertyRelative(m_ScriptableReferencesDesc);

                if(!string.IsNullOrEmpty(scriptableDataProp.stringValue))
                {
                    string[] scriptableInstanceIds = new string[scriptableReferencesProp.arraySize];
                    for (int i = 0; i < scriptableReferencesProp.arraySize; ++i)
                    {
                        UnityEngine.Object scriptableReference = scriptableReferencesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                        scriptableInstanceIds[i] = string.Format("{{\"instanceID\":{0}}}", scriptableReference?.GetInstanceID() ?? 0);
                    }

                    return string.Format(scriptableDataProp.stringValue, scriptableInstanceIds);
                }
                return null;
            }
            set
            {
                int selectedIndex = SelectedIndex;
                if (selectedIndex == -1) return;
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(selectedIndex);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);

                SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue);
                SerializedProperty scriptableDataProp = optionDataProp.FindPropertyRelative(m_ScriptableDataDesc);
                SerializedProperty scriptableReferencesProp = optionDataProp.FindPropertyRelative(m_ScriptableReferencesDesc);
                scriptableReferencesProp.ClearArray();

                int index = 0;
                string pattern = "{\"instanceID\":(-?[0-9]+)}|[{}]";
                value = Regex.Replace(value, pattern, (match) => {
                    if (match.Value == "{" || match.Value == "}")
                        return match.Value + match.Value;

                    string instanceStr = match.Value.Substring(14, match.Value.Length - 15);
                    int instanceID = int.Parse(instanceStr);
                    scriptableReferencesProp.InsertArrayElementAtIndex(index);
                    scriptableReferencesProp.GetArrayElementAtIndex(index).objectReferenceValue = EditorUtility.InstanceIDToObject(instanceID);
                    return "{" + (index++) + "}";
                });

                scriptableDataProp.stringValue = value;
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
            m_OptionBeginIndex = IsCustomEnable ? -1 : 0;
            m_HeaderName = string.Format("[SP-{0}]", ScriptableProcessorUtilities.FieldNameForDisplay(name));
        }

        public override SerializationInspectorBase Init(SerializedProperty prop,bool isLock = true,bool isHeader = true)
        {
            m_Lock = isLock;
            m_Header = isHeader;
            m_ScriptableProcessorProp = prop;
            m_TargetComponent = prop.serializedObject.Component();
            m_OptionTypeNamesProp = m_ScriptableProcessorProp.FindPropertyRelative(m_OptionTypeNamesDesc);
            m_ScriptableInfosProp = m_ScriptableProcessorProp.FindPropertyRelative(m_ScriptableInfosDesc);
            m_ScriptableInfosReorderableList = new ReorderableList(prop.serializedObject, m_ScriptableInfosProp, true, false, !isLock, !isLock);
            m_ScriptableProcessorProp.FindPropertyRelative(m_TargetDesc).objectReferenceValue = m_TargetComponent;

            m_ScriptableInfosReorderableList.drawElementCallback = (Rect position, int index, bool selected, bool focused) =>
            {
                position.height = EditorGUIUtility.singleLineHeight;
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
                using (new EditorGUI.PropertyScope(position, null, scriptableInfoProp))
                {
                    string displayName = ScriptableProcessorUtilities.FieldNameForDisplay(m_Name);
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

            if (isLock)
            {
                int scriptableCount = m_ScriptableInfosProp.arraySize;
                if (scriptableCount >= 2)
                {
                    for (int i = scriptableCount - 1; i > 0; --i)
                    {
                        OnRemoveReorderableList(i);
                        m_ScriptableInfosProp.DeleteArrayElementAtIndex(i);
                    }
                }
                else if (scriptableCount == 0)
                {
                    m_ScriptableInfosProp.InsertArrayElementAtIndex(0);
                    OnAddReorderableList(0);
                }
            }

            Refresh();

            return this;
        }

        public override void Draw()
        {
            m_ScriptableProcessorProp.isExpanded = m_Header ? EditorGUILayout.Foldout(m_ScriptableProcessorProp.isExpanded, m_HeaderName) : true;
            if (m_ScriptableProcessorProp.isExpanded)
            {
                if (OptionScriptableType != ScriptableType.NoSerialize && !m_Lock && GUILayout.Button("Clear NoReferences"))
                    OnNoReferencesClick();
                m_ScriptableInfosReorderableList.DoLayoutList();

                try
                {
                    if (OptionScriptableType != ScriptableType.NoSerialize && SelectedIndex != -1)
                    {
                        SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(SelectedIndex);
                        SerializedProperty customScriptableProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                        SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);

                        if (scriptableTypeIndexProp.intValue >= 0)
                        {
                            string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                            SerializedContent serializedContent = SerializedContent.Acquire(currentScriptableTypeName, m_ScriptableProcessorProp.propertyPath, OptionScriptableData, m_TargetComponent);

                            EditorGUI.BeginChangeCheck();
                            ScriptableProcessorUtilities.DoLayoutDrawDefaultInspector(serializedContent.targetProp);
                            if (EditorGUI.EndChangeCheck())
                            {
                                OptionScriptableData = serializedContent.ToJson();
                            }
                        }
                        else if (customScriptableProp.objectReferenceValue != null)
                        {
                            SerializedObject serializedObject = SerializedContent.Acquire(customScriptableProp.objectReferenceValue, m_TargetComponent);
                            ScriptableProcessorUtilities.DoLayoutDrawDefaultInspector(serializedObject);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("customscriptable is null.", MessageType.Warning);
                        }
                    }
                }
                catch(Exception e)
                {
                    SerializedContent.Release(this);
                    Debug.LogException(e);
                }
            }
        }

        public override float Draw(Rect rect)
        {
            rect.xMin -= identWidth;
            Rect buttonPos, lstPos = Rect.zero;
            rect.height = m_Header ? EditorGUIUtility.singleLineHeight : 0;
            m_PropertyHeight = rect.height + EditorGUIUtility.standardVerticalSpacing;
            m_ScriptableProcessorProp.isExpanded = m_Header ? EditorGUI.Foldout(rect, m_ScriptableProcessorProp.isExpanded, m_HeaderName) : true;
            float lstHeight = m_Lock ? m_ScriptableInfosReorderableList.GetHeight() - m_PropertyHeight : m_ScriptableInfosReorderableList.GetHeight();
            rect = EditorGUI.IndentedRect(rect);

            if (m_ScriptableProcessorProp.isExpanded)
            {
                if (OptionScriptableType != ScriptableType.NoSerialize && !m_Lock)
                {
                    buttonPos = new Rect(rect) { y = rect.y + rect.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight };
                    lstPos = new Rect(buttonPos) { y = buttonPos.y + buttonPos.height + EditorGUIUtility.standardVerticalSpacing, height = lstHeight };
                    m_PropertyHeight += (buttonPos.height + EditorGUIUtility.standardVerticalSpacing);

                    if (GUI.Button(buttonPos, "Clear NoReferences"))
                        OnNoReferencesClick();
                }
                else
                {
                    lstPos = new Rect(rect) { y = rect.y + rect.height + EditorGUIUtility.standardVerticalSpacing, height = lstHeight };
                }

                m_PropertyHeight += (lstPos.height + EditorGUIUtility.standardVerticalSpacing);
                m_ScriptableInfosReorderableList.DoList(lstPos);

                try
                {
                    if (OptionScriptableType != ScriptableType.NoSerialize && SelectedIndex != -1)
                    {
                        Rect infoPos = new Rect(lstPos) { y = lstPos.y + lstPos.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight * 2 };
                        SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(SelectedIndex);
                        SerializedProperty customScriptableProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                        SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);

                        if (scriptableTypeIndexProp.intValue >= 0)
                        {
                            string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                            SerializedContent serializedContent = SerializedContent.Acquire(currentScriptableTypeName, m_ScriptableProcessorProp.propertyPath, OptionScriptableData, m_TargetComponent);

                            EditorGUI.BeginChangeCheck(); 
                            m_PropertyHeight += ScriptableProcessorUtilities.DoRectDrawDefaultInspector(lstPos, serializedContent.targetProp);
                            if (EditorGUI.EndChangeCheck())
                            {
                                OptionScriptableData = serializedContent.ToJson();
                            }
                        }
                        else if (customScriptableProp.objectReferenceValue != null)
                        {
                            SerializedObject serializedObject = SerializedContent.Acquire(customScriptableProp.objectReferenceValue, m_TargetComponent);
                            m_PropertyHeight += ScriptableProcessorUtilities.DoRectDrawDefaultInspector(lstPos, serializedObject);
                        }
                        else
                        {
                            EditorGUI.HelpBox(infoPos, "customscriptable is null.", MessageType.Warning);
                            m_PropertyHeight += (infoPos.height + EditorGUIUtility.standardVerticalSpacing);
                        }
                    }
                }
                catch(Exception e)
                {
                    SerializedContent.Release(this);
                    Debug.LogException(e);
                }
            }
            return m_PropertyHeight;
        }

        public void OnGUIChanged()
        {
            if(m_TargetComponent != null)
            {
                int num = m_TargetComponent.GetComponents<Component>().Length;
                if (m_ComponentNum != num)
                {
                    m_ComponentNum = num;
                    m_Changed = 30;
                }
                if (m_Changed > 0)
                {
                    GUI.changed = true;
                    m_Changed--;
                }
            }
        }

        public override void Refresh()
        {
            bool isCustomEnable = IsCustomEnable;
            List<string> scriptableTypeNameLst = new List<string>(TypeExt.GetTypeFullNames(m_ScriptableType));

            if(CheckOptionTypeNames(scriptableTypeNameLst))
            {
                for (int i = 0; i < m_ScriptableInfosProp.arraySize; ++i)
                {
                    SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(i);
                    SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                    SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);

                    int optionDatasArraySize = optionSerializeDatasProp.arraySize;
                    string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                    Dictionary<string, ScriptableInstance> optionScriptableDict = new Dictionary<string, ScriptableInstance>();
                    scriptableTypeIndexProp.intValue = m_OptionBeginIndex;

                    for (int j = 0; j < optionDatasArraySize; ++j)
                    {
                        string scriptableTypeName = m_OptionTypeNamesProp.GetArrayElementAtIndex(j).stringValue;
                        SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(j);
                        SerializedProperty scriptableDataProp = optionDataProp.FindPropertyRelative(m_ScriptableDataDesc);
                        SerializedProperty scriptableReferencesProp = optionDataProp.FindPropertyRelative(m_ScriptableReferencesDesc);
                        ScriptableInstance scriptableInstance = new ScriptableInstance(scriptableDataProp, scriptableReferencesProp);
                        optionScriptableDict.Add(scriptableTypeName, scriptableInstance);
                    }

                    optionSerializeDatasProp.ClearArray();
                    for (int j = 0; j < scriptableTypeNameLst.Count; ++j)
                    {
                        ScriptableInstance scriptableInstance;
                        optionSerializeDatasProp.InsertArrayElementAtIndex(j);
                        if (optionScriptableDict.TryGetValue(scriptableTypeNameLst[j], out scriptableInstance))
                        {
                            SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(j);
                            SerializedProperty scriptableDataProp = optionDataProp.FindPropertyRelative(m_ScriptableDataDesc);
                            SerializedProperty scriptableReferencesProp = optionDataProp.FindPropertyRelative(m_ScriptableReferencesDesc);

                            int index = 0;
                            scriptableDataProp.stringValue = scriptableInstance.scriptableData;
                            foreach (UnityEngine.Object scriptableReference in scriptableInstance.scriptableReferences)
                            {
                                scriptableReferencesProp.InsertArrayElementAtIndex(index);
                                scriptableReferencesProp.GetArrayElementAtIndex(index).objectReferenceValue = scriptableReference;
                                index++;
                            }
                        }

                        if (scriptableTypeNameLst[j] == currentScriptableTypeName)
                            scriptableTypeIndexProp.intValue = j;
                    }
                }

                m_OptionTypeNamesProp.ClearArray();
                foreach (string scriptableTypeName in scriptableTypeNameLst)
                {
                    int index = m_OptionTypeNamesProp.arraySize;
                    m_OptionTypeNamesProp.InsertArrayElementAtIndex(index);
                    m_OptionTypeNamesProp.GetArrayElementAtIndex(index).stringValue = scriptableTypeName;
                }
            }

            if (isCustomEnable) scriptableTypeNameLst.Insert(0, m_CustomOptionName);
            m_ScriptableTypeNames = scriptableTypeNameLst.ToArray();
        }

        public override void Dispose()
        {
            SerializedContent.Release(this);
        }

        private bool CheckOptionTypeNames(List<string> scriptableTypeNameLst)
        {
            if (scriptableTypeNameLst.Count != m_OptionTypeNamesProp.arraySize)
                return true;
            for (int i = 0; i < scriptableTypeNameLst.Count; ++i)
            {
                string scriptableTypeName = m_OptionTypeNamesProp.GetArrayElementAtIndex(i).stringValue;
                if (scriptableTypeNameLst[i] != scriptableTypeName)
                    return true;
            }
            return false;
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

            for (int i = 0; i < m_OptionTypeNamesProp.arraySize; ++i)
            {
                if(optionSerializeDatasProp.arraySize < m_OptionTypeNamesProp.arraySize)
                    optionSerializeDatasProp.InsertArrayElementAtIndex(i);
                SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(i);
                SerializedProperty scriptableDataProp = optionDataProp.FindPropertyRelative(m_ScriptableDataDesc);
                SerializedProperty scriptableReferencesProp = optionDataProp.FindPropertyRelative(m_ScriptableReferencesDesc);
                scriptableDataProp.stringValue = string.Empty;
                scriptableReferencesProp.ClearArray();
            }

            scriptableTypeIndexProp.intValue = m_OptionBeginIndex;
        }

        private void OnRemoveReorderableList(int index)
        {
            SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
            SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
            optionSerializeDatasProp.ClearArray();
        }

        private void OnNoReferencesClick()
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
                        SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(j);
                        SerializedProperty scriptableDataProp = optionDataProp.FindPropertyRelative(m_ScriptableDataDesc);
                        SerializedProperty scriptableReferencesProp = optionDataProp.FindPropertyRelative(m_ScriptableReferencesDesc);
                        scriptableDataProp.stringValue = string.Empty;
                        scriptableReferencesProp.ClearArray();
                    }
                }
            }
        }
    }
}
