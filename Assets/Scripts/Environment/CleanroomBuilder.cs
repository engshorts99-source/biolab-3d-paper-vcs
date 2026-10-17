using UnityEngine;
using BioLab.Core;

namespace BioLab.Environment
{
    /// <summary>
    /// Constructs a high-contrast, authentic ISO Class 5 Cleanroom Laboratory.
    /// Features a deep slate-grey antistatic ESD epoxy floor with yellow caution striping,
    /// dark navy coved baseboards (걸레받이), modular wall panels, stainless workstation benches,
    /// reagent shelving, pipette tip boxes, and recessed ceiling HEPA units.
    /// </summary>
    public class CleanroomBuilder : MonoBehaviour
    {
        [Header("Room Dimensions (Meters)")]
        public float roomWidth = 24.0f;
        public float roomLength = 22.0f;
        public float roomHeight = 3.8f;

        [Header("Materials")]
        public Material floorMaterial;
        public Material wallMaterial;
        public Material baseboardMaterial;
        public Material metalMaterial;
        public Material benchTopMaterial;
        public Material accentBlueMaterial;
        public Material warningYellowMaterial;
        public Material cyanGlowMaterial;

        [Header("Equipment Prefabs / Spawn Points")]
        public GameObject centrifugeObj;
        public GameObject bscObj;
        public GameObject freezerObj;

        private void Start()
        {
            BuildHighContrastCleanroom();
            BuildThreeZonePartitionsAndMarkers();
            BuildWorkbenchesAndProps();
        }

        public void BuildHighContrastCleanroom()
        {
            GameObject labRoot = new GameObject("Cleanroom_HighContrast_Architecture");
            labRoot.transform.SetParent(transform, false);

            // =========================================================================
            // 1. Deep Slate Epoxy Antistatic Floor (With Safety Hazard Stripe Texture)
            // =========================================================================
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ESD_Epoxy_Floor";
            floor.transform.SetParent(labRoot.transform, false);
            floor.transform.position = new Vector3(0, -0.1f, 0);
            floor.transform.localScale = new Vector3(roomWidth, 0.2f, roomLength);

            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }

            // =========================================================================
            // 2. Dark Navy Coved Baseboards (걸레받이 - Sharp Floor-to-Wall Horizon)
            // =========================================================================
            float bbH = 0.22f; // Baseboard height
            float bbThick = 0.08f;
            CreateBaseboard(labRoot.transform, new Vector3(0, bbH * 0.5f, roomLength * 0.5f - bbThick * 0.5f), new Vector3(roomWidth, bbH, bbThick), "Baseboard_North");
            CreateBaseboard(labRoot.transform, new Vector3(0, bbH * 0.5f, -roomLength * 0.5f + bbThick * 0.5f), new Vector3(roomWidth, bbH, bbThick), "Baseboard_South");
            CreateBaseboard(labRoot.transform, new Vector3(-roomWidth * 0.5f + bbThick * 0.5f, bbH * 0.5f, 0), new Vector3(bbThick, bbH, roomLength), "Baseboard_West");
            CreateBaseboard(labRoot.transform, new Vector3(roomWidth * 0.5f - bbThick * 0.5f, bbH * 0.5f, 0), new Vector3(bbThick, bbH, roomLength), "Baseboard_East");

            // =========================================================================
            // 3. Modular Cleanroom Sandwich Panel Walls
            // =========================================================================
            CreateWall(labRoot.transform, new Vector3(0, roomHeight * 0.5f, roomLength * 0.5f), new Vector3(roomWidth, roomHeight, 0.15f), "Wall_North");
            CreateWall(labRoot.transform, new Vector3(0, roomHeight * 0.5f, -roomLength * 0.5f), new Vector3(roomWidth, roomHeight, 0.15f), "Wall_South");
            CreateWall(labRoot.transform, new Vector3(-roomWidth * 0.5f, roomHeight * 0.5f, 0), new Vector3(0.15f, roomHeight, roomLength), "Wall_West");
            CreateWall(labRoot.transform, new Vector3(roomWidth * 0.5f, roomHeight * 0.5f, 0), new Vector3(0.15f, roomHeight, roomLength), "Wall_East");

