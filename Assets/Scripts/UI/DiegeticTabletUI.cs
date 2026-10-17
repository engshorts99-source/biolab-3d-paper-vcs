using UnityEngine;
using UnityEngine.UI;

namespace BioLab.UI
{
    /// <summary>
    /// Controls the in-game diegetic datapad/tablet interface (toggled via Tab key).
    /// Displays protocol steps, equipment telemetries, and sample barcodes.
    /// </summary>
    public class DiegeticTabletUI : MonoBehaviour
    {
        [Header("Tablet Presentation")]
        public GameObject tabletCanvasRoot;
        public KeyCode toggleKey = KeyCode.Tab;
        public bool isOpen = false;

        [Header("UI Text & Elements")]
        public Text protocolTitleText;
        public Text stepHeaderNumText;
        public Text stepDescriptionText;
        public Text sampleBatchText;
        public Text equipmentTargetText;
        public Button completeStepButton;

        private void Start()
        {
            if (tabletCanvasRoot != null)
                tabletCanvasRoot.SetActive(isOpen);

            if (completeStepButton != null)
            {
                completeStepButton.onClick.AddListener(OnCompleteStepClicked);
            }

            RefreshUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleTablet();
            }
        }

        public void ToggleTablet()
        {
            isOpen = !isOpen;
            if (tabletCanvasRoot != null)
                tabletCanvasRoot.SetActive(isOpen);

            // Unlock cursor when tablet is open so researcher can interact with protocols
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;

            if (isOpen)
            {
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            var pm = Protocols.ProtocolManager.Instance;
            if (pm == null) return;

            if (protocolTitleText != null)
                protocolTitleText.text = pm.protocolTitle;

            if (sampleBatchText != null)
                sampleBatchText.text = "Sample: " + pm.sampleBatchId;

            var step = pm.GetCurrentStep();
            if (step != null)
            {
                if (stepHeaderNumText != null)
                    stepHeaderNumText.text = $"STEP {step.stepNumber}: {step.title}";

                if (stepDescriptionText != null)
                    stepDescriptionText.text = step.description;

                if (equipmentTargetText != null)
                    equipmentTargetText.text = $"Target Station: {step.targetEquipmentId}";
            }
            else
            {
                if (stepHeaderNumText != null)
                    stepHeaderNumText.text = "PROTOCOL COMPLETED";

                if (stepDescriptionText != null)
                    stepDescriptionText.text = "All procedures verified and recorded into LIMS database.";
            }
        }

        private void OnCompleteStepClicked()
        {
            var pm = Protocols.ProtocolManager.Instance;
            if (pm != null)
            {
                pm.CompleteCurrentStep();
                RefreshUI();
            }
        }
    }
}
