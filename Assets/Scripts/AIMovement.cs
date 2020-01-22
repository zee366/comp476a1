using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField]
    GameObject prefab;

    public UnityEvent onRoundEnd;

    // States
    private bool mIsMoving;
    private bool mIsTarget;
    private bool mHasTarget;
    private bool mIsTagged;
    private bool mIsFrozen;
    private bool mHitBoundary;

    private Vector3 mVelocity;
    private float mMaxVelocity;
    private float mPerceptionAngle;
    private float mSpeed;
    private float mChaseTimer;
    private float mBoundaryTimer;
    private GameObject mTarget;
    private GameObject mIceBlock;
    private GameController mGameController;

    private Animator mAnimator;

    private const float mEpsilon = 0.0001f;
    
    void Start()
    {
        mTarget = null;
        mIsMoving = false;
        mHitBoundary = false;
        mBoundaryTimer = 0.0f;
        mAnimator = GetComponent<Animator>();
        mGameController = GameObject.Find("GameController").GetComponent<GameController>();
        mPerceptionAngle = 45.0f;
    }

    void Update()
    {
        // Warp character to other side if they hit the level boundary
        if(CheckBoundary())
            gameObject.GetComponent<ParticleSystem>().Clear();

        mBoundaryTimer -= Time.deltaTime;
        if(mBoundaryTimer < 0.0f) {
            mBoundaryTimer = 0.0f;
            mHitBoundary = false;
        }

        // behavior for pursuer
        if(mIsTagged) {
            // start the particle system
            gameObject.GetComponent<ParticleSystem>().Play();

            mSpeed = chaseSpeed;
            mMaxVelocity = mSpeed;

            if(!mTarget) {
                mTarget = FindClosest("Free");
                if(mTarget) {
                    mTarget.GetComponent<AIMovement>().SetTarget(true);
                    mChaseTimer = 2.0f;
                }
                else {
                    Wander();
                    mAnimator.SetFloat("Blend", mVelocity.magnitude / mMaxVelocity);
                    onRoundEnd?.Invoke();
                }
            }
            else {
                mChaseTimer -= Time.deltaTime;
                if(mChaseTimer <= 0.0f) {
                    // chased target for too long, switch to another
                    AIMovement targetAI = mTarget.gameObject.GetComponent<AIMovement>();
                    targetAI.SetTarget(false);
                    targetAI.tag = "Free";
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
                            mAnimator.SetFloat("Blend", 0.0f);

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
                                mAnimator.SetFloat("Blend", mVelocity.magnitude / mMaxVelocity);
                            }
                            else {
                                mIsMoving = false;
                                Align(targetDirection);
                                mAnimator.SetFloat("Blend", 0.0f);
                            }
                        }
                    }
                }
            }
        }
        // behavior for chased characters
        else { // if(!mIsTagged)
            // turn off particles
            gameObject.GetComponent<ParticleSystem>().Stop();
            // behavior for untagged characters
            if(!mIsFrozen) {
                mSpeed = baseSpeed;
                mMaxVelocity = mSpeed;

                if(mIsTarget) {

                    // recently transitioned to other side of level, keep moving forward so it doesn't bounce back accross the boundary
                    if(mHitBoundary) {
                        Seek(transform.forward);
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
                                    mAnimator.SetFloat("Blend", 0.0f);

                                    if(transform.forward == targetDirection.normalized)
                                        mIsMoving = true;
                                }
                            }
                            else {
                                // TODO: Add speed dependant angle of perception
                                float angle = Vector3.Angle(targetDirection, transform.forward);
                                if(angle < mPerceptionAngle) {
                                    // Note that since targetDirection is inversed (transform.pos - target.pos) Seek is essentially a flee behavior
                                    Seek(targetDirection);
                                    mAnimator.SetFloat("Blend", mVelocity.magnitude / mMaxVelocity);
                                }
                                else {
                                    mIsMoving = false;
                                    mAnimator.SetFloat("Blend", 0.0f);
                                }
                                Align(targetDirection);
                            }
                        }
                    }
                }
                else { // if(mIsTarget)
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
                                mAnimator.SetFloat("Blend", 0.0f);

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
                                if(angle < mPerceptionAngle) {
                                    Arrive(targetDirection);
                                    mAnimator.SetFloat("Blend", mVelocity.magnitude / mMaxVelocity);
                                }
                                else {
                                    mIsMoving = false;
                                    mAnimator.SetFloat("Blend", 0.0f);
                                }
                                Align(targetDirection);
                            }
                        }
                    }
                    else { // if(mTarget)
                        Wander();
                        mAnimator.SetFloat("Blend", mVelocity.magnitude / mMaxVelocity);
                    }
                }
            }
            else { // if(!mIsFrozen)
                // frozen, do nothing
                mSpeed = 0.0f;
                mAnimator.SetFloat("Blend", 0.0f);
            }
        }
    }

    // HELPER METHODS

    public void Freeze() {
        SetFrozen(true);
        SetTarget(false);
        gameObject.tag = "Frozen";
        mIceBlock = Instantiate(prefab, transform.position, Quaternion.Euler(-90.0f, 0.0f, 0.0f));
    }

    public void UnFreeze() {
        SetFrozen(false);
        SetTagged(false);
        gameObject.tag = "Free";
        if(mIceBlock) {
            mIceBlock.GetComponent<Animation>().Play();
            Destroy(mIceBlock, 1.5f);
            mIceBlock = null;
        }
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

bool CheckBoundary() {
        Vector3 position = transform.position;
        if(Mathf.Abs(position.x) > 17.5f) {
            position.x += position.x > 0 ? -0.5f : 0.5f;
            position.x *= -1;
            if(mIsTarget) {
                mHitBoundary = true;
                mBoundaryTimer = 2.0f;
            }
            else {
                mHitBoundary = true;
            }
        }
        else if(Mathf.Abs(position.z) > 17.5f) {
            position.z += position.z > 0 ? -0.5f : 0.5f;
            position.z *= -1;
            if(mIsTarget) {
                mHitBoundary = true;
                mBoundaryTimer = 2.0f;
            }
            else {
                mHitBoundary = true;
            }
        }
        else {
            if(Mathf.Approximately(mBoundaryTimer, 0.0f))
                mHitBoundary = false;
        }

        transform.position = position;

        return mHitBoundary;
    }

    // MOVEMENT BEHAVIORS

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
        mVelocity = direction * mSpeed;
        if(mVelocity.magnitude > mMaxVelocity)
            mVelocity = mVelocity.normalized * mMaxVelocity;
        transform.position += mVelocity * Time.deltaTime;
    }

    void Flee(Vector3 direction) {
        mVelocity = direction.normalized * mSpeed;

        transform.position += mVelocity * Time.deltaTime;
    }

    void Arrive(Vector3 direction) {
        mVelocity = (direction / timeToTarget) * mSpeed;
        if(mVelocity.magnitude > mMaxVelocity) {
            mVelocity = mVelocity.normalized * mMaxVelocity;
        }
        transform.position += mVelocity * Time.deltaTime;
    }

    void Wander() {
        float angle = (Random.Range(-1.0f, 1.0f) - Random.Range(-1.0f, 1.0f));
        float step = angle * rotationSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

        mVelocity = transform.forward * mSpeed;
        if(mVelocity.magnitude > mMaxVelocity)
            mVelocity = mVelocity.normalized * mMaxVelocity;
        transform.position += mVelocity * Time.deltaTime;
    }

    // STATE CHANGERS

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
}
