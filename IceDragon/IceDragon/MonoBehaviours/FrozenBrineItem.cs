using IceDragon.MonoBehaviours.FreezeEffect;
using IceDragon.Registration;
using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class FrozenBrineItem : MonoBehaviour, IPropulsionCannonAmmo
{
    public float health = 10;
    
    public void OnGrab() { }
    public void OnShoot() { }
    public void OnRelease() { }
    public void OnImpact()
    {
        Destroy(gameObject);
        
        if (Vector3.Distance(transform.position, MainCamera.camera.transform.position) < 40)
        {
            FMODUWE.PlayOneShot(ModAudio.IceExplode, transform.position);
        }
        
        int hits = UWE.Utils.OverlapSphereIntoSharedBuffer(transform.position, 5);
        for (int i = 0; i < hits; i++)
        {
            Collider collider = UWE.Utils.sharedColliderBuffer[i];
            if (collider == null) continue;
            AddIceFreeze(collider);
        }
    }
    public bool GetAllowedToGrab() => true;
    public bool GetAllowedToShoot() => true;
    
    public static void AddIceFreeze(Collider collider)
    {
        Rigidbody rb = collider.GetComponentInParent<Rigidbody>();
        if (rb == null) return;
        rb.gameObject.EnsureComponent<FreezeEntity>();
    }
}