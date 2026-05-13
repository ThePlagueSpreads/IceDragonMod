using ECCLibrary.Mono;
using IceDragon.Registration;
using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonAttack : AttackLastTarget
{
    public CreatureVoice voice;
    
    public override void StartPerform(Creature creature, float time)
    {
        base.StartPerform(creature, time);
        if (Time.time > voice.TimeLastPlayed + 6)
        {
            voice.emitter.SetAsset(ModAudio.Snarl);
            voice.emitter.Play();
            if (voice.animator) voice.animator.SetTrigger(voice.animatorTriggerParam);
            voice.BlockIdleSoundsForTime(10);
        }
    }
}