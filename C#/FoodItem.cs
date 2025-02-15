using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Item", menuName = "Food/Food Item")]
public class FoodItem : ScriptableObject
{
    public string foodName;
    public string foodDescription;
    [Range(0f, 100f)]
    public float hungerRestoration;
    [Range(-100f, 100f)]
    public float sicknessRestoration;
}
