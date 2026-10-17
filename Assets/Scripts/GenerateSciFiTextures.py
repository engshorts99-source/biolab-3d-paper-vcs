import subprocess
import os

BASE_DIR = "/Users/pjw/.gemini/antigravity/scratch/BioCleanroomLab"

def run_cmd(cmd):
    res = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    if res.returncode != 0:
        print(f"Error running: {cmd}\nStderr: {res.stderr}")
    return res

def apply_texture(svg_content, orig_tex_path, width, height, dest_rel_paths):
    tex_dir = os.path.dirname(orig_tex_path)
    mask_path = os.path.join(tex_dir, "temp_mask.png")
    svg_path = os.path.join(tex_dir, "temp_overlay.svg")
    rendered_svg = os.path.join(tex_dir, "temp_rendered.png")
    temp_out = os.path.join(tex_dir, "temp_out.png")

    # 1. Extract alpha mask from original VRoid texture
    run_cmd(f"magick '{orig_tex_path}' -alpha extract '{mask_path}'")

    # 2. Write SVG
    with open(svg_path, "w") as f:
        f.write(svg_content)

    # 3. Render SVG
    run_cmd(f"magick '{svg_path}' '{rendered_svg}'")

    # 4. Apply alpha mask via CopyOpacity
    run_cmd(f"magick '{rendered_svg}' '{mask_path}' -alpha off -compose CopyOpacity -composite '{temp_out}'")

    # 5. Distribute to all target directories
    for rel in dest_rel_paths:
        full_dest = os.path.join(BASE_DIR, rel)
        os.makedirs(os.path.dirname(full_dest), exist_ok=True)
        run_cmd(f"cp '{temp_out}' '{full_dest}'")

    # 6. Cleanup
    for p in [mask_path, svg_path, rendered_svg, temp_out]:
        if os.path.exists(p):
            os.remove(p)

    print(f"Successfully generated and installed {os.path.basename(orig_tex_path)}!")

