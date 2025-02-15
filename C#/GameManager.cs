using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    ObjectPooling OP = null;
    void Start(){
        OP = ObjectPooling.Instance;
    }

    //Debug function
    public void GenerateFood(){
        Debug.Log("Generating food...");
        List<string> foodTokens = OP.GetFoodTokens();

        if(foodTokens.Count == 0) return;

        string rngToken = foodTokens[Random.Range(0, foodTokens.Count)];
        GameObject food = OP.SpawnFromPool(rngToken, Vector3.zero, Quaternion.identity);

        Debug.Log($"Generated food: {food}");
    }
}
