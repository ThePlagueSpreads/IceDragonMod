using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using UnityEngine;

namespace IceDragon.Registration.Prefabs;


public static class IceFruit
{
    public static TechType TechType { get; private set; }
    
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
        Texture2D leafFruitMainTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeFruitLeafMain", (texture2d) => leafFruitMainTexture = texture2d);
        Texture2D leafFruitSpecTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeFruitLeafMainSpec", (texture2d) => leafFruitSpecTexture = texture2d);
        Texture2D leafFruitIllumTexture = null;
        yield return ModRegistration.LoadTexture("iceTreeFruitLeafMainIllum", (texture2d) => leafFruitIllumTexture = texture2d);
        
        Material fruitMaterial = gameObject.transform.Find("Fruit_03").GetComponent<MeshRenderer>().material;
        ModifyFruitMaterial(fruitMaterial, leafFruitMainTexture, leafFruitSpecTexture,  leafFruitIllumTexture);

        Eatable eatable = gameObject.GetComponent<Eatable>();
        eatable.waterValue = 15;
        eatable.foodValue = 3;
        eatable.kDecayRate = 0.001f;//10x slower than lantern fruit
        
        Plantable plantable = gameObject.GetComponent<Plantable>();
        plantable.plantTechType = IceFruitTree.TechType;
        TaskResult<GameObject> instResult = new TaskResult<GameObject>();
        yield return IceFruitTree.getGrowingModel(plantable.model, instResult);
        plantable.model = instResult.value;
    }

    internal static void ModifyFruitMaterial(Material mat, Texture2D main, Texture2D spec, Texture2D illum)
    {
        mat.SetTexture(ShaderPropertyID._MainTex, main);
        mat.SetTexture(ShaderPropertyID._SpecTex, spec);
        mat.SetTexture(ShaderPropertyID._Illum, illum);
        mat.SetFloat(ShaderPropertyID._GlowStrength, 2.5f);
        mat.SetFloat(ShaderPropertyID._GlowStrengthNight, 2.5f);
    }
}
