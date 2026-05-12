using System.Collections;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using UnityEngine;

namespace IceDragon.Registration.Prefabs;


public class IceDragon(PrefabInfo prefabInfo) : CreatureAsset(prefabInfo)
{
    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => AssetBundles.assets.LoadAsset<GameObject>("ASSET NAME HERE KALLIE"),
            BehaviourType.Shark, EcoTargetType.Shark, 5000)
        {
            SwimRandomData = null,
            LocomotionData = new LocomotionData(40f),
            Mass = 1000,
            AvoidObstaclesData = new AvoidObstaclesData(0.4f, 6f, false, 5f, 6f, scanInterval: 0.2f)
        };
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        yield return null;
    }
}
