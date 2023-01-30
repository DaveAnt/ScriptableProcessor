/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public sealed class ScriptableProcessorInspector : SerializedInspectorBase
    {
        private readonly int[] m_ScriptableElementHeight = { 25, 45, 70 };

        private readonly string m_Name;
        private readonly string m_HeaderName;
        private readonly string m_CustomOptionName = "<Custom>";

        private readonly string m_TargetDesc = "m_Target";
        private readonly string m_ScriptableInfosDesc = "m_ScriptableInfos";
        private readonly string m_OptionTypeNamesDesc = "m_OptionTypeNames";

        private readonly string m_ScriptableTypeIndexDesc = "m_ScriptableTypeIndex";
        private readonly string m_CustomScriptableDesc = "m_CustomScriptable";
        private readonly string m_OptionScriptablesDesc = "m_OptionScriptables";
        private readonly string m_OptionSerializeDatasDesc = "m_OptionSerializeDatas";

        private Component m_TargetComponent;
        private ISerializedInspector m_ScriptableSerializeInspector;
        private SerializedObject m_OptionScriptableSerialize;

        private SerializedProperty m_ScriptableInfosProp;
        private SerializedProperty m_OptionTypeNamesProp;
        private SerializedProperty m_ScriptableProcessorProp;
        private ReorderableList m_ScriptableInfosReorderableList;

        private Dictionary<UnityEngine.Object, SerializedObject> m_ScriptableSerializeDict;
        private Dictionary<string, ISerializedInspector> m_SerializeInspectorDict;

        private string[] m_ScriptableTypeNames;
        private bool m_IsScriptableObject;
        private bool m_IsMonoBehaviour;
        private string m_PrefabAssetPath;
        private float m_PropertyHeight;
        private Type m_ScriptableType;

        public bool IsCustomEnable
        {
            get
            {
                return (m_IsMonoBehaviour || m_IsScriptableObject);
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
            m_IsMonoBehaviour = m_ScriptableType.IsSubclassOf(typeof(MonoBehaviour));
            m_IsScriptableObject = m_ScriptableType.IsSubclassOf(typeof(ScriptableObject));
            m_ScriptableSerializeDict = new Dictionary<UnityEngine.Object, SerializedObject>();
            m_SerializeInspectorDict = new Dictionary<string, ISerializedInspector>();
        }

        public override void Init(SerializedObject serializedObject)
        {
            serializedObject.Update();
            m_TargetComponent = serializedObject.targetObject as Component;
            m_ScriptableProcessorProp = serializedObject.FindProperty(m_Name);
            m_OptionTypeNamesProp = m_ScriptableProcessorProp.FindPropertyRelative(m_OptionTypeNamesDesc);
            m_ScriptableInfosProp = m_ScriptableProcessorProp.FindPropertyRelative(m_ScriptableInfosDesc);
            m_PrefabAssetPath = ScriptableProcessorUtility.GetScriptableAssetPath(m_TargetComponent.gameObject);
            m_ScriptableInfosReorderableList = new ReorderableList(serializedObject, m_ScriptableInfosProp, true, false, true, true);
            m_ScriptableProcessorProp.FindPropertyRelative(m_TargetDesc).objectReferenceValue = m_TargetComponent.gameObject;

            m_ScriptableInfosReorderableList.drawElementCallback = (Rect position, int index, bool selected, bool focused) =>
            {
                position.height = EditorGUIUtility.singleLineHeight;
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
                using (new EditorGUI.PropertyScope(position, null, scriptableInfoProp))
                {
                    string displayName = FieldNameForDisplay(m_Name);
                    SerializedProperty customScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
                    SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);

                    Rect popupPosition = new Rect(position) { y = position.y + 5 };
                    Rect customPosition = new Rect(position) { y = popupPosition.y + EditorGUIUtility.singleLineHeight + 5 };
                    Rect errorPosition = new Rect(position) { y = customPosition.y + EditorGUIUtility.singleLineHeight + 5 };

                    int scriptableTypeIndex = IsCustomEnable ? scriptableTypeIndexProp.intValue + 1 : scriptableTypeIndexProp.intValue;
                    int selectedIndex = EditorGUI.Popup(popupPosition, string.Format("{0}-{1}", displayName, index), scriptableTypeIndex , m_ScriptableTypeNames);

                    if (IsCustomEnable)
                    {
                        selectedIndex = selectedIndex - 1;

                        if (selectedIndex < 0)
                        {
                            EditorGUI.PropertyField(customPosition, customScriptablesProp);
                            if (customScriptablesProp.objectReferenceValue == null)
                                EditorGUI.HelpBox(errorPosition, string.Format("You must set Custom {0} .", displayName), MessageType.Error);
                        }
                    }

                    if (selectedIndex != scriptableTypeIndexProp.intValue)
                        scriptableTypeIndexProp.intValue = selectedIndex;
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

            serializedObject.ApplyModifiedProperties();
        }

        public override void Draw()
        {
            bool isCustomEnable = IsCustomEnable;
            m_ScriptableProcessorProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_ScriptableProcessorProp.isExpanded, m_HeaderName);
            if (m_ScriptableProcessorProp.isExpanded)
            {
                if (isCustomEnable && GUILayout.Button("Clear Trashes"))
                    OnClearTrashesClick();
                m_ScriptableInfosReorderableList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_ScriptableProcessorProp.isExpanded && isCustomEnable && SelectedIndex != -1)
            {
                bool showScriptable = OnSerializeScriptable();
                if (showScriptable)
                {
                    EditorGUI.BeginChangeCheck();
                    if (m_ScriptableSerializeInspector != null)
                    {
                        m_ScriptableSerializeInspector.Init(m_OptionScriptableSerialize);
                        m_ScriptableSerializeInspector.Draw();
                    }
                    else
                    {
                        DoLayoutDrawDefaultInspector(m_OptionScriptableSerialize);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        SaveScriptableSerialize();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("customscriptable is null.", MessageType.Warning);
                }
            }
        }

        public override void Draw(Rect position)
        {
            bool isCustomEnable = IsCustomEnable;
            position.height = EditorGUIUtility.singleLineHeight;
            m_PropertyHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            m_ScriptableProcessorProp.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, m_ScriptableProcessorProp.isExpanded, m_HeaderName);
            Rect buttonPos, lstPos = Rect.zero;
            if (m_ScriptableProcessorProp.isExpanded)
            {
                if (isCustomEnable)
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

            if (m_ScriptableProcessorProp.isExpanded && isCustomEnable && SelectedIndex != -1)
            {
                bool showScriptable = OnSerializeScriptable();
                Rect infoPos = new Rect(lstPos) { y = lstPos.y + lstPos.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight * 2 };
                if (showScriptable)
                {
                    EditorGUI.BeginChangeCheck();
                    if (m_ScriptableSerializeInspector != null)
                    {
                        m_ScriptableSerializeInspector.Init(m_OptionScriptableSerialize);
                        m_ScriptableSerializeInspector.Draw(lstPos);
                    }
                    else
                    {
                        m_PropertyHeight += DoRectDrawDefaultInspector(lstPos, m_OptionScriptableSerialize);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        SaveScriptableSerialize();
                    }
                }
                else
                {
                    EditorGUI.HelpBox(infoPos, "customscriptable is null.", MessageType.Warning);
                    m_PropertyHeight += (infoPos.height + EditorGUIUtility.standardVerticalSpacing);
                }
            }
        }

        public override void Refresh()
        {
            int otionTypeNamesArraySize = m_OptionTypeNamesProp.arraySize;
            List<string> scriptableTypeNameLst = new List<string>(TypeExt.GetTypeFullNames(m_ScriptableType));

            for (int i = 0; i < m_ScriptableInfosProp.arraySize; ++i)
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(i);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                Dictionary<string, object[]> optionScriptableDict = new Dictionary<string, object[]>();
                scriptableTypeIndexProp.intValue = 0;

                if (IsCustomEnable)
                {
                    SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
                    SerializedProperty optionScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_OptionScriptablesDesc);
                    ScriptableProcessorUtility.SetProps(optionScriptablesProp, optionSerializeDatasProp);
                    scriptableTypeIndexProp.intValue = -1;

                    for (int j = 0; j < otionTypeNamesArraySize; ++j)
                    {
                        string scriptableTypeName = m_OptionTypeNamesProp.GetArrayElementAtIndex(j).stringValue;
                        SerializedProperty[] optionInfosProp = ScriptableProcessorUtility.GetArrayElementAtIndex(0);
                        object[] optionDataValues = { optionInfosProp[0].objectReferenceValue, optionInfosProp[1].stringValue };
                        optionScriptableDict.Add(scriptableTypeName, optionDataValues);
                        ScriptableProcessorUtility.DeleteArrayElementAtIndex(0);
                    }
                }

                for (int j = 0; j < scriptableTypeNameLst.Count; ++j)
                {
                    if (IsCustomEnable)
                    {
                        object[] optionDataValues;
                        ScriptableProcessorUtility.InsertArrayElementAtIndex(j);
                        if (optionScriptableDict.TryGetValue(scriptableTypeNameLst[j], out optionDataValues))
                        {
                            SerializedProperty[] optionInfosProp = ScriptableProcessorUtility.GetArrayElementAtIndex(ScriptableProcessorUtility.ArraySize - 1);
                            optionInfosProp[0].objectReferenceValue = (UnityEngine.Object)optionDataValues[0];
                            optionInfosProp[1].stringValue = (string)optionDataValues[1];
                        }
                    }

                    if (scriptableTypeNameLst[j] == currentScriptableTypeName)
                        scriptableTypeIndexProp.intValue = j;
                }
            }

            for (int i = 0; i < otionTypeNamesArraySize; ++i)
                m_OptionTypeNamesProp.DeleteArrayElementAtIndex(0);
            foreach (string scriptableTypeName in scriptableTypeNameLst)
            {
                m_OptionTypeNamesProp.InsertArrayElementAtIndex(m_OptionTypeNamesProp.arraySize);
                m_OptionTypeNamesProp.GetArrayElementAtIndex(m_OptionTypeNamesProp.arraySize - 1).stringValue = scriptableTypeName;
            }

            if (IsCustomEnable) scriptableTypeNameLst.Insert(0, m_CustomOptionName);
            m_ScriptableTypeNames = scriptableTypeNameLst.ToArray();
            m_ScriptableSerializeInspector?.Refresh();
        }

        public override void Dispose()
        {
            foreach (var scriptableValue in m_ScriptableSerializeDict)
                scriptableValue.Value.Dispose();
            m_ScriptableSerializeDict.Clear();
            m_SerializeInspectorDict.Clear();
        }

        private string GetOptionTypeNameByIndex(int index)
        {
            if (index < 0)
                return null;
            else if (index >= m_OptionTypeNamesProp.arraySize)
                return null;
            return m_OptionTypeNamesProp.GetArrayElementAtIndex(index).stringValue;
        }

        private SerializedObject GetScriptableSerialize(UnityEngine.Object scriptableObject)
        {
            if (scriptableObject == null)
                return null;

            SerializedObject scriptableSerialize;
            if (!m_ScriptableSerializeDict.TryGetValue(scriptableObject, out scriptableSerialize))
            {
                scriptableSerialize = new SerializedObject(scriptableObject);
                m_ScriptableSerializeDict.Add(scriptableObject, scriptableSerialize);
            }
            return scriptableSerialize;
        }

        private void SaveScriptableSerialize()
        {
            SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(SelectedIndex);
            SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
            SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
            SerializedProperty optionScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_OptionScriptablesDesc);

            if (scriptableTypeIndexProp.intValue >= 0)
            {
                SerializedProperty optionScriptableProp = optionScriptablesProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue);
                SerializedProperty optionDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue);
                optionDataProp.stringValue = JsonUtility.ToJson(optionScriptableProp.objectReferenceValue);
            }
        }

        private bool OnSerializeScriptable()
        {
            Type scriptableType = null;
            SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(SelectedIndex);
            SerializedProperty customScriptableProp = scriptableInfoProp.FindPropertyRelative(m_CustomScriptableDesc);
            SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
            SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
            SerializedProperty optionScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_OptionScriptablesDesc);
            
            if (scriptableTypeIndexProp.intValue >= 0)
            {
                string currentScriptableTypeName = GetOptionTypeNameByIndex(scriptableTypeIndexProp.intValue);
                SerializedProperty optionSerializeDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue);
                SerializedProperty optionScriptableProp = optionScriptablesProp.GetArrayElementAtIndex(scriptableTypeIndexProp.intValue);

                if (optionScriptableProp.objectReferenceValue == null)
                {
                    optionScriptableProp.objectReferenceValue = TypeCreator.Create<UnityEngine.Object>(currentScriptableTypeName, new CreateParams(m_TargetComponent.gameObject, null, HideFlags.None), (instance, ScriptableType) => {
                        if (m_IsScriptableObject) AssetDatabase.AddObjectToAsset(instance, m_PrefabAssetPath);
                        instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
                    });
                }

                if (!string.IsNullOrEmpty(optionSerializeDataProp.stringValue))
                    JsonUtility.FromJsonOverwrite(optionSerializeDataProp.stringValue, optionScriptableProp.objectReferenceValue);

                m_OptionScriptableSerialize = GetScriptableSerialize(optionScriptableProp.objectReferenceValue);
                scriptableType = AssemblyExt.GetType(currentScriptableTypeName);
            }
            else if (customScriptableProp.objectReferenceValue != null)
            {
                m_OptionScriptableSerialize = GetScriptableSerialize(customScriptableProp.objectReferenceValue);
                scriptableType = customScriptableProp.GetType();
            }

            if (scriptableType != null)
            {
                if (!m_SerializeInspectorDict.TryGetValue(scriptableType.FullName, out m_ScriptableSerializeInspector))
                {
                    Attribute[] attributes = Attribute.GetCustomAttributes(scriptableType, false);
                    foreach (Attribute attribute in attributes)
                    {
                        if (attribute is ScriptableInspectorAttribute)
                        {
                            Type type = (attribute as ScriptableInspectorAttribute).type;
                            m_ScriptableSerializeInspector = (ISerializedInspector)Activator.CreateInstance(type);
                            m_SerializeInspectorDict.Add(scriptableType.FullName, m_ScriptableSerializeInspector);
                            break;
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private void OnAddReorderableList(int index)
        {
            SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
            SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
            scriptableTypeIndexProp.intValue = 0;

            if (IsCustomEnable)
            {
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
                SerializedProperty optionScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_OptionScriptablesDesc);

                if (optionSerializeDatasProp.arraySize != m_ScriptableTypeNames.Length - 1)
                {
                    for (int i = 0; i < m_ScriptableTypeNames.Length - 1; ++i)
                    {
                        optionSerializeDatasProp.InsertArrayElementAtIndex(i);
                        optionScriptablesProp.InsertArrayElementAtIndex(i);
                    }
                }
                for (int i = 0; i < m_ScriptableTypeNames.Length - 1; ++i)
                {
                    optionSerializeDatasProp.GetArrayElementAtIndex(i).stringValue = null;
                    optionScriptablesProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
                }
                scriptableTypeIndexProp.intValue = -1;
            }
        }

        private void OnRemoveReorderableList(int index)
        {
            if (IsCustomEnable)
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(index);
                SerializedProperty optionScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_OptionScriptablesDesc);

                for (int i = 0; i < m_ScriptableTypeNames.Length - 1; ++i)
                {
                    SerializedProperty optionScriptableProp = optionScriptablesProp.GetArrayElementAtIndex(i);
                    if (optionScriptableProp.objectReferenceValue != null)
                        UnityEngine.Object.DestroyImmediate(optionScriptableProp.objectReferenceValue, true);
                }
            }
        }

        private void OnClearTrashesClick()
        {
            for (int i = 0; i < m_ScriptableInfosProp.arraySize; ++i)
            {
                SerializedProperty scriptableInfoProp = m_ScriptableInfosProp.GetArrayElementAtIndex(i);
                SerializedProperty scriptableTypeIndexProp = scriptableInfoProp.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty optionSerializeDatasProp = scriptableInfoProp.FindPropertyRelative(m_OptionSerializeDatasDesc);
                SerializedProperty optionScriptablesProp = scriptableInfoProp.FindPropertyRelative(m_OptionScriptablesDesc);
                
                for (int j = 0; j < optionScriptablesProp.arraySize; ++j)
                {
                    if (scriptableTypeIndexProp.intValue != j)
                    {
                        SerializedProperty optionSerializeDataProp = optionSerializeDatasProp.GetArrayElementAtIndex(j);
                        SerializedProperty optionScriptableProp = optionScriptablesProp.GetArrayElementAtIndex(j);
                        
                        if (optionScriptableProp.objectReferenceValue != null)
                        {
                            UnityEngine.Object.DestroyImmediate(optionScriptableProp.objectReferenceValue,true);
                            optionScriptableProp.objectReferenceValue = null;
                        }
                        optionSerializeDataProp.stringValue = null;
                    }
                }
            }
        }
    }
}