            // Horizontal Wall Stainless Bumper Rails (0.95m height)
            CreateRail(labRoot.transform, new Vector3(0, 0.95f, roomLength * 0.5f - 0.05f), new Vector3(roomWidth, 0.06f, 0.04f));
            CreateRail(labRoot.transform, new Vector3(0, 0.95f, -roomLength * 0.5f + 0.05f), new Vector3(roomWidth, 0.06f, 0.04f));
            CreateRail(labRoot.transform, new Vector3(-roomWidth * 0.5f + 0.05f, 0.95f, 0), new Vector3(0.04f, 0.06f, roomLength));
            CreateRail(labRoot.transform, new Vector3(roomWidth * 0.5f - 0.05f, 0.95f, 0), new Vector3(0.04f, 0.06f, roomLength));

            // =========================================================================
            // 4. Dark Ceiling Grid with Recessed HEPA Diffusers (FFU) Across 3 Zones
            // =========================================================================
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_Structural_Slab";
            ceiling.transform.SetParent(labRoot.transform, false);
            ceiling.transform.position = new Vector3(0, roomHeight + 0.1f, 0);
            ceiling.transform.localScale = new Vector3(roomWidth, 0.2f, roomLength);
            if (baseboardMaterial != null) ceiling.GetComponent<Renderer>().material = baseboardMaterial;

            // HEPA Fan Filter Units with Cleanroom 5000K Lighting across X=(-2..2)
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    Vector3 ffuPos = new Vector3(x * 4.6f, roomHeight - 0.05f, z * 4.0f);

                    // FFU Frame
                    GameObject ffuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ffuObj.name = $"HEPA_Unit_{x}_{z}";
                    ffuObj.transform.SetParent(labRoot.transform, false);
                    ffuObj.transform.position = ffuPos;
                    ffuObj.transform.localScale = new Vector3(2.2f, 0.08f, 1.4f);
                    if (wallMaterial != null) ffuObj.GetComponent<Renderer>().material = wallMaterial;

