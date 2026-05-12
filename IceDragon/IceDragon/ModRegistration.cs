using System.Collections;
using System.IO;
using Nautilus.Handlers;
using UnityEngine;

namespace IceDragon;


public static class ModRegistration
{
    private const string ASSET_BUNDLE_FILE_NAME = "icedragonassets";
    
    public static AssetBundle Assets { get; private set; }
    
    internal static IEnumerator LoadAssetBundlesAsync(WaitScreenHandler.WaitScreenTask task)
    {
        string path = GetAssetBundlePath(ASSET_BUNDLE_FILE_NAME);
        AssetBundleCreateRequest assetBundleTask = AssetBundle.LoadFromFileAsync(path);
        while (!assetBundleTask.isDone)
        {
            task.Status = $"Assets ({assetBundleTask.progress:P1})";
            yield return null;
        }
    }
    
    private static string GetAssetBundlePath(string assetBundleFileName) => 
        Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location)!, "Assets", assetBundleFileName);
}
