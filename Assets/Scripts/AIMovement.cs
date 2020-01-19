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
    private bool mHitBoundary;
    private Vector3 mLastPosition;
    private Vector3 mVelocity;
    private float mSpeed;
    private float mChaseTimer;
    private float mBoundaryTimer;
    private GameObject mTarget;

    private Material mMaterial;

    private const float mEpsilon = 0.0001f;
    
    // Start is called before the first frame update
    void Start()
    {
        mTarget = null;
        mIsMoving = false;
        mBoundaryTimer = 0.0f;
        mLastPosition = transform.position;
        mMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        if(Mathf.Abs(position.x) > 17.5f) {
            position.x += position.x > 0 ? -0.5f : 0.5f;
            position.x *= -1;
            if(mIsTarget) {
                mHitBoundary = true;
                mBoundaryTimer = 4.0f;
            }
        }
        else if(Mathf.Abs(position.z) > 17.5f) {
            position.z += position.z > 0 ? -0.5f : 0.5f;
            position.z *= -1;
            if(mIsTarget) {
                mHitBoundary = true;
                mBoundaryTimer = 4.0f;
            }
        }

        transform.position = position;

        if(mIsTagged) {
            mSpeed = chaseSpeed;
            if(!mTarget) {
                // find a target
                FindClosest();
                mChaseTimer = 3.0f;
            }
            else {
                mChaseTimer -= Time.deltaTime;
                if(mChaseTimer < 0.0f) {
                    FindClosest();
                    mChaseTimer = 3.0f;
                }

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
                        // TODO: refactor this to a Freeze() method
                        AIMovement ai = mTarget.GetComponent<AIMovement>();
                        ai.SetFrozen(true);
                        ai.SetTarget(false);
                        ai.SetMaterial(Color.cyan);
                        mTarget.tag = "Frozen";
                        mTarget = null;
                        mIsMoving = false;
                    }
                    else {
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
        }
        else {
            if(!mIsFrozen) {
                mSpeed = baseSpeed;
                if(mIsTarget) {

                    // recently transitioned to other side of level, keep moving forward so it doesn't bounce back accross the boundary
                    if(mHitBoundary) {
                        mBoundaryTimer -= Time.deltaTime;
                        if(mBoundaryTimer < 0.0f)
                            mHitBoundary = false;
                        transform.position += transform.forward * mSpeed * Time.deltaTime;
                    }
                    // flee from pursuer
                    mTarget = GameObject.FindGameObjectWithTag("Tagged");
                    Vector3 targetDirection = transform.position - mTarget.transform.position;
                    if(transform.forward != targetDirection.normalized)
                        mIsMoving = false;

                    if(!mIsMoving) {
                        if(targetDirection.magnitude < mEpsilon) {
                            transform.position += targetDirection.normalized / 10.0f;
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
                        //targetDirection /= timeToTarget;
                        if(targetDirection.magnitude > mSpeed) {
                            targetDirection = targetDirection.normalized * mSpeed;
                        }
                        float step = rotationSpeed * Time.deltaTime; ;

                        Quaternion rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

                        transform.position += targetDirection * Time.deltaTime;
                    }
                }
                else {
                    // wander
                    transform.position += transform.forward * mSpeed * Time.deltaTime;

                    float angle = (Random.Range(-1.0f, 1.0f) - Random.Range(-1.0f, 1.0f));
                    float step = angle * rotationSpeed * Time.deltaTime;
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);
                }
            }
            else {
                // frozen, do nothing
            }
        }
        
        /*
        angle += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.position += transform.forward * speed * Time.deltaTime;
        */
    }

    void FindClosest() {
        if(mTarget)
            mTarget.GetComponent<AIMovement>().SetTarget(false);

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
        if(closestTarget) {
            mTarget = closestTarget;
            mTarget.GetComponent<AIMovement>().SetTarget(true);
        }
    }

    public void SetTagged(bool status) {
        mIsTagged = status;
    }

    public void SetFrozen(bool status) {
        mIsFrozen = status;
    }

    public void SetTarget(bool status) {
        mIsTarget = status;
    }

    public bool IsTarget() {
        return mIsTarget;
    }

    public void SetHitBoundary(bool status) {
        mHitBoundary = status;
    }

    public void SetBoundaryTimer() {
        mBoundaryTimer = 2.0f;
    }

    public void SetMaterial(Color c) {
        mMaterial.color = c;
    }

    void OnDestroy() {
        Destroy(mMaterial);
    }
}
