using UnityEngine;

namespace BioLab.Equipment
{
    /// <summary>
    /// Constructs high-fidelity, beveled laboratory equipment.
    /// Includes 24-place rotor with colored sample tubes, touchscreens,
    /// pneumatic hinges, sliding glass sashes, and vacuum cam-lock handles.
    /// </summary>
    public static class LabModelGenerator
    {
        public static GameObject CreateCentrifuge5424R(Material bodyMat, Material accentMat, Material glassMat, Material metalMat, Texture2D displayTex)
        {
            GameObject root = new GameObject("Centrifuge_5424R_Refrigerated");

            // 1. Aerodynamic Main Body Chassis
            GameObject bodyBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyBase.name = "Chassis_Base";
            bodyBase.transform.SetParent(root.transform, false);
            bodyBase.transform.localPosition = new Vector3(0, 0.16f, 0);
            bodyBase.transform.localScale = new Vector3(0.56f, 0.32f, 0.64f);
            if (bodyMat != null) bodyBase.GetComponent<Renderer>().material = bodyMat;

            // Curved Beveled Top Cowling
            GameObject cowling = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cowling.name = "Chassis_Top_Cowling";
            cowling.transform.SetParent(root.transform, false);
            cowling.transform.localPosition = new Vector3(0, 0.30f, 0.04f);
            cowling.transform.localScale = new Vector3(0.54f, 0.06f, 0.54f);
            if (bodyMat != null) cowling.GetComponent<Renderer>().material = bodyMat;

            // Rotor Bowl Cavity (Anodized Dark Metal)
            GameObject cavity = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cavity.name = "Rotor_Chamber_Cavity";
            cavity.transform.SetParent(root.transform, false);
            cavity.transform.localPosition = new Vector3(0, 0.31f, 0.04f);
            cavity.transform.localScale = new Vector3(0.40f, 0.04f, 0.40f);
            if (metalMat != null) cavity.GetComponent<Renderer>().material = metalMat;

            // 24-Place Aluminum Fixed-Angle Rotor
            GameObject rotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rotor.name = "Fixed_Angle_Rotor_24x1_5mL";
            rotor.transform.SetParent(cavity.transform, false);
            rotor.transform.localPosition = new Vector3(0, 0.4f, 0);
            rotor.transform.localScale = new Vector3(0.85f, 0.5f, 0.85f);
            if (accentMat != null) rotor.GetComponent<Renderer>().material = accentMat;

            // Microcentrifuge Tubes with Colored Caps inside Rotor
            Color[] capColors = { Color.red, Color.blue, Color.yellow, Color.green };
            for (int i = 0; i < 12; i++)
            {
                float angle = i * (360f / 12f) * Mathf.Deg2Rad;
                float rx = Mathf.Cos(angle) * 0.36f;
                float rz = Mathf.Sin(angle) * 0.36f;

                GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tube.name = $"Tube_Cap_{i}";
                tube.transform.SetParent(rotor.transform, false);
                tube.transform.localPosition = new Vector3(rx, 0.52f, rz);
                tube.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
                tube.transform.localRotation = Quaternion.Euler(25f * Mathf.Sin(angle), 0, -25f * Mathf.Cos(angle));

                Material tubeMat = new Material(Shader.Find("Diffuse"));
                tubeMat.color = capColors[i % capColors.Length];
                tube.GetComponent<Renderer>().material = tubeMat;
            }

            // 2. High-Tech Touchscreen Control Panel
            GameObject displayPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            displayPanel.name = "Digital_Touchscreen_Display";
            displayPanel.transform.SetParent(root.transform, false);
            displayPanel.transform.localPosition = new Vector3(0, 0.22f, 0.30f);
            displayPanel.transform.localScale = new Vector3(0.38f, 0.14f, 0.03f);
            displayPanel.transform.localRotation = Quaternion.Euler(30f, 0, 0);

            Material displayMat = new Material(Shader.Find("Diffuse"));
            if (displayTex != null)
            {
                displayMat.mainTexture = displayTex;
            }
            displayMat.color = Color.white;
            displayPanel.GetComponent<Renderer>().material = displayMat;

            // 3. Aerodynamic Hinged Lid with Smoked Inspection Window
            GameObject lidPivot = new GameObject("Lid_Hinge_Pivot");
            lidPivot.transform.SetParent(root.transform, false);
            lidPivot.transform.localPosition = new Vector3(0, 0.34f, -0.22f);

            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "Aerodynamic_Lid";
            lid.transform.SetParent(lidPivot.transform, false);
            lid.transform.localPosition = new Vector3(0, 0.02f, 0.24f);
            lid.transform.localScale = new Vector3(0.54f, 0.04f, 0.52f);
            if (bodyMat != null) lid.GetComponent<Renderer>().material = bodyMat;

            // Smoked Acrylic Inspection Glass
            GameObject lidGlass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lidGlass.name = "Inspection_Glass";
            lidGlass.transform.SetParent(lid.transform, false);
            lidGlass.transform.localPosition = new Vector3(0, 0.25f, 0);
            lidGlass.transform.localScale = new Vector3(0.48f, 0.15f, 0.48f);
            if (glassMat != null) lidGlass.GetComponent<Renderer>().material = glassMat;

            // Dual Rear Pneumatic Cylinders
            CreatePneumaticStrut(lidPivot.transform, new Vector3(-0.22f, -0.08f, 0.08f));
            CreatePneumaticStrut(lidPivot.transform, new Vector3(0.22f, -0.08f, 0.08f));

            // LabEquipment Component
            LabEquipment eq = root.AddComponent<LabEquipment>();
            eq.equipmentName = "Centrifuge 5424 R (Refrigerated)";
            eq.equipmentType = EquipmentType.RefrigeratedCentrifuge;
            eq.equipmentId = "EQ-CENT-01";
            eq.zoneName = "Zone 2: Wet-Lab Protocol Bay";
            eq.quickHint = "Press [E] Operate Centrifuge 5424 R (14,000 RPM / 4°C)";
            eq.movingPart = lidPivot.transform;
            eq.rotorTransform = rotor.transform;
            eq.targetRpm = 14000f;
            eq.targetTemp = 4.0f;
            eq.durationSeconds = 30f;
            eq.beaconTransform = CreateStationBeacon(root.transform, new Vector3(0, 0.85f, 0), new Color(0.0f, 0.90f, 1.0f));

            return root;
        }

