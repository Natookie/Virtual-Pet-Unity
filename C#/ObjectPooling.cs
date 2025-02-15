using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance { get; private set; }

    public enum PoolType{
        Default,
        Food
    }

    [System.Serializable]
    public class Pool{
        public string token;
        public GameObject prefab;
        public int size;
        public Transform parentTransformation;

        public PoolType poolType;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start(){
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools){
            Queue<GameObject> objectPool = new Queue<GameObject>(); 

            for(int i = 0; i < pool.size; i++){
                GameObject obj = Instantiate(pool.prefab, pool.parentTransformation);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.token, objectPool);
        }
    }

    public GameObject SpawnFromPool(string token, Vector3 position, Quaternion rotation){
        if(!poolDictionary.ContainsKey(token)){
            Debug.LogWarning($"Token: {token} null");
            return null;
        }

        GameObject obj = poolDictionary[token].Dequeue();

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        poolDictionary[token].Enqueue(obj);
        return obj;
    }

    public List<string> GetFoodTokens(){
        List<string> foodTokens = new List<string>();

        foreach(var pool in pools){
            if(pool.poolType == PoolType.Food){
                foodTokens.Add(pool.token);
            }
        }

        return foodTokens;
    }

}
