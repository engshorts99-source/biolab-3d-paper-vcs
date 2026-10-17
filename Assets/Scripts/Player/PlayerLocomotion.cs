using UnityEngine;

namespace BioLab.Player
{
    /// <summary>
    /// Physics-accurate locomotion controller for 3D researchers.
    /// Implements inertia ramp-up, braking friction, body banking into turns,
    /// dynamic procedural stride synchronization, and camera-relative WASD navigation.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("Character Mass & Specs")]
        public float characterMass = 72f;

        [Header("Locomotion Speeds (m/s)")]
        public float walkSpeed = 2.0f;
        public float runSpeed = 5.2f;
        public float sprintSpeed = 7.8f;

        [Header("Physics Inertia & Responsiveness")]
        public float accelerationTime = 0.22f;
        public float decelerationTime = 0.14f;
        public float rotationSpeed = 14f;

        [Header("Dynamic Body Banking (Tilt into Turns)")]
        public float maxBankAngle = 14f;
        public float bankSmoothing = 8f;
        public Transform characterModelRoot;

        [Header("Ground & Gravity Physics")]
        public float gravity = -9.81f;

        [Header("Test & Automation")]
        public bool autoRunTest = false;

        // Internal physics state
        private CharacterController _controller;
        private ProceduralHumanoidAnimator _proceduralAnimator;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _currentBankAngle;
        private float _currentSpeedMagnitude;
        private bool _isGrounded;

        // Camera reference
        private Transform _cameraTransform;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            ResolveCamera();
            ResolveAnimator();
        }

        public void ResolveCamera()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        public void ResolveAnimator()
        {
            if (characterModelRoot != null)
            {
                _proceduralAnimator = characterModelRoot.GetComponentInChildren<ProceduralHumanoidAnimator>();
            }
            if (_proceduralAnimator == null)
            {
                _proceduralAnimator = GetComponentInChildren<ProceduralHumanoidAnimator>();
            }
        }

        private void Update()
        {
            if (_cameraTransform == null)
            {
                ResolveCamera();
            }

            CheckGrounded();
            HandleLocomotion(Time.deltaTime);
            HandleBanking(Time.deltaTime);
            UpdateAnimation(Time.deltaTime);
        }

        private void CheckGrounded()
        {
            _isGrounded = _controller.isGrounded;
            if (_isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f; // Downward snap for ground contact
            }
        }

        private void HandleLocomotion(float dt)
        {
            // Direct & Axis Input Support (Guarantees WASD works on Mac standalone)
            float inputH = 0f;
            float inputV = 0f;

            bool uiModalOpen = (BioLab.UI.DiegeticTabletUI.Instance != null && BioLab.UI.DiegeticTabletUI.Instance.isOpen);

            if (autoRunTest)
            {
                inputV += 1f;
            }
            else if (!uiModalOpen)
            {
                try
                {
                    inputH = Input.GetAxisRaw("Horizontal");
                    inputV = Input.GetAxisRaw("Vertical");
                }
                catch { }

                // Explicit Keycode Fallbacks
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputV += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputV -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputH += 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputH -= 1f;
            }

            inputH = Mathf.Clamp(inputH, -1f, 1f);
            inputV = Mathf.Clamp(inputV, -1f, 1f);

            Vector3 inputDir = new Vector3(inputH, 0f, inputV);
            if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

            // Sprint check
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float targetSpeed = 0f;
            if (inputDir.sqrMagnitude > 0.01f)
            {
                targetSpeed = isSprinting ? sprintSpeed : runSpeed;
            }

            // Project input relative to Camera orientation
            Vector3 targetDir = Vector3.zero;
            if (autoRunTest)
            {
                targetDir = Vector3.forward;
                targetSpeed = runSpeed;
            }
            else if (inputDir.sqrMagnitude > 0.01f)
            {
                if (_cameraTransform != null)
                {
                    Vector3 camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
                    Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
                    targetDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;
                }
                else
                {
                    // Fallback to world direction
                    targetDir = inputDir.normalized;
                }
            }

            Vector3 targetHorizontalVelocity = targetDir * targetSpeed;

            // Physics Inertia Ramp / Deceleration using smooth exponential rate
            float rate = (targetSpeed > _currentSpeedMagnitude) ? (1f / Mathf.Max(accelerationTime, 0.01f)) : (1f / Mathf.Max(decelerationTime, 0.01f));
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetHorizontalVelocity, rate * (sprintSpeed + 12f) * dt);
            _currentSpeedMagnitude = _horizontalVelocity.magnitude;

            // Smooth Character Rotation towards movement direction
            if (_horizontalVelocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_horizontalVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * dt);
            }

            // Apply gravity
            _verticalVelocity += gravity * dt;

            // Total Velocity motion
            Vector3 motion = _horizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(motion * dt);
        }

        private void HandleBanking(float dt)
        {
            if (characterModelRoot == null) return;

            float turnInput = 0f;
            try { turnInput = Input.GetAxis("Horizontal"); } catch { }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turnInput -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turnInput += 1f;
            turnInput = Mathf.Clamp(turnInput, -1f, 1f);

            float targetBank = 0f;
            if (_currentSpeedMagnitude > 1.0f)
            {
                targetBank = -turnInput * (_currentSpeedMagnitude / sprintSpeed) * maxBankAngle;
            }

            _currentBankAngle = Mathf.Lerp(_currentBankAngle, targetBank, bankSmoothing * dt);

            Vector3 localEuler = characterModelRoot.localEulerAngles;
            characterModelRoot.localRotation = Quaternion.Euler(localEuler.x, localEuler.y, _currentBankAngle);
        }

        private void UpdateAnimation(float dt)
        {
            if (_proceduralAnimator == null)
            {
                ResolveAnimator();
            }

            if (_proceduralAnimator != null)
            {
                _proceduralAnimator.AnimateMotion(_currentSpeedMagnitude, sprintSpeed, dt);
            }
        }

        /// <summary>
        /// Switch mass and physical stature parameters.
        /// </summary>
        public void ApplyCharacterSpecs(float mass, float height, float radius)
        {
            characterMass = mass;
            _controller.height = height;
            _controller.radius = radius;
            _controller.center = new Vector3(0f, height * 0.5f, 0f);
            ResolveAnimator();
        }
    }
}
