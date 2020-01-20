using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMover : MonoBehaviour
{
    GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
