using System;
using System.Collections.Generic;
using UnityEngine;

namespace BioLab.Protocols
{
    [Serializable]
    public class ProtocolStep
    {
        public int stepNumber;
        public string title;
        public string description;
        public string targetEquipmentId;
        public bool isCompleted;
        public float requiredTimeSeconds;
    }

    /// <summary>
    /// Manages SOP protocol workflows, step verification, and sample state transitions.
    /// </summary>
    public class ProtocolManager : MonoBehaviour
    {
        public static ProtocolManager Instance { get; private set; }

        [Header("Active Protocol")]
        public string protocolTitle = "Standard Plasmid DNA Miniprep SOP";
        public string sampleBatchId = "BATCH-2026-DNA-042";
        public int currentStepIndex = 0;

        [SerializeField]
        public List<ProtocolStep> steps = new List<ProtocolStep>();

        public event Action<ProtocolStep> OnStepCompleted;
        public event Action<ProtocolStep> OnStepAdvanced;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeDefaultProtocol();
        }

        private void InitializeDefaultProtocol()
        {
            steps.Clear();
            steps.Add(new ProtocolStep
            {
                stepNumber = 1,
                title = "Harvest Bacterial Culture",
                description = "Transfer 1.5 mL overnight E. coli culture into a 1.5 mL microcentrifuge tube.",
                targetEquipmentId = "BSC-01",
                isCompleted = false,
                requiredTimeSeconds = 60f
            });

            steps.Add(new ProtocolStep
            {
                stepNumber = 2,
                title = "Pellet Cells by Centrifugation",
                description = "Centrifuge at 14,000 RPM (approx. 18,000 x g) for 3 minutes at 4°C to pellet bacteria.",
                targetEquipmentId = "EQ-CENT-01",
                isCompleted = false,
                requiredTimeSeconds = 180f
            });

            steps.Add(new ProtocolStep
            {
                stepNumber = 3,
                title = "Aspirate Supernatant & Resuspend",
                description = "Discard culture medium supernatant completely. Resuspend pellet in 250 µL Resuspension Buffer (P1) with RNase A.",
                targetEquipmentId = "BSC-01",
                isCompleted = false,
                requiredTimeSeconds = 90f
            });

            steps.Add(new ProtocolStep
            {
                stepNumber = 4,
                title = "Alkaline Lysis & Neutralization",
                description = "Add 250 µL Lysis Buffer (P2), invert gently 4-6 times. Add 350 µL Neutralization Buffer (N3) and invert immediately.",
                targetEquipmentId = "BSC-01",
                isCompleted = false,
                requiredTimeSeconds = 120f
            });

            steps.Add(new ProtocolStep
            {
                stepNumber = 5,
                title = "Clarify Lysate via Centrifugation",
                description = "Centrifuge at 14,000 RPM for 10 minutes at 4°C to precipitate genomic DNA and SDS-protein complexes.",
                targetEquipmentId = "EQ-CENT-01",
                isCompleted = false,
                requiredTimeSeconds = 600f
            });

            steps.Add(new ProtocolStep
            {
                stepNumber = 6,
                title = "Cryogenic Storage",
                description = "Transfer purified plasmid DNA aliquot into labeled cryogenic vial and place into -80°C Ultra-Low Freezer rack A3.",
                targetEquipmentId = "FREEZER-ULT-01",
                isCompleted = false,
                requiredTimeSeconds = 30f
            });
        }

        public ProtocolStep GetCurrentStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
                return steps[currentStepIndex];
            return null;
        }

        public void CompleteCurrentStep()
        {
            if (currentStepIndex < steps.Count)
            {
                steps[currentStepIndex].isCompleted = true;
                OnStepCompleted?.Invoke(steps[currentStepIndex]);

                currentStepIndex++;
                if (currentStepIndex < steps.Count)
                {
                    OnStepAdvanced?.Invoke(steps[currentStepIndex]);
                }
            }
        }
    }
}
