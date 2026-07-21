using HarmonyLib;
using Timberborn.BlockObjectPickingSystem;
using Timberborn.BlockSystem;
using Timberborn.LevelVisibilitySystem;
using UnityEngine;

namespace Tobbert.MorePlatforms
{
  [HarmonyPatch(typeof(BlockObjectPreviewPicker))]
  public static class BlockObjectPreviewPickerPatch
  {
    private static PlaceableBlockObjectSpec _currentSpec;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockObjectPreviewPicker.CenteredPreviewCoordinates))]
    private static void CenteredPreviewCoordinatesPrefix(PlaceableBlockObjectSpec placeableBlockObjectSpec)
    {
      _currentSpec = placeableBlockObjectSpec;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BlockObjectPreviewPicker.CenteredPreviewCoordinates))]
    private static void CenteredPreviewCoordinatesPostfix()
    {
      _currentSpec = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch("IsTerrainOrUnfinishedTerrain")]
    private static void IsTerrainOrUnfinishedTerrainPostfix(
            ref bool __result,
            Vector3Int coords,
            StackableBlockService ____stackableBlockService,
            ILevelVisibilityService ____levelVisibilityService)
    {
      // If vanilla already recognized this as valid terrain, leave it true
      if (__result)
      {
        return;
      }

      // Strictly restrict side-building attachment to objects with SidePlatformSpec
      if (_currentSpec != null && _currentSpec.GetSpec<SidePlatformSpec>() != null)
      {
        var blockService = ____stackableBlockService._blockService;
        if (blockService != null && blockService.AnyObjectAt(coords) && coords.z <= ____levelVisibilityService.MaxVisibleLevel)
        {
          __result = true;
        }
      }
    }
  }
}