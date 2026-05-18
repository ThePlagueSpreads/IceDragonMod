using System.Collections;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using UnityEngine;
using UWE;
namespace IceDragon.Registration.Prefabs;


public static class IceFruitTree
{
    public static TechType TechType { get; private set; }
    private static GameObject growingModel { get; set; } = null;
    
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
        
        PDAScanner.EntryData entryData = new PDAScanner.EntryData
        {
            key = TechType,
            encyclopedia = "IceFruitTree",
            scanTime = 4,
        };
        PDAHandler.AddCustomScannerEntry(entryData);

        PDAHandler.AddEncyclopediaEntry(
            key: "IceFruitTree",
            path: "Lifeforms/Flora/Exploitable",
            title: null,
            desc: null
        );
    }

    private static IEnumerator ModifyPrefabAsync(GameObject gameObject)
    {
        //Load Trunk Textures
        Texture2D mainTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeMainTexture", (texture2d) => mainTexture = texture2d);
        //Load Leaf Textures
        Texture2D leafFruitMainTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeFruitLeafMain", (texture2d) => leafFruitMainTexture = texture2d);
        Texture2D leafFruitSpecTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeFruitLeafMainSpec", (texture2d) => leafFruitSpecTexture = texture2d);
        Texture2D leafFruitIllumTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeFruitLeafMainIllum", (texture2d) => leafFruitIllumTexture = texture2d);
        
        Transform modelsRoot = gameObject.transform.Find("farming_plant_03");
        foreach (Transform child in modelsRoot)
        {
            if (child.name == "farming_plant_03") continue;
            
            Material fruitMaterial = child.GetComponent<MeshRenderer>().material;
            IceFruit.ModifyFruitMaterial(fruitMaterial, leafFruitMainTexture, leafFruitSpecTexture, leafFruitIllumTexture);
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
    
    public static IEnumerator getGrowingModel(GameObject original, IOut<GameObject> model)
    {
        if (growingModel != null) model.Set(growingModel);

        growingModel = UWE.Utils.InstantiateDeactivated(original);
        GrowingPlant growingPlant = growingModel.GetComponent<GrowingPlant>();
        growingPlant.plantTechType = TechType;
        growingPlant.grownModelPrefab = new CustomGameObjectReference("IceFruitTree");//from ecc library

        yield return ModifyPrefabAsync(growingModel.transform.Find("farming_plant_03").gameObject);
        
        model.Set(growingModel);
    }
}
