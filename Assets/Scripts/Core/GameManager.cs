using UnityEngine;
using BioLab.Player;
using BioLab.Equipment;
using BioLab.Environment;
using BioLab.UI;
using BioLab.Warehouse;
using BioLab.PaperVCS;

namespace BioLab.Core
{
    /// <summary>
    /// Master game entrypoint. Orchestrates procedural high-resolution textures,
    /// 3-Zone integrated cleanroom architecture (Reagent Warehouse, Wet-Lab Bench, Dry-Lab PaperGit Office),
    /// sculpted anime researcher characters, realistic physics locomotion, and LabOS PaperGit VCS.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Global Shaders")]
        public Shader toonShader;
        public Shader faceShader;

        // Procedural Textures
        private Texture2D _floorTex;
        private Texture2D _wallTex;
        private Texture2D _faceMaleTex;
        private Texture2D _faceFemaleTex;
        private Texture2D _centrifugeDisplayTex;
        private Texture2D _freezerDisplayTex;
        private Texture2D _paperGitMonitorTex;
        private Texture2D _reagentKioskTex;
        private Texture2D _pcrDisplayTex;
        private Texture2D _holoDiagTex;

        // Character Materials
        private Material _labCoatMaterial;
        private Material _coatAccentMaterial;
        private Material _suitMaterial;
        private Material _skinMaterial;
        private Material _faceMaleMaterial;
        private Material _faceFemaleMaterial;
        private Material _hairMaleMaterial;
        private Material _hairFemaleMaterial;
        private Material _glassMaterial;
        private Material _bootMaterial;
        private Material _gloveMaterial;
        private Material _cyanGlowMaterial;

        // Environment & Equipment Materials
        private Material _floorMaterial;
        private Material _wallMaterial;
        private Material _baseboardMaterial;
        private Material _metalMaterial;
        private Material _benchTopMaterial;
        private Material _equipmentBodyMat;
        private Material _equipmentAccentMat;

        private GameObject _playerRoot;
        private CharacterSelector _characterSelector;
        private ThirdPersonCamera _thirdPersonCamera;

        private void Awake()
        {
            // Enable high-fidelity anti-aliasing and smooth edges
            QualitySettings.antiAliasing = 8;
            QualitySettings.vSyncCount = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

            GenerateTextures();
            InitializeMaterials();
            SetupCleanroom();
            SetupCamera();
            SetupCharacters();
            SetupTabletUI();
        }

        private void GenerateTextures()
        {
            _floorTex = LabTextureGenerator.CreateFloorTexture();
            _wallTex = LabTextureGenerator.CreateWallTexture();
            _faceMaleTex = LabTextureGenerator.CreateAnimeFaceTexture(false);
            _faceFemaleTex = LabTextureGenerator.CreateAnimeFaceTexture(true);
            _centrifugeDisplayTex = LabTextureGenerator.CreateCentrifugeDisplayTexture();
            _freezerDisplayTex = LabTextureGenerator.CreateFreezerDisplayTexture();
            _paperGitMonitorTex = LabTextureGenerator.CreatePaperGitMonitorTexture();
            _reagentKioskTex = LabTextureGenerator.CreateReagentKioskTexture();
            _pcrDisplayTex = LabTextureGenerator.CreatePcrAmplificationTexture();
            _holoDiagTex = LabTextureGenerator.CreateHoloDiagnosticTexture();
        }

