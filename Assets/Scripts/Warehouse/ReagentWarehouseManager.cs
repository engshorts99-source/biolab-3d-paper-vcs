using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BioLab.Warehouse
{
    [Serializable]
    public class ReagentUsageLog
    {
        public string timestamp;
        public string protocolId;
        public string stepTitle;
        public string reagentId;
        public string reagentName;
        public string lotNumber;
        public float amountUsed;
        public string unit;
    }

    [Serializable]
    public class ReagentItem
    {
        public string reagentId;
        public string name;
        public string category;       // Media & Serum, Enzymes & qPCR, Buffers & Lysis, Antibodies, Cryo & Vectors
        public string vendor;
        public string catalogNumber;
        public string lotNumber;      // Tracked in PaperGit commits for reproducibility
        public string casNumber;
        public string storageStationId; // WAREHOUSE-01 or FREEZER-ULT-01
        public string storageLocation;  // e.g. "Zone 1 | Rack A-2 (4°C)" or "Zone 1 | -80°C Vault Box C4"
        public string storageTemp;      // "4°C Cold", "-20°C Freezer", "-80°C Ultra-Low", "RT (22°C)", "-196°C LN2"
        public float currentVolume;
        public float maxVolume;
        public string unit;             // mL, uL, rxn, vials
        public string expiryDate;
        public string hazardClass;      // Optimal / Biohazard / Flammable / Cryogenic
    }

    [Serializable]
    public class WarehouseSaveData
    {
        public List<ReagentItem> reagents = new List<ReagentItem>();
        public List<ReagentUsageLog> usageLogs = new List<ReagentUsageLog>();
    }

    /// <summary>
    /// Manages the Zone 1 Smart Reagent Warehouse & Cryo Vault inventory,
    /// Lot numbers, real-time volume consumption during protocol execution,
    /// and provenance snapshots for the Paper Version Control System.
    /// </summary>
    public class ReagentWarehouseManager : MonoBehaviour
    {
        public static ReagentWarehouseManager Instance { get; private set; }

        public List<ReagentItem> reagents = new List<ReagentItem>();
        public List<ReagentUsageLog> usageLogs = new List<ReagentUsageLog>();

        public event Action OnInventoryChanged;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "biolab_warehouse_v1.json");

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            LoadOrInitializeWarehouse();
        }

        public void LoadOrInitializeWarehouse()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var data = JsonUtility.FromJson<WarehouseSaveData>(json);
                    if (data != null && data.reagents != null && data.reagents.Count > 0)
                    {
                        reagents = data.reagents;
                        usageLogs = data.usageLogs ?? new List<ReagentUsageLog>();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ReagentWarehouse] Failed to load saved warehouse JSON: {e.Message}");
                }
            }

            InitializeDefaultReagents();
            SaveWarehouse();
        }

        public void InitializeDefaultReagents()
        {
            reagents.Clear();
            usageLogs.Clear();

            reagents.Add(new ReagentItem
            {
                reagentId = "RG-DMEM-01",
                name = "DMEM High Glucose (w/ L-Glutamine, Pyruvate)",
                category = "Media & Serum",
                vendor = "Gibco / Thermo Fisher",
                catalogNumber = "11965-092",
                lotNumber = "LOT-2026-DM882",
                casNumber = "N/A (Sterile Mixture)",
                storageStationId = "WAREHOUSE-01",
                storageLocation = "Zone 1 | Smart Rack Tier 1-A (4°C Cold)",
                storageTemp = "4°C Cold",
                currentVolume = 420f,
                maxVolume = 500f,
                unit = "mL",
                expiryDate = "2027-04-15",
                hazardClass = "Non-Hazardous"
            });

            reagents.Add(new ReagentItem
            {
                reagentId = "RG-FBS-02",
                name = "Fetal Bovine Serum (Heat-Inactivated, US Origin)",
                category = "Media & Serum",
                vendor = "Gibco",
                catalogNumber = "10082-147",
                lotNumber = "LOT-2026-FB914",
                casNumber = "9014-81-7",
                storageStationId = "FREEZER-ULT-01",
                storageLocation = "Zone 1 | -80°C Freezer Rack A1",
                storageTemp = "-80°C Ultra-Low",
                currentVolume = 185f,
                maxVolume = 250f,
                unit = "mL",
                expiryDate = "2028-01-30",
                hazardClass = "Bio-Origin"
            });

            reagents.Add(new ReagentItem
            {
                reagentId = "RG-LYSIS-03",
                name = "Alkaline Lysis & Neutralization Kit (P1/P2/N3)",
                category = "Buffers & Lysis",
                vendor = "QIAGEN",
                catalogNumber = "27106",
                lotNumber = "LOT-2026-QG551",
                casNumber = "1310-73-2 (NaOH/SDS)",
                storageStationId = "WAREHOUSE-01",
                storageLocation = "Zone 1 | Smart Rack Tier 2-B (RT / P1 @ 4°C)",
                storageTemp = "Room Temp / 4°C",
                currentVolume = 160f,
                maxVolume = 250f,
                unit = "mL",
                expiryDate = "2027-09-01",
                hazardClass = "Corrosive"
            });

            reagents.Add(new ReagentItem
            {
                reagentId = "RG-SYBR-04",
                name = "PowerUp SYBR Green qPCR Master Mix (2X)",
                category = "Enzymes & qPCR",
                vendor = "Applied Biosystems",
                catalogNumber = "A25742",
                lotNumber = "LOT-2026-SY409",
                casNumber = "163795-75-3",
                storageStationId = "FREEZER-ULT-01",
                storageLocation = "Zone 1 | Cryo Box B2 (-20°C / -80°C)",
                storageTemp = "-20°C Dark",
                currentVolume = 4200f,
                maxVolume = 5000f,
                unit = "uL",
                expiryDate = "2027-06-20",
                hazardClass = "Light-Sensitive"
            });

            reagents.Add(new ReagentItem
            {
                reagentId = "RG-AB-NLRP6",
                name = "Anti-NLRP6 / NOD2 Monoclonal Antibody Cocktail",
                category = "Antibodies",
                vendor = "Cell Signaling Technology",
                catalogNumber = "CST-8902S",
                lotNumber = "LOT-2026-NL773",
                casNumber = "IgG1-Kappa",
                storageStationId = "FREEZER-ULT-01",
                storageLocation = "Zone 1 | -80°C Cryo Vault Box D4",
                storageTemp = "-80°C Ultra-Low",
                currentVolume = 180f,
                maxVolume = 200f,
                unit = "uL",
                expiryDate = "2027-11-10",
                hazardClass = "Cryogenic"
            });

            reagents.Add(new ReagentItem
            {
                reagentId = "RG-ETOH-06",
                name = "Molecular Grade Absolute Ethanol (200 Proof, RNase-Free)",
                category = "Buffers & Lysis",
                vendor = "Sigma-Aldrich",
                catalogNumber = "E7023-500ML",
                lotNumber = "LOT-2026-ET312",
                casNumber = "64-17-5",
                storageStationId = "WAREHOUSE-01",
                storageLocation = "Zone 1 | Yellow Hazmat Safety Cabinet #1",
                storageTemp = "RT (Flammable Vault)",
                currentVolume = 380f,
                maxVolume = 500f,
                unit = "mL",
                expiryDate = "2028-08-01",
                hazardClass = "Flammable"
            });

            usageLogs.Add(new ReagentUsageLog
            {
                timestamp = "2026-10-16 14:20",
                protocolId = "PRT-2026-001",
                stepTitle = "Seed HEK293T & Harvest Culture",
                reagentId = "RG-DMEM-01",
                reagentName = "DMEM High Glucose",
                lotNumber = "LOT-2026-DM882",
                amountUsed = 15f,
                unit = "mL"
            });
        }

        public ReagentItem GetReagentById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return reagents.Find(r => string.Equals(r.reagentId, id, StringComparison.OrdinalIgnoreCase));
        }

        public bool ConsumeReagent(string reagentId, float amount, string protocolId, string stepTitle)
        {
            var item = GetReagentById(reagentId);
            if (item == null) return false;

            item.currentVolume = Mathf.Max(0f, item.currentVolume - amount);

            usageLogs.Insert(0, new ReagentUsageLog
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                protocolId = protocolId,
                stepTitle = stepTitle,
                reagentId = item.reagentId,
                reagentName = item.name,
                lotNumber = item.lotNumber,
                amountUsed = amount,
                unit = item.unit
            });

            SaveWarehouse();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void RestockReagent(string reagentId, float addAmount, string newLotNumber = null)
        {
            var item = GetReagentById(reagentId);
            if (item == null) return;

            item.currentVolume = Mathf.Clamp(item.currentVolume + addAmount, 0f, item.maxVolume);
            if (!string.IsNullOrEmpty(newLotNumber))
            {
                item.lotNumber = newLotNumber;
            }

            SaveWarehouse();
            OnInventoryChanged?.Invoke();
        }

        public ReagentItem RegisterNewReagent(
            string name, string category, string vendor, string catalogNum, string lotNum,
            string storageStationId, string storageLocation, string storageTemp, float maxVol, string unit, string hazard)
        {
            var newItem = new ReagentItem
            {
                reagentId = $"RG-CUSTOM-{reagents.Count + 1:D2}",
                name = string.IsNullOrWhiteSpace(name) ? "Custom Assay Reagent" : name,
                category = string.IsNullOrWhiteSpace(category) ? "Enzymes & qPCR" : category,
                vendor = string.IsNullOrWhiteSpace(vendor) ? "Thermo Fisher" : vendor,
                catalogNumber = string.IsNullOrWhiteSpace(catalogNum) ? "CAT-9901" : catalogNum,
                lotNumber = string.IsNullOrWhiteSpace(lotNum) ? $"LOT-2026-NW{reagents.Count + 10}" : lotNum,
                casNumber = "Registered-In-Lab",
                storageStationId = string.IsNullOrWhiteSpace(storageStationId) ? "WAREHOUSE-01" : storageStationId,
                storageLocation = string.IsNullOrWhiteSpace(storageLocation) ? "Zone 1 | Smart Rack Tier 3" : storageLocation,
                storageTemp = string.IsNullOrWhiteSpace(storageTemp) ? "4°C Cold" : storageTemp,
                currentVolume = maxVol,
                maxVolume = maxVol,
                unit = string.IsNullOrWhiteSpace(unit) ? "mL" : unit,
                expiryDate = "2027-12-31",
                hazardClass = string.IsNullOrWhiteSpace(hazard) ? "Optimal" : hazard
            };

            reagents.Add(newItem);
            SaveWarehouse();
            OnInventoryChanged?.Invoke();
            return newItem;
        }

        public void SaveWarehouse()
        {
            try
            {
                var data = new WarehouseSaveData
                {
                    reagents = reagents,
                    usageLogs = usageLogs
                };
                File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ReagentWarehouse] Save failed: {e.Message}");
            }
        }
    }
}
