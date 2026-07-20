# Origin Fix v2 — 2026-07-19

## Problem
`HorizontalPlatformEnd2x1`–`5x1` models appeared 1 block down and 1 block too near in game.

## Fix
Offset: **(1, 0, 1)** — origin moved 1 block right in X, 1 block up in Z.

## Process (per file)
1. Copied fresh from original source (`MorePlatforms/AssetBundles/Resources/Models/`)
2. Applied material fixes (underscore → dot notation)
3. Translated all object locations by (1, 0, 1)
4. Counter-translated all geometry back (meshes + curves)
5. Exported only the 4 End platform timbermeshes (no other files affected)

## Files Modified
- HorizontalPlatformEnd2x1.blend
- HorizontalPlatformEnd3x1.blend
- HorizontalPlatformEnd4x1.blend
- HorizontalPlatformEnd5x1.blend

## Scripts
- `.scratch/fix_origin_v2.py` — the fix
- `.scratch/export_end_only.py` — timbermesh export for just these 4 files
