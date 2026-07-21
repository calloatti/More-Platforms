import bpy
import os
import sys

sys.path.append(r"C:\Users\calloatti\source\repos\Tools\timbermesh")
sys.path.append(r"C:\Users\calloatti\source\repos\Tools\timbermesh\timbermesh_blender_plugin")
from timbermesh_blender_plugin import timbermesh_exporter

BLEND_DIR = r"C:\Users\calloatti\source\repos\Mods\More Platforms\Version-1.0\Models"
OUT_DIR = r"C:\Users\calloatti\source\repos\Mods\More Platforms\Version-1.0\Buildings\SidePlatforms"

FACTIONS = ["Folktails", "IronTeeth"]

FACTION_INDEPENDENT = {
    "ConstructionSite1x1.5",
}

EXPORT_STEMS = [
    "ConstructionSite1x1.5",
    "DoubleReinforcedPlatform",
    "HorizontalPlatformEnd1x1",
    "HorizontalPlatformEnd2x1",
    "HorizontalPlatformEnd3x1",
    "HorizontalPlatformEnd4x1",
    "HorizontalPlatformEnd5x1",
    "ReinforcedPlatform",
    "TripleReinforcedPlatform",
]

SWAP_MAP = {
    "Folktails": {
        "BaseMetal.IronTeeth":        "BaseMetal.Folktails",
        "BaseWood_DarkBrown.IronTeeth": "BaseWood_Brown.Folktails",
        "BaseWood_Grey.IronTeeth":    "BaseWood_White.Folktails",
        "Details.IronTeeth":          "Details.Folktails",
        "PaintedMetal.IronTeeth":     "PaintedMetal.Folktails",
    },
    "IronTeeth": {
        "BaseMetal.Folktails":        "BaseMetal.IronTeeth",
        "BaseWood_Brown.Folktails":   "BaseWood_DarkBrown.IronTeeth",
        "BaseWood_White.Folktails":   "BaseWood_Grey.IronTeeth",
        "Details.Folktails":          "Details.IronTeeth",
        "PaintedMetal.Folktails":     "PaintedMetal.IronTeeth",
    },
}

def get_material_usage(mat_name):
    used = []
    for obj in bpy.data.objects:
        if obj.type == 'MESH':
            for i, slot in enumerate(obj.material_slots):
                if slot.material and slot.material.name == mat_name:
                    used.append((obj.name, i))
    return used

def rename_or_merge(src_name, target_name):
    if src_name == target_name:
        return
    src = bpy.data.materials.get(src_name)
    target = bpy.data.materials.get(target_name)
    if src is None:
        return
    if target is None:
        src.name = target_name
    else:
        usage = get_material_usage(src_name)
        for obj_name, slot_idx in usage:
            obj = bpy.data.objects.get(obj_name)
            if obj and len(obj.material_slots) > slot_idx:
                obj.material_slots[slot_idx].material = target
        bpy.data.materials.remove(src)
        print(f"    Merged '{src_name}' -> '{target_name}' ({len(usage)} slots)")

def apply_faction_swap(faction):
    swap = SWAP_MAP.get(faction, {})
    for src, dst in swap.items():
        rename_or_merge(src, dst)

def export_stem(stem, settings):
    if stem in FACTION_INDEPENDENT:
        name = f"{stem}.Model"
        out = os.path.join(OUT_DIR, f"{name}.timbermesh")
        for c in bpy.data.collections:
            timbermesh_exporter.Exporter.export_collection(c, out, settings)
        print(f"  -> {name} (faction-independent)")
    else:
        for faction in FACTIONS:
            apply_faction_swap(faction)
            name = f"{stem}.{faction}.Model"
            out = os.path.join(OUT_DIR, f"{name}.timbermesh")
            for c in bpy.data.collections:
                timbermesh_exporter.Exporter.export_collection(c, out, settings)
            print(f"  -> {name}")

def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    settings = timbermesh_exporter.ExportSettings(bpy.context, merge_meshes=True, single_animation=True, use_vertex_animations=False)

    for fname in sorted(os.listdir(BLEND_DIR)):
        if not fname.endswith(".blend"):
            continue
        stem = fname[:-6]
        if stem not in EXPORT_STEMS:
            continue
        print(f"\n=== {stem} ===")
        bpy.ops.wm.open_mainfile(filepath=os.path.join(BLEND_DIR, fname))
        export_stem(stem, settings)
        # Close without saving — next blend starts fresh

    print("\nDone")

if __name__ == "__main__":
    main()
