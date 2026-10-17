using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BioLab.Equipment;
using BioLab.Protocols;
using BioLab.Warehouse;
using BioLab.PaperVCS;

namespace BioLab.UI
{
    /// <summary>
    /// Integrated 3D LabOS & PaperGit Workstation Interface.
    /// Provides:
    ///   - Real-time 3D Cleanroom HUD (Zone indicator, Active Protocol tracker, 3D Station proximity prompt, Zone teleport bar)
    ///   - Tab 1: PaperGit Manuscript Version Control (Branching, Commit Graph, Working Tree Editor, Live Lab Methods Sync)
    ///   - Tab 2: Visual Line-by-Line & Quantitative Provenance Diff Viewer (Red/Green Git Diff across versions)
    ///   - Tab 3: Protocol Authoring Studio & 3D Step Execution Runner
    ///   - Tab 4: Zone 1 Smart Reagent Warehouse & Cold-Chain Lot Inventory DB
    /// </summary>
    public class DiegeticTabletUI : MonoBehaviour
    {
        public static DiegeticTabletUI Instance { get; private set; }

        [Header("Tablet Presentation")]
        public GameObject tabletCanvasRoot;
        public KeyCode toggleKey = KeyCode.Tab;
        public bool isOpen = false;
        public int activeTab = 0; // 0: PaperGit VCS, 1: Diff Viewer, 2: Protocol Studio, 3: Reagent Warehouse

        // Legacy fields preserved for compatibility
        public Text protocolTitleText;
        public Text stepHeaderNumText;
        public Text stepDescriptionText;
        public Text sampleBatchText;
        public Text equipmentTargetText;
        public Button completeStepButton;

        // Scroll positions
        private Vector2 _commitListScroll;
        private Vector2 _editorScroll;
        private Vector2 _diffScroll;
        private Vector2 _protocolScroll;
        private Vector2 _warehouseScroll;

        // PaperGit Working Inputs
        private string _newCommitMsg = "Update Methods & Results with verified 14,000 RPM & -80C Lot provenance";
        private string _newVersionTag = "v1.3-verified";
        private string _newBranchInput = "revision/reviewer-2";
        private string _statusToast = "";
        private float _statusToastTimer = 0f;

        // Diff Selection
        private int _diffBaseIndex = 1;   // Default: v1.0-submitted (9d3e72b)
        private int _diffTargetIndex = 2; // Default: v1.1-rev1 (4a0b88c)

        // Protocol Authoring Inputs
        private string _newStepTitle = "Verify Supernatant Purity via NanoDrop";
        private string _newStepDesc = "Measure A260/A280 ratio inside BSC-01 prior to qPCR loading.";
        private string _newStepStation = "BSC-01";
        private string _newStepReagent = "RG-SYBR-04";
        private string _newStepVol = "50";
        private string _newStepParam = "A260/280 = 1.89 | 4°C";

        private string _newProtoTitle = "Reviewer #2 Follow-up Dose-Response SOP";
        private string _newProtoBatch = "BATCH-2026-REV2-01";

        // Warehouse Registration Inputs
        private string _warehouseFilter = "ALL";
        private string _newRgName = "Lipofectamine 3000 Transfection Reagent";
        private string _newRgVendor = "Invitrogen";
        private string _newRgCat = "L3000-015";
        private string _newRgLot = "LOT-2026-LP308";
        private string _newRgStation = "WAREHOUSE-01";
        private string _newRgLoc = "Zone 1 | Smart Rack Tier 1-B (4°C Cold)";
        private string _newRgVol = "1.5";
        private string _newRgUnit = "mL";

        // Procedural UI Textures & Styles
        private Texture2D _darkGlassTex;
        private Texture2D _panelCardTex;
        private Texture2D _headerBarTex;
        private Texture2D _diffAddTex;
        private Texture2D _diffDelTex;
        private Texture2D _diffHeaderTex;
        private Texture2D _btnCyanTex;
        private Texture2D _btnEmeraldTex;
        private Texture2D _btnVioletTex;
        private Texture2D _btnSlateTex;
        private bool _stylesInitialized = false;

        private GUIStyle _windowBoxStyle;
        private GUIStyle _cardBoxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _monoDiffAddStyle;
        private GUIStyle _monoDiffDelStyle;
        private GUIStyle _monoDiffHeaderStyle;
        private GUIStyle _monoDiffNormStyle;
        private GUIStyle _btnCyanStyle;
        private GUIStyle _btnEmeraldStyle;
        private GUIStyle _btnVioletStyle;
        private GUIStyle _btnSlateStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _textAreaStyle;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            // Check command line for automated UI tab inspection
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-openTab", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int tabIdx))
                    {
                        isOpen = true;
                        activeTab = Mathf.Clamp(tabIdx - 1, 0, 3);
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }
            }

