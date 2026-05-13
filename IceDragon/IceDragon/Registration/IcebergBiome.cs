using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
namespace IceDragon.Registration;


public class IcebergBiome
{
    public static void Register()
    {
        WaterscapeVolume.Settings settings = BiomeUtils.CreateBiomeSettings(
            absorption: new Vector3(10f, 5f, 4f),
            scattering: 1.5f,
            scatteringColor: new Color(0f, 0.571f, 0.745f, 1f),
            murkiness: 0.5f,
            emissive: new Color(0.099f, 0.287f, 0.396f, 1f),
            emissiveScale: 0.2f,
            startDistance: 50f,
            sunlightScale: 0.8f,
            ambientScale: 1f,
            temperature: -50f
        );
        
        /*BiomeUtils.SkyPrefabFixer skyPrefab = BiomeUtils.CreateSkyPrefab(
            name: "AbyssalMeadowsSky",
            specularCube: null, // This should NOT be null but is for now since idk what texture to use
            affectedByDayNightCycle: true,
            outdoors: true
        );
        skyPrefab.masterIntensity = 10f;*/

        BiomeHandler.RegisterBiome("Icebergs", settings, new BiomeHandler.SkyReference("SkyKooshZone"));
        
        ConsoleCommandsHandler.AddBiomeTeleportPosition("icebergs", new Vector3(-339, 0, 1046));

        RegisterAtmosphereVolume(PrefabInfo.WithTechType("IcebergsBiomeSphereVolume"));
    }
    
    private static void RegisterAtmosphereVolume(PrefabInfo info)
    {
        CustomPrefab volumePrefab = new CustomPrefab(info);
        AtmosphereVolumeTemplate volumeTemplate = new AtmosphereVolumeTemplate(info, 
            AtmosphereVolumeTemplate.VolumeShape.Sphere, "Icebergs", 10, LargeWorldEntity.CellLevel.VeryFar);
        volumePrefab.SetGameObject(volumeTemplate);
        volumePrefab.Register();
    }
}
