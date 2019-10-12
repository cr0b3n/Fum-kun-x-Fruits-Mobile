using UnityEngine;

[DisallowMultipleComponent]
public class HitPoint : MonoBehaviour
{
    [Range(0, 20)]
    public int hitPoints = 1;
    //[Range(0.0f, 5.0f)]
    //public float damageRate = 0f;

    //float timer;
    //bool canTakeDamage;
    protected int hp;

    protected virtual void OnEnable() {
        hp = hitPoints;
        //timer = damageRate;
        //canTakeDamage = true;
    }

    //private void Update() {
    //    if (!canTakeDamage)
    //        canTakeDamage = CheckIfCountDownElapsed(damageRate);
    //}

    protected virtual void OnTriggerEnter2D(Collider2D collision) {

        //if (!canTakeDamage) return;

        //canTakeDamage = false;
        //timer = 0f;
        hp--;

        if (hp > 0) return;
        
        IDeath death = GetComponent<IDeath>();

        if (death != null) death.OnDeath();
    }

    //private bool CheckIfCountDownElapsed(float duration) {
    //    timer += Time.deltaTime;
    //    return timer >= duration;
    //}
}
