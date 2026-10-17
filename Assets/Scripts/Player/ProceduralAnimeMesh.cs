using System.Collections.Generic;
using UnityEngine;

namespace BioLab.Player
{
    /// <summary>
    /// High-precision procedural anime mesh generation engine.
    /// Replaces primitive boxes with smooth, subdivided, organic 3D geometry:
    /// sculpted anime heads with planar frontal facial UVs, curved tapered hair locks,
    /// standing high collars, flowing flared coat tails, tapered limbs, and cyber boots.
    /// </summary>
    public static class ProceduralAnimeMesh
    {
        /// <summary>
        /// Generates a contoured anime head mesh with tapered jaw, sharp chin, and planar frontal UVs.
        /// </summary>
        public static Mesh CreateAnimeHeadMesh(float rx = 0.12f, float ry = 0.145f, float rz = 0.13f, int latSegments = 18, int lonSegments = 26)
        {
            Mesh mesh = new Mesh { name = "AnimeHead_Procedural" };
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float v = (float)lat / latSegments;
                float phi = v * Mathf.PI; // 0 (top) to PI (bottom chin)

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float u = (float)lon / lonSegments;
                    float theta = u * Mathf.PI * 2f; // 0 to 2PI

                    // Standard ellipsoid
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    float sinTheta = Mathf.Sin(theta);
                    float cosTheta = Mathf.Cos(theta);

                    float x = rx * sinPhi * cosTheta;
                    float y = ry * cosPhi;
                    float z = rz * sinPhi * sinTheta;

                    // Anime facial sculpting:
                    // 1. Lower face taper into sleek anime jaw and pointed chin
                    if (y < 0f)
                    {
                        float taperProgress = -y / ry; // 0 at mouth level to 1 at chin tip
                        // Taper lateral width (jawline)
                        float taperX = 1f - (taperProgress * 0.45f);
                        x *= taperX;

                        // Push chin apex slightly forward and down
                        if (z > 0f)
                        {
                            z += taperProgress * 0.022f;
                        }
                    }
                    else
                    {
                        // Upper forehead curvature
                        float domeProgress = y / ry;
                        x *= (1f - domeProgress * 0.15f);
                    }

                    // 2. Mid-face flatness for eyes/cheeks (Z > 0)
                    if (z > 0f && y > -ry * 0.4f && y < ry * 0.5f)
                    {
                        // Soften frontal curve to form anime cheek/eye plane
                        z = Mathf.Lerp(z, rz * 0.82f, 0.28f);
                    }

                    Vector3 pos = new Vector3(x, y, z);
                    vertices.Add(pos);

                    // UV Mapping:
                    // For the frontal half (z >= -0.02f), project planar frontal UVs
                    // so the anime face texture (eyes, lashes, blush, mouth) maps directly
                    if (z >= -0.015f)
                    {
                        float uvU = (x / (rx * 2.45f)) + 0.5f;
                        float uvV = 0.50f + (y / (ry * 1.55f));
                        uvs.Add(new Vector2(Mathf.Clamp01(uvU), Mathf.Clamp01(uvV)));
                    }
                    else
                    {
                        // Back/sides map to neutral skin tone border
                        uvs.Add(new Vector2(0.05f, 0.05f));
                    }
                }
            }

