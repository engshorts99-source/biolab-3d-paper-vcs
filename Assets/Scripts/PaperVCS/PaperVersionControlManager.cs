using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BioLab.Protocols;
using BioLab.Warehouse;

namespace BioLab.PaperVCS
{
    public enum DiffLineType
    {
        Header,
        Unchanged,
        Removed,
        Added,
        ProvenanceChange
    }

    [Serializable]
    public class DiffLine
    {
        public DiffLineType lineType;
        public string prefix; // " ", "-", "+", "@@"
        public string content;
    }

    [Serializable]
    public class PaperCommit
    {
        public string commitHash;
        public string parentHash;
        public string branchName;
        public string versionTag;
        public string commitMessage;
        public string author;
        public string timestamp;

        // Structured Manuscript Sections
        public string title;
        public string abstractText;
        public string methodsText;
        public string resultsText;
        public string discussionText;

        // Experimental Provenance Snapshot (Protocol + Reagent Warehouse Lots + Quantitative Metrics)
        public string linkedProtocolId;
        public string linkedProtocolVersion;
        public string linkedProtocolTitle;
        public string linkedSampleBatch;
        public string quantitativeMetrics;
        public List<string> reagentLotSnapshot = new List<string>();
    }

    [Serializable]
    public class PaperRepositorySaveData
    {
        public string currentBranch = "main";
        public string headCommitHash = "";
        public List<string> branches = new List<string>();
        public List<PaperCommit> commits = new List<PaperCommit>();
    }

    /// <summary>
    /// Git-Style Manuscript Version Control System (PaperGit) integrated with
    /// Zone 1 Reagent Warehouse Lots and Zone 2 Wet-Lab Protocol Executions.
    /// Supports commits, branching, checkout, merging, line-by-line & numeric Diff,
    /// automatic Methods generation from live lab protocols, and Markdown export.
    /// </summary>
    public class PaperVersionControlManager : MonoBehaviour
    {
        public static PaperVersionControlManager Instance { get; private set; }

        public string currentBranch = "revision/reviewer-1";
        public string headCommitHash = "4a0b88c";
        public List<string> branches = new List<string>();
        public List<PaperCommit> commits = new List<PaperCommit>();

        // Editable Working Tree (Uncommitted / Staged Manuscript State)
        public string workingTitle;
        public string workingAbstract;
        public string workingMethods;
        public string workingResults;
        public string workingDiscussion;
        public string workingQuantitativeMetrics;
        public string lastExportedFilePath = "";

