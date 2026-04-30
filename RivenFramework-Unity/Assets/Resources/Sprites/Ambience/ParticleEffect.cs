using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParticleEffect : MonoBehaviour
{
    public Animator animator;
    [Space]
    [Unbox] public AnimatedVariable animationSpeed = new AnimatedVariable(1, 0, new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1)));
    [Unbox] public AnimatedVariable scale = new AnimatedVariable(1, 0, new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1)));
    [Unbox] public AnimatedVariable velocity = new AnimatedVariable(0, 0, new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1)));
    [Unbox] public AnimatedVariable rotationDegrees = new AnimatedVariable(0, 0, new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1)));
    public bool startWithRandomRotation;
    //public bool rotateInDirection;

    [Space, Header("Animator inputs")]
    public float animationTime = 0;

    public Quaternion randomRotationDirection = Quaternion.identity;

    private Vector3 direction;
    private Transform cam;
    Vector3 facingDirection => cam.forward;

    [Serializable]
    public struct AnimatedVariable
    {
        public AnimatedVariable(float baseFactor, float randomBaseOffset, AnimationCurve lifetime)
        {
            this.baseFactor = baseFactor;
            this.randomBaseOffset = randomBaseOffset;
            this.lifetime = lifetime;
            this.randomOffset = 0f;
        }

        public float baseFactor;
        public float randomBaseOffset;
        public AnimationCurve lifetime;

        private float randomOffset;
        public float ApplyRandomFactor() =>
            randomOffset = Random.Range(randomBaseOffset, -randomBaseOffset);
        public float Get(float time) => lifetime.Evaluate(time) * (baseFactor + randomOffset);
    }

    void Start()
    {
        cam = Camera.main.transform;
        
        direction = GetRandomDirection();
        if (startWithRandomRotation)
            randomRotationDirection = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);

        animationSpeed.ApplyRandomFactor();
        scale.ApplyRandomFactor();
        velocity.ApplyRandomFactor();
        rotationDegrees.ApplyRandomFactor();
    }
    private void OnValidate()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        if (animator.enabled == false)
            Destroy(gameObject);

        animator.speed = animationSpeed.Get(animationTime);
        transform.rotation = Quaternion.identity;
        transform.rotation = 
            randomRotationDirection *
            Quaternion.LookRotation(facingDirection, transform.up) * 
            Quaternion.AngleAxis(rotationDegrees.Get(animationTime), facingDirection);

        transform.localScale = Vector3.one * scale.Get(animationTime);
        transform.position += direction * velocity.Get(animationTime) * Time.deltaTime;
        //if (rotateInDirection) transform.LookAt(Vector3.forward);
    }
    public void OnAnimationDone()
    {
        animator.enabled = false;
    }
    public void SetAnimatorToRandomTime()
    {
        // Ensure the animator is updated at least once before modifying its time
        animator.Update(0f);

        // Get random time between 0 and 1 normalized
        float randomTime = Random.value;

        animator.Play("", 0, randomTime);
        animator.Update(0f);
    }
    public Vector2 GetRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        return Quaternion.AngleAxis(randomAngle, Vector3.forward) * Vector3.up;
    }
}