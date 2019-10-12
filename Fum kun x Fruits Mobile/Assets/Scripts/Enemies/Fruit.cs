using UnityEngine;

[DisallowMultipleComponent]
public class Fruit : MovingObject, IDeath
{
    [Range(0.0f, 20.0f)]
    public float maxSpeed;
    [Range(0, 10)]
    public int bonusPoints = 1;

    private float speed;

    private void OnEnable() {
        speed = Random.Range(minSpeed, maxSpeed);
    }
    
    protected override void Update() {
        MoveObject(-speed);
    }

    public void OnDeath() {

        int points = bonusPoints + (int)(speed / 2);

        GameManager.instance.SetScore(points, transform.position);
        gameObject.SetActive(false);       
    }
}
