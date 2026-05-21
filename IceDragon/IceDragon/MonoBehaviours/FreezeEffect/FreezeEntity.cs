using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace IceDragon.MonoBehaviours.FreezeEffect;

public class FreezeEntity : MonoBehaviour
{
    private Rigidbody rb;
    private Creature creature;
    private readonly List<GameObject> iceCubes = new();
    
    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (rb == null || rb.isKinematic || GetComponent<FreezeEntity>() != null)
        {
            DestroyImmediate(this);
            return;
        }
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, true);
        SendMessage("OnFreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
        creature = GetComponent<Creature>();
        creature?.GetAnimator()?.speed = 0;
        SpawnIceCubes();
        Invoke(nameof(UnFreeze), IceCubeFreezeAnimator.GetTotalAnimationTime());
    }

    private void SpawnIceCubes()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            if(collider.isTrigger || !collider.gameObject.activeInHierarchy || !collider.enabled) continue;
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

        GameObject iceCube = Instantiate(iceCubePrefab, collider.transform, false);

        DestroyImmediate(iceCube.transform.GetChild(0).GetComponent<MeshCollider>());
        DestroyImmediate(iceCube.GetComponent<PrefabIdentifier>());
        DestroyImmediate(iceCube.GetComponent<LargeWorldEntity>());
        DestroyImmediate(iceCube.GetComponent<ConstructionObstacle>());
        IceCubeFreezeAnimator iceCubeFreezeAnimator = iceCube.EnsureComponent<IceCubeFreezeAnimator>();
        
        Renderer r = iceCube.GetComponentInChildren<Renderer>();
        Vector3 prefabSize = r.bounds.size;
        float prefabMax = Mathf.Max(prefabSize.x, prefabSize.y, prefabSize.z);
        Vector3 colliderSize = collider.bounds.size;
        float colliderMaxSize = Mathf.Max(colliderSize.x, colliderSize.y, colliderSize.z);
        float newScale = colliderMaxSize / prefabMax;
        
        iceCube.transform.position = collider.bounds.center;
        iceCube.transform.rotation = collider.transform.rotation;
        iceCube.transform.Rotate(0, 90, 0, Space.Self);
        iceCube.transform.localScale = Vector3.one * newScale; 
        
        iceCubeFreezeAnimator.targetScale = newScale;
        
        iceCubes.Add(iceCube);
    }

    private void UnFreeze()
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

    private void OnDestroy() => UnFreeze();
}
