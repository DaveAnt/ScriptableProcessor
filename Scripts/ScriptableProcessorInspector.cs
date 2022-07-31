/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableProcessor
{
    public enum ScriptableProcessorStatus
    {
        Null,
        NoPrefab,
        ShowScriptable,
    }

    internal sealed class ScriptableProcessorData : IEnumerable<KeyValuePair<string, UnityEngine.Object>>
    {
        private int m_ScriptableTypeNameIndex = 0;
        private Dictionary<string,UnityEngine.Object> m_ScriptableTypeObjectDicts = new Dictionary<string,UnityEngine.Object>();
        private Dictionary<UnityEngine.Object, SerializedObject> m_ScriptableTypeSerializedObjectDicts = new Dictionary<UnityEngine.Object, SerializedObject>();

        public int ScriptableTypeNameIndex
        {
            get
            {
                return m_ScriptableTypeNameIndex;
            }
            set
            {
                m_ScriptableTypeNameIndex = value;
            }
        }


        public UnityEngine.Object this[string scriptableTypeName]
        {
            get
            {
                UnityEngine.Object scriptableTypeObject = null;
                m_ScriptableTypeObjectDicts.TryGetValue(scriptableTypeName,out scriptableTypeObject);
                return scriptableTypeObject;
            }
            set
            {
                UnityEngine.Object scriptableTypeObject = null;
                if (!m_ScriptableTypeObjectDicts.TryGetValue(scriptableTypeName, out scriptableTypeObject))
                {
                    m_ScriptableTypeObjectDicts.Add(scriptableTypeName, value);
                }
                else if (scriptableTypeObject == null)
                {
                    m_ScriptableTypeObjectDicts[scriptableTypeName] = value;
                }
            }
        }

        public SerializedObject this[UnityEngine.Object scriptableTypeObject]
        {
            get
            {
                SerializedObject scriptableTypeSerializedObject = null;
                if (!m_ScriptableTypeSerializedObjectDicts.TryGetValue(scriptableTypeObject, out scriptableTypeSerializedObject))
                {
                    scriptableTypeSerializedObject = new SerializedObject(scriptableTypeObject);
                    m_ScriptableTypeSerializedObjectDicts.Add(scriptableTypeObject, scriptableTypeSerializedObject);
                }
                else if (scriptableTypeSerializedObject == null)
                {
                    scriptableTypeSerializedObject = new SerializedObject(scriptableTypeObject);
                    m_ScriptableTypeSerializedObjectDicts[scriptableTypeObject] = scriptableTypeSerializedObject;
                }

                return scriptableTypeSerializedObject;
            }
        }

        public void ConvertScriptableTypeIndexs(int index)
        {
            Dictionary<string, UnityEngine.Object> tmpScriptableTypeObjectDicts = m_ScriptableTypeObjectDicts;
            m_ScriptableTypeObjectDicts = new Dictionary<string, UnityEngine.Object>();

            foreach (var val in tmpScriptableTypeObjectDicts)
            {
                int indexSign = val.Key.LastIndexOf('-');
                val.Value.name = val.Key.Substring(0, indexSign + 1) + index.ToString();
                m_ScriptableTypeObjectDicts.Add(val.Value.name, val.Value);
            }
        }

        IEnumerator<KeyValuePair<string, UnityEngine.Object>> IEnumerable<KeyValuePair<string, UnityEngine.Object>>.GetEnumerator()
        {
            return m_ScriptableTypeObjectDicts.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return m_ScriptableTypeObjectDicts.GetEnumerator();
        }
    }

    internal sealed class ScriptableProcessorInspector : SerializedInspectorBase
    {
        private readonly int[] m_ScriptableElementHeight = { 25, 45, 70 };

        private readonly string m_Name;
        private readonly string m_HeaderName;
        private readonly string m_CustomOptionName = "<Custom>";
        private readonly string m_TargetObjectPath = "m_TargetObject";
        private readonly string m_ScriptableAttrsPath = "m_ScriptableAttrs";
        private readonly string m_OptionScriptablePath = "m_OptionScriptable";
        private readonly string m_CustomScriptablePath = "m_CustomScriptable";
        private readonly string m_ScriptableTypeNamePath = "m_ScriptableTypeName";


        private UnityEngine.Object m_Target;
        private UnityEngine.Transform m_TargetTransform;
        private ISerializedInspector m_SerializedInspector;
        private SerializedObject m_SelectedSerializedObject;

        private ReorderableList m_ScriptableAttrsReorderableList;
        private SerializedProperty m_ScriptableAttrsProperty;
        private SerializedProperty m_TargetObjectProperty;

        private Dictionary<string, ISerializedInspector> m_SerializedInspectorDicts;
        private List<ScriptableProcessorData> m_ScriptableProcessorDatas;

        private Type m_ScriptableType;
        private string m_TargetPrefabPath;
        private string[] m_ScriptableTypeNames;
        private float m_PropertyHeight = 0;
        private int m_OnSelectedIndex = -1;

        private bool m_ShowFoldoutHeader = true;
        private bool m_IsScriptableObject;
        private bool m_IsMonoBehaviour;

        private bool IsCustomEnable
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
                if (m_ScriptableAttrsReorderableList.serializedProperty.arraySize < 1)
                    return -1;
                return m_ScriptableAttrsReorderableList.index;
            }
        }

        public ScriptableProcessorData SelectedScriptableProcessorData
        {
            get
            {
                int selectedIndex = SelectedIndex;
                if (selectedIndex < 0)
                    return null;
                return m_ScriptableProcessorDatas[selectedIndex];
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
            m_SerializedInspectorDicts = new Dictionary<string, ISerializedInspector>();
            m_ScriptableProcessorDatas = new List<ScriptableProcessorData>();
        }

        public override void Init(SerializedObject serializedObject)
        {
            serializedObject.Update();
            m_Target = serializedObject.targetObject;
            m_TargetTransform = (m_Target as Component).transform;
            m_TargetPrefabPath = m_Target == null ? "" : PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Target);

            SerializedProperty targetProperty = serializedObject.FindProperty(m_Name);
            m_ScriptableAttrsProperty = targetProperty.FindPropertyRelative(m_ScriptableAttrsPath);
            m_TargetObjectProperty = targetProperty.FindPropertyRelative(m_TargetObjectPath);
            m_ScriptableAttrsReorderableList = new ReorderableList(serializedObject, m_ScriptableAttrsProperty, true, false, true, true);
            m_TargetObjectProperty.objectReferenceValue = m_TargetTransform;

            m_ScriptableAttrsReorderableList.drawElementCallback = (Rect position, int index, bool selected, bool focused) =>
            {
                if (index > m_ScriptableProcessorDatas.Count - 1) return;
                SerializedProperty scriptableAttrProperty = m_ScriptableAttrsReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                using (new EditorGUI.PropertyScope(position, null, scriptableAttrProperty))
                {
                    string displayName = FieldNameForDisplay(m_Name);
                    position.height = EditorGUIUtility.singleLineHeight;
                    var scriptableTypeName = scriptableAttrProperty.FindPropertyRelative(m_ScriptableTypeNamePath);
                    var optionScriptable = scriptableAttrProperty.FindPropertyRelative(m_OptionScriptablePath);
                    var customScriptable = scriptableAttrProperty.FindPropertyRelative(m_CustomScriptablePath);

                    var popupPosition = new Rect(position) { y = position.y + 5 };
                    var customPosition = new Rect(position) { y = popupPosition.y + EditorGUIUtility.singleLineHeight + 5 };
                    var errorPosition = new Rect(position) { y = customPosition.y + EditorGUIUtility.singleLineHeight + 5 };
                    var scriptableTypeNameIndex = m_ScriptableProcessorDatas[index].ScriptableTypeNameIndex;

                    int selectedIndex = EditorGUI.Popup(popupPosition, string.Format("{0}-{1}", displayName, index), scriptableTypeNameIndex, m_ScriptableTypeNames);
                    if (selectedIndex != scriptableTypeNameIndex)
                    {
                        scriptableTypeName.stringValue = selectedIndex <= 0 ? null : m_ScriptableTypeNames[selectedIndex];
                        m_ScriptableProcessorDatas[index].ScriptableTypeNameIndex = selectedIndex;
                    }

                    if (scriptableTypeNameIndex <= 0 && IsCustomEnable)
                    {
                        EditorGUI.PropertyField(customPosition, customScriptable);
                        if (customScriptable.objectReferenceValue == null)
                        {
                            EditorGUI.HelpBox(errorPosition, string.Format("You must set Custom {0} Helper.", displayName), MessageType.Error);
                        }
                    }
                }
            };

            m_ScriptableAttrsReorderableList.elementHeightCallback = (int index) =>
            {
                if (index > m_ScriptableProcessorDatas.Count - 1)
                    return 0;
                SerializedProperty scriptableAttrProperty = m_ScriptableAttrsReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var customScriptable = scriptableAttrProperty.FindPropertyRelative(m_CustomScriptablePath);
                if (m_ScriptableProcessorDatas[index].ScriptableTypeNameIndex > 0)
                    return m_ScriptableElementHeight[0];
                if (customScriptable.objectReferenceValue != null)
                    return m_ScriptableElementHeight[1];
                return m_ScriptableElementHeight[2];
            };

            m_ScriptableAttrsReorderableList.onAddCallback = (ReorderableList list) =>
            {
                OnAddReorderableList(m_ScriptableAttrsReorderableList.serializedProperty.arraySize);
                ReorderableList.defaultBehaviours.DoAddButton(list);
            };

            m_ScriptableAttrsReorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                OnRemoveReorderableList(SelectedIndex);
                ReorderableList.defaultBehaviours.DoRemoveButton(list); 
            };

            m_ScriptableAttrsReorderableList.onSelectCallback = (ReorderableList list) =>
            {
                m_OnSelectedIndex = SelectedIndex;
            };

            m_ScriptableAttrsReorderableList.onChangedCallback = (ReorderableList list) =>
            {
                OnSwapReorderableList(m_OnSelectedIndex, SelectedIndex);
            };

            serializedObject.ApplyModifiedProperties();
        }

        public override void Draw()
        {
            m_ShowFoldoutHeader = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowFoldoutHeader, m_HeaderName);

            if (m_ShowFoldoutHeader)
            {
                m_ScriptableAttrsReorderableList.DoLayoutList();
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (IsCustomEnable && SelectedIndex != -1)
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

            if(!m_ShowFoldoutHeader)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        public override void Draw(Rect position)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            Rect lstPos = new Rect(position) { y = position.y + position.height + EditorGUIUtility.standardVerticalSpacing, height = m_ScriptableAttrsReorderableList.GetHeight() };
            m_PropertyHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            m_ShowFoldoutHeader = EditorGUI.BeginFoldoutHeaderGroup(position, m_ShowFoldoutHeader, m_HeaderName);
            if (m_ShowFoldoutHeader) 
            {
                m_ScriptableAttrsReorderableList.DoList(lstPos);
                m_PropertyHeight += (lstPos.height + EditorGUIUtility.standardVerticalSpacing);

                EditorGUI.EndFoldoutHeaderGroup();

                if (IsCustomEnable && SelectedIndex != -1)
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

            if (!m_ShowFoldoutHeader)
            {
                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

        public override void Refresh()
        {
            m_ScriptableProcessorDatas.Clear();
            List<string> scriptableTypeNameList = new List<string> { m_CustomOptionName };
            scriptableTypeNameList.AddRange(TypeExt.GetTypeFullNames(m_ScriptableType));
            m_ScriptableTypeNames = scriptableTypeNameList.ToArray();

            for (int i = 0; i < m_ScriptableAttrsReorderableList.count; ++i)
            {
                ScriptableProcessorData scriptableProcessorData = new ScriptableProcessorData();
                m_ScriptableProcessorDatas.Add(scriptableProcessorData);
                if (IsCustomEnable)
                {
                    for (int j = 1; j < m_ScriptableTypeNames.Length; ++j)
                    {
                        UnityEngine.Object assetObject = null;
                        string scriptableAssetName = TypeCreator.GetScriptableAssetName(m_Target.name, m_ScriptableTypeNames[j], i);
                        if (m_IsScriptableObject)
                            assetObject = AssetDatabaseExt.LoadAssetNameAtPath(m_TargetPrefabPath, scriptableAssetName);
                        else if (m_IsMonoBehaviour)
                            assetObject = m_TargetTransform.Find(scriptableAssetName)?.GetComponent(m_ScriptableTypeNames[j]);

                        if (assetObject != null)
                            scriptableProcessorData[scriptableAssetName] = assetObject;
                    }
                }
            }

            for (int i = 0; i < m_ScriptableAttrsProperty.arraySize; ++i)
            {
                SerializedProperty scriptableAttr = m_ScriptableAttrsProperty.GetArrayElementAtIndex(i);
                SerializedProperty scriptableTypeName = scriptableAttr.FindPropertyRelative(m_ScriptableTypeNamePath);

                if (!string.IsNullOrEmpty(scriptableTypeName.stringValue))
                {
                    m_ScriptableProcessorDatas[i].ScriptableTypeNameIndex = scriptableTypeNameList.IndexOf(scriptableTypeName.stringValue);
                    if (m_ScriptableProcessorDatas[i].ScriptableTypeNameIndex <= 0)
                    {
                        m_ScriptableProcessorDatas[i].ScriptableTypeNameIndex = 0;
                        scriptableTypeName.stringValue = null;
                    }
                }
            }

            m_SerializedInspector?.Refresh();
        }

        private ScriptableProcessorStatus OnSerializeScriptable()
        {
            Type scriptableType = null;
            m_SelectedSerializedObject = null;

            SerializedProperty scriptableAttrProperty = m_ScriptableAttrsReorderableList.serializedProperty.GetArrayElementAtIndex(SelectedIndex);
            var scriptableTypeName = scriptableAttrProperty.FindPropertyRelative(m_ScriptableTypeNamePath);
            var optionScriptable = scriptableAttrProperty.FindPropertyRelative(m_OptionScriptablePath);
            var customScriptable = scriptableAttrProperty.FindPropertyRelative(m_CustomScriptablePath);
            var scriptableAssetName = TypeCreator.GetScriptableAssetName(m_Target.name, scriptableTypeName.stringValue, SelectedIndex);
            var scriptableProcessorData = SelectedScriptableProcessorData;

            if (scriptableProcessorData.ScriptableTypeNameIndex > 0)
            {
                if (string.IsNullOrEmpty(m_TargetPrefabPath))
                    return ScriptableProcessorStatus.NoPrefab;

                scriptableType = AssemblyExt.GetType(scriptableTypeName.stringValue);
                if (scriptableProcessorData[scriptableAssetName] == null)
                {
                    UnityEngine.Object scriptableObject = CreateAssetObject(scriptableTypeName.stringValue, scriptableAssetName);
                    scriptableProcessorData[scriptableAssetName] = scriptableObject;
                }

                optionScriptable.objectReferenceValue = scriptableProcessorData[scriptableAssetName];
                m_SelectedSerializedObject = scriptableProcessorData[optionScriptable.objectReferenceValue];
            }
            else if (customScriptable.objectReferenceValue != null)
            {
                scriptableType = customScriptable.objectReferenceValue.GetType();
                m_SelectedSerializedObject = scriptableProcessorData[customScriptable.objectReferenceValue];
            }

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
                AssetDatabaseExt.AddScriptObjectToAsset(assetObject, m_TargetPrefabPath);
                SelectedScriptableProcessorData[scriptableAssetName] = assetObject;
            }
            else if (m_IsMonoBehaviour)
            {
                assetObject = TypeCreator.Create<UnityEngine.Object>(scriptableTypeName, scriptableAssetName, m_TargetTransform);
                SelectedScriptableProcessorData[scriptableAssetName] = assetObject;
            }

            return assetObject;
        }

        private void OnAddReorderableList(int index)
        {  
            if (index < m_ScriptableProcessorDatas.Count)
            {
                for (int i = index; i < m_ScriptableProcessorDatas.Count; ++i)
                    m_ScriptableProcessorDatas[i].ConvertScriptableTypeIndexs(i+1);
            }

            m_ScriptableProcessorDatas.Insert(index, new ScriptableProcessorData());
            AssetDatabase.SaveAssets();
        }

        private void OnRemoveReorderableList(int index)
        {
            foreach (KeyValuePair<string,UnityEngine.Object> data in m_ScriptableProcessorDatas[index])
            {
                if (m_IsScriptableObject)
                    AssetDatabaseExt.DelScriptObjectToAsset(data.Value as ScriptableObject);
                else if(m_IsMonoBehaviour)
                    GameObject.DestroyImmediate((data.Value as Component).gameObject);
            }

            if (index < m_ScriptableProcessorDatas.Count - 1)
            {
                for (int i = index + 1; i < m_ScriptableProcessorDatas.Count; ++i)
                {
                    m_ScriptableProcessorDatas[i].ConvertScriptableTypeIndexs(i - 1);
                }
            }

            m_ScriptableProcessorDatas.RemoveAt(index);
            AssetDatabase.SaveAssets();
        }

        private void OnSwapReorderableList(int fromIndex, int toIndex)
        {
            ScriptableProcessorData tmpScriptableProcessorData = m_ScriptableProcessorDatas[fromIndex];
            if (fromIndex > toIndex)
            {
                for (int i = fromIndex; i > toIndex; --i)
                    m_ScriptableProcessorDatas[i] = m_ScriptableProcessorDatas[i - 1];
                m_ScriptableProcessorDatas[toIndex] = tmpScriptableProcessorData;

                for (int i = toIndex; i <= fromIndex; ++i)
                {
                    m_ScriptableProcessorDatas[i].ConvertScriptableTypeIndexs(i);
                }
            }
            else
            {
                for(int i = fromIndex;i < toIndex; ++i)
                {
                    m_ScriptableProcessorDatas[i] = m_ScriptableProcessorDatas[i + 1];
                }
                m_ScriptableProcessorDatas[toIndex] = tmpScriptableProcessorData;

                for (int i = fromIndex; i <= toIndex; ++i)
                {
                    m_ScriptableProcessorDatas[i].ConvertScriptableTypeIndexs(i);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
