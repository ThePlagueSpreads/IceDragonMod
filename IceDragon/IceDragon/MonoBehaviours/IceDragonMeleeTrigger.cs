using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonMeleeTrigger : MonoBehaviour
{
    public IceDragonMeleeAttack melee;
    
    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.isTrigger || collider.gameObject.layer == LayerID.Useable)
        {
            melee.OnTouch(collider);
        }
    }
}