def build_male_tops():
    svg = '''<svg width="2048" height="2048" xmlns="http://www.w3.org/2000/svg">
  <!-- Base Crisp White Lab Coat Fabric -->
  <rect x="0" y="0" width="2048" height="2048" fill="#f0f3f8"/>

  <!-- ==================== 1. SLEEVES ==================== -->
  <!-- Left Sleeve (x: 55..800, y: 7..430) -->
  <line x1="280" y1="20" x2="280" y2="360" stroke="#cbd5e1" stroke-width="4"/>
  <line x1="560" y1="20" x2="560" y2="360" stroke="#cbd5e1" stroke-width="4"/>
  <!-- Glowing Cyan Cyber Conduit on outer arm -->
  <line x1="420" y1="15" x2="420" y2="355" stroke="#00e5ff" stroke-width="12" stroke-linecap="round"/>
  <line x1="420" y1="15" x2="420" y2="355" stroke="#ffffff" stroke-width="4" stroke-linecap="round"/>
  <circle cx="420" cy="180" r="14" fill="#00e5ff"/>
  <circle cx="420" cy="180" r="6" fill="#ffffff"/>
  <!-- Wrist Gauntlet Cuff (Dark Tech Glove/Wristband) -->
  <rect x="50" y="360" width="750" height="70" fill="#181b24"/>
  <line x1="50" y1="365" x2="800" y2="365" stroke="#00e5ff" stroke-width="10"/>
  <line x1="50" y1="365" x2="800" y2="365" stroke="#ffffff" stroke-width="3"/>
  <line x1="50" y1="420" x2="800" y2="420" stroke="#00e5ff" stroke-width="5"/>

  <!-- Right Sleeve (x: 1240..1985, y: 7..430) -->
  <line x1="1480" y1="20" x2="1480" y2="360" stroke="#cbd5e1" stroke-width="4"/>
  <line x1="1760" y1="20" x2="1760" y2="360" stroke="#cbd5e1" stroke-width="4"/>
  <!-- Glowing Cyan Cyber Conduit on outer arm -->
  <line x1="1628" y1="15" x2="1628" y2="355" stroke="#00e5ff" stroke-width="12" stroke-linecap="round"/>
  <line x1="1628" y1="15" x2="1628" y2="355" stroke="#ffffff" stroke-width="4" stroke-linecap="round"/>
  <circle cx="1628" cy="180" r="14" fill="#00e5ff"/>
  <circle cx="1628" cy="180" r="6" fill="#ffffff"/>
  <!-- Wrist Gauntlet Cuff -->
  <rect x="1240" y="360" width="750" height="70" fill="#181b24"/>
  <line x1="1240" y1="365" x2="1990" y2="365" stroke="#00e5ff" stroke-width="10"/>
  <line x1="1240" y1="365" x2="1990" y2="365" stroke="#ffffff" stroke-width="3"/>
  <line x1="1240" y1="420" x2="1990" y2="420" stroke="#00e5ff" stroke-width="5"/>

  <!-- ==================== 2. COLLARS ==================== -->
  <!-- Top Middle Collars (x: 800..1240, y: 90..410) -->
  <rect x="800" y="90" width="440" height="320" fill="#f0f3f8"/>
  <line x1="810" y1="96" x2="1235" y2="96" stroke="#00e5ff" stroke-width="14"/>
  <line x1="810" y1="96" x2="1235" y2="96" stroke="#ffffff" stroke-width="4"/>
  <line x1="810" y1="240" x2="1235" y2="240" stroke="#00e5ff" stroke-width="8"/>
  <line x1="1024" y1="90" x2="1024" y2="410" stroke="#cbd5e1" stroke-width="4"/>

  <!-- ==================== 3. TORSO FLANKS & BACK ==================== -->
  <!-- Left Flank & Back (x: 390..750, y: 420..1250) -->
  <rect x="390" y="420" width="360" height="830" fill="#f0f3f8"/>
  <line x1="560" y1="420" x2="560" y2="1250" stroke="#cbd5e1" stroke-width="6"/>
  <!-- Cleanroom Sci-Fi Emblem on Back -->
  <circle cx="560" cy="720" r="48" fill="none" stroke="#00e5ff" stroke-width="6"/>
  <polygon points="560,690 540,735 580,735" fill="none" stroke="#00e5ff" stroke-width="5"/>

  <!-- Right Flank & Back (x: 1290..1660, y: 420..1250) -->
  <rect x="1290" y="420" width="370" height="830" fill="#f0f3f8"/>
  <line x1="1480" y1="420" x2="1480" y2="1250" stroke="#cbd5e1" stroke-width="6"/>
  <circle cx="1480" cy="720" r="48" fill="none" stroke="#00e5ff" stroke-width="6"/>
  <polygon points="1480,690 1460,735 1500,735" fill="none" stroke="#00e5ff" stroke-width="5"/>

  <!-- ==================== 4. FRONT CHEST & TECH SUIT ==================== -->
  <!-- Center Open Lab Coat: Revealing Dark Undersuit (x: 880..1168, y: 530..1250) -->
  <polygon points="900,530 1148,530 1168,1250 880,1250" fill="#181b24"/>

  <!-- Glowing Lapel Edges (Separating White Coat from Dark Undersuit) -->
  <line x1="900" y1="530" x2="880" y2="1250" stroke="#00e5ff" stroke-width="12" stroke-linecap="round"/>
  <line x1="900" y1="530" x2="880" y2="1250" stroke="#ffffff" stroke-width="4" stroke-linecap="round"/>
  <line x1="1148" y1="530" x2="1168" y2="1250" stroke="#00e5ff" stroke-width="12" stroke-linecap="round"/>
  <line x1="1148" y1="530" x2="1168" y2="1250" stroke="#ffffff" stroke-width="4" stroke-linecap="round"/>

  <!-- Chest Cyber Core (Arc Reactor) -->
  <circle cx="1024" cy="730" r="54" fill="#0d111a" stroke="#00e5ff" stroke-width="8"/>
  <circle cx="1024" cy="730" r="38" fill="none" stroke="#55ffff" stroke-width="6"/>
  <circle cx="1024" cy="730" r="22" fill="#00e5ff"/>
  <circle cx="1024" cy="730" r="10" fill="#ffffff"/>

  <!-- Cyber Conduits radiating across chest & ribs -->
  <line x1="970" y1="730" x2="915" y2="680" stroke="#00e5ff" stroke-width="8" stroke-linecap="round"/>
  <line x1="915" y1="680" x2="890" y2="680" stroke="#00e5ff" stroke-width="6" stroke-linecap="round"/>
  <line x1="970" y1="730" x2="915" y2="780" stroke="#00e5ff" stroke-width="8" stroke-linecap="round"/>
  <line x1="915" y1="780" x2="890" y2="780" stroke="#00e5ff" stroke-width="6" stroke-linecap="round"/>

  <line x1="1078" y1="730" x2="1133" y2="680" stroke="#00e5ff" stroke-width="8" stroke-linecap="round"/>
  <line x1="1133" y1="680" x2="1158" y2="680" stroke="#00e5ff" stroke-width="6" stroke-linecap="round"/>
  <line x1="1078" y1="730" x2="1133" y2="780" stroke="#00e5ff" stroke-width="8" stroke-linecap="round"/>
  <line x1="1133" y1="780" x2="1158" y2="780" stroke="#00e5ff" stroke-width="6" stroke-linecap="round"/>

  <!-- Center vertical abdominal power line -->
  <line x1="1024" y1="784" x2="1024" y2="1100" stroke="#00e5ff" stroke-width="10" stroke-linecap="round"/>
  <line x1="1024" y1="784" x2="1024" y2="1100" stroke="#ffffff" stroke-width="3" stroke-linecap="round"/>

  <!-- Abdominal Muscle Plate Seams -->
  <path d="M 960,860 Q 1024,885 1088,860" fill="none" stroke="#2c3345" stroke-width="6"/>
  <path d="M 955,950 Q 1024,980 1093,950" fill="none" stroke="#2c3345" stroke-width="6"/>
  <path d="M 950,1040 Q 1024,1070 1098,1040" fill="none" stroke="#2c3345" stroke-width="6"/>

  <!-- Tactical Harness Belt at Bottom (y: 1130..1190) -->
  <rect x="880" y="1130" width="288" height="55" fill="#14161f"/>
  <line x1="880" y1="1130" x2="1168" y2="1130" stroke="#3a4256" stroke-width="4"/>
  <line x1="880" y1="1185" x2="1168" y2="1185" stroke="#3a4256" stroke-width="4"/>
  <!-- Metallic Buckle -->
  <rect x="990" y="1125" width="68" height="65" rx="8" fill="#3a4256" stroke="#00e5ff" stroke-width="5"/>
  <rect x="1006" y="1140" width="36" height="35" rx="4" fill="#14161f"/>
  <circle cx="1024" cy="1157" r="6" fill="#00e5ff"/>

  <!-- Lab Coat Breast Pocket with Holographic ID Comm (x: 815..875, y: 650..740) -->
  <rect x="815" y="650" width="60" height="90" rx="4" fill="#e8edf5" stroke="#b0bece" stroke-width="4"/>
  <line x1="815" y1="675" x2="875" y2="675" stroke="#00e5ff" stroke-width="4"/>
  <rect x="828" y="625" width="10" height="35" rx="2" fill="#00e5ff"/>
  <rect x="846" y="630" width="10" height="30" rx="2" fill="#3a4256"/>

</svg>'''
    apply_texture(svg,
        os.path.join(BASE_DIR, "Assets/Models/Masc/textures/M00_008_03_Tops_01.png"),
        2048, 2048,
        [
            "Assets/Models/Masc/textures/M00_008_03_Tops_01.png",
            "Assets/StreamingAssets/Models/Masc/textures/M00_008_03_Tops_01.png",
            "Assets/Resources/Models/Masc/textures/M00_008_03_Tops_01.png"
        ]
    )

