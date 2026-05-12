using System.Collections.Generic;
using System.Linq;
using Nautilus.FMod;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace IceDragon.Registration;

public static class ModAudio
{
    public static FMODAsset Roar { get; } = AudioUtils.GetFmodAsset("PodshellMusic");

    public static void RegisterAudio()
    {
        RegisterDragonSound(Roar, 10, 400, [
            "icedragon_roar1",
            "icedragon_roar2",
            "icedragon_roar3",
            "icedragon_roar4",
            "icedragon_roar5"
        ]);
    }

    private static void RegisterDragonSound(FMODAsset asset, string clipName, float minDistance, float maxDistance, string bus = AudioUtils.BusPaths.UnderwaterCreatures)
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
        
        var multiSoundsEvent = new FModMultiSounds(sounds.ToArray(), AudioUtils.BusPaths.UnderwaterCreatures, true);
        
        CustomSoundHandler.RegisterCustomSound(asset.path, multiSoundsEvent);
    }
}