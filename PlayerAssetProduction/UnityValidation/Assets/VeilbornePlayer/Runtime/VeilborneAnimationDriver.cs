using UnityEngine;

namespace Veilbound.PlayerStaging
{
    // Feed the actual collision-resolved velocity and aim direction from the game.
    // This component deliberately owns no input, collision, saves, or equipment.
    [RequireComponent(typeof(Animator))]
    public sealed class VeilborneAnimationDriver : MonoBehaviour
    {
        static readonly int PlanarSpeed = Animator.StringToHash("PlanarSpeed");
        [SerializeField, Min(0.01f)] float blendDamping = 0.14f;
        [SerializeField, Min(0.01f)] float turnResponse = 12f;
        [SerializeField] bool rotateVisual = true;
        Animator animator;
        Vector3 velocity;
        Vector3 facing;

        void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
        }

        public void SubmitMotion(Vector3 resolvedWorldVelocity, Vector3 desiredWorldFacing)
        {
            velocity = Vector3.ProjectOnPlane(resolvedWorldVelocity, Vector3.up);
            facing = Vector3.ProjectOnPlane(desiredWorldFacing, Vector3.up);
        }

        void Update()
        {
            animator.SetFloat(PlanarSpeed, velocity.magnitude, blendDamping, Time.deltaTime);
            if (!rotateVisual || facing.sqrMagnitude < 0.0001f) return;
            Quaternion target = Quaternion.LookRotation(facing.normalized, Vector3.up);
            float t = 1f - Mathf.Exp(-turnResponse * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
        }

        void OnDisable()
        {
            velocity = Vector3.zero;
            facing = Vector3.zero;
            if (animator && animator.isActiveAndEnabled) animator.SetFloat(PlanarSpeed, 0f);
        }
    }
}
