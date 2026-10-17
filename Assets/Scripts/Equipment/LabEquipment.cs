using System;
using System.Collections.Generic;
using UnityEngine;

namespace BioLab.Equipment
{
    public enum EquipmentType
    {
        RefrigeratedCentrifuge,  // Zone 2: 5424 R
        BiosafetyCabinet,        // Zone 2: BSC Class II A2
        UltraLowFreezer,         // Zone 1: -80°C Freezer
        ReagentWarehouseRack,    // Zone 1: Smart Reagent Shelving & Kiosk
        CryoDewarTank,           // Zone 1: LN2 Cryo Tank & 4°C Fridge
        PcrThermalCycler,        // Zone 2: qPCR & Gel Station
        LiquidHandlerRobot,      // Zone 2: Automated Pipetting Robot
        ManuscriptCommandDesk,   // Zone 3: Dry-Lab PaperGit Workstation
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
    /// Interactive laboratory equipment & station controller across the 3-Zone Research Facility.
    /// Handles player proximity, hover highlights, rotor/gantry physics, 3D beacon indicators, and LabOS triggers.
    /// </summary>
    public class LabEquipment : MonoBehaviour
    {
        public static readonly List<LabEquipment> AllEquipment = new List<LabEquipment>();
        public static event Action<LabEquipment> OnAnyEquipmentInteracted;
        public static event Action<LabEquipment> OnNearestEquipmentChanged;
        public static LabEquipment NearestActiveEquipment { get; private set; }

        [Header("Equipment Identity")]
        public string equipmentName = "Refrigerated Centrifuge 5424 R";
        public EquipmentType equipmentType = EquipmentType.RefrigeratedCentrifuge;
        public string equipmentId = "EQ-CENT-01";
        [NonSerialized] public string zoneName = "Zone 2: Wet-Lab Protocol Bay";
        [NonSerialized] public string quickHint = "Press [E] to Operate Station";

        [Header("Physical State")]
        public EquipmentState currentState = EquipmentState.Idle;
        public float interactionRadius = 2.6f;

        [Header("Parameters")]
        public float targetRpm = 14000f;
        public float currentRpm = 0f;
        public float targetTemp = 4.0f;
        public float currentTemp = 4.0f;
        public float durationSeconds = 45f;
        public float remainingSeconds = 0f;

        [Header("Visual Elements")]
        public Transform movingPart;     // Lid, Door, Sash, or Gantry Arm
        public Transform rotorTransform; // Centrifuge Rotor or Stirrer
        [NonSerialized] public Transform beaconTransform;// Floating 3D indicator ring above station
        public Renderer outlineRenderer;
        public Material outlineMaterial;

        // Events
        public event Action<LabEquipment, EquipmentState> OnStateChanged;

        private bool _isPlayerNearby;
        private Transform _playerTransform;

        public bool IsPlayerNearby => _isPlayerNearby;

        private void OnEnable()
        {
            if (!AllEquipment.Contains(this))
            {
                AllEquipment.Add(this);
            }
        }

        private void OnDestroy()
        {
            AllEquipment.Remove(this);
            if (NearestActiveEquipment == this)
            {
                NearestActiveEquipment = null;
            }
        }

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
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _playerTransform = player.transform;
            }

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
                if (_isPlayerNearby)
                {
                    NearestActiveEquipment = this;
                    OnNearestEquipmentChanged?.Invoke(this);
                }
                else if (NearestActiveEquipment == this)
                {
                    NearestActiveEquipment = null;
                    OnNearestEquipmentChanged?.Invoke(null);
                }
            }

            if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }

        private void HighlightEquipment(bool highlight)
        {
            if (beaconTransform != null)
            {
                beaconTransform.localScale = highlight ? new Vector3(0.55f, 0.03f, 0.55f) : new Vector3(0.38f, 0.02f, 0.38f);
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
                    SetState(EquipmentState.Complete);
                    remainingSeconds = 0f;
                    break;
                case EquipmentState.Complete:
                    SetState(EquipmentState.Idle);
                    break;
            }

            OnAnyEquipmentInteracted?.Invoke(this);
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
                currentRpm = Mathf.MoveTowards(currentRpm, targetRpm, 4000f * dt);

                if (rotorTransform != null)
                {
                    rotorTransform.Rotate(Vector3.up, currentRpm * 6f * dt, Space.Self);
                }

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
                currentRpm = Mathf.MoveTowards(currentRpm, 0f, 6000f * dt);
                if (rotorTransform != null && currentRpm > 0.1f)
                {
                    rotorTransform.Rotate(Vector3.up, currentRpm * 6f * dt, Space.Self);
                }
            }

            // Animate Lid / Door / Sash / Robot Gantry
            if (movingPart != null)
            {
                if (equipmentType == EquipmentType.UltraLowFreezer || equipmentType == EquipmentType.CryoDewarTank)
                {
                    float targetYaw = (currentState == EquipmentState.LidOpen || currentState == EquipmentState.Running) ? -75f : 0f;
                    Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
                    movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, targetRot, 5f * dt);
                }
                else if (equipmentType == EquipmentType.BiosafetyCabinet)
                {
                    float targetY = (currentState == EquipmentState.LidOpen || currentState == EquipmentState.Running) ? 1.56f : 1.35f;
                    Vector3 pos = movingPart.localPosition;
                    pos.y = Mathf.Lerp(pos.y, targetY, 6f * dt);
                    movingPart.localPosition = pos;
                }
                else if (equipmentType == EquipmentType.LiquidHandlerRobot)
                {
                    if (currentState == EquipmentState.Running)
                    {
                        Vector3 pos = movingPart.localPosition;
                        pos.x = Mathf.Sin(Time.time * 3.2f) * 0.32f;
                        movingPart.localPosition = pos;
                    }
                }
                else if (equipmentType == EquipmentType.RefrigeratedCentrifuge || equipmentType == EquipmentType.PcrThermalCycler)
                {
                    float targetAngle = (currentState == EquipmentState.LidOpen) ? -65f : 0f;
                    Quaternion targetRot = Quaternion.Euler(targetAngle, 0f, 0f);
                    movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, targetRot, 6f * dt);
                }
            }

            if (beaconTransform != null)
            {
                beaconTransform.Rotate(Vector3.up, 65f * dt, Space.World);
            }
        }

        public static LabEquipment FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var eq in AllEquipment)
            {
                if (eq != null && string.Equals(eq.equipmentId, id, StringComparison.OrdinalIgnoreCase))
                    return eq;
            }
            return null;
        }
    }
}

