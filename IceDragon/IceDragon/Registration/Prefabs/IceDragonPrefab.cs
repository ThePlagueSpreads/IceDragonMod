using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using IceDragon.MaterialModifiers;
using IceDragon.MonoBehaviours;
using Nautilus.Assets;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace IceDragon.Registration.Prefabs;

public class IceDragonPrefab() : CreatureAsset(PrefabInfo.WithTechType("IceDragon"))
{
    private const float SwimVelocity = 16f;
    private const float ChaseVelocity = 22f;
    private const float MaxVelocity = 20f;
    
    private const float SwimPriority = 0.3f;
    private const float AvoidTerrainPriority = 0.7f;
    private const float StayAtLeashPriority = 0.79f;
    private const float FleePriority = 0.35f;
    private const float AttackLastTargetPriority = 0.75f;
    private const float AttackCyclopsPriority = 0.83f;

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => ModRegistration.Assets.LoadAsset<GameObject>("IceDragonPrefab"),
            BehaviourType.Leviathan, EcoTargetType.Leviathan, 8_000)
        {
            SwimRandomData = new SwimRandomData(SwimPriority, SwimVelocity, new Vector3(33, 2, 33), 3.5f, 1.2f, true),
            SwimBehaviourData = new SwimBehaviourData(0.3f),
            LocomotionData = new LocomotionData(13f, 0.03f, 1, 0f, true),
            Mass = 3500,
            AvoidTerrainData = new AvoidTerrainData(AvoidTerrainPriority, SwimVelocity, 30, 30),
            AnimateByVelocityData = new AnimateByVelocityData(MaxVelocity, 30, 3, false, 3),
            StayAtLeashData = new StayAtLeashData(StayAtLeashPriority, SwimVelocity, 90),
            FleeOnDamageData = new FleeOnDamageData(FleePriority, SwimVelocity, 400f),
            EyeFOV = -0.9f,
            BioReactorCharge = 10000,
            CellLevel = LargeWorldEntity.CellLevel.Far,
            RespawnData = new RespawnData(false),
            CanBeInfected = false,
            BehaviourLODData = new BehaviourLODData(1000, 2000, 5000),
            AggressiveWhenSeeTargetList = [
                new AggressiveWhenSeeTargetData(EcoTargetType.Shark, 1.3f, 150, 3, false)
            ],
            AggressiveToPilotingVehicleData = new AggressiveToPilotingVehicleData(40, 0.3f),
            AttackCyclopsData = new AttackCyclopsData(AttackCyclopsPriority, ChaseVelocity, 140, 0.4f, 4, 0.01f, 0.6f),
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
        
        var emitter = prefab.transform.SearchChild("Head").gameObject.AddComponent<FMOD_CustomEmitter>();
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
        
        var attack = new GameObject().AddComponent<IceDragonAttack>();
        attack.evaluatePriority = AttackLastTargetPriority;
        attack.swimVelocity = ChaseVelocity;
        attack.aggressionThreshold = 0.6f;
        attack.minAttackDuration = 3;
        attack.maxAttackDuration = 12;
        attack.pauseInterval = 17;
        attack.rememberTargetTime = 5;
        attack.resetAggressionOnTime = true;
        attack.lastTarget = components.LastTarget;
        attack.voice = voice;
        
        yield return null;
    }
    
    protected override void PostRegister()
    {
        PDAScanner.EntryData entryData = new PDAScanner.EntryData
        {
            key = TechType,
            encyclopedia = "IceDragon",
            scanTime = 8,
            isFragment = false,
            destroyAfterScan = false
        };
        PDAHandler.AddCustomScannerEntry(entryData);

        PDAHandler.AddEncyclopediaEntry(
            key: "IceDragon",
            path: "Lifeforms/Fauna/Leviathans",
            title: null,
            desc: null,
            image: ModRegistration.Assets.LoadAsset<Texture2D>("IceDragonEncyFramed"),
            popupImage: ModRegistration.Assets.LoadAsset<Sprite>("IceDragonPopup")
        );
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 4, 5, 1f, new IceDragonMaterialModifier());
    }
}
