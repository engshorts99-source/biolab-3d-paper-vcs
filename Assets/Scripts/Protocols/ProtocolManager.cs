using System;
using System.Collections.Generic;
using UnityEngine;
using BioLab.Equipment;
using BioLab.Warehouse;

namespace BioLab.Protocols
{
    [Serializable]
    public class ProtocolStep
    {
        public int stepNumber;
        public string title;
        public string description;
        public string targetEquipmentId;   // e.g. WAREHOUSE-01, BSC-01, EQ-CENT-01, EQ-PCR-01, FREEZER-ULT-01, PAPER-DESK-01
        public string linkedReagentId;     // e.g. RG-DMEM-01
        public float reagentVolumeUsed;    // e.g. 15.0
        public string reagentUnit;         // e.g. mL or uL
        public string parameterSummary;    // e.g. "14,000 RPM, 4°C, 3 min"
        public string recordedLotNumber;   // Auto-stamped upon completion from Warehouse
        public string completedTimestamp;
        public bool isCompleted;
        public float requiredTimeSeconds;
    }

    [Serializable]
    public class ProtocolDefinition
    {
        public string protocolId;
        public string version;
        public string title;
        public string category;
        public string sampleBatchId;
        public string author;
        public List<ProtocolStep> steps = new List<ProtocolStep>();
    }

    /// <summary>
    /// Manages SOP protocol library, custom protocol authoring, 3D station step verification,
    /// automatic reagent warehouse stock deduction, and 3D floor laser path guidance.
    /// </summary>
    public class ProtocolManager : MonoBehaviour
    {
        public static ProtocolManager Instance { get; private set; }

        [Header("Active Protocol Summary")]
        public string protocolId = "PRT-2026-001";
        public string protocolVersion = "v2.1";
        public string protocolTitle = "NLRP6 & NOD2 Inflammasome Kinetic & qPCR Assay SOP";
        public string sampleBatchId = "BATCH-2026-NLRP6-09";
        public int currentStepIndex = 0;

        public List<ProtocolDefinition> protocolLibrary = new List<ProtocolDefinition>();
        public List<ProtocolStep> steps = new List<ProtocolStep>();

        public event Action<ProtocolStep> OnStepCompleted;
        public event Action<ProtocolStep> OnStepAdvanced;
        public event Action<ProtocolDefinition> OnProtocolChanged;

        // 3D Floor Navigation Laser Line
        private LineRenderer _guideLine;
        private Transform _playerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            InitializeProtocolLibrary();
            SelectProtocol(0);
            SetupGuideLineRenderer();
        }

        private void OnEnable()
        {
            LabEquipment.OnAnyEquipmentInteracted += HandleEquipmentInteracted;
        }

        private void OnDisable()
        {
            LabEquipment.OnAnyEquipmentInteracted -= HandleEquipmentInteracted;
        }

        private void SetupGuideLineRenderer()
        {
            GameObject lineObj = new GameObject("Protocol_3D_Floor_Laser_Guide");
            lineObj.transform.SetParent(transform, false);
            _guideLine = lineObj.AddComponent<LineRenderer>();
            _guideLine.positionCount = 2;
            _guideLine.startWidth = 0.08f;
            _guideLine.endWidth = 0.08f;
            _guideLine.useWorldSpace = true;

            Shader s = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Diffuse");
            Material lineMat = new Material(s);
            lineMat.color = new Color(0.0f, 0.95f, 1.0f, 0.85f);
            _guideLine.material = lineMat;
            _guideLine.startColor = new Color(0.0f, 0.95f, 1.0f, 0.90f);
            _guideLine.endColor = new Color(0.15f, 1.0f, 0.55f, 0.90f);
        }

        private void Update()
        {
            UpdateFloorGuideLine();
        }

        private void UpdateFloorGuideLine()
        {
            if (_guideLine == null) return;

            if (_playerTransform == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _playerTransform = p.transform;
            }

            var currentStep = GetCurrentStep();
            if (_playerTransform == null || currentStep == null)
            {
                _guideLine.enabled = false;
                return;
            }

            var targetEq = LabEquipment.FindById(currentStep.targetEquipmentId);
            if (targetEq == null)
            {
                _guideLine.enabled = false;
                return;
            }

            _guideLine.enabled = true;
            Vector3 startPos = new Vector3(_playerTransform.position.x, 0.04f, _playerTransform.position.z);
            Vector3 endPos = new Vector3(targetEq.transform.position.x, 0.04f, targetEq.transform.position.z);
            _guideLine.SetPosition(0, startPos);
            _guideLine.SetPosition(1, endPos);
        }

