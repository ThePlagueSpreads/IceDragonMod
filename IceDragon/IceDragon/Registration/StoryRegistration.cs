using Nautilus.Handlers;
using Nautilus.Utility;
using Story;
using UnityEngine;

namespace IceDragon.Registration;

public static class StoryRegistration
{
    private const string PdaVoiceBus = "bus:/master/SFX_for_pause/all_no_pda_pause/all_voice_no_pda_pause/aI_voice";

    // private static BiomeGoal IcebergsHint { get; set; }
    private static BiomeGoal IcebergsDiscovery { get; set; }
    
    public static void Register()
    {
        IcebergsDiscovery = StoryGoalHandler.RegisterBiomeGoal("IcebergsDiscovery", Story.GoalType.PDA, "icebergs", 2f, 0.5f);
        RegisterVoiceLog(IcebergsDiscovery.key, "IceSpikesDiscoveryPDA", ModRegistration.Assets.LoadAsset<Sprite>("LogIcon-PDA"));
    }
    
    private static void RegisterVoiceLog(string id, string clipName, Sprite icon)
    {
        var sound = AudioUtils.CreateSound(ModRegistration.Assets.LoadAsset<AudioClip>(clipName),
            AudioUtils.StandardSoundModes_2D);

        CustomSoundHandler.RegisterCustomSound(id, sound, PdaVoiceBus);

        PDAHandler.AddLogEntry(id, id, AudioUtils.GetFmodAsset(id), icon);
    }
}