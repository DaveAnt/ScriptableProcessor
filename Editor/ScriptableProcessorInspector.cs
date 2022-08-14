/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public enum ScriptableProcessorStatus
    {
        Null,
        NoPrefab,
        ShowScriptable,
    }

    public sealed class ScriptableProcessorInspector : SerializedInspectorBase
    {
        private readonly int[] m_ScriptableElementHeight = { 25, 45, 70 };

        private readonly string m_Name;
        private readonly string m_HeaderName;
        private readonly string m_CustomOptionName = "<Custom>";

        private readonly string m_TransformDesc = "m_Transform";
        private readonly string m_ScriptableInfosDesc = "m_ScriptableInfos";
        private readonly string m_CustomScriptablesDesc = "m_CustomScriptables";
        private readonly string m_ScriptableTypeNameDesc = "m_ScriptableTypeName";
        private readonly string m_ScriptableTypeIndexDesc = "m_ScriptableTypeIndex";

        private Transform m_Transform;
        private ISerializedInspector m_SerializedInspector;
        private SerializedObject m_SelectedSerializedObject;

        private SerializedProperty m_ScriptableProperty;
        private SerializedProperty m_ScriptableInfosProperty;
        private ReorderableList m_ScriptableInfosReorderableList;

        private Dictionary<UnityEngine.Object, SerializedObject> m_SerializedScriptableDicts;
        private Dictionary<string, ISerializedInspector> m_SerializedInspectorDicts;

        private Type m_ScriptableType;
        private string m_TargetAssetPath;
        private string[] m_ScriptableTypeNames;
        private float m_PropertyHeight;
        private bool m_IsScriptableObject;
        private bool m_IsMonoBehaviour;

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
                if (m_ScriptableInfosProperty.arraySize < 1)
                    return -1;
                return m_ScriptableInfosReorderableList.index;
            }
        }

        public SerializedObject this[UnityEngine.Object scriptableObject]
        {
            get
            {
                if (scriptableObject == null)
                    return null;

                SerializedObject serializedObject;
                if (!m_SerializedScriptableDicts.TryGetValue(scriptableObject, out serializedObject))
                {
                    serializedObject = new SerializedObject(scriptableObject);
                    m_SerializedScriptableDicts.Add(scriptableObject, serializedObject);
                }
                return serializedObject;
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
            m_SerializedScriptableDicts = new Dictionary<UnityEngine.Object, SerializedObject>();
            m_SerializedInspectorDicts = new Dictionary<string, ISerializedInspector>();
        }

        public override void Init(SerializedObject serializedObject)
        {
            serializedObject.Update();
            m_Transform = (serializedObject.targetObject as Component).transform;
            m_TargetAssetPath = ScriptableProcessorUtility.GetAssetPath(m_Transform);

            m_ScriptableProperty = serializedObject.FindProperty(m_Name);
            m_ScriptableInfosProperty = m_ScriptableProperty.FindPropertyRelative(m_ScriptableInfosDesc);
            m_ScriptableInfosReorderableList = new ReorderableList(serializedObject, m_ScriptableInfosProperty, true, false, true, true);
            m_ScriptableProperty.FindPropertyRelative(m_TransformDesc).objectReferenceValue = m_Transform;

            m_ScriptableInfosReorderableList.drawElementCallback = (Rect position, int index, bool selected, bool focused) =>
            {
                position.height = EditorGUIUtility.singleLineHeight;
                var scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(index);
                using (new EditorGUI.PropertyScope(position, null, scriptableInfo))
                {
                    string displayName = FieldNameForDisplay(m_Name);
                    var customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
                    var scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                    var scriptableTypeName = scriptableInfo.FindPropertyRelative(m_ScriptableTypeNameDesc);

                    var popupPosition = new Rect(position) { y = position.y + 5 };
                    var customPosition = new Rect(position) { y = popupPosition.y + EditorGUIUtility.singleLineHeight + 5 };
                    var errorPosition = new Rect(position) { y = customPosition.y + EditorGUIUtility.singleLineHeight + 5 };


                    int selectedIndex = EditorGUI.Popup(popupPosition, string.Format("{0}-{1}", displayName, index), scriptableTypeIndex.intValue, m_ScriptableTypeNames);
                    if (selectedIndex != scriptableTypeIndex.intValue)
                    {
                        scriptableTypeName.stringValue = m_ScriptableTypeNames[selectedIndex];
                        scriptableTypeIndex.intValue = selectedIndex;
                    }

                    if (selectedIndex <= 0 && IsCustomEnable)
                    {
                        scriptableTypeName.stringValue = null;
                        EditorGUI.PropertyField(customPosition, customScriptables.GetArrayElementAtIndex(0));
                        if (customScriptables.GetArrayElementAtIndex(0).objectReferenceValue == null)
                        {
                            EditorGUI.HelpBox(errorPosition, string.Format("You must set Custom {0} .", displayName), MessageType.Error);
                        }
                    }
                }
            };

            m_ScriptableInfosReorderableList.elementHeightCallback = (int index) =>
            {
                SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(index);
                SerializedProperty scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
                if (!IsCustomEnable || scriptableTypeIndex.intValue > 0)
                    return m_ScriptableElementHeight[0];
                if (customScriptables.GetArrayElementAtIndex(0).objectReferenceValue != null)
                    return m_ScriptableElementHeight[1];
                return m_ScriptableElementHeight[2];
            };

            m_ScriptableInfosReorderableList.onAddCallback = (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                OnAddReorderableList(m_ScriptableInfosProperty.arraySize - 1);
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
            m_ScriptableProperty.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_ScriptableProperty.isExpanded, m_HeaderName);
            if (m_ScriptableProperty.isExpanded)
            {
                if (isCustomEnable && GUILayout.Button("Clear NoReference"))
                    OnClearNoReference();
                m_ScriptableInfosReorderableList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_ScriptableProperty.isExpanded && isCustomEnable && SelectedIndex != -1)
            {
                ScriptableProcessorStatus state = OnSerializeScriptable();
                switch (state)
                {
                    case ScriptableProcessorStatus.NoPrefab:
                        EditorGUILayout.HelpBox("can't editor attributes，You must set the target as a prefab.", MessageType.Warning);
                        break;
                    case ScriptableProcessorStatus.Null:
                        EditorGUILayout.HelpBox("customscriptable is null.", MessageType.Warning);
                        break;
                    case ScriptableProcessorStatus.ShowScriptable:
                        if (m_SerializedInspector != null)
                        {
                            m_SerializedInspector.Init(m_SelectedSerializedObject);
                            m_SerializedInspector.Draw();
                        }
                        else
                        {
                            DoLayoutDrawDefaultInspector(m_SelectedSerializedObject);
                        }
                        break;
                }
            }
        }

        public override void Draw(Rect position)
        {
            bool isCustomEnable = IsCustomEnable;
            position.height = EditorGUIUtility.singleLineHeight;
            m_PropertyHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            m_ScriptableProperty.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, m_ScriptableProperty.isExpanded, m_HeaderName);
            Rect buttonPos, lstPos = Rect.zero;
            if (m_ScriptableProperty.isExpanded)
            {
                buttonPos = new Rect(position) { y = position.y + position.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight };
                m_PropertyHeight += (buttonPos.height + EditorGUIUtility.standardVerticalSpacing);

                if (isCustomEnable && GUI.Button(buttonPos, "Clear NoReference"))
                    OnClearNoReference();

                lstPos = new Rect(buttonPos) { y = buttonPos.y + buttonPos.height + EditorGUIUtility.standardVerticalSpacing, height = m_ScriptableInfosReorderableList.GetHeight() };
                m_PropertyHeight += (lstPos.height + EditorGUIUtility.standardVerticalSpacing);
                m_ScriptableInfosReorderableList.DoList(lstPos);
            }
            EditorGUI.EndFoldoutHeaderGroup();

            if (m_ScriptableProperty.isExpanded && isCustomEnable && SelectedIndex != -1)
            {
                ScriptableProcessorStatus state = OnSerializeScriptable();
                Rect infoPos = new Rect(lstPos) { y = lstPos.y + lstPos.height + EditorGUIUtility.standardVerticalSpacing, height = EditorGUIUtility.singleLineHeight * 2 };
                switch (state)
                {
                    case ScriptableProcessorStatus.NoPrefab:
                        EditorGUI.HelpBox(infoPos, "can't editor attributes，You must set the target as a prefab.", MessageType.Warning);
                        m_PropertyHeight += (infoPos.height + EditorGUIUtility.standardVerticalSpacing);
                        break;
                    case ScriptableProcessorStatus.Null:
                        EditorGUI.HelpBox(infoPos, "customscriptable is null.", MessageType.Warning);
                        m_PropertyHeight += (infoPos.height + EditorGUIUtility.standardVerticalSpacing);
                        break;
                    case ScriptableProcessorStatus.ShowScriptable:
                        if (m_SerializedInspector != null)
                        {
                            m_SerializedInspector.Init(m_SelectedSerializedObject);
                            m_SerializedInspector.Draw(lstPos);
                        }
                        else
                        {
                            m_PropertyHeight += DoRectDrawDefaultInspector(lstPos, m_SelectedSerializedObject);
                        }
                        break;
                }
            }
        }

        public override void Refresh()
        {
            bool isCustomEnable = IsCustomEnable;
            List<string> scriptableTypeNameList = new List<string>(TypeExt.GetTypeFullNames(m_ScriptableType));
            if(isCustomEnable) { scriptableTypeNameList.Insert(0, m_CustomOptionName); };
            m_ScriptableTypeNames = scriptableTypeNameList.ToArray();

            for (int i = 0; i < m_ScriptableInfosProperty.arraySize; ++i)
            {
                SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(i);
                SerializedProperty scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty scriptableTypeName = scriptableInfo.FindPropertyRelative(m_ScriptableTypeNameDesc);
                SerializedProperty customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
                scriptableTypeName.stringValue = m_ScriptableTypeNames[scriptableTypeIndex.intValue];

                if (isCustomEnable) 
                    OnRefreshCustomScriptables(customScriptables);

                if (isCustomEnable && scriptableTypeIndex.intValue == 0)
                    scriptableTypeName.stringValue = null;
            }
            m_SerializedInspector?.Refresh();
        }

        private void OnRefreshCustomScriptables(SerializedProperty customScriptables)
        {
            Dictionary<string, UnityEngine.Object> customScriptableDicts = new Dictionary<string, UnityEngine.Object>();
            if (customScriptables.arraySize > 1)
            {
                UnityEngine.Object scriptableObject = customScriptables.GetArrayElementAtIndex(0).objectReferenceValue;
                if (scriptableObject != null)
                    customScriptableDicts.Add(m_CustomOptionName, scriptableObject);
            }

            for (int i = 1; i < customScriptables.arraySize; ++i)
            {
                UnityEngine.Object scriptableObject = customScriptables.GetArrayElementAtIndex(i).objectReferenceValue;
                if (scriptableObject != null)
                    customScriptableDicts.Add(scriptableObject.GetType().FullName, scriptableObject);
            }

            int offsetCount = customScriptables.arraySize - m_ScriptableTypeNames.Length;
            for (int i = 0; i < Math.Abs(offsetCount); ++i)
            {
                if (offsetCount < 0)
                    customScriptables.InsertArrayElementAtIndex(0);
                else
                    customScriptables.DeleteArrayElementAtIndex(0);
            }

            for (int i = 0; i < m_ScriptableTypeNames.Length; ++i)
            {
                UnityEngine.Object assetObject;
                if (customScriptableDicts.TryGetValue(m_ScriptableTypeNames[i], out assetObject))
                {
                    customScriptables.GetArrayElementAtIndex(i).objectReferenceValue = assetObject;
                    assetObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                }
                else
                {
                    customScriptables.GetArrayElementAtIndex(i).objectReferenceValue = null;
                }
            }
        }

        private ScriptableProcessorStatus OnSerializeScriptable()
        {
            Type scriptableType = null;
            SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(SelectedIndex);
            SerializedProperty customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
            SerializedProperty scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
            SerializedProperty scriptableTypeName = scriptableInfo.FindPropertyRelative(m_ScriptableTypeNameDesc);
            SerializedProperty scriptableTypeObject = customScriptables.GetArrayElementAtIndex(scriptableTypeIndex.intValue);
            string scriptableAssetName = TypeCreator.GetScriptableAssetName(m_Transform.name, scriptableTypeName.stringValue);

            if (scriptableTypeIndex.intValue > 0)
            {
                if (string.IsNullOrEmpty(m_TargetAssetPath))
                    return ScriptableProcessorStatus.NoPrefab;

                scriptableType = AssemblyExt.GetType(scriptableTypeName.stringValue);
                if (scriptableTypeObject.objectReferenceValue == null)
                {
                    UnityEngine.Object scriptableObject = CreateAssetObject(scriptableTypeName.stringValue, scriptableAssetName);
                    scriptableTypeObject.objectReferenceValue = scriptableObject;
                }
            }
            else if (scriptableTypeObject.objectReferenceValue != null)
            {
                scriptableType = scriptableTypeObject.GetType();
            }

            m_SelectedSerializedObject = this[scriptableTypeObject.objectReferenceValue];

            if (scriptableType != null)
            {
                if (!m_SerializedInspectorDicts.TryGetValue(scriptableType.FullName, out m_SerializedInspector))
                {
                    Attribute[] attributes = Attribute.GetCustomAttributes(scriptableType, false);
                    foreach (Attribute attribute in attributes)
                    {
                        if (attribute is ScriptableInspectorAttribute)
                        {
                            Type type = (attribute as ScriptableInspectorAttribute).type;
                            m_SerializedInspector = (ISerializedInspector)Activator.CreateInstance(type);
                            m_SerializedInspectorDicts.Add(scriptableType.FullName, m_SerializedInspector);
                            break;
                        }
                    }
                }

                return ScriptableProcessorStatus.ShowScriptable;
            }
            else
            {
                return ScriptableProcessorStatus.Null;
            }
        }

        private UnityEngine.Object CreateAssetObject(string scriptableTypeName, string scriptableAssetName)
        {
            UnityEngine.Object assetObject = null;
            if (m_IsScriptableObject)
            {
                assetObject = TypeCreator.Create<UnityEngine.Object>(scriptableTypeName, scriptableAssetName);
                AssetDatabase.AddObjectToAsset(assetObject, m_TargetAssetPath);
            }
            else if (m_IsMonoBehaviour)
            {
                assetObject = TypeCreator.Create<UnityEngine.Object>(scriptableTypeName, scriptableAssetName, m_Transform);
            }

            if (assetObject != null)
            {
                assetObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                ScriptableProcessorUtility.AddAssetObjectToCheck(assetObject);
            }

            return assetObject;
        }

        private void OnAddReorderableList(int index)
        {
            if (IsCustomEnable)
            {
                SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(index);
                SerializedProperty scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty scriptableTypeName = scriptableInfo.FindPropertyRelative(m_ScriptableTypeNameDesc);
                SerializedProperty customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
                for (int i = 0; i < m_ScriptableTypeNames.Length; ++i)
                {
                    if (index > 0)
                        customScriptables.GetArrayElementAtIndex(i).objectReferenceValue = null;
                    else
                        customScriptables.InsertArrayElementAtIndex(i);
                }

                scriptableTypeName.stringValue = null;
                scriptableTypeIndex.intValue = 0;
            }
            else
            {
                SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(index);
                SerializedProperty scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                SerializedProperty scriptableTypeName = scriptableInfo.FindPropertyRelative(m_ScriptableTypeNameDesc);
                scriptableTypeName.stringValue = m_ScriptableTypeNames[0];
                scriptableTypeIndex.intValue = 0;
            }
        }

        private void OnRemoveReorderableList(int index)
        {
            if (IsCustomEnable)
            {
                SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(index);
                SerializedProperty customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
                for (int i = 0; i < customScriptables.arraySize; ++i)
                {
                    UnityEngine.Object scriptableObject = customScriptables.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (scriptableObject != null)
                    {
                        customScriptables.GetArrayElementAtIndex(i).objectReferenceValue = null;
                        UnityEngine.Object.DestroyImmediate(scriptableObject, true);
                    }
                }
            }
        }

        private void OnClearNoReference()
        {
            for (int i = 0; i < m_ScriptableInfosProperty.arraySize; ++i)
            {
                SerializedProperty scriptableInfo = m_ScriptableInfosProperty.GetArrayElementAtIndex(i);
                SerializedProperty customScriptables = scriptableInfo.FindPropertyRelative(m_CustomScriptablesDesc);
                SerializedProperty scriptableTypeIndex = scriptableInfo.FindPropertyRelative(m_ScriptableTypeIndexDesc);
                for (int j = 1; j < customScriptables.arraySize; ++j)
                {
                    if (scriptableTypeIndex.intValue != j)
                    {
                        UnityEngine.Object scriptableObject = customScriptables.GetArrayElementAtIndex(j).objectReferenceValue;
                        if (scriptableObject != null)
                        {
                            customScriptables.GetArrayElementAtIndex(i).objectReferenceValue = null;
                            UnityEngine.Object.DestroyImmediate(scriptableObject, true);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
