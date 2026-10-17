using UnityEngine;

namespace BioLab.Core
{
    /// <summary>
    /// Smooth third-person orbital camera controller with collision detection,
    /// 360-degree mouse orbit, scroll wheel zoom, and camera-relative shoulder offset.
    /// Allows inspecting the character from all angles up close.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target & Positioning")]
        public Transform target;
        public float heightOffset = 1.12f;
        public float rightOffset = 0.20f;
        public float defaultDistance = 2.4f;
        public float minDistance = 0.6f;
        public float maxDistance = 6.5f;

        [Header("Sensitivity & Rotation Limits")]
        public float mouseSensitivityX = 3.2f;
        public float mouseSensitivityY = 2.4f;
        public float minPitch = -35f;
        public float maxPitch = 75f;
        public float rotationSmoothTime = 0.04f;

        [Header("Collision Avoidance")]
        public LayerMask obstacleLayers;
        public float collisionRadius = 0.2f;

        private float _yaw = 168f;
        private float _pitch = 4.0f;
        private float _currentDistance;
        private float _targetDistance;
        private Vector3 _currentRotation;
        private Vector3 _rotationVelocity;

        private void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-camYaw", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _yaw);
                }
                else if (args[i].Equals("-camPitch", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _pitch);
                }
                else if (args[i].Equals("-camDist", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    float d;
                    if (float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                    {
                        defaultDistance = d;
                    }
                }
            }

            _currentDistance = defaultDistance;
            _targetDistance = defaultDistance;
            _currentRotation = new Vector3(_pitch, _yaw, 0f);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetOrbitPose(float yaw, float pitch, float distance)
        {
            _yaw = yaw;
            _pitch = pitch;
            defaultDistance = distance;
            _currentDistance = distance;
            _targetDistance = distance;
            _currentRotation = new Vector3(_pitch, _yaw, 0f);
            _rotationVelocity = Vector3.zero;
        }

        private void Update()
        {
            bool uiModalOpen = (BioLab.UI.DiegeticTabletUI.Instance != null && BioLab.UI.DiegeticTabletUI.Instance.isOpen);

            if (uiModalOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            // Do not lock cursor if clicking top command bar or top-right protocol HUD
            float guiMouseY = Screen.height - Input.mousePosition.y;
            bool clickingHud = (guiMouseY <= 52f) || (Input.mousePosition.x >= Screen.width - 380f && guiMouseY <= 215f);

            // Cursor Lock & Unlock controls
            if (!clickingHud && Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Zoom via Mouse ScrollWheel
            float scroll = 0f;
            try { scroll = Input.GetAxis("Mouse ScrollWheel"); } catch { }
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _targetDistance = Mathf.Clamp(_targetDistance - scroll * 2.5f, minDistance, maxDistance);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            bool uiModalOpen = (BioLab.UI.DiegeticTabletUI.Instance != null && BioLab.UI.DiegeticTabletUI.Instance.isOpen);

            // Rotate camera if locked OR dragging right mouse button (when UI modal is closed)
            bool canRotate = !uiModalOpen && ((Cursor.lockState == CursorLockMode.Locked) || Input.GetMouseButton(1));
            if (canRotate)
            {
                float mx = 0f;
                float my = 0f;
                try
                {
                    mx = Input.GetAxis("Mouse X");
                    my = Input.GetAxis("Mouse Y");
                }
                catch { }

                _yaw += mx * mouseSensitivityX;
                _pitch -= my * mouseSensitivityY;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            // Smooth distance damping
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * 12f);

            // Smooth rotation damping
            Vector3 targetEuler = new Vector3(_pitch, _yaw, 0f);
            _currentRotation = Vector3.SmoothDamp(_currentRotation, targetEuler, ref _rotationVelocity, rotationSmoothTime);
            Quaternion rotation = Quaternion.Euler(_currentRotation);

            // Camera-relative pivot: stable over-the-shoulder view that does NOT jerk when character turns
            Vector3 pivot = target.position + Vector3.up * heightOffset + (rotation * Vector3.right * rightOffset);

            // Desired camera position
            Vector3 desiredCamPos = pivot - (rotation * Vector3.forward * _currentDistance);

            // Obstacle collision check
            if (obstacleLayers.value != 0 && Physics.SphereCast(pivot, collisionRadius, (desiredCamPos - pivot).normalized, out RaycastHit hit, _currentDistance, obstacleLayers))
            {
                if (hit.distance > 0.5f)
                    transform.position = pivot + (desiredCamPos - pivot).normalized * (hit.distance - 0.1f);
                else
                    transform.position = desiredCamPos;
            }
            else
            {
                transform.position = desiredCamPos;
            }

            transform.LookAt(pivot);
        }
    }
}
