using System.Collections;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonRangedAttack : RangedAttackLastTarget
{
    public float destroyDelay = 20f;
    public float launchVelocity = 70;
    public float shootDelay = 1.25f;

    public GameObject projectile;
    public Collider[] bodyColliders;
    
    public override void Cast(RangedAttackType attackType, Vector3 directionToTarget)
    {
        StartCoroutine(ShootProjectileCoroutine(directionToTarget));
    }

    private IEnumerator ShootProjectileCoroutine(Vector3 direction)
    {
        creature.GetAnimator().SetTrigger("attack");

        yield return new WaitForSeconds(shootDelay);
        
        var instance = Instantiate(projectile, ammoSpawnPoint.position, ammoSpawnPoint.rotation);
        instance.SetActive(false);
        instance.transform.forward = ammoSpawnPoint.forward;
        var rb = instance.GetComponent<Rigidbody>();
        var worldForces = instance.AddComponent<WorldForces>();
        worldForces.useRigidbody = rb;
        ApplyMaterialsAndLighting(instance);
        
        instance.SetActive(true);
        IgnoreCollisions(projectile.GetComponent<Collider>());
        rb.AddForce(ammoSpawnPoint.forward * launchVelocity, ForceMode.VelocityChange);

        Destroy(instance, destroyDelay);
    }

    private static void ApplyMaterialsAndLighting(GameObject obj)
    {
        MaterialUtils.ApplySNShaders(obj, 5.4f, 50, 1.1f, new FresnelModifier(0.65f),
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent));
        obj.EnsureComponent<SkyApplier>().renderers = obj.GetComponentsInChildren<Renderer>();
    }

    private void IgnoreCollisions(Collider projectile)
    {
        if (projectile == null)
        {
            Plugin.Logger.LogWarning("Projectile collider not found");
            return;
        }
        
        foreach (var collider in bodyColliders)
        {
            if (collider == null)
                continue;
            
            Physics.IgnoreCollision(collider, projectile);
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