        private static void CreatePneumaticStrut(Transform parent, Vector3 localPos)
        {
            GameObject strut = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strut.name = "Pneumatic_Gas_Strut";
            strut.transform.SetParent(parent, false);
            strut.transform.localPosition = localPos;
            strut.transform.localScale = new Vector3(0.02f, 0.08f, 0.02f);
            strut.transform.localRotation = Quaternion.Euler(-45f, 0, 0);
        }

        public static GameObject CreateBiosafetyCabinet(Material bodyMat, Material glassMat, Material metalMat, Material accentMat)
        {
            GameObject root = new GameObject("BSC_ClassII_TypeA2");

            // Outer Structural Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Outer_Canopy_Frame";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localPosition = new Vector3(0, 1.35f, 0);
            frame.transform.localScale = new Vector3(1.7f, 2.3f, 0.95f);
            if (bodyMat != null) frame.GetComponent<Renderer>().material = bodyMat;

            // Stainless Steel Interior Chamber
            GameObject interior = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interior.name = "Stainless_Work_Chamber";
            interior.transform.SetParent(root.transform, false);
            interior.transform.localPosition = new Vector3(0, 1.25f, 0.04f);
            interior.transform.localScale = new Vector3(1.5f, 0.90f, 0.82f);
            if (metalMat != null) interior.GetComponent<Renderer>().material = metalMat;

            // Angled Glass Sash
            GameObject sash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sash.name = "Sliding_Laminated_Sash";
            sash.transform.SetParent(root.transform, false);
            sash.transform.localPosition = new Vector3(0, 1.35f, 0.44f);
            sash.transform.localScale = new Vector3(1.52f, 0.72f, 0.025f);
            if (glassMat != null) sash.GetComponent<Renderer>().material = glassMat;

            // Front Perforated Intake Grille
            GameObject grille = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grille.name = "Airflow_Intake_Perforated_Grille";
            grille.transform.SetParent(root.transform, false);
            grille.transform.localPosition = new Vector3(0, 0.82f, 0.42f);
            grille.transform.localScale = new Vector3(1.5f, 0.04f, 0.08f);
            if (metalMat != null) grille.GetComponent<Renderer>().material = metalMat;

            // Interior Micropipette Carousel Stand
            GameObject carousel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            carousel.name = "Pipette_Carousel_Stand";
            carousel.transform.SetParent(root.transform, false);
            carousel.transform.localPosition = new Vector3(0.55f, 1.05f, -0.15f);
            carousel.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
            if (accentMat != null) carousel.GetComponent<Renderer>().material = accentMat;

            // 4 Pipettes on Stand
            Color[] pColors = { Color.cyan, Color.yellow, Color.blue, Color.red };
            for (int p = 0; p < 4; p++)
            {
                float ang = p * 90f * Mathf.Deg2Rad;
                GameObject pip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pip.name = $"Pipette_{p}";
                pip.transform.SetParent(carousel.transform, false);
                pip.transform.localPosition = new Vector3(Mathf.Cos(ang) * 0.45f, 0.2f, Mathf.Sin(ang) * 0.45f);
                pip.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
                Material pMat = new Material(Shader.Find("Diffuse"));
                pMat.color = pColors[p];
                pip.GetComponent<Renderer>().material = pMat;
            }

            // LabEquipment Component
            LabEquipment eq = root.AddComponent<LabEquipment>();
            eq.equipmentName = "Biosafety Cabinet (Class II Type A2)";
            eq.equipmentType = EquipmentType.BiosafetyCabinet;
            eq.equipmentId = "BSC-01";
            eq.zoneName = "Zone 2: Wet-Lab Protocol Bay";
            eq.quickHint = "Press [E] Operate Biosafety Cabinet Sash & Laminar Flow";
            eq.movingPart = sash.transform;
            eq.beaconTransform = CreateStationBeacon(root.transform, new Vector3(0, 2.65f, 0), new Color(0.0f, 0.90f, 1.0f));

            return root;
        }


