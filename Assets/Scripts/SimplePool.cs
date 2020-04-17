using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    public List<PoolObject> objectsToPool = new List<PoolObject>();
    public static SimplePool instance;
    private Dictionary<string, List<GameObject>> objectPool = new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        if(SimplePool.instance != null) Debug.LogError("Multiple ParticlePools present in the scene!!");
        SimplePool.instance = this;
    }

    void Start()
    {
        foreach (var poolObj in objectsToPool)
        {
            for (int i = 0; i < poolObj.count; i++)
            {
                CreateGameObject(poolObj.poolObject, poolObj.name);
            }
        }
        

        StartCoroutine(ClearSceneGraph());
    }

    public GameObject GetObject(string objName)
    {
        foreach (var go in objectPool[objName])
        {
            if (!go.activeInHierarchy)
            {
                go.SetActive(true);
                go.transform.parent = null;
                return go;
            }
        }

        return CreateGameObject(objectPool[objName][0], objName);
    }

    GameObject CreateGameObject(GameObject objToClone, string objName)
    {
        GameObject go = GameObject.Instantiate(objToClone);
        go.SetActive(false);
        go.transform.parent = transform;
        if(!objectPool.ContainsKey(objName)) objectPool.Add(objName, new List<GameObject>(){go});
        else objectPool[objName].Add(go);
        return go;
    }

    IEnumerator ClearSceneGraph()
    {
        yield return new WaitForSeconds(1.0f);
        while (true)
        {
            foreach (var pool in objectPool)
            {
                foreach (var poolObj in pool.Value)
                {
                    if (!poolObj.activeInHierarchy)
                    {
                        poolObj.transform.parent = transform;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
