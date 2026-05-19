using HarmonyLib;
using IceDragon.Registration.Prefabs;
using UnityEngine;

namespace IceDragon.Patches;

[HarmonyPatch(typeof(GrowingPlant))]
internal class GrowingPlantPatches
{
    private const float failedGrowthPlantPercent = 0.25f;
    private const int maxGrowthTemperature = 10;
    
    [HarmonyPatch(nameof(GrowingPlant.GetProgress))]
    [HarmonyPrefix]
    private static bool GetProgress_PreFix(GrowingPlant __instance, ref float __result)
    {
        if (!isIceFruitTree(__instance)) return true;
        
        if (WaterTemperatureSimulation.main.GetTemperature(__instance.transform.position) > maxGrowthTemperature)
        {
            __instance.SetProgress(failedGrowthPlantPercent);
            __result = failedGrowthPlantPercent;
            return false;
        }
        
        return true;
    }

    [HarmonyPatch(nameof(GrowingPlant.OnHandHover))]
    [HarmonyPrefix]
    private static bool OnHandHover_PreFix(GrowingPlant __instance)
    {
        if (!isIceFruitTree(__instance)) return true;
        if (!__instance.enabled) return false;
        
        float temperature = WaterTemperatureSimulation.main.GetTemperature(__instance.transform.position);
        if (temperature < maxGrowthTemperature)
        {
            return true;
        }

        string hoverText = Language.main.Get("IceFruitTreeTooHot").Replace("%temp%", temperature.ToString("F0"));
        HandReticle.main.SetText(HandReticle.TextType.Hand, hoverText, false);
        HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1.5f);

        return false;
    }

    private static bool isIceFruitTree(GrowingPlant __instance) => __instance.plantTechType == IceFruitTree.TechType;
}
