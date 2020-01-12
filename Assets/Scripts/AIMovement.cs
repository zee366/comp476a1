using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    [SerializeField]
    float speed = 5;
    [SerializeField]
    float rotationSpeed = 1;

    Vector3 target;
    float angle;

    private bool mIsMoving;
    private Vector3 mLastPosition;
    private const float mEpsilon = 0.0001f;
    
    // Start is called before the first frame update
    void Start()
    {
        mIsMoving = false;
        mLastPosition = transform.position;
        target = GameObject.Find("Target").GetComponent<Transform>().position;
    }

    // Update is called once per frame
    void Update()
    {
        float deltaPositionLength = (transform.position - mLastPosition).magnitude;
        if(!mIsMoving) {
            Vector3 targetDirection = target - transform.position;

            if(targetDirection.magnitude < mEpsilon) {
                transform.position = target;
            }
            else {
                float step = rotationSpeed * Time.deltaTime;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection.normalized, step, 0.0f);
                Debug.Log(newDirection);
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }
        /*
        angle += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.position += transform.forward * speed * Time.deltaTime;
        */
    }
}
