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
            absorption: new Vector3(40f, 38f, 10f),
            scattering: 1f,
            scatteringColor: new Color(0.3f, 0.4f, 1f, 1f),
            murkiness: 0.3f,
            emissive: new Color(0.3f, 0.4f, 1f, 1f),
            emissiveScale: 0.05f,
            startDistance: 20f,
            sunlightScale: 1f,
            ambientScale: 1f,
            temperature: -10f
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