        public static GameObject CreateUltraLowFreezer(Material bodyMat, Material accentMat, Material metalMat, Texture2D displayTex)
        {
            GameObject root = new GameObject("Freezer_Minus80C_UltraLow");

            // Heavy Insulated Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Insulated_Steel_Cabinet";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0, 1.15f, 0);
            body.transform.localScale = new Vector3(1.15f, 2.25f, 1.05f);
            if (bodyMat != null) body.GetComponent<Renderer>().material = bodyMat;

            // Door Hinge Pivot
            GameObject doorPivot = new GameObject("Heavy_Door_Pivot");
            doorPivot.transform.SetParent(root.transform, false);
            doorPivot.transform.localPosition = new Vector3(-0.58f, 1.15f, 0.52f);

            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Insulated_Gasket_Door";
            door.transform.SetParent(doorPivot.transform, false);
            door.transform.localPosition = new Vector3(0.58f, 0, 0.06f);
            door.transform.localScale = new Vector3(1.12f, 2.18f, 0.12f);
            if (bodyMat != null) door.GetComponent<Renderer>().material = bodyMat;

            // Cam-Action Vacuum Relief Handle (Chrome)
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Vacuum_CamLock_Handle";
            handle.transform.SetParent(door.transform, false);
            handle.transform.localPosition = new Vector3(0.48f, 0, 0.10f);
            handle.transform.localScale = new Vector3(0.06f, 0.45f, 0.08f);
            if (metalMat != null) handle.GetComponent<Renderer>().material = metalMat;

            // Digital 7-Segment Temperature Display (-80.0°C)
            GameObject ledDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ledDisplay.name = "LED_7Segment_Temperature_Matrix";
            ledDisplay.transform.SetParent(door.transform, false);
            ledDisplay.transform.localPosition = new Vector3(0, 0.42f, 0.07f);
            ledDisplay.transform.localScale = new Vector3(0.32f, 0.15f, 0.02f);

            Material dMat = new Material(Shader.Find("Diffuse"));
            if (displayTex != null)
            {
                dMat.mainTexture = displayTex;
            }
            dMat.color = Color.white;
            ledDisplay.GetComponent<Renderer>().material = dMat;

            // Bottom Compressor Grille Louvers
            GameObject louver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            louver.name = "Compressor_Ventilation_Louvers";
            louver.transform.SetParent(body.transform, false);
            louver.transform.localPosition = new Vector3(0, -0.42f, 0.51f);
            louver.transform.localScale = new Vector3(0.9f, 0.18f, 0.02f);
            if (accentMat != null) louver.GetComponent<Renderer>().material = accentMat;

            // LabEquipment Component
            LabEquipment eq = root.AddComponent<LabEquipment>();
            eq.equipmentName = "Ultra-Low Temperature Freezer (-80°C)";
            eq.equipmentType = EquipmentType.UltraLowFreezer;
            eq.equipmentId = "FREEZER-ULT-01";
            eq.zoneName = "Zone 1: Reagent Warehouse & Cryo Vault";
            eq.quickHint = "Press [E] Open -80°C Cryo Vault / Inspect Frozen Lots";
            eq.movingPart = doorPivot.transform;
            eq.targetTemp = -80.0f;
            eq.currentTemp = -80.0f;
            eq.beaconTransform = CreateStationBeacon(root.transform, new Vector3(0, 2.45f, 0), new Color(0.0f, 0.85f, 1.0f));

            // Companion LN2 Cryo Dewar Tank beside the -80C Freezer
            GameObject dewar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dewar.name = "LN2_Cryogenic_Dewar_Tank";
            dewar.transform.SetParent(root.transform, false);
            dewar.transform.localPosition = new Vector3(-1.05f, 0.65f, 0.05f);
            dewar.transform.localScale = new Vector3(0.68f, 0.65f, 0.68f);
            if (metalMat != null) dewar.GetComponent<Renderer>().material = metalMat;

            GameObject dewarCollar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dewarCollar.name = "LN2_Vacuum_Neck";
            dewarCollar.transform.SetParent(root.transform, false);
            dewarCollar.transform.localPosition = new Vector3(-1.05f, 1.38f, 0.05f);
            dewarCollar.transform.localScale = new Vector3(0.28f, 0.14f, 0.28f);
            if (accentMat != null) dewarCollar.GetComponent<Renderer>().material = accentMat;

            return root;
        }

