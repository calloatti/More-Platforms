using System.Collections.Generic;
using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.TerrainPhysics;
using UnityEngine;

namespace Tobbert.MorePlatforms
{
  [HarmonyPatch(typeof(TerrainPhysicsPostLoader), "ValidateBlockObjects")]
  public static class ValidateBlockObjectsPatch
  {
    [HarmonyPostfix]
    public static void Postfix(TerrainPhysicsPostLoader __instance, Vector3Int coordinates)
    {
      HashSet<BlockObject> validSet = __instance._validBlockObjects;
      IBlockService blockService = __instance._blockService;

      foreach (BlockObject bo in blockService.GetObjectsAt(coordinates))
      {
        if (bo == null || !validSet.Contains(bo))
          continue;

        foreach (Block block in bo.PositionedBlocks.GetAllBlocks())
        {
          TryAttachSidePlatforms(__instance, validSet, blockService, bo, block);
        }
      }
    }

    private static void TryAttachSidePlatforms(
        TerrainPhysicsPostLoader instance,
        HashSet<BlockObject> validSet,
        IBlockService blockService,
        BlockObject bo,
        Block block)
    {
      Vector3Int[] dirs =
      {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0)
      };

      foreach (Vector3Int dir in dirs)
      {
        Vector3Int neighborCoord = block.Coordinates + dir;
        foreach (BlockObject neighbor in blockService.GetObjectsAt(neighborCoord))
        {
          if (neighbor == null || neighbor.GetComponent<SidePlatform>() == null || validSet.Contains(neighbor))
            continue;

          Vector3Int attachmentCoord = neighbor.TransformCoordinates(new Vector3Int(-1, 0, 0));
          if (bo.PositionedBlocks.TryGetBlock(attachmentCoord, out Block attBlock) && attBlock.IsOccupied)
          {
            validSet.Add(neighbor);

            foreach (Block nb in neighbor.PositionedBlocks.GetAllBlocks())
            {
              Vector3Int above = nb.Coordinates.Above();
              int idx = instance._mapIndexService.CoordinatesToIndex3D(above);
              instance._visited[idx] = byte.MaxValue;
              instance.Enqueue(above, 0);
            }
          }
        }
      }
    }
  }


}
