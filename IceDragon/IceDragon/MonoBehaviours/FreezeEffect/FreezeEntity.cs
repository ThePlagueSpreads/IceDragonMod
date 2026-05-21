using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace IceDragon.MonoBehaviours.FreezeEffect;

public class FreezeEntity : MonoBehaviour
{
    private const float playerAnimationSpeedMultiplier = 2.5f;
    
    private Rigidbody rb;
    private Creature creature;
    private Player player;
    private readonly List<GameObject> iceCubes = new();

    //private static GameObject iceCubePrefabCached;
    
    private void Start()
    { 
        rb = GetComponentInParent<Rigidbody>();
        player = GetComponentInParent<Player>();
        creature = GetComponentInParent<Creature>();
        if ((rb == null || rb.isKinematic) ||
            (player != null && player.motorMode == Player.MotorMode.Walk) ||
            (creature != null && creature.GetComponent<IceDragonAttack>() != null))//dont allow on ice dragon
        {
            DestroyImmediate(this);
            return;
        }
        
        SpawnIceCubes();
    }

    private void SpawnIceCubes()
    {
        
        if (player != null)
        {
            player.cinematicModeActive = true;
            StartCoroutine(SpawnPlayerIceCube());
            Invoke(nameof(DestroyComponent), IceCubeFreezeAnimator.GetTotalAnimationTime() * (1/playerAnimationSpeedMultiplier));
            InvokeRepeating(nameof(DamagePlayer), 0f, 0.4f);
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
        creature?.GetAnimator()?.speed = 0;
        Invoke(nameof(DestroyComponent), IceCubeFreezeAnimator.GetTotalAnimationTime());
    }

    private IEnumerator SpawnPlayerIceCube()
    {
        TaskResult<GameObject> result = new TaskResult<GameObject>();
        yield return GetIceCubePrefab(result);
        GameObject iceCubePrefab = result.value;
        GameObject iceCube = Instantiate(iceCubePrefab, transform, false);
        iceCube.SetActive(true);

        const float iceCubePlayerScale = 1;
        iceCube.transform.position = MainCamera.camera.transform.position; 
        iceCube.transform.localPosition += new Vector3(0, 0, -0.15f);
        iceCube.transform.Rotate(0, 90, 0, Space.Self);
        iceCube.transform.localScale = Vector3.one * iceCubePlayerScale;
        IceCubeFreezeAnimator iceCubeFreezeAnimator = iceCube.GetComponent<IceCubeFreezeAnimator>();
        iceCubeFreezeAnimator.targetScale = iceCubePlayerScale;
        iceCubeFreezeAnimator.animSpeed = playerAnimationSpeedMultiplier;
        iceCubes.Add(iceCube);
    }
    
    private void DamagePlayer()
    {
        player.liveMixin.TakeDamage(1, transform.position, DamageType.Cold);
    }

    private IEnumerator SpawnGenericIceCube(Collider collider)
    {
        TaskResult<GameObject> result = new TaskResult<GameObject>();
        yield return GetIceCubePrefab(result);
        GameObject iceCubePrefab = result.value;
        GameObject iceCube = Instantiate(iceCubePrefab, transform, false);
        iceCube.SetActive(true);
        
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
        /*
        if (iceCubePrefabCached)
        {
            iceCubePrefab.Set(iceCubePrefabCached);
            yield break;
        }*/
        
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
        //iceCubePrefabCached = iceCube;
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
        player?.cinematicModeActive = false;
    }

    private void OnDestroy() => UnFreeze();

    private void DestroyComponent() => DestroyImmediate(this);
}