                    // Diffused Daylight Luminaire
                    GameObject lightObj = new GameObject($"Luminaire_{x}_{z}");
                    lightObj.transform.SetParent(labRoot.transform, false);
                    lightObj.transform.position = ffuPos - new Vector3(0, 0.25f, 0);
                    Light lightComp = lightObj.AddComponent<Light>();
                    lightComp.type = LightType.Point;
                    lightComp.range = 8.5f;
                    lightComp.intensity = 1.05f;
                    lightComp.color = (x < -1)
                        ? new Color(0.88f, 1.0f, 0.95f)   // Zone 1 subtle cool-emerald tint
                        : (x > 1 ? new Color(0.95f, 0.92f, 1.0f) : new Color(0.92f, 0.96f, 1.0f));
                }
            }

            // =========================================================================
            // 5. Airlock Entrance Door (South Wall)
            // =========================================================================
            CreateAirlockDoor(labRoot.transform, new Vector3(0, 1.25f, -roomLength * 0.5f + 0.1f));
        }

        /// <summary>
        /// Builds the architectural glass partitions, zone floor inlays, and glowing zone header beams
        /// dividing the facility into:
        ///   Zone 1 (West): Reagent Warehouse & Cryo Storage Vault
        ///   Zone 2 (Center): Wet-Lab Protocol Execution Bay
        ///   Zone 3 (East): Dry-Lab Manuscript & PaperGit Command Office
        /// </summary>
        public void BuildThreeZonePartitionsAndMarkers()
        {
            GameObject zonesRoot = new GameObject("Three_Zones_Architectural_Dividers");
            zonesRoot.transform.SetParent(transform, false);

            Material glassPartitionMat = new Material(Shader.Find("Diffuse"));
            glassPartitionMat.color = new Color(0.35f, 0.72f, 0.92f, 0.35f);

            Material emeraldMat = new Material(Shader.Find("Diffuse"));
            emeraldMat.color = new Color(0.10f, 0.88f, 0.55f);

            Material violetMat = new Material(Shader.Find("Diffuse"));
            violetMat.color = new Color(0.68f, 0.35f, 0.98f);

            Material dryLabFloorMat = new Material(Shader.Find("Diffuse"));
            dryLabFloorMat.color = new Color(0.14f, 0.16f, 0.24f);

            // --- ZONE 1 (WEST WING: X = -12 to -5.2) REAGENT WAREHOUSE & CRYO VAULT ---
            // Emerald Zone Floor Boundary Line
            GameObject z1Line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            z1Line.name = "Zone1_Warehouse_Floor_Border";
            z1Line.transform.SetParent(zonesRoot.transform, false);
            z1Line.transform.position = new Vector3(-5.2f, 0.006f, 0f);
            z1Line.transform.localScale = new Vector3(0.12f, 0.01f, 18.0f);
            if (z1Line.GetComponent<Collider>()) Object.Destroy(z1Line.GetComponent<Collider>());
            z1Line.GetComponent<Renderer>().material = emeraldMat;

            // North & South Glass Partition Wings for Zone 1 (Leaving wide 6m center archway open)
            CreateGlassPartitionSegment(zonesRoot.transform, new Vector3(-5.2f, 1.6f, 6.5f), new Vector3(0.08f, 3.2f, 6.0f), glassPartitionMat, emeraldMat, "Zone1_North_Glass_Wall");
            CreateGlassPartitionSegment(zonesRoot.transform, new Vector3(-5.2f, 1.6f, -6.5f), new Vector3(0.08f, 3.2f, 6.0f), glassPartitionMat, emeraldMat, "Zone1_South_Glass_Wall");

            // Overhead Archway Beam for Zone 1
            GameObject z1Header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            z1Header.name = "Zone1_Header_Arch";
            z1Header.transform.SetParent(zonesRoot.transform, false);
            z1Header.transform.position = new Vector3(-5.2f, 3.05f, 0f);
            z1Header.transform.localScale = new Vector3(0.18f, 0.28f, 7.2f);
            z1Header.GetComponent<Renderer>().material = emeraldMat;

            // --- ZONE 3 (EAST WING: X = +5.2 to +12) DRY-LAB MANUSCRIPT & GIT OFFICE ---
            // Executive Dry-Lab Floor Mat Inlay
            GameObject z3Floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            z3Floor.name = "Zone3_DryLab_Office_Floor_Inlay";
            z3Floor.transform.SetParent(zonesRoot.transform, false);
            z3Floor.transform.position = new Vector3(8.5f, 0.004f, -1.0f);
            z3Floor.transform.localScale = new Vector3(6.4f, 0.008f, 16.0f);
            if (z3Floor.GetComponent<Collider>()) Object.Destroy(z3Floor.GetComponent<Collider>());
            z3Floor.GetComponent<Renderer>().material = dryLabFloorMat;

            // Violet Zone Floor Boundary Line
            GameObject z3Line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            z3Line.name = "Zone3_PaperGit_Floor_Border";
            z3Line.transform.SetParent(zonesRoot.transform, false);
            z3Line.transform.position = new Vector3(5.2f, 0.006f, 0f);
            z3Line.transform.localScale = new Vector3(0.12f, 0.01f, 18.0f);
            if (z3Line.GetComponent<Collider>()) Object.Destroy(z3Line.GetComponent<Collider>());
            z3Line.GetComponent<Renderer>().material = violetMat;

            // Acoustic Glass Partition Wings for Zone 3 Dry-Lab Office
            CreateGlassPartitionSegment(zonesRoot.transform, new Vector3(5.2f, 1.6f, 6.5f), new Vector3(0.08f, 3.2f, 6.0f), glassPartitionMat, violetMat, "Zone3_North_Glass_Wall");
            CreateGlassPartitionSegment(zonesRoot.transform, new Vector3(5.2f, 1.6f, -6.5f), new Vector3(0.08f, 3.2f, 6.0f), glassPartitionMat, violetMat, "Zone3_South_Glass_Wall");

            // Overhead Archway Beam for Zone 3
            GameObject z3Header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            z3Header.name = "Zone3_Header_Arch";
            z3Header.transform.SetParent(zonesRoot.transform, false);
            z3Header.transform.position = new Vector3(5.2f, 3.05f, 0f);
            z3Header.transform.localScale = new Vector3(0.18f, 0.28f, 7.2f);
            z3Header.GetComponent<Renderer>().material = violetMat;
        }

        private void CreateGlassPartitionSegment(Transform parent, Vector3 pos, Vector3 scale, Material glassMat, Material trimMat, string name)
        {
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = name;
            glass.transform.SetParent(parent, false);
            glass.transform.position = pos;
            glass.transform.localScale = scale;
            glass.GetComponent<Renderer>().material = glassMat;

            // Glowing vertical edge pillar
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = name + "_Frame_Pillar";
            pillar.transform.SetParent(parent, false);
            float zEdge = pos.z > 0 ? (pos.z - scale.z * 0.5f) : (pos.z + scale.z * 0.5f);
            pillar.transform.position = new Vector3(pos.x, pos.y, zEdge);
            pillar.transform.localScale = new Vector3(0.14f, scale.y, 0.14f);
            pillar.GetComponent<Renderer>().material = trimMat;
        }


        private void CreateBaseboard(Transform parent, Vector3 pos, Vector3 scale, string name)
        {
            GameObject bb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bb.name = name;
            bb.transform.SetParent(parent, false);
            bb.transform.position = pos;
            bb.transform.localScale = scale;
            if (baseboardMaterial != null) bb.GetComponent<Renderer>().material = baseboardMaterial;
        }

        private void CreateWall(Transform parent, Vector3 pos, Vector3 scale, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            if (wallMaterial != null) wall.GetComponent<Renderer>().material = wallMaterial;
        }

        private void CreateRail(Transform parent, Vector3 pos, Vector3 scale)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Stainless_Bumper_Rail";
            rail.transform.SetParent(parent, false);
            rail.transform.position = pos;
            rail.transform.localScale = scale;
            if (metalMaterial != null) rail.GetComponent<Renderer>().material = metalMaterial;
        }

        private void CreateAirlockDoor(Transform parent, Vector3 pos)
        {
            GameObject doorRoot = new GameObject("Cleanroom_Airlock_Interlock");
            doorRoot.transform.SetParent(parent, false);
            doorRoot.transform.position = pos;

            // Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Door_Frame";
            frame.transform.SetParent(doorRoot.transform, false);
            frame.transform.localScale = new Vector3(2.2f, 2.5f, 0.18f);
            if (baseboardMaterial != null) frame.GetComponent<Renderer>().material = baseboardMaterial;

            // Glass door leaf
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.name = "Door_Glass_Leaf";
            leaf.transform.SetParent(doorRoot.transform, false);
            leaf.transform.localScale = new Vector3(1.9f, 2.3f, 0.05f);
            if (accentBlueMaterial != null) leaf.GetComponent<Renderer>().material = accentBlueMaterial;

            // Sticky Mat on floor in front of door
            GameObject stickyMat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stickyMat.name = "Tacky_Mat_Contamination_Control";
            stickyMat.transform.SetParent(doorRoot.transform, false);
            stickyMat.transform.localPosition = new Vector3(0, -1.24f, 0.9f);
            stickyMat.transform.localScale = new Vector3(1.6f, 0.02f, 1.0f);
            if (accentBlueMaterial != null) stickyMat.GetComponent<Renderer>().material = accentBlueMaterial;
        }

        public void BuildWorkbenchesAndProps()
        {
            GameObject benchRoot = new GameObject("Workstations_And_Props");
            benchRoot.transform.SetParent(transform, false);

            // Zone 2 West Wet-Lab Workstation (Centrifuge & Sample Prep Bench)
            CreateBench(benchRoot.transform, new Vector3(-3.6f, 0, 3.2f), 5.2f);

            // Zone 2 East Wet-Lab Workstation (qPCR & Automated Liquid Handler Bench)
            CreateBench(benchRoot.transform, new Vector3(3.6f, 0, 3.2f), 5.2f);

            // Heroic Sci-Fi Pedestal & Floating Holographic Bio-Terminals
            BuildSciFiPedestalAndHoloConsoles(benchRoot.transform);
        }

        private void CreateBench(Transform parent, Vector3 pos, float length)
        {
            GameObject bench = new GameObject("Lab_Bench_Workstation");
            bench.transform.SetParent(parent, false);
            bench.transform.position = pos;

            // Black Granite / Anti-Vibration Countertop
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Granite_Countertop";
            top.transform.SetParent(bench.transform, false);
            top.transform.localPosition = new Vector3(0, 0.88f, 0);
            top.transform.localScale = new Vector3(1.4f, 0.08f, length);
            if (benchTopMaterial != null) top.GetComponent<Renderer>().material = benchTopMaterial;

            // Stainless Steel Legs & Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Satin_Steel_Cabinet";
            frame.transform.SetParent(bench.transform, false);
            frame.transform.localPosition = new Vector3(0, 0.42f, 0);
            frame.transform.localScale = new Vector3(1.28f, 0.84f, length - 0.2f);
            if (metalMaterial != null) frame.GetComponent<Renderer>().material = metalMaterial;

            // Reagent Shelf Above Bench
            GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "Reagent_Reagent_Racks";
            shelf.transform.SetParent(bench.transform, false);
            shelf.transform.localPosition = new Vector3(0.4f, 1.45f, 0);
            shelf.transform.localScale = new Vector3(0.45f, 0.04f, length);
            if (metalMaterial != null) shelf.GetComponent<Renderer>().material = metalMaterial;

            // Pipette Tip Boxes on Bench (Blue 1000uL and Yellow 200uL boxes)
            CreateTipBox(bench.transform, new Vector3(-0.25f, 0.96f, 0.8f), new Color(0.1f, 0.5f, 0.95f, 0.85f), "TipBox_1000uL_Blue");
            CreateTipBox(bench.transform, new Vector3(-0.25f, 0.96f, 0.55f), new Color(0.95f, 0.8f, 0.1f, 0.85f), "TipBox_200uL_Yellow");

            // Tube Rack with 1.5mL Tubes
            CreateTubeRack(bench.transform, new Vector3(-0.1f, 0.94f, -0.4f));
        }

        private void CreateTipBox(Transform parent, Vector3 localPos, Color color, string name)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPos;
            box.transform.localScale = new Vector3(0.18f, 0.09f, 0.14f);
            Material mat = new Material(Shader.Find("Diffuse"));
            mat.color = color;
            box.GetComponent<Renderer>().material = mat;
        }

        private void CreateTubeRack(Transform parent, Vector3 localPos)
        {
            GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.name = "Microcentrifuge_Tube_Rack_96Well";
            rack.transform.SetParent(parent, false);
            rack.transform.localPosition = localPos;
            rack.transform.localScale = new Vector3(0.24f, 0.04f, 0.36f);
            if (accentBlueMaterial != null) rack.GetComponent<Renderer>().material = accentBlueMaterial;

            // Individual sample tubes
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -2; c <= 2; c++)
                {
                    GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tube.name = $"Sample_Tube_{r}_{c}";
                    tube.transform.SetParent(rack.transform, false);
                    tube.transform.localPosition = new Vector3(r * 0.3f, 0.6f, c * 0.18f);
                    tube.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
                }
            }
        }

        public void BuildSciFiPedestalAndHoloConsoles(Transform parent)
        {
            Vector3 pedestalCenter = new Vector3(0f, 0f, -2.0f);
            GameObject pedestalRoot = new GameObject("SciFi_Command_Pedestal");
            pedestalRoot.transform.SetParent(parent, false);
            pedestalRoot.transform.position = pedestalCenter;

            // 1. Multi-tiered Circular Metallic Pedestal Base
            GameObject baseTier = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseTier.name = "Pedestal_Base_Tier";
            baseTier.transform.SetParent(pedestalRoot.transform, false);
            baseTier.transform.localPosition = new Vector3(0, 0.04f, 0);
            baseTier.transform.localScale = new Vector3(2.6f, 0.04f, 2.6f);
            if (metalMaterial != null) baseTier.GetComponent<Renderer>().material = metalMaterial;

            GameObject midTier = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midTier.name = "Pedestal_Mid_Tier";
            midTier.transform.SetParent(pedestalRoot.transform, false);
            midTier.transform.localPosition = new Vector3(0, 0.09f, 0);
            midTier.transform.localScale = new Vector3(2.2f, 0.03f, 2.2f);
            if (baseboardMaterial != null) midTier.GetComponent<Renderer>().material = baseboardMaterial;

            // Glowing Concentric LED Rings (Cyan & Blue)
            CreateGlowRing(pedestalRoot.transform, new Vector3(0, 0.125f, 0), 2.12f, 0.008f, cyanGlowMaterial);
            CreateGlowRing(pedestalRoot.transform, new Vector3(0, 0.126f, 0), 1.70f, 0.008f, accentBlueMaterial ?? cyanGlowMaterial);
            CreateGlowRing(pedestalRoot.transform, new Vector3(0, 0.127f, 0), 1.30f, 0.008f, cyanGlowMaterial);

            // Center Standing Disc (where character stands)
            GameObject standDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            standDisc.name = "Pedestal_Standing_Surface";
            standDisc.transform.SetParent(pedestalRoot.transform, false);
            standDisc.transform.localPosition = new Vector3(0, 0.120f, 0);
            standDisc.transform.localScale = new Vector3(1.22f, 0.015f, 1.22f);
            if (benchTopMaterial != null) standDisc.GetComponent<Renderer>().material = benchTopMaterial;

            // 2. Floating Holographic Bio-Terminals (Panoramic screens standing gracefully behind pedestal)
            Vector3 terminalBase = new Vector3(0f, 0f, -3.2f);

            // Center Main Holographic Display (Eye-level telemetry HUD framing the researcher)
            CreateHoloScreen(pedestalRoot.transform, terminalBase + new Vector3(0f, 1.85f, 0f), 
                new Vector3(1.75f, 0.96f, 1f), Quaternion.Euler(-4f, 180f, 0f), "Holo_Main_Biometrics");

            // Left Angled Hologram Display
            CreateHoloScreen(pedestalRoot.transform, terminalBase + new Vector3(-1.60f, 1.85f, 0.30f), 
                new Vector3(1.30f, 0.88f, 1f), Quaternion.Euler(-4f, 154f, 0f), "Holo_Left_GenomeSequence");

            // Right Angled Hologram Display
            CreateHoloScreen(pedestalRoot.transform, terminalBase + new Vector3(1.60f, 1.85f, 0.30f), 
                new Vector3(1.30f, 0.88f, 1f), Quaternion.Euler(-4f, 206f, 0f), "Holo_Right_AirPurityMetrics");

            // Two Sleek Angled Sci-Fi Control Consoles flanking the pedestal on Left and Right
            // Leaving the center axis completely open to highlight the researcher's full silhouette
            CreateSideConsole(pedestalRoot.transform, terminalBase + new Vector3(-2.2f, 0.38f, 0.6f), Quaternion.Euler(0f, 24f, 0f), "Console_Left_Wing");
            CreateSideConsole(pedestalRoot.transform, terminalBase + new Vector3(2.2f, 0.38f, 0.6f), Quaternion.Euler(0f, -24f, 0f), "Console_Right_Wing");
        }

        private void CreateSideConsole(Transform parent, Vector3 localPos, Quaternion localRot, string name)
        {
            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = name;
            desk.transform.SetParent(parent, false);
            desk.transform.localPosition = localPos;
            desk.transform.localRotation = localRot;
            desk.transform.localScale = new Vector3(1.65f, 0.76f, 0.60f);
            if (desk.GetComponent<Collider>()) Object.Destroy(desk.GetComponent<Collider>());
            if (benchTopMaterial != null) desk.GetComponent<Renderer>().material = benchTopMaterial;

            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = name + "_Trim";
            trim.transform.SetParent(desk.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.48f, 0.48f);
            trim.transform.localScale = new Vector3(1.0f, 0.04f, 0.05f);
            if (trim.GetComponent<Collider>()) Object.Destroy(trim.GetComponent<Collider>());
            if (cyanGlowMaterial != null) trim.GetComponent<Renderer>().material = cyanGlowMaterial;
        }

        private void CreateGlowRing(Transform parent, Vector3 pos, float diameter, float height, Material mat)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Neon_Glow_Ring";
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = pos;
            ring.transform.localScale = new Vector3(diameter, height, diameter);
            if (ring.GetComponent<Collider>()) Object.Destroy(ring.GetComponent<Collider>());
            if (mat != null) ring.GetComponent<Renderer>().material = mat;
        }

        private void CreateHoloScreen(Transform parent, Vector3 pos, Vector3 scale, Quaternion rot, string name)
        {
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = name;
            screen.transform.SetParent(parent, false);
            screen.transform.localPosition = pos;
            screen.transform.localRotation = rot;
            screen.transform.localScale = scale;
            if (screen.GetComponent<Collider>()) Object.Destroy(screen.GetComponent<Collider>());

            Shader s = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Diffuse");
            Material holoMat = new Material(s);
            Texture2D holoTex = LabTextureGenerator.CreateHoloDiagnosticTexture();
            if (holoMat.HasProperty("_MainTex")) holoMat.SetTexture("_MainTex", holoTex);
            holoMat.color = new Color(0.1f, 0.95f, 1.0f, 0.92f);
            screen.GetComponent<Renderer>().material = holoMat;
        }
    }
}
