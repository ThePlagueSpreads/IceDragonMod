using System.Collections;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using IceDragon.MaterialModifiers;
using Nautilus.Assets;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;

namespace IceDragon.Registration.Prefabs;

public class IceDragonPrefab() : CreatureAsset(PrefabInfo.WithTechType("IceDragon"))
{
    private const float SwimVelocity = 16f;
    private const float ChaseVelocity = 19f;
    private const float MaxVelocity = 22f;
    
    private const float SwimPriority = 0.3f;
    private const float AvoidObstaclesPriority = 0.4f;
    private const float AvoidTerrainPriority = 0.9f;
    private const float StayAtLeashPriority = 0.9f;
    private const float FleePriority = 0.35f;

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => ModRegistration.Assets.LoadAsset<GameObject>("IceDragonPrefab"),
            BehaviourType.Leviathan, EcoTargetType.Leviathan, 8_000)
        {
            SwimRandomData = new SwimRandomData(SwimPriority, SwimVelocity, new Vector3(30, 2, 30), 3, 1.2f, true),
            SwimBehaviourData = new SwimBehaviourData(0.2f),
            LocomotionData = new LocomotionData(12f, 0.02f, 1, 0.1f, true),
            Mass = 3500,
            AvoidTerrainData = new AvoidTerrainData(AvoidTerrainPriority, SwimVelocity, 30, 30),
            AnimateByVelocityData = new AnimateByVelocityData(MaxVelocity),
            StayAtLeashData = new StayAtLeashData(StayAtLeashPriority, SwimVelocity, 100),
            FleeOnDamageData = new FleeOnDamageData(FleePriority, SwimVelocity, 400f),
            EyeFOV = -0.9f,
            BioReactorCharge = 10000,
            CellLevel = LargeWorldEntity.CellLevel.Far,
            RespawnData = new RespawnData(false),
            CanBeInfected = false,
            BehaviourLODData = new BehaviourLODData(1000, 2000, 5000)
        };
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        var tailRoot = prefab.transform.SearchChild("Tail");
        var tailTrailManager = new TrailManagerBuilder(components, tailRoot, 2f);
        tailTrailManager.SetTrailArrayToChildrenWithCondition(t => t.name.ToLower().StartsWith("tail"));
        tailTrailManager.Apply();
        
        components.Rigidbody.angularDrag = 0.05f;
        components.WorldForces.underwaterDrag = 0.05f;

        var modifier = prefab.AddComponent<DamageModifier>();
        modifier.damageType = DamageType.Cold;
        modifier.multiplier = 0f;
        
        if (Plugin.RedPlagueInstalled)
        {
            var infectedMixin = prefab.AddComponent<InfectedMixin>();
            infectedMixin.renderers = prefab.GetComponentsInChildren<Renderer>(true);
        }
        
        var emitter = prefab.AddComponent<FMOD_CustomEmitter>();
        emitter.followParent = true;
        emitter.SetAsset(ModAudio.Roar);
        
        var voice = prefab.AddComponent<CreatureVoice>();
        voice.emitter = emitter;
        voice.closeIdleSound = ModAudio.Roar;
        voice.animator = components.Animator;
        voice.animatorTriggerParam = "roar";
        voice.minInterval = 20;
        voice.maxInterval = 30;
        voice.playSoundOnStart = true;
        voice.farThreshold = 100f;
        
        yield return null;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 4, 5, 1f, new IceDragonMaterialModifier());
    }
}
