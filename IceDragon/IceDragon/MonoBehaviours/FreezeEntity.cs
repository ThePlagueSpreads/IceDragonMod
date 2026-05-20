using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
namespace IceDragon.MonoBehaviours;


public class FreezeEntity : MonoBehaviour
{
    private const float freezeTimeSeconds = 7;
    private const float scaleDown = 4;
    
    private Rigidbody rb;
    private Creature creature;
    private readonly List<GameObject> iceCubes = new();
    
    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (rb == null && !rb.isKinematic)
        {
            DestroyImmediate(this);
            return;
        }
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, true);
        SendMessage("OnFreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
        creature = GetComponent<Creature>();
        creature?.GetAnimator()?.speed = 0;
        SpawnIceCubes();
    }

    private void SpawnIceCubes()
    {
        Collider[] colliders = this.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            if(collider.isTrigger || !collider.gameObject.activeInHierarchy ||  !collider.enabled) continue;
            StartCoroutine(SpawnIceCube(collider));
        }
    }

    private IEnumerator SpawnIceCube(Collider collider)
    {
        IPrefabRequest iceCubeRequest = PrefabDatabase.GetPrefabAsync("Kallies_GlacialRock_2_Transparent");
        yield return iceCubeRequest;
        if (!iceCubeRequest.TryGetPrefab(out GameObject iceCubePrefab))
        {
            Plugin.Logger.LogError("Failed to ice cube prefab!");
            yield break;
        }

        float colliderScaleMultiplier = 1;
        switch (collider)
        {
            case SphereCollider sphereCollider:
                colliderScaleMultiplier = sphereCollider.radius;
                break;
            case CapsuleCollider capsuleCollider:
                colliderScaleMultiplier = capsuleCollider.radius;
                break;
            case BoxCollider boxCollider:
                colliderScaleMultiplier = (boxCollider.size.x + boxCollider.size.y + boxCollider.size.z) / 3;
                break;
        }

        GameObject iceCube = Instantiate(iceCubePrefab, collider.transform, false);
        DestroyImmediate(iceCube.transform.GetChild(0).GetComponent<MeshCollider>());
        DestroyImmediate(iceCube.GetComponent<PrefabIdentifier>());
        DestroyImmediate(iceCube.GetComponent<LargeWorldEntity>());
        DestroyImmediate(iceCube.GetComponent<ConstructionObstacle>());
        iceCube.transform.position = collider.bounds.center;
        iceCube.transform.localScale = (new Vector3(1/collider.transform.lossyScale.x, 1/collider.transform.lossyScale.y, 1/collider.transform.lossyScale.z) * colliderScaleMultiplier) / scaleDown;
        iceCube.transform.rotation = collider.transform.rotation;
        iceCube.transform.Rotate(0, 90, 0, Space.Self);
        iceCubes.Add(iceCube);
    }

    private void OnDestroy()
    {
        if (!gameObject.activeInHierarchy) return;
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, false);
        SendMessage("OnUnfreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
        foreach (GameObject iceCube in iceCubes)
        {
            DestroyImmediate(iceCube); 
        }
        creature?.GetAnimator()?.speed = 1;
    }
}