        /// <summary>
        /// Zone 1: Smart Reagent Warehouse Rack, 4°C Glass Refrigerator, Flammable Safety Cabinet & Barcode Kiosk.
        /// </summary>
        public static GameObject CreateReagentWarehouseStation(
            Material bodyMat, Material metalMat, Material glassMat, Material accentMat, Material cyanGlowMat, Texture2D kioskTex)
        {
            GameObject root = new GameObject("Zone1_Smart_Reagent_Warehouse");

            // 1. Multi-Tier Heavy Stainless Steel Smart Rack (4 Shelves)
            float rackW = 2.8f;
            float rackD = 0.75f;
            float[] shelfHeights = { 0.22f, 0.78f, 1.34f, 1.90f };

            // 4 Vertical Corner Posts
            Vector3[] postOffsets = {
                new Vector3(-rackW * 0.48f, 1.15f, -rackD * 0.45f),
                new Vector3( rackW * 0.48f, 1.15f, -rackD * 0.45f),
                new Vector3(-rackW * 0.48f, 1.15f,  rackD * 0.45f),
                new Vector3( rackW * 0.48f, 1.15f,  rackD * 0.45f)
            };
            foreach (var pOff in postOffsets)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "Rack_Stanchion_Post";
                post.transform.SetParent(root.transform, false);
                post.transform.localPosition = pOff;
                post.transform.localScale = new Vector3(0.06f, 2.30f, 0.06f);
                if (metalMat != null) post.GetComponent<Renderer>().material = metalMat;
            }

            Color[] reagentPalette = {
                new Color(0.92f, 0.22f, 0.38f), // DMEM Phenol Red Media
                new Color(0.88f, 0.62f, 0.12f), // FBS Amber Serum Bottle
                new Color(0.15f, 0.65f, 0.95f), // PBS / Tris Buffer Blue Cap
                new Color(0.15f, 0.90f, 0.48f), // SYBR Green / Fluorescent MasterMix
                new Color(0.68f, 0.32f, 0.92f)  // Lysis / Plasmid Extraction Kit
            };

