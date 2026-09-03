using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.TerrainPhysics;
using UnityEngine;

namespace Tobbert.MorePlatforms
{
  [HarmonyPatch(typeof(TerrainAndBlockObjectsToDeleteFinder), "AddNextBlockObjectToValidate")]
  public static class AddNextBlockObjectToValidatePatch
  {
    [HarmonyPostfix]
    public static void Postfix(
        TerrainAndBlockObjectsToDeleteFinder __instance,
        BlockObject blockObject)
    {
      foreach (var block in blockObject.PositionedBlocks.GetOccupiedBlocks())
      {
        foreach (var delta in Deltas.Neighbors4Vector3Int)
        {
          var neighborCoord = block.Coordinates + delta;
          foreach (var obj in __instance._blockService.GetObjectsAt(neighborCoord))
          {
            if (obj == null || obj.GetComponent<SidePlatform>() == null)
              continue;

            var anchor = obj.TransformCoordinates(new Vector3Int(-1, 0, 0));
            if (anchor == block.Coordinates)
            {
              __instance._blockObjectsToCheck.Enqueue(obj);
              __instance.MarkBlockObjectBlocksForDeletion(obj);
              __instance.AddNextBlockObjectToValidate(obj);
            }
          }
        }
      }
    }
  }
}
