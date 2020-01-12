using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        GameObject other = collision.gameObject;
        Vector3 position = other.transform.position;
        if(Mathf.Abs(position.x) > 17.5f)
            position.x *= -1;
        else if(Mathf.Abs(position.z) > 17.5f)
            position.z *= -1;

        other.transform.position = position;
    }
}