            for (int s = 0; s < shelfHeights.Length; s++)
            {
                float sy = shelfHeights[s];
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.name = $"Smart_Shelf_Tier_{s + 1}";
                shelf.transform.SetParent(root.transform, false);
                shelf.transform.localPosition = new Vector3(0, sy, 0);
                shelf.transform.localScale = new Vector3(rackW, 0.04f, rackD);
                if (metalMat != null) shelf.GetComponent<Renderer>().material = metalMat;

                // Glowing LED Inventory Status Strip along front edge of shelf
                GameObject ledStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ledStrip.name = $"Shelf_LED_Strip_{s + 1}";
                ledStrip.transform.SetParent(root.transform, false);
                ledStrip.transform.localPosition = new Vector3(0, sy + 0.022f, rackD * 0.5f + 0.005f);
                ledStrip.transform.localScale = new Vector3(rackW - 0.08f, 0.015f, 0.015f);
                if (cyanGlowMat != null) ledStrip.GetComponent<Renderer>().material = cyanGlowMat;

                // Populate Shelf with Reagent Bottles & Kit Boxes
                for (int b = -4; b <= 4; b++)
                {
                    float bx = b * 0.27f;
                    Color rCol = reagentPalette[Mathf.Abs(b + s * 2) % reagentPalette.Length];
                    Material bMat = new Material(Shader.Find("Diffuse"));
                    bMat.color = rCol;

                    if ((b + s) % 3 == 0)
                    {
                        // Enzyme / Assay Kit Box
                        GameObject kitBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        kitBox.name = $"Reagent_Kit_Box_{s}_{b}";
                        kitBox.transform.SetParent(root.transform, false);
                        kitBox.transform.localPosition = new Vector3(bx, sy + 0.14f, 0.05f);
                        kitBox.transform.localScale = new Vector3(0.20f, 0.22f, 0.28f);
                        kitBox.GetComponent<Renderer>().material = bMat;
                    }
                    else
                    {
                        // Borosilicate Reagent Bottle + Screw Cap
                        GameObject bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        bottle.name = $"Reagent_Bottle_{s}_{b}";
                        bottle.transform.SetParent(root.transform, false);
                        bottle.transform.localPosition = new Vector3(bx, sy + 0.16f, 0.06f);
                        bottle.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
                        bottle.GetComponent<Renderer>().material = bMat;

                        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        cap.name = "Screw_Cap";
                        cap.transform.SetParent(bottle.transform, false);
                        cap.transform.localPosition = new Vector3(0, 1.15f, 0);
                        cap.transform.localScale = new Vector3(0.55f, 0.22f, 0.55f);
                        if (accentMat != null) cap.GetComponent<Renderer>().material = accentMat;
                    }
                }
            }

            // 2. Yellow Hazmat / Flammable Chemical Cabinet on Left Side
            GameObject hazmatCab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazmatCab.name = "Flammable_Hazmat_Safety_Cabinet";
            hazmatCab.transform.SetParent(root.transform, false);
            hazmatCab.transform.localPosition = new Vector3(-2.15f, 0.95f, 0.05f);
            hazmatCab.transform.localScale = new Vector3(1.05f, 1.90f, 0.72f);
            Material yellowMat = new Material(Shader.Find("Diffuse"));
            yellowMat.color = new Color(0.95f, 0.74f, 0.12f);
            hazmatCab.GetComponent<Renderer>().material = yellowMat;

            // Red Warning Diamond Strip on Hazmat Cabinet
            GameObject hazBadge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazBadge.name = "Hazmat_Warning_Sign";
            hazBadge.transform.SetParent(hazmatCab.transform, false);
            hazBadge.transform.localPosition = new Vector3(0, 0.22f, 0.51f);
            hazBadge.transform.localScale = new Vector3(0.55f, 0.12f, 0.02f);
            Material redMat = new Material(Shader.Find("Diffuse"));
            redMat.color = new Color(0.90f, 0.18f, 0.18f);
            hazBadge.GetComponent<Renderer>().material = redMat;

            // 3. Smart Warehouse Barcode & Lot Tracking Kiosk on Right Side
            GameObject kioskStand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kioskStand.name = "Warehouse_Inventory_Kiosk_Pedestal";
            kioskStand.transform.SetParent(root.transform, false);
            kioskStand.transform.localPosition = new Vector3(1.95f, 0.65f, 0.25f);
            kioskStand.transform.localScale = new Vector3(0.48f, 1.30f, 0.42f);
            if (bodyMat != null) kioskStand.GetComponent<Renderer>().material = bodyMat;

            GameObject kioskScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kioskScreen.name = "Warehouse_OLED_Touchscreen";
            kioskScreen.transform.SetParent(root.transform, false);
            kioskScreen.transform.localPosition = new Vector3(1.95f, 1.42f, 0.32f);
            kioskScreen.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            kioskScreen.transform.localScale = new Vector3(0.62f, 0.48f, 0.04f);
            Material kMat = new Material(Shader.Find("Diffuse"));
            if (kioskTex != null) kMat.mainTexture = kioskTex;
            kMat.color = Color.white;
            kioskScreen.GetComponent<Renderer>().material = kMat;

            LabEquipment eq = root.AddComponent<LabEquipment>();
            eq.equipmentName = "Smart Reagent Warehouse & Cold-Chain Kiosk";
            eq.equipmentType = EquipmentType.ReagentWarehouseRack;
            eq.equipmentId = "WAREHOUSE-01";
            eq.zoneName = "Zone 1: Reagent Warehouse & Cryo Vault";
            eq.quickHint = "Press [E] Open Reagent Warehouse Inventory & Lot Tracker";
            eq.interactionRadius = 3.2f;
            eq.beaconTransform = CreateStationBeacon(root.transform, new Vector3(0, 2.35f, 0), new Color(0.15f, 0.95f, 0.55f));

