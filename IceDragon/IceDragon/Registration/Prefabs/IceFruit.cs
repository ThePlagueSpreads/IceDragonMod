using System.Collections;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using UnityEngine;

namespace IceDragon.Registration.Prefabs;


public static class IceFruit
{
    public static TechType TechType { get; private set; }
    private static GameObject growingModel { get; set; } = null;
    
    public static void Register()
    {
        PrefabInfo info = PrefabInfo.WithTechType("IceFruit")
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(ModRegistration.Assets.LoadAsset<Sprite>("IceFruitIcon"));
        CraftDataHandler.SetBackgroundType(info.TechType, CraftData.BackgroundType.PlantAir);
        TechType = info.TechType;
        
        CloneTemplate clone = new CloneTemplate(info, TechType.HangingFruit);
        clone.ModifyPrefabAsync = ModifyPrefabAsync;
        CustomPrefab prefab = new CustomPrefab(info);
        prefab.SetGameObject(clone);
        prefab.Register();
    }
    
    private static IEnumerator ModifyPrefabAsync(GameObject gameObject)
    {
        //Load Textures
        AssetBundleRequest assetRequest = ModRegistration.Assets.LoadAssetAsync<Texture2D>("iceTreeFruitLeafMain");
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
        
        Material fruitMaterial = gameObject.transform.Find("Fruit_03").GetComponent<MeshRenderer>().material;
        fruitMaterial.SetTexture(ShaderPropertyID._MainTex, leafFruitMainTexture);
        fruitMaterial.SetTexture(ShaderPropertyID._SpecTex, leafFruitSpecTexture);
        fruitMaterial.SetTexture(ShaderPropertyID._Illum, leafFruitIllumTexture);
        fruitMaterial.SetFloat(ShaderPropertyID._GlowStrength, 2.5f);
        fruitMaterial.SetFloat(ShaderPropertyID._GlowStrengthNight, 2.5f);

        Plantable plantable = gameObject.transform.GetComponent<Plantable>();
        plantable.plantTechType = IceFruitTree.TechType;
        TaskResult<GameObject> instResult = new TaskResult<GameObject>();
        yield return getGrowingModel(plantable.model, instResult);
        plantable.model = instResult.value;
    }

    private static IEnumerator getGrowingModel(GameObject original, IOut<GameObject> model)
    {
        if (growingModel != null) model.Set(growingModel);

        growingModel = UWE.Utils.InstantiateDeactivated(original);
        GrowingPlant growingPlant = growingModel.GetComponent<GrowingPlant>();
        growingPlant.plantTechType = IceFruitTree.TechType;
        growingPlant.grownModelPrefab = new CustomGameObjectReference("IceFruitTree");

        yield return IceFruitTree.ModifyPrefabAsync(growingModel.transform.Find("farming_plant_03").gameObject);
        
        model.Set(growingModel);
    }
}
