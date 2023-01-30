/*
ScriptableProcessor
Copyright © 2021-2022 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    public static class ScriptableProcessorUtility
    {
        #region SerializedPropertyManager
        private static SerializedProperty[] m_EditSerializedProps;

        public static int ArraySize
        {
            get
            {
                return m_EditSerializedProps[0].arraySize;
            }
        }

        public static void SetProps(params SerializedProperty[] serializedProps)
        {
            int arraySize = serializedProps[0].arraySize;
            for (int i = 1; i < serializedProps.Length; ++i)
            {
                if (serializedProps[i].arraySize != arraySize)
                {
                    Debug.LogError("EditSerializedProps length is different");
                    return;
                }
            }
            m_EditSerializedProps = serializedProps;
        }

        public static SerializedProperty[] GetArrayElementAtIndex(int index)
        {
            SerializedProperty[] propResutls = new SerializedProperty[m_EditSerializedProps.Length];
            for (int i = 0; i < m_EditSerializedProps.Length; ++i)
                propResutls[i] = m_EditSerializedProps[i].GetArrayElementAtIndex(index);
            return propResutls;
        }

        public static void InsertArrayElementAtIndex(int index, byte range = byte.MaxValue)
        {
            for (int i = 0; i < m_EditSerializedProps.Length; ++i)
            {
                SerializedProperty prop = m_EditSerializedProps[i];
                if (index > prop.arraySize)
                    Debug.LogError(string.Format("Insert {0} prop index out of range!", prop.name));
                if ((range >> i & 1) == 1)
                {
                    prop.InsertArrayElementAtIndex(index);
                }
            }
        }

        public static void DeleteArrayElementAtIndex(int index, byte range = byte.MaxValue)
        {
            for (int i = 0; i < m_EditSerializedProps.Length; ++i)
            {
                SerializedProperty prop = m_EditSerializedProps[i];
                if (index >= prop.arraySize)
                    Debug.LogError(string.Format("Delete {0} prop index out of range!", prop.name));
                if ((range >> i & 1) == 1)
                {
                    prop.DeleteArrayElementAtIndex(index);
                }
            }
        }
        #endregion

        #region ScriptableProcessorUtility
        public static string GetScriptableAssetPath(Object assetObject)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(assetObject))
            {
                return AssetDatabase.GetAssetPath(assetObject);
            }

            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                return prefabStage.assetPath;
            }

            return AssetDatabase.GetAssetOrScenePath(assetObject);
        }

        private static ScriptableObject CreateScriptableAsset(Object ms)
        {
            var path = AssetDatabase.GetAssetPath(ms);
            path = path.Substring(0, path.LastIndexOf("/"));
            var type = (ms as MonoScript).GetClass();
            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/ {type.Name}.asset");
            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }
        #endregion

        #region MenuItemEvent
        [MenuItem("Assets/Delete[SP]", false, 19)]
        static private void DeleteScriptableProcessor()
        {
            foreach (var activeObject in Selection.objects)
            {
                if (activeObject is ScriptableObject)
                {
                    UnityEngine.Object.DestroyImmediate(activeObject, true);
                }
                else
                {
                    Debug.LogWarning(string.Format("{0} is't activeObject, can't delete", activeObject.name));
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Delete[SP]", true)]
        static private bool CheckScriptableProcessor()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string fileExt = Path.GetExtension(assetPath);
            return (Selection.activeObject is ScriptableObject) && fileExt.Equals(".prefab");
        }

        [MenuItem("Assets/Create/ScriptableObject", priority = 50)]
        static void CreateScriptableObject()
        {
            foreach (var item in Selection.objects)
                CreateScriptableAsset(item);
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/ScriptableObject", true)]
        static bool IsScriptableObject()
        {
            System.Func<Object, bool> predicate = (obj) =>
            {
                if (obj is MonoScript)
                {
                    var type = (obj as MonoScript).GetClass();
                    return (!type.IsAbstract && type.IsSubclassOf(typeof(ScriptableObject)));
                }
                return false;
            };
            return Selection.objects.Length > 0 && Selection.objects.Any(predicate);
        }
        #endregion
    }
}
