import struct, json, os, sys, math

def read_accessor_data(meta, bin_data, acc_id):
    acc = meta["accessors"][acc_id]
    bv = meta["bufferViews"][acc["bufferView"]]
    offset = bv.get("byteOffset", 0) + acc.get("byteOffset", 0)
    count = acc["count"]
    comp_type = acc["componentType"]
    acc_type = acc["type"]

    type_map = {"SCALAR": 1, "VEC2": 2, "VEC3": 3, "VEC4": 4}
    nc = type_map[acc_type]

    if comp_type == 5126: # FLOAT
        fmt = f"<{nc}f"
        stride = bv.get("byteStride", 4 * nc)
    elif comp_type == 5123: # USHORT
        fmt = f"<{nc}H"
        stride = bv.get("byteStride", 2 * nc)
    elif comp_type == 5125: # UINT
        fmt = f"<{nc}I"
        stride = bv.get("byteStride", 4 * nc)
    elif comp_type == 5121: # UBYTE
        fmt = f"<{nc}B"
        stride = bv.get("byteStride", 1 * nc)
    else:
        raise ValueError(f"Unsupported component type: {comp_type}")

    items = []
    for i in range(count):
        val = struct.unpack_from(fmt, bin_data, offset + i * stride)
        items.append(val if nc > 1 else val[0])
    return items

def rot_z(rad):
    c, s = math.cos(rad), math.sin(rad)
    return [[c, -s, 0], [s, c, 0], [0, 0, 1]]

def rot_y(rad):
    c, s = math.cos(rad), math.sin(rad)
    return [[c, 0, s], [0, 1, 0], [-s, 0, c]]

def mat_mul3(A, B):
    C = [[0.0]*3 for _ in range(3)]
    for i in range(3):
        for j in range(3):
            for k in range(3):
                C[i][j] += A[i][k] * B[k][j]
    return C

def apply3(M, v):
    return [
        M[0][0]*v[0] + M[0][1]*v[1] + M[0][2]*v[2],
        M[1][0]*v[0] + M[1][1]*v[1] + M[1][2]*v[2],
        M[2][0]*v[0] + M[2][1]*v[1] + M[2][2]*v[2]
    ]

