using UnityEngine;
using UnityEngine.Events;

public class AIMovement : MonoBehaviour
{
    // modifiable attributes (in the Unity inspector)
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

    // States
    private bool mIsMoving;
    private bool mIsTarget;
    private bool mIsTagged;
    private bool mIsFrozen;
    private bool mHitBoundary;

    // Movement related variables
    private Vector3 mVelocity;
    private float mMaxVelocity;
    private float mPerceptionAngle;
    private float mSpeed;
    private float mChaseTimer;
    private float mBoundaryTimer;
    private GameObject mTarget;

    // other
    public UnityEvent onRoundEnd;
    private GameObject mIceBlock;
    private Animator mAnimator;
    
    void Start()
    {
        mTarget = null;
        mIsMoving = false;
        mHitBoundary = false;
        mBoundaryTimer = 0.0f;
        mPerceptionAngle = 45.0f;
        mAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Warp character to other side if they hit the level boundary, also clear the particle system particles
        if(CheckBoundary())
            gameObject.GetComponent<ParticleSystem>().Clear();

        // We update how long it's been since the character hit a boundary
        mBoundaryTimer -= Time.deltaTime;
        if(mBoundaryTimer < 0.0f) {
            mBoundaryTimer = 0.0f;
            mHitBoundary = false;
        }

        // ********************
        // BEHAVIOR FOR PURSUER
        // ********************
        if(mIsTagged) {
            // start the particle system
            gameObject.GetComponent<ParticleSystem>().Play();

            mSpeed = chaseSpeed;
            mMaxVelocity = mSpeed;

            // try to find a target if we don't have one
            if(!mTarget) {
                mTarget = FindClosest("Free");
                if(mTarget) {
                    // notify that character that they are the target and start chasing it for 2 seconds
                    mTarget.GetComponent<AIMovement>().SetTarget(true);
                    mChaseTimer = 2.0f;
                }
                else {
                    // no targets can be found so wander around and trigger the end of round
                    Wander();
                    mAnimator.SetFloat("Blend", mVelocity.magnitude / mMaxVelocity);
                    onRoundEnd?.Invoke();
                }
            }
            // behavior to chase our current target
            else {
                mChaseTimer -= Time.deltaTime;

                // chased target for too long, switch to another
                if(mChaseTimer <= 0.0f) {
                    // notify our current target that they are no longer being chased
                    AIMovement targetAI = mTarget.gameObject.GetComponent<AIMovement>();
                    targetAI.SetTarget(false);
                    targetAI.tag = "Free";

                    // find the next target (might be the same)
                    mTarget = FindClosest("Free");
                    if(mTarget) {
                        // notify that character that they are the target and start chasing it for 2 seconds
                        mTarget.GetComponent<AIMovement>().SetTarget(true);
                        mChaseTimer = 2.0f;
                    }
                }

                if(mTarget) {
                    Vector3 targetDirection = mTarget.transform.position - transform.position;

                    // **********************
                    // HEURISTIC A.i and A.ii
                    // **********************
                    if(!mIsMoving) {
                        // step directly to the target if we are close enough
                        if(Mathf.Approximately(targetDirection.magnitude, 0.0f)) {
                            transform.position = mTarget.transform.position;
                        }
                        // turn on the spot to face the target
                        else {
                            Align(targetDirection);
                            mAnimator.SetFloat("Blend", 0.0f);

                            if(transform.forward == targetDirection.normalized)
                                mIsMoving = true;
                        }
                    }
                    // **********************
                    // HEURISTIC B.i and B.ii
                    // **********************
                    else {
                        if(targetDirection.magnitude < radius) {
                            // reached the target
                            mTarget.GetComponent<AIMovement>().Freeze();
                            mTarget = null;
                            mIsMoving = false;
                        }
                        else {
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
        
        // ********************************
        // BEHAVIOR FOR UNTAGGED CHARACTERS
        // ********************************
        else { // if(!mIsTagged)
            // turn off particles
            gameObject.GetComponent<ParticleSystem>().Stop();

            if(!mIsFrozen) {
                mSpeed = baseSpeed;
                mMaxVelocity = mSpeed;

                // behavior for the currently chased character
                if(mIsTarget) {

                    // We recently hit a boundary so keep moving forward for the remainder of mBoundaryTimer time.
                    // This hack helps prevent a situation where the last unfrozen character endlessly moves back 
                    // and forth accross the same boundary (since crossing a boundary now makes the character face the pursuer,
                    // so they turn away)
                    if(mHitBoundary) {
                        Seek(transform.forward);
                    }
                    else {
                        // flee from pursuer
                        mTarget = GameObject.FindGameObjectWithTag("Tagged");
                        if(mTarget) {
                            Vector3 targetDirection = transform.position - mTarget.transform.position;

                            // **********************
                            // HEURISTIC C.i and C.ii
                            // **********************
                            if(!mIsMoving) {
                                if(Mathf.Approximately(targetDirection.magnitude, 0.0f)) {
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
                                float angle = Vector3.Angle(targetDirection, transform.forward);
                                if(angle < mPerceptionAngle) {
                                    // Note that since targetDirection is inversed (transform.pos - target.pos),
                                    // Seek is essentially a flee behavior here
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
                    // free and not currently being chased so find a character to unfreeze
                    mTarget = FindClosest("Frozen");
                    if(mTarget) {
                        Vector3 targetDirection = mTarget.transform.position - transform.position;

                        // **********************
                        // HEURISTIC A.i and A.ii
                        // **********************
                        if(!mIsMoving) {
                            if(Mathf.Approximately(targetDirection.magnitude, 0.0f)) {
                                transform.position = mTarget.transform.position;
                            }
                            else {
                                Align(targetDirection);
                                mAnimator.SetFloat("Blend", 0.0f);

                                if(transform.forward == targetDirection.normalized)
                                    mIsMoving = true;
                            }
                        }

                        // **********************
                        // HEURISTIC B.i and B.ii
                        // **********************
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

    // **************
    // HELPER METHODS
    // **************

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

        // return closest target if it exists, otherwise return null
        return closestTarget ? closestTarget : null;
    }

    // Check the characters position on the xz plane and warp it to the opposite side if we get too far from the origin.
    // note that we first bring the character back by 0.5 units before warping so that they don't immediately retrigger this
    // method next frame.
    bool CheckBoundary() {
        Vector3 position = transform.position;
        if(Mathf.Abs(position.x) > 17.5f) {
            position.x += position.x > 0 ? -0.5f : 0.5f;
            position.x *= -1;
            // if the character that hit the boundary is the current target, we should set their boundary timer,
            // otherwise we don't care.
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
            // if the character that hit the boundary is the current target, we should set their boundary timer,
            // otherwise we don't care.
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

    // ******************
    // MOVEMENT BEHAVIORS
    // ******************

    // Incrementally look in the direction of the target
    void Align(Vector3 direction) {
        float step = rotationSpeed * Time.deltaTime; ;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);
    }

    // Seek a position 1 unit ahead of where the target character is facing
    void Pursue(GameObject target) {
        Vector3 targetPosition = target.transform.position + target.transform.forward - transform.position;
        Seek(targetPosition);
        Align(targetPosition);
    }

    // move towards direction with maximum velocity
    void Seek(Vector3 direction) {
        mVelocity = direction * mSpeed;
        if(mVelocity.magnitude > mMaxVelocity)
            mVelocity = mVelocity.normalized * mMaxVelocity;
        transform.position += mVelocity * Time.deltaTime;
    }

    // use kinematic arrive to slow down as we reach the target position
    // prevents overshooting the target
    void Arrive(Vector3 direction) {
        mVelocity = direction / timeToTarget;
        if(mVelocity.magnitude > mMaxVelocity) {
            mVelocity = mVelocity.normalized * mMaxVelocity;
        }
        transform.position += mVelocity * Time.deltaTime;
    }

    // wander in a random direction
    void Wander() {
        // generate a random angle (higher distribution of angles near 0.0f)
        float angle = (Random.Range(-1.0f, 1.0f) - Random.Range(-1.0f, 1.0f));
        float step = angle * rotationSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

        mVelocity = transform.forward * mSpeed;
        if(mVelocity.magnitude > mMaxVelocity)
            mVelocity = mVelocity.normalized * mMaxVelocity;
        transform.position += mVelocity * Time.deltaTime;
    }

    // **************
    // STATE CHANGERS
    // **************

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
}