            return root;
        }

        /// <summary>
        /// Zone 2: Real-Time qPCR Thermal Cycler & Automated 3-Axis Liquid Handling Robot.
        /// </summary>
        public static GameObject CreatePcrAndLiquidHandlerStation(
            Material bodyMat, Material accentMat, Material glassMat, Material metalMat, Material cyanGlowMat, Texture2D pcrTex)
        {
            GameObject root = new GameObject("Zone2_qPCR_And_LiquidHandler");

            // 1. Real-Time qPCR Thermal Cycler (Left side of East Bench)
            GameObject pcrBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pcrBody.name = "qPCR_Thermal_Cycler_Chassis";
            pcrBody.transform.SetParent(root.transform, false);
            pcrBody.transform.localPosition = new Vector3(0, 0.22f, -1.2f);
            pcrBody.transform.localScale = new Vector3(0.46f, 0.38f, 0.52f);
            if (bodyMat != null) pcrBody.GetComponent<Renderer>().material = bodyMat;

            GameObject pcrScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pcrScreen.name = "qPCR_Amplification_Screen";
            pcrScreen.transform.SetParent(pcrBody.transform, false);
            pcrScreen.transform.localPosition = new Vector3(-0.48f, 0.25f, 0f);
            pcrScreen.transform.localRotation = Quaternion.Euler(0f, 90f, -22f);
            pcrScreen.transform.localScale = new Vector3(0.68f, 0.42f, 0.04f);
            Material pcrMat = new Material(Shader.Find("Diffuse"));
            if (pcrTex != null) pcrMat.mainTexture = pcrTex;
            pcrMat.color = Color.white;
            pcrScreen.GetComponent<Renderer>().material = pcrMat;

            // 2. Automated 3-Axis Liquid Handling Robot (Right side of East Bench)
            GameObject robotBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            robotBase.name = "LiquidHandler_Deck_Base";
            robotBase.transform.SetParent(root.transform, false);
            robotBase.transform.localPosition = new Vector3(0, 0.08f, 0.85f);
            robotBase.transform.localScale = new Vector3(0.82f, 0.12f, 1.35f);
            if (metalMat != null) robotBase.GetComponent<Renderer>().material = metalMat;

            // Transparent Safety Enclosure Hood
            GameObject enclosure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enclosure.name = "Robot_Acrylic_Safety_Shield";
            enclosure.transform.SetParent(root.transform, false);
            enclosure.transform.localPosition = new Vector3(0, 0.48f, 0.85f);
            enclosure.transform.localScale = new Vector3(0.80f, 0.68f, 1.32f);
            if (glassMat != null) enclosure.GetComponent<Renderer>().material = glassMat;

            // Moving 3-Axis Pipetting Gantry Arm
            GameObject gantryArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gantryArm.name = "Robot_Pipetting_Gantry_Head";
            gantryArm.transform.SetParent(root.transform, false);
            gantryArm.transform.localPosition = new Vector3(0, 0.62f, 0.85f);
            gantryArm.transform.localScale = new Vector3(0.65f, 0.08f, 0.14f);
            if (accentMat != null) gantryArm.GetComponent<Renderer>().material = accentMat;

            // 96-Well Microplates on Deck
            for (int mp = -1; mp <= 1; mp++)
            {
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = $"Microplate_96Well_{mp + 2}";
                plate.transform.SetParent(root.transform, false);
                plate.transform.localPosition = new Vector3(0.05f, 0.16f, 0.85f + mp * 0.34f);
                plate.transform.localScale = new Vector3(0.24f, 0.03f, 0.18f);
                if (cyanGlowMat != null) plate.GetComponent<Renderer>().material = cyanGlowMat;
            }

            LabEquipment eq = root.AddComponent<LabEquipment>();
            eq.equipmentName = "qPCR Thermal Cycler & Automated Liquid Handler";
            eq.equipmentType = EquipmentType.LiquidHandlerRobot;
            eq.equipmentId = "EQ-PCR-01";
            eq.zoneName = "Zone 2: Wet-Lab Protocol Bay";
            eq.quickHint = "Press [E] Run qPCR / Automated Liquid Handler Assay";
            eq.movingPart = gantryArm.transform;
            eq.durationSeconds = 30f;
            eq.beaconTransform = CreateStationBeacon(root.transform, new Vector3(0, 1.25f, 0.2f), new Color(0.0f, 0.90f, 1.0f));

            return root;
        }