def extract_vrm_model(vrm_path, out_dir, model_name, is_male=True):
    os.makedirs(out_dir, exist_ok=True)
    tex_dir = os.path.join(out_dir, "textures")
    os.makedirs(tex_dir, exist_ok=True)

    with open(vrm_path, "rb") as f:
        magic, version, length = struct.unpack("<4sII", f.read(12))
        assert magic == b"glTF", f"Not glTF: {magic}"
        c0_len, _ = struct.unpack("<II", f.read(8))
        meta = json.loads(f.read(c0_len).decode("utf-8"))
        c1_len, _ = struct.unpack("<II", f.read(8))
        bin_data = f.read(c1_len)

    bvs = meta["bufferViews"]
    materials = meta.get("materials", [])
    textures = meta.get("textures", [])
    images = meta.get("images", [])

    # 1. Extract textures
    image_filenames = {}
    for i, img in enumerate(images):
        bv = bvs[img["bufferView"]]
        data = bin_data[bv["byteOffset"] : bv["byteOffset"] + bv["byteLength"]]
        ext = "png" if img.get("mimeType") == "image/png" else "jpg"
        raw_name = img.get("name", f"tex_{i}")
        clean_name = raw_name.replace(" ", "_").replace("(", "").replace(")", "")
        filename = f"{clean_name}.{ext}"
        filepath = os.path.join(tex_dir, filename)
        with open(filepath, "wb") as out_f:
            out_f.write(data)
        image_filenames[i] = filename

    # 2. Extract MTL
    mtl_path = os.path.join(out_dir, f"{model_name}.mtl")
    mat_names = []
    with open(mtl_path, "w", encoding="utf-8") as mtl_f:
        for idx, mat in enumerate(materials):
            raw_name = mat.get("name", f"mat_{idx}")
            clean_name = raw_name.replace(" ", "_").replace("(", "").replace(")", "")
            mat_names.append(clean_name)

            mtl_f.write(f"newmtl {clean_name}\n")
            mtl_f.write("Ka 1.0 1.0 1.0\n")
            mtl_f.write("Kd 1.0 1.0 1.0\n")
            mtl_f.write("Ks 0.1 0.1 0.1\n")
            mtl_f.write("d 1.0\n")
            mtl_f.write("illum 2\n")

            pbr = mat.get("pbrMetallicRoughness", {})
            if "baseColorTexture" in pbr:
                t_idx = pbr["baseColorTexture"]["index"]
                img_idx = textures[t_idx]["source"]
                tex_file = image_filenames.get(img_idx)
                if tex_file:
                    mtl_f.write(f"map_Kd textures/{tex_file}\n")
            mtl_f.write("\n")

    # Joint index setup for posing arms from T-pose to relaxed standing A-pose
    # Male: L=[30..52], R=[54..76]. Female: L=[46..68], R=[70..92]
    l_upper_joint = 30 if is_male else 46
    l_lower_joint = 31 if is_male else 47
    l_arm_joints = set(range(30, 53)) if is_male else set(range(46, 69))

    r_upper_joint = 54 if is_male else 70
    r_lower_joint = 55 if is_male else 71
    r_arm_joints = set(range(54, 77)) if is_male else set(range(70, 93))

    # Kinematic bone pivots
    pu_l = [-0.128, 1.404, -0.004] if is_male else [-0.115, 1.285, -0.003]
    pe_l = [-0.348, 1.394, -0.005] if is_male else [-0.312, 1.275, -0.004]

    pu_r = [0.129, 1.404, -0.004] if is_male else [0.116, 1.285, -0.003]
    pe_r = [0.349, 1.394, -0.005] if is_male else [0.313, 1.275, -0.004]

    # Arm rotations: rotate down ~64 deg, bend forward ~14 deg
    R_u_L = mat_mul3(rot_z(math.radians(64.0)), rot_y(math.radians(10.0)))
    R_e_L = mat_mul3(rot_z(math.radians(6.0)), rot_y(math.radians(16.0)))
    R_tot_L = mat_mul3(R_u_L, R_e_L)

    pe_l_posed = [pu_l[i] + apply3(R_u_L, [pe_l[j] - pu_l[j] for j in range(3)])[i] for i in range(3)]

    R_u_R = mat_mul3(rot_z(math.radians(-64.0)), rot_y(math.radians(-10.0)))
    R_e_R = mat_mul3(rot_z(math.radians(-6.0)), rot_y(math.radians(-16.0)))
    R_tot_R = mat_mul3(R_u_R, R_e_R)

    pe_r_posed = [pu_r[i] + apply3(R_u_R, [pe_r[j] - pu_r[j] for j in range(3)])[i] for i in range(3)]

    # Leg joint setup for natural athletic stance (separating feet so pants and boots don't touch/clip)
    l_leg_joints = set([77, 78, 86, 87, 88]) if is_male else set([93, 94, 95, 96, 97])
    r_leg_joints = set([89, 90, 98, 99, 100]) if is_male else set([98, 99, 100, 101, 102])

    p_hip_l = [-0.0843, 0.9744, -0.0060] if is_male else [-0.0691, 0.8607, -0.0012]
    p_hip_r = [0.0854, 0.9744, -0.0060] if is_male else [0.0692, 0.8607, -0.0012]

    R_leg_L = rot_z(math.radians(-2.6))
    R_leg_R = rot_z(math.radians(2.6))

    # 3. Extract OBJ
    obj_path = os.path.join(out_dir, f"{model_name}.obj")
    print(f"Exporting posed {model_name} to {obj_path}...")

    vert_offset = 1
    total_prims = 0
    total_exported_verts = 0
    total_exported_faces = 0

    with open(obj_path, "w", encoding="utf-8") as obj_f:
        obj_f.write(f"# Exported Posed 3D Anime Model: {model_name}\n")
        obj_f.write(f"mtllib {model_name}.mtl\n\n")

        for m_idx, mesh in enumerate(meta.get("meshes", [])):
            mesh_name = mesh.get("name", f"mesh_{m_idx}").replace(" ", "_")
            obj_f.write(f"o {mesh_name}\n")

            is_body_mesh = "Body" in mesh_name

            for p_idx, prim in enumerate(mesh.get("primitives", [])):
                total_prims += 1
                attrs = prim.get("attributes", {})

                pos_all = read_accessor_data(meta, bin_data, attrs["POSITION"])
                norm_all = read_accessor_data(meta, bin_data, attrs["NORMAL"]) if "NORMAL" in attrs else []
                uv_all = read_accessor_data(meta, bin_data, attrs["TEXCOORD_0"]) if "TEXCOORD_0" in attrs else []
                indices_all = read_accessor_data(meta, bin_data, prim["indices"])

                joints_all = read_accessor_data(meta, bin_data, attrs["JOINTS_0"]) if (is_body_mesh and "JOINTS_0" in attrs) else []
                weights_all = read_accessor_data(meta, bin_data, attrs["WEIGHTS_0"]) if (is_body_mesh and "WEIGHTS_0" in attrs) else []

                used_verts = sorted(list(set(indices_all)))
                v_map = {old: new for new, old in enumerate(used_verts)}

                mat_id = prim.get("material")
                if mat_id is not None and mat_id < len(mat_names):
                    obj_f.write(f"usemtl {mat_names[mat_id]}\n")

                # Write vertex positions (apply arm pose skinning if body)
                for vi in used_verts:
                    p = list(pos_all[vi])

                    if joints_all and weights_all:
                        j_list = joints_all[vi]
                        w_list = weights_all[vi]

                        w_upper_l = sum(w for j, w in zip(j_list, w_list) if j == l_upper_joint)
                        w_lower_l = sum(w for j, w in zip(j_list, w_list) if j in l_arm_joints and j != l_upper_joint)

                        w_upper_r = sum(w for j, w in zip(j_list, w_list) if j == r_upper_joint)
                        w_lower_r = sum(w for j, w in zip(j_list, w_list) if j in r_arm_joints and j != r_upper_joint)

                        w_l_tot = w_upper_l + w_lower_l
                        w_r_tot = w_upper_r + w_lower_r

                        if w_l_tot > 0.001:
                            v_u = [pu_l[i] + apply3(R_u_L, [p[j] - pu_l[j] for j in range(3)])[i] for i in range(3)]
                            v_l = [pe_l_posed[i] + apply3(R_tot_L, [p[j] - pe_l[j] for j in range(3)])[i] for i in range(3)]
                            p_arm = [w_upper_l * v_u[i] + w_lower_l * v_l[i] for i in range(3)]
                            p = [(1.0 - w_l_tot) * p[i] + p_arm[i] for i in range(3)]

                        elif w_r_tot > 0.001:
                            v_u = [pu_r[i] + apply3(R_u_R, [p[j] - pu_r[j] for j in range(3)])[i] for i in range(3)]
                            v_l = [pe_r_posed[i] + apply3(R_tot_R, [p[j] - pe_r[j] for j in range(3)])[i] for i in range(3)]
                            p_arm = [w_upper_r * v_u[i] + w_lower_r * v_l[i] for i in range(3)]
                            p = [(1.0 - w_r_tot) * p[i] + p_arm[i] for i in range(3)]

                        w_leg_l = sum(w for j, w in zip(j_list, w_list) if j in l_leg_joints)
                        w_leg_r = sum(w for j, w in zip(j_list, w_list) if j in r_leg_joints)

                        if w_leg_l > 0.001:
                            v_leg = [p_hip_l[i] + apply3(R_leg_L, [p[j] - p_hip_l[j] for j in range(3)])[i] for i in range(3)]
                            p = [(1.0 - w_leg_l) * p[i] + w_leg_l * v_leg[i] for i in range(3)]
                        elif w_leg_r > 0.001:
                            v_leg = [p_hip_r[i] + apply3(R_leg_R, [p[j] - p_hip_r[j] for j in range(3)])[i] for i in range(3)]
                            p = [(1.0 - w_leg_r) * p[i] + w_leg_r * v_leg[i] for i in range(3)]

                    # Flip Z for Unity coordinate system
                    obj_f.write(f"v {p[0]:.6f} {p[1]:.6f} {-p[2]:.6f}\n")

                # Write UVs
                if uv_all:
                    for vi in used_verts:
                        uv = uv_all[vi]
                        obj_f.write(f"vt {uv[0]:.6f} {1.0 - uv[1]:.6f}\n")

                # Write Normals
                if norm_all:
                    for vi in used_verts:
                        n = list(norm_all[vi])
                        if joints_all and weights_all:
                            j_list = joints_all[vi]
                            w_list = weights_all[vi]
                            w_upper_l = sum(w for j, w in zip(j_list, w_list) if j == l_upper_joint)
                            w_lower_l = sum(w for j, w in zip(j_list, w_list) if j in l_arm_joints and j != l_upper_joint)
                            w_upper_r = sum(w for j, w in zip(j_list, w_list) if j == r_upper_joint)
                            w_lower_r = sum(w for j, w in zip(j_list, w_list) if j in r_arm_joints and j != r_upper_joint)
                            w_leg_l = sum(w for j, w in zip(j_list, w_list) if j in l_leg_joints)
                            w_leg_r = sum(w for j, w in zip(j_list, w_list) if j in r_leg_joints)

                            if w_upper_l + w_lower_l > 0.5:
                                n = apply3(R_tot_L if w_lower_l > w_upper_l else R_u_L, n)
                            elif w_upper_r + w_lower_r > 0.5:
                                n = apply3(R_tot_R if w_lower_r > w_upper_r else R_u_R, n)
                            elif w_leg_l > 0.5:
                                n = apply3(R_leg_L, n)
                            elif w_leg_r > 0.5:
                                n = apply3(R_leg_R, n)
                        obj_f.write(f"vn {n[0]:.6f} {n[1]:.6f} {-n[2]:.6f}\n")

                has_uv = bool(uv_all)
                has_norm = bool(norm_all)

                for i in range(0, len(indices_all), 3):
                    i0 = v_map[indices_all[i]]
                    i1 = v_map[indices_all[i+1]]
                    i2 = v_map[indices_all[i+2]]

                    v0 = vert_offset + i0
                    v1 = vert_offset + i1
                    v2 = vert_offset + i2

                    if has_uv and has_norm:
                        obj_f.write(f"f {v0}/{v0}/{v0} {v2}/{v2}/{v2} {v1}/{v1}/{v1}\n")
                    elif has_uv:
                        obj_f.write(f"f {v0}/{v0} {v2}/{v2} {v1}/{v1}\n")
                    else:
                        obj_f.write(f"f {v0} {v2} {v1}\n")

                    total_exported_faces += 1

                vert_offset += len(used_verts)
                total_exported_verts += len(used_verts)

    print(f"Done: {model_name} -> {total_exported_verts} verts, {total_exported_faces} faces.")

if __name__ == "__main__":
    base_dir = "/Users/pjw/.gemini/antigravity/scratch/BioCleanroomLab"
    extract_vrm_model(
        os.path.join(base_dir, "Assets/Models/RawVRM/AvatarSample_C.vrm"),
        os.path.join(base_dir, "Assets/Models/Masc"),
        "male_researcher",
        is_male=True
    )
    extract_vrm_model(
        os.path.join(base_dir, "Assets/Models/RawVRM/AvatarSample_A.vrm"),
        os.path.join(base_dir, "Assets/Models/Fem"),
        "female_researcher",
        is_male=False
    )
