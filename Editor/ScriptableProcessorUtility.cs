/*
ScriptableProcessor
Copyright © 2021-2023 DaveAnt. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ScriptableProcessor.Editor
{
    [InitializeOnLoad]
    public static class ScriptableProcessorUtility
    {
        static ScriptableProcessorUtility()
        {
            CompilationPipeline.compilationStarted += ScriptableProcessorManager.Dispose;
        }

        #region ScriptableProcessorUtility
        public static string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }

        public static float DoRectDrawDefaultInspector(Rect beginPos, SerializedObject obj)
        {
            obj.UpdateIfRequiredOrScript();
            SerializedProperty property = obj.GetIterator();
            float height = DoRectDrawDefaultInspector(beginPos, property);
            obj.ApplyModifiedProperties();
            return height;
        }

        public static float DoRectDrawDefaultInspector(Rect beginPos, SerializedProperty property)
        {
            float height = 0;
            bool expanded = true;

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
            return height;
        }

        public static void DoLayoutDrawDefaultInspector(SerializedObject obj)
        {
            obj.UpdateIfRequiredOrScript();
            SerializedProperty property = obj.GetIterator();
            DoLayoutDrawDefaultInspector(property);
            obj.ApplyModifiedProperties();
        }

        public static void DoLayoutDrawDefaultInspector(SerializedProperty property)
        {
            bool expanded = true;

            while (property.NextVisible(expanded))
            {
                using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                expanded = false;
            }
        }

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
