using System.Collections.Immutable;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Coordinates;
using UnityEngine;

namespace Tobbert.MorePlatforms
{
  public class SidePlatformSupportBlocker : BaseComponent, IBlockObjectDeletionBlocker
  {
    private readonly IBlockService _blockService;
    private BlockObject _blockObject;

    public bool NoForcedDelete => false;
    public bool IsStackedDeletionBlocked => false;

    // This is called by the game's demolition tool when hovering over the building
    public bool IsDeletionBlocked => HasAttachedSidePlatform();

    // The tooltip message shown to the player when they try to delete the supporting wall
    public string ReasonLocKey => "Tobbert.DeletionBlocker.SidePlatformAttached";

    public SidePlatformSupportBlocker(IBlockService blockService)
    {
      _blockService = blockService;
    }

    public void Awake()
    {
      _blockObject = GetComponent<BlockObject>();
    }

    private bool HasAttachedSidePlatform()
    {
      if (_blockObject == null)
      {
        _blockObject = GetComponent<BlockObject>();
      }

      if (_blockObject == null)
      {
        return false;
      }

      // Iterate through all blocks occupied by the building the player is trying to delete
      ImmutableArray<Block>.Enumerator enumerator = _blockObject.PositionedBlocks.GetAllBlocks().GetEnumerator();
      while (enumerator.MoveNext())
      {
        Block block = enumerator.Current;

        // Check the 4 adjacent horizontal neighbors of this block
        Vector3Int[] neighbors = Deltas.Neighbors4Vector3Int;
        for (int i = 0; i < neighbors.Length; i++)
        {
          Vector3Int neighborCoords = block.Coordinates + neighbors[i];

          // Look for any objects in the neighboring voxel
          ReadOnlyList<BlockObject> objectsAt = _blockService.GetObjectsAt(neighborCoords);
          for (int j = 0; j < objectsAt.Count; j++)
          {
            BlockObject neighborObj = objectsAt[j];
            if (neighborObj == null)
            {
              continue;
            }

            // If the neighbor is one of your SidePlatforms
            if (neighborObj.TryGetComponent(out SidePlatform sidePlatform))
            {
              // Calculate where this specific platform is anchored
              Vector3Int localAnchor = new Vector3Int(-1, 0, 0);
              Vector3Int worldAnchor = neighborObj.TransformCoordinates(localAnchor);

              // If its anchor perfectly matches the block we are trying to delete, block the deletion
              if (worldAnchor == block.Coordinates)
              {
                return true;
              }
            }
          }
        }
      }

      return false;
    }
  }
}