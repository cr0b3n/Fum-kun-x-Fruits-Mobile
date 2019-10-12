using UnityEngine;

[DisallowMultipleComponent]
public class Boss : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;    
    public Sprite originalSprite;
    public Sprite damageSprite;

    bool hasChangeSprite;

    private void OnEnable() {
        hasChangeSprite = false;
        spriteRenderer.sprite = originalSprite;
        //tears.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (hasChangeSprite) return;

        hasChangeSprite = true;
        spriteRenderer.sprite = damageSprite;
        //tears.SetActive(true);
    }
}
