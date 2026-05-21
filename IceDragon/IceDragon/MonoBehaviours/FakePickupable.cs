namespace IceDragon.MonoBehaviours;

public class FakePickupable : HandTarget, IHandTarget
{
	private bool _pickedUp;

	private TechType _techType;

	public TechType overrideTechType;

	private TechType GetTechType()
    {
	    if (overrideTechType != TechType.None)
	    {
		    return overrideTechType;
	    }
	    
	    if (_techType == TechType.None)
	    {
		    _techType = CraftData.GetTechType(gameObject);
	    }

	    return _techType;
    }
    
    public void OnHandClick(GUIHand hand)
    {
	    if (_pickedUp)
		    return;
	    
		if (!hand.IsFreeToInteract())
		{
			return;
		}
		if (!Inventory.main.HasRoomFor(GetTechType()))
		{
			ErrorMessage.AddWarning(Language.main.Get("InventoryFull"));
			return;
		}
		Player.main.PlayGrab();
		PickUp();
	}

	private void PickUp()
	{
		_pickedUp = true;
		CraftData.AddToInventory(GetTechType());
		Destroy(gameObject);
	}

	public void OnHandHover(GUIHand hand)
	{
		var main = HandReticle.main;
		if (!hand.IsFreeToInteract())
		{
			return;
		}

		var techType = GetTechType();
		if (Inventory.main.HasRoomFor(techType))
		{
			string mainText = string.Empty;
			Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
			bool canPickUp = exosuit == null || exosuit.HasClaw();
			if (canPickUp)
			{
				mainText = LanguageCache.GetPickupText(techType);
				main.SetIcon(HandReticle.IconType.Hand);
			}
			if ((bool)exosuit)
			{
				GameInput.Button button = (canPickUp ? GameInput.Button.LeftHand : GameInput.Button.None);
				if (exosuit.leftArmType != TechType.ExosuitClawArmModule)
				{
					button = GameInput.Button.RightHand;
				}
				HandReticle.main.SetText(HandReticle.TextType.Hand, mainText, translate: false, button);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, mainText, translate: false, GameInput.Button.LeftHand);
			}
		}
		else if (!Inventory.main.HasRoomFor(techType))
		{
			main.SetText(HandReticle.TextType.Hand, techType.AsString(), translate: true);
			main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", translate: true);
		}
		else
		{
			main.SetText(HandReticle.TextType.Hand, techType.AsString(), translate: true);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}
}