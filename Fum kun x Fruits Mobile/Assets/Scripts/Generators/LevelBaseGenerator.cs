using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBaseGenerator : Generator
{
    [Range(0.0f, 1.0f)]
    public float rateMultiplier = 0.1f;

    protected override void Start() {
        GameManager.instance.OnLevelUp += IncreaseGenerationRate;
        base.Start();
    }

    private void IncreaseGenerationRate() {
        spawnRate += (spawnRate * rateMultiplier);
        //Debug.Log("new spawn rate: " + spawnRate);
    }

    protected override void OnDestroy() {
        GameManager.instance.OnLevelUp -= IncreaseGenerationRate;
        base.OnDestroy();        
    }
}
