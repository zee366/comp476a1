using UnityEngine;

public class LightMover : MonoBehaviour
{
    GameObject target;

    // have the spot light stay directly above the currently tagged character
    void Update()
    {
        target = GameObject.FindGameObjectWithTag("Tagged");
        float x = target.transform.position.x;
        float y = transform.position.y;
        float z = target.transform.position.z;
        Vector3 position = new Vector3(x, y, z);
        transform.position = position;
    }
}