def build_male_bottoms():
    svg = '''<svg width="1024" height="1024" xmlns="http://www.w3.org/2000/svg">
  <!-- Base Tactical Fabric -->
  <rect x="0" y="0" width="1024" height="1024" fill="#242834"/>

  <!-- Top Tactical Waistband (y: 0..90) -->
  <rect x="0" y="0" width="1024" height="90" fill="#141720"/>
  <line x1="0" y1="88" x2="1024" y2="88" stroke="#384054" stroke-width="4"/>

  <!-- ==================== LEFT LEG (x: 12..500) ==================== -->
  <!-- Outer Lateral Cyan Conduit Stripe (Runs vertically from hip to ankle) -->
  <line x1="72" y1="90" x2="72" y2="860" stroke="#00e5ff" stroke-width="8" stroke-linecap="round"/>
  <line x1="72" y1="90" x2="72" y2="860" stroke="#ffffff" stroke-width="2" stroke-linecap="round"/>
  <circle cx="72" cy="300" r="10" fill="#00e5ff"/>
  <circle cx="72" cy="620" r="10" fill="#00e5ff"/>

  <!-- Thigh Tactical Holster Straps (y: 280..340) -->
  <rect x="50" y="295" width="410" height="30" fill="#141620" stroke="#363e52" stroke-width="3"/>
  <rect x="240" y="288" width="45" height="44" rx="6" fill="#363e52" stroke="#00e5ff" stroke-width="3"/>

  <!-- Thigh Pocket with Cyan Tab -->
  <rect x="210" y="345" width="150" height="110" rx="4" fill="#1e222e" stroke="#363e52" stroke-width="3"/>
  <rect x="265" y="340" width="40" height="12" rx="2" fill="#00e5ff"/>

  <!-- Hexagonal Contoured Knee Armor Plate (y: 490..660, x: 190..370) -->
  <polygon points="280,490 360,520 360,620 280,660 200,620 200,520" fill="#14161f" stroke="#00e5ff" stroke-width="5"/>
  <polygon points="280,510 340,535 340,605 280,640 220,605 220,535" fill="none" stroke="#55ffff" stroke-width="2"/>
  <circle cx="280" cy="575" r="12" fill="#00e5ff"/>
  <circle cx="280" cy="575" r="6" fill="#ffffff"/>

  <!-- Shin Protective Ribs -->
  <line x1="220" y1="720" x2="340" y2="720" stroke="#384054" stroke-width="6"/>
  <line x1="220" y1="760" x2="340" y2="760" stroke="#384054" stroke-width="6"/>
  <line x1="220" y1="800" x2="340" y2="800" stroke="#384054" stroke-width="6"/>

  <!-- ==================== RIGHT LEG (x: 512..1012) ==================== -->
  <!-- Outer Lateral Cyan Conduit Stripe -->
  <line x1="952" y1="90" x2="952" y2="860" stroke="#00e5ff" stroke-width="8" stroke-linecap="round"/>
  <line x1="952" y1="90" x2="952" y2="860" stroke="#ffffff" stroke-width="2" stroke-linecap="round"/>
  <circle cx="952" cy="300" r="10" fill="#00e5ff"/>
  <circle cx="952" cy="620" r="10" fill="#00e5ff"/>

  <!-- Right Thigh Strap -->
  <rect x="560" y="295" width="410" height="30" fill="#141620" stroke="#363e52" stroke-width="3"/>
  <rect x="740" y="288" width="45" height="44" rx="6" fill="#363e52" stroke="#00e5ff" stroke-width="3"/>

  <!-- Right Thigh Pocket with Cyan Tab -->
  <rect x="660" y="345" width="150" height="110" rx="4" fill="#1e222e" stroke="#363e52" stroke-width="3"/>
  <rect x="715" y="340" width="40" height="12" rx="2" fill="#00e5ff"/>

  <!-- Right Hexagonal Knee Armor Plate (y: 490..660, x: 650..830) -->
  <polygon points="740,490 820,520 820,620 740,660 660,620 660,520" fill="#14161f" stroke="#00e5ff" stroke-width="5"/>
  <polygon points="740,510 800,535 800,605 740,640 680,605 680,535" fill="none" stroke="#55ffff" stroke-width="2"/>
  <circle cx="740" cy="575" r="12" fill="#00e5ff"/>
  <circle cx="740" cy="575" r="6" fill="#ffffff"/>

  <!-- Right Shin Protective Ribs -->
  <line x1="680" y1="720" x2="800" y2="720" stroke="#384054" stroke-width="6"/>
  <line x1="680" y1="760" x2="800" y2="760" stroke="#384054" stroke-width="6"/>
  <line x1="680" y1="800" x2="800" y2="800" stroke="#384054" stroke-width="6"/>
</svg>'''
    apply_texture(svg,
        os.path.join(BASE_DIR, "Assets/Models/Masc/textures/M00_008_01_Bottoms_01.png"),
        1024, 1024,
        [
            "Assets/Models/Masc/textures/M00_008_01_Bottoms_01.png",
            "Assets/StreamingAssets/Models/Masc/textures/M00_008_01_Bottoms_01.png",
            "Assets/Resources/Models/Masc/textures/M00_008_01_Bottoms_01.png"
        ]
    )

