using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonMeleeTrigger : MonoBehaviour
{
    public IceDragonMeleeAttack melee;
    public bool lower;
    
    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.isTrigger || collider.gameObject.layer == LayerID.Useable)
        {
            if (lower && !collider.gameObject.CompareTag("Player") && collider.gameObject.layer != LayerID.Vehicle)
                return;
            
            melee.OnTouch(collider, lower);
        }
    }
}