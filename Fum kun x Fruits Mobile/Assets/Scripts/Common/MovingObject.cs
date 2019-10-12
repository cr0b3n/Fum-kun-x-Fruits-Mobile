using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    [Range(0.0f, 20.0f)]
    public float minSpeed = 5.0f;
   
    protected virtual void Update() {
        MoveObject(minSpeed);
    }

    protected virtual void MoveObject(float objectSpeed) {
        transform.Translate(new Vector3(objectSpeed * Time.deltaTime, 0));
    }
}
