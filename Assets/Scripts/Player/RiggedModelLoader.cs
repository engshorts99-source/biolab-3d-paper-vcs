using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BioLab.Core;

namespace BioLab.Player
{
    /// <summary>
    /// Runtime rigged humanoid loader for authentic 3D anime models.
    /// Loads binary skeletal meshes (.bin), builds bone hierarchies, assigns SkinnedMeshRenderers,
    /// sets up bindposes, binds GenshinFace and GenshinToonNPR shaders, and hooks up ProceduralHumanoidAnimator.
    /// </summary>
    public static class RiggedModelLoader
    {
        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        public static GameObject LoadResearcher(bool isMale, Shader toonShader, Shader faceShader)
        {
            string folderName = isMale ? "Masc" : "Fem";
            string fileName = isMale ? "male_rigged.bin" : "female_rigged.bin";

            string[] candidatePaths = new[]
            {
                Path.Combine(Application.streamingAssetsPath, "Models", folderName, fileName),
                Path.Combine(Application.dataPath, "StreamingAssets", "Models", folderName, fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "StreamingAssets", "Models", folderName, fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Models", folderName, fileName)
            };

            string binPath = null;
            foreach (var p in candidatePaths)
            {
                if (File.Exists(p))
                {
                    binPath = p;
                    break;
                }
            }

            if (binPath == null)
            {
                Debug.LogWarning($"[RiggedModelLoader] Binary file not found for {(isMale ? "male" : "female")}: '{fileName}'");
                return null;
            }

            string texDir = Path.Combine(Path.GetDirectoryName(binPath), "textures");

            Debug.Log($"[RiggedModelLoader] Loading rigged 3D anime character from '{binPath}'...");

            try
            {
                using (FileStream fs = new FileStream(binPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader r = new BinaryReader(fs))
                {
                    uint magic = r.ReadUInt32(); // 0x44474952 "RIGD"
                    uint ver = r.ReadUInt32();
                    if (ver != 2)
                    {
                        Debug.LogError($"[RiggedModelLoader] Unsupported binary version {ver}");
                        return null;
                    }

                    GameObject root = new GameObject(isMale ? "Male_Researcher_3D" : "Female_Researcher_3D");

                    // 1. Bones Hierarchy
                    uint boneCount = r.ReadUInt32();
                    Transform[] bones = new Transform[boneCount];
                    string[] boneNames = new string[boneCount];

                    Transform hipsTransform = null;
                    Transform leftLegPivot = null;
                    Transform leftKneePivot = null;
                    Transform rightLegPivot = null;
                    Transform rightKneePivot = null;
                    Transform leftArmPivot = null;
                    Transform leftForearmPivot = null;
                    Transform rightArmPivot = null;
                    Transform rightForearmPivot = null;

                    for (int i = 0; i < boneCount; i++)
                    {
                        byte nl = r.ReadByte();
                        string bName = System.Text.Encoding.UTF8.GetString(r.ReadBytes(nl));
                        boneNames[i] = bName;

                        int parentBone = r.ReadInt32();
                        float px = r.ReadSingle();
                        float py = r.ReadSingle();
                        float pz = r.ReadSingle();

                        float rx = r.ReadSingle();
                        float ry = r.ReadSingle();
                        float rz = r.ReadSingle();
                        float rw = r.ReadSingle();

                        GameObject boneGo = new GameObject(bName);
                        Transform bt = boneGo.transform;

                        if (parentBone >= 0 && parentBone < i)
                        {
                            bt.SetParent(bones[parentBone], false);
                        }
                        else
                        {
                            bt.SetParent(root.transform, false);
                        }

                        bt.localPosition = new Vector3(px, py, pz);
                        bt.localRotation = new Quaternion(rx, ry, rz, rw);
                        bt.localScale = Vector3.one;

                        bones[i] = bt;

                        // Wire up procedural pivots
                        if (bName == "J_Bip_C_Hips") hipsTransform = bt;
                        else if (bName == "J_Bip_L_UpperLeg") leftLegPivot = bt;
                        else if (bName == "J_Bip_L_LowerLeg") leftKneePivot = bt;
                        else if (bName == "J_Bip_R_UpperLeg") rightLegPivot = bt;
                        else if (bName == "J_Bip_R_LowerLeg") rightKneePivot = bt;
                        else if (bName == "J_Bip_L_UpperArm") leftArmPivot = bt;
                        else if (bName == "J_Bip_L_LowerArm") leftForearmPivot = bt;
                        else if (bName == "J_Bip_R_UpperArm") rightArmPivot = bt;
                        else if (bName == "J_Bip_R_LowerArm") rightForearmPivot = bt;
                    }

                    // 2. Compute bindposes in rest pose
                    Matrix4x4[] bindposes = new Matrix4x4[boneCount];
                    for (int i = 0; i < boneCount; i++)
                    {
                        bindposes[i] = bones[i].worldToLocalMatrix * root.transform.localToWorldMatrix;
                    }

                    // 3. Attach ProceduralHumanoidAnimator
                    ProceduralHumanoidAnimator anim = root.AddComponent<ProceduralHumanoidAnimator>();
                    anim.spinePivot = hipsTransform;
                    anim.leftLegPivot = leftLegPivot;
                    anim.leftKneePivot = leftKneePivot;
                    anim.rightLegPivot = rightLegPivot;
                    anim.rightKneePivot = rightKneePivot;
                    anim.leftArmPivot = leftArmPivot;
                    anim.leftForearmPivot = leftForearmPivot;
                    anim.rightArmPivot = rightArmPivot;
                    anim.rightForearmPivot = rightForearmPivot;

                    // Set natural resting arm angles immediately (brings T-pose arms down to sides)
                    if (leftArmPivot != null) leftArmPivot.localRotation = Quaternion.Euler(0f, 0f, 75f);
                    if (rightArmPivot != null) rightArmPivot.localRotation = Quaternion.Euler(0f, 0f, -75f);
                    if (leftForearmPivot != null) leftForearmPivot.localRotation = Quaternion.Euler(0f, 22f, 0f);
                    if (rightForearmPivot != null) rightForearmPivot.localRotation = Quaternion.Euler(0f, -22f, 0f);

                    // 4. Meshes
                    uint meshCount = r.ReadUInt32();
                    for (int m = 0; m < meshCount; m++)
                    {
                        byte mnl = r.ReadByte();
                        string meshName = System.Text.Encoding.UTF8.GetString(r.ReadBytes(mnl));

                        uint vertCount = r.ReadUInt32();
                        Vector3[] positions = new Vector3[vertCount];
                        Vector3[] normals = new Vector3[vertCount];
                        Vector2[] uvs = new Vector2[vertCount];
                        BoneWeight[] weights = new BoneWeight[vertCount];

                        for (int v = 0; v < vertCount; v++)
                        {
                            positions[v] = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                            normals[v] = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                            uvs[v] = new Vector2(r.ReadSingle(), r.ReadSingle());

                            ushort j0 = r.ReadUInt16();
                            ushort j1 = r.ReadUInt16();
                            ushort j2 = r.ReadUInt16();
                            ushort j3 = r.ReadUInt16();

                            float w0 = r.ReadSingle();
                            float w1 = r.ReadSingle();
                            float w2 = r.ReadSingle();
                            float w3 = r.ReadSingle();

                            BoneWeight bw = new BoneWeight
                            {
                                boneIndex0 = j0,
                                weight0 = w0,
                                boneIndex1 = j1,
                                weight1 = w1,
                                boneIndex2 = j2,
                                weight2 = w2,
                                boneIndex3 = j3,
                                weight3 = w3
                            };
                            weights[v] = bw;
                        }

                        // Submeshes
                        uint subCount = r.ReadUInt32();
                        List<int[]> subTriangles = new List<int[]>();
                        Material[] subMats = new Material[subCount];

                        for (int s = 0; s < subCount; s++)
                        {
                            byte matNl = r.ReadByte();
                            string matName = System.Text.Encoding.UTF8.GetString(r.ReadBytes(matNl));

                            byte texNl = r.ReadByte();
                            string texName = System.Text.Encoding.UTF8.GetString(r.ReadBytes(texNl));

                            uint idxCount = r.ReadUInt32();
                            int[] tris = new int[idxCount];
                            for (int k = 0; k < idxCount; k++)
                            {
                                tris[k] = (int)r.ReadUInt32();
                            }
                            subTriangles.Add(tris);

                            // Load texture
                            Texture2D tex = null;
                            if (!string.IsNullOrEmpty(texName) && Directory.Exists(texDir))
                            {
                                string texPath = Path.Combine(texDir, texName);
                                if (File.Exists(texPath))
                                {
                                    if (!TextureCache.TryGetValue(texPath, out tex))
                                    {
                                        byte[] bytes = File.ReadAllBytes(texPath);
                                        tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                                        tex.LoadImage(bytes);
                                        tex.filterMode = FilterMode.Bilinear;
                                        tex.wrapMode = TextureWrapMode.Clamp;
                                        TextureCache[texPath] = tex;
                                    }
                                }
                            }

                            // Material properties
                            bool isHair = matName.IndexOf("Hair", StringComparison.OrdinalIgnoreCase) >= 0;
                            bool isCloth = matName.IndexOf("Tops", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                           matName.IndexOf("Bottoms", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                           matName.IndexOf("Shoes", StringComparison.OrdinalIgnoreCase) >= 0;
                            bool isSkin = matName.IndexOf("Skin", StringComparison.OrdinalIgnoreCase) >= 0;
                            bool isFace = matName.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                          matName.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0;

                            if (tex != null)
                            {
                                if (isHair) tex.wrapMode = TextureWrapMode.Repeat;

                                Color32[] pixels = tex.GetPixels32();
                                if (isSkin || isHair)
                                {
                                    for (int p = 0; p < pixels.Length; p++) pixels[p].a = 255;
                                }
                                else if (isFace)
                                {
                                    for (int p = 0; p < pixels.Length; p++)
                                    {
                                        Color32 c = pixels[p];
                                        if (c.r > 10 || c.g > 10 || c.b > 10 || c.a > 40)
                                            pixels[p].a = 255;
                                        else
                                            pixels[p].a = 0;
                                    }
                                }
                                tex.SetPixels32(pixels);
                                tex.Apply(true);
                            }

                            Material mat;
                            if (isFace && faceShader != null)
                            {
                                mat = new Material(faceShader) { name = matName + "_Mat" };
                                if (tex != null)
                                {
                                    mat.mainTexture = tex;
                                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                                }
                                mat.color = Color.white;
                                if (mat.HasProperty("_ShadowTint"))
                                    mat.SetColor("_ShadowTint", new Color(0.92f, 0.85f, 0.87f));

                                bool isIris = matName.IndexOf("Iris", StringComparison.OrdinalIgnoreCase) >= 0;
                                bool isHighlight = matName.IndexOf("Highlight", StringComparison.OrdinalIgnoreCase) >= 0;
                                bool isEyeWhite = matName.IndexOf("EyeWhite", StringComparison.OrdinalIgnoreCase) >= 0;

                                if (isSkin)
                                {
                                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.0f);
                                    if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.0f);
                                    mat.renderQueue = 2450;
                                }
                                else
                                {
                                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
                                    if (isIris)
                                    {
                                        if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.45f);
                                        mat.renderQueue = 2452;
                                    }
                                    else if (isHighlight)
                                    {
                                        if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.85f);
                                        mat.renderQueue = 2453;
                                    }
                                    else if (isEyeWhite)
                                    {
                                        if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.15f);
                                        mat.renderQueue = 2451;
                                    }
                                    else
                                    {
                                        if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.0f);
                                        mat.renderQueue = 2454;
                                    }
                                }
                            }
                            else if (toonShader != null)
                            {
                                mat = new Material(toonShader) { name = matName + "_Mat" };
                                if (tex != null)
                                {
                                    mat.mainTexture = tex;
                                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                                }
                                mat.color = Color.white;

                                if (isHair)
                                {
                                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.05f);
                                    if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.28f, 0.30f, 0.42f));
                                    if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.95f, 0.98f, 1.0f));
                                    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 48.0f);
                                    if (mat.HasProperty("_RimColor")) mat.SetColor("_RimColor", new Color(0.85f, 0.92f, 1.0f));
                                    if (mat.HasProperty("_RimPower")) mat.SetFloat("_RimPower", 3.2f);
                                    if (mat.HasProperty("_RampSmooth")) mat.SetFloat("_RampSmooth", 0.20f);
                                }
                                else if (isCloth)
                                {
                                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
                                    if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.55f, 0.58f, 0.70f));
                                    if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.85f, 0.90f, 1.0f));
                                    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 24.0f);
                                    if (mat.HasProperty("_RampSmooth")) mat.SetFloat("_RampSmooth", 0.18f);
                                }
                                else if (isSkin)
                                {
                                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.0f);
                                    if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.90f, 0.82f, 0.85f));
                                    if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.70f, 0.75f, 0.85f));
                                    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 16.0f);
                                    if (mat.HasProperty("_RampSmooth")) mat.SetFloat("_RampSmooth", 0.22f);
                                }
                                else
                                {
                                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.50f);
                                    if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.50f, 0.53f, 0.65f));
                                    if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.70f, 0.75f, 0.85f));
                                    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 20.0f);
                                    if (mat.HasProperty("_RampSmooth")) mat.SetFloat("_RampSmooth", 0.18f);
                                }
                            }
                            else
                            {
                                mat = new Material(Shader.Find("Standard"));
                                if (tex != null) mat.mainTexture = tex;
                            }

                            subMats[s] = mat;
                        }

                        // Create Unity Mesh
                        Mesh unityMesh = new Mesh
                        {
                            name = meshName,
                            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
                        };

                        unityMesh.vertices = positions;
                        unityMesh.normals = normals;
                        unityMesh.uv = uvs;
                        unityMesh.boneWeights = weights;
                        unityMesh.bindposes = bindposes;
                        unityMesh.subMeshCount = (int)subCount;

                        for (int s = 0; s < subCount; s++)
                        {
                            unityMesh.SetTriangles(subTriangles[s], s);
                        }

                        unityMesh.RecalculateBounds();
                        unityMesh.RecalculateTangents();

                        // Create child GameObject with SkinnedMeshRenderer
                        GameObject childGo = new GameObject(meshName);
                        childGo.transform.SetParent(root.transform, false);

                        SkinnedMeshRenderer smr = childGo.AddComponent<SkinnedMeshRenderer>();
                        smr.sharedMesh = unityMesh;
                        smr.bones = bones;
                        smr.rootBone = hipsTransform != null ? hipsTransform : bones[0];
                        smr.sharedMaterials = subMats;
                        smr.updateWhenOffscreen = true;
                    }

                    Debug.Log($"[RiggedModelLoader] Successfully loaded rigged 3D anime {(isMale ? "male" : "female")} researcher with {boneCount} bones!");
                    return root;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RiggedModelLoader] Failed to load binary model from '{binPath}': {ex}");
                return null;
            }
        }
    }
}
