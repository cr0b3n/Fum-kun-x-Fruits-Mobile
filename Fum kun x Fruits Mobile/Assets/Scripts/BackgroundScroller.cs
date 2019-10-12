using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float speed = 5.0f;

    public float backgroundWidth;

    private float maxPosition;

    //Vector3 moveTowards; do not use will cause uneven speed!!!

    private void Start() {
        //moveTowards = new Vector3(-speed * Time.deltaTime, 0, 0);
        maxPosition = -backgroundWidth * 1.5f;
    }

    private void Update() {
        //transform.position += moveTowards; DO NOT USE WILL CAUSE UNEVEN SPEED

        transform.Translate(new Vector3(-speed * Time.deltaTime, 0));

        if (transform.position.x < maxPosition)
            RepositionBackground();
    }

    private void RepositionBackground() {

        Vector2 groundOffset = new Vector2(backgroundWidth * 3f, 0);
        transform.position = (Vector2)transform.position + groundOffset;
    }
}
