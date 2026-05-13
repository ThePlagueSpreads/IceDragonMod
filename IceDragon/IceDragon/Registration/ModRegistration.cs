using System.Collections;
using System.IO;
using IceDragon.Registration.Prefabs;
using Nautilus.Handlers;
using UnityEngine;

namespace IceDragon.Registration;

public static class ModRegistration
{
    private const string ASSET_BUNDLE_FILE_NAME = "icedragonassets";
    
    public static AssetBundle Assets { get; private set; }
    
    internal static IEnumerator LoadModAsync(WaitScreenHandler.WaitScreenTask task)
    {
        string path = GetAssetBundlePath(ASSET_BUNDLE_FILE_NAME);
        AssetBundleCreateRequest assetBundleTask = AssetBundle.LoadFromFileAsync(path);
        while (!assetBundleTask.isDone)
        {
            task.Status = $"Assets ({assetBundleTask.progress:P1})";
            yield return null;
        }

        Assets = assetBundleTask.assetBundle;
        
        new IceDragonPrefab().Register();
        IcebergBiome.Register();
        
        ModAudio.RegisterAudio();
    }
    
    private static string GetAssetBundlePath(string assetBundleFileName) => 
        Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location)!, "Assets", assetBundleFileName);
}
