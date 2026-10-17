using System;
using UnityEngine;

namespace BioLab.Equipment
{
    public enum EquipmentType
    {
        RefrigeratedCentrifuge,  // e.g. 5424 R
        BiosafetyCabinet,        // BSC Class II A2
        UltraLowFreezer,         // -80°C Freezer
        VortexMixer,
        MicropipetteStation
    }

    public enum EquipmentState
    {
        Idle,
        LidOpen,
        Running,
        Error,
        Complete
    }

    /// <summary>
    /// Interactive laboratory equipment controller.
    /// Handles player proximity, hover highlights, rotor spin physics, sound, and step validation.
    /// </summary>
    public class LabEquipment : MonoBehaviour
    {
        [Header("Equipment Identity")]
        public string equipmentName = "Refrigerated Centrifuge 5424 R";
        public EquipmentType equipmentType = EquipmentType.RefrigeratedCentrifuge;
        public string equipmentId = "EQ-CENT-01";

        [Header("Physical State")]
        public EquipmentState currentState = EquipmentState.Idle;
        public float interactionRadius = 2.2f;

        [Header("Parameters")]
        public float targetRpm = 14000f;
        public float currentRpm = 0f;
        public float targetTemp = 4.0f;
        public float currentTemp = 4.0f;
        public float durationSeconds = 300f;
        public float remainingSeconds = 0f;

        [Header("Visual Elements")]
        public Transform movingPart; // e.g. Centrifuge Lid or BSC Sash
        public Transform rotorTransform; // Centrifuge Rotor
        public Renderer outlineRenderer;
        public Material outlineMaterial;

        // Events
        public event Action<LabEquipment, EquipmentState> OnStateChanged;

        private bool _isPlayerNearby;
        private Transform _playerTransform;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void Update()
        {
            CheckPlayerProximity();
            UpdatePhysicsSimulation(Time.deltaTime);
        }

        private void CheckPlayerProximity()
        {
            if (_playerTransform == null) return;

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            bool wasNearby = _isPlayerNearby;
            _isPlayerNearby = (dist <= interactionRadius);

            if (_isPlayerNearby != wasNearby)
            {
                HighlightEquipment(_isPlayerNearby);
            }

            if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }

        private void HighlightEquipment(bool highlight)
        {
            // Toggle outline highlight
            if (outlineRenderer != null)
            {
                // Highlight logic (e.g. outline intensity property)
            }
        }

        public void Interact()
        {
            switch (currentState)
            {
                case EquipmentState.Idle:
                    SetState(EquipmentState.LidOpen);
                    break;
                case EquipmentState.LidOpen:
                    SetState(EquipmentState.Running);
                    remainingSeconds = durationSeconds;
                    break;
                case EquipmentState.Running:
                    // Stop or cancel
                    SetState(EquipmentState.Idle);
                    break;
                case EquipmentState.Complete:
                    SetState(EquipmentState.Idle);
                    break;
            }
        }

        public void SetState(EquipmentState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(this, newState);
        }

        private void UpdatePhysicsSimulation(float dt)
        {
            if (currentState == EquipmentState.Running)
            {
                // Ramp up RPM
                currentRpm = Mathf.MoveTowards(currentRpm, targetRpm, 4000f * dt);

                // Spin rotor
                if (rotorTransform != null)
                {
                    rotorTransform.Rotate(Vector3.up, currentRpm * 6f * dt, Space.Self);
                }

                // Countdown
                if (remainingSeconds > 0f)
                {
                    remainingSeconds -= dt;
                    if (remainingSeconds <= 0f)
                    {
                        remainingSeconds = 0f;
                        SetState(EquipmentState.Complete);
                    }
                }
            }
            else
            {
                // Ramp down RPM
                currentRpm = Mathf.MoveTowards(currentRpm, 0f, 6000f * dt);
                if (rotorTransform != null && currentRpm > 0.1f)
                {
                    rotorTransform.Rotate(Vector3.up, currentRpm * 6f * dt, Space.Self);
                }
            }

            // Animate Lid opening / closing
            if (movingPart != null)
            {
                float targetAngle = (currentState == EquipmentState.LidOpen) ? -65f : 0f;
                Quaternion targetRot = Quaternion.Euler(targetAngle, 0f, 0f);
                movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, targetRot, 6f * dt);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
