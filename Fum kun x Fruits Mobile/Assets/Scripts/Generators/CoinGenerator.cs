using UnityEngine;

[DisallowMultipleComponent]
public class CoinGenerator : MonoBehaviour {

    [Range(-5.0f, 5.0f)]
    public float minYPosition;
    [Range(-5.0f, 5.0f)]
    public float maxYPosition;
    [Range(0.0f, 1.0f)]
    public float rateMultiplier = 0.05f;
    public ObjectPooler coinPool;

    private float generationRate = 0f;

    private void Start() {
        GameManager.instance.OnLevelUp += TryToGenerateCoin;
    }

    private void OnDestroy() {
        GameManager.instance.OnLevelUp -= TryToGenerateCoin;
    }

    private void TryToGenerateCoin() {

        generationRate += rateMultiplier;

        if (Random.value >= generationRate)
            return;

        coinPool.GetPooledObject(
            new Vector3(transform.position.x, Random.Range(minYPosition, maxYPosition)),
            Quaternion.identity);
    }
}
