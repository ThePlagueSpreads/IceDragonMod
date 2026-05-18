using System.Collections;
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
    private const float MaxVelocity = 18f;
    
    private const float SwimPriority = 0.3f;
    private const float AvoidTerrainPriority = 0.7f;
    private const float AvoidObstaclesPriority = 0.69f;
    private const float StayAtLeashPriority = 0.79f;
    private const float FleePriority = 0.35f;
    private const float AttackLastTargetPriority = 0.75f;
    private const float AttackCyclopsPriority = 0.83f;

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => ModRegistration.Assets.LoadAsset<GameObject>("IceDragonPrefab"),
            BehaviourType.Leviathan, EcoTargetType.Leviathan, 8_000)
        {
            SwimRandomData = new SwimRandomData(SwimPriority, SwimVelocity, new Vector3(33, 2, 33), 3.5f, 1f, true),
            SwimBehaviourData = new SwimBehaviourData(0.3f),
            LocomotionData = new LocomotionData(12, 0.15f, 1, 0f, true),
            Mass = 3500,
            AvoidTerrainData = new AvoidTerrainData(AvoidTerrainPriority, SwimVelocity, 30, 30),
            AvoidObstaclesData = new AvoidObstaclesData(AvoidObstaclesPriority, SwimVelocity, true, 10, 10),
            AnimateByVelocityData = new AnimateByVelocityData(MaxVelocity, 30, 3, false, 3),
            StayAtLeashData = new StayAtLeashData(StayAtLeashPriority, SwimVelocity, 90),
            FleeOnDamageData = new FleeOnDamageData(FleePriority, SwimVelocity, 400f),
            EyeFOV = -0.9f,
            BioReactorCharge = 10000,
            CellLevel = LargeWorldEntity.CellLevel.Far,
            RespawnData = new RespawnData(false),
            CanBeInfected = false,
            BehaviourLODData = new BehaviourLODData(1000, 2000, 5000),
            AggressiveToPilotingVehicleData = new AggressiveToPilotingVehicleData(40, 0.3f),
            AttackCyclopsData = new AttackCyclopsData(AttackCyclopsPriority, ChaseVelocity, 140, 0.4f, 4, 0.01f, 0.6f),
        };
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        var tailRoot = prefab.transform.SearchChild("Tail");
        var tailTrailManager = new TrailManagerBuilder(components, tailRoot, 6f);
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

        var head = prefab.transform.SearchChild("Head").gameObject;
        
        var emitter = head.AddComponent<FMOD_CustomEmitter>();
        emitter.followParent = true;
        emitter.SetAsset(ModAudio.Roar);
        
        var voice = prefab.AddComponent<CreatureVoice>();
        voice.emitter = emitter;
        voice.closeIdleSound = ModAudio.Roar;
        voice.farIdleSound = ModAudio.FarRoar;
        voice.animator = components.Animator;
        voice.animatorTriggerParam = "roar";
        voice.minInterval = 20;
        voice.maxInterval = 30;
        voice.playSoundOnStart = true;
        voice.farThreshold = 50f;
        
        // AGGRESSION
        var aggression = prefab.AddComponent<AggressiveWhenSeePlayer>();
        aggression.targetType = EcoTargetType.Shark;
        aggression.playerAttackInterval = 10f;
        aggression.maxRangeMultiplier = MaxRangeMultiplierCurve;
        aggression.distanceAggressionMultiplier = DistanceAggressionMultiplierCurve;
        aggression.lastTarget = components.LastTarget;
        aggression.creature = components.Creature;
        aggression.aggressionPerSecond = 1.6f;
        aggression.maxRangeScalar = 150;
        aggression.maxSearchRings = 3;
        aggression.ignoreSameKind = true;
        aggression.targetShouldBeInfected = false;
        aggression.minimumVelocity = 0;
        aggression.hungerThreshold = 0;
        
        var attackLastTarget = prefab.AddComponent<IceDragonAttack>();
        attackLastTarget.evaluatePriority = AttackLastTargetPriority;
        attackLastTarget.swimVelocity = ChaseVelocity;
        attackLastTarget.aggressionThreshold = 0.6f;
        attackLastTarget.minAttackDuration = 8;
        attackLastTarget.maxAttackDuration = 18;
        attackLastTarget.pauseInterval = 17;
        attackLastTarget.rememberTargetTime = 5;
        attackLastTarget.resetAggressionOnTime = true;
        attackLastTarget.lastTarget = components.LastTarget;
        attackLastTarget.voice = voice;

        // GRAB ATTACK
        var grab = prefab.AddComponent<IceDragonGrab>();
        grab.creature = components.Creature;
        grab.seamothAttachPoint = prefab.transform.SearchChild("VehicleAttachPoint");
        var grabSound = head.AddComponent<FMOD_CustomLoopingEmitter>();
        grabSound.followParent = true;
        grabSound.playOnAwake = false;
        grabSound.SetAsset(AudioUtils.GetFmodAsset("event:/creature/reaper/attack_seamoth"));
        grab.grabSound = grabSound;
        
        // MELEE ATTACK
        var meleeTrigger = prefab.transform.SearchChild("AttackTrigger");
        var lowerMeleeTrigger = prefab.transform.SearchChild("LowerAttackTrigger");
        
        var meleeAttack = prefab.AddComponent<IceDragonMeleeAttack>();
        meleeAttack.grab = grab;
        meleeAttack.creature = components.Creature;
        meleeAttack.animator = components.Animator;
        meleeAttack.lastTarget = components.LastTarget;
        meleeAttack.liveMixin = components.LiveMixin;
        meleeAttack.mouth = meleeAttack.gameObject;
        meleeAttack.lowerTrigger = lowerMeleeTrigger.gameObject;
        
        var biteSound = head.AddComponent<FMOD_CustomEmitter>();
        biteSound.followParent = true;
        biteSound.playOnAwake = false;
        biteSound.SetAsset(AudioUtils.GetFmodAsset("event:/creature/spine_eel/bite"));
        meleeAttack.biteSoundEmitter = biteSound;
        
        // upper trigger
        meleeTrigger.gameObject.AddComponent<IceDragonMeleeTrigger>().melee = meleeAttack;
        
        // lower trigger
        var lower = lowerMeleeTrigger.gameObject.AddComponent<IceDragonMeleeTrigger>();
        lower.melee = meleeAttack;
        lower.lower = true;

        prefab.AddComponent<VFXSchoolFishRepulsor>();
        
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
        MaterialUtils.ApplySNShaders(prefab, 5, 5, 1f, new IceDragonMaterialModifier());
    }
    
    private static readonly AnimationCurve MaxRangeMultiplierCurve = new(new Keyframe[3]
    {
        new Keyframe(0.0f, 1f, 0.0f, 0.0f, 0.333f, 0.333f),
        new Keyframe(0.5f, 0.5f, 0.0f, 0.0f, 0.333f, 0.333f),
        new Keyframe(1f, 1f, 0.0f, 0.0f, 0.333f, 0.333f)
    });
    
    private static readonly AnimationCurve DistanceAggressionMultiplierCurve = new(new Keyframe[2]
    {
        new Keyframe(0.0f, 1f, 0.0f, 0.0f, 0.333f, 0.333f),
        new Keyframe(1f, 0.0f, -3f, -3f, 0.333f, 0.333f)
    });
}
