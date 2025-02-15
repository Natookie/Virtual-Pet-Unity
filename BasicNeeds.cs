using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasicNeeds : MonoBehaviour
{
    [Header("NEEDS LEVEL")]
    public float hungerLevel = 100f;
    public float cleanlinessLevel = 100f;
    public float energyLevel = 100f;
    public float boredomLevel = 100f;

    public float sicknessLevel = 0f; //Higher => sicker

    [Header("NEEDS DECAY RATE")]
    public float hungerDecayRate = 1f;
    public float cleanlinessDecayRate = 1f;
    public float energyDecayRate = 1f;
    public float boredomDecayRate = 1f;

    public float sicknessIncreaseRate = 1f;

    [Header("UI REFERENCES")]
    public Image hungerFill;
    public Image cleanlinessFill;
    public Image energyFill;
    public Image boredomFill;

    public Image sicknessFill;
    [Space(10)]
    public TextMeshProUGUI hungerValue;
    public TextMeshProUGUI cleanlinessValue;
    public TextMeshProUGUI energyValue;
    public TextMeshProUGUI boredomValue;

    void Update(){
        HandleNeeds();
        HandleUI();
    }

    void HandleNeeds(){
        //Decay every sec
        if(hungerLevel > 0f) hungerLevel -= hungerDecayRate * Time.deltaTime;
        if(cleanlinessLevel > 0f) cleanlinessLevel -= cleanlinessDecayRate * Time.deltaTime;
        if(energyLevel > 0f) energyLevel -= energyDecayRate * Time.deltaTime;
        if(boredomLevel > 0f) boredomLevel -= boredomDecayRate * Time.deltaTime;

        //Handle sickness, check thresholds
        float cleanlinessThreshold = 30f;
        float energyThreshold = 30f;
        if(cleanlinessLevel <= cleanlinessThreshold || energyLevel <= energyThreshold){
            sicknessLevel += sicknessIncreaseRate * Time.deltaTime;
        }

        HandleClamping();
    }

    //Kalo mau lebih rapih, pindahin HandleUI ke GameManager.cs
    void HandleUI(){
        //Handle Fill
        hungerFill.fillAmount = hungerLevel / 100f;
        cleanlinessFill.fillAmount = cleanlinessLevel / 100f;
        energyFill.fillAmount = energyLevel / 100f;
        boredomFill.fillAmount = boredomLevel / 100f;

        //sicknessFill.fillAmount = sicknessLevel / 100f;

        //Handle Text
        hungerValue.text = $"Hunger: {hungerLevel:F2}";
        cleanlinessValue.text = $"Cleanliness: {cleanlinessLevel:F2}";
        energyValue.text = $"Energy: {energyLevel:F2}";
        boredomValue.text = $"Boredom: {boredomLevel:F2}";

        //sicknessValue.text = $"Sickness: {sicknessLevel:F2}";
    }

    void HandleClamping(){
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, 100f);
        cleanlinessLevel = Mathf.Clamp(cleanlinessLevel, 0f, 100f);
        energyLevel = Mathf.Clamp(energyLevel, 0f, 100f);
        boredomLevel = Mathf.Clamp(boredomLevel, 0f, 100f);
        sicknessLevel = Mathf.Clamp(sicknessLevel, 0f, 100f);
    }

    #region INTERACTIONS
    public void FeedPet(FoodItem food){
        hungerLevel += food.hungerRestoration;
        sicknessLevel += food.sicknessRestoration;
    }

    public void CleanPet(){
        cleanlinessLevel += 20f;
        sicknessLevel -= 10f;
    }

    public void PlayPet(){
        boredomLevel += 20f;
    }

    public void SleepPet(){
        energyLevel += 20f;
        sicknessLevel -= 10f;
    }
    #endregion
}
