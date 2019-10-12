using UnityEngine;

[DisallowMultipleComponent]
public class Coin : MovingObject, IDeath
{
    protected override void Update() {
        MoveObject(-minSpeed);
    }

    public void OnDeath() {
        GameManager.instance.CoinPickedUp(transform.position);
        gameObject.SetActive(false);
    }
}
