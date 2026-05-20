using HarmonyLib;
using IceDragon.Registration.Prefabs;
using UnityEngine;
namespace IceDragon.Patches;

[HarmonyPatch(typeof(Survival))]
public class SurvivalPatches
{
    [HarmonyPatch(nameof(Survival.Eat))]
    [HarmonyPostfix]
    private static void Eat_PostFix(GameObject useObj)
    {
        if (useObj == null) return;
        if (!useObj.TryGetComponent(out Plantable plantable)) return;
        if (plantable.plantTechType != IceFruitTree.TechType) return;
        
        Player.main.liveMixin.TakeDamage(1, useObj.transform.position, DamageType.Cold);
    }
}
