using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using BioLab.Core;

namespace BioLab.Player
{
    /// <summary>
    /// High-performance runtime OBJ and texture loader tailored for authentic 3D anime models.
    /// Loads meshes, assigns multi-material submeshes, binds GenshinFace and GenshinToonNPR shaders,
    /// and attaches futuristic cleanroom gear (high standing collar, flowing lab coat, wrist holo-HUD).
    /// </summary>
    public static class ObjLoader
    {
        public static GameObject LoadResearcher(bool isMale, Shader toonShader, Shader faceShader,
            Material labCoatMat, Material coatAccentMat, Material cyanGlowMat)
        {
            string folderName = isMale ? "Masc" : "Fem";
            string fileName = isMale ? "male_researcher" : "female_researcher";

            string[] candidatePaths = new[]
            {
                Path.Combine(Application.streamingAssetsPath, "Models", folderName, fileName + ".obj"),
                Path.Combine(Application.dataPath, "StreamingAssets", "Models", folderName, fileName + ".obj"),
                Path.Combine(Application.dataPath, "Models", folderName, fileName + ".obj"),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "StreamingAssets", "Models", folderName, fileName + ".obj"),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Models", folderName, fileName + ".obj"),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Resources", "Models", folderName, fileName + ".obj")
            };

            string objPath = null;
            string texDir = null;

            foreach (var p in candidatePaths)
            {
                if (File.Exists(p))
                {
                    objPath = p;
                    texDir = Path.Combine(Path.GetDirectoryName(p), "textures");
                    break;
                }
            }

            if (objPath == null)
            {
                Debug.LogWarning($"[ObjLoader] OBJ file not found for {(isMale ? "male" : "female")}, falling back to procedural sculptor.");
                return null;
            }

            Debug.Log($"[ObjLoader] Loading authentic 3D anime researcher from '{objPath}'...");

            GameObject root = new GameObject(isMale ? "Male_Researcher_3D" : "Female_Researcher_3D");

            // Cache of loaded textures
            Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

            // Read MTL file to get material -> texture mapping
            string mtlPath = Path.ChangeExtension(objPath, ".mtl");
            Dictionary<string, string> matToTexMap = ParseMtl(mtlPath);

            // Parse OBJ file
            List<Vector3> allPositions = new List<Vector3>();
            List<Vector2> allUvs = new List<Vector2>();
            List<Vector3> allNormals = new List<Vector3>();

            // Object-level grouping: objectName -> (materialName -> list of triangle indices)
            Dictionary<string, Dictionary<string, List<int>>> objects = new Dictionary<string, Dictionary<string, List<int>>>();

            string currentObj = "Body";
            string currentMtl = "Default";

            if (!objects.ContainsKey(currentObj))
                objects[currentObj] = new Dictionary<string, List<int>>();

            using (StreamReader reader = new StreamReader(objPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;

                    if (line.StartsWith("v "))
                    {
                        string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        float x = float.Parse(p[1], CultureInfo.InvariantCulture);
                        float y = float.Parse(p[2], CultureInfo.InvariantCulture);
                        float z = float.Parse(p[3], CultureInfo.InvariantCulture);
                        allPositions.Add(new Vector3(x, y, z));
                    }
                    else if (line.StartsWith("vt "))
                    {
                        string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        float u = float.Parse(p[1], CultureInfo.InvariantCulture);
                        float v = float.Parse(p[2], CultureInfo.InvariantCulture);
                        allUvs.Add(new Vector2(u, v));
                    }
                    else if (line.StartsWith("vn "))
                    {
                        string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        float x = float.Parse(p[1], CultureInfo.InvariantCulture);
                        float y = float.Parse(p[2], CultureInfo.InvariantCulture);
                        float z = float.Parse(p[3], CultureInfo.InvariantCulture);
                        allNormals.Add(new Vector3(x, y, z));
                    }
                    else if (line.StartsWith("o "))
                    {
                        currentObj = line.Substring(2).Trim();
                        if (!objects.ContainsKey(currentObj))
                            objects[currentObj] = new Dictionary<string, List<int>>();
                    }
                    else if (line.StartsWith("usemtl "))
                    {
                        currentMtl = line.Substring(7).Trim();
                        if (!objects[currentObj].ContainsKey(currentMtl))
                            objects[currentObj][currentMtl] = new List<int>();
                    }
                    else if (line.StartsWith("f "))
                    {
                        if (!objects[currentObj].ContainsKey(currentMtl))
                            objects[currentObj][currentMtl] = new List<int>();

                        List<int> currentIndices = objects[currentObj][currentMtl];
                        string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int i0 = ParseFaceIndex(tokens[1]);
                        int i1 = ParseFaceIndex(tokens[2]);
                        int i2 = ParseFaceIndex(tokens[3]);

                        currentIndices.Add(i0);
                        currentIndices.Add(i1);
                        currentIndices.Add(i2);
                    }
                }
            }

