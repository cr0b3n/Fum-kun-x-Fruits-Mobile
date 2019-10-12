using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TimeDisabler : MonoBehaviour
{
    public float lifeSpan = 2.0f;
    //public GameObject disableTarget;

    float disableTimer = 0f;
    bool hasDisabled;

    private void OnEnable() {
        hasDisabled = false;
        disableTimer = Time.time + lifeSpan;
    }

    private void Update() {
        if (disableTimer <= Time.time && !hasDisabled) {

            hasDisabled = true;

            //if (disableTarget == null)
            gameObject.SetActive(false);
            //else
            //    OnTimerExpire();
        }
    }

    private void OnBecameInvisible() {
        gameObject.SetActive(false);
    }

    //private void OnTimerExpire() {
    //     disableTarget.SetActive(false);
    // }
}
