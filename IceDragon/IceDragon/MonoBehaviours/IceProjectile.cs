using IceDragon.MonoBehaviours.FreezeEffect;
using IceDragon.Registration;
using IceDragon.Registration.Prefabs;

using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceProjectile : MonoBehaviour, IManagedUpdateBehaviour
{
    public GameObject fractureVfxChild;
    public float maxLifeTime = 10;
    public float fractureDespawnDelay = 20;
    public float damage = 40;
    
    private bool _fractured;

    private float _killTime;

    public int managedUpdateIndex { get; set; }

    public string GetProfileTag()
    {
        return "IceDragon.MonoBehaviours:IceProjectile";
    }

    private void Start()
    {
        _killTime = Time.time + maxLifeTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var lm = collision.gameObject.GetComponentInParent<LiveMixin>();
        
        if (lm != null && lm.gameObject.GetComponent<IceDragonRangedAttack>() != null)
            return;
        
        bool shatter = collision.impulse.magnitude > 4;
        
        if (lm != null && lm.IsAlive())
        {
            lm.TakeDamage(damage, transform.position, DamageType.Normal, gameObject);
            AddIceFreeze(collision.collider);
            shatter = true;
        }
        
        if (shatter)
            Fracture();
    }

    public void ManagedUpdate()
    {
        if (_fractured)
            return;

        if (Time.time > _killTime)
        {
            Fracture();
        }
    }

    private void OnEnable()
    {
        BehaviourUpdateUtils.Register(this);
    }

    private void OnDisable()
    {
        BehaviourUpdateUtils.Deregister(this);
    }
    
    private void Fracture()
    {
        if (_fractured)
            return;
        
        _fractured = true;
        
        Destroy(gameObject);
        
        int hits = UWE.Utils.OverlapSphereIntoSharedBuffer(transform.position, 7);
        for (int i = 0; i < hits; i++)
        {
            Collider collider = UWE.Utils.sharedColliderBuffer[i];
            if (collider == null) continue;
            AddIceFreeze(collider);
        }
        
        fractureVfxChild.transform.SetParent(null);
        var rigidbodies = fractureVfxChild.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rigidbodies)
        {
            var wf = rb.gameObject.AddComponent<WorldForces>();
            wf.useRigidbody = rb;
            wf.underwaterGravity = 4;
            rb.gameObject.EnsureComponent<FakePickupable>().overrideTechType = FrozenBrineFragment.Info.TechType;
        }
        fractureVfxChild.SetActive(true);
        
        var center = transform.position;
        foreach (var rb in rigidbodies)
        {
            rb.AddExplosionForce(400, center, 15, 0);
        }

        if (Vector3.Distance(transform.position, MainCamera.camera.transform.position) < 40)
        {
            FMODUWE.PlayOneShot(ModAudio.IceExplode, transform.position);
        }
        Destroy(fractureVfxChild, fractureDespawnDelay);
    }

    public void AddIceFreeze(Collider collider)
    {
        Rigidbody rb = collider.GetComponentInParent<Rigidbody>();
        if (rb == null) return;
        rb.gameObject.EnsureComponent<FreezeEntity>();
    }
}
