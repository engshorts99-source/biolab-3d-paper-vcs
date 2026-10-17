using UnityEngine;

namespace BioLab.Player
{
    public enum CharacterGender
    {
        Male,
        Female
    }

    /// <summary>
    /// Manages switching between Male and Female 3D researcher models,
    /// updating bone weights, collider sizes, and mass attributes seamlessly.
    /// </summary>
    public class CharacterSelector : MonoBehaviour
    {
        [Header("Character Models")]
        public GameObject maleModel;
        public GameObject femaleModel;

        [Header("Physical Attributes")]
        public float maleMass = 72f;
        public float maleHeight = 1.82f;
        public float maleRadius = 0.32f;

        public float femaleMass = 54f;
        public float femaleHeight = 1.68f;
        public float femaleRadius = 0.28f;

        [Header("Active Character")]
        public CharacterGender currentGender = CharacterGender.Male;

        private PlayerLocomotion _locomotion;

        private void Awake()
        {
            _locomotion = GetComponent<PlayerLocomotion>();

            ApplyCommandLineGender();
        }

        public void ApplyCommandLineGender()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-gender", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    if (args[i + 1].Equals("female", System.StringComparison.OrdinalIgnoreCase))
                    {
                        SetGender(CharacterGender.Female);
                        return;
                    }
                    else if (args[i + 1].Equals("male", System.StringComparison.OrdinalIgnoreCase))
                    {
                        SetGender(CharacterGender.Male);
                        return;
                    }
                }
            }
            SetGender(currentGender);
        }

        private void Update()
        {
            // Hotkey toggle: F1 for Male, F2 for Female, or Tab / C in character select menu
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.F1))
            {
                SetGender(CharacterGender.Male);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.F2))
            {
                SetGender(CharacterGender.Female);
            }
        }

        public void SetGender(CharacterGender gender)
        {
            currentGender = gender;

            if (maleModel != null)
                maleModel.SetActive(gender == CharacterGender.Male);

            if (femaleModel != null)
                femaleModel.SetActive(gender == CharacterGender.Female);

            if (_locomotion != null)
            {
                if (gender == CharacterGender.Male)
                {
                    if (maleModel != null) _locomotion.characterModelRoot = maleModel.transform;
                    _locomotion.ApplyCharacterSpecs(maleMass, maleHeight, maleRadius);
                }
                else
                {
                    if (femaleModel != null) _locomotion.characterModelRoot = femaleModel.transform;
                    _locomotion.ApplyCharacterSpecs(femaleMass, femaleHeight, femaleRadius);
                }
            }
        }
    }
}
