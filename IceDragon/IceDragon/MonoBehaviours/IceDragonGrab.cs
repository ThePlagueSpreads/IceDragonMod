using UnityEngine;

namespace IceDragon.MonoBehaviours;

public class IceDragonGrab : MonoBehaviour
{
	public Creature creature;
    public Transform seamothAttachPoint;
    public FMOD_CustomLoopingEmitter grabSound;
    public float damagePerSecond = 17;
    public float duration = 9.33f;

    private Vehicle _holdingVehicle;
    private VehicleType _holdingVehicleType;
    private float _timeVehicleGrabbed;
    private float _timeVehicleReleased;
    private Quaternion _vehicleInitialRotation;
    private Vector3 _vehicleInitialPosition;
    
    private static readonly int GrabSmall = Animator.StringToHash("reaper_small");
    private static readonly int GrabLarge = Animator.StringToHash("reaper_large");
    
    private enum VehicleType
    {
        None,
        Seamoth,
        Exosuit,
        Modded
    }
    
    public void GrabModdedVehicle(Vehicle vehicle)
    {
	    GrabVehicle(vehicle, VehicleType.Modded);
    }
	
    public void GrabSeamoth(SeaMoth seamoth)
    {
	    GrabVehicle(seamoth, VehicleType.Seamoth);
    }

    public void GrabExosuit(Exosuit exosuit)
    {
	    GrabVehicle(exosuit, VehicleType.Exosuit);
    }

	private void GrabVehicle(Vehicle vehicle, VehicleType type)
	{
		vehicle.SetPhysicsGrabbed(true);
		_holdingVehicle = vehicle;
		_holdingVehicleType = type;
		if (_holdingVehicleType == VehicleType.Exosuit)
		{
			SafeAnimator.SetBool(_holdingVehicle.mainAnimator, "reaper_attack", true);
			var exosuit = _holdingVehicle.GetComponent<Exosuit>();
			if (exosuit != null)
			{
				exosuit.cinematicMode = true;
			}
		}
		creature.Aggression.Value = 0f;
		_timeVehicleGrabbed = Time.time;
		_vehicleInitialRotation = vehicle.transform.rotation;
		_vehicleInitialPosition = vehicle.transform.position;
		grabSound.Play();
		InvokeRepeating(nameof(DamageVehicle), 1f, 1f);
		Invoke(nameof(ReleaseVehicle), duration);
	}

	public bool GetCanGrabVehicle()
	{
		if (_timeVehicleReleased + 10f < Time.time)
		{
			return !IsHoldingVehicle();
		}
		return false;
	}

	public void ReleaseVehicle()
	{
		if (_holdingVehicle != null)
		{
			if (_holdingVehicleType == VehicleType.Exosuit)
			{
				SafeAnimator.SetBool(_holdingVehicle.mainAnimator, "reaper_attack", false);
				var exosuit = _holdingVehicle.GetComponent<Exosuit>();
				if (exosuit != null)
				{
					exosuit.cinematicMode = false;
				}
			}
			_holdingVehicle.SetPhysicsGrabbed(false);
			_holdingVehicle = null;
			_timeVehicleReleased = Time.time;
		}
		_holdingVehicleType = VehicleType.None;
		CancelInvoke(nameof(DamageVehicle));
		grabSound.Stop();
	}

	private void DamageVehicle()
	{
		if (_holdingVehicle != null)
		{
			_holdingVehicle.liveMixin.TakeDamage(damagePerSecond);
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (damageInfo.type is DamageType.Electrical or DamageType.Poison && _holdingVehicle != null)
		{
			ReleaseVehicle();
		}
	}

	public void Update()
	{
		if (_holdingVehicleType != 0 && _holdingVehicle == null)
		{
			ReleaseVehicle();
		}

		creature.GetAnimator().SetBool(GrabSmall, IsHoldingSeamoth());
		creature.GetAnimator().SetBool(GrabLarge, IsHoldingLargeVehicle());

		if (_holdingVehicle == null) return;
		
		var transitionPercent = Mathf.Clamp01(Time.time - _timeVehicleGrabbed);
		if (transitionPercent >= 1f)
		{
			_holdingVehicle.transform.position = seamothAttachPoint.position;
			_holdingVehicle.transform.rotation = seamothAttachPoint.transform.rotation;
		}
		else
		{
			_holdingVehicle.transform.position = (seamothAttachPoint.position - _vehicleInitialPosition) * transitionPercent + _vehicleInitialPosition;
			_holdingVehicle.transform.rotation = Quaternion.Lerp(_vehicleInitialRotation, seamothAttachPoint.transform.rotation, transitionPercent);
		}
	}

	private void OnDisable()
	{
		if (_holdingVehicle != null)
		{
			ReleaseVehicle();
		}
	}
	
	public bool IsHoldingVehicle()
	{
		return _holdingVehicle != null;
	}

	public bool IsHoldingSmallVehicle()
	{
		return IsHoldingSeamoth();
	}
	
	public bool IsHoldingLargeVehicle()
	{
		if (_holdingVehicleType is VehicleType.Exosuit or VehicleType.Modded)
		{
			return _holdingVehicle != null;
		}
		return false;
	}

	public bool IsHoldingSeamoth()
	{
		if (_holdingVehicleType == VehicleType.Seamoth)
		{
			return _holdingVehicle != null;
		}
		return false;
	}

	public bool IsHoldingExosuit()
	{
		if (_holdingVehicleType == VehicleType.Exosuit)
		{
			return _holdingVehicle != null;
		}
		return false;
	}
}