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

    private static GameObject iceCubePrefabCached;
    
    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (rb == null || rb.isKinematic)
        {
            DestroyImmediate(this);
            return;
        }
        SpawnIceCubes();
        Invoke(nameof(UnFreeze), IceCubeFreezeAnimator.GetTotalAnimationTime());
    }

    private void SpawnIceCubes()
    {
        Player player = gameObject.GetComponentInParent<Player>();
        if (player != null)
        {
            
            return;
        }
        
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            if(collider.isTrigger || !collider.gameObject.activeInHierarchy || !collider.enabled) continue;
            StartCoroutine(SpawnGenericIceCube(collider));
        }
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, true);
        SendMessage("OnFreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
        creature = GetComponent<Creature>();
        creature?.GetAnimator()?.speed = 0;
    }

    private IEnumerator SpawnGenericIceCube(Collider collider)
    {
        TaskResult<GameObject> result = new TaskResult<GameObject>();
        yield return GetIceCubePrefab(result);
        GameObject iceCube = result.value;
        iceCube.SetActive(true);
        iceCube.transform.SetParent(transform, false);
        
        Renderer r = iceCube.GetComponentInChildren<Renderer>();
        Vector3 prefabSize = r.bounds.size;
        float prefabMax = Mathf.Max(prefabSize.x, prefabSize.y, prefabSize.z);
        Vector3 colliderSize = collider.bounds.size;
        float colliderMaxSize = Mathf.Max(colliderSize.x, colliderSize.y, colliderSize.z);
        float newScale = colliderMaxSize / prefabMax;
        
        iceCube.transform.position = collider.bounds.center;
        iceCube.transform.rotation = collider.transform.rotation;
        iceCube.transform.Rotate(0, 90, 0, Space.Self);
        iceCube.transform.localScale = Vector3.zero;
        
        IceCubeFreezeAnimator iceCubeFreezeAnimator = iceCube.GetComponent<IceCubeFreezeAnimator>();
        iceCubeFreezeAnimator.targetScale = newScale;
        
        iceCubes.Add(iceCube);
    }

    private IEnumerator GetIceCubePrefab(IOut<GameObject> iceCubePrefab)
    {
        if (iceCubePrefabCached)
        {
            iceCubePrefab.Set(iceCubePrefabCached);
            yield break;
        }
        
        IPrefabRequest prefabRequest = PrefabDatabase.GetPrefabAsync("Kallies_GlacialRock_2_Transparent");
        yield return prefabRequest;
        if (!prefabRequest.TryGetPrefab(out GameObject originalPrefab))
        {
            Plugin.Logger.LogError("Failed to ice cube prefab!");
            yield break;
        }

        GameObject iceCube = UWE.Utils.InstantiateDeactivated(originalPrefab);
        DestroyImmediate(iceCube.transform.GetChild(0).GetComponent<MeshCollider>());
        DestroyImmediate(iceCube.GetComponent<PrefabIdentifier>());
        DestroyImmediate(iceCube.GetComponent<LargeWorldEntity>());
        DestroyImmediate(iceCube.GetComponent<ConstructionObstacle>());
        iceCube.EnsureComponent<IceCubeFreezeAnimator>();
        iceCubePrefab.Set(iceCube);
        iceCubePrefabCached = iceCube;
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
