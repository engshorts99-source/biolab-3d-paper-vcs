using System.Collections.Generic;
using UnityEngine;

namespace BioLab.Player
{
    /// <summary>
    /// Master 3D Anime Humanoid Sculptor.
    /// Sculpted directly against the 4 orthographic views (Front, Left, Right, Back)
    /// of the concept benchmarks (game_male_character_concept and game_female_character_concept).
    /// Generates high-poly organic anime cranium & face with planar UVs, full voluminous
    /// 3D hair (bangs, side locks, crown spikes, nape locks, ponytail), high standing coat collar,
    /// tailored lab coat with inner dark tech suit, flowing draped coat tails, gauntlets,
    /// cyan nitrile gloves, floating wrist hologram, and cleanroom cyber combat boots.
    /// </summary>
    public static class AnimeModelSculptor
    {
        public static GameObject SculptMaleResearcher(
            Material suitMat, Material coatMat, Material coatAccentMat, 
            Material skinMat, Material faceMat, Material hairMat, Material glassMat, 
            Material bootMat, Material gloveMat, Material cyanGlowMat)
        {
            GameObject root = new GameObject("Male_Researcher_Sculpt");
            var animator = root.AddComponent<ProceduralHumanoidAnimator>();

            // Pelvis / Spine root (y = 0.96m)
            GameObject spine = new GameObject("Spine_Root");
            spine.transform.SetParent(root.transform, false);
            spine.transform.localPosition = new Vector3(0f, 0.96f, 0f);
            animator.spinePivot = spine.transform;

            // =========================================================================
            // 1. Head & Anime Facial Sculpt (Front, Side, and Back Contours)
            // =========================================================================
            GameObject headRoot = new GameObject("Head_Root");
            headRoot.transform.SetParent(spine.transform, false);
            headRoot.transform.localPosition = new Vector3(0f, 0.58f, 0f);

            // Sculpted Anime Head Mesh (24 lat x 32 lon segments = smooth organic curve)
            Mesh headMesh = BuildSculptedHeadMesh(0.112f, 0.142f, 0.124f, isFemale: false);
            CreateMeshObj("Anime_Head", headMesh, faceMat ?? skinMat, headRoot.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // =========================================================================
            // 2. Full Voluminous 3D Anime Hair System (Navy / Dark Slate)
            // =========================================================================
            GameObject hairRoot = new GameObject("Hair_System");
            hairRoot.transform.SetParent(headRoot.transform, false);

            // Base Cranial Hair Helmet Cap (Ensures full skull coverage, no bald spots)
            Mesh skullCap = BuildSkullCapMesh(0.124f, 0.138f, 0.132f, isFemale: false);
            CreateMeshObj("Hair_Skull_Cap", skullCap, hairMat, hairRoot.transform,
                new Vector3(0f, 0.015f, -0.012f), Quaternion.identity, Vector3.one);

            // FRONT FRINGE & BANGS (Cascading swept locks across forehead)
            // Bang 1: Left sweep (cascading from crown across forehead to left eye/cheek)
            Mesh b1 = BuildCurvedLockMesh(0.038f, 0.024f, 0.17f, new Vector3(-0.065f, -0.06f, 0.035f), 10, 8);
            CreateMeshObj("Hair_Bang_L1", b1, hairMat, hairRoot.transform,
                new Vector3(-0.025f, 0.095f, 0.095f), Quaternion.Euler(72f, -28f, 12f), Vector3.one);

            // Bang 2: Left outer sweep
            Mesh b2 = BuildCurvedLockMesh(0.034f, 0.022f, 0.15f, new Vector3(-0.055f, -0.05f, 0.025f), 8, 6);
            CreateMeshObj("Hair_Bang_L2", b2, hairMat, hairRoot.transform,
                new Vector3(-0.055f, 0.085f, 0.090f), Quaternion.Euler(68f, -38f, 15f), Vector3.one);

            // Bang 3: Central Signature Tuft (Dipping sharply between the eyes)
            Mesh bCenter = BuildCurvedLockMesh(0.032f, 0.020f, 0.155f, new Vector3(-0.012f, -0.055f, 0.028f), 10, 8);
            CreateMeshObj("Hair_Bang_Center", bCenter, hairMat, hairRoot.transform,
                new Vector3(0.005f, 0.105f, 0.108f), Quaternion.Euler(78f, 4f, -4f), Vector3.one);

            // Bang 4: Right parted sweep (Sweeping over right eye towards right temple)
            Mesh bR1 = BuildCurvedLockMesh(0.036f, 0.022f, 0.15f, new Vector3(0.055f, -0.045f, 0.030f), 10, 8);
            CreateMeshObj("Hair_Bang_R1", bR1, hairMat, hairRoot.transform,
                new Vector3(0.035f, 0.098f, 0.098f), Quaternion.Euler(70f, 26f, -12f), Vector3.one);

            // Bang 5: Right outer cheek fringe
            Mesh bR2 = BuildCurvedLockMesh(0.032f, 0.020f, 0.14f, new Vector3(0.05f, -0.04f, 0.02f), 8, 6);
            CreateMeshObj("Hair_Bang_R2", bR2, hairMat, hairRoot.transform,
                new Vector3(0.065f, 0.085f, 0.088f), Quaternion.Euler(65f, 40f, -18f), Vector3.one);

            // FACE-FRAMING SIDE LOCKS (Long elegant strands framing the jawline)
            // Left Side Lock Main
            Mesh sideL1 = BuildCurvedLockMesh(0.038f, 0.022f, 0.22f, new Vector3(-0.022f, -0.09f, 0.025f), 10, 8);
            CreateMeshObj("Hair_SideLock_L", sideL1, hairMat, hairRoot.transform,
                new Vector3(-0.112f, 0.045f, 0.032f), Quaternion.Euler(85f, -12f, 15f), Vector3.one);

            // Right Side Lock Main
            Mesh sideR1 = BuildCurvedLockMesh(0.038f, 0.022f, 0.22f, new Vector3(0.022f, -0.09f, 0.025f), 10, 8);
            CreateMeshObj("Hair_SideLock_R", sideR1, hairMat, hairRoot.transform,
                new Vector3(0.112f, 0.045f, 0.032f), Quaternion.Euler(85f, 12f, -15f), Vector3.one);

            // CROWN & CREST SPIKES (Stylized dynamic tufts along cranial crest)
            Mesh cSpike1 = BuildCurvedLockMesh(0.040f, 0.028f, 0.16f, new Vector3(-0.03f, 0.05f, -0.08f), 8, 6);
            CreateMeshObj("Hair_Crest_1", cSpike1, hairMat, hairRoot.transform,
                new Vector3(-0.035f, 0.14f, -0.015f), Quaternion.Euler(-55f, -22f, 8f), Vector3.one);

            Mesh cSpike2 = BuildCurvedLockMesh(0.040f, 0.028f, 0.16f, new Vector3(0.03f, 0.05f, -0.08f), 8, 6);
            CreateMeshObj("Hair_Crest_2", cSpike2, hairMat, hairRoot.transform,
                new Vector3(0.035f, 0.14f, -0.015f), Quaternion.Euler(-52f, 24f, -8f), Vector3.one);

            Mesh cSpikeTop = BuildCurvedLockMesh(0.036f, 0.024f, 0.14f, new Vector3(0f, 0.06f, -0.06f), 8, 6);
            CreateMeshObj("Hair_Crest_Top", cSpikeTop, hairMat, hairRoot.transform,
                new Vector3(0f, 0.155f, -0.03f), Quaternion.Euler(-42f, 0f, 0f), Vector3.one);

            // NAPE & BACK LOCKS (Cascading down the back of the skull into coat collar)
            Mesh nape1 = BuildCurvedLockMesh(0.055f, 0.028f, 0.18f, new Vector3(-0.015f, -0.06f, -0.04f), 8, 6);
            CreateMeshObj("Hair_Nape_L", nape1, hairMat, hairRoot.transform,
                new Vector3(-0.04f, 0.01f, -0.105f), Quaternion.Euler(118f, -10f, 0f), Vector3.one);

            Mesh nape2 = BuildCurvedLockMesh(0.055f, 0.028f, 0.18f, new Vector3(0.015f, -0.06f, -0.04f), 8, 6);
            CreateMeshObj("Hair_Nape_R", nape2, hairMat, hairRoot.transform,
                new Vector3(0.04f, 0.01f, -0.105f), Quaternion.Euler(118f, 10f, 0f), Vector3.one);

            // =========================================================================
            // 3. Laboratory Safety Glasses / Cyber Visor
            // =========================================================================
            Mesh visorMesh = BuildCurvedVisorMesh(0.21f, 0.044f, 0.038f);
            CreateMeshObj("Safety_Visor", visorMesh, glassMat, headRoot.transform,
                new Vector3(0f, 0.018f, 0.118f), Quaternion.identity, Vector3.one);

            Mesh visorFrame = BuildCurvedVisorMesh(0.215f, 0.010f, 0.042f);
            CreateMeshObj("Visor_Frame", visorFrame, coatAccentMat, headRoot.transform,
                new Vector3(0f, 0.038f, 0.116f), Quaternion.identity, Vector3.one);

            // =========================================================================
            // 4. Neck & High Standing Cyber Coat Collar
            // =========================================================================
            // Neck
            Mesh neck = BuildTaperedTube(0.048f, 0.054f, 0.12f, 16);
            CreateMeshObj("Neck", neck, skinMat, spine.transform,
                new Vector3(0f, 0.54f, 0f), Quaternion.identity, Vector3.one);

            // High Standing Open Coat Collar (Rises to jaw level, flares outward 20°)
            Mesh collar = BuildStandingCollarMesh(0.095f, 0.104f, 0.125f, 19f, 65f);
            CreateMeshObj("High_Standing_Collar", collar, coatMat, spine.transform,
                new Vector3(0f, 0.44f, 0.01f), Quaternion.Euler(-6f, 0f, 0f), Vector3.one);

            // Glowing Cyan Piping on Collar Rim
            Mesh collarTrim = BuildStandingCollarMesh(0.097f, 0.106f, 0.018f, 19f, 65f);
            CreateMeshObj("Collar_Neon_Trim", collarTrim, cyanGlowMat ?? coatAccentMat, spine.transform,
                new Vector3(0f, 0.545f, 0.01f), Quaternion.Euler(-6f, 0f, 0f), Vector3.one);

            // =========================================================================
            // 5. Tailored Lab Coat over Compression Tech Suit
            // =========================================================================
            // Torso Main Coat
            Mesh torsoCoat = BuildContouredTorsoMesh(0.19f, 0.155f, 0.46f, 16);
            CreateMeshObj("Coat_Torso", torsoCoat, coatMat, spine.transform,
                new Vector3(0f, 0.42f, 0f), Quaternion.identity, new Vector3(1.08f, 1f, 0.70f));

            // Dark Inner Tech Compression Suit (Visible in the open front V of the coat)
            Mesh techSuitChest = BuildContouredTorsoMesh(0.12f, 0.085f, 0.38f, 12);
            CreateMeshObj("Inner_Tech_Suit", techSuitChest, suitMat, spine.transform,
                new Vector3(0f, 0.41f, 0.028f), Quaternion.identity, new Vector3(0.92f, 1f, 0.65f));

            // Luminescent Cyan Circuit Conduits on Chest (matching concept art)
            Mesh chestConduit = BuildTaperedTube(0.016f, 0.008f, 0.28f, 8);
            CreateMeshObj("Chest_Neon_Circuit", chestConduit, cyanGlowMat ?? coatAccentMat, spine.transform,
                new Vector3(0f, 0.39f, 0.088f), Quaternion.identity, new Vector3(0.4f, 1f, 0.2f));

            // Tactical Utility Belt at waist
            Mesh belt = BuildStandingCollarMesh(0.165f, 0.122f, 0.052f, 0f, 0f);
            CreateMeshObj("Utility_Belt", belt, coatAccentMat, spine.transform,
                new Vector3(0f, 0.02f, 0f), Quaternion.identity, Vector3.one);

            // Belt Buckle & Tactical Holster Pouch on right hip
            Mesh pouch = BuildSculptedPouchMesh(0.065f, 0.045f, 0.085f);
            CreateMeshObj("Belt_Pouch_R", pouch, coatAccentMat, spine.transform,
                new Vector3(0.14f, 0.01f, 0.04f), Quaternion.Euler(0f, 32f, 0f), Vector3.one);

            // =========================================================================
            // 6. Flared Draped Asymmetrical Coat Tails
            // =========================================================================
            // Left Coat Tail (Drapes gracefully around hip and past knee with diagonal cut)
            GameObject tailL = new GameObject("CoatTail_L_Pivot");
            tailL.transform.SetParent(spine.transform, false);
            tailL.transform.localPosition = new Vector3(-0.06f, 0.03f, -0.01f);
            animator.coatTailLPivot = tailL.transform;

            Mesh tailLMesh = BuildDrapedCoatTailMesh(0.20f, 0.32f, 0.64f, 15f, 12f, isLeft: true);
            CreateMeshObj("CoatTail_L", tailLMesh, coatMat, tailL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Right Coat Tail
            GameObject tailR = new GameObject("CoatTail_R_Pivot");
            tailR.transform.SetParent(spine.transform, false);
            tailR.transform.localPosition = new Vector3(0.06f, 0.03f, -0.01f);
            animator.coatTailRPivot = tailR.transform;

            Mesh tailRMesh = BuildDrapedCoatTailMesh(0.20f, 0.32f, 0.64f, 15f, 12f, isLeft: false);
            CreateMeshObj("CoatTail_R", tailRMesh, coatMat, tailR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Back Coat Tail (Split center vent)
            GameObject tailB = new GameObject("CoatTail_Back_Pivot");
            tailB.transform.SetParent(spine.transform, false);
            tailB.transform.localPosition = new Vector3(0f, 0.03f, -0.09f);
            animator.coatTailBackPivot = tailB.transform;

            Mesh tailBMesh = BuildDrapedCoatTailMesh(0.26f, 0.36f, 0.60f, 18f, 0f, isLeft: true);
            CreateMeshObj("CoatTail_Back", tailBMesh, coatMat, tailB.transform,
                Vector3.zero, Quaternion.Euler(0f, 90f, 0f), Vector3.one);

            // =========================================================================
            // 7. Shoulders, Gauntlets, Cyan Nitrile Gloves & Wrist Holo HUD
            // =========================================================================
            // LEFT ARM
            GameObject armL = new GameObject("Arm_L_Pivot");
            armL.transform.SetParent(spine.transform, false);
            armL.transform.localPosition = new Vector3(-0.22f, 0.40f, 0f);
            animator.leftArmPivot = armL.transform;

            // Epaulette
            Mesh epaulette = BuildCurvedLockMesh(0.052f, 0.038f, 0.13f, new Vector3(-0.02f, -0.01f, 0f), 8, 6);
            CreateMeshObj("Epaulette_L", epaulette, coatAccentMat, armL.transform,
                Vector3.zero, Quaternion.Euler(0f, 0f, 75f), Vector3.one);

            // Upper Arm (White coat sleeve)
            Mesh upperArm = BuildTaperedTube(0.054f, 0.046f, 0.27f, 14);
            CreateMeshObj("UpperArm_L", upperArm, coatMat, armL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Left Forearm
            GameObject forearmL = new GameObject("Forearm_L_Pivot");
            forearmL.transform.SetParent(armL.transform, false);
            forearmL.transform.localPosition = new Vector3(0f, -0.27f, 0f);
            animator.leftForearmPivot = forearmL.transform;

            Mesh forearm = BuildTaperedTube(0.050f, 0.042f, 0.23f, 14);
            CreateMeshObj("Forearm_L", forearm, coatMat, forearmL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Gauntlet Cuff with Cyan Neon Ring
            Mesh cuff = BuildStandingCollarMesh(0.056f, 0.056f, 0.055f, 16f, 0f);
            CreateMeshObj("Gauntlet_Cuff_L", cuff, coatAccentMat, forearmL.transform,
                new Vector3(0f, -0.17f, 0f), Quaternion.identity, Vector3.one);

            Mesh cuffTrim = BuildStandingCollarMesh(0.058f, 0.058f, 0.014f, 16f, 0f);
            CreateMeshObj("Cuff_Neon_L", cuffTrim, cyanGlowMat ?? coatAccentMat, forearmL.transform,
                new Vector3(0f, -0.14f, 0f), Quaternion.identity, Vector3.one);

            // Cyan Nitrile Glove
            Mesh glove = BuildTaperedTube(0.044f, 0.030f, 0.13f, 12);
            CreateMeshObj("Glove_L", glove, gloveMat, forearmL.transform,
                new Vector3(0f, -0.21f, 0f), Quaternion.identity, Vector3.one);

            // Floating Holographic Wrist HUD Display (Floating 5cm above forearm)
            Mesh holoQuad = BuildHologramQuadMesh(0.14f, 0.095f);
            CreateMeshObj("Wrist_Holo_HUD", holoQuad, cyanGlowMat ?? coatAccentMat, forearmL.transform,
                new Vector3(-0.065f, -0.15f, 0.045f), Quaternion.Euler(20f, -42f, -15f), Vector3.one);

            // RIGHT ARM
            GameObject armR = new GameObject("Arm_R_Pivot");
            armR.transform.SetParent(spine.transform, false);
            armR.transform.localPosition = new Vector3(0.22f, 0.40f, 0f);
            animator.rightArmPivot = armR.transform;

            CreateMeshObj("Epaulette_R", epaulette, coatAccentMat, armR.transform,
                Vector3.zero, Quaternion.Euler(0f, 0f, -75f), Vector3.one);

            CreateMeshObj("UpperArm_R", upperArm, coatMat, armR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Right Forearm
            GameObject forearmR = new GameObject("Forearm_R_Pivot");
            forearmR.transform.SetParent(armR.transform, false);
            forearmR.transform.localPosition = new Vector3(0f, -0.27f, 0f);
            animator.rightForearmPivot = forearmR.transform;

            CreateMeshObj("Forearm_R", forearm, coatMat, forearmR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            CreateMeshObj("Gauntlet_Cuff_R", cuff, coatAccentMat, forearmR.transform,
                new Vector3(0f, -0.17f, 0f), Quaternion.identity, Vector3.one);

            CreateMeshObj("Glove_R", glove, gloveMat, forearmR.transform,
                new Vector3(0f, -0.21f, 0f), Quaternion.identity, Vector3.one);

            // Laboratory Data Tablet / Datapad (Held in right hand)
            Mesh tablet = BuildTabletMesh(0.15f, 0.015f, 0.22f);
            CreateMeshObj("Lab_Tablet", tablet, coatAccentMat, forearmR.transform,
                new Vector3(0.045f, -0.28f, 0.045f), Quaternion.Euler(28f, 16f, 0f), Vector3.one);

            // =========================================================================
            // 8. Articulated Legs, Knee Armor & Cleanroom Cyber Combat Boots
            // =========================================================================
            // LEFT LEG
            GameObject legL = new GameObject("Leg_L_Pivot");
            legL.transform.SetParent(spine.transform, false);
            legL.transform.localPosition = new Vector3(-0.11f, 0.02f, 0f);
            animator.leftLegPivot = legL.transform;

            // Thigh (Dark tech suit with cyan conduit)
            Mesh thigh = BuildTaperedTube(0.082f, 0.060f, 0.45f, 14);
            CreateMeshObj("Thigh_L", thigh, suitMat, legL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            Mesh legNeon = BuildTaperedTube(0.012f, 0.008f, 0.40f, 6);
            CreateMeshObj("Thigh_Neon_L", legNeon, cyanGlowMat ?? coatAccentMat, legL.transform,
                new Vector3(-0.068f, -0.02f, 0f), Quaternion.identity, Vector3.one);

            // Left Knee & Calf
            GameObject kneeL = new GameObject("Knee_L_Pivot");
            kneeL.transform.SetParent(legL.transform, false);
            kneeL.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            animator.leftKneePivot = kneeL.transform;

            // Articulated Knee Armor Guard Plate
            Mesh kneeGuard = BuildCurvedLockMesh(0.048f, 0.024f, 0.09f, new Vector3(0f, 0f, 0.025f), 8, 6);
            CreateMeshObj("Knee_Armor_L", kneeGuard, coatAccentMat, kneeL.transform,
                new Vector3(0f, 0f, 0.052f), Quaternion.Euler(90f, 0f, 0f), Vector3.one);

            // Calf
            Mesh calf = BuildTaperedTube(0.060f, 0.046f, 0.43f, 14);
            CreateMeshObj("Calf_L", calf, suitMat, kneeL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Cyber Cleanroom Combat Boot
            Mesh boot = BuildCyberBootMesh(0.125f, 0.25f, 0.23f, 0.048f);
            CreateMeshObj("Cyber_Boot_L", boot, bootMat, kneeL.transform,
                new Vector3(0f, -0.44f, 0.035f), Quaternion.identity, Vector3.one);

            Mesh bootNeon = BuildCyberBootMesh(0.13f, 0.255f, 0.02f, 0.016f);
            CreateMeshObj("Boot_Neon_L", bootNeon, cyanGlowMat ?? coatAccentMat, kneeL.transform,
                new Vector3(0f, -0.44f, 0.035f), Quaternion.identity, Vector3.one);

            // RIGHT LEG
            GameObject legR = new GameObject("Leg_R_Pivot");
            legR.transform.SetParent(spine.transform, false);
            legR.transform.localPosition = new Vector3(0.11f, 0.02f, 0f);
            animator.rightLegPivot = legR.transform;

            CreateMeshObj("Thigh_R", thigh, suitMat, legR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            CreateMeshObj("Thigh_Neon_R", legNeon, cyanGlowMat ?? coatAccentMat, legR.transform,
                new Vector3(0.068f, -0.02f, 0f), Quaternion.identity, Vector3.one);

            // Right Knee & Calf
            GameObject kneeR = new GameObject("Knee_R_Pivot");
            kneeR.transform.SetParent(legR.transform, false);
            kneeR.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            animator.rightKneePivot = kneeR.transform;

            CreateMeshObj("Knee_Armor_R", kneeGuard, coatAccentMat, kneeR.transform,
                new Vector3(0f, 0f, 0.052f), Quaternion.Euler(90f, 0f, 0f), Vector3.one);

            CreateMeshObj("Calf_R", calf, suitMat, kneeR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            CreateMeshObj("Cyber_Boot_R", boot, bootMat, kneeR.transform,
                new Vector3(0f, -0.44f, 0.035f), Quaternion.identity, Vector3.one);

            CreateMeshObj("Boot_Neon_R", bootNeon, cyanGlowMat ?? coatAccentMat, kneeR.transform,
                new Vector3(0f, -0.44f, 0.035f), Quaternion.identity, Vector3.one);

            return root;
        }

        public static GameObject SculptFemaleResearcher(
            Material suitMat, Material coatMat, Material coatAccentMat, 
            Material skinMat, Material faceMat, Material hairMat, Material glassMat, 
            Material bootMat, Material gloveMat, Material cyanGlowMat)
        {
            GameObject root = new GameObject("Female_Researcher_Sculpt");
            var animator = root.AddComponent<ProceduralHumanoidAnimator>();

            // Pelvis / Spine root (y = 0.88m)
            GameObject spine = new GameObject("Spine_Root");
            spine.transform.SetParent(root.transform, false);
            spine.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            animator.spinePivot = spine.transform;

            // Head Root
            GameObject headRoot = new GameObject("Head_Root");
            headRoot.transform.SetParent(spine.transform, false);
            headRoot.transform.localPosition = new Vector3(0f, 0.54f, 0f);

            Mesh headMesh = BuildSculptedHeadMesh(0.104f, 0.134f, 0.116f, isFemale: true);
            CreateMeshObj("Anime_Head", headMesh, faceMat ?? skinMat, headRoot.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            // Hair System (Warm Ash Caramel)
            GameObject hairRoot = new GameObject("Hair_System");
            hairRoot.transform.SetParent(headRoot.transform, false);

            Mesh skullCap = BuildSkullCapMesh(0.116f, 0.128f, 0.122f, isFemale: true);
            CreateMeshObj("Hair_Skull_Cap", skullCap, hairMat, hairRoot.transform,
                new Vector3(0f, 0.015f, -0.01f), Quaternion.identity, Vector3.one);

            // Front Bangs
            Mesh fb1 = BuildCurvedLockMesh(0.035f, 0.020f, 0.14f, new Vector3(-0.045f, -0.05f, 0.03f), 10, 8);
            CreateMeshObj("Hair_Bang_L", fb1, hairMat, hairRoot.transform,
                new Vector3(-0.025f, 0.088f, 0.092f), Quaternion.Euler(70f, -22f, 10f), Vector3.one);

            Mesh fb2 = BuildCurvedLockMesh(0.034f, 0.020f, 0.13f, new Vector3(0.045f, -0.04f, 0.025f), 10, 8);
            CreateMeshObj("Hair_Bang_R", fb2, hairMat, hairRoot.transform,
                new Vector3(0.025f, 0.088f, 0.092f), Quaternion.Euler(68f, 22f, -10f), Vector3.one);

            // Long Twin Face-Framing Tresses
            Mesh fSideL = BuildCurvedLockMesh(0.030f, 0.018f, 0.26f, new Vector3(-0.02f, -0.11f, 0.03f), 10, 8);
            CreateMeshObj("Hair_Side_L", fSideL, hairMat, hairRoot.transform,
                new Vector3(-0.102f, 0.045f, 0.038f), Quaternion.Euler(85f, -10f, 10f), Vector3.one);

            Mesh fSideR = BuildCurvedLockMesh(0.030f, 0.018f, 0.26f, new Vector3(0.02f, -0.11f, 0.03f), 10, 8);
            CreateMeshObj("Hair_Side_R", fSideR, hairMat, hairRoot.transform,
                new Vector3(0.102f, 0.045f, 0.038f), Quaternion.Euler(85f, 10f, -10f), Vector3.one);

            // Grand High Arched Anime Ponytail (Flowing curve tied with cyber ring)
            Mesh ponytail = BuildCurvedLockMesh(0.060f, 0.048f, 0.42f, new Vector3(0.025f, -0.20f, -0.18f), 12, 10);
            CreateMeshObj("Hair_Ponytail", ponytail, hairMat, hairRoot.transform,
                new Vector3(0.01f, 0.13f, -0.10f), Quaternion.Euler(-32f, 10f, 0f), Vector3.one);

            Mesh hairRing = BuildStandingCollarMesh(0.062f, 0.050f, 0.026f, 0f, 0f);
            CreateMeshObj("Hair_Ring", hairRing, cyanGlowMat ?? coatAccentMat, hairRoot.transform,
                new Vector3(0.01f, 0.13f, -0.10f), Quaternion.Euler(-32f, 10f, 0f), Vector3.one);

            // Neck & Collar
            Mesh neck = BuildTaperedTube(0.042f, 0.046f, 0.11f, 16);
            CreateMeshObj("Neck", neck, skinMat, spine.transform,
                new Vector3(0f, 0.52f, 0f), Quaternion.identity, Vector3.one);

            Mesh collar = BuildStandingCollarMesh(0.085f, 0.092f, 0.11f, 18f, 60f);
            CreateMeshObj("Collar", collar, coatMat, spine.transform,
                new Vector3(0f, 0.43f, 0.01f), Quaternion.Euler(-6f, 0f, 0f), Vector3.one);

            Mesh collarTrim = BuildStandingCollarMesh(0.087f, 0.094f, 0.014f, 18f, 60f);
            CreateMeshObj("Collar_Trim", collarTrim, cyanGlowMat ?? coatAccentMat, spine.transform,
                new Vector3(0f, 0.525f, 0.01f), Quaternion.Euler(-6f, 0f, 0f), Vector3.one);

            // Slender Torso & Tailored Coat
            Mesh torso = BuildContouredTorsoMesh(0.16f, 0.13f, 0.43f, 16);
            CreateMeshObj("Coat_Torso", torso, coatMat, spine.transform,
                new Vector3(0f, 0.40f, 0f), Quaternion.identity, new Vector3(1.05f, 1f, 0.66f));

            Mesh waistNeonL = BuildTaperedTube(0.010f, 0.007f, 0.24f, 6);
            CreateMeshObj("Waist_Neon_L", waistNeonL, cyanGlowMat ?? coatAccentMat, spine.transform,
                new Vector3(-0.085f, 0.30f, 0.042f), Quaternion.identity, Vector3.one);
            CreateMeshObj("Waist_Neon_R", waistNeonL, cyanGlowMat ?? coatAccentMat, spine.transform,
                new Vector3(0.085f, 0.30f, 0.042f), Quaternion.identity, Vector3.one);

            // Skirt-like Flared Coat Tails
            GameObject tailL = new GameObject("CoatTail_L_Pivot");
            tailL.transform.SetParent(spine.transform, false);
            tailL.transform.localPosition = new Vector3(-0.05f, 0.03f, 0f);
            animator.coatTailLPivot = tailL.transform;

            Mesh tailLMesh = BuildDrapedCoatTailMesh(0.18f, 0.29f, 0.60f, 16f, 13f, isLeft: true);
            CreateMeshObj("CoatTail_L", tailLMesh, coatMat, tailL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            GameObject tailR = new GameObject("CoatTail_R_Pivot");
            tailR.transform.SetParent(spine.transform, false);
            tailR.transform.localPosition = new Vector3(0.05f, 0.03f, 0f);
            animator.coatTailRPivot = tailR.transform;

            Mesh tailRMesh = BuildDrapedCoatTailMesh(0.18f, 0.29f, 0.60f, 16f, 13f, isLeft: false);
            CreateMeshObj("CoatTail_R", tailRMesh, coatMat, tailR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            GameObject tailB = new GameObject("CoatTail_Back_Pivot");
            tailB.transform.SetParent(spine.transform, false);
            tailB.transform.localPosition = new Vector3(0f, 0.03f, -0.08f);
            animator.coatTailBackPivot = tailB.transform;

            Mesh tailBMesh = BuildDrapedCoatTailMesh(0.24f, 0.34f, 0.56f, 18f, 0f, isLeft: true);
            CreateMeshObj("CoatTail_Back", tailBMesh, coatMat, tailB.transform,
                Vector3.zero, Quaternion.Euler(0f, 90f, 0f), Vector3.one);

            // Arms & Gauntlets
            GameObject armL = new GameObject("Arm_L_Pivot");
            armL.transform.SetParent(spine.transform, false);
            armL.transform.localPosition = new Vector3(-0.19f, 0.38f, 0f);
            animator.leftArmPivot = armL.transform;

            Mesh upperArm = BuildTaperedTube(0.046f, 0.039f, 0.25f, 14);
            CreateMeshObj("UpperArm_L", upperArm, coatMat, armL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            GameObject forearmL = new GameObject("Forearm_L_Pivot");
            forearmL.transform.SetParent(armL.transform, false);
            forearmL.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            animator.leftForearmPivot = forearmL.transform;

            Mesh forearm = BuildTaperedTube(0.042f, 0.035f, 0.21f, 14);
            CreateMeshObj("Forearm_L", forearm, coatMat, forearmL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            Mesh cuff = BuildStandingCollarMesh(0.050f, 0.050f, 0.045f, 16f, 0f);
            CreateMeshObj("Cuff_L", cuff, coatAccentMat, forearmL.transform,
                new Vector3(0f, -0.15f, 0f), Quaternion.identity, Vector3.one);

            Mesh glove = BuildTaperedTube(0.038f, 0.026f, 0.12f, 12);
            CreateMeshObj("Glove_L", glove, gloveMat, forearmL.transform,
                new Vector3(0f, -0.19f, 0f), Quaternion.identity, Vector3.one);

            Mesh holoQuad = BuildHologramQuadMesh(0.12f, 0.085f);
            CreateMeshObj("Wrist_Holo_HUD", holoQuad, cyanGlowMat ?? coatAccentMat, forearmL.transform,
                new Vector3(-0.055f, -0.13f, 0.04f), Quaternion.Euler(20f, -42f, -15f), Vector3.one);

            // Right Arm with Tablet
            GameObject armR = new GameObject("Arm_R_Pivot");
            armR.transform.SetParent(spine.transform, false);
            armR.transform.localPosition = new Vector3(0.19f, 0.38f, 0f);
            animator.rightArmPivot = armR.transform;

            CreateMeshObj("UpperArm_R", upperArm, coatMat, armR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            GameObject forearmR = new GameObject("Forearm_R_Pivot");
            forearmR.transform.SetParent(armR.transform, false);
            forearmR.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            animator.rightForearmPivot = forearmR.transform;

            CreateMeshObj("Forearm_R", forearm, coatMat, forearmR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            CreateMeshObj("Cuff_R", cuff, coatAccentMat, forearmR.transform,
                new Vector3(0f, -0.15f, 0f), Quaternion.identity, Vector3.one);

            CreateMeshObj("Glove_R", glove, gloveMat, forearmR.transform,
                new Vector3(0f, -0.19f, 0f), Quaternion.identity, Vector3.one);

            Mesh tablet = BuildTabletMesh(0.14f, 0.012f, 0.20f);
            CreateMeshObj("Lab_Tablet", tablet, coatAccentMat, forearmR.transform,
                new Vector3(0.038f, -0.25f, 0.038f), Quaternion.Euler(30f, 20f, 0f), Vector3.one);

            // Legs & Boots
            GameObject legL = new GameObject("Leg_L_Pivot");
            legL.transform.SetParent(spine.transform, false);
            legL.transform.localPosition = new Vector3(-0.10f, 0.02f, 0f);
            animator.leftLegPivot = legL.transform;

            Mesh thigh = BuildTaperedTube(0.072f, 0.050f, 0.43f, 14);
            CreateMeshObj("Thigh_L", thigh, suitMat, legL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            Mesh legNeon = BuildTaperedTube(0.010f, 0.007f, 0.37f, 6);
            CreateMeshObj("Thigh_Neon_L", legNeon, cyanGlowMat ?? coatAccentMat, legL.transform,
                new Vector3(-0.058f, -0.02f, 0f), Quaternion.identity, Vector3.one);

            GameObject kneeL = new GameObject("Knee_L_Pivot");
            kneeL.transform.SetParent(legL.transform, false);
            kneeL.transform.localPosition = new Vector3(0f, -0.43f, 0f);
            animator.leftKneePivot = kneeL.transform;

            Mesh calf = BuildTaperedTube(0.050f, 0.039f, 0.41f, 14);
            CreateMeshObj("Calf_L", calf, suitMat, kneeL.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            Mesh boot = BuildCyberBootMesh(0.11f, 0.23f, 0.21f, 0.042f);
            CreateMeshObj("Cyber_Boot_L", boot, bootMat, kneeL.transform,
                new Vector3(0f, -0.41f, 0.03f), Quaternion.identity, Vector3.one);

            Mesh bootNeon = BuildCyberBootMesh(0.115f, 0.235f, 0.018f, 0.014f);
            CreateMeshObj("Boot_Neon_L", bootNeon, cyanGlowMat ?? coatAccentMat, kneeL.transform,
                new Vector3(0f, -0.41f, 0.03f), Quaternion.identity, Vector3.one);

            // Right Leg
            GameObject legR = new GameObject("Leg_R_Pivot");
            legR.transform.SetParent(spine.transform, false);
            legR.transform.localPosition = new Vector3(0.10f, 0.02f, 0f);
            animator.rightLegPivot = legR.transform;

            CreateMeshObj("Thigh_R", thigh, suitMat, legR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            CreateMeshObj("Thigh_Neon_R", legNeon, cyanGlowMat ?? coatAccentMat, legR.transform,
                new Vector3(0.058f, -0.02f, 0f), Quaternion.identity, Vector3.one);

            GameObject kneeR = new GameObject("Knee_R_Pivot");
            kneeR.transform.SetParent(legR.transform, false);
            kneeR.transform.localPosition = new Vector3(0f, -0.43f, 0f);
            animator.rightKneePivot = kneeR.transform;

            CreateMeshObj("Calf_R", calf, suitMat, kneeR.transform,
                Vector3.zero, Quaternion.identity, Vector3.one);

            CreateMeshObj("Cyber_Boot_R", boot, bootMat, kneeR.transform,
                new Vector3(0f, -0.41f, 0.03f), Quaternion.identity, Vector3.one);

            CreateMeshObj("Boot_Neon_R", bootNeon, cyanGlowMat ?? coatAccentMat, kneeR.transform,
                new Vector3(0f, -0.41f, 0.03f), Quaternion.identity, Vector3.one);

            return root;
        }

        // =========================================================================
        // MESH GENERATION FACTORY (Continuous smooth organic surfaces)
        // =========================================================================

        public static Mesh BuildSculptedHeadMesh(float rx, float ry, float rz, bool isFemale, int latSegments = 22, int lonSegments = 30)
        {
            Mesh mesh = new Mesh { name = "SculptedAnimeHead" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float v = (float)lat / latSegments;
                float phi = v * Mathf.PI; // 0 (top) to PI (chin)

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float u = (float)lon / lonSegments;
                    float theta = u * Mathf.PI * 2f; // 0 to 2PI

                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    float sinTheta = Mathf.Sin(theta);
                    float cosTheta = Mathf.Cos(theta);

                    float x = rx * sinPhi * cosTheta;
                    float y = ry * cosPhi;
                    float z = rz * sinPhi * sinTheta;

                    // Anatomical anime head sculpting:
                    // 1. Chin & Jawline taper
                    if (y < 0f)
                    {
                        float taper = -y / ry;
                        float taperRatio = isFemale ? 0.52f : 0.44f;
                        x *= (1f - taper * taperRatio);

                        // Chin forward projection
                        if (z > 0f)
                        {
                            z += taper * 0.024f;
                        }
                    }
                    else
                    {
                        // Forehead dome
                        float dome = y / ry;
                        x *= (1f - dome * 0.12f);
                    }

                    // 2. Eye & cheek planar facial region (Front hemisphere)
                    if (z > 0f && y > -ry * 0.45f && y < ry * 0.45f)
                    {
                        z = Mathf.Lerp(z, rz * 0.84f, 0.25f);
                    }

                    verts.Add(new Vector3(x, y, z));

                    // Exact Frontal Planar UV Alignment
                    if (z >= -0.015f)
                    {
                        float uvU = (x / (rx * 2.45f)) + 0.5f;
                        float uvV = 0.50f + (y / (ry * 1.55f));
                        uvs.Add(new Vector2(Mathf.Clamp01(uvU), Mathf.Clamp01(uvV)));
                    }
                    else
                    {
                        uvs.Add(new Vector2(0.04f, 0.04f));
                    }
                }
            }

            int stride = lonSegments + 1;
            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int c = lat * stride + lon;
                    int n = c + stride;

                    tris.Add(c); tris.Add(c + 1); tris.Add(n);
                    tris.Add(n); tris.Add(c + 1); tris.Add(n + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildSkullCapMesh(float rx, float ry, float rz, bool isFemale)
        {
            Mesh mesh = new Mesh { name = "HairSkullCap" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int latSegs = 14;
            int lonSegs = 24;

            for (int lat = 0; lat <= latSegs; lat++)
            {
                float v = (float)lat / latSegs;
                float phi = v * (Mathf.PI * 0.65f); // Covers top and back skull down to nape

                for (int lon = 0; lon <= lonSegs; lon++)
                {
                    float u = (float)lon / lonSegs;
                    float theta = u * Mathf.PI * 2f;

                    float x = rx * Mathf.Sin(phi) * Mathf.Cos(theta);
                    float y = ry * Mathf.Cos(phi);
                    float z = rz * Mathf.Sin(phi) * Mathf.Sin(theta);

                    // Pull backward slightly to cover occipital bone
                    if (z < 0f) z *= 1.08f;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(u, v));
                }
            }

            int stride = lonSegs + 1;
            for (int lat = 0; lat < latSegs; lat++)
            {
                for (int lon = 0; lon < lonSegs; lon++)
                {
                    int c = lat * stride + lon;
                    int n = c + stride;

                    tris.Add(c); tris.Add(c + 1); tris.Add(n);
                    tris.Add(n); tris.Add(c + 1); tris.Add(n + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildCurvedLockMesh(float baseRx, float baseRz, float length, Vector3 tipBend, int radSegs = 8, int hSegs = 6)
        {
            Mesh mesh = new Mesh { name = "CurvedHairLock" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int stride = radSegs + 1;

            for (int h = 0; h <= hSegs; h++)
            {
                float t = (float)h / hSegs;
                Vector3 center = (Vector3.forward * (t * length)) + (tipBend * (t * t));

                // Smooth aerodynamic taper to sharp point
                float scale = Mathf.Lerp(1.0f, 0.015f, Mathf.Pow(t, 1.15f));
                float curRx = baseRx * scale;
                float curRz = baseRz * scale;

                for (int r = 0; r <= radSegs; r++)
                {
                    float u = (float)r / radSegs;
                    float angle = u * Mathf.PI * 2f;

                    float vx = Mathf.Cos(angle) * curRx;
                    float vy = Mathf.Sin(angle) * curRz;

                    verts.Add(center + new Vector3(vx, vy, 0f));
                    uvs.Add(new Vector2(u, t));
                }
            }

            for (int h = 0; h < hSegs; h++)
            {
                for (int r = 0; r < radSegs; r++)
                {
                    int c = h * stride + r;
                    int n = (h + 1) * stride + r;

                    tris.Add(c); tris.Add(n); tris.Add(c + 1);
                    tris.Add(c + 1); tris.Add(n); tris.Add(n + 1);
                }
            }

            // Base cap
            int baseC = verts.Count;
            verts.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));
            for (int r = 0; r < radSegs; r++)
            {
                tris.Add(baseC); tris.Add(r + 1); tris.Add(r);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildStandingCollarMesh(float rx, float rz, float height, float flareAngle, float openingAngleDeg)
        {
            Mesh mesh = new Mesh { name = "StandingCollar" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int segs = 18;
            float startAngle = (-180f + openingAngleDeg * 0.5f) * Mathf.Deg2Rad;
            float endAngle = (180f - openingAngleDeg * 0.5f) * Mathf.Deg2Rad;
            float span = endAngle - startAngle;

            float flareOut = Mathf.Tan(flareAngle * Mathf.Deg2Rad) * height;
            float thickness = 0.014f;

            for (int s = 0; s <= segs; s++)
            {
                float t = (float)s / segs;
                float angle = startAngle + t * span;
                float sinA = Mathf.Sin(angle);
                float cosA = Mathf.Cos(angle);

                float bxO = cosA * (rx + thickness * 0.5f);
                float bzO = sinA * (rz + thickness * 0.5f);
                float bxI = cosA * (rx - thickness * 0.5f);
                float bzI = sinA * (rz - thickness * 0.5f);

                float txO = cosA * (rx + flareOut + thickness * 0.5f);
                float tzO = sinA * (rz + flareOut + thickness * 0.5f);
                float txI = cosA * (rx + flareOut - thickness * 0.5f);
                float tzI = sinA * (rz + flareOut - thickness * 0.5f);

                verts.Add(new Vector3(bxO, 0f, bzO));
                verts.Add(new Vector3(txO, height, tzO));
                verts.Add(new Vector3(txI, height, tzI));
                verts.Add(new Vector3(bxI, 0f, bzI));

                uvs.Add(new Vector2(t, 0f));
                uvs.Add(new Vector2(t, 0.5f));
                uvs.Add(new Vector2(t, 0.75f));
                uvs.Add(new Vector2(t, 1.0f));
            }

            for (int s = 0; s < segs; s++)
            {
                int c = s * 4;
                int n = (s + 1) * 4;

                // Outer
                tris.Add(c + 0); tris.Add(c + 1); tris.Add(n + 0);
                tris.Add(n + 0); tris.Add(c + 1); tris.Add(n + 1);

                // Top rim
                tris.Add(c + 1); tris.Add(c + 2); tris.Add(n + 1);
                tris.Add(n + 1); tris.Add(c + 2); tris.Add(n + 2);

                // Inner
                tris.Add(c + 2); tris.Add(c + 3); tris.Add(n + 2);
                tris.Add(n + 2); tris.Add(c + 3); tris.Add(n + 3);

                // Bottom rim
                tris.Add(c + 3); tris.Add(c + 0); tris.Add(n + 3);
                tris.Add(n + 3); tris.Add(c + 0); tris.Add(n + 0);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildDrapedCoatTailMesh(float topW, float bottomW, float length, float flareOut, float flareSide, bool isLeft)
        {
            Mesh mesh = new Mesh { name = "DrapedCoatTail" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int slices = 6;
            float sideSign = isLeft ? -1f : 1f;
            float outRad = flareOut * Mathf.Deg2Rad;
            float sideRad = flareSide * Mathf.Deg2Rad;

            for (int row = 0; row <= slices; row++)
            {
                float t = (float)row / slices;
                float y = -t * length;
                float z = -Mathf.Sin(outRad * t) * length * 0.45f;
                float xOffset = sideSign * Mathf.Sin(sideRad * t) * length * 0.35f;

                float curW = Mathf.Lerp(topW, bottomW, t);

                // Inner seam
                verts.Add(new Vector3(xOffset, y, z));
                uvs.Add(new Vector2(0f, t));

                // Outer edge (diagonal cut)
                float hemCut = (row == slices) ? (isLeft ? 0.05f : -0.05f) : 0f;
                verts.Add(new Vector3(xOffset + sideSign * curW, y + hemCut, z));
                uvs.Add(new Vector2(1f, t));
            }

            for (int row = 0; row < slices; row++)
            {
                int c = row * 2;
                int n = (row + 1) * 2;

                tris.Add(c + 0); tris.Add(c + 1); tris.Add(n + 0);
                tris.Add(n + 0); tris.Add(c + 1); tris.Add(n + 1);

                // Double sided
                tris.Add(c + 0); tris.Add(n + 0); tris.Add(c + 1);
                tris.Add(n + 0); tris.Add(n + 1); tris.Add(c + 1);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildContouredTorsoMesh(float rx, float rz, float length, int segments = 16)
        {
            Mesh mesh = new Mesh { name = "ContouredTorso" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int hSlices = 6;
            int stride = segments + 1;

            for (int h = 0; h <= hSlices; h++)
            {
                float t = (float)h / hSlices;
                float y = -t * length;

                // Waist cinch: narrow in middle, flare at pelvis
                float waistTaper = 1.0f - 0.18f * Mathf.Sin(t * Mathf.PI);
                float curRx = rx * waistTaper;
                float curRz = rz * waistTaper;

                for (int s = 0; s <= segments; s++)
                {
                    float u = (float)s / segments;
                    float angle = u * Mathf.PI * 2f;

                    float x = Mathf.Cos(angle) * curRx;
                    float z = Mathf.Sin(angle) * curRz;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(u, t));
                }
            }

            for (int h = 0; h < hSlices; h++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int c = h * stride + s;
                    int n = (h + 1) * stride + s;

                    tris.Add(c); tris.Add(c + 1); tris.Add(n);
                    tris.Add(n); tris.Add(c + 1); tris.Add(n + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildTaperedTube(float topR, float bottomR, float length, int segments = 12)
        {
            Mesh mesh = new Mesh { name = "TaperedTube" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int hSlices = 4;
            int stride = segments + 1;

            for (int h = 0; h <= hSlices; h++)
            {
                float t = (float)h / hSlices;
                float y = -t * length;
                float r = Mathf.Lerp(topR, bottomR, t);

                for (int s = 0; s <= segments; s++)
                {
                    float u = (float)s / segments;
                    float angle = u * Mathf.PI * 2f;

                    verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                    uvs.Add(new Vector2(u, t));
                }
            }

            for (int h = 0; h < hSlices; h++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int c = h * stride + s;
                    int n = (h + 1) * stride + s;

                    tris.Add(c); tris.Add(c + 1); tris.Add(n);
                    tris.Add(n); tris.Add(c + 1); tris.Add(n + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildCurvedVisorMesh(float width, float height, float curveDepth)
        {
            Mesh mesh = new Mesh { name = "CurvedVisor" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int segs = 14;
            float hw = width * 0.5f;

            for (int s = 0; s <= segs; s++)
            {
                float t = (float)s / segs;
                float x = Mathf.Lerp(-hw, hw, t);
                float normX = (t - 0.5f) * 2f;
                float z = (1f - normX * normX) * curveDepth;

                verts.Add(new Vector3(x, -height * 0.5f, z));
                verts.Add(new Vector3(x,  height * 0.5f, z));

                uvs.Add(new Vector2(t, 0f));
                uvs.Add(new Vector2(t, 1f));
            }

            for (int s = 0; s < segs; s++)
            {
                int c = s * 2;
                int n = (s + 1) * 2;

                tris.Add(c + 0); tris.Add(c + 1); tris.Add(n + 0);
                tris.Add(n + 0); tris.Add(c + 1); tris.Add(n + 1);

                // Double sided
                tris.Add(c + 0); tris.Add(n + 0); tris.Add(c + 1);
                tris.Add(n + 0); tris.Add(n + 1); tris.Add(c + 1);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildCyberBootMesh(float width, float length, float height, float soleHeight)
        {
            Mesh mesh = new Mesh { name = "CyberBoot" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            float hw = width * 0.5f;
            float hl = length * 0.5f;

            Vector2[] footprint = new Vector2[]
            {
                new Vector2(-hw * 0.75f, -hl),
                new Vector2( hw * 0.75f, -hl),
                new Vector2( hw * 0.85f, -hl * 0.35f),
                new Vector2( hw,          hl * 0.25f),
                new Vector2( hw * 0.65f,  hl),
                new Vector2(-hw * 0.65f,  hl),
                new Vector2(-hw,          hl * 0.25f),
                new Vector2(-hw * 0.85f, -hl * 0.35f)
            };

            int count = footprint.Length;

            // Layer 0: Sole bottom
            for (int i = 0; i < count; i++)
            {
                verts.Add(new Vector3(footprint[i].x * 1.05f, 0f, footprint[i].y * 1.05f));
                uvs.Add(new Vector2((float)i / count, 0f));
            }
            // Layer 1: Sole top
            for (int i = 0; i < count; i++)
            {
                verts.Add(new Vector3(footprint[i].x, soleHeight, footprint[i].y));
                uvs.Add(new Vector2((float)i / count, 0.3f));
            }
            // Layer 2: Ankle cuff top
            for (int i = 0; i < count; i++)
            {
                float taper = 0.70f;
                verts.Add(new Vector3(footprint[i].x * taper, height, footprint[i].y * taper * 0.7f));
                uvs.Add(new Vector2((float)i / count, 1.0f));
            }

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                tris.Add(i); tris.Add(count + i); tris.Add(next);
                tris.Add(next); tris.Add(count + i); tris.Add(count + next);

                int c1 = count + i;
                int n1 = count + next;
                int c2 = count * 2 + i;
                int n2 = count * 2 + next;
                tris.Add(c1); tris.Add(c2); tris.Add(n1);
                tris.Add(n1); tris.Add(c2); tris.Add(n2);
            }

            int bottomC = verts.Count;
            verts.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i < count; i++)
            {
                tris.Add(bottomC); tris.Add((i + 1) % count); tris.Add(i);
            }

            int topC = verts.Count;
            verts.Add(new Vector3(0f, height, 0f));
            uvs.Add(new Vector2(0.5f, 0.5f));
            int topStart = count * 2;
            for (int i = 0; i < count; i++)
            {
                tris.Add(topC); tris.Add(topStart + i); tris.Add(topStart + (i + 1) % count);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildSculptedPouchMesh(float w, float d, float h)
        {
            Mesh mesh = new Mesh { name = "Pouch" };
            float hw = w * 0.5f;
            float hd = d * 0.5f;

            Vector3[] v = new Vector3[]
            {
                new Vector3(-hw, 0, -hd), new Vector3(hw, 0, -hd), new Vector3(hw, 0, hd), new Vector3(-hw, 0, hd),
                new Vector3(-hw, h, -hd), new Vector3(hw, h, -hd), new Vector3(hw, h, hd), new Vector3(-hw, h, hd)
            };

            int[] t = new int[]
            {
                0, 4, 1, 1, 4, 5, // Front
                1, 5, 2, 2, 5, 6, // Right
                2, 6, 3, 3, 6, 7, // Back
                3, 7, 0, 0, 7, 4, // Left
                4, 7, 5, 5, 7, 6  // Top
            };

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Mesh BuildTabletMesh(float w, float d, float h)
        {
            return BuildSculptedPouchMesh(w, d, h);
        }

        public static Mesh BuildHologramQuadMesh(float w, float h)
        {
            Mesh mesh = new Mesh { name = "HoloQuad" };
            float hw = w * 0.5f;
            float hh = h * 0.5f;

            Vector3[] v = new Vector3[]
            {
                new Vector3(-hw, -hh, 0), new Vector3(hw, -hh, 0), new Vector3(hw, hh, 0), new Vector3(-hw, hh, 0),
                new Vector3(-hw, -hh, 0), new Vector3(hw, -hh, 0), new Vector3(hw, hh, 0), new Vector3(-hw, hh, 0)
            };
            Vector2[] u = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)
            };
            int[] t = new int[] { 0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7 };

            mesh.vertices = v;
            mesh.uv = u;
            mesh.triangles = t;
            mesh.RecalculateNormals();
            return mesh;
        }

        private static GameObject CreateMeshObj(string name, Mesh mesh, Material mat, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = localRot;
            obj.transform.localScale = localScale;

            var mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = obj.AddComponent<MeshRenderer>();
            if (mat != null) mr.sharedMaterial = mat;

            return obj;
        }
    }
}