            Vector3[] posArray = allPositions.ToArray();
            Vector2[] uvArray = allUvs.ToArray();
            Vector3[] normArray = allNormals.ToArray();

            // Build child GameObjects for each OBJ group (Face.baked, Body.baked, Hair001.baked)
            foreach (var kvp in objects)
            {
                string objName = kvp.Key;
                var matGroups = kvp.Value;

                if (matGroups.Count == 0) continue;

                GameObject childObj = new GameObject(objName);
                childObj.transform.SetParent(root.transform, false);

                MeshFilter mf = childObj.AddComponent<MeshFilter>();
                MeshRenderer mr = childObj.AddComponent<MeshRenderer>();

                Mesh mesh = new Mesh();
                mesh.name = objName + "_Mesh";
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = posArray;
                if (uvArray.Length == posArray.Length) mesh.uv = uvArray;
                if (normArray.Length == posArray.Length) mesh.normals = normArray;

                mesh.subMeshCount = matGroups.Count;
                Material[] materials = new Material[matGroups.Count];

                var matList = new List<KeyValuePair<string, List<int>>>(matGroups);
                if (objName.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matList.Sort((a, b) => GetFaceMaterialPriority(a.Key).CompareTo(GetFaceMaterialPriority(b.Key)));
                }

                int subIndex = 0;
                foreach (var matKvp in matList)
                {
                    string matName = matKvp.Key;
                    List<int> tris = matKvp.Value;

                    mesh.SetTriangles(tris, subIndex);

                    // Load matching texture
                    Texture2D tex = null;
                    if (matToTexMap.TryGetValue(matName, out string texRelPath))
                    {
                        string texFullPath = Path.Combine(texDir, Path.GetFileName(texRelPath));
                        if (!textureCache.TryGetValue(texFullPath, out tex))
                        {
                            if (File.Exists(texFullPath))
                            {
                                byte[] bytes = File.ReadAllBytes(texFullPath);
                                tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                                tex.LoadImage(bytes);
                                tex.filterMode = FilterMode.Bilinear;
                                tex.wrapMode = TextureWrapMode.Clamp;
                                textureCache[texFullPath] = tex;
                            }
                        }
                    }

                    // Select proper shader
                    bool isHair = matName.IndexOf("Hair", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isTops = matName.IndexOf("Tops", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isSkin = matName.IndexOf("Skin", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isFace = matName.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  matName.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (tex != null)
                    {
                        if (isHair) tex.wrapMode = TextureWrapMode.Repeat;

                        Color32[] pixels = tex.GetPixels32();
                        if (isSkin || isHair)
                        {
                            // Skin and hair are 100% opaque
                            for (int p = 0; p < pixels.Length; p++)
                            {
                                pixels[p].a = 255;
                            }
                        }
                        else if (isFace)
                        {
                            // Facial overlays: Eyes, Iris, Eyeline, Eyebrows, Mouth
                            // VRoid OBJ exports have RGB features on black background (0,0,0) with A=0.
                            // Active feature strokes have RGB > 10 or A > 40 -> set A = 255.
                            // Pure black pixels are background cutout -> set A = 0.
                            for (int p = 0; p < pixels.Length; p++)
                            {
                                Color32 c = pixels[p];
                                if (c.r > 10 || c.g > 10 || c.b > 10 || c.a > 40)
                                {
                                    pixels[p].a = 255;
                                }
                                else
                                {
                                    pixels[p].a = 0;
                                }
                            }
                        }
                        // For non-face clothes (Tops, Bottoms, Shoes): keep authentic texture alpha intact!
                        tex.SetPixels32(pixels);
                        tex.Apply(true);
                    }

                    Material mat;
                    if (isFace && faceShader != null)
                    {
                        mat = new Material(faceShader);
                        mat.name = matName + "_Mat";
                        if (tex != null)
                        {
                            mat.mainTexture = tex;
                            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                        }
                        mat.color = Color.white;
                        if (mat.HasProperty("_ShadowTint"))
                            mat.SetColor("_ShadowTint", new Color(0.90f, 0.82f, 0.84f));

                        bool isIris = matName.IndexOf("Iris", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isHighlight = matName.IndexOf("Highlight", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isEyeWhite = matName.IndexOf("EyeWhite", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (isSkin)
                        {
                            // Skin rendered first, never discarded by alpha cutout
                            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.0f);
                            if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.0f);
                            mat.renderQueue = 2450;
                        }
                        else
                        {
                            // Facial overlays (Eyes, brows, eyelashes, mouth)
                            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);

                            if (isIris)
                            {
                                // Vibrant glowing anime iris
                                if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.45f);
                                mat.renderQueue = 2452;
                            }
                            else if (isHighlight)
                            {
                                // Sparkling eye highlight reflections
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
                                // Brows, eyeline, eyelashes, mouth
                                if (mat.HasProperty("_Emission")) mat.SetFloat("_Emission", 0.0f);
                                mat.renderQueue = 2454;
                            }
                        }
                    }
                    else if (toonShader != null)
                    {
                        mat = new Material(toonShader);
                        mat.name = matName + "_Mat";
                        if (tex != null)
                        {
                            mat.mainTexture = tex;
                            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                        }
                        mat.color = Color.white;

                        if (isHair)
                        {
                            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.05f);
                            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.20f, 0.22f, 0.32f));
                            if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.95f, 0.98f, 1.0f));
                            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 48.0f);
                            if (mat.HasProperty("_RimColor")) mat.SetColor("_RimColor", new Color(0.85f, 0.92f, 1.0f));
                            if (mat.HasProperty("_RimPower")) mat.SetFloat("_RimPower", 3.2f);
                        }
                        else if (isTops)
                        {
                            // VRoid Tops: 0.5f cutoff reveals authentic cargo pants underneath
                            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
                            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.50f, 0.54f, 0.68f));
                            if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.85f, 0.90f, 1.0f));
                            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 24.0f);
                        }
                        else
                        {
                            // Bottoms, shoes, body skin
                            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.10f);
                            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(0.40f, 0.44f, 0.58f));
                            if (mat.HasProperty("_SpecularColor")) mat.SetColor("_SpecularColor", new Color(0.70f, 0.75f, 0.85f));
                            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 20.0f);
                        }
                    }
                    else
                    {
                        mat = new Material(Shader.Find("Standard"));
                        if (tex != null) mat.mainTexture = tex;
                    }

                    materials[subIndex] = mat;
                    subIndex++;
                }

                mesh.RecalculateBounds();
                if (normArray.Length == 0) mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                mf.sharedMesh = mesh;
                mr.sharedMaterials = materials;
            }

            // Authentic cleanroom silhouette without extraneous detached accessories
            // (Removed procedural floating datapad and wrist hologram HUD per user feedback)

            // Add smooth breathing idle and locomotion stride animation
            root.AddComponent<AnimeIdleBreather>();

            Debug.Log($"[ObjLoader] Successfully instantiated authentic 3D anime {(isMale ? "male" : "female")} researcher!");
            return root;
        }

        private static int GetFaceMaterialPriority(string matName)
        {
            if (matName.IndexOf("Skin", StringComparison.OrdinalIgnoreCase) >= 0) return 0;
            if (matName.IndexOf("EyeWhite", StringComparison.OrdinalIgnoreCase) >= 0) return 10;
            if (matName.IndexOf("EyeIris", StringComparison.OrdinalIgnoreCase) >= 0) return 20;
            if (matName.IndexOf("EyeHighlight", StringComparison.OrdinalIgnoreCase) >= 0) return 30;
            if (matName.IndexOf("EyeExtra", StringComparison.OrdinalIgnoreCase) >= 0) return 40;
            if (matName.IndexOf("Mouth", StringComparison.OrdinalIgnoreCase) >= 0) return 50;
            if (matName.IndexOf("Eyeline", StringComparison.OrdinalIgnoreCase) >= 0) return 60;
            if (matName.IndexOf("Eyelash", StringComparison.OrdinalIgnoreCase) >= 0) return 70;
            if (matName.IndexOf("Brow", StringComparison.OrdinalIgnoreCase) >= 0) return 80;
            return 100;
        }

        private static int ParseFaceIndex(string token)
        {
            int slash = token.IndexOf('/');
            if (slash >= 0)
            {
                return int.Parse(token.Substring(0, slash)) - 1;
            }
            return int.Parse(token) - 1;
        }

        private static Dictionary<string, string> ParseMtl(string mtlPath)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(mtlPath)) return map;

            string currentMtl = null;
            using (StreamReader reader = new StreamReader(mtlPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("newmtl "))
                    {
                        currentMtl = line.Substring(7).Trim();
                    }
                    else if (line.StartsWith("map_Kd ") && currentMtl != null)
                    {
                        map[currentMtl] = line.Substring(7).Trim();
                    }
                }
            }
            return map;
        }

        private static void AttachFuturisticCleanroomGear(GameObject root, bool isMale,
            Material coatMat, Material coatAccentMat, Material cyanGlowMat)
        {
            float scale = isMale ? 1.0f : 0.92f;
            float waistY = isMale ? 0.92f : 0.84f;

            // Accessories: Datapad and Floating Wrist Hologram

            // 2. Right-hand Laboratory Data Tablet / Datapad (held naturally beside hip)
            GameObject datapadObj = new GameObject("SciFi_RightHand_Datapad");
            datapadObj.transform.SetParent(root.transform, false);
            float dpX = isMale ? 0.22f : 0.19f;
            float dpY = isMale ? 0.88f : 0.82f;
            float dpZ = 0.05f;
            datapadObj.transform.localPosition = new Vector3(dpX, dpY, dpZ);
            datapadObj.transform.localRotation = Quaternion.Euler(15f, -18f, 10f);

            MeshFilter dpMf = datapadObj.AddComponent<MeshFilter>();
            MeshRenderer dpMr = datapadObj.AddComponent<MeshRenderer>();
            dpMf.sharedMesh = AnimeModelSculptor.BuildTabletMesh(0.11f, 0.008f, 0.18f);
            dpMr.sharedMaterial = coatAccentMat;

            GameObject dpScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dpScreen.name = "Datapad_Screen";
            dpScreen.transform.SetParent(datapadObj.transform, false);
            dpScreen.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            dpScreen.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            dpScreen.transform.localScale = new Vector3(0.09f, 0.15f, 1f);
            if (dpScreen.GetComponent<Collider>()) UnityEngine.Object.Destroy(dpScreen.GetComponent<Collider>());
            dpScreen.GetComponent<Renderer>().sharedMaterial = cyanGlowMat;

            // 3. Left Wrist Floating Holographic HUD Display
            GameObject wristHolo = new GameObject("Holo_Wrist_Datapad");
            wristHolo.transform.SetParent(root.transform, false);
            float armX = isMale ? -0.23f : -0.19f;
            float armY = isMale ? 0.92f : 0.85f;
            float armZ = 0.10f;
            wristHolo.transform.localPosition = new Vector3(armX, armY, armZ);
            wristHolo.transform.localRotation = Quaternion.Euler(18f, 25f, -8f);

            MeshFilter holoMf = wristHolo.AddComponent<MeshFilter>();
            MeshRenderer holoMr = wristHolo.AddComponent<MeshRenderer>();
            holoMf.sharedMesh = AnimeModelSculptor.BuildHologramQuadMesh(0.13f, 0.13f);

            Shader s = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Diffuse");
            Material wristHoloMat = new Material(s);
            Texture2D wristTex = LabTextureGenerator.CreateWristHoloTexture();
            if (wristHoloMat.HasProperty("_MainTex")) wristHoloMat.SetTexture("_MainTex", wristTex);
            wristHoloMat.color = new Color(0.1f, 0.95f, 1.0f, 0.92f);
            holoMr.sharedMaterial = wristHoloMat;
        }

        public static Mesh BuildTailoredCoatSkirtMesh(float topRx, float topRz, float botRx, float botRz,
            float length, float openAngleTop, float openAngleBot, int hSlices = 8, int rSteps = 24)
        {
            Mesh mesh = new Mesh { name = "TailoredLabCoatSkirt" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            List<int> trisOuter = new List<int>();
            List<int> trisInner = new List<int>();

            int vertsPerRow = rSteps + 1;

            // 1. Outer Shell Vertices
            for (int h = 0; h <= hSlices; h++)
            {
                float t = (float)h / hSlices;
                float y = -t * length;
                float f = t + 0.15f * t * t;
                float rx = Mathf.Lerp(topRx, botRx, f);
                float rz = Mathf.Lerp(topRz, botRz, f);
                float openAngle = Mathf.Lerp(openAngleTop, openAngleBot, t);
                float angleSpan = 360f - 2f * openAngle;

                for (int s = 0; s <= rSteps; s++)
                {
                    float u = (float)s / rSteps;
                    float angleDeg = openAngle + u * angleSpan;
                    float rad = angleDeg * Mathf.Deg2Rad;

                    float nx = Mathf.Sin(rad);
                    float nz = Mathf.Cos(rad);
                    float x = rx * nx;
                    float z = rz * nz;

                    verts.Add(new Vector3(x, y, z));
                    norms.Add(new Vector3(nx, -0.05f, nz).normalized);
                    uvs.Add(new Vector2(u, t));
                }
            }

            // Outer Shell Triangles
            for (int h = 0; h < hSlices; h++)
            {
                for (int s = 0; s < rSteps; s++)
                {
                    int i0 = h * vertsPerRow + s;
                    int i1 = i0 + 1;
                    int i2 = (h + 1) * vertsPerRow + s;
                    int i3 = i2 + 1;

                    trisOuter.Add(i0);
                    trisOuter.Add(i2);
                    trisOuter.Add(i1);

                    trisOuter.Add(i1);
                    trisOuter.Add(i2);
                    trisOuter.Add(i3);
                }
            }

            int outerVertCount = verts.Count;

            // 2. Inner Shell Vertices (Offset 5mm inward, reversed normals)
            for (int h = 0; h <= hSlices; h++)
            {
                float t = (float)h / hSlices;
                float y = -t * length;
                float f = t + 0.15f * t * t;
                float rx = Mathf.Max(0.08f, Mathf.Lerp(topRx, botRx, f) - 0.005f);
                float rz = Mathf.Max(0.08f, Mathf.Lerp(topRz, botRz, f) - 0.005f);
                float openAngle = Mathf.Lerp(openAngleTop, openAngleBot, t);
                float angleSpan = 360f - 2f * openAngle;

                for (int s = 0; s <= rSteps; s++)
                {
                    float u = (float)s / rSteps;
                    float angleDeg = openAngle + u * angleSpan;
                    float rad = angleDeg * Mathf.Deg2Rad;

                    float nx = Mathf.Sin(rad);
                    float nz = Mathf.Cos(rad);
                    float x = rx * nx;
                    float z = rz * nz;

                    verts.Add(new Vector3(x, y, z));
                    norms.Add(new Vector3(-nx, 0.05f, -nz).normalized);
                    uvs.Add(new Vector2(u, t));
                }
            }

            // Inner Shell Triangles (Facing inward)
            for (int h = 0; h < hSlices; h++)
            {
                for (int s = 0; s < rSteps; s++)
                {
                    int o0 = outerVertCount + h * vertsPerRow + s;
                    int o1 = o0 + 1;
                    int o2 = outerVertCount + (h + 1) * vertsPerRow + s;
                    int o3 = o2 + 1;

                    trisInner.Add(o0);
                    trisInner.Add(o1);
                    trisInner.Add(o2);

                    trisInner.Add(o1);
                    trisInner.Add(o3);
                    trisInner.Add(o2);
                }
            }

            List<int> trisCyanTrim = new List<int>();

            // 3. Side Lapels (Neon Cyan Glow Trim)
            // Left Front Lapel Seam (s = 0)
            for (int h = 0; h < hSlices; h++)
            {
                int out0 = h * vertsPerRow;
                int out1 = (h + 1) * vertsPerRow;
                int in0 = outerVertCount + h * vertsPerRow;
                int in1 = outerVertCount + (h + 1) * vertsPerRow;

                trisCyanTrim.Add(out0);
                trisCyanTrim.Add(in0);
                trisCyanTrim.Add(out1);

                trisCyanTrim.Add(in0);
                trisCyanTrim.Add(in1);
                trisCyanTrim.Add(out1);
            }

            // Right Front Lapel Seam (s = rSteps)
            for (int h = 0; h < hSlices; h++)
            {
                int out0 = h * vertsPerRow + rSteps;
                int out1 = (h + 1) * vertsPerRow + rSteps;
                int in0 = outerVertCount + h * vertsPerRow + rSteps;
                int in1 = outerVertCount + (h + 1) * vertsPerRow + rSteps;

                trisCyanTrim.Add(out0);
                trisCyanTrim.Add(out1);
                trisCyanTrim.Add(in0);

                trisCyanTrim.Add(in0);
                trisCyanTrim.Add(out1);
                trisCyanTrim.Add(in1);
            }

            // Bottom Hem (h = hSlices)
            int botRowStartOut = hSlices * vertsPerRow;
            int botRowStartIn = outerVertCount + hSlices * vertsPerRow;
            for (int s = 0; s < rSteps; s++)
            {
                int out0 = botRowStartOut + s;
                int out1 = out0 + 1;
                int in0 = botRowStartIn + s;
                int in1 = in0 + 1;

                trisInner.Add(out0);
                trisInner.Add(out1);
                trisInner.Add(in0);

                trisInner.Add(in0);
                trisInner.Add(out1);
                trisInner.Add(in1);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);

            mesh.subMeshCount = 3;
            mesh.SetTriangles(trisOuter, 0);
            mesh.SetTriangles(trisInner, 1);
            mesh.SetTriangles(trisCyanTrim, 2);

            mesh.RecalculateBounds();
            return mesh;
        }
    }

    /// <summary>
    /// Organic anime idle breathing & dynamic walking stride bobbing.
    /// Keeps the character feeling alive while standing or walking around the lab.
    /// </summary>
    public class AnimeIdleBreather : MonoBehaviour
    {
        private Vector3 _basePos;
        private float _time;
        private CharacterController _cc;

        private void Start()
        {
            _basePos = transform.localPosition;
            _cc = GetComponentInParent<CharacterController>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _time += dt;

            float speed = _cc != null ? _cc.velocity.magnitude : 0f;

            if (speed > 0.1f)
            {
                // Active locomotion stride bobbing
                float stepFreq = Mathf.Clamp(speed * 3.2f, 6.0f, 14.0f);
                float stepCycle = _time * stepFreq;
                float bobY = Mathf.Abs(Mathf.Sin(stepCycle)) * 0.022f;
                float rollZ = Mathf.Sin(stepCycle) * 1.2f;

                transform.localPosition = _basePos + new Vector3(0f, bobY, 0f);
                transform.localRotation = Quaternion.Euler(0f, 0f, rollZ);
            }
            else
            {
                // Subtle organic anime idle breathing & gentle weight shift
                float breatheY = Mathf.Sin(_time * 1.6f) * 0.008f;
                float swayX = Mathf.Sin(_time * 0.8f) * 0.003f;
                transform.localPosition = _basePos + new Vector3(swayX, breatheY, 0f);
                transform.localRotation = Quaternion.identity;
            }
        }
    }
}
