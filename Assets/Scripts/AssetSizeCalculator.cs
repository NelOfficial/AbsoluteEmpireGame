//using UnityEditor;
//using UnityEngine;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;

//public class AssetSizeCalculator : MonoBehaviour
//{
//    [MenuItem("Tools/Calculate Asset Sizes")]
//    private static void CalculateAssetSizes()
//    {
//        //string[] assetGuids = AssetDatabase.FindAssets("t:Texture2D");
//        string[] assetGuids = AssetDatabase.FindAssets("t:sprite");

//        string result = "";

//        double totalSize = 0;

//        List<AssetDataSize> list = new List<AssetDataSize>();

//        foreach (string guid in assetGuids)
//        {
//            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
//            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

//            if (texture != null)
//            {
//                string path = AssetDatabase.GetAssetPath(texture);
//                FileInfo fileInfo = new FileInfo(path);
//                double sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
//                double sizeInKB = fileInfo.Length / (1024.0);
//                totalSize += sizeInMB;

//                AssetDataSize asset = new AssetDataSize(assetPath, sizeInKB);

//                list.Add(asset);
//            }
//        }

//        list = list.OrderByDescending(item => item.sizeInKB).ToList();

//        foreach (var item in list)
//        {
//            Debug.Log($"{item.path} - {item.sizeInKB:F2} KB");
//        }

//        Debug.Log($"Total size of all textures: {totalSize:F2} MB");
//    }

//    [System.Serializable]
//    public class AssetDataSize
//    {
//        public string path;
//        public double sizeInKB;

//        public AssetDataSize(string path, double sizeInKB)
//        {
//            this.path = path;
//            this.sizeInKB = sizeInKB;
//        }
//    }
//}
