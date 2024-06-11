//using UnityEditor;
//using UnityEngine;
//using System.IO;

//public class AssetSizeCalculator : MonoBehaviour
//{
//    [MenuItem("Tools/Calculate Asset Sizes")]
//    private static void CalculateAssetSizes()
//    {
//        string[] assetGuids = AssetDatabase.FindAssets("t:Texture2D");

//        string result = "";

//        double totalSize = 0;

//        foreach (string guid in assetGuids)
//        {
//            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
//            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

//            if (texture != null)
//            {
//                string path = AssetDatabase.GetAssetPath(texture);
//                FileInfo fileInfo = new FileInfo(path);
//                double sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
//                totalSize += sizeInMB;

//                Debug.Log($"{assetPath} - {sizeInMB:F2} MB");
//            }
//        }

//        Debug.Log($"Total size of all textures: {totalSize:F2} MB");
//    }
//}