        /// <summary>
        /// Zone 3: Dry-Lab Manuscript & Git Version Control Command Desk with Triple Ultrawide Monitors
        /// and a 3D Holographic Git Commit Graph Sculpture.
        /// </summary>
        public static GameObject CreateManuscriptCommandDesk(
            Material deskMat, Material metalMat, Material bodyMat, Material cyanGlowMat,
            Texture2D gitMonitorTex, Texture2D holoTex)
        {
            GameObject root = new GameObject("Zone3_Manuscript_Git_Command_Desk");

            // 1. L-Shaped Executive Carbon-Slate Desk Surface
            GameObject mainDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainDesk.name = "Manuscript_Main_Desk_Top";
            mainDesk.transform.SetParent(root.transform, false);
            mainDesk.transform.localPosition = new Vector3(0, 0.78f, 0);
            mainDesk.transform.localScale = new Vector3(2.60f, 0.07f, 1.05f);
            if (deskMat != null) mainDesk.GetComponent<Renderer>().material = deskMat;

            GameObject sideReturn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideReturn.name = "Manuscript_Side_Return_Wing";
            sideReturn.transform.SetParent(root.transform, false);
            sideReturn.transform.localPosition = new Vector3(1.15f, 0.78f, 0.95f);
            sideReturn.transform.localScale = new Vector3(0.85f, 0.07f, 1.25f);
            if (deskMat != null) sideReturn.GetComponent<Renderer>().material = deskMat;

            // Brushed Steel Desk Legs
            Vector3[] legPositions = {
                new Vector3(-1.18f, 0.38f, -0.42f),
                new Vector3( 1.18f, 0.38f, -0.42f),
                new Vector3(-1.18f, 0.38f,  0.42f),
                new Vector3( 1.45f, 0.38f,  1.45f)
            };
            foreach (var lp in legPositions)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "Desk_Support_Pillar";
                leg.transform.SetParent(root.transform, false);
                leg.transform.localPosition = lp;
                leg.transform.localScale = new Vector3(0.09f, 0.38f, 0.09f);
                if (metalMat != null) leg.GetComponent<Renderer>().material = metalMat;
            }

            // Neon Cyan Edge Trim along Desk Front
            GameObject deskTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deskTrim.name = "Desk_Neon_Edge_Trim";
            deskTrim.transform.SetParent(root.transform, false);
            deskTrim.transform.localPosition = new Vector3(0, 0.78f, 0.53f);
            deskTrim.transform.localScale = new Vector3(2.60f, 0.025f, 0.02f);
            if (cyanGlowMat != null) deskTrim.GetComponent<Renderer>().material = cyanGlowMat;

            // 2. Triple Curved Ultrawide Monitors (PaperGit Diff, Branch Graph, and Provenance Matrix)
            Material gitScreenMat = new Material(Shader.Find("Diffuse"));
            if (gitMonitorTex != null) gitScreenMat.mainTexture = gitMonitorTex;
            gitScreenMat.color = Color.white;

            Material holoScreenMat = new Material(Shader.Find("Diffuse"));
            if (holoTex != null) holoScreenMat.mainTexture = holoTex;
            holoScreenMat.color = Color.white;

            // Center 38-inch Ultrawide Monitor (Manuscript Diff & Editor)
            CreateMonitor(root.transform, new Vector3(0f, 1.16f, -0.22f), Quaternion.Euler(0f, 0f, 0f),
                new Vector3(1.12f, 0.52f, 0.03f), gitScreenMat, metalMat, "Monitor_Center_PaperGit_Diff");

            // Left Angled Monitor (Git Commit Tree & Reviewer Tracker)
            CreateMonitor(root.transform, new Vector3(-0.98f, 1.16f, -0.02f), Quaternion.Euler(0f, -24f, 0f),
                new Vector3(0.82f, 0.48f, 0.03f), gitScreenMat, metalMat, "Monitor_Left_Branch_Graph");

            // Right Angled Monitor (Linked Protocol & Reagent Lot Provenance)
            CreateMonitor(root.transform, new Vector3(0.98f, 1.16f, -0.02f), Quaternion.Euler(0f, 24f, 0f),
                new Vector3(0.82f, 0.48f, 0.03f), holoScreenMat, metalMat, "Monitor_Right_Provenance_Matrix");

            // Mechanical Keyboard & Mouse Pad on Desk
            GameObject keyboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyboard.name = "Mechanical_Research_Keyboard";
            keyboard.transform.SetParent(root.transform, false);
            keyboard.transform.localPosition = new Vector3(0f, 0.83f, 0.22f);
            keyboard.transform.localScale = new Vector3(0.46f, 0.02f, 0.16f);
            if (bodyMat != null) keyboard.GetComponent<Renderer>().material = bodyMat;

