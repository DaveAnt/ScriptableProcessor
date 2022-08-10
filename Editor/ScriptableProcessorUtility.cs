/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor.Editor
{

    [InitializeOnLoad]
    public static class ScriptableProcessorUtility
    {
        private static List<Object> m_OnCheckAssetObjectLst = new List<Object>();
        static ScriptableProcessorUtility()
        {
            EditorApplication.update += OnCheckScriptableProcessor;
        }

        public static void AddAssetObjectToCheck(UnityEngine.Object assetObject)
        {
            if (assetObject != null)
            {
                m_OnCheckAssetObjectLst.Add(assetObject);
            }
        }

        private static void OnCheckScriptableProcessor()
        {
            foreach(UnityEngine.Object assetObj in m_OnCheckAssetObjectLst)
            {
                if (assetObj != null)
                {
                    //Debug.Log(assetObj.hideFlags); 
                    //Debug.Log(PrefabUtility.IsAddedComponentOverride(assetObj as Component));
                    //MonoScript monoScript = MonoScript.FromMonoBehaviour(assetObj as MonoBehaviour);
                    //monoScript.name = "ssssss";
                    //Debug.Log(monoScript.name);
                }
                else
                {
                    m_OnCheckAssetObjectLst.Remove(assetObj);
                    Debug.Log("---------------------"+ m_OnCheckAssetObjectLst.Count);
                    break;
                }
            }
        }

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
    }
}
