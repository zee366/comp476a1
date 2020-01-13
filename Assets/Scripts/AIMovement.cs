using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    [SerializeField]
    float baseSpeed;
    [SerializeField]
    float chaseSpeed;
    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float radius;
    [SerializeField]
    float timeToTarget;

    private bool mIsMoving;
    private bool mIsTarget;
    private bool mHasTarget;
    private bool mIsTagged;
    private bool mIsFrozen;
    private Vector3 mLastPosition;
    private Vector3 mVelocity;
    private float mSpeed;
    private GameObject mTarget;
    private const float mEpsilon = 0.0001f;
    
    // Start is called before the first frame update
    void Start()
    {
        mTarget = null;
        mIsMoving = false;
        mLastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(mIsTagged) {
            mSpeed = chaseSpeed;
            if(!mTarget) {
                // find a target
                GameObject[] targets = GameObject.FindGameObjectsWithTag("Free");

                GameObject closestTarget = null;
                float distanceToTarget = Mathf.Infinity;
                for(int i = 0; i < targets.Length; i++) {
                    Vector3 targetDirection = targets[i].transform.position - transform.position;
                    if(targetDirection.sqrMagnitude < distanceToTarget) {
                        closestTarget = targets[i];
                        distanceToTarget = targetDirection.sqrMagnitude;
                    }
                }
                mTarget = closestTarget;
            }
            else {
                float deltaPositionLength = (transform.position - mLastPosition).magnitude;
                Vector3 targetDirection = mTarget.transform.position - transform.position;

                if(!mIsMoving) {
                    if(targetDirection.magnitude < mEpsilon) {
                        transform.position = mTarget.transform.position;
                    }
                    else {
                        float step = rotationSpeed * Time.deltaTime; ;

                        Quaternion rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

                        if(transform.forward == targetDirection.normalized)
                            mIsMoving = true;
                    }
                }
                else {
                    if(targetDirection.magnitude < radius) {
                        // reached the target
                        mTarget.GetComponent<AIMovement>().SetFrozen(true);
                        mTarget.tag = "Frozen";
                    }
                    targetDirection /= timeToTarget;
                    if(targetDirection.magnitude > mSpeed) {
                        targetDirection = targetDirection.normalized * mSpeed;
                    }
                    float step = rotationSpeed * Time.deltaTime; ;

                    Quaternion rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

                    transform.position += targetDirection * Time.deltaTime;
                }
            }
        }
        else {
            mSpeed = baseSpeed;
        }
        
        /*
        angle += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.position += transform.forward * speed * Time.deltaTime;
        */
    }

    public void SetFrozen(bool status) {
        mIsFrozen = status;
    }

    public void SetTarget(bool status) {
        mIsTarget = status;
    }
}
