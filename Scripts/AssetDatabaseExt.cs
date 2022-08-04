/*
ScriptableProcessor
Copyright © 2021-2022 Ding Qi Ming. All rights reserved.
Blog: https://daveant.gitee.io/
*/
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScriptableProcessor
{
    public sealed class AssetDatabaseExt
    {
        private static string localIdSign = "--- !u!114 &";

        private static long GenUniquelocalIdentifier(long localIdentifier, long[] localIdentifiers = null)
        {
            foreach (long ident in localIdentifiers)
            {
                if (localIdentifier > ident)
                    continue;
                if (localIdentifier == ident)
                    localIdentifier += 1;
                if (localIdentifier < ident)
                    break;
            }
            return localIdentifier;
        }

        private static long[] GetLocalFileIdentifiers(string assetPath)
        {
            int index = 0;
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            long[] localIdentifiers = new long[objects.Length];
            foreach (var obj in objects)
            {
                string guid;
                long localid;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out localid);
                localIdentifiers[index] = localid;
                index++;
            }
            System.Array.Sort(localIdentifiers);
            return localIdentifiers;
        }

        public static ScriptableObject LoadAssetNameAtPath(string assetPath,string scriptableName)
        {
            var objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            foreach(var obj in objects)
            {
                if (obj.name == scriptableName)
                {
                    return (obj as ScriptableObject);
                }
            }
            return null;
        }

        public static void AddScriptObjectToAsset(Object scriptableObject, string assetPath)
        {
            AssetDatabase.AddObjectToAsset(scriptableObject, assetPath);
            scriptableObject.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        [MenuItem("Assets/[SP]Delete", false, 10)]
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

        [MenuItem("Assets/[SP]Delete", true)]
        static private bool CheckSP()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string fileExt = Path.GetExtension(assetPath);
            return (Selection.activeObject is ScriptableObject) && fileExt.Equals(".prefab");
        }
    }
}
