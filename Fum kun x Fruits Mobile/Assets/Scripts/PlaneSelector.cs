using UnityEngine;

[DisallowMultipleComponent]
public class PlaneSelector : MonoBehaviour
{

    public int price;
    public int index;

    public void TryPlane() {
        Player.instance.SetPlaneDetails(index);
        ShopManager.instance.SetPlanePrice(price, transform, index);
        GameManager.instance.PlayButtonSound();
    }
}