        private void InitializeMaterials()
        {
            if (toonShader == null)
            {
                toonShader = Resources.Load<Shader>("GenshinToonNPR") 
                    ?? Shader.Find("Custom/GenshinToonNPR") 
                    ?? Shader.Find("Diffuse") 
                    ?? Shader.Find("Standard");
            }

            // Character Materials with Rich Cel-Shading Contrast
            _labCoatMaterial = CreateToonMat(new Color(0.98f, 0.99f, 1.0f), new Color(0.55f, 0.60f, 0.74f), 1.0f, null);
            if (_labCoatMaterial.HasProperty("_RimColor")) _labCoatMaterial.SetColor("_RimColor", new Color(0.0f, 0.90f, 1.0f));
            if (_labCoatMaterial.HasProperty("_RimPower")) _labCoatMaterial.SetFloat("_RimPower", 2.8f);

            _coatAccentMaterial = CreateToonMat(new Color(0.10f, 0.16f, 0.28f), new Color(0.04f, 0.08f, 0.16f), 1.0f, null);
            _suitMaterial = CreateToonMat(new Color(0.10f, 0.12f, 0.17f), new Color(0.04f, 0.05f, 0.08f), 0.9f, null);
            _skinMaterial = CreateToonMat(new Color(1.0f, 0.89f, 0.82f), new Color(0.85f, 0.68f, 0.64f), 0.8f, null);

            // Dedicated Anime Face Materials (uncompromised eye/blush/lip rendering)
            if (faceShader == null)
            {
                faceShader = Resources.Load<Shader>("GenshinFace")
                    ?? Shader.Find("Custom/GenshinFace") 
                    ?? Shader.Find("Unlit/Texture")
                    ?? Shader.Find("Diffuse");
            }
            _faceMaleMaterial = new Material(faceShader);
            _faceMaleMaterial.mainTexture = _faceMaleTex;
            _faceMaleMaterial.color = Color.white;
            if (_faceMaleMaterial.HasProperty("_ShadowTint")) _faceMaleMaterial.SetColor("_ShadowTint", new Color(0.92f, 0.86f, 0.88f));

            _faceFemaleMaterial = new Material(faceShader);
            _faceFemaleMaterial.mainTexture = _faceFemaleTex;
            _faceFemaleMaterial.color = Color.white;
            if (_faceFemaleMaterial.HasProperty("_ShadowTint")) _faceFemaleMaterial.SetColor("_ShadowTint", new Color(0.94f, 0.88f, 0.90f));

            _hairMaleMaterial = CreateToonMat(new Color(0.11f, 0.17f, 0.28f), new Color(0.05f, 0.08f, 0.15f), 1.2f, null);
            _hairFemaleMaterial = CreateToonMat(new Color(0.68f, 0.52f, 0.40f), new Color(0.42f, 0.30f, 0.22f), 1.2f, null);

            _glassMaterial = new Material(Shader.Find("Diffuse"));
            _glassMaterial.color = new Color(0.35f, 0.75f, 0.95f, 0.5f);

            _bootMaterial = CreateToonMat(new Color(0.92f, 0.94f, 0.97f), new Color(0.55f, 0.60f, 0.72f), 1.0f, null);
            _gloveMaterial = CreateToonMat(new Color(0.0f, 0.66f, 0.80f), new Color(0.0f, 0.32f, 0.50f), 1.0f, null);

            // Neon Glowing Cyan Accent Material (Suit seams, visor & holo displays)
            _cyanGlowMaterial = CreateToonMat(new Color(0.0f, 0.95f, 1.0f), new Color(0.0f, 0.5f, 0.7f), 0.8f, null);
            if (_cyanGlowMaterial.HasProperty("_EmissionColor"))
            {
                _cyanGlowMaterial.SetColor("_EmissionColor", new Color(0.0f, 0.95f, 1.0f) * 1.8f);
            }

            // High-Contrast Cinematic Cleanroom Architectural Materials
            _floorMaterial = CreateEnvMat(new Color(0.80f, 0.84f, 0.90f), _floorTex, 0.55f, 0.08f);
            _floorMaterial.mainTextureScale = new Vector2(4f, 5f);

            _wallMaterial = CreateEnvMat(new Color(0.90f, 0.93f, 0.96f), _wallTex, 0.25f, 0.02f);
            _wallMaterial.mainTextureScale = new Vector2(6f, 1.5f);

            _baseboardMaterial = CreateEnvMat(new Color(0.12f, 0.16f, 0.24f), null, 0.40f, 0.15f);
            _metalMaterial = CreateEnvMat(new Color(0.88f, 0.90f, 0.94f), null, 0.85f, 0.80f);
            _benchTopMaterial = CreateEnvMat(new Color(0.18f, 0.22f, 0.30f), null, 0.60f, 0.20f);

            _equipmentBodyMat = CreateEnvMat(new Color(0.96f, 0.97f, 0.99f), null, 0.50f, 0.05f);
            _equipmentAccentMat = CreateEnvMat(new Color(0.0f, 0.55f, 0.90f), null, 0.65f, 0.25f);
        }

