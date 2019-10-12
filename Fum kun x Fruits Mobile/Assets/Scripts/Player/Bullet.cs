using UnityEngine;

[DisallowMultipleComponent]
public class Bullet : MovingObject, IDeath
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;

    public void OnDeath() {
        GameManager.instance.ShowDestroyEffect(transform.position, true);
        gameObject.SetActive(false);
    }

    private void OnEnable() {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
    }

    //private void OnTriggerEnter2D(Collider2D collision) {
    //    Debug.Log(collision.name);
    //}
}
