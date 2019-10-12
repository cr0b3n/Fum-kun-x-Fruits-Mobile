using UnityEngine;

[DisallowMultipleComponent]
public class ChangePlayerPlane : MonoBehaviour
{
    private int index = 0;

    private void Update() {
        if(Input.GetKeyDown(KeyCode.F)) {
            index++;

            if (index > 2)
                index = 0;

            Player.instance.SetPlaneDetails(index);
        }
    }
}