            // Triangle indexing
            int lonStride = lonSegments + 1;
            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int current = lat * lonStride + lon;
                    int next = current + lonStride;

                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);

                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a smooth, curved, tapered 3D anime hair strand or tuft.
        /// </summary>
        public static Mesh CreateCurvedSpike(float baseRadiusX, float baseRadiusZ, float length, Vector3 tipOffset, float tipSharpness = 0.012f, int radialSegments = 8, int heightSegments = 6)
        {
            Mesh mesh = new Mesh { name = "CurvedHairSpike_Procedural" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            int radialStride = radialSegments + 1;

            // Generate rings along spine
            for (int h = 0; h <= heightSegments; h++)
            {
                float t = (float)h / heightSegments;
                // Quadratic curve for natural hair bend
                Vector3 spinePos = (Vector3.forward * (t * length)) + (tipOffset * (t * t));

                // Taper radius towards tip
                float scale = Mathf.Lerp(1.0f, tipSharpness, Mathf.Pow(t, 1.2f));
                float curRx = baseRadiusX * scale;
                float curRz = baseRadiusZ * scale;

                for (int r = 0; r <= radialSegments; r++)
                {
                    float u = (float)r / radialSegments;
                    float angle = u * Mathf.PI * 2f;

                    float vx = Mathf.Cos(angle) * curRx;
                    float vy = Mathf.Sin(angle) * curRz;

                    Vector3 pt = spinePos + new Vector3(vx, vy, 0f);
                    vertices.Add(pt);
                    uvs.Add(new Vector2(u, t));
                }
            }

            // Quads between rings
            for (int h = 0; h < heightSegments; h++)
            {
                for (int r = 0; r < radialSegments; r++)
                {
                    int current = h * radialStride + r;
                    int next = current + radialStride;

                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);

                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }

            // Base cap
            int baseCenterIdx = vertices.Count;
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));
            for (int r = 0; r < radialSegments; r++)
            {
                triangles.Add(baseCenterIdx);
                triangles.Add(r + 1);
                triangles.Add(r);
            }

            // Tip cap
            int tipCenterIdx = vertices.Count;
            Vector3 tipApex = (Vector3.forward * length) + tipOffset;
            vertices.Add(tipApex);
            uvs.Add(new Vector2(0.5f, 1f));
            int lastRingBase = heightSegments * radialStride;
            for (int r = 0; r < radialSegments; r++)
            {
                triangles.Add(tipCenterIdx);
                triangles.Add(lastRingBase + r);
                triangles.Add(lastRingBase + r + 1);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a high-standing futuristic open coat collar ribbon hugging the neck with flare.
        /// </summary>
        public static Mesh CreateStandingCollar(float radiusX = 0.11f, float radiusZ = 0.12f, float height = 0.12f, float flareAngle = 16f, float openingAngleDeg = 65f, int segments = 16)
        {
            Mesh mesh = new Mesh { name = "StandingCollar_Procedural" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float startAngle = (-180f + openingAngleDeg * 0.5f) * Mathf.Deg2Rad;
            float endAngle = (180f - openingAngleDeg * 0.5f) * Mathf.Deg2Rad;
            float totalSpan = endAngle - startAngle;

            float flareRad = flareAngle * Mathf.Deg2Rad;
            float flareOut = Mathf.Tan(flareRad) * height;

            float thickness = 0.015f;

            int stride = (segments + 1);

            // 4 profiles: Outer-Bottom, Outer-Top, Inner-Top, Inner-Bottom
            for (int seg = 0; seg <= segments; seg++)
            {
                float t = (float)seg / segments;
                float angle = startAngle + t * totalSpan;
                float sinA = Mathf.Sin(angle);
                float cosA = Mathf.Cos(angle);

                // Base ring
                float bxOut = cosA * (radiusX + thickness * 0.5f);
                float bzOut = sinA * (radiusZ + thickness * 0.5f);
                float bxIn = cosA * (radiusX - thickness * 0.5f);
                float bzIn = sinA * (radiusZ - thickness * 0.5f);

                // Top ring (flared)
                float txOut = cosA * (radiusX + flareOut + thickness * 0.5f);
                float tzOut = sinA * (radiusZ + flareOut + thickness * 0.5f);
                float txIn = cosA * (radiusX + flareOut - thickness * 0.5f);
                float tzIn = sinA * (radiusZ + flareOut - thickness * 0.5f);

                // Vertices
                vertices.Add(new Vector3(bxOut, 0f, bzOut));      // 0: OB
                vertices.Add(new Vector3(txOut, height, tzOut));  // 1: OT
                vertices.Add(new Vector3(txIn, height, tzIn));    // 2: IT
                vertices.Add(new Vector3(bxIn, 0f, bzIn));        // 3: IB

                uvs.Add(new Vector2(t, 0f));
                uvs.Add(new Vector2(t, 0.5f));
                uvs.Add(new Vector2(t, 0.75f));
                uvs.Add(new Vector2(t, 1.0f));
            }

            for (int seg = 0; seg < segments; seg++)
            {
                int c = seg * 4;
                int n = (seg + 1) * 4;

                // Outer face
                triangles.Add(c + 0); triangles.Add(c + 1); triangles.Add(n + 0);
                triangles.Add(n + 0); triangles.Add(c + 1); triangles.Add(n + 1);

                // Top rim
                triangles.Add(c + 1); triangles.Add(c + 2); triangles.Add(n + 1);
                triangles.Add(n + 1); triangles.Add(c + 2); triangles.Add(n + 2);

                // Inner face
                triangles.Add(c + 2); triangles.Add(c + 3); triangles.Add(n + 2);
                triangles.Add(n + 2); triangles.Add(c + 3); triangles.Add(n + 3);

                // Bottom rim
                triangles.Add(c + 3); triangles.Add(c + 0); triangles.Add(n + 3);
                triangles.Add(n + 3); triangles.Add(c + 0); triangles.Add(n + 0);
            }

            // Left & right edge caps
            int leftBase = 0;
            triangles.Add(leftBase + 0); triangles.Add(leftBase + 3); triangles.Add(leftBase + 1);
            triangles.Add(leftBase + 1); triangles.Add(leftBase + 3); triangles.Add(leftBase + 2);

            int rightBase = segments * 4;
            triangles.Add(rightBase + 0); triangles.Add(rightBase + 1); triangles.Add(rightBase + 3);
            triangles.Add(rightBase + 1); triangles.Add(rightBase + 2); triangles.Add(rightBase + 3);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a curved, flowing coat tail panel with diagonal hem cut and edge bevel.
        /// </summary>
        public static Mesh CreateFlaredCoatTail(float topWidth = 0.22f, float bottomWidth = 0.32f, float length = 0.65f, float flareOutAngle = 14f, float flareSideAngle = 10f, bool isLeft = true, int slices = 5)
        {
            Mesh mesh = new Mesh { name = "FlaredCoatTail_Procedural" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float sideSign = isLeft ? -1f : 1f;
            float flareOutRad = flareOutAngle * Mathf.Deg2Rad;
            float flareSideRad = flareSideAngle * Mathf.Deg2Rad;

            for (int row = 0; row <= slices; row++)
            {
                float t = (float)row / slices;
                float curY = -t * length;
                float curZ = -Mathf.Sin(flareOutRad * t) * length * 0.4f;
                float curSideOffset = sideSign * Mathf.Sin(flareSideRad * t) * length * 0.35f;

                float curW = Mathf.Lerp(topWidth, bottomWidth, t);

                // Diagonal hem cut (outer side hangs lower than inner lapel)
                float hemAdjust = 0f;
                if (row == slices)
                {
                    hemAdjust = isLeft ? 0.04f : -0.04f;
                }

                // Inner edge
                vertices.Add(new Vector3(curSideOffset, curY, curZ));
                uvs.Add(new Vector2(0f, t));

                // Outer edge
                vertices.Add(new Vector3(curSideOffset + sideSign * curW, curY + hemAdjust, curZ));
                uvs.Add(new Vector2(1f, t));
            }

            for (int row = 0; row < slices; row++)
            {
                int c = row * 2;
                int n = (row + 1) * 2;

                if (isLeft)
                {
                    triangles.Add(c + 0); triangles.Add(c + 1); triangles.Add(n + 0);
                    triangles.Add(n + 0); triangles.Add(c + 1); triangles.Add(n + 1);

                    // Double sided
                    triangles.Add(c + 0); triangles.Add(n + 0); triangles.Add(c + 1);
                    triangles.Add(n + 0); triangles.Add(n + 1); triangles.Add(c + 1);
                }
                else
                {
                    triangles.Add(c + 0); triangles.Add(n + 0); triangles.Add(c + 1);
                    triangles.Add(n + 0); triangles.Add(n + 1); triangles.Add(c + 1);

                    // Double sided
                    triangles.Add(c + 0); triangles.Add(c + 1); triangles.Add(n + 0);
                    triangles.Add(n + 0); triangles.Add(c + 1); triangles.Add(n + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a smooth, tapered cylindrical limb (arms, forearms, nitrile gloves, thighs, calves).
        /// </summary>
        public static Mesh CreateTaperedLimb(float topRadius, float bottomRadius, float length, int radialSegments = 12, int heightSegments = 4)
        {
            Mesh mesh = new Mesh { name = "TaperedLimb_Procedural" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            int stride = radialSegments + 1;

            for (int h = 0; h <= heightSegments; h++)
            {
                float t = (float)h / heightSegments;
                float y = -t * length;
                float r = Mathf.Lerp(topRadius, bottomRadius, t);

                for (int seg = 0; seg <= radialSegments; seg++)
                {
                    float u = (float)seg / radialSegments;
                    float angle = u * Mathf.PI * 2f;

                    float x = Mathf.Cos(angle) * r;
                    float z = Mathf.Sin(angle) * r;

                    vertices.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(u, t));
                }
            }

            for (int h = 0; h < heightSegments; h++)
            {
                for (int seg = 0; seg < radialSegments; seg++)
                {
                    int c = h * stride + seg;
                    int n = (h + 1) * stride + seg;

                    triangles.Add(c);
                    triangles.Add(c + 1);
                    triangles.Add(n);

                    triangles.Add(n);
                    triangles.Add(c + 1);
                    triangles.Add(n + 1);
                }
            }

            // Top cap
            int topCenter = vertices.Count;
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));
            for (int seg = 0; seg < radialSegments; seg++)
            {
                triangles.Add(topCenter);
                triangles.Add(seg);
                triangles.Add(seg + 1);
            }

            // Bottom cap
            int bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0, -length, 0));
            uvs.Add(new Vector2(0.5f, 1f));
            int baseStart = heightSegments * stride;
            for (int seg = 0; seg < radialSegments; seg++)
            {
                triangles.Add(bottomCenter);
                triangles.Add(baseStart + seg + 1);
                triangles.Add(baseStart + seg);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a sculpted antistatic cyber boot with raised heel wedge and rounded toe.
        /// </summary>
        public static Mesh CreateCyberBoot(float width = 0.12f, float length = 0.24f, float height = 0.22f, float soleHeight = 0.045f)
        {
            Mesh mesh = new Mesh { name = "CyberBoot_Procedural" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            // 8-point outline of shoe footprint
            // Points from heel to toe
            float hw = width * 0.5f;
            float hl = length * 0.5f;

            Vector2[] footprint = new Vector2[]
            {
                new Vector2(-hw * 0.75f, -hl),         // 0: Heel back L
                new Vector2( hw * 0.75f, -hl),         // 1: Heel back R
                new Vector2( hw * 0.85f, -hl * 0.4f),  // 2: Heel mid R
                new Vector2( hw,          hl * 0.2f),  // 3: Ball R
                new Vector2( hw * 0.7f,   hl),         // 4: Toe tip R
                new Vector2(-hw * 0.7f,   hl),         // 5: Toe tip L
                new Vector2(-hw,          hl * 0.2f),  // 6: Ball L
                new Vector2(-hw * 0.85f, -hl * 0.4f)   // 7: Heel mid L
            };

            int ptCount = footprint.Length;

            // 3 Layers: Sole Bottom (0), Sole Top (soleHeight), Shaft Top (height)
            // Layer 0: Sole Bottom
            for (int i = 0; i < ptCount; i++)
            {
                vertices.Add(new Vector3(footprint[i].x * 1.05f, 0f, footprint[i].y * 1.05f));
                uvs.Add(new Vector2((float)i / ptCount, 0f));
            }
            // Layer 1: Sole Top
            for (int i = 0; i < ptCount; i++)
            {
                vertices.Add(new Vector3(footprint[i].x, soleHeight, footprint[i].y));
                uvs.Add(new Vector2((float)i / ptCount, 0.25f));
            }
            // Layer 2: Ankle Shaft Top
            for (int i = 0; i < ptCount; i++)
            {
                float ankleTaper = 0.72f;
                vertices.Add(new Vector3(footprint[i].x * ankleTaper, height, footprint[i].y * ankleTaper * 0.7f));
                uvs.Add(new Vector2((float)i / ptCount, 1.0f));
            }

            // Wall faces: Layer 0 to 1 (Sole rim)
            for (int i = 0; i < ptCount; i++)
            {
                int next = (i + 1) % ptCount;
                triangles.Add(i); triangles.Add(ptCount + i); triangles.Add(next);
                triangles.Add(next); triangles.Add(ptCount + i); triangles.Add(ptCount + next);
            }

            // Wall faces: Layer 1 to 2 (Upper boot shaft)
            for (int i = 0; i < ptCount; i++)
            {
                int next = (i + 1) % ptCount;
                int c1 = ptCount + i;
                int n1 = ptCount + next;
                int c2 = ptCount * 2 + i;
                int n2 = ptCount * 2 + next;
                triangles.Add(c1); triangles.Add(c2); triangles.Add(n1);
                triangles.Add(n1); triangles.Add(c2); triangles.Add(n2);
            }

            // Bottom sole cap
            int soleCenter = vertices.Count;
            vertices.Add(new Vector3(0, 0, 0));
            uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i < ptCount; i++)
            {
                int next = (i + 1) % ptCount;
                triangles.Add(soleCenter);
                triangles.Add(next);
                triangles.Add(i);
            }

            // Top shaft opening cap
            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0, height, 0));
            uvs.Add(new Vector2(0.5f, 0.5f));
            int shaftStart = ptCount * 2;
            for (int i = 0; i < ptCount; i++)
            {
                int next = (i + 1) % ptCount;
                triangles.Add(topCenter);
                triangles.Add(shaftStart + i);
                triangles.Add(shaftStart + next);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a quad mesh for floating holographic HUD projection.
        /// </summary>
        public static Mesh CreateHologramQuad(float width = 0.16f, float height = 0.11f)
        {
            Mesh mesh = new Mesh { name = "HologramQuad_Procedural" };
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(-hw, -hh, 0),
                new Vector3( hw, -hh, 0),
                new Vector3( hw,  hh, 0),
                new Vector3(-hw,  hh, 0),
                // Double-sided back
                new Vector3(-hw, -hh, 0),
                new Vector3( hw, -hh, 0),
                new Vector3( hw,  hh, 0),
                new Vector3(-hw,  hh, 0)
            };

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)
            };

            int[] tris = new int[]
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7
            };

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
