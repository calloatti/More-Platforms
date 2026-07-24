Include ..\AGENTS.md

# More Platforms — Timberborn 1.0 Migration Guide

> **Version context:** This repository (`More Platforms`) is the **active working copy** for migrating Tobbert's More Platforms mod from **Timberborn 0.7** → **Timberborn 1.0**. The original 0.7 source lives in `MorePlatforms` (read-only upstream). All migration work — C# code, blueprints, localization, model exports — goes here.

## Overview

- **Original Mod:** More Platforms by Tobbert
- **Current Version:** 3.0.0 (in-progress migration)
- **Original Game Version:** 0.6.0.0 – 0.7.x
- **Target:** Timberborn 1.0.x
- **Original Source Location (READ-ONLY):** `C:\Users\calloatti\source\repos\Mods\MorePlatforms`
- **New Version Location (WORKING COPY):** `C:\Users\calloatti\source\repos\Mods\More Platforms`

- **Blender:** `C:\Users\calloatti\source\repos\Tools\blender-4.2.22-windows-x64\blender.exe`

**IMPORTANT:** The original `Mods\MorePlatforms` folder is the immutable upstream source. Never edit files there.
**IMPORTANT:** The deployment folder (`Documents\Timberborn\Mods\More Platforms\Version-1.0\`) is managed entirely by the PostBuild step in `CommonModSettings.props`. Never create, delete, or modify any file inside it — not directly, not via scripts, not via tools. If the deploy is stale, rebuild the `.csproj` and let the PostBuild handle it.
**IMPORTANT:** This root `AGENTS.md` is the SINGLE canonical project knowledge file. Do not add new content to `Version-1.0\AGENTS.md` — it's a stale summary only.
**IMPORTANT:** Do NOT create `AGENTS.md` files in subdirectories. Put documentation `.md` files in `\.meta\` (one file per subject). Put scripts, temp files, and work files in `\.scratch\`.

## Project Structure (v3.0.0 Working Copy)

```
Version-1.0/
├── CommonModSettings.props
├── More Platforms.csproj
├── manifest.json
├── export_timbermesh.py       ← Blender batch export script
├── Source/
│   ├── ModStarter.cs              ← IModStarter entry point + Harmony patches
│   ├── Configurator.cs            ← TemplateModule.AddDecorator registrations
│   ├── SidePlatform.cs            ← marker BaseComponent for side platforms
│   ├── SidePlatformSpec.cs        ← empty ComponentSpec record
│   ├── SidePlatformValidator.cs   ← IBlockObjectValidator for placement validation
│   ├── SidePlatformSupportBlocker.cs ← IBlockObjectDeletionBlocker for demolition
│   ├── TerrainPhysicsPostLoaderPatch.cs ← Harmony postfix on ValidateBlockObjects
│   ├── BlockObjectPatch.cs        ← Harmony on BlockObject.IsAlmostValid
│   ├── BlockObjectPreviewPickerPatch.cs ← Harmony on BlockObjectPreviewPicker
│   ├── DeconstructionToolPatch.cs ← Harmony postfix on AddNextBlockObjectToValidate
│   ├── DirectionalConnector.cs
│   ├── DirectionalConnectorSpec.cs
│   ├── OverhangingBuilding.cs
│   ├── OverhangingBuildingSpec.cs
│   └── BuildingModelUpdaterPatch.cs
├── Buildings/SidePlatforms/   ← blueprints + .timbermesh + PNG icons
├── Models/                    ← working copies of .blend files
├── TemplateCollections/
├── ToolGroups/
├── Localizations/
├── CategorySprites/           ← category icon PNGs
├── ../.meta/
│   └── prefab_converter.csv   ← prefab/blueprint GUID conversion ref
└── ../.scratch/                  ← temp scripts, work folders
```

### Original (Pre-Migration) Directory Structure

For reference, the original mod's file layout (under `MorePlatforms/`, read-only):

```
MorePlatforms\
├── manifest.json
├── MorePlatforms.asmdef
├── AssetBundles\Resources\
│   ├── CategorySprites\            # Category icons (png)
│   ├── CustomMaterials\            # Custom materials (mat)
│   ├── Models\                     # 3D Blender source files (READ-ONLY)
│   ├── Prefabs\                    # Unity prefabs
│   │   ├── HorizontalPlatformEnds\ # Side Platform prefabs (1x1-4x1, Folktails/IronTeeth)
│   │   └── HorizontalPlatformMiddle\ # Middle Piece prefabs (1x1, 1x2, Folktails/IronTeeth)
│   └── Sprites\                    # Building icons
├── Data\
│   ├── Localizations\              # 14 locale csvs + enUS.csv
│   ├── Specifications\             # TimberAPI specs (OBSOLETE)
│   │   ├── ToolGroupSpecification.CategoryReinforcedPlatforms.json
│   │   ├── ToolGroupSpecification.CategorySidePlatforms.json
│   │   ├── PrefabGroupSpecification.Buildings.Folktails.json
│   │   ├── PrefabGroupSpecification.Buildings.IronTeeth.json
│   │   └── PrefabGroupSpecification.Buildings.GreedyBuilders.optional.json
│   ├── Thumbnail.png
│   └── workshop_data.json
├── Other\
│   ├── ExampleImages\
│   ├── ModIoFiles\README.md
│   └── UnusedPrefabs\ReinforcedPlatforms\
└── Scripts\
    ├── Plugin.cs                    # ENTIRELY COMMENTED OUT (dead)
    ├── MorePlatformsCore.cs         # ACTIVE
    ├── MorePlatformsConfigurator.cs # ENTIRELY COMMENTED OUT (dead)
    ├── DirectionalConnector.cs      # ACTIVE
    ├── OverhangingBuilding.cs       # ACTIVE
    ├── BlockServicePatch.cs         # ENTIRELY COMMENTED OUT (dead)
    ├── BuildingModelUpdaterPatch.cs # ACTIVE
    ├── WorldBlockPatch.cs           # ACTIVE
    └── ShaderFix.cs                 # ENTIRELY COMMENTED OUT (dead)
```

## Key Paths

- **Source .blend files (READ-ONLY, DO NOT EDIT):** `C:\Users\calloatti\source\repos\Mods\MorePlatforms\AssetBundles\Resources\Models\`
- **Working .blend copies:** `Version-1.0\Models\`
- **Build output:** Deploys to `Documents\Timberborn\Mods\More Platforms\Version-1.0\moreplatforms.dll`

- **Blender:** `C:\Users\calloatti\source\repos\Tools\blender-4.2.22-windows-x64\blender.exe`
- **Timbermesh plugin:** `C:\Users\calloatti\source\repos\Tools\timbermesh\timbermesh_blender_plugin\`
- **Timbermesh tools:** `C:\Users\calloatti\source\repos\Tools\timbermesh\timbermesh.ps1` (decoder), `patch_banner.py`, `bags.py`

## Current State

- **Build:** Succeeds (0 errors, 0 warnings). Csproj has `<Publicize Remove="Timberborn.BlueprintSystem" />` to prevent record inheritance issues.
- **Runtime crash FIXED:** `Material BaseWood_Brown_Folktails.001 not found in repository` was caused by .blend materials using underscore naming (`BaseWood_Brown_Folktails`) with Blender dedup suffixes (`.001`, `.002`, etc.) instead of game's dot notation (`BaseWood_Brown.Folktails`)
- **Fix applied:** `.scratch/fix_materials.py` renamed/merged all materials to correct game names. `export_timbermesh.py` updated to read from `Models/`. All .timbermesh re-exported and deployed.
- **Load-time validation FIXED:** Side platforms attached to other buildings (not terrain) were being deleted on game load by `TerrainPhysicsPostLoader.RemoveBlockObjects()`. The BFS flood fill only propagates horizontally through `UnfinishedGround` blocks, not regular `BlockObject` stackable. Fixed via Harmony postfix `TerrainPhysicsPostLoaderPatch.cs` on `ValidateBlockObjects` that scans each validated building's neighbors for attached side platforms and adds them to `_validBlockObjects`. Required publicizing `Timberborn.TerrainPhysics`.
- **ConstructionSite1x1.5 model REBUILT:** Imported vanilla `ConstructionBase1x1.Model.timbermesh` from ripped assets via `timbermesh_plugin_2026-07-15.py`, deleted Dirt mesh (kept BeaverCarryingModels wood frame), rotated -90° around Y (horizontal → vertical in YZ plane, faces -X), origin at Blender location `(0, 0, 1)`. Game-space bounds: X[0, 0.322] Y[0, 1] Z[0, 1] — narrow scaffold at cliff-facing edge.

## Active C# Source Code — Full Details

### `DirectionalConnector.cs`
- **Namespace:** `MorePlatforms`
- **Base class:** `BaseComponent`
- **Interfaces:** `IModelUpdater`
- **Purpose:** Dynamically shows/hides serialized model child GameObjects (ModelUp/Down/Right/Left/UpDown/LeftRight) based on whether adjacent coordinates are occupied by other blocks or terrain.
- **Triggers:** Via `[OnEvent] OnBlockObjectSetEvent(BlockObjectSetEvent)` — calls `BuildingModelUpdater.UpdateBuildingModelsAt()` for all 4 neighbors.
- **How model updates fire:** `UpdateModel()` is called by the game engine via `IModelUpdater` whenever `BuildingModelUpdater.UpdateBuildingModelsAt()` runs at those coordinates.
- **Injection:** Uses `[Inject]` on `InjectDependencies(BuildingModelUpdater, PreviewBlockService, ITerrainService, BlockService, EventBus)` — OLD pattern.
- **`GetComponentFast<BlockObject>()` in Awake()** — must become `GetComponent<BlockObject>()`.
- **`BlockService`** is `internal` in 1.0 — should inject `IBlockService` instead.

### `OverhangingBuilding.cs`
- **Namespace:** `MorePlatforms`
- **Base class:** `BaseComponent`
- **Fields:** `public int BlockObjectBlockIndex`
- **Purpose:** Marker component for buildings that overhang (occupy a second block). Used by old BlockServicePatch (now dead).
- **No changes needed for 1.0 migration if unused.** If reactivated, `TryGetComponentFast` → `TryGetComponent`.

### `BuildingModelUpdaterPatch.cs`
- **Type:** Harmony Prefix
- **Target:** `BuildingModelUpdater.UpdateBuildingsModelsAround(BlockObject)` — private method
- **How target is found:** `AccessTools.Method(AccessTools.TypeByName("BuildingModelUpdater"), "UpdateBuildingsModelsAround", new[] { typeof(BlockObject) })`
- **What it does:** If blockObject has `OverhangingBuilding`, it replaces the default neighbor-update logic with expanded bounds that cover the overhang extent. Returns `false` (skip original) if overhanging, `true` (run original) if not.
- **1.0 status:** `UpdateBuildingsModelsAround` still exists and is still private. `AccessTools.TypeByName` still works. However, `UpdateBuildingModelsAt(Vector3Int)` is also now private. The patch calls it on `__instance` directly — Harmony can still invoke private methods on the patched instance.
- **Note:** `BuildingModelUpdater` now uses constructor injection (takes `IBlockService blockService, EventBus eventBus`).

### `WorldBlockPatch.cs`
- **Type:** Harmony Prefix
- **Target:** `WorldBlock.SetBlockObject(BlockObject, BlockOccupations)` — was class+enum, now struct+Block
- **1.0 signature:** `SetBlockObject(BlockObject blockObject, Block block)` — `WorldBlock` is a **struct** (was class), `BlockOccupations` param replaced by `Block`.
- **What it does:** If blockObject has `OverhangingBuilding`, adds it to the internal `____blockObjects` list before the original method runs.
- **Must update:** `TargetMethod()` — change parameter type from `BlockOccupations` to `Block`.

### `MorePlatformsCore.cs`
- **Namespace:** `MorePlatforms`
- **Class:** `static class MorePlatformsCore`
- **Methods:**
  - `FindBodyPart(Transform parent, string bodyPartName)` — recursive search returning first match
  - `FindAllBodyParts(Transform parent, string bodyPartName)` — collects all matching children
- **Bug in FindAllBodyParts:** It calls `FindBodyPart` (singular) recursively instead of calling itself. Only the top-level direct children are checked; deeper nesting calls the wrong method. Bug inherited from original, fix if reactivated.
- **1.0 changes:** None. Standard Unity `Transform` API.

### `ModStarter.cs`
- **Namespace:** `Tobbert.MorePlatforms`
- **Implements:** `IModStarter`
- **Purpose:** Entry point. Creates Harmony instance `"tobbert.moreplatforms"` and calls `PatchAll()`.

### `Configurator.cs`
- **Namespace:** `Tobbert.MorePlatforms`
- **Base class:** `Configurator` (Bindito)
- **Registers:**
  - `SidePlatform` as transient
  - `SidePlatformSupportBlocker` as transient
  - `SidePlatformValidator` as singleton `IBlockObjectValidator`
  - `TemplateModule` provider that decorates `SidePlatformSpec` → `SidePlatform`, and `BlockObject` → `SidePlatformSupportBlocker`

### `SidePlatform.cs`
- **Base class:** `BaseComponent`
- **Purpose:** Empty marker component identifying side platform entities. Used by validation, demolition blocker, and Harmony patches to detect side platforms.

### `SidePlatformSpec.cs`
- **Record:** `SidePlatformSpec : ComponentSpec`
- **Purpose:** Empty marker spec. Blueprint JSON has `"SidePlatformSpec": {}` key. Used by `TemplateModule.AddDecorator` to attach `SidePlatform` component.

### `SidePlatformValidator.cs`
- **Implements:** `IBlockObjectValidator`
- **Purpose:** Validates side platforms at placement AND load time. Checks that the attachment target at local `(-1, 0, 0)` (transformed to world via `blockObject.TransformCoordinates()`) is either solid terrain or a structural block object.
- **Validation logic:** Uses `IsStructuralBlock()` which requires the block to be occupied but excludes `Floor` occupations (paths, thin platforms). Accepts `Stackable` blocks or blocks with `Top`, `Corners`, or `Middle` occupations. Self-filters (`GetComponent<SidePlatform>() == null`) so side platforms can't validate against each other.
- **Load-time fallback:** If `isPreview` is false, also scans `EntityRegistry.Entities` for instantiated-but-not-yet-in-BlockService block objects.

### `SidePlatformSupportBlocker.cs`
- **Base class:** `BaseComponent`
- **Implements:** `IBlockObjectDeletionBlocker`
- **Purpose:** Prevents demolition of walls/buildings that have side platforms attached. Scans all 4 horizontal neighbors of each block occupied by the target building; if a neighbor block belongs to a `SidePlatform` whose attachment point matches the target's block, deletion is blocked.
- **Attached to:** ALL `BlockObject` instances via `builder.AddDecorator<BlockObject, SidePlatformSupportBlocker>()`.
- **Localization key:** `"Tobbert.DeletionBlocker.SidePlatformAttached"`

### `BlockObjectPreviewPickerPatch.cs`
- **Harmony patches on `BlockObjectPreviewPicker`:**
  - `CenteredPreviewCoordinates` prefix/postfix to capture the current `PlaceableBlockObjectSpec`
  - `IsTerrainOrUnfinishedTerrain` postfix to force `true` for cells occupied by any block object when the current building has `SidePlatformSpec` — allows previewing side platforms on existing buildings
- **Uses direct field access** on publicized `StackableBlockService._blockService` (was `Traverse` before publicization).

### `BlockObjectPatch.cs`
- **Harmony postfix on `BlockObject.IsAlmostValid`** — forces `true` for objects with `SidePlatform` component so the preview renders as the red "unbuildable" ghost instead of being dropped entirely.

### `TerrainPhysicsPostLoaderPatch.cs`
- **Harmony postfix on `TerrainPhysicsPostLoader.ValidateBlockObjects(Vector3Int)`** — after the BFS validates each coordinate, scans the block objects at that coordinate. For each validated building, checks its 4 horizontal neighbors for `SidePlatform` block objects NOT yet in `_validBlockObjects`. If a platform's attachment coordinate `(-1, 0, 0)` transformed to world matches a structural block on the validated building, the platform is added to `_validBlockObjects` and its occupied cells above are enqueued for continued BFS propagation.
- **Requires publicizing `Timberborn.TerrainPhysics`** (in csproj) to access `_validBlockObjects`, `_blockService`, `_mapIndexService`, `_visited`, and `Enqueue`.

### `DeconstructionToolPatch.cs`
- **Single postfix on `TerrainAndBlockObjectsToDeleteFinder.AddNextBlockObjectToValidate(BlockObject)`** — when a building is queued for deletion validation, scans all occupied blocks of that building and checks each of the 4 horizontal neighbors. If a neighbor is a `SidePlatform` whose attachment coordinate matches one of the building's blocks, enqueues the platform for deletion and recursively calls `AddNextBlockObjectToValidate` on it to cascade any further attachments.
- **Cascade pattern:** `MarkBlockObjectBlocksForDeletion(obj)` + `AddNextBlockObjectToValidate(obj)` ensures recursive discovery of chained platforms.
- **Requires publicizing `Timberborn.BlockObjectPickingSystem`** (in csproj) for access to `_blockService` and `_blockObjectsToCheck` on `TerrainAndBlockObjectsToDeleteFinder`.

## Dead (Commented-Out) Source Code — History Only

### `Plugin.cs`
- Implemented `IModStarter`
- Called `new Harmony(PluginGuid).PatchAll()`
- Contained `FakeParentedNeighborCalculator` helper class
- Contained 2 disabled Harmony patches: `RemoveMiddlePatch` (broken `|` vs `||` logic) and `ParentedNeighborCalculatorPatch`

### `MorePlatformsConfigurator.cs`
- `[Context("Game")] class PluginConfigurator : IConfigurator`
- Registered `FakeParentedNeighborCalculator` as singleton

### `BlockServicePatch.cs`
- 3 Harmony patches on `BlockService` (pre-1.0 API): `AnyObjectAt`, `SetObject`, `UnsetObject`
- These directly accessed `____blocks` (Array3D<WorldBlock>) and `____eventBus` private fields
- `IBlockService` public interface no longer exposes these methods the same way

### `ShaderFix.cs`
- Harmony Postfix on `MaterialRepository.Load()`
- Loaded custom materials from `CustomMaterials/` and overrode their shader
- Used old `TimberAPI.DependencyContainerSystem` — completely obsolete

## Original TimberAPI Build System (for learning)

### No `.csproj` — Unity `.asmdef` instead
The mod had a `MorePlatforms.asmdef` file referencing game DLLs and dependencies (Harmony, Bindito, TimberAPI, all Timberborn assemblies).

**Key difference in 1.0:** You create a `.csproj` that references the game DLLs directly. No `.asmdef` needed.

### TimberAPI Spec Files
`ToolGroupSpecification.*.json` injected tool groups via `GroupId` nesting. `PrefabGroupSpecification.*.json` added prefab paths to building groups via `Paths#append`.

**Key difference in 1.0:** Replaced by native `.blueprint.json` spec files. Game discovers them automatically via `ModSystemFileProvider<BlueprintAsset>`.

### 3D Model Pipeline (Old)
1. Models created in **Blender** (`.blend` source files)
2. Exported/imported into Unity as **`.prefab`** files
3. TimberAPI loaded the `.prefab` as a `GameObject` from Resources

### Prefab Components (Old)
Custom components like `DirectionalConnector` were added directly to the `.prefab` in the Unity Editor. In 1.0, blueprints define components via specs + TemplateModule decorators.

## Timberborn 1.0 Spec System (Blueprint JSON)

### How Building Blueprints Work

Every building in 1.0 is defined by a `.blueprint.json` file. The JSON has top-level keys matching `ComponentSpec` subclass type names. The game discovers these via `ModSystemFileProvider<BlueprintAsset>` which scans mod directories for `*.blueprint.json` files.

The relevant `ComponentSpec` types:

| JSON Key | C# Type | Purpose |
|----------|---------|---------|
| `BuildingSpec` | `Timberborn.Buildings.BuildingSpec` | Costs (BuildingCost, ScienceCost), sounds, PlaceFinished flag |
| `PlaceableBlockObjectSpec` | `Timberborn.BlockSystem.PlaceableBlockObjectSpec` | ToolGroupId, ToolOrder, CanBeAttachedToTerrainSide, layout info |
| `LabeledEntitySpec` | `Timberborn.Common.LabeledEntitySpec` | DisplayNameLocKey, DescriptionLocKey, FlavorDescriptionLocKey |
| `BlockObjectSpec` | `Timberborn.BlockSystem.BlockObjectSpec` | Size (Vector3Int), Blocks array (coordinates + occupation + offset) |
| `BuildingModelSpec` | `Timberborn.Buildings.BuildingModelSpec` (internal) | FinishedModelName, UnfinishedModelName, UndergroundModelName |
| `TemplateSpec` | `Timberborn.TemplateSystem.TemplateSpec` | TemplateName (must match `{Name}.{Faction}` convention), RequiredFeatureToggle |
| `BlockObjectToolGroupSpec` | `Timberborn.BlockObjectTools.BlockObjectToolGroupSpec` | Id, Order, NameLocKey, Icon, FallbackGroup |
| `TimbermeshSpec` | `Timberborn.Timbermesh.TimbermeshSpec` | Model (AssetRef<BinaryData> — path to `.bytes` timbermesh file) |
| `CollidersSpec` | `Timberborn.UnityEngineSpecs.CollidersSpec` | Colliders array (for physics) |

### Blueprint JSON Structure Example

```json
{
  "BuildingSpec": { ... },
  "BuildingModelSpec": { ... },
  "PlaceableBlockObjectSpec": { "ToolGroupId": "Wood", "ToolOrder": 80, ... },
  "LabeledEntitySpec": { "DisplayNameLocKey": "...", ... },
  "BlockObjectSpec": { "Size": {...}, "Blocks": [...], ... },
  "TemplateSpec": { "TemplateName": "WoodWorkshop.Folktails", ... },
  "Children": {
    "#Finished": { "TimbermeshSpec": { "Model": "..." }, "CollidersSpec": {...} },
    "#Unfinished": { "Children": { "ConstructionStage0": { ... } } }
  }
}
```

### Tool Group Specs

Tool groups are defined in `Blueprints/BlockObjectToolGroups/BlockObjectToolGroup.{GroupId}.blueprint.json`:
```json
{
  "BlockObjectToolGroupSpec": {
    "Id": "Wood",
    "Order": 60,
    "NameLocKey": "ToolGroups.Wood",
    "Icon": "Sprites/BottomBar/BuildingGroups/Wood",
    "FallbackGroup": false
  }
}
```

Tool groups do NOT have a `GroupId` (nesting) field in 1.0. They are independent groups.

### Blueprint Discovery & Template System

1. `TemplateCollectionSpec` blueprints define collections (e.g., `"CollectionId": "Common"`)
2. `TemplateCollectionService` loads all template collections
3. All templates from all loaded collections are available for placement
4. Each building blueprint has a `TemplateSpec` with `TemplateName` to register it

### Asset Loading

- `ModSystemFileProvider<T>` walks each enabled mod's directory tree
- Converts files via `IModFileConverter` implementations:
  - `.png` → `Sprite` (ModSpriteConverter)
  - `.json` → `TextAsset` (ModTextAssetConverter)
  - `.blueprint.json` → `BlueprintAsset` (ModBlueprintConverter)
  - `.bytes` → `BinaryData` (ModTimbermeshConverter)
- Assets in `AssetBundles/` folder are loaded as Unity AssetBundles (with platform suffix: `_win`, `_mac`)

### Vanilla Power Shaft Reference (Directional Connection Canonical Pattern)

**Source:** `Timberborn.ModularShafts.cs` (internal — see root AGENTS.md §4 for decompiled source paths)

The power shafts implement direction-aware visual adaptation (same problem as MorePlatforms DirectionalConnector). Key architecture:

1. **`ModularShaft : BaseComponent`** — peripheral component that detects placement/destruction events and notifies neighbors. Uses **constructor injection**:
   ```csharp
   public ModularShaft(IBlockService blockService, PreviewBlockService previewBlockService)
   ```
   Implements `IPrePlacementChangeListener`, `IPostPlacementChangeListener`, `IPreviewSelectionListener`, `IFinishedStateListener`.
   Notifies neighbors via `BlockObjectModelController.UpdateModel()`.

2. **`ModularShaftVariantFinder : BaseComponent`** — determines the `ShaftVariant` (which directions have mechanical connections). Uses `MechanicalNode.Transputs` to check actual connections.

3. **`ModularShaftModelUpdater : BaseComponent`** — contains the `UpdateModel()` logic. Implements `IAwakableComponent`, `IStartableComponent`, `IMechanicalModelUpdater`, **`IModelUpdater`**. Builds model procedurally from `ModularShaftPartsSpec` parts via `ShaftModelFactory`.

4. **`ModularShaftSpec : ComponentSpec`** — empty marker spec.

5. **Configurator chaining pattern:**
   ```csharp
   builder.AddDecorator<ModularShaftSpec, ModularShaft>();
   builder.AddDecorator<ModularShaft, ModularShaftModelUpdater>();
   builder.AddDecorator<ModularShaft, ModularShaftVariantFinder>();
   builder.AddDecorator<ModularShaftCoverSpec, ModularShaftCover>();
   ```

6. **`IBlockService`** (public interface) exposes:
   - `GetObjectsAt(Vector3Int)` → `ReadOnlyList<BlockObject>`
   - `GetFirstObjectWithComponentAt<T>(Vector3Int coordinates)`
   - `AnyObjectAt(Vector3Int)`

**Key takeaway for DirectionalConnector:** Use constructor injection, implement the same listener interfaces, and use `IBlockService.GetObjectsAt()` to check neighbors. `[Inject]` method injection is legacy — do not use.

## Blueprint Construction Site Architecture

Every building blueprint that is NOT `PlaceFinished: true` has a `#Unfinished` child in `Children` containing exactly two sub-children:

1. **ConstructionBase<NxM>#nested** — A reference to a shared, faction-independent construction base blueprint via `BlueprintPath: "ConstructionBases/ConstructionBase<NxM>/ConstructionBase<NxM>.blueprint"`. These blueprints live in `Blueprints/ConstructionBases/` (25 variants). Their `.timbermesh` models use `BeaverCarryingModels` and `Dirt` materials. When the building is larger than the construction base, a `Modification.TransformSpec.Position` offset repositions it within the footprint.

2. **ConstructionStage0** — A model + colliders defined inline. The model path is faction-specific. The `.timbermesh` file uses faction-colored materials (e.g., `BaseWood_Brown.Folktails` + `Dirt` for Folktails, `BaseWood_DarkBrown.IronTeeth` + `Dirt` for IronTeeth).

When `PlaceFinished: true`, `#Unfinished` only has the ConstructionBase (no ConstructionStage0).

Standard `ConstructionSiteProgressVisualizerSpec` has `ProgressThresholds: [0.0]` — two stages: initial (ConstructionBase + Stage0) and finished (#Finished).

**This mod uses:** shared `ConstructionSite1x1.5.Model` across all blueprints (ConstructionBase pattern).

## Material Name Mapping (Blender → Game)

Critical: Use `Name.FactionId` dot notation (e.g., `BaseWood_Brown.Folktails`), NOT underscore. The game's `MaterialRepository.GetMaterial()` looks up by Unity `m_Name` field.

| Blender Material | Game Material | Files Affected |
|---|---|---|
| `BaseWood_Brown_Folktails` / `.001`/`.002`/`.003`/`.004`/`.006` | `BaseWood_Brown.Folktails` | All platform blends |
| `BaseMetal_IronTeeth.001` / `.004` | `BaseMetal.IronTeeth` | End3x1, End4x1 |
| `BaseWood_DarkBrown_IronTeeth.001` / `.002` | `BaseWood_DarkBrown.IronTeeth` | End3x1, End4x1 |
| `BeaverCarryingModels` | `BeaverCarryingModels` (no change) | ConstructionSite1x1.5 |
| `Dots Stroke` | Unused / remove | ConstructionSite1x1.5 |

## Export Process

`export_timbermesh.py` reads from `Models/` (working copies). Uses `timbermesh_exporter.ExportSettings(merge_meshes=True, single_animation=True, use_vertex_animations=False)`. Exports all collections per faction.

**Official timbermesh Blender plugin:** https://github.com/mechanistry/timbermesh (MIT, v1.2.0+)

**Headless batch export workflow:**
```python
# export_all.py — run with: blender --background model.blend --python export_all.py
import bpy, sys, os
sys.path.append("path/to/timbermesh_blender_plugin")
from timbermesh_blender_plugin import timbermesh_exporter

input_dir = "Models/"
output_dir = "AssetBundles/"

for fname in os.listdir(input_dir):
    if fname.endswith(".blend"):
        bpy.ops.wm.open_mainfile(filepath=os.path.join(input_dir, fname))
        settings = timbermesh_exporter.ExportSettings(bpy.context, True, True, False)
        for collection in bpy.data.collections:
            out_path = os.path.join(output_dir, collection.name + ".Model.timbermesh")
            timbermesh_exporter.Exporter.export_collection(collection, out_path, settings)
```

**Existing local tools** at `C:\Users\calloatti\source\repos\Tools\timbermesh\`:
- `timbermesh.ps1` — diagnostic decoder that decompresses and prints `.timbermesh` structure
- `timbermesh_plugin_2026-07-15.py` — custom importer plugin (reads `.timbermesh` back into Blender)
- `patch_banner.py` — binary-level vertex patcher (modifies positions in `.timbermesh` files directly)
- `bags.py` — binary-level generator (duplicates, rotates, scales meshes within `.timbermesh`)

## 1.0 API Changes Summary

| Old (0.6/0.7) | 1.0 Replacement | Files Affected |
|---|---|---|
| `GetComponentFast<T>()` | `GetComponent<T>()` | DirectionalConnector.cs, OverhangingBuilding.cs |
| `TryGetComponentFast<T>()` | `TryGetComponent<T>(out T)` | (in dead BlockServicePatch) |
| `[Inject]` method injection (legacy) | Constructor injection (vanilla `ModularShaft` pattern) | DirectionalConnector.cs |
| `BlockService` (public class) | `IBlockService` (public interface) | DirectionalConnector.cs |
| `WorldBlock` (class) | `WorldBlock` (struct) | WorldBlockPatch.cs |
| `WorldBlock.SetBlockObject(BlockObject, BlockOccupations)` | `SetBlockObject(BlockObject, Block)` | WorldBlockPatch.cs |
| `BuildingModelUpdater.UpdateBuildingsModelsAround(BlockObject)` (public) | Still exists but private | BuildingModelUpdaterPatch.cs |
| `BuildingModelUpdater.UpdateBuildingModelsAt(Vector3Int)` (public) | Still exists but private | BuildingModelUpdaterPatch.cs |
| `Array3D<WorldBlock>` (public) | Still `Array3D<WorldBlock>` but WorldBlock is a struct (value type semantics differ) | (dead BlockServicePatch) |
| `IConfigurator` (from Bindito) | `Configurator` (abstract class from Bindito) | MorePlatformsConfigurator.cs (dead) |
| `TimberAPI.DependencyContainerSystem` | Native Bindito DI via `Configurator` | ShaderFix.cs (dead) |
| `IModEnvironment` (from TimberAPI) | Same interface but native now | Plugin.cs (dead) |
| `PrefabGroupSpecification` (TimberAPI) | `.blueprint.json` + `TemplateSpec` + `PlaceableBlockObjectSpec` | Spec files |
| `ToolGroupSpecification` (TimberAPI) | `BlockObjectToolGroupSpec` | Spec files |
| `.prefab` files (Unity) | `.blueprint.json` + Timbermesh `.bytes` | Asset pipeline |
| `AssetBundles/Resources/Prefabs/` (Unity Resources) | `Blueprints/` (spec JSON) + `AssetBundles/` (timbermesh bytes) | Asset pipeline |

## Migration Plan

### Phase 1: Project Scaffolding
1. **Create `MorePlatforms.csproj`:**
   - `<TargetFramework>netstandard2.1</TargetFramework>`
   - Reference all Timberborn DLLs from the game folder
   - Reference `0Harmony.dll` and `Bindito.Core.dll`
   - Set up PreBuild/PostBuild targets to copy to mod folder if desired
2. **Update `manifest.json`:**
   - Change `"MinimumGameVersion"` to `"1.0.0.0"`
   - Remove the TimberAPI dependency from `"RequiredMods"`
   - Keep Harmony dependency
3. **Remove `MorePlatforms.asmdef`** and `*.meta` files (no longer needed)
4. **Delete dead files that won't be revived:**
   - `BlockServicePatch.cs`
   - `ShaderFix.cs`
   - `MorePlatformsConfigurator.cs` (old — new one will be created)
   - `Plugin.cs` (old — new one will be created)

### Phase 2: C# Code Migration

#### New `Plugin.cs`
```csharp
using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace MorePlatforms
{
    public class Plugin : IModStarter
    {
        public void StartMod(IModEnvironment modEnvironment)
        {
            new Harmony("tobbert.moreplatforms").PatchAll();
        }
    }
}
```

#### New Configurator
```csharp
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.TemplateSystem;

namespace MorePlatforms
{
    [Context("Game")]
    public class MorePlatformsConfigurator : Configurator
    {
        protected override void Configure()
        {
            Bind<DirectionalConnector>().AsTransient();
            Bind<DirectionalConnectorModelUpdater>().AsTransient();
            MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private static TemplateModule ProvideTemplateModule()
        {
            var builder = new TemplateModule.Builder();
            builder.AddDecorator<DirectionalConnectorSpec, DirectionalConnector>();
            builder.AddDecorator<DirectionalConnector, DirectionalConnectorModelUpdater>();
            return builder.Build();
        }
    }
}
```

#### Fix `DirectionalConnector.cs`
1. **Switch to constructor injection:**
   ```csharp
   private readonly IBlockService _blockService;
   private readonly PreviewBlockService _previewBlockService;
   private readonly ITerrainService _terrainService;
   private BlockObject _blockObject;

   public DirectionalConnector(IBlockService blockService, PreviewBlockService previewBlockService, ITerrainService terrainService)
   {
       _blockService = blockService;
       _previewBlockService = previewBlockService;
       _terrainService = terrainService;
   }
   ```
2. Replace `GetComponentFast<BlockObject>()` with `GetComponent<BlockObject>()` in `Awake()`.
3. Replace `BlockService` type with `IBlockService` interface.
4. Remove `_eventBus` and `[OnEvent]` listener — use `IPrePlacementChangeListener` / `IPostPlacementChangeListener` / `IFinishedStateListener` interfaces instead.
5. Replace `BuildingModelUpdater.UpdateBuildingModelsAt()` with neighbor notification via `BlockObjectModelController.UpdateModel()`.
6. Create `DirectionalConnectorSpec : ComponentSpec` as marker spec.
7. Register via chaining in Configurator.

#### Fix `WorldBlockPatch.cs`
1. Update `TargetMethod()` — change second parameter from `BlockOccupations` to `Block`.
2. `WorldBlock` is now a `struct` — verify field access patterns.

#### Fix `BuildingModelUpdaterPatch.cs`
1. Verify `TargetMethod()` still resolves correctly.
2. `UpdateBuildingModelsAt` is now private — Harmony can still invoke it through the patched instance.

### Phase 3: Blueprint Spec Files

#### Tool Groups
Create `Blueprints/BlockObjectToolGroups/`:
- `BlockObjectToolGroup.CategorySidePlatforms.blueprint.json`
- `BlockObjectToolGroup.CategoryReinforcedPlatforms.blueprint.json`

#### Building Blueprints
Create `Blueprints/Buildings/SidePlatforms/` and `Blueprints/Buildings/ReinforcedPlatforms/`:
- One `.blueprint.json` per building prefab per faction (12 total for active prefabs)

#### Template Registration
Create `Blueprints/TemplateCollections/TemplateCollection.Common.blueprint.json` or rely on each blueprint's `TemplateSpec`.

### Phase 4: 3D Asset Pipeline
Use Blender → Timbermesh `.bytes` via the official timbermesh Blender plugin. See [Export Process](#export-process) section above for the scriptable CLI workflow.

### Phase 5: Localizations
- Keep existing CSV files in `Data/Localizations/` — format is compatible with 1.0
- Verify enUS.csv keys match the blueprint `LabeledEntitySpec` localization keys

## Build & Test

- Open `.csproj` in Visual Studio/Rider, build
- DLL auto-deploys to `Documents\Timberborn\Mods\More Platforms\Version-1.0\`
- Test in-game: new game with Folktails or IronTeeth, check building menu for Side Platforms

### Quick Restart Workflow

`C:\Users\calloatti\source\repos\Tools\timberborn_restart_and_continue.ps1`

Finds the newest `.timber` save, closes Timberborn gracefully, then relaunches via Steam with `-skipModManager -settlementName <name> -saveName <name>` to auto-load that save. Run this after rebuilding the DLL to test changes quickly without navigating menus.

## Known Issues / Lessons Learned

1. **Material naming:** Use `Name.FactionId` dot notation (e.g., `BaseWood_Brown.Folktails`), NOT underscore. The game's `MaterialRepository.GetMaterial()` looks up by Unity `m_Name` field.
2. **Blender dedup:** Importing Unity assets with duplicate material names creates `.001`/`.002` etc. variants — these must be merged/renamed before export.
3. **Namespace corrections:** `ComponentSpec` is in `Timberborn.BlueprintSystem`, `TemplateModule` is in `Timberborn.TemplateInstantiation` — NOT `Timberborn.BaseComponentSystem` or `Timberborn.TemplateSystem`.
4. **Publicizer:** `<Publicize Remove="Timberborn.BlueprintSystem" />` needed to prevent record inheritance breakage.
5. **NO AssetBundles:** 1.0 mod system does not use them. Category sprites go in `CategorySprites/` at mod root.
6. **Prefab converter:** See `.meta/prefab_converter.csv` for GUID/field migration between game versions (0.6 → 1.1).
7. **Timbermesh visualisation fix (1.1.0.2):** Material script ref must use `{fileID: 738743559, guid: 79a76570d9fab1d82517314361c9ddd8, type: 3}` not `{instanceID: 0}`.
8. **Bug in FindAllBodyParts (MorePlatformsCore.cs):** It calls `FindBodyPart` (singular) recursively instead of calling itself. Fix if reactivated.
9. **Moving origin in Blender — DO NOT counter-translate vertices:** The timbermesh exporter bakes `obj.matrix_world` into vertex positions. To shift a model's world-space position (which is the goal of an origin fix), you ONLY need to change `obj.location`. Counter-translating vertices (shifting them opposite to the location change) preserves the original world positions, which undoes the fix. The correct approach is `obj.location += OFFSET` with NO vertex modification — the exporter then produces vertices at `original_world + OFFSET`, which is the desired correction. The working script is `.scratch/fix_location_only.py`.
10. **Blender 4.2 vertex persistence bug:** Direct vertex modifications (`v.co -= offset`, `mesh.vertices.foreach_set()`) fail to persist on save for meshes with coordinate ranges beyond ±100 units. The in-memory changes are correct, but `bpy.ops.wm.save_as_mainfile()` silently drops them. Location changes always persist. If you MUST modify vertex coordinates for such meshes, use `bpy.ops.object.origin_set()` (which uses Blender's internal C++ machinery) or work in world-space export rather than counter-translation.
11. **HorizontalPlatformEnd origin offset:** The correct origin translation for End platform files (2x1–5x1) relative to the original source `.blend` files is **(1, 0, 1)** — 1 block right in X, 1 block up in Z. Only modify these 4 files; the 1x1 is already correct. Always start from fresh copies of the original source, apply material fixes first, then move the origin (location only, no vertex counter-translation). Working script: `.scratch/fix_location_only.py`.
12. **Timbermesh export order:** Always apply material fixes to ALL `.blend` files before running `export_timbermesh.py`. Exporting all stems from unfixed blends will produce `.timbermesh` files with wrong material names (underscore instead of dot notation), which crash at runtime with "not found in repository" errors.
13. **`export_timbermesh.py` caveat:** The script exports ALL collections in a blend to the same output path per stem+faction — if a blend has multiple collections, the last one overwrites the rest. Each blend should have exactly one collection for this to work correctly. When only specific files were modified, run a targeted export (see `.scratch/export_end_only.py`) instead of the full export to avoid overwriting working timbermeshes.
14. **TerrainPhysicsPostLoader flood fill limitation:** The BFS only propagates horizontally through `UnfinishedGround` blocks and terrain cells. Regular `BlockObject` stackable blocks only propagate **upward**, never sideways. This means side platforms attached to buildings (not terrain) are never reached by the flood fill and get deleted on load. Fix: Harmony postfix on `ValidateBlockObjects` that scans each validated building's neighbors for attached side platforms and adds them to `_validBlockObjects`. Requires `Timberborn.TerrainPhysics` publicized.
15. **ConstructionSite1x1.5 rebuild process:** Import vanilla `ConstructionBase1x1.Model.timbermesh` via `timbermesh_plugin_2026-07-15.py` (which applies Unity→Blender coordinate transform). Delete Dirt mesh, keep `BeaverCarryingModels` wood frame. Rotate -90° around Y (horizontal→vertical in YZ plane, faces -X). Set origin via `obj.location = (0, 0, 1)`. Use `frame.data.transform(rot)` for rotation + `frame.location` for positioning — never counter-translate vertices. The timbermesh exporter bakes `matrix_world`.
16. **Blender ↔ Game coordinate mapping:** The import plugin converts Unity→Blender via `(bx, by, bz) = (-ux, -uz, uy)`. The timbermesh exporter does the reverse: `(ux, uy, uz) = (-bx, bz, -by)`. To position a model in game-space `[tx, ty, tz] + local`, compute the required Blender location by solving the export transform.
17. **ConstructionSite1x1.5 final specs:** Material `BeaverCarryingModels` only (no Dirt). Game-space bounds: X[0, 0.322] Y[-0.022, 1.022] Z[-0.022, 1.022] — narrow scaffold at left/cliff-facing edge of the block. Faction-independent; all blueprints reference `ConstructionSite1x1.5.Model` without faction suffix.
18. **Publicizing `Timberborn.TerrainPhysics`, `Timberborn.BlockObjectPickingSystem`:** Required to access private fields of `TerrainPhysicsPostLoader` and types used by `DeconstructionToolPatch`. Both are in the csproj's per-project `<Publicize>` list — NOT in `CommonModSettings.props` (which is shared across mods).
19. **Deconstruction tool cascade:** The `TerrainAndBlockObjectsToDeleteFinder.AddNextBlockObjectToValidate` method is the single hook point for both preview highlighting and actual deletion. Patched via Harmony postfix: when a building is queued for deletion, it scans all 4 horizontal neighbors of every occupied block; any `SidePlatform` whose attachment coordinate matches is enqueued for deletion and recursed into for chained platforms. This approach works because the base `TerrainAndBlockObjectsToDeleteFinder` is used by both the preview (red highlight) and the actual deletion pass.

## Open Decisions

1. **DirectionalConnector** — needed for visual neighbor adaptation. If included, use vanilla `ModularShaft` pattern. Can be deferred if static models are used initially.
2. **OverhangingBuilding** — may not be needed if overhanging logic isn't re-implemented.
3. **BuildingModelUpdaterPatch** — if DirectionalConnector is deferred, this patch may be unnecessary.
4. **WorldBlockPatch** — only needed if OverhangingBuilding logic is revived.
5. **Reinforced platforms/ladder** — currently in UnusedPrefabs/. Can be included or excluded in the 1.0 port.
6. **DirectionalConnectorSpec + TemplateModule** — Use vanilla `ModularShaft` pattern with constructor injection and listener interfaces.

## Localization Keys (from enUS.csv)

| Key | English Text |
|-----|--------------|
| `Tobbert.HorizontalPlatformEnd1x1.DisplayName` | Side Platform 1x1 |
| `Tobbert.HorizontalPlatformEnd1x1.Description` | A platform that can be placed on the side of a cliffs, levees, reinforced platforms or Platform Middle Pieces. |
| `Tobbert.HorizontalPlatformEnd1x1.FlavorDescription` | Imagine if you could stack these on itself. |
| `Tobbert.HorizontalPlatformEnd2x1.DisplayName` | Side Platform 2x1 |
| `Tobbert.HorizontalPlatformEnd2x1.Description` | A platform that can be placed on the side of a cliffs, reinforced platforms or Platform Middle Pieces. |
| `Tobbert.HorizontalPlatformEnd2x1.FlavorDescription` | Double the platforms, double the fun! |
| `Tobbert.HorizontalPlatformEnd3x1.DisplayName` | Side Platform 3x1 |
| `Tobbert.HorizontalPlatformEnd3x1.Description` | A platform that can be placed on the side of a cliffs, reinforced platforms or Platform Middle Pieces. |
| `Tobbert.HorizontalPlatformEnd3x1.FlavorDescription` | This is for the looooooong bois. |
| `Tobbert.HorizontalPlatformEnd4x1.DisplayName` | Side Platform 4x1 |
| `Tobbert.HorizontalPlatformEnd4x1.Description` | A platform that can be placed on the side of a cliffs, reinforced platforms or Platform Middle Pieces. |
| `Tobbert.HorizontalPlatformEnd4x1.FlavorDescription` | This is not how physics work. |
| `Tobbert.ReinforcedPlatform.DisplayName` | Reinforced Platform |
| `Tobbert.ReinforcedPlatform.Description` | A platform which can be used in combination with the Side Platforms. |
| `Tobbert.ReinforcedPlatform.FlavorDescription` | Stack it to the top! |
| `Tobbert.DoubleReinforcedPlatform.DisplayName` | Double Reinforced Platform |
| `Tobbert.DoubleReinforcedPlatform.Description` | A platform which can be used in combination with the Side Platforms. |
| `Tobbert.DoubleReinforcedPlatform.FlavorDescription` | Stack, stack it to the top! |
| `Tobbert.TripleReinforcedPlatform.DisplayName` | Triple Reinforced Platform |
| `Tobbert.TripleReinforcedPlatform.Description` | A platform which can be used in combination with the Side Platforms. |
| `Tobbert.TripleReinforcedPlatform.FlavorDescription` | Stack, stack, stack it to the top! |
| `Tobbert.CategoryReinforcedPlatforms.DisplayName` | Reinforced Platforms |
| `Tobbert.CategorySidePlatforms.DisplayName` | Side Platforms |

All 14 locale files have identical keys with translated text. Keep all 14.

## Verification Checklist

- [x] Build succeeds with 0 errors, 0 warnings
- [ ] Mod loads without TimberAPI dependency
- [ ] Tool groups appear in the bottom bar
- [ ] Building prefabs are placeable
- [ ] Models display correctly (after timbermesh conversion)
- [ ] Localizations display correctly
- [ ] DirectionalConnector model switching works (if included)
- [ ] Save/load works (blueprint system handles this natively — verify no custom save data needed)
