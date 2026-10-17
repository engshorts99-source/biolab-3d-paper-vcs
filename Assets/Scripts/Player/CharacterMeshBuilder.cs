using UnityEngine;

namespace BioLab.Player
{
    /// <summary>
    /// Master anime researcher humanoid builder.
    /// Loads authentic 3D anime models with high-res textures, Genshin toon shading,
    /// and fitted futuristic cleanroom gear (high standing open lab coat collar,
    /// flowing asymmetrical tails, left wrist holographic HUD, and cyber conduits).
    /// </summary>
    public static class CharacterMeshBuilder
    {
        public static GameObject BuildMaleResearcher(
            Material suitMat, Material coatMat, Material coatAccentMat, 
            Material skinMat, Material faceMat, Material hairMat, Material glassMat, 
            Material bootMat, Material gloveMat, Material cyanGlowMat)
        {
            Shader toonShader = suitMat != null ? suitMat.shader : Shader.Find("Custom/GenshinToonNPR");
            Shader faceShader = faceMat != null ? faceMat.shader : Shader.Find("Custom/GenshinFace");

            GameObject riggedModel = RiggedModelLoader.LoadResearcher(true, toonShader, faceShader);
            if (riggedModel != null)
            {
                return riggedModel;
            }

            GameObject model = ObjLoader.LoadResearcher(true, toonShader, faceShader, coatMat, coatAccentMat, cyanGlowMat);
            if (model != null)
            {
                return model;
            }

            // Fallback to sculpted mesh if OBJ is missing
            return AnimeModelSculptor.SculptMaleResearcher(
                suitMat, coatMat, coatAccentMat, skinMat, faceMat, hairMat, glassMat, bootMat, gloveMat, cyanGlowMat);
        }

        public static GameObject BuildFemaleResearcher(
            Material suitMat, Material coatMat, Material coatAccentMat, 
            Material skinMat, Material faceMat, Material hairMat, Material glassMat, 
            Material bootMat, Material gloveMat, Material cyanGlowMat)
        {
            Shader toonShader = suitMat != null ? suitMat.shader : Shader.Find("Custom/GenshinToonNPR");
            Shader faceShader = faceMat != null ? faceMat.shader : Shader.Find("Custom/GenshinFace");

            GameObject riggedModel = RiggedModelLoader.LoadResearcher(false, toonShader, faceShader);
            if (riggedModel != null)
            {
                return riggedModel;
            }

            GameObject model = ObjLoader.LoadResearcher(false, toonShader, faceShader, coatMat, coatAccentMat, cyanGlowMat);
            if (model != null)
            {
                return model;
            }

            // Fallback to sculpted mesh if OBJ is missing
            return AnimeModelSculptor.SculptFemaleResearcher(
                suitMat, coatMat, coatAccentMat, skinMat, faceMat, hairMat, glassMat, bootMat, gloveMat, cyanGlowMat);
        }
    }
}
