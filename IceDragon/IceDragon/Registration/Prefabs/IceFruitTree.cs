using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using UnityEngine;
using UWE;
namespace IceDragon.Registration.Prefabs;


public static class IceFruitTree
{
    public static TechType TechType { get; private set; }
    
    public static void Register()
    {
        PrefabInfo info = PrefabInfo.WithTechType("IceFruitTree")
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(SpriteManager.Get(TechType.HangingFruitTree));
        
        TechType = info.TechType;
        
        CloneTemplate clone = new CloneTemplate(info, TechType.HangingFruitTree);
        clone.ModifyPrefabAsync = ModifyPrefabAsync;
        CustomPrefab prefab = new CustomPrefab(info);
        prefab.SetGameObject(clone);
        prefab.Register();
    }

    public static IEnumerator ModifyPrefabAsync(GameObject gameObject)
    {
        //Load Trunk Textures
        AssetBundleRequest assetRequest = ModRegistration.Assets.LoadAssetAsync<Texture2D>("iceTreeMainTexture");
        yield return assetRequest;
        if (assetRequest == null) { Plugin.Logger.LogError("Failed to find iceTreeMainTexture from asset bundle"); yield break; }
        Texture2D mainTexture = assetRequest.asset as Texture2D;
        //Load Leaf Textures
        assetRequest = ModRegistration.Assets.LoadAssetAsync<Texture2D>("iceTreeFruitLeafMain");
        yield return assetRequest;
        if (assetRequest == null) { Plugin.Logger.LogError("Failed to find iceTreeFruitLeafMain from asset bundle"); yield break; }
        Texture2D leafFruitMainTexture = assetRequest.asset as Texture2D;
        assetRequest = ModRegistration.Assets.LoadAssetAsync<Texture2D>("iceTreeFruitLeafMainSpec");
        yield return assetRequest;
        if (assetRequest == null) { Plugin.Logger.LogError("Failed to find iceTreeFruitLeafMainSpec from asset bundle"); yield break; }
        Texture2D leafFruitSpecTexture = assetRequest.asset as Texture2D;
        assetRequest = ModRegistration.Assets.LoadAssetAsync<Texture2D>("iceTreeFruitLeafMainIllum");
        yield return assetRequest;
        if (assetRequest == null) { Plugin.Logger.LogError("Failed to find iceTreeFruitLeafMainIllum from asset bundle"); yield break; }
        Texture2D leafFruitIllumTexture = assetRequest.asset as Texture2D;
        
        Transform modelsRoot = gameObject.transform.Find("farming_plant_03");
        foreach (Transform child in modelsRoot)
        {
            if (child.name == "farming_plant_03") continue;
            
            Material fruitMaterial = child.GetComponent<MeshRenderer>().material;
            fruitMaterial.SetTexture(ShaderPropertyID._MainTex, leafFruitMainTexture);
            fruitMaterial.SetTexture(ShaderPropertyID._SpecTex, leafFruitSpecTexture);
            fruitMaterial.SetTexture(ShaderPropertyID._Illum, leafFruitIllumTexture);
            fruitMaterial.SetFloat(ShaderPropertyID._GlowStrength, 2.5f);
            fruitMaterial.SetFloat(ShaderPropertyID._GlowStrengthNight, 2.5f);
            child.GetComponent<PickPrefab>().pickTech = IceFruit.TechType;
        }
        
        GameObject treeModel = modelsRoot.Find("farming_plant_03").gameObject;
        Material trunkMaterial = treeModel.GetComponent<MeshRenderer>().material;
        trunkMaterial.SetTexture(ShaderPropertyID._MainTex, mainTexture);
        trunkMaterial.SetTexture(ShaderPropertyID._SpecTex, mainTexture);
        
        Material LeafMaterial = treeModel.GetComponent<MeshRenderer>().materials[1];
        LeafMaterial.SetTexture(ShaderPropertyID._MainTex, leafFruitMainTexture);
        LeafMaterial.SetTexture(ShaderPropertyID._SpecTex, leafFruitSpecTexture);
        LeafMaterial.SetTexture(ShaderPropertyID._Illum, leafFruitIllumTexture);
        LeafMaterial.DisableKeyword("MARMO_EMISSION");//looks better without it
    }
}
