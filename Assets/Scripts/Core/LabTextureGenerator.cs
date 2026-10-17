using System.IO;
using UnityEngine;

namespace BioLab.Core
{
    /// <summary>
    /// Procedural texture engine generating high-resolution cleanroom flooring,
    /// modular wall panels, anime facial textures, digital displays, and ID badges.
    /// </summary>
    public static class LabTextureGenerator
    {
        public static Texture2D CreateFloorTexture()
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            Color slateBase = new Color(0.12f, 0.16f, 0.22f, 1f); // Deep slate-grey epoxy
            Color gridLine = new Color(0.08f, 0.11f, 0.16f, 1f);  // Tile seams
            Color hazardYellow = new Color(0.95f, 0.72f, 0.1f, 1f); // Safety yellow
            Color hazardBlack = new Color(0.1f, 0.1f, 0.12f, 1f);   // Caution stripe black

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = slateBase;

                    // Tile grid lines (64px interval)
                    if (x % 128 < 3 || y % 128 < 3)
                    {
                        col = gridLine;
                    }

                    // Safety hazard zone stripe along the top and bottom borders
                    if (y < 24 || y > height - 25)
                    {
                        int stripe = (x + y) / 16;
                        col = (stripe % 2 == 0) ? hazardYellow : hazardBlack;
                    }

                    // Walkway center dashed line
                    if (x > 250 && x < 262 && (y / 32) % 2 == 0)
                    {
                        col = new Color(0.85f, 0.9f, 0.95f, 1f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateWallTexture()
        {
            int width = 256;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            Color panelWhite = new Color(0.88f, 0.91f, 0.94f, 1f); // Cleanroom sterile off-white
            Color seamDark = new Color(0.25f, 0.3f, 0.38f, 1f);    // Anodized joint seam
            Color baseboardCol = new Color(0.1f, 0.13f, 0.2f, 1f); // Dark coved baseboard bottom

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = panelWhite;

                    // Vertical panel seams every 128px
                    if (x < 4 || x > width - 5)
                    {
                        col = seamDark;
                    }

                    // Bottom baseboard strip (bottom 20px)
                    if (y < 22)
                    {
                        col = baseboardCol;
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateAnimeFaceTexture(bool isFemale)
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            Color skinTone = isFemale ? new Color(1.0f, 0.92f, 0.88f, 1f) : new Color(0.98f, 0.89f, 0.84f, 1f);
            Color eyeOuter = isFemale ? new Color(0.05f, 0.45f, 0.65f, 1f) : new Color(0.1f, 0.25f, 0.48f, 1f);
            Color eyeInner = isFemale ? new Color(0.2f, 0.75f, 0.88f, 1f) : new Color(0.25f, 0.55f, 0.85f, 1f);
            Color pupilCol = new Color(0.05f, 0.08f, 0.15f, 1f);
            Color blushCol = new Color(0.98f, 0.65f, 0.65f, 0.4f);
            Color browCol = isFemale ? new Color(0.25f, 0.45f, 0.65f, 1f) : new Color(0.18f, 0.22f, 0.35f, 1f);

            // Fill skin base
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, skinTone);
                }
            }

            // Draw Eyes (Left eye center ~ (180, 260), Right eye center ~ (332, 260))
            int eyeY = isFemale ? 260 : 255;
            int eyeW = isFemale ? 46 : 38;
            int eyeH = isFemale ? 65 : 48;

            DrawAnimeEye(tex, 180, eyeY, eyeW, eyeH, eyeOuter, eyeInner, pupilCol, browCol, true);
            DrawAnimeEye(tex, 332, eyeY, eyeW, eyeH, eyeOuter, eyeInner, pupilCol, browCol, false);

            // Anime Blush cheeks for female
            if (isFemale)
            {
                DrawBlush(tex, 150, 205, 30, 14, blushCol);
                DrawBlush(tex, 362, 205, 30, 14, blushCol);
            }

            // Subtle nose dot & lip contour
            DrawDot(tex, 256, isFemale ? 200 : 195, 3, new Color(0.85f, 0.65f, 0.6f, 0.7f));
            DrawMouth(tex, 256, isFemale ? 165 : 155, isFemale ? 24 : 30, new Color(0.82f, 0.45f, 0.48f, 0.9f));

            tex.Apply();
            return tex;
        }

        private static void DrawAnimeEye(Texture2D tex, int cx, int cy, int rx, int ry, Color outer, Color inner, Color pupil, Color brow, bool isLeft)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float d = dx * dx + dy * dy;

