using UnityEngine;

namespace BioLab.Player
{
    /// <summary>
    /// Expressive procedural humanoid animator.
    /// Drives dynamic limb stride cycles, knee flexion, counter-arm swinging,
    /// vertical hip bounce, coat tail flutter, and idle breathing without requiring
    /// external skeletal animation clips.
    /// </summary>
    public class ProceduralHumanoidAnimator : MonoBehaviour
    {
        [Header("Limb Pivot References")]
        public Transform spinePivot;
        public Transform leftArmPivot;
        public Transform rightArmPivot;
        public Transform leftForearmPivot;
        public Transform rightForearmPivot;
        public Transform leftLegPivot;
        public Transform rightLegPivot;
        public Transform leftKneePivot;
        public Transform rightKneePivot;

        [Header("Coat Tail References")]
        public Transform coatTailLPivot;
        public Transform coatTailRPivot;
        public Transform coatTailBackPivot;

        [Header("Stride Tuning")]
        public float strideLength = 1.4f;
        public float maxLegSwingAngle = 36f;
        public float maxArmSwingAngle = 32f;
        public float maxKneeBendAngle = 45f;
        public float verticalBobAmount = 0.045f;

        private float _stridePhase;
        private float _tailLag;
        private Vector3 _initialSpinePos;

        private void Start()
        {
            if (spinePivot != null)
            {
                _initialSpinePos = spinePivot.localPosition;
            }
        }

        public void AnimateMotion(float currentSpeed, float sprintSpeed, float dt)
        {
            if (spinePivot != null && _initialSpinePos == Vector3.zero)
            {
                _initialSpinePos = spinePivot.localPosition;
            }

            float normSpeed = Mathf.Clamp01(currentSpeed / Mathf.Max(sprintSpeed, 0.1f));

            if (currentSpeed > 0.08f)
            {
                // Active locomotion cycle
                float cycleSpeed = (currentSpeed / strideLength) * Mathf.PI * 2f;
                _stridePhase += cycleSpeed * dt;
                if (_stridePhase > Mathf.PI * 2f) _stridePhase -= Mathf.PI * 2f;

                float legCycle = Mathf.Sin(_stridePhase);
                float armCycle = -legCycle;

                // 1. Leg Swings
                float legAngle = legCycle * maxLegSwingAngle * Mathf.Clamp01(currentSpeed / 2.5f);
                if (leftLegPivot != null) leftLegPivot.localRotation = Quaternion.Euler(legAngle, 0f, 0f);
                if (rightLegPivot != null) rightLegPivot.localRotation = Quaternion.Euler(-legAngle, 0f, 0f);

                // Knee flexion when swinging forward (backward knee bend)
                if (leftKneePivot != null)
                {
                    float leftKnee = (legCycle < 0f) ? Mathf.Abs(legCycle) * maxKneeBendAngle : 0f;
                    leftKneePivot.localRotation = Quaternion.Euler(leftKnee, 0f, 0f);
                }
                if (rightKneePivot != null)
                {
                    float rightKnee = (legCycle > 0f) ? legCycle * maxKneeBendAngle : 0f;
                    rightKneePivot.localRotation = Quaternion.Euler(rightKnee, 0f, 0f);
                }

                // 2. Counter Arm Swings (VRM rest pose has arms horizontal at Z=0; Z=+75 rotates left arm down, Z=-75 rotates right arm down)
                float armAngle = armCycle * maxArmSwingAngle * Mathf.Clamp01(currentSpeed / 2.5f);
                if (leftArmPivot != null) leftArmPivot.localRotation = Quaternion.Euler(armAngle, 0f, 75f);
                if (rightArmPivot != null) rightArmPivot.localRotation = Quaternion.Euler(-armAngle, 0f, -75f);

                if (leftForearmPivot != null)
                    leftForearmPivot.localRotation = Quaternion.Euler(0f, 22f + Mathf.Abs(armCycle) * 16f, 0f);
                if (rightForearmPivot != null)
                    rightForearmPivot.localRotation = Quaternion.Euler(0f, -(22f + Mathf.Abs(armCycle) * 16f), 0f);

                // 3. Vertical Hip Bobbing
                if (spinePivot != null)
                {
                    float bob = Mathf.Sin(_stridePhase * 2f) * verticalBobAmount * normSpeed;
                    spinePivot.localPosition = _initialSpinePos + new Vector3(0f, bob, 0f);
                }

                // 4. Coat Tail Flutter & Aerodynamic Lag
                float targetTailLag = normSpeed * 28f + Mathf.Sin(_stridePhase * 2f) * 6f;
                _tailLag = Mathf.Lerp(_tailLag, targetTailLag, 10f * dt);
            }
            else
            {
                // Idle breathing cycle
                float breath = Mathf.Sin(Time.time * 2.2f);
                if (spinePivot != null)
                {
                    spinePivot.localPosition = _initialSpinePos + new Vector3(0f, breath * 0.008f, 0f);
                    spinePivot.localRotation = Quaternion.Euler(breath * 0.8f, 0f, 0f);
                }

                if (leftArmPivot != null) leftArmPivot.localRotation = Quaternion.Slerp(leftArmPivot.localRotation, Quaternion.Euler(0f, 0f, 75f + breath * 1.0f), 5f * dt);
                if (rightArmPivot != null) rightArmPivot.localRotation = Quaternion.Slerp(rightArmPivot.localRotation, Quaternion.Euler(0f, 0f, -75f - breath * 1.0f), 5f * dt);
                if (leftForearmPivot != null) leftForearmPivot.localRotation = Quaternion.Slerp(leftForearmPivot.localRotation, Quaternion.Euler(0f, 22f, 0f), 5f * dt);
                if (rightForearmPivot != null) rightForearmPivot.localRotation = Quaternion.Slerp(rightForearmPivot.localRotation, Quaternion.Euler(0f, -22f, 0f), 5f * dt);

                if (leftLegPivot != null) leftLegPivot.localRotation = Quaternion.Slerp(leftLegPivot.localRotation, Quaternion.identity, 6f * dt);
                if (rightLegPivot != null) rightLegPivot.localRotation = Quaternion.Slerp(rightLegPivot.localRotation, Quaternion.identity, 6f * dt);
                if (leftKneePivot != null) leftKneePivot.localRotation = Quaternion.Slerp(leftKneePivot.localRotation, Quaternion.identity, 6f * dt);
                if (rightKneePivot != null) rightKneePivot.localRotation = Quaternion.Slerp(rightKneePivot.localRotation, Quaternion.identity, 6f * dt);

                _tailLag = Mathf.Lerp(_tailLag, 0f, 6f * dt);
            }

            // Apply Coat Tail Rotations
            if (coatTailLPivot != null)
                coatTailLPivot.localRotation = Quaternion.Euler(-_tailLag, 0f, -6f);
            if (coatTailRPivot != null)
                coatTailRPivot.localRotation = Quaternion.Euler(-_tailLag, 0f, 6f);
            if (coatTailBackPivot != null)
                coatTailBackPivot.localRotation = Quaternion.Euler(-_tailLag * 1.15f, 0f, 0f);
        }
    }
}
