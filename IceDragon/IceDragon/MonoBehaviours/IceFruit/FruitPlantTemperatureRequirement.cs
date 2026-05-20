using UnityEngine;

namespace IceDragon.MonoBehaviours.IceFruit;

public class FruitPlantTemperatureRequirement :  HandTarget, IHandTarget
{
    private const int maxFruitGrowthTemperature = 10;
    private bool tooHotForGrowth = false;
    private float latestCheckedTemp;
    private FruitPlant fruitPlant;

    private void Start()
    {
        fruitPlant = GetComponent<FruitPlant>();
        InvokeRepeating(nameof(WaterTemperatureCheck), 0, 10);
    }
    
    public void WaterTemperatureCheck()
    {
        float temperature = WaterTemperatureSimulation.main.GetTemperature(transform.position);
        bool enableFruitSpawn = temperature < maxFruitGrowthTemperature;
        if (fruitPlant.fruitSpawnEnabled != enableFruitSpawn)
        {
            //changed, reset growth times so it doesn't pop in a bunch of fruit
            fruitPlant.timeNextFruit = DayNightCycle.main.timePassedAsFloat + fruitPlant.fruitSpawnInterval;;
        }
        if (!fruitPlant.initialized && enableFruitSpawn)
        {
            //The plant must be initialized before fruit can spawn. this can happen if it loaded into a save uninitialized
            fruitPlant.Initialize();
        }
        
        fruitPlant.fruitSpawnEnabled = enableFruitSpawn;
        tooHotForGrowth = !enableFruitSpawn;
        latestCheckedTemp = temperature;
    }
    public void OnHandHover(GUIHand hand)
    { 
        if (!tooHotForGrowth) return;
        
        string hoverText = Language.main.Get("IceFruitTreeTooHot").Replace("%temp%", latestCheckedTemp.ToString("F0"));
        HandReticle.main.SetText(HandReticle.TextType.Hand, hoverText, false);
        HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1.5f);
    }
    
    public void OnHandClick(GUIHand hand) { }
}