                    if (d <= 1.0f)
                    {
                        // Sclera (White of eye)
                        Color col = new Color(0.97f, 0.98f, 1.0f, 1f);

                        // Iris region (inner 75% width)
                        float irisDx = dx / 0.75f;
                        float irisD = irisDx * irisDx + dy * dy;
                        if (irisD <= 1.0f)
                        {
                            // Vertical color gradient
                            float t = Mathf.Clamp01((dy + 1f) * 0.5f);
                            col = Color.Lerp(inner, outer, t);

                            // Pupil in center
                            if (irisD <= 0.25f)
                            {
                                col = pupil;
                            }

                            // Specular catchlights (White anime sparkles)
                            float hx1 = (float)(x - (cx - 8)) / 8f;
                            float hy1 = (float)(y - (cy + 14)) / 10f;
                            if (hx1 * hx1 + hy1 * hy1 <= 1.0f)
                            {
                                col = Color.white;
                            }

                            float hx2 = (float)(x - (cx + 10)) / 5f;
                            float hy2 = (float)(y - (cy - 10)) / 6f;
                            if (hx2 * hx2 + hy2 * hy2 <= 1.0f)
                            {
                                col = Color.white;
                            }
                        }

                        // Upper dark anime eyelid / eyelash line
                        if (dy > 0.7f)
                        {
                            col = new Color(0.1f, 0.1f, 0.15f, 1f);
                        }

                        tex.SetPixel(x, y, col);
                    }
                }
            }

            // Eyebrow above eye
            int browY = cy + ry + 18;
            for (int bx = cx - rx - 4; bx <= cx + rx + 4; bx++)
            {
                float t = (float)(bx - (cx - rx)) / (rx * 2f);
                int by = browY + (int)(Mathf.Sin(t * Mathf.PI) * 8f);
                for (int thick = -2; thick <= 2; thick++)
                {
                    tex.SetPixel(bx, by + thick, brow);
                }
            }
        }

        private static void DrawBlush(Texture2D tex, int cx, int cy, int rx, int ry, Color col)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    if (dx * dx + dy * dy <= 1.0f)
                    {
                        Color existing = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, Color.Lerp(existing, col, 0.45f));
                    }
                }
            }
        }

        private static void DrawDot(Texture2D tex, int cx, int cy, int radius, Color col)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                    {
                        tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static void DrawMouth(Texture2D tex, int cx, int cy, int width, Color col)
        {
            int halfW = width / 2;
            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                float t = (float)(x - (cx - halfW)) / width;
                int y = cy - (int)(Mathf.Sin(t * Mathf.PI) * 4f);
                tex.SetPixel(x, y, col);
                tex.SetPixel(x, y - 1, col);
            }
        }

        public static Texture2D CreateCentrifugeDisplayTexture()
        {
            int width = 256;
            int height = 128;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            Color bg = new Color(0.04f, 0.06f, 0.1f, 1f); // Glossy OLED black
            Color cyanGlow = new Color(0.0f, 0.85f, 0.95f, 1f);
            Color greenLED = new Color(0.1f, 0.95f, 0.4f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = bg;

                    // Border
                    if (x < 3 || x > width - 4 || y < 3 || y > height - 4)
                        col = cyanGlow * 0.4f;

                    // Status bar line
                    if (y == 32)
                        col = cyanGlow * 0.7f;

                    // Progress indicators
                    if (y > 10 && y < 22 && x > 20 && x < 236)
                    {
                        col = (x < 180) ? greenLED : new Color(0.15f, 0.2f, 0.25f, 1f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateFreezerDisplayTexture()
        {
            int width = 128;
            int height = 64;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);

            Color bg = new Color(0.02f, 0.05f, 0.08f, 1f);
            Color segGreen = new Color(0.1f, 0.98f, 0.5f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = bg;
                    // Digital segments mock (-80.0 C)
                    if (y > 20 && y < 44 && x > 15 && x < 113)
                    {
                        if ((x % 24 < 18) && (y == 22 || y == 32 || y == 42 || x % 24 == 2 || x % 24 == 16))
                            col = segGreen;
                    }
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateWristHoloTexture()
        {
            int width = 256;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color clear = new Color(0, 0, 0, 0);
            Color cyan = new Color(0.0f, 0.92f, 1.0f, 0.95f);
            Color glow = new Color(0.0f, 0.55f, 0.85f, 0.22f);
            float cx = width * 0.5f;
            float cy = height * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    Color col = clear;

                    if (dist < 105f)
                    {
                        col = glow;

                        // Outer ring (r = 100)
                        if (Mathf.Abs(dist - 100f) < 2.0f) col = cyan;
                        // Mid ring (r = 82)
                        if (Mathf.Abs(dist - 82f) < 1.5f) col = cyan * 0.85f;
                        // Inner ring (r = 55)
                        if (Mathf.Abs(dist - 55f) < 1.5f) col = cyan;
                        // Center core
                        if (dist < 12f) col = cyan;

                        // Radial tick marks every 30 degrees
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (Mathf.Abs(angle % 30f) < 1.8f && dist > 78f && dist < 102f)
                            col = cyan;

                        // Horizontal crosshair
                        if (Mathf.Abs(dy) < 1.5f && (dist < 40f || dist > 65f && dist < 95f))
                            col = cyan;

                        // Data waveforms inside inner circle
                        if (dist < 50f && Mathf.Abs(dy - Mathf.Sin(dx * 0.25f) * 14f) < 2.0f)
                            col = new Color(0.1f, 1.0f, 0.6f, 0.95f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D CreateHoloDiagnosticTexture()
        {
            int width = 512;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color bg = new Color(0.01f, 0.04f, 0.08f, 0.65f);
            Color cyan = new Color(0.0f, 0.90f, 1.0f, 0.95f);
            Color cyanDim = new Color(0.0f, 0.45f, 0.65f, 0.70f);
            Color green = new Color(0.1f, 1.0f, 0.55f, 0.95f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = bg;

                    // Holographic Border Frame
                    if (x < 6 || x > width - 7 || y < 6 || y > height - 7)
                        col = cyan;
                    // Corner Brackets
                    if ((x < 28 || x > width - 29) && (y < 28 || y > height - 29))
                    {
                        if (x < 12 || x > width - 13 || y < 12 || y > height - 13)
                            col = cyan;
                    }

                    // Top Header Bar (y: 224 to 244)
                    if (y > 224 && y < 244)
                    {
                        if (y == 225 || y == 243 || x == 14 || x == width - 15)
                            col = cyan;
                        else if (x > 20 && x < 280 && (x / 8) % 2 == 0)
                            col = cyanDim;
                    }

                    // Left Pane: DNA Helix Wave (x: 20 to 160)
                    if (x > 24 && x < 160 && y > 30 && y < 210)
                    {
                        float waveY1 = 120f + Mathf.Sin((x - 24) * 0.08f) * 45f;
                        float waveY2 = 120f - Mathf.Sin((x - 24) * 0.08f) * 45f;
                        if (Mathf.Abs(y - waveY1) < 2.5f) col = cyan;
                        if (Mathf.Abs(y - waveY2) < 2.5f) col = green;
                        // DNA base pair rungs
                        if ((x - 24) % 18 == 0 && y >= Mathf.Min(waveY1, waveY2) && y <= Mathf.Max(waveY1, waveY2))
                            col = cyanDim;
                    }

                    // Center Pane: Cardiac ECG Waveform (x: 180 to 340)
                    if (x > 180 && x < 340 && y > 30 && y < 210)
                    {
                        // Grid background
                        if (x % 20 == 0 || y % 20 == 0) col = Color.Lerp(bg, cyanDim, 0.25f);

                        // ECG Pulse
                        float localX = (x - 180) % 80;
                        float ecgY = 120f;
                        if (localX > 30 && localX < 36) ecgY = 120f + (localX - 30) * 8f; // P wave
                        else if (localX >= 36 && localX < 40) ecgY = 120f - (localX - 36) * 10f; // Q dip
                        else if (localX >= 40 && localX < 46) ecgY = 80f + (localX - 40) * 16f; // R spike
                        else if (localX >= 46 && localX < 52) ecgY = 176f - (localX - 46) * 14f; // S dip
                        else if (localX >= 52 && localX < 62) ecgY = 120f + Mathf.Sin((localX - 52) * 0.31f) * 14f; // T wave

                        if (Mathf.Abs(y - ecgY) < 2.5f) col = cyan;
                    }

                    // Right Pane: Telemetry Gauges (x: 360 to 490)
                    if (x > 360 && x < 490 && y > 30 && y < 210)
                    {
                        // Horizontal status bars
                        for (int b = 0; b < 5; b++)
                        {
                            int barY = 50 + b * 32;
                            if (y >= barY && y <= barY + 16)
                            {
                                int fillW = 360 + (b == 0 ? 110 : (b == 1 ? 85 : (b == 2 ? 100 : 70)));
                                col = (x < fillW) ? ((b % 2 == 0) ? cyan : green) : cyanDim * 0.3f;
                            }
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
