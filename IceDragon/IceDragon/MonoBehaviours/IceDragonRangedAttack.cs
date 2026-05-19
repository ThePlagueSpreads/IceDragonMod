using System.Collections;
using IceDragon.Registration;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonRangedAttack : RangedAttackLastTarget
{
    public float launchVelocity = 120;
    public float shootDelay = 1.25f;

    public GameObject projectile;
    public Collider[] bodyColliders;
    
    public override void Cast(RangedAttackType attackType, Vector3 directionToTarget)
    {
        StartCoroutine(ShootProjectileCoroutine(directionToTarget));
        FMODUWE.PlayOneShot(ModAudio.ShootIce, transform.position);
    }

    private IEnumerator ShootProjectileCoroutine(Vector3 dir)
    {
        creature.GetAnimator().SetTrigger("shoot_ice");

        yield return new WaitForSeconds(shootDelay);
        
        var instance = Instantiate(projectile, ammoSpawnPoint.position, ammoSpawnPoint.rotation);
        instance.SetActive(false);
        instance.transform.forward = ammoSpawnPoint.forward;
        var rb = instance.GetComponent<Rigidbody>();
        var worldForces = instance.AddComponent<WorldForces>();
        worldForces.useRigidbody = rb;
        ApplyMaterialsAndLighting(instance);
        instance.AddComponent<VFXSurface>().surfaceType = VFXSurfaceTypes.glass;
        
        IgnoreCollisions(projectile.GetComponent<Collider>());
        instance.SetActive(true);
        rb.AddForce((ammoSpawnPoint.forward + dir * 0.5f).normalized * launchVelocity, ForceMode.VelocityChange);
        instance.AddComponent<IceProjectile>().fractureVfxChild = instance.transform.Find("Fracture").gameObject;
    }

    private static void ApplyMaterialsAndLighting(GameObject obj)
    {
        var model = obj.transform.Find("IceDragonProjectile");
        
        MaterialUtils.ApplySNShaders(model.gameObject, 5.4f, 50, 1.1f, new FresnelModifier(0.65f),
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent));
        model.gameObject.EnsureComponent<SkyApplier>().renderers = model.GetComponents<Renderer>();
        
        var material = model.GetComponent<Renderer>().sharedMaterial;
        
        var fractureParent = obj.transform.Find("Fracture");
        foreach (var fragment in fractureParent.GetComponentsInChildren<Renderer>())
        {
            fragment.sharedMaterial = material;
        }
        fractureParent.gameObject.AddComponent<SkyApplier>().renderers = fractureParent.GetComponentsInChildren<Renderer>(true);
    }

    private void IgnoreCollisions(Collider projectileCollider)
    {
        if (projectileCollider == null)
        {
            Plugin.Logger.LogWarning("Projectile collider not found");
            return;
        }
        
        foreach (var collider in bodyColliders)
        {
            if (collider == null)
                continue;
            
            Physics.IgnoreCollision(collider, projectileCollider);
        }
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        base.Perform(creature, time, deltaTime);
        if (currentTarget != null)
            swimBehaviour.LookAt(currentTarget.transform);
    }

    public override void StopPerform(Creature creature, float time)
    {
        base.StopPerform(creature, time);
        swimBehaviour.LookAt(null);
    }
}