        public event Action OnRepositoryChanged;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "biolab_papergit_v1.json");

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            LoadOrInitializeRepository();
        }

        public void LoadOrInitializeRepository()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var data = JsonUtility.FromJson<PaperRepositorySaveData>(json);
                    if (data != null && data.commits != null && data.commits.Count > 0)
                    {
                        currentBranch = data.currentBranch;
                        headCommitHash = data.headCommitHash;
                        branches = data.branches;
                        commits = data.commits;
                        LoadCommitIntoWorkingTree(GetCommitByHash(headCommitHash) ?? commits[commits.Count - 1]);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PaperGit] Failed to load repository JSON: {e.Message}");
                }
            }

            InitializeDefaultRepository();
            SaveRepository();
        }

        public void InitializeDefaultRepository()
        {
            branches = new List<string> { "main", "revision/reviewer-1", "exp/nlrp6-kinetics" };
            commits.Clear();

            // Commit 1: v0.1-draft (Initial Preliminary Draft on main)
            var c1 = new PaperCommit
            {
                commitHash = "c81f4a2",
                parentHash = "root",
                branchName = "main",
                versionTag = "v0.1-draft",
                commitMessage = "Initial manuscript draft: baseline NLRP6/NOD2 inflammasome signaling",
                author = "Dr. J. Park",
                timestamp = "2026-10-10 11:15",
                title = "Cooperative Innate Immune Sensing by NLRP6 and NOD2 During Viral Infection",
                abstractText = "Innate immune sensors NLRP6 and NOD2 detect viral and bacterial PAMPs in mucosal epithelia.\nHere we report initial observations of NLRP6 inflammasome activation in HEK293T cells.\nPreliminary centrifugation at 10,000 RPM showed moderate supernatant clarification.",
                methodsText = "1. Cell Culture: HEK293T cells were cultured in standard DMEM with 10% FBS (Lot# LOT-2025-OLD11).\n2. Clarification: Cell lysates were centrifuged at 10,000 RPM at room temperature (22°C) for 3 minutes.\n3. RT-qPCR: Gene expression was assessed across 35 cycles using standard SYBR Green.",
                resultsText = "- Baseline NLRP6 fold-induction reached 3.1-fold at 24 hours post-stimulation (p = 0.042, n = 2).\n- Room-temperature centrifugation left residual cellular debris in 2 of 6 wells.\n- Ct variance across technical replicates was ± 0.85 cycles.",
                discussionText = "While our preliminary data suggest cooperative NLRP6-NOD2 signaling, lysate clarification at room temperature introduced background noise requiring cold-chain optimization.",
                linkedProtocolId = "PRT-2026-001",
                linkedProtocolVersion = "v1.0",
                linkedProtocolTitle = "Preliminary NLRP6 Assay SOP",
                linkedSampleBatch = "BATCH-2026-NLRP6-01",
                quantitativeMetrics = "Centrifuge: 10,000 RPM (22°C) | qPCR Ct: 22.8 ± 0.85 | Viability: 89.5% | p = 0.042 (n=2)",
                reagentLotSnapshot = new List<string>
                {
                    "DMEM High Glucose [Cat# 11965-092 | Lot# LOT-2025-DM104 | 4°C Rack]",
                    "Fetal Bovine Serum [Cat# 10082-147 | Lot# LOT-2025-OLD11 | -20°C Freezer]"
                }
            };
            commits.Add(c1);

            // Commit 2: v1.0-submitted (Journal Submission on main)
            var c2 = new PaperCommit
            {
                commitHash = "9d3e72b",
                parentHash = "c81f4a2",
                branchName = "main",
                versionTag = "v1.0-submitted",
                commitMessage = "Journal submission v1.0: standardize 12,000 RPM spin and 24h qPCR panel",
                author = "Dr. J. Park",
                timestamp = "2026-10-14 16:40",
                title = "Cooperative Innate Immune Sensing by NLRP6 and NOD2 During Viral Infection",
                abstractText = "Innate immune sensors NLRP6 and NOD2 orchestrate mucosal antiviral defense.\nHere we demonstrate synergistic NLRP6 and NOD2 activation in HEK293T cells at 24h post-infection.\nStandardized lysate clarification at 12,000 RPM (4°C) improved signal-to-noise ratio.",
                methodsText = "1. Cell Culture: HEK293T cells were maintained in Gibco DMEM High Glucose (Lot# LOT-2026-DM882) with 10% FBS.\n2. Clarification: Lysates were centrifuged at 12,000 RPM at 4.0°C for 3 minutes in Centrifuge 5424 R.\n3. RT-qPCR: 40-cycle amplification using PowerUp SYBR Green Master Mix (Lot# LOT-2026-SY409).",
                resultsText = "- Synergistic NLRP6 + NOD2 activation increased IFN-lambda induction to 5.4-fold at 24h (p = 0.008, n = 3).\n- Refrigerated 12,000 RPM centrifugation reduced non-specific background by 42%.\n- Mean qPCR Ct value stabilized at 20.14 ± 0.38 cycles.",
                discussionText = "These findings establish a cooperative axis between NLRP6 and NOD2, though Reviewer #1 requested higher RCF clarification (14,000 RPM) and heat-inactivated FBS Lot verification.",
                linkedProtocolId = "PRT-2026-001",
                linkedProtocolVersion = "v1.8",
                linkedProtocolTitle = "NLRP6 & NOD2 Inflammasome Assay SOP",
                linkedSampleBatch = "BATCH-2026-NLRP6-05",
                quantitativeMetrics = "Centrifuge: 12,000 RPM (4.0°C) | qPCR Ct: 20.14 ± 0.38 | Viability: 94.1% | p = 0.008 (n=3)",
                reagentLotSnapshot = new List<string>
                {
                    "DMEM High Glucose [Cat# 11965-092 | Lot# LOT-2026-DM882 | Zone 1 Rack 1-A]",
                    "PowerUp SYBR Green 2X [Cat# A25742 | Lot# LOT-2026-SY409 | Zone 1 Cryo Box B2]"
                }
            };
            commits.Add(c2);

            // Commit 3: v1.1-rev1 (Reviewer #1 Revision Branch)
            var c3 = new PaperCommit
            {
                commitHash = "4a0b88c",
                parentHash = "9d3e72b",
                branchName = "revision/reviewer-1",
                versionTag = "v1.1-rev1",
                commitMessage = "Address Reviewer #1: upgrade to 14,000 RPM (18,400xg) & link verified -80C Reagent Lots",
                author = "Dr. J. Park",
                timestamp = "2026-10-16 19:05",
                title = "Synergistic Antiviral Innate Immune Sensing by NLRP6 and NOD2 Validated by Cold-Chain Quantitative PCR",
                abstractText = "Innate immune sensors NLRP6 and NOD2 orchestrate mucosal antiviral defense.\nHere we demonstrate robust synergistic NLRP6 and NOD2 inflammasome kinetics across a 36-hour window.\nHigh-speed refrigerated clarification at 14,000 RPM (18,400 x g, 4.0°C) and automated liquid handling eliminated batch variance.",
                methodsText = "1. Aseptic Seeding (BSC-01): HEK293T-NLRP6/NOD2 cells were seeded at 2x10^5 cells/well in Gibco DMEM (Cat# 11965-092, Lot# LOT-2026-DM882) supplemented with 10% US-Origin Heat-Inactivated FBS (Cat# 10082-147, Lot# LOT-2026-FB914).\n2. High-Speed Clarification (EQ-CENT-01): Cellular debris was pelleted in Centrifuge 5424 R at 14,000 RPM (18,400 x g) at 4.0°C for 5 minutes using QIAGEN Lysis Kit (Lot# LOT-2026-QG551).\n3. Automated 96-Well RT-qPCR (EQ-PCR-01): 40-cycle amplification was performed with PowerUp SYBR Green 2X Master Mix (Cat# A25742, Lot# LOT-2026-SY409) and Anti-NLRP6/NOD2 mAb validation (Lot# LOT-2026-NL773).\n4. Cryo-Archiving (FREEZER-ULT-01): Clarified aliquots were archived at -80.0°C in Rack A3.",
                resultsText = "- Synergistic NLRP6 + NOD2 co-stimulation elevated IFN-lambda & IL-18 output by 8.9-fold at 36h (p < 0.0001, n = 4).\n- Upgrading centrifugation from 12,000 RPM to 14,000 RPM (18,400 x g) at 4.0°C eliminated SDS-protein carryover completely.\n- Automated 96-well qPCR improved Ct precision to 18.42 ± 0.12 cycles (CV < 0.7%).",
                discussionText = "By integrating strict 3D cleanroom SOP tracking, -80°C reagent Lot provenance, and 14,000 RPM cold clarification, we fully resolve Reviewer #1's reproducibility concerns and demonstrate high-confidence NLRP6-NOD2 synergy.",
                linkedProtocolId = "PRT-2026-001",
                linkedProtocolVersion = "v2.1",
                linkedProtocolTitle = "NLRP6 & NOD2 Inflammasome Kinetic & qPCR Assay SOP",
                linkedSampleBatch = "BATCH-2026-NLRP6-09",
                quantitativeMetrics = "Centrifuge: 14,000 RPM (4.0°C, 18,400xg) | qPCR Ct: 18.42 ± 0.12 | Viability: 98.1% | p < 0.0001 (n=4)",
                reagentLotSnapshot = new List<string>
                {
                    "DMEM High Glucose [Cat# 11965-092 | Lot# LOT-2026-DM882 | Zone 1 Rack 1-A (4°C)]",
                    "Fetal Bovine Serum HI [Cat# 10082-147 | Lot# LOT-2026-FB914 | Zone 1 -80°C Freezer A1]",
                    "QIAGEN Lysis Kit [Cat# 27106 | Lot# LOT-2026-QG551 | Zone 1 Rack 2-B]",
                    "PowerUp SYBR Green 2X [Cat# A25742 | Lot# LOT-2026-SY409 | Zone 1 Cryo Box B2]",
                    "Anti-NLRP6/NOD2 mAb [Cat# CST-8902S | Lot# LOT-2026-NL773 | Zone 1 -80°C Vault D4]"
                }
            };
            commits.Add(c3);

            // Commit 4: v1.2-kinetics (Kinetic Timecourse Branch)
            var c4 = new PaperCommit
            {
                commitHash = "f72c19e",
                parentHash = "4a0b88c",
                branchName = "exp/nlrp6-kinetics",
                versionTag = "v1.2-kinetics",
                commitMessage = "Add 36h PR8 viral timecourse AUC normalization & automated liquid handler dilution curve",
                author = "Dr. J. Park",
                timestamp = "2026-10-17 10:30",
                title = "Synergistic Antiviral Innate Immune Sensing by NLRP6 and NOD2 Validated by Cold-Chain Quantitative PCR",
                abstractText = "Innate immune sensors NLRP6 and NOD2 orchestrate mucosal antiviral defense.\nHere we demonstrate robust synergistic NLRP6 and NOD2 inflammasome kinetics across a 36-hour window with AUC-normalized dose-response profiling.\nHigh-speed refrigerated clarification at 14,000 RPM (18,400 x g, 4.0°C) and automated 8-channel robotic pipetting achieved sub-1% inter-well variance.",
                methodsText = c3.methodsText + "\n5. Kinetic AUC Normalization: 36-hour luminescence & qPCR curves across Rows A-H were normalized by median baseline subtraction.",
                resultsText = c3.resultsText + "\n- 36-hour AUC kinetic integration confirmed dose-dependent NOD2-NLRP6 cooperativity across 8 serial dilutions (R^2 = 0.994).",
                discussionText = c3.discussionText,
                linkedProtocolId = "PRT-2026-001",
                linkedProtocolVersion = "v2.1",
                linkedProtocolTitle = "NLRP6 & NOD2 Inflammasome Kinetic & qPCR Assay SOP",
                linkedSampleBatch = "BATCH-2026-NLRP6-09",
                quantitativeMetrics = "Centrifuge: 14,000 RPM (4.0°C) | qPCR Ct: 18.42 ± 0.12 | 36h AUC R^2: 0.994 | p < 0.0001 (n=4)",
                reagentLotSnapshot = new List<string>(c3.reagentLotSnapshot)
            };
            commits.Add(c4);

            currentBranch = "revision/reviewer-1";
            headCommitHash = c3.commitHash;
            LoadCommitIntoWorkingTree(c3);
        }

        public PaperCommit GetCommitByHash(string hash)
        {
            if (string.IsNullOrEmpty(hash)) return null;
            return commits.Find(c => string.Equals(c.commitHash, hash, StringComparison.OrdinalIgnoreCase));
        }

        public PaperCommit GetHeadCommit()
        {
            return GetCommitByHash(headCommitHash) ?? (commits.Count > 0 ? commits[commits.Count - 1] : null);
        }

        public void LoadCommitIntoWorkingTree(PaperCommit c)
        {
            if (c == null) return;
            workingTitle = c.title;
            workingAbstract = c.abstractText;
            workingMethods = c.methodsText;
            workingResults = c.resultsText;
            workingDiscussion = c.discussionText;
            workingQuantitativeMetrics = c.quantitativeMetrics;
        }

        public void CheckoutCommit(string hash)
        {
            var c = GetCommitByHash(hash);
            if (c == null) return;
            headCommitHash = c.commitHash;
            currentBranch = c.branchName;
            LoadCommitIntoWorkingTree(c);
            SaveRepository();
            OnRepositoryChanged?.Invoke();
        }

        public void CheckoutBranch(string branch)
        {
            if (string.IsNullOrEmpty(branch)) return;
            if (!branches.Contains(branch)) branches.Add(branch);
            currentBranch = branch;

            // Find latest commit on this branch
            for (int i = commits.Count - 1; i >= 0; i--)
            {
                if (string.Equals(commits[i].branchName, branch, StringComparison.OrdinalIgnoreCase))
                {
                    headCommitHash = commits[i].commitHash;
                    LoadCommitIntoWorkingTree(commits[i]);
                    break;
                }
            }

            SaveRepository();
            OnRepositoryChanged?.Invoke();
        }

        public void CreateBranch(string newBranchName)
        {
            if (string.IsNullOrWhiteSpace(newBranchName)) return;
            string cleanName = newBranchName.Trim().Replace(" ", "-");
            if (!branches.Contains(cleanName))
            {
                branches.Add(cleanName);
            }
            currentBranch = cleanName;
            SaveRepository();
            OnRepositoryChanged?.Invoke();
        }

        /// <summary>
        /// Automatically synthesizes the Materials & Methods section and Reagent Lot Provenance
        /// directly from the live ProtocolManager steps and Zone 1 ReagentWarehouseManager inventory.
        /// </summary>
        public void SyncMethodsFromLiveLab()
        {
            var pm = ProtocolManager.Instance;
            var wm = ReagentWarehouseManager.Instance;
            if (pm == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[Auto-Generated from Protocol {pm.protocolId} ({pm.protocolVersion}): {pm.protocolTitle} | Batch: {pm.sampleBatchId}]");

            int completedCount = 0;
            foreach (var step in pm.steps)
            {
                if (step.isCompleted) completedCount++;
                string lotInfo = "";
                if (wm != null && !string.IsNullOrEmpty(step.linkedReagentId))
                {
                    var rg = wm.GetReagentById(step.linkedReagentId);
                    if (rg != null)
                    {
                        string usedLot = !string.IsNullOrEmpty(step.recordedLotNumber) ? step.recordedLotNumber : rg.lotNumber;
                        lotInfo = $" [Reagent: {rg.name}, {rg.vendor} Cat# {rg.catalogNumber}, Lot# {usedLot}, {step.reagentVolumeUsed} {step.reagentUnit}, Stored @ {rg.storageLocation}]";
                    }
                }

                string statusTag = step.isCompleted ? $"[VERIFIED {step.completedTimestamp}]" : "[SOP SPEC]";
                sb.AppendLine($"{step.stepNumber}. {step.title} ({step.targetEquipmentId}): {step.description} ({step.parameterSummary}){lotInfo} {statusTag}");
            }

            workingMethods = sb.ToString().TrimEnd();
            workingQuantitativeMetrics = $"Protocol {pm.protocolId} ({pm.protocolVersion}) | Verified Steps: {completedCount}/{pm.steps.Count} | Batch: {pm.sampleBatchId} | 14,000 RPM @ 4.0°C";
            OnRepositoryChanged?.Invoke();
        }

        public PaperCommit CommitWorkingTree(string commitMessage, string versionTag)
        {
            var pm = ProtocolManager.Instance;
            var wm = ReagentWarehouseManager.Instance;

            string newHash = Guid.NewGuid().ToString("N").Substring(0, 7);
            List<string> lotSnap = new List<string>();
            if (wm != null)
            {
                foreach (var r in wm.reagents)
                {
                    lotSnap.Add($"{r.name} [Cat# {r.catalogNumber} | Lot# {r.lotNumber} | Rem: {r.currentVolume:F0}/{r.maxVolume:F0} {r.unit} | {r.storageLocation}]");
                }
            }

            var newCommit = new PaperCommit
            {
                commitHash = newHash,
                parentHash = headCommitHash,
                branchName = currentBranch,
                versionTag = string.IsNullOrWhiteSpace(versionTag) ? $"v1.{commits.Count}" : versionTag.Trim(),
                commitMessage = string.IsNullOrWhiteSpace(commitMessage) ? "Update manuscript sections & sync protocol/reagent provenance" : commitMessage.Trim(),
                author = "Dr. J. Park",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                title = workingTitle,
                abstractText = workingAbstract,
                methodsText = workingMethods,
                resultsText = workingResults,
                discussionText = workingDiscussion,
                linkedProtocolId = pm != null ? pm.protocolId : "PRT-2026-001",
                linkedProtocolVersion = pm != null ? pm.protocolVersion : "v2.1",
                linkedProtocolTitle = pm != null ? pm.protocolTitle : "Standard Lab Protocol",
                linkedSampleBatch = pm != null ? pm.sampleBatchId : "BATCH-2026-01",
                quantitativeMetrics = workingQuantitativeMetrics,
                reagentLotSnapshot = lotSnap
            };

            commits.Add(newCommit);
            headCommitHash = newCommit.commitHash;
            SaveRepository();
            OnRepositoryChanged?.Invoke();
            return newCommit;
        }

        public PaperCommit MergeBranchIntoCurrent(string sourceBranch)
        {
            PaperCommit sourceLatest = null;
            for (int i = commits.Count - 1; i >= 0; i--)
            {
                if (string.Equals(commits[i].branchName, sourceBranch, StringComparison.OrdinalIgnoreCase))
                {
                    sourceLatest = commits[i];
                    break;
                }
            }

            if (sourceLatest == null) return null;

            LoadCommitIntoWorkingTree(sourceLatest);
            return CommitWorkingTree($"Merge branch '{sourceBranch}' ({sourceLatest.commitHash}) into {currentBranch}", $"v1.{commits.Count}-merge");
        }

        /// <summary>
        /// Computes a Git-style line-by-line & provenance diff between two manuscript commits.
        /// </summary>
        public List<DiffLine> ComputeDiff(PaperCommit baseCommit, PaperCommit targetCommit)
        {
            var list = new List<DiffLine>();
            if (baseCommit == null || targetCommit == null) return list;

            list.Add(new DiffLine
            {
                lineType = DiffLineType.Header,
                prefix = "@@",
                content = $"DIFF COMPARISON: [{baseCommit.commitHash} ({baseCommit.versionTag} on {baseCommit.branchName})]  --->  [{targetCommit.commitHash} ({targetCommit.versionTag} on {targetCommit.branchName})]"
            });

            // 1. Provenance & Quantitative Metrics Diff
            if (baseCommit.linkedProtocolVersion != targetCommit.linkedProtocolVersion ||
                baseCommit.quantitativeMetrics != targetCommit.quantitativeMetrics)
            {
                list.Add(new DiffLine { lineType = DiffLineType.Header, prefix = "@@", content = "--- EXPERIMENTAL PROVENANCE & METRICS ---" });
                if (baseCommit.linkedProtocolVersion != targetCommit.linkedProtocolVersion)
                {
                    list.Add(new DiffLine { lineType = DiffLineType.Removed, prefix = "-", content = $"Protocol: {baseCommit.linkedProtocolId} ({baseCommit.linkedProtocolVersion}) | Batch: {baseCommit.linkedSampleBatch}" });
                    list.Add(new DiffLine { lineType = DiffLineType.Added, prefix = "+", content = $"Protocol: {targetCommit.linkedProtocolId} ({targetCommit.linkedProtocolVersion}) | Batch: {targetCommit.linkedSampleBatch}" });
                }
                if (baseCommit.quantitativeMetrics != targetCommit.quantitativeMetrics)
                {
                    list.Add(new DiffLine { lineType = DiffLineType.Removed, prefix = "-", content = $"Metrics:  {baseCommit.quantitativeMetrics}" });
                    list.Add(new DiffLine { lineType = DiffLineType.Added, prefix = "+", content = $"Metrics:  {targetCommit.quantitativeMetrics}" });
                }
            }

            // 2. Section-by-Section Line Diff
            AppendSectionDiff(list, "TITLE", baseCommit.title, targetCommit.title);
            AppendSectionDiff(list, "ABSTRACT", baseCommit.abstractText, targetCommit.abstractText);
            AppendSectionDiff(list, "MATERIALS & METHODS (Protocol & Reagent Lot Linked)", baseCommit.methodsText, targetCommit.methodsText);
            AppendSectionDiff(list, "RESULTS & QUANTITATIVE FIGURES", baseCommit.resultsText, targetCommit.resultsText);
            AppendSectionDiff(list, "DISCUSSION", baseCommit.discussionText, targetCommit.discussionText);

            // 3. Reagent Warehouse Lot Snapshot Diff
            list.Add(new DiffLine { lineType = DiffLineType.Header, prefix = "@@", content = "--- ZONE 1 REAGENT WAREHOUSE LOT SNAPSHOT ---" });
            var baseLots = baseCommit.reagentLotSnapshot ?? new List<string>();
            var targetLots = targetCommit.reagentLotSnapshot ?? new List<string>();

            foreach (var oldLot in baseLots)
            {
                if (!targetLots.Contains(oldLot))
                    list.Add(new DiffLine { lineType = DiffLineType.Removed, prefix = "-", content = oldLot });
            }
            foreach (var newLot in targetLots)
            {
                if (!baseLots.Contains(newLot))
                    list.Add(new DiffLine { lineType = DiffLineType.Added, prefix = "+", content = newLot });
                else
                    list.Add(new DiffLine { lineType = DiffLineType.Unchanged, prefix = " ", content = newLot });
            }

            return list;
        }

        private void AppendSectionDiff(List<DiffLine> list, string sectionTitle, string oldText, string newText)
        {
            list.Add(new DiffLine { lineType = DiffLineType.Header, prefix = "@@", content = $"--- SECTION: {sectionTitle} ---" });

            string[] oldLines = (oldText ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] newLines = (newText ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            HashSet<string> oldSet = new HashSet<string>(oldLines);
            HashSet<string> newSet = new HashSet<string>(newLines);

            foreach (var ol in oldLines)
            {
                if (!newSet.Contains(ol))
                {
                    list.Add(new DiffLine { lineType = DiffLineType.Removed, prefix = "-", content = ol });
                }
            }

            foreach (var nl in newLines)
            {
                if (!oldSet.Contains(nl))
                {
                    list.Add(new DiffLine { lineType = DiffLineType.Added, prefix = "+", content = nl });
                }
                else
                {
                    list.Add(new DiffLine { lineType = DiffLineType.Unchanged, prefix = " ", content = nl });
                }
            }
        }

        public string ExportCurrentManuscriptToMarkdown()
        {
            try
            {
                string exportDir = Path.Combine(Directory.GetCurrentDirectory(), "ExportedPapers");
                if (!Directory.Exists(exportDir)) Directory.CreateDirectory(exportDir);

                var head = GetHeadCommit();
                string hashTag = head != null ? $"{head.commitHash}_{head.versionTag}" : "working_draft";
                string filePath = Path.Combine(exportDir, $"Manuscript_{hashTag}.md");

                StringBuilder md = new StringBuilder();
                md.AppendLine($"# {workingTitle}");
                md.AppendLine();
                md.AppendLine($"> **PaperGit Branch**: `{currentBranch}` | **HEAD Commit**: `{headCommitHash}` ({head?.versionTag}) | **Exported**: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
                md.AppendLine($"> **Linked Protocol**: `{head?.linkedProtocolId} ({head?.linkedProtocolVersion}) - {head?.linkedProtocolTitle}`");
                md.AppendLine($"> **Quantitative Summary**: `{workingQuantitativeMetrics}`");
                md.AppendLine();
                md.AppendLine("## Abstract");
                md.AppendLine(workingAbstract);
                md.AppendLine();
                md.AppendLine("## Materials & Methods (3D Cleanroom Protocol & Reagent Lot Provenance)");
                md.AppendLine(workingMethods);
                md.AppendLine();
                md.AppendLine("## Results");
                md.AppendLine(workingResults);
                md.AppendLine();
                md.AppendLine("## Discussion");
                md.AppendLine(workingDiscussion);
                md.AppendLine();
                md.AppendLine("## Zone 1 Reagent Warehouse Lot Provenance Table");
                if (head != null && head.reagentLotSnapshot != null)
                {
                    foreach (var lot in head.reagentLotSnapshot)
                    {
                        md.AppendLine($"- {lot}");
                    }
                }
                md.AppendLine();
                md.AppendLine("## Git Commit History");
                for (int i = commits.Count - 1; i >= 0; i--)
                {
                    var c = commits[i];
                    md.AppendLine($"- **`{c.commitHash}`** (`{c.branchName}` | `{c.versionTag}`) - {c.timestamp} by {c.author}: *{c.commitMessage}*");
                }

                File.WriteAllText(filePath, md.ToString());
                lastExportedFilePath = filePath;
                return filePath;
            }
            catch (Exception e)
            {
                lastExportedFilePath = $"Export Error: {e.Message}";
                return lastExportedFilePath;
            }
        }

        public void SaveRepository()
        {
            try
            {
                var data = new PaperRepositorySaveData
                {
                    currentBranch = currentBranch,
                    headCommitHash = headCommitHash,
                    branches = branches,
                    commits = commits
                };
                File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PaperGit] Save failed: {e.Message}");
            }
        }
    }
}
