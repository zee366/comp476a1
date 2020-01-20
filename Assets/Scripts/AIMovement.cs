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

    // States
    private bool mIsMoving;
    private bool mIsTarget;
    private bool mHasTarget;
    private bool mIsTagged;
    private bool mIsFrozen;
    private bool mHitBoundary;

    private Vector3 mLastPosition;
    private Vector3 mVelocity;
    private float mPerceptionAngle;
    private float mSpeed;
    private float mChaseTimer;
    private float mBoundaryTimer;
    private GameObject mTarget;

    private Animator mAnimator;
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
        mAnimator = GetComponent<Animator>();
        mPerceptionAngle = 45.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Implement reset if all targets are frozen

        // Warp character to other side if they hit the level boundary
        CheckBoundary();

        if(mIsTagged) {
            // start the particle system
            gameObject.GetComponent<ParticleSystem>().Play();

            // behavior for tagged character
            mSpeed = chaseSpeed;

            float deltaPositionLength = (transform.position - mLastPosition).magnitude;
            if(!mTarget) {
                mTarget = FindClosest("Free");
                if(mTarget) {
                    mTarget.GetComponent<AIMovement>().SetTarget(true);
                    mChaseTimer = 2.0f;
                }
                else {
                    Wander();
                }
            }
            else {
                mChaseTimer -= Time.deltaTime;
                if(mChaseTimer <= 0.0f) {
                    mTarget.gameObject.GetComponent<AIMovement>().SetTarget(false);
                    mTarget = FindClosest("Free");
                    if(mTarget) {
                        mTarget.GetComponent<AIMovement>().SetTarget(true);
                        mChaseTimer = 2.0f;
                    }
                }
                if(mTarget) {
                    Vector3 targetDirection = mTarget.transform.position - transform.position;

                    if(!mIsMoving) {
                        if(targetDirection.magnitude < mEpsilon) {
                            transform.position = mTarget.transform.position;
                        }
                        else {
                            Align(targetDirection);

                            if(transform.forward == targetDirection.normalized)
                                mIsMoving = true;
                        }
                    }
                    else {
                        if(targetDirection.magnitude < radius) {
                            // reached the target
                            mTarget.GetComponent<AIMovement>().Freeze();
                            mTarget = null;
                            mIsMoving = false;
                        }
                        else {
                            // TODO: Add speed dependant angle of perception
                            float angle = Vector3.Angle(targetDirection, transform.forward);
                            if(angle < mPerceptionAngle) {
                                Pursue(mTarget);
                            }
                            else {
                                mIsMoving = false;
                                Align(targetDirection);
                            }
                        }
                    }
                }
            }
        }
        else {
            // turn off particles
            gameObject.GetComponent<ParticleSystem>().Stop();
            // behavior for untagged characters
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
                    else {
                        // flee from pursuer
                        mTarget = GameObject.FindGameObjectWithTag("Tagged");
                        if(mTarget) {
                            Vector3 targetDirection = transform.position - mTarget.transform.position;

                            if(!mIsMoving) {
                                if(targetDirection.magnitude < mEpsilon) {
                                    transform.position += targetDirection.normalized / 10.0f;
                                }
                                else {
                                    Align(targetDirection);
                                    if(transform.forward == targetDirection.normalized)
                                        mIsMoving = true;
                                }
                            }
                            else {
                                // TODO: Add speed dependant angle of perception
                                float angle = Vector3.Angle(targetDirection, transform.forward);
                                if(angle < mPerceptionAngle)
                                    // Note that since targetDirection is inversed (transform.pos - target.pos) Seek is essentially a flee behavior
                                    Seek(targetDirection);
                                else
                                    mIsMoving = false;
                                Align(targetDirection);
                            }
                        }
                    }
                }
                else {
                    // find a target to free
                    mTarget = FindClosest("Frozen");
                    if(mTarget) {
                        Vector3 targetDirection = mTarget.transform.position - transform.position;
                        if(!mIsMoving) {
                            if(targetDirection.magnitude < mEpsilon) {
                                transform.position += targetDirection.normalized / 10.0f;
                            }
                            else {
                                Align(targetDirection);
                                if(transform.forward == targetDirection.normalized)
                                    mIsMoving = true;
                            }
                        }
                        else {
                            if(targetDirection.magnitude < radius) {
                                // reached the target
                                mTarget.GetComponent<AIMovement>().UnFreeze();
                                mTarget = null;
                                mIsMoving = false;
                            }
                            else {
                                float angle = Vector3.Angle(targetDirection, transform.forward);
                                if(angle < mPerceptionAngle)
                                    Arrive(targetDirection);
                                else
                                    mIsMoving = false;
                                Align(targetDirection);
                            }
                        }
                    }
                    else {
                        Wander();
                    }
                }
            }
            else {
                // frozen, do nothing
            }
        }
    }

    void Freeze() {
        SetFrozen(true);
        SetTarget(false);
        SetMaterial(Color.cyan);
        gameObject.tag = "Frozen";
    }

    void UnFreeze() {
        SetFrozen(false);
        SetMaterial(Color.green);
        gameObject.tag = "Free";
    }

    GameObject FindClosest(string tag) {
        GameObject closestTarget = null;
        float distanceToTarget = Mathf.Infinity;
        if(mTarget && gameObject.tag == "Tagged") {
            closestTarget = mTarget;
            distanceToTarget = (mTarget.transform.position - transform.position).sqrMagnitude;
        }

        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        for(int i = 0; i < targets.Length; i++) {
            Vector3 targetDirection = targets[i].transform.position - transform.position;
            if(targetDirection.sqrMagnitude < distanceToTarget) {
                closestTarget = targets[i];
                distanceToTarget = targetDirection.sqrMagnitude;
            }
        }
        if(closestTarget)
            return closestTarget;
        return null;
    }

    void CheckBoundary() {
        Vector3 position = transform.position;
        if(Mathf.Abs(position.x) > 17.5f) {
            position.x += position.x > 0 ? -0.5f : 0.5f;
            position.x *= -1;
            if(mIsTarget) {
                mHitBoundary = true;
                mBoundaryTimer = 2.0f;
            }
            gameObject.GetComponent<ParticleSystem>().Clear();
        }
        else if(Mathf.Abs(position.z) > 17.5f) {
            position.z += position.z > 0 ? -0.5f : 0.5f;
            position.z *= -1;
            if(mIsTarget) {
                mHitBoundary = true;
                mBoundaryTimer = 2.0f;
            }
            gameObject.GetComponent<ParticleSystem>().Clear();
        }

        transform.position = position;
    }

    void Align(Vector3 direction) {
        float step = rotationSpeed * Time.deltaTime; ;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);
    }

    void Pursue(GameObject target) {
        Vector3 targetPosition = (target.transform.position + target.transform.forward) - transform.position;
        Seek(targetPosition);
        Align(targetPosition);
    }

    void Seek(Vector3 direction) {
        mVelocity = direction.normalized * mSpeed;

        transform.position += mVelocity * Time.deltaTime;
    }

    void Flee(Vector3 direction) {
        mVelocity = direction.normalized * mSpeed;

        transform.position += mVelocity * Time.deltaTime;
    }

    void Arrive(Vector3 direction) {
        mVelocity = direction /= timeToTarget;
        if(mVelocity.magnitude > mSpeed) {
            mVelocity = mVelocity.normalized * mSpeed;
        }
        transform.position += mVelocity * Time.deltaTime;
    }

    void Wander() {
        float angle = (Random.Range(-1.0f, 1.0f) - Random.Range(-1.0f, 1.0f));
        float step = angle * rotationSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

        transform.position += transform.forward * mSpeed * Time.deltaTime;
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