        private void HandleEquipmentInteracted(LabEquipment equipment)
        {
            var step = GetCurrentStep();
            if (step != null && equipment != null &&
                string.Equals(step.targetEquipmentId, equipment.equipmentId, StringComparison.OrdinalIgnoreCase))
            {
                CompleteCurrentStep();
            }
        }

        public void InitializeProtocolLibrary()
        {
            protocolLibrary.Clear();

            // Protocol 1: NLRP6 & NOD2 Kinetic + qPCR SOP (Integrated across all 3 Zones)
            var prt1 = new ProtocolDefinition
            {
                protocolId = "PRT-2026-001",
                version = "v2.1",
                title = "NLRP6 & NOD2 Inflammasome Kinetic & qPCR Assay SOP",
                category = "Innate Immunity / qPCR",
                sampleBatchId = "BATCH-2026-NLRP6-09",
                author = "Dr. J. Park",
                steps = new List<ProtocolStep>
                {
                    new ProtocolStep
                    {
                        stepNumber = 1,
                        title = "Retrieve DMEM & Antibody Aliquots from Warehouse",
                        description = "Scan and dispense 25 mL DMEM High Glucose (4°C Rack) and 20 uL Anti-NLRP6 mAb from Zone 1 Warehouse.",
                        targetEquipmentId = "WAREHOUSE-01",
                        linkedReagentId = "RG-DMEM-01",
                        reagentVolumeUsed = 25f,
                        reagentUnit = "mL",
                        parameterSummary = "4°C Cold Chain Verified | 25 mL Dispensed",
                        isCompleted = false,
                        requiredTimeSeconds = 45f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 2,
                        title = "Aseptic Cell Seeding & PR8/Ligand Stimulation in BSC",
                        description = "In Class II Biosafety Cabinet (BSC-01), seed HEK293T-NLRP6/NOD2 cells at 2x10^5 cells/well with 10% FBS.",
                        targetEquipmentId = "BSC-01",
                        linkedReagentId = "RG-FBS-02",
                        reagentVolumeUsed = 5f,
                        reagentUnit = "mL",
                        parameterSummary = "Laminar Flow 0.53 m/s | 37°C 5% CO2",
                        isCompleted = false,
                        requiredTimeSeconds = 90f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 3,
                        title = "High-Speed Refrigerated Clarification (14,000 RPM)",
                        description = "Pellet cellular debris and clarify lysate in Centrifuge 5424 R at 14,000 RPM (18,400 x g) for 5 min at 4°C.",
                        targetEquipmentId = "EQ-CENT-01",
                        linkedReagentId = "RG-LYSIS-03",
                        reagentVolumeUsed = 2.5f,
                        reagentUnit = "mL",
                        parameterSummary = "14,000 RPM (18,400 x g) | 4.0°C | 5 min",
                        isCompleted = false,
                        requiredTimeSeconds = 180f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 4,
                        title = "Automated 96-Well qPCR Amplification & Ct Quantification",
                        description = "Load 96-well plate on Automated Liquid Handler & run 40-cycle SYBR Green qPCR amplification.",
                        targetEquipmentId = "EQ-PCR-01",
                        linkedReagentId = "RG-SYBR-04",
                        reagentVolumeUsed = 250f,
                        reagentUnit = "uL",
                        parameterSummary = "40 Cycles (95°C 15s / 60°C 60s) | SYBR 2X",
                        isCompleted = false,
                        requiredTimeSeconds = 240f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 5,
                        title = "Archive Supernatant in -80°C Ultra-Low Cryo Vault",
                        description = "Store remaining clarified RNA/protein aliquots in Zone 1 -80°C Freezer Rack A3 for revision reproducibility.",
                        targetEquipmentId = "FREEZER-ULT-01",
                        linkedReagentId = "RG-AB-NLRP6",
                        reagentVolumeUsed = 15f,
                        reagentUnit = "uL",
                        parameterSummary = "-80.0°C Cryo Rack A3 | Barcode Locked",
                        isCompleted = false,
                        requiredTimeSeconds = 30f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 6,
                        title = "Commit Methods & Lot Provenance to PaperGit Desk",
                        description = "At Zone 3 Dry-Lab Manuscript Command Desk, sync protocol parameters & Reagent Lots into a new PaperGit commit.",
                        targetEquipmentId = "PAPER-DESK-01",
                        linkedReagentId = "RG-ETOH-06",
                        reagentVolumeUsed = 5f,
                        reagentUnit = "mL",
                        parameterSummary = "Git Auto-Sync Methods & Lot Table",
                        isCompleted = false,
                        requiredTimeSeconds = 30f
                    }
                }
            };
            protocolLibrary.Add(prt1);

            // Protocol 2: Standard Plasmid Miniprep SOP
            var prt2 = new ProtocolDefinition
            {
                protocolId = "PRT-2026-002",
                version = "v1.4",
                title = "High-Purity Endotoxin-Free Plasmid DNA Miniprep SOP",
                category = "Molecular Cloning",
                sampleBatchId = "BATCH-2026-DNA-042",
                author = "Dr. S. Kim",
                steps = new List<ProtocolStep>
                {
                    new ProtocolStep
                    {
                        stepNumber = 1,
                        title = "Dispense QIAGEN Alkaline Lysis Buffers (P1/P2/N3)",
                        description = "Check out RNase-verified P1, P2, and N3 buffers from Zone 1 Smart Reagent Warehouse.",
                        targetEquipmentId = "WAREHOUSE-01",
                        linkedReagentId = "RG-LYSIS-03",
                        reagentVolumeUsed = 10f,
                        reagentUnit = "mL",
                        parameterSummary = "Buffer P1 (4°C) + P2/N3 Checked",
                        isCompleted = false,
                        requiredTimeSeconds = 40f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 2,
                        title = "Alkaline Lysis & Neutralization in BSC-01",
                        description = "Resuspend bacterial pellet in 250 uL P1, lyse with 250 uL P2 for 4 min, neutralize with 350 uL N3.",
                        targetEquipmentId = "BSC-01",
                        linkedReagentId = "RG-ETOH-06",
                        reagentVolumeUsed = 15f,
                        reagentUnit = "mL",
                        parameterSummary = "250 uL P1 / 250 uL P2 / 350 uL N3",
                        isCompleted = false,
                        requiredTimeSeconds = 120f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 3,
                        title = "Spin Column Clarification & Elution (14,000 RPM)",
                        description = "Centrifuge spin column at 14,000 RPM for 10 min at 4°C, wash with PE buffer, elute in 50 uL EB.",
                        targetEquipmentId = "EQ-CENT-01",
                        linkedReagentId = "RG-ETOH-06",
                        reagentVolumeUsed = 5f,
                        reagentUnit = "mL",
                        parameterSummary = "14,000 RPM | 10 min | A260/280 = 1.88",
                        isCompleted = false,
                        requiredTimeSeconds = 300f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 4,
                        title = "Cryogenic Archiving (-80°C Vault)",
                        description = "Transfer purified plasmid DNA into labeled cryovial in -80°C Ultra-Low Freezer.",
                        targetEquipmentId = "FREEZER-ULT-01",
                        linkedReagentId = "RG-FBS-02",
                        reagentVolumeUsed = 1f,
                        reagentUnit = "mL",
                        parameterSummary = "-80.0°C Box B1 | 420 ng/uL",
                        isCompleted = false,
                        requiredTimeSeconds = 30f
                    }
                }
            };
            protocolLibrary.Add(prt2);
        }