def build_male_shoes():
    svg = '''<svg width="512" height="512" xmlns="http://www.w3.org/2000/svg">
  <!-- Dark Combat Boot Base -->
  <rect x="0" y="0" width="512" height="512" fill="#181b24"/>
  <!-- White High-Tech Shin Armor Panels -->
  <rect x="40" y="30" width="200" height="200" rx="12" fill="#f0f4fa" stroke="#c0ccdc" stroke-width="4"/>
  <rect x="272" y="30" width="200" height="200" rx="12" fill="#f0f4fa" stroke="#c0ccdc" stroke-width="4"/>
  <!-- Glowing Cyan Ankle Straps -->
  <line x1="20" y1="120" x2="240" y2="120" stroke="#00e5ff" stroke-width="8"/>
  <line x1="272" y1="120" x2="492" y2="120" stroke="#00e5ff" stroke-width="8"/>
  <!-- Sole Tread & Cyan Glow Trim -->
  <rect x="20" y="380" width="220" height="80" rx="8" fill="#101218" stroke="#00e5ff" stroke-width="6"/>
  <rect x="272" y="380" width="220" height="80" rx="8" fill="#101218" stroke="#00e5ff" stroke-width="6"/>
  <line x1="20" y1="440" x2="240" y2="440" stroke="#00e5ff" stroke-width="6"/>
  <line x1="272" y1="440" x2="492" y2="440" stroke="#00e5ff" stroke-width="6"/>
</svg>'''
    apply_texture(svg,
        os.path.join(BASE_DIR, "Assets/Models/Masc/textures/M00_007_01_Shoes_01.png"),
        512, 512,
        [
            "Assets/Models/Masc/textures/M00_007_01_Shoes_01.png",
            "Assets/StreamingAssets/Models/Masc/textures/M00_007_01_Shoes_01.png",
            "Assets/Resources/Models/Masc/textures/M00_007_01_Shoes_01.png"
        ]
    )

