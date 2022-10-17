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
        public static string GetAssetPath(Object assetObject)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(assetObject))
            {
                return AssetDatabase.GetAssetPath(assetObject);
            }     

            if (PrefabUtility.IsPartOfPrefabInstance(assetObject))
            {
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(assetObject);
                return AssetDatabase.GetAssetPath(prefabAsset);
            }

            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                return prefabStage.assetPath;
            }

            return AssetDatabase.GetAssetOrScenePath(assetObject);
        }


        [MenuItem("Assets/Delete[SP]", false, 19)]
        static private void DeleteSP()
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
        static private bool CheckSP()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string fileExt = Path.GetExtension(assetPath);
            return (Selection.activeObject is ScriptableObject) && fileExt.Equals(".prefab");
        }

        [MenuItem("Assets/Create/ScriptableObject", priority = 50)]
        static void CreateScriptableObject()
        {
            foreach (var item in Selection.objects)
                CreateAsset(item);
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

        static ScriptableObject CreateAsset(Object ms)
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
    }
}
