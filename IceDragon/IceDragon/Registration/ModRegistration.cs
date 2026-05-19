using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using IceDragon.Registration.Prefabs;
using IceDragon.StructureLoading;
using Nautilus.Handlers;
using UnityEngine;

namespace IceDragon.Registration;

public static class ModRegistration
{
    private const string ASSET_BUNDLE_FILE_NAME = "icedragonassets";
    
    public static AssetBundle Assets { get; private set; }

    private static bool registered = false;
    
    internal static IEnumerator LoadModAsync(WaitScreenHandler.WaitScreenTask task)
    {
        if (registered) yield break;
        
        string path = GetAssetBundlePath(ASSET_BUNDLE_FILE_NAME);
        AssetBundleCreateRequest assetBundleTask = AssetBundle.LoadFromFileAsync(path);
        while (!assetBundleTask.isDone)
        {
            task.Status = $"Assets ({assetBundleTask.progress:P1})";
            yield return null;
        }

        Assets = assetBundleTask.assetBundle;
        
        new IceDragonPrefab().Register();
        IceFruit.Register();
        IceFruitTree.Register();
        IcebergBiome.Register();
        ArcticKelpFactory.RegisterArcticKelpVariants();
        
        StructureRegistrationUtils.RegisterStructures(StructureRegistrationUtils.GetStructuresFolderPath(Plugin.Assembly));
        
        ModAudio.RegisterAudio();
        registered = true;
    }
    
    private static string GetAssetBundlePath(string assetBundleFileName) => 
        Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location)!, "Assets", assetBundleFileName);
    
    public static IEnumerator LoadTexture(string assetName, Action<Texture2D> callback)
    {
        AssetBundleRequest request = Assets.LoadAssetAsync<Texture2D>(assetName);
        yield return request;
        if (request.asset == null) throw new Exception($"Failed to load texture: {assetName}");
        callback(request.asset as Texture2D);
    }
}
