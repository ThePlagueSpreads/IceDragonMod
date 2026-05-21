using IceDragon.MonoBehaviours;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace IceDragon.Registration.Prefabs;

public static class FrozenBrineFragment
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("FrozenBrineFragment")
        .WithIcon(ModRegistration.Assets.LoadAsset<Sprite>("FrozenBrineFragmentIcon"));
    
    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
        CraftDataHandler.SetEatingSound(Info.TechType, "event:/player/eat");
    }

    private static GameObject GetPrefab()
    {
        var prefab = Object.Instantiate(ModRegistration.Assets.LoadAsset<GameObject>("FrozenBrineFragment"));
        prefab.SetActive(false);
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(prefab, 5.4f, 50, 1.1f,
            new FresnelModifier(0.65f),
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent));
        prefab.AddComponent<Pickupable>();
        PrefabUtils.AddWorldForces(prefab, 10);
        var eatable = prefab.AddComponent<Eatable>();
        eatable.decomposes = false;
        eatable.foodValue = 6;
        eatable.waterValue = -4;
        prefab.AddComponent<FrozenBrineHeal>();
        return prefab;
    }
}