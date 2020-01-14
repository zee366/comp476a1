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

    private Material mMaterial;

    private const float mEpsilon = 0.0001f;
    
    // Start is called before the first frame update
    void Start()
    {
        mTarget = null;
        mIsMoving = false;
        mLastPosition = transform.position;
        mMaterial = GetComponent<Renderer>().material;
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
                if(closestTarget) {
                    mTarget = closestTarget;
                    mTarget.GetComponent<AIMovement>().SetTarget(true);
                }
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
                        // TODO: refactor this to a Freeze() method
                        AIMovement ai = mTarget.GetComponent<AIMovement>();
                        ai.SetFrozen(true);
                        ai.SetTarget(false);
                        ai.SetMaterial(Color.cyan);
                        mTarget.tag = "Frozen";
                        mTarget = null;
                        mIsMoving = false;
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
            if(!mIsFrozen) {
                mSpeed = baseSpeed;
                if(mIsTarget) {
                    // flee from pursuer
                    mTarget = GameObject.FindGameObjectWithTag("Tagged");
                    Vector3 targetDirection = transform.position - mTarget.transform.position;

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

    public void SetTagged(bool status) {
        mIsTagged = status;
    }

    public void SetFrozen(bool status) {
        mIsFrozen = status;
    }

    public void SetTarget(bool status) {
        mIsTarget = status;
    }

    public void SetMaterial(Color c) {
        mMaterial.color = c;
    }

    void OnDestroy() {
        Destroy(mMaterial);
    }
}
