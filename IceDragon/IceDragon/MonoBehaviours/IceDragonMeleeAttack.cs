using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonMeleeAttack : MonoBehaviour, IManagedUpdateBehaviour
{
	private static readonly int BiteAnimID = Animator.StringToHash("bite");
	private static readonly int LowerBiteAnimID = Animator.StringToHash("lower_bite");
	
	public float biteAggressionThreshold = 0.2f;
	public float biteInterval = 3.5f;
	public float biteDamage = 90;
	public float cyclopsDamage = 240f;
	private float _timeLastBite;
	public float biteAggressionDecrement = 0.4f;

	public IceDragonGrab grab;
	public GameObject mouth;
	public GameObject lowerTrigger;
	public LastTarget lastTarget;
	public Creature creature;
	public LiveMixin liveMixin;
	public Animator animator;
	public FMOD_CustomEmitter biteSoundEmitter;

	public GameObject damageFX;
	
	public bool ignoreSameKind = false;

	public bool canBiteCreature = true;
	public bool canBitePlayer = true;
	public bool canBiteVehicle = false; // it grabs, does not bite
	public bool canBiteCyclops = true;

	private bool _frozen;
	private bool _wasBiting;
	private bool _initBiting;
	private bool _lastBiteIsLower;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "IceDragonMeleeAttack";
	}

	private void OnEnable()
	{
		BehaviourUpdateUtils.Register(this);
	}

	protected virtual void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
		animator.SetBool(BiteAnimID, value: false);
		animator.SetBool(LowerBiteAnimID, value: false);
		_wasBiting = false;
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private bool CanBite(GameObject target)
	{
		if (_frozen)
		{
			return false;
		}
		if (creature.Aggression.Value < biteAggressionThreshold)
		{
			return false;
		}
		if (Time.time < _timeLastBite + biteInterval)
		{
			return false;
		}
		return CanDealDamageTo(target);
	}

	private bool CanDealDamageTo(GameObject target)
	{
		var player = target.GetComponent<Player>();
		if (player != null && !player.CanBeAttacked())
		{
			return false;
		}
		var isSubmarine = target.GetComponent<SubControl>() != null;
		if (isSubmarine && target != lastTarget.target)
		{
			return false;
		}
		if ((!canBitePlayer || player == null) && (!canBiteCreature || target.GetComponent<Creature>() == null) && (!canBiteVehicle || target.GetComponent<Vehicle>() == null) && (!canBiteCyclops || (!isSubmarine && target.GetComponent<CyclopsDecoy>() == null)))
		{
			return false;
		}
		var direction = target.transform.position - transform.position;
		var magnitude = direction.magnitude;
		int hits = UWE.Utils.RaycastIntoSharedBuffer(transform.position, direction, magnitude, -5, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < hits; i++)
		{
			var collider = UWE.Utils.sharedHitBuffer[i].collider;
			var hit = collider.attachedRigidbody != null ? collider.attachedRigidbody.gameObject : collider.gameObject;
			if (!(hit == target) && !(hit == gameObject) && !(hit.GetComponent<Creature>() != null))
			{
				return false;
			}
		}
		return true;
	}

	private float GetBiteDamage(GameObject target)
	{
		if (target.GetComponent<SubControl>() != null)
		{
			return cyclopsDamage;
		}
		return biteDamage;
	}

	private GameObject GetTarget(Collider collider)
	{
		var other = collider.gameObject;
		if (other.GetComponent<LiveMixin>() == null && collider.attachedRigidbody != null)
		{
			other = collider.attachedRigidbody.gameObject;
		}
		return other;
	}

	public void OnTouch(Collider collider, bool lower)
	{
		if (!liveMixin.IsAlive() || !(Time.time > _timeLastBite + biteInterval) || !(creature.Aggression.Value >= 0.5f))
		{
			return;
		}
		
		_lastBiteIsLower = lower;
		
		GameObject target = GetTarget(collider);
		if (grab.IsHoldingVehicle())
		{
			return;
		}
		
		if (!lower && grab.GetCanGrabVehicle())
		{
			bool grabbedAny = false;
			var seamoth = target.GetComponent<SeaMoth>();
			if (seamoth && !seamoth.docked)
			{
				grab.GrabSeamoth(seamoth);
				grabbedAny = true;
			}
			
			var exosuit = target.GetComponent<Exosuit>();
			if (exosuit && !exosuit.docked)
			{
				grab.GrabExosuit(exosuit);
				grabbedAny = true;
			}

			if (!grabbedAny)
			{
				var vehicle = target.GetComponent<Vehicle>();
				if (vehicle && !vehicle.docked)
				{
					grab.GrabModdedVehicle(vehicle);
				}
			}
		}
		
		OnTouchNormalPrey(collider, lower);
	}

	private void OnTouchNormalPrey(Collider collider, bool lower)
	{
		if (!enabled)
		{
			return;
		}
		
		var target = GetTarget(collider);
		
		if ((ignoreSameKind && CreatureData.GetCreatureType(gameObject) == CreatureData.GetCreatureType(target)) || !liveMixin.IsAlive())
		{
			return;
		}

		if (!CanBite(target)) return;
		
		_timeLastBite = Time.time;
		var targetLm = target.GetComponent<LiveMixin>();
		if (targetLm != null && targetLm.IsAlive())
		{
			if (_lastBiteIsLower)
				Invoke(nameof(DelayedDamagePlayer), 0.84f);
			else
				targetLm.TakeDamage(GetBiteDamage(target), default, DamageType.Normal, gameObject);
			targetLm.NotifyCreatureDeathsOfCreatureAttack();
		}
		
		var position = collider.ClosestPointOnBounds((lower ? lowerTrigger : mouth).transform.position);
		if (damageFX != null)
		{
			Instantiate(damageFX, position, damageFX.transform.rotation);
		}
		
		biteSoundEmitter.Play();
		creature.Aggression.Add(0f - biteAggressionDecrement);
		gameObject.SendMessage("OnMeleeAttack", target, SendMessageOptions.DontRequireReceiver);
	}

	private void DelayedDamagePlayer()
	{
		Player.main.liveMixin.TakeDamage(GetBiteDamage(Player.main.gameObject), default, DamageType.Normal, gameObject);
	}
	
	private bool CanEat(BehaviourType behaviourType, bool holdingByPlayer = false)
	{
		if (behaviourType != BehaviourType.Shark && behaviourType != BehaviourType.MediumFish)
		{
			return behaviourType == BehaviourType.SmallFish;
		}
		return true;
	}

	private bool TryEat(GameObject preyGameObject, bool holdingByPlayer = false)
	{
		BehaviourType behaviourType = CreatureData.GetBehaviourType(preyGameObject);
		
		if (!CanEat(behaviourType, holdingByPlayer)) return false;
		
		SendMessage("OnFishEat", preyGameObject, SendMessageOptions.DontRequireReceiver);
		Peeper component = preyGameObject.GetComponent<Peeper>();
		if (component != null && component.isHero)
		{
			InfectedMixin component2 = GetComponent<InfectedMixin>();
			if (component2 != null)
			{
				component2.Heal(0.5f);
			}
		}
		
		LiveMixin component3 = preyGameObject.GetComponent<LiveMixin>();
		if (component3 != null && component3.IsAlive())
		{
			component3.Kill();
		}
		
		if (preyGameObject.GetComponent<Creature>() != null)
		{
			Destroy(preyGameObject);
		}
		
		return true;
	}

	public void ManagedUpdate()
	{
		var isBiting = Time.time - _timeLastBite < 0.2f;
		if (isBiting != _wasBiting || !_initBiting)
		{
			animator.SetBool(BiteAnimID, isBiting && !_lastBiteIsLower);
			animator.SetBool(LowerBiteAnimID, isBiting && _lastBiteIsLower);
		}
		_wasBiting = isBiting;
		_initBiting = true;
	}

	public void OnFreezeByStasisSphere()
	{
		_frozen = true;
	}

	public void OnUnfreezeByStasisSphere()
	{
		_frozen = false;
	}

	private void OnKill()
	{
		enabled = false;
	}
}