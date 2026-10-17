import struct, json, os, sys
from collections import defaultdict

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

def export_rigged_model(vrm_path, out_bin_path):
    with open(vrm_path, "rb") as f:
        magic, version, length = struct.unpack("<4sII", f.read(12))
        assert magic == b"glTF", f"Not glTF: {magic}"
        c0_len, _ = struct.unpack("<II", f.read(8))
        meta = json.loads(f.read(c0_len).decode("utf-8"))
        c1_len, _ = struct.unpack("<II", f.read(8))
        bin_data = f.read(c1_len)

    nodes = meta["nodes"]
    skin = meta["skins"][0]
    joints = skin["joints"] # list of node indices
    materials = meta.get("materials", [])
    texs = meta.get("textures", [])
    imgs = meta.get("images", [])

    mat_names = []
    mat_tex_names = []
    for i, m in enumerate(materials):
        m_name = m.get("name", f"mat_{i}")
        mat_names.append(m_name)
        pbr = m.get("pbrMetallicRoughness", {})
        bc = pbr.get("baseColorTexture", {})
        t_idx = bc.get("index")
        img_name = ""
        if t_idx is not None and t_idx < len(texs):
            source = texs[t_idx].get("source")
            if source is not None and source < len(imgs):
                img_name = imgs[source].get("name", f"img_{source}")
        if img_name and not img_name.endswith(".png"):
            img_name += ".png"
        mat_tex_names.append(img_name)

    # Parent map
    parent_map = {}
    for p_idx, n in enumerate(nodes):
        for c_idx in n.get("children", []):
            parent_map[c_idx] = p_idx

    os.makedirs(os.path.dirname(out_bin_path), exist_ok=True)
    with open(out_bin_path, "wb") as out:
        out.write(b"RIGD")
        out.write(struct.pack("<I", 2)) # Version 2: includes texture filename
        
        # 1. Bones
        bone_count = len(joints)
        out.write(struct.pack("<I", bone_count))
        node_to_bone = {node_idx: bone_idx for bone_idx, node_idx in enumerate(joints)}

        for b_idx, node_idx in enumerate(joints):
            n = nodes[node_idx]
            name = n.get("name", f"joint_{b_idx}").encode("utf-8")
            p_node = parent_map.get(node_idx, -1)
            p_bone = node_to_bone.get(p_node, -1)

            t = n.get("translation", [0.0, 0.0, 0.0])
            r = n.get("rotation", [0.0, 0.0, 0.0, 1.0])

            # In Unity: x, y, -z
            pos_unity = (float(t[0]), float(t[1]), -float(t[2]))
            rot_unity = (float(r[0]), float(r[1]), float(r[2]), float(r[3]))

            out.write(struct.pack("<B", len(name)))
            out.write(name)
            out.write(struct.pack("<i", p_bone))
            out.write(struct.pack("<3f", *pos_unity))
            out.write(struct.pack("<4f", *rot_unity))

        # 2. Meshes
        meshes = meta.get("meshes", [])
        out.write(struct.pack("<I", len(meshes)))

        for m_idx, mesh in enumerate(meshes):
            mesh_name = mesh.get("name", f"mesh_{m_idx}").encode("utf-8")
            out.write(struct.pack("<B", len(mesh_name)))
            out.write(mesh_name)

            prims = mesh.get("primitives", [])
            prim0_attrs = prims[0].get("attributes", {})
            pos_all = read_accessor_data(meta, bin_data, prim0_attrs["POSITION"])
            norm_all = read_accessor_data(meta, bin_data, prim0_attrs["NORMAL"]) if "NORMAL" in prim0_attrs else []
            uv_all = read_accessor_data(meta, bin_data, prim0_attrs["TEXCOORD_0"]) if "TEXCOORD_0" in prim0_attrs else []
            joints_all = read_accessor_data(meta, bin_data, prim0_attrs["JOINTS_0"]) if "JOINTS_0" in prim0_attrs else []
            weights_all = read_accessor_data(meta, bin_data, prim0_attrs["WEIGHTS_0"]) if "WEIGHTS_0" in prim0_attrs else []

            v_count = len(pos_all)
            out.write(struct.pack("<I", v_count))

            for i in range(v_count):
                p = pos_all[i]
                n = norm_all[i] if norm_all else (0.0, 1.0, 0.0)
                uv = uv_all[i] if uv_all else (0.0, 0.0)
                j = joints_all[i] if joints_all else (0, 0, 0, 0)
                w = weights_all[i] if weights_all else (1.0, 0.0, 0.0, 0.0)

                out.write(struct.pack("<3f", p[0], p[1], -p[2]))
                out.write(struct.pack("<3f", n[0], n[1], -n[2]))
                out.write(struct.pack("<2f", uv[0], 1.0 - uv[1]))
                out.write(struct.pack("<4H", int(j[0]), int(j[1]), int(j[2]), int(j[3])))
                out.write(struct.pack("<4f", float(w[0]), float(w[1]), float(w[2]), float(w[3])))

            # Group triangles by material ID
            mat_to_tris = defaultdict(list)
            mat_order = []

            for p in prims:
                mat_id = p.get("material", 0)
                if mat_id not in mat_to_tris:
                    mat_order.append(mat_id)
                indices = read_accessor_data(meta, bin_data, p["indices"])
                m_name = mat_names[mat_id] if mat_id < len(mat_names) else ""
                # Reverse winding: (i0, i2, i1)
                for k in range(0, len(indices), 3):
                    i0, i1, i2 = indices[k], indices[k+1], indices[k+2]
                    # Female miniskirt geometry cleanup: VRoid skirt template extends to floor (Y=-0.05)
                    # but miniskirt texture alpha is only drawn for V <= 0.30; strip out the transparent drape triangles.
                    if "F00_008_01_Bottoms" in m_name:
                        v0 = uv_all[i0][1]
                        v1 = uv_all[i1][1]
                        v2 = uv_all[i2][1]
                        if max(v0, v1, v2) > 0.32:
                            continue
                    mat_to_tris[mat_id].extend([i0, i2, i1])

            # Write submeshes
            out.write(struct.pack("<I", len(mat_order)))
            for mat_id in mat_order:
                m_name = mat_names[mat_id] if mat_id < len(mat_names) else f"mat_{mat_id}"
                m_tex = mat_tex_names[mat_id] if mat_id < len(mat_tex_names) else ""
                
                m_name_bytes = m_name.encode("utf-8")
                m_tex_bytes = m_tex.encode("utf-8")
                tris = mat_to_tris[mat_id]

                out.write(struct.pack("<B", len(m_name_bytes)))
                out.write(m_name_bytes)
                out.write(struct.pack("<B", len(m_tex_bytes)))
                out.write(m_tex_bytes)
                out.write(struct.pack("<I", len(tris)))
                out.write(struct.pack(f"<{len(tris)}I", *tris))

    print(f"Exported v2 {out_bin_path}: {os.path.getsize(out_bin_path):,} bytes")

export_rigged_model("Assets/Models/RawVRM/AvatarSample_C.vrm", "Assets/StreamingAssets/Models/Masc/male_rigged.bin")
export_rigged_model("Assets/Models/RawVRM/AvatarSample_A.vrm", "Assets/StreamingAssets/Models/Fem/female_rigged.bin")