            // 3. Ergonomic High-Back Researcher Chair
            GameObject chairSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairSeat.name = "Ergonomic_Chair_Seat";
            chairSeat.transform.SetParent(root.transform, false);
            chairSeat.transform.localPosition = new Vector3(0f, 0.48f, 0.95f);
            chairSeat.transform.localScale = new Vector3(0.56f, 0.07f, 0.54f);
            if (deskMat != null) chairSeat.GetComponent<Renderer>().material = deskMat;

            GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairBack.name = "Ergonomic_Chair_Backrest";
            chairBack.transform.SetParent(root.transform, false);
            chairBack.transform.localPosition = new Vector3(0f, 0.92f, 1.20f);
            chairBack.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            chairBack.transform.localScale = new Vector3(0.52f, 0.82f, 0.07f);
            if (deskMat != null) chairBack.GetComponent<Renderer>().material = deskMat;

            // 4. Floating 3D Holographic Git Commit Tree Sculpture on Side Return Wing
            GameObject gitHoloRoot = new GameObject("Holo_3D_Git_Commit_Tree");
            gitHoloRoot.transform.SetParent(root.transform, false);
            gitHoloRoot.transform.localPosition = new Vector3(1.15f, 1.25f, 1.05f);

            Material violetMat = new Material(Shader.Find("Diffuse"));
            violetMat.color = new Color(0.78f, 0.32f, 1.0f);

            // Main Branch 3D Commit Spheres
            for (int c = 0; c < 4; c++)
            {
                GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                node.name = $"Git_Main_Commit_Node_{c}";
                node.transform.SetParent(gitHoloRoot.transform, false);
                node.transform.localPosition = new Vector3(0f, c * 0.22f, -0.30f + c * 0.18f);
                node.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
                if (cyanGlowMat != null) node.GetComponent<Renderer>().material = cyanGlowMat;
            }

            // Revision Branch 3D Commit Spheres
            for (int r = 0; r < 3; r++)
            {
                GameObject rNode = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rNode.name = $"Git_Revision_Commit_Node_{r}";
                rNode.transform.SetParent(gitHoloRoot.transform, false);
                rNode.transform.localPosition = new Vector3(0.22f, 0.15f + r * 0.22f, -0.12f + r * 0.18f);
                rNode.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
                rNode.GetComponent<Renderer>().material = violetMat;
            }

            LabEquipment eq = root.AddComponent<LabEquipment>();
            eq.equipmentName = "PaperGit Manuscript Version Control Workstation";
            eq.equipmentType = EquipmentType.ManuscriptCommandDesk;
            eq.equipmentId = "PAPER-DESK-01";
            eq.zoneName = "Zone 3: PaperGit Manuscript Office";
            eq.quickHint = "Press [E] or [Tab] Open PaperGit Version Control & Diff Studio";
            eq.interactionRadius = 3.2f;
            eq.rotorTransform = gitHoloRoot.transform;
            eq.beaconTransform = CreateStationBeacon(root.transform, new Vector3(0, 1.95f, 0), new Color(0.75f, 0.38f, 1.0f));

            return root;
        }

        private static void CreateMonitor(
            Transform parent, Vector3 localPos, Quaternion localRot, Vector3 screenScale,
            Material screenMat, Material standMat, string name)
        {
            GameObject mon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mon.name = name;
            mon.transform.SetParent(parent, false);
            mon.transform.localPosition = localPos;
            mon.transform.localRotation = localRot;
            mon.transform.localScale = screenScale;
            if (screenMat != null) mon.GetComponent<Renderer>().material = screenMat;

            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = name + "_Stand";
            arm.transform.SetParent(parent, false);
            arm.transform.localPosition = new Vector3(localPos.x, 0.92f, localPos.z - 0.06f);
            arm.transform.localScale = new Vector3(0.05f, 0.14f, 0.05f);
            if (standMat != null) arm.GetComponent<Renderer>().material = standMat;
        }

        public static Transform CreateStationBeacon(Transform parent, Vector3 localPos, Color color)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Station_3D_Beacon";
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = localPos;
            ring.transform.localScale = new Vector3(0.38f, 0.02f, 0.38f);
            if (ring.GetComponent<Collider>()) Object.Destroy(ring.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Diffuse"));
            mat.color = color;
            ring.GetComponent<Renderer>().material = mat;
            return ring.transform;
        }
    }
}

