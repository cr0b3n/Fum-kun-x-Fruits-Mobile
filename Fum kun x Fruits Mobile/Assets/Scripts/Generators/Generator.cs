using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public float minYPosition;
    public float maxYPosition;
    public float generationDelay;
    [Range(0.0f, 1.0f)]
    public float generationRate = .5f;
    public ObjectPooler[] fruitPools;

    protected float spawnRate;
    //public float crazyTime = 2f;
    //GameManager gameManager;
    protected bool isPlaying;
    //bool isCrazy = false;

    protected virtual void Start() {

        spawnRate = generationRate;
        isPlaying = false;
        GameManager.instance.OnGameStart += GameStart;
        InvokeRepeating("GenerateObject", generationDelay, generationDelay);
    }

    protected virtual void OnDestroy() {
        GameManager.instance.OnGameStart -= GameStart;
    }
    
    //private void Update() {

    //    //if (!isPlaying && gameManager.IsPlaying) {
    //    //    isPlaying = true;
    //    //    InvokeRepeating("GenerateObject", generationDelay, generationDelay);
    //    //}

    //    //if (!isCrazy && gameManager.GoCrazy) {
    //    //    generationDelay /= crazyTime;
    //    //    isCrazy = true;
    //    //    CancelInvoke();
    //    //    InvokeRepeating("GenerateObject", generationDelay, generationDelay);
    //    //}

    //}

    protected virtual void GenerateObject() {

        if (Random.value >= spawnRate || !isPlaying)
            return;

       fruitPools[Random.Range(0, fruitPools.Length)].GetPooledObject(
           new Vector3(transform.position.x, Random.Range(minYPosition, maxYPosition)),
           Quaternion.identity);
    }

    protected virtual void GameStart() {
        isPlaying = true;
    } 
}