def build_female_tops():
    svg = '''<svg width="2048" height="2048" xmlns="http://www.w3.org/2000/svg">
  <rect x="0" y="0" width="2048" height="2048" fill="#f0f3f8"/>

  <!-- Sleeves (x: 0..700 and 1300..2048, y: 0..900) -->
  <line x1="350" y1="50" x2="350" y2="700" stroke="#00e5ff" stroke-width="12"/>
  <line x1="350" y1="50" x2="350" y2="700" stroke="#ffffff" stroke-width="4"/>
  <circle cx="350" cy="300" r="14" fill="#00e5ff"/>

  <line x1="1700" y1="50" x2="1700" y2="700" stroke="#00e5ff" stroke-width="12"/>
  <line x1="1700" y1="50" x2="1700" y2="700" stroke="#ffffff" stroke-width="4"/>
  <circle cx="1700" cy="300" r="14" fill="#00e5ff"/>

  <!-- Torso (y: 900..1900) -->
  <polygon points="900,950 1148,950 1120,1850 928,1850" fill="#1c202c"/>
  <line x1="900" y1="950" x2="928" y2="1850" stroke="#00e5ff" stroke-width="10"/>
  <line x1="1148" y1="950" x2="1120" y2="1850" stroke="#00e5ff" stroke-width="10"/>

  <!-- Glowing Core -->
  <circle cx="1024" cy="1300" r="42" fill="#141822" stroke="#00e5ff" stroke-width="7"/>
  <circle cx="1024" cy="1300" r="22" fill="#00e5ff"/>
  <circle cx="1024" cy="1300" r="9" fill="#ffffff"/>

  <!-- Conduits -->
  <line x1="1024" y1="1342" x2="1024" y2="1750" stroke="#00e5ff" stroke-width="8"/>
</svg>'''
    apply_texture(svg,
        os.path.join(BASE_DIR, "Assets/Models/Fem/textures/F00_006_01_Tops_01.png"),
        2048, 2048,
        [
            "Assets/Models/Fem/textures/F00_006_01_Tops_01.png",
            "Assets/StreamingAssets/Models/Fem/textures/F00_006_01_Tops_01.png",
            "Assets/Resources/Models/Fem/textures/F00_006_01_Tops_01.png"
        ]
    )

def build_female_bottoms():
    svg = '''<svg width="2048" height="2048" xmlns="http://www.w3.org/2000/svg">
  <rect x="0" y="0" width="2048" height="2048" fill="#1c202c"/>
  <!-- Top Belt -->
  <rect x="0" y="80" width="2048" height="60" fill="#12141c" stroke="#384054" stroke-width="4"/>
  <!-- Cyber Piping Hem -->
  <line x1="0" y1="600" x2="2048" y2="600" stroke="#00e5ff" stroke-width="12"/>
  <line x1="0" y1="600" x2="2048" y2="600" stroke="#ffffff" stroke-width="4"/>
</svg>'''
    apply_texture(svg,
        os.path.join(BASE_DIR, "Assets/Models/Fem/textures/F00_008_01_Bottoms_01.png"),
        2048, 2048,
        [
            "Assets/Models/Fem/textures/F00_008_01_Bottoms_01.png",
            "Assets/StreamingAssets/Models/Fem/textures/F00_008_01_Bottoms_01.png",
            "Assets/Resources/Models/Fem/textures/F00_008_01_Bottoms_01.png"
        ]
    )

if __name__ == "__main__":
    build_male_tops()
    build_male_bottoms()
    build_male_shoes()
    build_female_tops()
    build_female_bottoms()
