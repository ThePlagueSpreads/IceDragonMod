using HarmonyLib;
using IceDragon.MonoBehaviours;
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
        if (useObj.TryGetComponent(out Plantable plantable) && plantable.plantTechType == IceFruitTree.TechType)
        {
            Player.main.liveMixin.TakeDamage(1, useObj.transform.position, DamageType.Cold);
        }
        else if (useObj.TryGetComponent<FrozenBrineHeal>(out var heal))
        {
            Player.main.liveMixin.AddHealth(heal.health);
        }
    }
}
