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
        static ScriptableProcessorUtility()
        {
            EditorApplication.update += OnCheckScriptableProcessor;
        }

        static void OnCheckScriptableProcessor()
        {
            //Debug.Log("Updating");
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