        private Material CreateEnvMat(Color baseCol, Texture2D mainTex, float smoothness = 0.35f, float metallic = 0.05f)
        {
            Shader s = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material mat = new Material(s);
            mat.color = baseCol;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseCol);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mainTex != null)
            {
                mat.mainTexture = mainTex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
            }
            return mat;
        }

        private Material CreateToonMat(Color baseCol, Color shadowTint, float outlineWidth, Texture2D mainTex)
        {
            Material mat = null;
            if (toonShader != null) mat = new Material(toonShader);
            else mat = new Material(Shader.Find("Diffuse"));

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseCol);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseCol);
            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", shadowTint);
            if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", outlineWidth);
            if (mat.HasProperty("_OutlineColor")) mat.SetColor("_OutlineColor", new Color(0.12f, 0.14f, 0.20f, 1f));
            if (mat.HasProperty("_RimColor")) mat.SetColor("_RimColor", new Color(0.85f, 0.92f, 1.0f, 1f));
            if (mat.HasProperty("_RimPower")) mat.SetFloat("_RimPower", 3.2f);

            if (mainTex != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            }

            return mat;
        }

        private void SetupCleanroom()
        {
            GameObject labEnv = new GameObject("Cleanroom_Environment");
            var builder = labEnv.AddComponent<CleanroomBuilder>();
            builder.floorMaterial = _floorMaterial;
            builder.wallMaterial = _wallMaterial;
            builder.baseboardMaterial = _baseboardMaterial;
            builder.metalMaterial = _metalMaterial;
            builder.benchTopMaterial = _benchTopMaterial;
            builder.accentBlueMaterial = _equipmentAccentMat;
            builder.cyanGlowMaterial = _cyanGlowMaterial;

            // =======================================================================
            // ZONE 1 (WEST WING): SMART REAGENT WAREHOUSE & -80°C CRYO VAULT
            // =======================================================================
            var warehouseStation = LabModelGenerator.CreateReagentWarehouseStation(
                _equipmentBodyMat, _metalMaterial, _glassMaterial, _equipmentAccentMat, _cyanGlowMaterial, _reagentKioskTex);
            warehouseStation.transform.position = new Vector3(-8.8f, 0f, 2.2f);
            warehouseStation.transform.rotation = Quaternion.Euler(0, 90f, 0);

            var freezer = LabModelGenerator.CreateUltraLowFreezer(
                _equipmentBodyMat, _equipmentAccentMat, _metalMaterial, _freezerDisplayTex);
            freezer.transform.position = new Vector3(-8.8f, 0f, -3.6f);
            freezer.transform.rotation = Quaternion.Euler(0, 90f, 0);

            // =======================================================================
            // ZONE 2 (CENTER & NORTH BAY): WET-LAB PROTOCOL EXECUTION BAY
            // =======================================================================
            var bsc = LabModelGenerator.CreateBiosafetyCabinet(
                _equipmentBodyMat, _glassMaterial, _metalMaterial, _equipmentAccentMat);
            bsc.transform.position = new Vector3(0f, 0f, 9.2f);
            bsc.transform.rotation = Quaternion.Euler(0, 180f, 0);

            var centrifuge = LabModelGenerator.CreateCentrifuge5424R(
                _equipmentBodyMat, _equipmentAccentMat, _glassMaterial, _metalMaterial, _centrifugeDisplayTex);
            centrifuge.transform.position = new Vector3(-3.6f, 0.92f, 3.2f);
            centrifuge.transform.rotation = Quaternion.Euler(0, 90f, 0);

            var pcrAndRobot = LabModelGenerator.CreatePcrAndLiquidHandlerStation(
                _equipmentBodyMat, _equipmentAccentMat, _glassMaterial, _metalMaterial, _cyanGlowMaterial, _pcrDisplayTex);
            pcrAndRobot.transform.position = new Vector3(3.6f, 0.92f, 3.2f);
            pcrAndRobot.transform.rotation = Quaternion.Euler(0, 0f, 0);

            // =======================================================================
            // ZONE 3 (EAST WING): DRY-LAB MANUSCRIPT & PAPERGIT COMMAND OFFICE
            // =======================================================================
            var paperDesk = LabModelGenerator.CreateManuscriptCommandDesk(
                _benchTopMaterial, _metalMaterial, _equipmentBodyMat, _cyanGlowMaterial, _paperGitMonitorTex, _holoDiagTex);
            paperDesk.transform.position = new Vector3(8.6f, 0f, -1.0f);
            paperDesk.transform.rotation = Quaternion.Euler(0, -90f, 0);
        }

        private void SetupCharacters()
        {
            _playerRoot = new GameObject("Player_Researcher");
            _playerRoot.tag = "Player";
            _playerRoot.transform.position = new Vector3(0, 0.135f, -2.0f);

            var cc = _playerRoot.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.32f;
            cc.center = new Vector3(0, 0.9f, 0);

            var locomotion = _playerRoot.AddComponent<PlayerLocomotion>();

            // Build High-Fidelity Sculpted Male & Female Models matching concept benchmarks
            var maleModel = CharacterMeshBuilder.BuildMaleResearcher(
                _suitMaterial, _labCoatMaterial, _coatAccentMaterial, 
                _skinMaterial, _faceMaleMaterial, _hairMaleMaterial, _glassMaterial, _bootMaterial, _gloveMaterial, _cyanGlowMaterial);
            maleModel.transform.SetParent(_playerRoot.transform, false);

            var femaleModel = CharacterMeshBuilder.BuildFemaleResearcher(
                _suitMaterial, _labCoatMaterial, _coatAccentMaterial, 
                _skinMaterial, _faceFemaleMaterial, _hairFemaleMaterial, _glassMaterial, _bootMaterial, _gloveMaterial, _cyanGlowMaterial);
            femaleModel.transform.SetParent(_playerRoot.transform, false);
            femaleModel.SetActive(false);

            locomotion.characterModelRoot = maleModel.transform;

            _characterSelector = _playerRoot.AddComponent<CharacterSelector>();
            _characterSelector.maleModel = maleModel;
            _characterSelector.femaleModel = femaleModel;
            _characterSelector.ApplyCommandLineGender();

            locomotion.ResolveCamera();
            locomotion.ResolveAnimator();

            if (_thirdPersonCamera != null)
            {
                _thirdPersonCamera.target = _playerRoot.transform;
            }
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.11f, 0.16f); // Deep cinematic sci-fi slate atmosphere
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;

            // Key Directional Sunlight
            Light keyLight = FindAnyObjectByType<Light>();
            if (keyLight == null)
            {
                GameObject lightObj = new GameObject("Key_Cleanroom_Light");
                keyLight = lightObj.AddComponent<Light>();
                keyLight.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(48f, -28f, 0);
            }
            keyLight.color = new Color(1.0f, 0.98f, 0.95f);
            keyLight.intensity = 1.35f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.52f, 0.62f);

            _thirdPersonCamera = cam.GetComponent<ThirdPersonCamera>();
            if (_thirdPersonCamera == null)
            {
                _thirdPersonCamera = cam.gameObject.AddComponent<ThirdPersonCamera>();
            }
            if (_playerRoot != null)
            {
                _thirdPersonCamera.target = _playerRoot.transform;
            }
        }

        private void SetupTabletUI()
        {
            GameObject uiRoot = new GameObject("UI_LabOS_And_PaperGit_Managers");
            uiRoot.AddComponent<ReagentWarehouseManager>();
            uiRoot.AddComponent<Protocols.ProtocolManager>();
            uiRoot.AddComponent<PaperVersionControlManager>();
            uiRoot.AddComponent<DiegeticTabletUI>();
        }

        private void Start()
        {
            StartCoroutine(HandleCommandLineTestRoutine());
        }

        private System.Collections.IEnumerator HandleCommandLineTestRoutine()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string screenshotPath = null;
            string spawnZone = null;
            bool testLocomotion = false;
            float delay = 1.8f;
            bool quitAfter = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-testLocomotion", System.StringComparison.OrdinalIgnoreCase))
                {
                    testLocomotion = true;
                }
                else if (args[i].Equals("-spawnZone", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    spawnZone = args[i + 1];
                }
                else if (args[i].Equals("-screenshot", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    screenshotPath = args[i + 1];
                }
                else if (args[i].Equals("-delay", System.StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out delay);
                }
                else if (args[i].Equals("-quitAfterScreenshot", System.StringComparison.OrdinalIgnoreCase))
                {
                    quitAfter = true;
                }
            }

            if (!string.IsNullOrEmpty(spawnZone) && DiegeticTabletUI.Instance != null)
            {
                DiegeticTabletUI.Instance.TeleportPlayerToZone(spawnZone.ToUpperInvariant());
            }

            if (testLocomotion && _playerRoot != null)
            {
                var loc = _playerRoot.GetComponent<PlayerLocomotion>();
                if (loc != null) loc.autoRunTest = true;
            }

            if (!string.IsNullOrEmpty(screenshotPath))
            {
                yield return new WaitForSeconds(delay);
                ScreenCapture.CaptureScreenshot(screenshotPath);
                Debug.Log($"[GameManager] Captured screenshot to '{screenshotPath}'");
                if (quitAfter)
                {
                    yield return new WaitForSeconds(0.4f);
                    Application.Quit();
                }
            }
        }
    }
}
