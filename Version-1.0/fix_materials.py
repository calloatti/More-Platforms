import bpy
import os
import sys

blend_dir = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "."
errors = []

# Material name mapping: old_name -> new_name
rename_map = {
    "BaseWood_Brown_Folktails": "BaseWood_Brown.Folktails",
    "BaseWood_Brown_Folktails.001": "BaseWood_Brown.Folktails",
    "BaseWood_Brown_IronTeeth": "BaseWood_Brown.IronTeeth",
    "BaseWood_DarkBrown.IronTeeth.001": "BaseWood_DarkBrown.IronTeeth",
    "BaseMetal_IronTeeth": "BaseMetal.IronTeeth",
    "BaseMetal_Folktails": "BaseMetal.Folktails",
}

# ConstructionSite uses wrong materials - remap to match game's pattern
# Vanilla ConstructionStage0 uses BaseWood_Brown.Folktails + Dirt
construction_remap = {
    "BaseWood_Brown": "BaseWood_Brown.Folktails",
    "BaseWood_White": "Dirt",
    "Dots Stroke": None,  # remove unused material
}

for fname in sorted(os.listdir(blend_dir)):
    if not fname.endswith(".blend"):
        continue
    fpath = os.path.join(blend_dir, fname)
    print(f"Processing: {fname}")

    bpy.ops.wm.open_mainfile(filepath=fpath)

    renamed = 0
    for old_name, new_name in rename_map.items():
        if old_name in bpy.data.materials and old_name != new_name:
            mat = bpy.data.materials[old_name]
            if new_name in bpy.data.materials:
                target = bpy.data.materials[new_name]
                for obj in bpy.data.objects:
                    if obj.type == "MESH":
                        for slot in obj.material_slots:
                            if slot.material == mat:
                                slot.material = target
                bpy.data.materials.remove(mat)
                print(f"  Merged '{old_name}' -> '{new_name}'")
            else:
                mat.name = new_name
                print(f"  Renamed '{old_name}' -> '{new_name}'")
            renamed += 1

    # ConstructionSite fixup
    for old_name, new_name in construction_remap.items():
        if old_name in bpy.data.materials:
            mat = bpy.data.materials[old_name]
            if new_name is None:
                # Remove the material - reassign slots to first remaining
                first_mat = None
                for m in bpy.data.materials:
                    if m.name != old_name:
                        first_mat = m
                        break
                for obj in bpy.data.objects:
                    if obj.type == "MESH":
                        for slot in obj.material_slots:
                            if slot.material == mat and first_mat:
                                slot.material = first_mat
                bpy.data.materials.remove(mat)
                print(f"  Removed unused '{old_name}'")
                renamed += 1
            elif old_name != new_name:
                if new_name in bpy.data.materials:
                    target = bpy.data.materials[new_name]
                    for obj in bpy.data.objects:
                        if obj.type == "MESH":
                            for slot in obj.material_slots:
                                if slot.material == mat:
                                    slot.material = target
                    bpy.data.materials.remove(mat)
                    print(f"  Merged '{old_name}' -> '{new_name}'")
                else:
                    mat.name = new_name
                    print(f"  Renamed '{old_name}' -> '{new_name}'")
                renamed += 1

    # Remove .001 duplicates
    mats_to_remove = []
    for mat in bpy.data.materials:
        if mat.name.endswith(".001") and mat.name[:-4] in bpy.data.materials:
            target = bpy.data.materials[mat.name[:-4]]
            for obj in bpy.data.objects:
                if obj.type == "MESH":
                    for slot in obj.material_slots:
                        if slot.material == mat:
                            slot.material = target
            mats_to_remove.append(mat.name)

    for name in mats_to_remove:
        bpy.data.materials.remove(bpy.data.materials[name])
        print(f"  Removed duplicate '{name}'")

    if renamed > 0 or mats_to_remove:
        bpy.ops.wm.save_mainfile(filepath=fpath)
        print(f"  Saved {fname}")
    else:
        print(f"  No changes needed")

print("\nDone!")
