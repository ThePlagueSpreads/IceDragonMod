using System.Collections.Generic;
using System.Linq;
using FMOD;
using Nautilus.Extensions;
using Nautilus.FMod;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace IceDragon.Registration;

public static class ModAudio
{
    public static FMODAsset Roar { get; } = AudioUtils.GetFmodAsset("IceDragonRoar");
    public static FMODAsset Snarl { get; } = AudioUtils.GetFmodAsset("IceDragonSnarl");
    public static FMODAsset Bite { get; } = AudioUtils.GetFmodAsset("IceDragonBite");
    public static FMODAsset ShootIce { get; } = AudioUtils.GetFmodAsset("IceDragonShootIce");
    public static FMODAsset IceExplode { get; } = AudioUtils.GetFmodAsset("IceDragonProjectileShoot");
    public static FMODAsset VehicleAttack { get; } = AudioUtils.GetFmodAsset("IceDragonVehicleAttack");
    
    public static FMODAsset BiomeMusic { get; } = AudioUtils.GetFmodAsset("IceSpikesMusic");

    private const string ReverbBus = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/reverbsend";

    public static void RegisterAudio()
    {
        RegisterDragonSound(Roar, 10, 400, [
            "icedragon_new_roar_1",
            "icedragon_new_roar_2",
            "icedragon_new_roar_3",
            "icedragon_new_roar_4",
            "icedragon_new_roar_5"
        ]);
        
        RegisterDragonSound(Snarl, 10, 120f, [
            "icedragon_snarl1",
            "icedragon_snarl2",
            "icedragon_snarl3",
            "icedragon_snarl4"
        ]);
        
        RegisterDragonSound(Bite, 10, 40f, [
            "icedragon_bite"
        ]);
        
        RegisterDragonSound(ShootIce, "IceDragonProjectileShoot", 8, 100, AudioUtils.BusPaths.UnderwaterCreatures);
        RegisterDragonSound(IceExplode, "IceProjectileShatter", 5, 40, AudioUtils.BusPaths.SFX);
        RegisterDragonSound(VehicleAttack, "IceDragonVehicleAttack", 8, 70, AudioUtils.BusPaths.SFX);
        
        RegisterMusic(BiomeMusic, "IceSpikes");
    }

    private static void RegisterDragonSound(FMODAsset asset, string clipName, float minDistance, float maxDistance, string bus = ReverbBus)
    {
        var sound = AudioUtils.CreateSound(ModRegistration.Assets.LoadAsset<AudioClip>(clipName), AudioUtils.StandardSoundModes_3D);
        sound.set3DMinMaxDistance(minDistance, maxDistance);
        CustomSoundHandler.RegisterCustomSound(asset.path, sound, bus);
    }
    
    private static void RegisterDragonSound(FMODAsset asset, float minDistance, float maxDistance, string[] clipNames)
    {
        var clipList = new List<AudioClip>();
        clipNames.ForEach(clipName => clipList.Add(ModRegistration.Assets.LoadAsset<AudioClip>(clipName)));

        var sounds = AudioUtils.CreateSounds(clipList,
            maxDistance >= 0 ? AudioUtils.StandardSoundModes_3D : AudioUtils.StandardSoundModes_2D);
        sounds.ForEach(sound =>
        {
            if (maxDistance >= 0)
                sound.set3DMinMaxDistance(minDistance, maxDistance);
        });
        
        var multiSoundsEvent = new FModMultiSounds(sounds.ToArray(), ReverbBus, true);
        
        CustomSoundHandler.RegisterCustomSound(asset.path, multiSoundsEvent);
    }
    
    private static void RegisterMusic(FMODAsset asset, string clipName, float fadeOutDuration = 3f,
        bool useSoundEffectsBus = false, bool looping = false)
    {
        var mode = AudioUtils.StandardSoundModes_2D;
        if (looping)
            mode |= MODE.LOOP_NORMAL;
        var sound = AudioUtils.CreateSound(ModRegistration.Assets.LoadAsset<AudioClip>(clipName), mode);
        if (fadeOutDuration > Mathf.Epsilon)
            sound.AddFadeOut(fadeOutDuration);
        CustomSoundHandler.RegisterCustomSound(asset.id, sound,
            useSoundEffectsBus ? "bus:/master/SFX_for_pause/PDA_pause/all" : AudioUtils.BusPaths.Music);
    }
}