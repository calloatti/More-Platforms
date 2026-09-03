using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Coordinates;
using Timberborn.Explosions;
using Timberborn.Localization;
using UnityEngine;

namespace Tobbert.MorePlatforms
{
  public class SidePlatformDynamiteBlocker : IBlockObjectValidator
  {
    private static readonly string BlockedKey = "Tobbert.DynamiteBlocker.SidePlatformAttached";

    private readonly IBlockService _blockService;
    private readonly ILoc _loc;

    public SidePlatformDynamiteBlocker(IBlockService blockService, ILoc loc)
    {
      _blockService = blockService;
      _loc = loc;
    }

    public bool IsValid(BlockObject blockObject, out string errorMessage)
    {
      errorMessage = null;

      var dynamiteSpec = blockObject.GetComponent<DynamiteSpec>();
      if (dynamiteSpec == null)
        return true;

      foreach (var block in blockObject.PositionedBlocks.GetAllBlocks())
      {
        if (block.MatterBelow != MatterBelow.Ground)
          continue;

        for (int i = 0; i < dynamiteSpec.Depth; i++)
        {
          var terrainCoord = block.Coordinates.Below() - new Vector3Int(0, 0, i);
          foreach (var delta in Deltas.Neighbors4Vector3Int)
          {
            var possiblePlatformCoord = terrainCoord + delta;
            foreach (var obj in _blockService.GetObjectsAt(possiblePlatformCoord))
            {
              if (obj == null || obj.GetComponent<SidePlatform>() == null)
                continue;
              if (obj.TransformCoordinates(new Vector3Int(-1, 0, 0)) == terrainCoord)
              {
                errorMessage = _loc.T(BlockedKey);
                return false;
              }
            }
          }
        }
      }

      return true;
    }
  }
}