            LabEquipment.OnAnyEquipmentInteracted += OnStationInteracted;
        }

        private void OnDestroy()
        {
            LabEquipment.OnAnyEquipmentInteracted -= OnStationInteracted;
        }

        private void OnStationInteracted(LabEquipment eq)
        {
            if (eq == null) return;

            ShowToast($"[3D Station Operated] {eq.equipmentName} ({eq.equipmentId}) — State: {eq.currentState}");

            // Automatically open the corresponding LabOS tab when interacting with key command stations
            if (eq.equipmentType == EquipmentType.ManuscriptCommandDesk)
            {
                OpenTabletToTab(0); // Open PaperGit VCS
            }
            else if (eq.equipmentType == EquipmentType.ReagentWarehouseRack)
            {
                OpenTabletToTab(3); // Open Zone 1 Reagent Warehouse DB
            }
        }

        public void OpenTabletToTab(int tabIndex)
        {
            activeTab = Mathf.Clamp(tabIndex, 0, 3);
            if (!isOpen)
            {
                ToggleTablet();
            }
        }

        public void ShowToast(string msg)
        {
            _statusToast = msg;
            _statusToastTimer = 5.0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleTablet();
            }

            // Quick Zone Teleport Hotkeys (F1 - F4)
            if (Input.GetKeyDown(KeyCode.F1)) TeleportPlayerToZone("CENTER");
            if (Input.GetKeyDown(KeyCode.F2)) TeleportPlayerToZone("ZONE1");
            if (Input.GetKeyDown(KeyCode.F3)) TeleportPlayerToZone("ZONE2");
            if (Input.GetKeyDown(KeyCode.F4)) TeleportPlayerToZone("ZONE3");

            if (_statusToastTimer > 0f)
            {
                _statusToastTimer -= Time.deltaTime;
            }
        }

        public void ToggleTablet()
        {
            isOpen = !isOpen;
            if (tabletCanvasRoot != null)
                tabletCanvasRoot.SetActive(isOpen);

            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }

        public void RefreshUI() { }

        public void TeleportPlayerToZone(string zoneCode)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            switch (zoneCode)
            {
                case "ZONE1": // Zone 1: Reagent Warehouse & Cryo Vault (West Wing)
                    player.transform.position = new Vector3(-7.2f, 0.15f, 0.5f);
                    player.transform.rotation = Quaternion.Euler(0, -45f, 0);
                    ShowToast("Teleported to Zone 1: Smart Reagent Warehouse & -80°C Cryo Vault");
                    break;
                case "ZONE2": // Zone 2: Wet-Lab Protocol Execution Bay (North/Center)
                    player.transform.position = new Vector3(0f, 0.15f, 4.5f);
                    player.transform.rotation = Quaternion.Euler(0, 0f, 0);
                    ShowToast("Teleported to Zone 2: Wet-Lab Protocol Execution Bay (BSC / Centrifuge / qPCR)");
                    break;
                case "ZONE3": // Zone 3: Dry-Lab PaperGit Command Office (East Wing)
                    player.transform.position = new Vector3(7.2f, 0.15f, -0.5f);
                    player.transform.rotation = Quaternion.Euler(0, 135f, 0);
                    ShowToast("Teleported to Zone 3: Dry-Lab Manuscript & PaperGit Command Office");
                    break;
                default:      // Center Command Pedestal
                    player.transform.position = new Vector3(0f, 0.15f, -2.0f);
                    player.transform.rotation = Quaternion.Euler(0, 0f, 0);
                    ShowToast("Teleported to Central Holo-Command Pedestal");
                    break;
            }

            if (cc != null) cc.enabled = true;
        }

        public void TeleportPlayerToStation(string stationId)
        {
            var eq = LabEquipment.FindById(stationId);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (eq == null || player == null) return;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 dirToCenter = (Vector3.zero - eq.transform.position).normalized;
            dirToCenter.y = 0;
            if (dirToCenter.sqrMagnitude < 0.01f) dirToCenter = Vector3.back;

            player.transform.position = eq.transform.position + dirToCenter * 1.65f + Vector3.up * 0.15f;
            player.transform.LookAt(new Vector3(eq.transform.position.x, player.transform.position.y, eq.transform.position.z));

            if (cc != null) cc.enabled = true;
            ShowToast($"Moved Researcher to Station: {eq.equipmentName} ({eq.equipmentId})");
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _darkGlassTex = MakeTex(new Color(0.04f, 0.07f, 0.12f, 0.94f));
            _panelCardTex = MakeTex(new Color(0.08f, 0.12f, 0.20f, 0.92f));
            _headerBarTex = MakeTex(new Color(0.05f, 0.09f, 0.16f, 0.92f));
            _diffAddTex = MakeTex(new Color(0.06f, 0.28f, 0.16f, 0.90f));
            _diffDelTex = MakeTex(new Color(0.32f, 0.08f, 0.12f, 0.90f));
            _diffHeaderTex = MakeTex(new Color(0.08f, 0.18f, 0.30f, 0.92f));
            _btnCyanTex = MakeTex(new Color(0.0f, 0.52f, 0.78f, 0.95f));
            _btnEmeraldTex = MakeTex(new Color(0.08f, 0.56f, 0.38f, 0.95f));
            _btnVioletTex = MakeTex(new Color(0.45f, 0.22f, 0.72f, 0.95f));
            _btnSlateTex = MakeTex(new Color(0.15f, 0.20f, 0.30f, 0.92f));

            _windowBoxStyle = new GUIStyle(GUI.skin.box);
            _windowBoxStyle.normal.background = _darkGlassTex;
            _windowBoxStyle.padding = new RectOffset(14, 14, 12, 12);

            _cardBoxStyle = new GUIStyle(GUI.skin.box);
            _cardBoxStyle.normal.background = _panelCardTex;
            _cardBoxStyle.padding = new RectOffset(10, 10, 8, 8);

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                richText = true
            };
            _titleStyle.normal.textColor = new Color(0.92f, 0.97f, 1.0f);

            _subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                richText = true
            };
            _subHeaderStyle.normal.textColor = new Color(0.35f, 0.90f, 1.0f);

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                richText = true
            };
            _bodyStyle.normal.textColor = new Color(0.84f, 0.90f, 0.96f);

            _monoDiffAddStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                richText = true,
                padding = new RectOffset(8, 8, 3, 3)
            };
            _monoDiffAddStyle.normal.background = _diffAddTex;
            _monoDiffAddStyle.normal.textColor = new Color(0.55f, 1.0f, 0.72f);

            _monoDiffDelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                richText = true,
                padding = new RectOffset(8, 8, 3, 3)
            };
            _monoDiffDelStyle.normal.background = _diffDelTex;
            _monoDiffDelStyle.normal.textColor = new Color(1.0f, 0.62f, 0.66f);

            _monoDiffHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                richText = true,
                padding = new RectOffset(8, 8, 4, 4)
            };
            _monoDiffHeaderStyle.normal.background = _diffHeaderTex;
            _monoDiffHeaderStyle.normal.textColor = new Color(0.35f, 0.92f, 1.0f);

            _monoDiffNormStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                richText = true,
                padding = new RectOffset(8, 8, 2, 2)
            };
            _monoDiffNormStyle.normal.textColor = new Color(0.76f, 0.82f, 0.90f);

            _btnCyanStyle = CreateButtonStyle(_btnCyanTex);
            _btnEmeraldStyle = CreateButtonStyle(_btnEmeraldTex);
            _btnVioletStyle = CreateButtonStyle(_btnVioletTex);
            _btnSlateStyle = CreateButtonStyle(_btnSlateTex);

            _textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                padding = new RectOffset(6, 6, 4, 4)
            };
            _textFieldStyle.normal.textColor = Color.white;

            _textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = 12,
                wordWrap = true,
                padding = new RectOffset(6, 6, 6, 6)
            };
            _textAreaStyle.normal.textColor = Color.white;

            _stylesInitialized = true;
        }

        private GUIStyle CreateButtonStyle(Texture2D bg)
        {
            var s = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                richText = true,
                padding = new RectOffset(10, 10, 5, 5)
            };
            s.normal.background = bg;
            s.hover.background = bg;
            s.active.background = bg;
            s.normal.textColor = Color.white;
            s.hover.textColor = Color.cyan;
            return s;
        }

        private Texture2D MakeTex(Color col)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            for (int i = 0; i < 4; i++) tex.SetPixel(i % 2, i / 2, col);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitStyles();

            DrawTopCommandBar();
            DrawActiveProtocolMiniHUD();
            DrawBottomProximityPrompt();

            if (isOpen)
            {
                DrawFullLabOSModal();
            }
        }

        private string DetectCurrentPlayerZone()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return "ZONE 2 // WET-LAB PROTOCOL BAY";
            float x = player.transform.position.x;
            if (x < -5.0f) return "<color=#22EE88>ZONE 1 // REAGENT WAREHOUSE & -80°C CRYO VAULT</color>";
            if (x > 5.0f) return "<color=#C47DFF>ZONE 3 // DRY-LAB MANUSCRIPT & PAPERGIT OFFICE</color>";
            return "<color=#00E5FF>ZONE 2 // WET-LAB PROTOCOL EXECUTION BAY</color>";
        }

        private void DrawTopCommandBar()
        {
            float w = Screen.width;
            GUI.Box(new Rect(0, 0, w, 46), "", _windowBoxStyle);

            var vcs = PaperVersionControlManager.Instance;
            string branchTag = vcs != null ? $"<b>{vcs.currentBranch}</b> @ <color=#00E5FF>{vcs.headCommitHash}</color>" : "main";

            GUILayout.BeginArea(new Rect(14, 7, w - 28, 34));
            GUILayout.BeginHorizontal();

            GUILayout.Label($"<b>🔬 BIOLAB 3D OS</b>  |  {DetectCurrentPlayerZone()}  |  🌿 PaperGit: {branchTag}", _bodyStyle, GUILayout.Width(w * 0.48f));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("📦 [F2] Zone1 시약창고", _btnEmeraldStyle, GUILayout.Height(28)))
                TeleportPlayerToZone("ZONE1");
            if (GUILayout.Button("🧪 [F3] Zone2 실험벤치", _btnCyanStyle, GUILayout.Height(28)))
                TeleportPlayerToZone("ZONE2");
            if (GUILayout.Button("📄 [F4] Zone3 논문오피스", _btnVioletStyle, GUILayout.Height(28)))
                TeleportPlayerToZone("ZONE3");
            if (GUILayout.Button(isOpen ? "✖ [Tab] 3D 뷰로 복귀" : "🖥 [Tab] 논문Git·프로토콜·창고 통합OS", _btnSlateStyle, GUILayout.Height(28)))
                ToggleTablet();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawActiveProtocolMiniHUD()
        {
            if (isOpen) return;
            var pm = ProtocolManager.Instance;
            if (pm == null) return;

            float panelW = 360f;
            float panelH = 148f;
            Rect r = new Rect(Screen.width - panelW - 16f, 58f, panelW, panelH);
            GUI.Box(r, "", _windowBoxStyle);

            GUILayout.BeginArea(new Rect(r.x + 12, r.y + 10, r.width - 24, r.height - 20));
            GUILayout.Label($"<b>📋 ACTIVE SOP: {pm.protocolId} ({pm.protocolVersion})</b>", _subHeaderStyle);

            var step = pm.GetCurrentStep();
            if (step != null)
            {
                GUILayout.Label($"<b>Step {step.stepNumber}/{pm.steps.Count}: {step.title}</b>", _bodyStyle);
                GUILayout.Label($"<color=#A8C0D8>{step.description}</color>", _bodyStyle);
                GUILayout.Label($"🎯 3D Station: <color=#00E5FF><b>{step.targetEquipmentId}</b></color> | Reagent: <color=#22EE88>{step.linkedReagentId} ({step.reagentVolumeUsed}{step.reagentUnit})</color>", _bodyStyle);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("📍 3D 장비로 이동", _btnSlateStyle, GUILayout.Height(24)))
                {
                    TeleportPlayerToStation(step.targetEquipmentId);
                }
                if (GUILayout.Button("✓ 스텝 완료 & 시약 차감", _btnEmeraldStyle, GUILayout.Height(24)))
                {
                    pm.CompleteCurrentStep();
                    ShowToast($"Completed Step {step.stepNumber}: {step.title} (Lot Recorded)");
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("<color=#22EE88><b>✓ ALL PROTOCOL STEPS COMPLETED!</b></color>", _bodyStyle);
                GUILayout.Label("Ready to auto-sync Methods & Reagent Lots into PaperGit.", _bodyStyle);
                if (GUILayout.Button("📄 PaperGit 논문 커밋 창 열기", _btnVioletStyle, GUILayout.Height(26)))
                {
                    if (PaperVersionControlManager.Instance != null)
                        PaperVersionControlManager.Instance.SyncMethodsFromLiveLab();
                    OpenTabletToTab(0);
                }
            }
            GUILayout.EndArea();
        }

        private void DrawBottomProximityPrompt()
        {
            if (_statusToastTimer > 0f && !string.IsNullOrEmpty(_statusToast))
            {
                Rect toastRect = new Rect(Screen.width * 0.5f - 290f, Screen.height - 96f, 580f, 36f);
                GUI.Box(toastRect, "", _cardBoxStyle);
                GUI.Label(new Rect(toastRect.x + 14, toastRect.y + 8, toastRect.width - 28, 22),
                    $"<color=#22EE88><b>⚡ {_statusToast}</b></color>", _bodyStyle);
            }

            if (isOpen) return;

            var nearEq = LabEquipment.NearestActiveEquipment;
            if (nearEq != null)
            {
                Rect promptRect = new Rect(Screen.width * 0.5f - 310f, Screen.height - 52f, 620f, 40f);
                GUI.Box(promptRect, "", _windowBoxStyle);
                GUI.Label(new Rect(promptRect.x + 16, promptRect.y + 10, promptRect.width - 32, 24),
                    $"<b>[{nearEq.equipmentId}] {nearEq.equipmentName}</b>  —  <color=#00E5FF>{nearEq.quickHint}</color>", _bodyStyle);
            }
            else
            {
                Rect helpRect = new Rect(16f, Screen.height - 38f, 520f, 28f);
                GUI.Box(helpRect, "", _cardBoxStyle);
                GUI.Label(new Rect(helpRect.x + 10, helpRect.y + 5, helpRect.width - 20, 20),
                    "조작: <b>WASD</b> 이동 | <b>Shift</b> 달리기 | <b>마우스</b> 회전/줌 | <b>[E]</b> 장비 작동 | <b>[Tab]</b> 논문Git·프로토콜·창고 OS", _bodyStyle);
            }
        }

        private void DrawFullLabOSModal()
        {
            float padX = Mathf.Max(24f, Screen.width * 0.035f);
            float padY = 56f;
            float modalW = Screen.width - padX * 2f;
            float modalH = Screen.height - padY - 24f;

            Rect modalRect = new Rect(padX, padY, modalW, modalH);
            GUI.Box(modalRect, "", _windowBoxStyle);

            GUILayout.BeginArea(new Rect(modalRect.x + 16, modalRect.y + 12, modalRect.width - 32, modalRect.height - 24));

            // Modal Navigation Tabs
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("📄 1. PaperGit 논문 버전 관리 & 에디터", activeTab == 0 ? _btnVioletStyle : _btnSlateStyle, GUILayout.Height(34)))
                activeTab = 0;
            if (GUILayout.Button("🔍 2. 버전 간 Diff 비교 (텍스트·수치·시약Lot)", activeTab == 1 ? _btnCyanStyle : _btnSlateStyle, GUILayout.Height(34)))
                activeTab = 1;
            if (GUILayout.Button("🧪 3. 프로토콜 수립 & 3D 실험 런너", activeTab == 2 ? _btnEmeraldStyle : _btnSlateStyle, GUILayout.Height(34)))
                activeTab = 2;
            if (GUILayout.Button("📦 4. Zone 1 시약 창고 & Lot 재고 DB", activeTab == 3 ? _btnEmeraldStyle : _btnSlateStyle, GUILayout.Height(34)))
                activeTab = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            switch (activeTab)
            {
                case 0:
                    DrawPaperGitTab(modalW - 32, modalH - 72);
                    break;
                case 1:
                    DrawDiffViewerTab(modalW - 32, modalH - 72);
                    break;
                case 2:
                    DrawProtocolStudioTab(modalW - 32, modalH - 72);
                    break;
                case 3:
                    DrawReagentWarehouseTab(modalW - 32, modalH - 72);
                    break;
            }

            GUILayout.EndArea();
        }

        // ========================================================================================
        // TAB 1: PAPERGIT MANUSCRIPT VERSION CONTROL & WORKING TREE EDITOR
        // ========================================================================================
        private void DrawPaperGitTab(float areaW, float areaH)
        {
            var vcs = PaperVersionControlManager.Instance;
            if (vcs == null) return;

            float leftW = areaW * 0.40f;
            float rightW = areaW - leftW - 14f;

            GUILayout.BeginHorizontal();

            // --- LEFT PANE: Branches & Commit Revision Timeline ---
            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(leftW), GUILayout.Height(areaH));
            GUILayout.Label("<b>🌿 GIT BRANCHES & REVISION TIMELINE</b>", _subHeaderStyle);

            // Branch Selector Row
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Branch:", _bodyStyle, GUILayout.Width(100));
            foreach (var br in vcs.branches)
            {
                bool isCurr = (br == vcs.currentBranch);
                if (GUILayout.Button(isCurr ? $"● {br}" : br, isCurr ? _btnVioletStyle : _btnSlateStyle, GUILayout.Height(24)))
                {
                    vcs.CheckoutBranch(br);
                    ShowToast($"Switched branch to '{br}'");
                }
            }
            GUILayout.EndHorizontal();

            // New Branch Creation Row
            GUILayout.BeginHorizontal();
            _newBranchInput = GUILayout.TextField(_newBranchInput, _textFieldStyle, GUILayout.Width(leftW * 0.52f));
            if (GUILayout.Button("+ 새 브랜치 분기 (git branch)", _btnSlateStyle, GUILayout.Height(24)))
            {
                vcs.CreateBranch(_newBranchInput);
                ShowToast($"Created & checked out new branch '{_newBranchInput}'");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label($"<b>📜 COMMIT HISTORY ({vcs.commits.Count} Snapshots)</b>", _subHeaderStyle);

            _commitListScroll = GUILayout.BeginScrollView(_commitListScroll);
            for (int i = vcs.commits.Count - 1; i >= 0; i--)
            {
                var c = vcs.commits[i];
                bool isHead = (c.commitHash == vcs.headCommitHash);

                GUILayout.BeginVertical(_cardBoxStyle);
                GUILayout.BeginHorizontal();
                string headBadge = isHead ? "<color=#22EE88><b>[HEAD]</b></color> " : "";
                GUILayout.Label($"{headBadge}<color=#00E5FF><b>{c.commitHash}</b></color>  <color=#C47DFF>[{c.versionTag}]</color>  ({c.branchName})", _bodyStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<color=#88A0B8>{c.timestamp}</color>", _bodyStyle);
                GUILayout.EndHorizontal();

                GUILayout.Label($"<b>{c.commitMessage}</b>", _bodyStyle);
                GUILayout.Label($"<color=#90B4D4>🧪 {c.linkedProtocolId} ({c.linkedProtocolVersion}) | 📊 {c.quantitativeMetrics}</color>", _bodyStyle);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Checkout (불러오기)", _btnSlateStyle, GUILayout.Height(22)))
                {
                    vcs.CheckoutCommit(c.commitHash);
                    ShowToast($"Checked out commit {c.commitHash} ({c.versionTag})");
                }
                if (GUILayout.Button("Diff Base로 지정", _btnSlateStyle, GUILayout.Height(22)))
                {
                    _diffBaseIndex = i;
                    ShowToast($"Set {c.commitHash} as Diff Base");
                }
                if (GUILayout.Button("Diff 비교 보기 →", _btnCyanStyle, GUILayout.Height(22)))
                {
                    _diffTargetIndex = i;
                    activeTab = 1;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // --- RIGHT PANE: Manuscript Working Tree Editor & Auto-Sync ---
            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(rightW), GUILayout.Height(areaH));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>📝 MANUSCRIPT WORKING TREE ({vcs.currentBranch})</b>", _subHeaderStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("⚡ 프로토콜 & 시약 창고 Lot → Methods 자동 반영", _btnEmeraldStyle, GUILayout.Height(28)))
            {
                vcs.SyncMethodsFromLiveLab();
                ShowToast("Synthesized Materials & Methods from active 3D Protocol & Zone 1 Reagent Lots!");
            }
            if (GUILayout.Button("📤 Markdown 논문 내보내기", _btnCyanStyle, GUILayout.Height(28)))
            {
                string path = vcs.ExportCurrentManuscriptToMarkdown();
                ShowToast($"Exported Manuscript Markdown to: {path}");
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(vcs.lastExportedFilePath))
            {
                GUILayout.Label($"<color=#22EE88>✓ 최근 내보내기 파일: {vcs.lastExportedFilePath}</color>", _bodyStyle);
            }

            _editorScroll = GUILayout.BeginScrollView(_editorScroll);

            GUILayout.Label("<b>1. Manuscript Title (논문 제목)</b>", _bodyStyle);
            vcs.workingTitle = GUILayout.TextField(vcs.workingTitle ?? "", _textFieldStyle);

            GUILayout.Label("<b>2. Quantitative Metrics Summary (핵심 실험 수치 요약)</b>", _bodyStyle);
            vcs.workingQuantitativeMetrics = GUILayout.TextField(vcs.workingQuantitativeMetrics ?? "", _textFieldStyle);

            GUILayout.Label("<b>3. Abstract (초록)</b>", _bodyStyle);
            vcs.workingAbstract = GUILayout.TextArea(vcs.workingAbstract ?? "", _textAreaStyle, GUILayout.Height(64));

            GUILayout.Label("<b>4. Materials & Methods (3D 프로토콜 & Zone 1 시약 Lot 자동 연동 섹션)</b>", _bodyStyle);
            vcs.workingMethods = GUILayout.TextArea(vcs.workingMethods ?? "", _textAreaStyle, GUILayout.Height(115));

            GUILayout.Label("<b>5. Results & Figures (실험 결과 및 통계)</b>", _bodyStyle);
            vcs.workingResults = GUILayout.TextArea(vcs.workingResults ?? "", _textAreaStyle, GUILayout.Height(78));

            GUILayout.Label("<b>6. Discussion (고찰 및 리뷰어 답변)</b>", _bodyStyle);
            vcs.workingDiscussion = GUILayout.TextArea(vcs.workingDiscussion ?? "", _textAreaStyle, GUILayout.Height(60));

            GUILayout.EndScrollView();

            // Commit Bar at Bottom of Editor
            GUILayout.Space(6);
            GUILayout.BeginHorizontal(_cardBoxStyle);
            GUILayout.Label("Tag:", _bodyStyle, GUILayout.Width(32));
            _newVersionTag = GUILayout.TextField(_newVersionTag, _textFieldStyle, GUILayout.Width(95));
            GUILayout.Label("Commit Msg:", _bodyStyle, GUILayout.Width(82));
            _newCommitMsg = GUILayout.TextField(_newCommitMsg, _textFieldStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("💾 새 버전 커밋 (git commit)", _btnVioletStyle, GUILayout.Width(190), GUILayout.Height(28)))
            {
                var created = vcs.CommitWorkingTree(_newCommitMsg, _newVersionTag);
                _diffBaseIndex = Mathf.Max(0, vcs.commits.Count - 2);
                _diffTargetIndex = vcs.commits.Count - 1;
                ShowToast($"Committed new paper snapshot {created.commitHash} ({created.versionTag}) on {created.branchName}!");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        // ========================================================================================
        // TAB 2: GIT DIFF VIEWER (LINE-BY-LINE MANUSCRIPT & REAGENT LOT PROVENANCE DIFF)
        // ========================================================================================
        private void DrawDiffViewerTab(float areaW, float areaH)
        {
            var vcs = PaperVersionControlManager.Instance;
            if (vcs == null || vcs.commits.Count == 0) return;

            _diffBaseIndex = Mathf.Clamp(_diffBaseIndex, 0, vcs.commits.Count - 1);
            _diffTargetIndex = Mathf.Clamp(_diffTargetIndex, 0, vcs.commits.Count - 1);

            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(areaW), GUILayout.Height(areaH));

            // Selector Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>🔴 BASE COMMIT (이전 버전):</b>", _bodyStyle, GUILayout.Width(175));
            for (int i = 0; i < vcs.commits.Count; i++)
            {
                var c = vcs.commits[i];
                if (GUILayout.Button($"{c.commitHash} ({c.versionTag})", _diffBaseIndex == i ? _btnVioletStyle : _btnSlateStyle, GUILayout.Height(26)))
                    _diffBaseIndex = i;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>🟢 TARGET COMMIT (비교 버전):</b>", _bodyStyle, GUILayout.Width(175));
            for (int i = 0; i < vcs.commits.Count; i++)
            {
                var c = vcs.commits[i];
                if (GUILayout.Button($"{c.commitHash} ({c.versionTag})", _diffTargetIndex == i ? _btnEmeraldStyle : _btnSlateStyle, GUILayout.Height(26)))
                    _diffTargetIndex = i;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            var baseCommit = vcs.commits[_diffBaseIndex];
            var targetCommit = vcs.commits[_diffTargetIndex];
            var diffLines = vcs.ComputeDiff(baseCommit, targetCommit);

            _diffScroll = GUILayout.BeginScrollView(_diffScroll);
            foreach (var dl in diffLines)
            {
                GUIStyle st = _monoDiffNormStyle;
                if (dl.lineType == DiffLineType.Header) st = _monoDiffHeaderStyle;
                else if (dl.lineType == DiffLineType.Added) st = _monoDiffAddStyle;
                else if (dl.lineType == DiffLineType.Removed) st = _monoDiffDelStyle;

                GUILayout.Label($"{dl.prefix}  {dl.content}", st);
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        // ========================================================================================
        // TAB 3: PROTOCOL AUTHORING STUDIO & 3D STEP EXECUTION RUNNER
        // ========================================================================================
        private void DrawProtocolStudioTab(float areaW, float areaH)
        {
            var pm = ProtocolManager.Instance;
            if (pm == null) return;

            float leftW = areaW * 0.56f;
            float rightW = areaW - leftW - 14f;

            GUILayout.BeginHorizontal();

            // Left: Active Protocol Steps & 3D Execution Controls
            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(leftW), GUILayout.Height(areaH));
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>🧪 SOP PROTOCOL LIBRARY:</b>", _subHeaderStyle);
            for (int i = 0; i < pm.protocolLibrary.Count; i++)
            {
                var p = pm.protocolLibrary[i];
                bool isSel = (p.protocolId == pm.protocolId);
                if (GUILayout.Button($"{p.protocolId} ({p.version})", isSel ? _btnCyanStyle : _btnSlateStyle, GUILayout.Height(26)))
                {
                    pm.SelectProtocol(i);
                }
            }
            if (GUILayout.Button("↺ 스텝 초기화", _btnSlateStyle, GUILayout.Height(26)))
            {
                pm.ResetCurrentProtocolSteps();
                ShowToast("Reset protocol steps for fresh 3D execution run.");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label($"<b>{pm.protocolTitle}</b>  |  Batch: <color=#00E5FF>{pm.sampleBatchId}</color>  |  Version: <color=#22EE88>{pm.protocolVersion}</color>", _bodyStyle);

            _protocolScroll = GUILayout.BeginScrollView(_protocolScroll);
            for (int i = 0; i < pm.steps.Count; i++)
            {
                var s = pm.steps[i];
                bool isCurr = (i == pm.currentStepIndex);

                GUILayout.BeginVertical(_cardBoxStyle);
                GUILayout.BeginHorizontal();
                string badge = s.isCompleted
                    ? $"<color=#22EE88><b>[✓ DONE {s.completedTimestamp}]</b></color>"
                    : (isCurr ? "<color=#00E5FF><b>[▶ ACTIVE STEP]</b></color>" : "<color=#88A0B8>[PENDING]</color>");

                GUILayout.Label($"{badge}  <b>Step {s.stepNumber}: {s.title}</b>", _bodyStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button($"📍 3D 이동 ({s.targetEquipmentId})", _btnSlateStyle, GUILayout.Height(22)))
                {
                    TeleportPlayerToStation(s.targetEquipmentId);
                }
                if (!s.isCompleted && GUILayout.Button("✓ 수행 완료 (시약 차감)", _btnEmeraldStyle, GUILayout.Height(22)))
                {
                    pm.currentStepIndex = i;
                    pm.CompleteCurrentStep();
                    ShowToast($"Executed Step {s.stepNumber} at {s.targetEquipmentId} & recorded Lot!");
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(s.description, _bodyStyle);
                string lotStamp = !string.IsNullOrEmpty(s.recordedLotNumber) ? $" | <color=#22EE88>Recorded Lot#: {s.recordedLotNumber}</color>" : "";
                GUILayout.Label($"<color=#90B4D4>⚙ Params: {s.parameterSummary}  |  📦 Reagent: {s.linkedReagentId} ({s.reagentVolumeUsed} {s.reagentUnit}){lotStamp}</color>", _bodyStyle);
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // Right: Protocol Authoring (Add New Step / Create New SOP Protocol)
            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(rightW), GUILayout.Height(areaH));
            GUILayout.Label("<b>➕ ADD STEP TO CURRENT PROTOCOL (프로토콜 스텝 추가)</b>", _subHeaderStyle);

            GUILayout.Label("Step Title (단계명):", _bodyStyle);
            _newStepTitle = GUILayout.TextField(_newStepTitle, _textFieldStyle);

            GUILayout.Label("Step Description (상세 실험 절차):", _bodyStyle);
            _newStepDesc = GUILayout.TextArea(_newStepDesc, _textAreaStyle, GUILayout.Height(48));

            GUILayout.BeginHorizontal();
            GUILayout.Label("3D Target Station:", _bodyStyle, GUILayout.Width(120));
            string[] stations = { "WAREHOUSE-01", "BSC-01", "EQ-CENT-01", "EQ-PCR-01", "FREEZER-ULT-01", "PAPER-DESK-01" };
            foreach (var st in stations)
            {
                if (GUILayout.Button(st.Split('-')[0], _newStepStation == st ? _btnCyanStyle : _btnSlateStyle, GUILayout.Height(22)))
                    _newStepStation = st;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Linked Reagent ID:", _bodyStyle, GUILayout.Width(120));
            _newStepReagent = GUILayout.TextField(_newStepReagent, _textFieldStyle, GUILayout.Width(120));
            GUILayout.Label("Volume:", _bodyStyle, GUILayout.Width(52));
            _newStepVol = GUILayout.TextField(_newStepVol, _textFieldStyle, GUILayout.Width(60));
            GUILayout.EndHorizontal();

            GUILayout.Label("Parameter Spec (RPM / Temp / Cycles):", _bodyStyle);
            _newStepParam = GUILayout.TextField(_newStepParam, _textFieldStyle);

            if (GUILayout.Button("+ 현재 프로토콜에 스텝 추가 (버전 자동 증가)", _btnEmeraldStyle, GUILayout.Height(30)))
            {
                float.TryParse(_newStepVol, out float v);
                pm.AddStepToActiveProtocol(_newStepTitle, _newStepDesc, _newStepStation, _newStepReagent, v > 0 ? v : 10f, "uL", _newStepParam);
                ShowToast($"Added Step to {pm.protocolId} -> Upgraded to {pm.protocolVersion}");
            }

            GUILayout.Space(16);
            GUILayout.Label("<b>🆕 CREATE NEW SOP PROTOCOL (신규 프로토콜 수립)</b>", _subHeaderStyle);
            GUILayout.Label("New Protocol Title:", _bodyStyle);
            _newProtoTitle = GUILayout.TextField(_newProtoTitle, _textFieldStyle);
            GUILayout.Label("Sample Batch ID:", _bodyStyle);
            _newProtoBatch = GUILayout.TextField(_newProtoBatch, _textFieldStyle);

            if (GUILayout.Button("✨ 신규 프로토콜 생성 및 활성화", _btnVioletStyle, GUILayout.Height(30)))
            {
                var created = pm.CreateNewProtocol(_newProtoTitle, "v1.0", _newProtoBatch, "Dr. J. Park");
                ShowToast($"Created new SOP Protocol: {created.protocolId} - {created.title}");
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        // ========================================================================================
        // TAB 4: ZONE 1 SMART REAGENT WAREHOUSE & LOT PROVENANCE INVENTORY
        // ========================================================================================
        private void DrawReagentWarehouseTab(float areaW, float areaH)
        {
            var wm = ReagentWarehouseManager.Instance;
            if (wm == null) return;

            float leftW = areaW * 0.62f;
            float rightW = areaW - leftW - 14f;

            GUILayout.BeginHorizontal();

            // Left: Warehouse Reagent Cards
            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(leftW), GUILayout.Height(areaH));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>📦 ZONE 1 REAGENT WAREHOUSE ({wm.reagents.Count} Lots Tracked)</b>", _subHeaderStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("All Zones", _warehouseFilter == "ALL" ? _btnEmeraldStyle : _btnSlateStyle, GUILayout.Height(24)))
                _warehouseFilter = "ALL";
            if (GUILayout.Button("4°C / RT Rack", _warehouseFilter == "WAREHOUSE-01" ? _btnCyanStyle : _btnSlateStyle, GUILayout.Height(24)))
                _warehouseFilter = "WAREHOUSE-01";
            if (GUILayout.Button("-80°C Cryo Vault", _warehouseFilter == "FREEZER-ULT-01" ? _btnVioletStyle : _btnSlateStyle, GUILayout.Height(24)))
                _warehouseFilter = "FREEZER-ULT-01";
            GUILayout.EndHorizontal();

            _warehouseScroll = GUILayout.BeginScrollView(_warehouseScroll);
            foreach (var rg in wm.reagents)
            {
                if (_warehouseFilter != "ALL" && rg.storageStationId != _warehouseFilter) continue;

                float pct = rg.maxVolume > 0 ? (rg.currentVolume / rg.maxVolume) * 100f : 0f;
                string stockColor = pct > 35f ? "#22EE88" : "#FFAA22";

                GUILayout.BeginVertical(_cardBoxStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>[{rg.reagentId}] {rg.name}</b>  <color=#00E5FF><b>Lot#: {rg.lotNumber}</b></color>", _bodyStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<color={stockColor}><b>Stock: {rg.currentVolume:F1} / {rg.maxVolume:F0} {rg.unit} ({pct:F0}%)</b></color>", _bodyStyle);
                GUILayout.EndHorizontal();

                GUILayout.Label($"<color=#90B4D4>Vendor: {rg.vendor} (Cat# {rg.catalogNumber})  |  📍 {rg.storageLocation} ({rg.storageTemp})  |  Exp: {rg.expiryDate}</color>", _bodyStyle);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("📍 3D 창고/냉동고 위치로 이동", _btnSlateStyle, GUILayout.Height(22)))
                {
                    TeleportPlayerToStation(rg.storageStationId);
                }
                if (GUILayout.Button("-10 분주 사용 (Dispense)", _btnCyanStyle, GUILayout.Height(22)))
                {
                    wm.ConsumeReagent(rg.reagentId, 10f, "MANUAL-DISPENSE", "Direct Warehouse Aliquot");
                    ShowToast($"Dispensed 10 {rg.unit} of {rg.name} (Lot# {rg.lotNumber})");
                }
                if (GUILayout.Button("+50 재고 보충 (Restock)", _btnEmeraldStyle, GUILayout.Height(22)))
                {
                    wm.RestockReagent(rg.reagentId, 50f);
                    ShowToast($"Restocked +50 {rg.unit} of {rg.name}");
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // Right: New Reagent Registration & Consumption Audit Log
            GUILayout.BeginVertical(_cardBoxStyle, GUILayout.Width(rightW), GUILayout.Height(areaH));
            GUILayout.Label("<b>➕ REGISTER NEW REAGENT LOT (신규 시약 입고 등록)</b>", _subHeaderStyle);

            GUILayout.Label("Reagent Name (시약명):", _bodyStyle);
            _newRgName = GUILayout.TextField(_newRgName, _textFieldStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Vendor:", _bodyStyle, GUILayout.Width(55));
            _newRgVendor = GUILayout.TextField(_newRgVendor, _textFieldStyle, GUILayout.Width(110));
            GUILayout.Label("Cat#:", _bodyStyle, GUILayout.Width(40));
            _newRgCat = GUILayout.TextField(_newRgCat, _textFieldStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Lot Number (논문 추적용):", _bodyStyle, GUILayout.Width(145));
            _newRgLot = GUILayout.TextField(_newRgLot, _textFieldStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("3D Storage Unit:", _bodyStyle, GUILayout.Width(110));
            if (GUILayout.Button("WAREHOUSE-01 (4°C/RT)", _newRgStation == "WAREHOUSE-01" ? _btnEmeraldStyle : _btnSlateStyle, GUILayout.Height(22)))
            {
                _newRgStation = "WAREHOUSE-01";
                _newRgLoc = "Zone 1 | Smart Rack Tier 3 (4°C Cold)";
            }
            if (GUILayout.Button("FREEZER-ULT-01 (-80°C)", _newRgStation == "FREEZER-ULT-01" ? _btnVioletStyle : _btnSlateStyle, GUILayout.Height(22)))
            {
                _newRgStation = "FREEZER-ULT-01";
                _newRgLoc = "Zone 1 | -80°C Cryo Vault Box E2";
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vol:", _bodyStyle, GUILayout.Width(60));
            _newRgVol = GUILayout.TextField(_newRgVol, _textFieldStyle, GUILayout.Width(70));
            GUILayout.Label("Unit:", _bodyStyle, GUILayout.Width(40));
            _newRgUnit = GUILayout.TextField(_newRgUnit, _textFieldStyle, GUILayout.Width(60));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("📦 Zone 1 시약 창고에 신규 Lot 입고 등록", _btnEmeraldStyle, GUILayout.Height(28)))
            {
                float.TryParse(_newRgVol, out float mv);
                var item = wm.RegisterNewReagent(_newRgName, "Enzymes & qPCR", _newRgVendor, _newRgCat, _newRgLot,
                    _newRgStation, _newRgLoc, _newRgStation == "FREEZER-ULT-01" ? "-80°C Ultra-Low" : "4°C Cold",
                    mv > 0 ? mv : 100f, _newRgUnit, "Optimal");
                ShowToast($"Registered {item.name} [Lot# {item.lotNumber}] in {item.storageLocation}");
            }

            GUILayout.Space(12);
            GUILayout.Label("<b>🧾 LIVE REAGENT CONSUMPTION AUDIT LOG (시약 출고/사용 이력)</b>", _subHeaderStyle);
            int maxLogs = Mathf.Min(6, wm.usageLogs.Count);
            for (int i = 0; i < maxLogs; i++)
            {
                var log = wm.usageLogs[i];
                GUILayout.Label($"• <color=#00E5FF>{log.timestamp}</color> | <b>{log.reagentName}</b> (<color=#22EE88>{log.lotNumber}</color>) -{log.amountUsed}{log.unit} via <i>{log.protocolId}</i>", _bodyStyle);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
