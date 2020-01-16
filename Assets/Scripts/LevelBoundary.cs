using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        GameObject other = collision.gameObject;
        AIMovement ai = other.GetComponent<AIMovement>();

        if(ai.IsTarget()) {
            ai.SetHitBoundary(true);
            ai.SetBoundaryTimer();
        }

        Vector3 position = other.transform.position;
        if(Mathf.Abs(position.x) > 17.5f) {
            position.x += position.x > 0 ? -0.5f : 0.5f;
            position.x *= -1;
        }
        else if(Mathf.Abs(position.z) > 17.5f) {
            position.z += position.z > 0 ? -0.5f : 0.5f;
            position.z *= -1;
        }

        other.transform.position = position;
    }
}
