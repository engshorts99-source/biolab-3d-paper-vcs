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
            eq.movingPart = lidPivot.transform;
            eq.rotorTransform = rotor.transform;
            eq.targetRpm = 14000f;
            eq.targetTemp = 4.0f;
            eq.durationSeconds = 180f;

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
            eq.movingPart = sash.transform;

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
            eq.movingPart = doorPivot.transform;
            eq.targetTemp = -80.0f;
            eq.currentTemp = -80.0f;

            return root;
        }
    }
}
