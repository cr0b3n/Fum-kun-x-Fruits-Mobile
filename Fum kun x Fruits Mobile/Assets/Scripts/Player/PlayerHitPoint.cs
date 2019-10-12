using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHitPoint : HitPoint {

    public GameObject shield;
    [Range(0.0f, 5.0f)]
    public float damageRate = 3f;

    float timer;
    bool canTakeDamage;

    protected override void OnEnable() {
        base.OnEnable();
        timer = damageRate;
        canTakeDamage = true;
    }

    private void Update() {

        if (!canTakeDamage) {

            canTakeDamage = CheckIfCountDownElapsed(damageRate);

            if(canTakeDamage)
             shield.SetActive(false);
        }         
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        
        if (collision.CompareTag("Coin")) return;
        if (!canTakeDamage) return;

        canTakeDamage = false;
        timer = 0f;
        base.OnTriggerEnter2D(collision);
    }

    public void OnRiveve() {
        canTakeDamage = false;
        timer = 0f;
        shield.SetActive(true);
    }

    private bool CheckIfCountDownElapsed(float duration) {
        timer += Time.deltaTime;
        return timer >= duration;
    }
}