        public void SelectProtocol(int index)
        {
            if (index < 0 || index >= protocolLibrary.Count) return;
            var def = protocolLibrary[index];
            protocolId = def.protocolId;
            protocolVersion = def.version;
            protocolTitle = def.title;
            sampleBatchId = def.sampleBatchId;
            steps = def.steps;

            // Find first incomplete step
            currentStepIndex = 0;
            for (int i = 0; i < steps.Count; i++)
            {
                if (!steps[i].isCompleted)
                {
                    currentStepIndex = i;
                    break;
                }
                if (i == steps.Count - 1) currentStepIndex = steps.Count;
            }

            OnProtocolChanged?.Invoke(def);
        }

        public ProtocolDefinition GetActiveProtocolDefinition()
        {
            return protocolLibrary.Find(p => p.protocolId == protocolId) ?? (protocolLibrary.Count > 0 ? protocolLibrary[0] : null);
        }

        public ProtocolStep GetCurrentStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
                return steps[currentStepIndex];
            return null;
        }

        public void CompleteCurrentStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
            {
                var step = steps[currentStepIndex];
                step.isCompleted = true;
                step.completedTimestamp = DateTime.Now.ToString("HH:mm:ss");

                // Automatically consume linked reagent from Zone 1 Warehouse & stamp Lot Number
                if (ReagentWarehouseManager.Instance != null && !string.IsNullOrEmpty(step.linkedReagentId))
                {
                    var rg = ReagentWarehouseManager.Instance.GetReagentById(step.linkedReagentId);
                    if (rg != null)
                    {
                        step.recordedLotNumber = rg.lotNumber;
                        ReagentWarehouseManager.Instance.ConsumeReagent(
                            step.linkedReagentId,
                            Mathf.Max(0.5f, step.reagentVolumeUsed),
                            protocolId,
                            step.title);
                    }
                }

                // Trigger physical animation on the 3D station if idle
                var eq = LabEquipment.FindById(step.targetEquipmentId);
                if (eq != null && eq.currentState == EquipmentState.Idle)
                {
                    eq.SetState(EquipmentState.Running);
                    eq.remainingSeconds = 12f;
                }

                OnStepCompleted?.Invoke(step);

                currentStepIndex++;
                if (currentStepIndex < steps.Count)
                {
                    OnStepAdvanced?.Invoke(steps[currentStepIndex]);
                }
            }
        }

        public void ResetCurrentProtocolSteps()
        {
            foreach (var s in steps)
            {
                s.isCompleted = false;
                s.completedTimestamp = "";
            }
            currentStepIndex = 0;
            var def = GetActiveProtocolDefinition();
            if (def != null) OnProtocolChanged?.Invoke(def);
        }

        public void AddStepToActiveProtocol(string stepTitle, string stepDesc, string targetStationId, string reagentId, float vol, string unit, string paramSummary)
        {
            var def = GetActiveProtocolDefinition();
            if (def == null) return;

            var newStep = new ProtocolStep
            {
                stepNumber = def.steps.Count + 1,
                title = string.IsNullOrWhiteSpace(stepTitle) ? "Custom Verification Step" : stepTitle,
                description = string.IsNullOrWhiteSpace(stepDesc) ? "Execute experimental procedure and verify parameters." : stepDesc,
                targetEquipmentId = string.IsNullOrWhiteSpace(targetStationId) ? "BSC-01" : targetStationId,
                linkedReagentId = string.IsNullOrWhiteSpace(reagentId) ? "RG-DMEM-01" : reagentId,
                reagentVolumeUsed = vol > 0f ? vol : 5f,
                reagentUnit = string.IsNullOrWhiteSpace(unit) ? "mL" : unit,
                parameterSummary = string.IsNullOrWhiteSpace(paramSummary) ? "37°C | Verified" : paramSummary,
                isCompleted = false,
                requiredTimeSeconds = 60f
            };

            def.steps.Add(newStep);
            // Bump minor version automatically when protocol steps are modified
            def.version = $"v2.{def.steps.Count}";
            protocolVersion = def.version;
            OnProtocolChanged?.Invoke(def);
        }

        public ProtocolDefinition CreateNewProtocol(string newTitle, string newVersion, string batchId, string authorName)
        {
            var newDef = new ProtocolDefinition
            {
                protocolId = $"PRT-2026-{protocolLibrary.Count + 1:D3}",
                version = string.IsNullOrWhiteSpace(newVersion) ? "v1.0" : newVersion,
                title = string.IsNullOrWhiteSpace(newTitle) ? "Custom Experimental Protocol SOP" : newTitle,
                category = "Custom Protocol",
                sampleBatchId = string.IsNullOrWhiteSpace(batchId) ? $"BATCH-2026-{protocolLibrary.Count + 10}" : batchId,
                author = string.IsNullOrWhiteSpace(authorName) ? "Dr. Researcher" : authorName,
                steps = new List<ProtocolStep>
                {
                    new ProtocolStep
                    {
                        stepNumber = 1,
                        title = "Dispense Reagents from Zone 1 Warehouse",
                        description = "Scan reagent barcode at Zone 1 Kiosk and verify Lot number before bringing to Wet-Lab.",
                        targetEquipmentId = "WAREHOUSE-01",
                        linkedReagentId = "RG-DMEM-01",
                        reagentVolumeUsed = 10f,
                        reagentUnit = "mL",
                        parameterSummary = "4°C Cold Storage | Lot Verified",
                        isCompleted = false,
                        requiredTimeSeconds = 45f
                    },
                    new ProtocolStep
                    {
                        stepNumber = 2,
                        title = "Execute Reaction in Biosafety Cabinet (BSC-01)",
                        description = "Perform sterile reaction setup inside ISO Class 5 laminar flow hood.",
                        targetEquipmentId = "BSC-01",
                        linkedReagentId = "RG-SYBR-04",
                        reagentVolumeUsed = 100f,
                        reagentUnit = "uL",
                        parameterSummary = "ISO Class 5 | Sterile Technique",
                        isCompleted = false,
                        requiredTimeSeconds = 90f
                    }
                }
            };

            protocolLibrary.Add(newDef);
            SelectProtocol(protocolLibrary.Count - 1);
            return newDef;
        }
    }
}

