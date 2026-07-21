using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace Tobbert.MorePlatforms
{
  public class SidePlatformValidator : IBlockObjectValidator
  {
    private readonly IBlockService _blockService;
    private readonly ITerrainService _terrainService;
    private readonly EntityRegistry _entityRegistry;

    public SidePlatformValidator(
        IBlockService blockService,
        ITerrainService terrainService,
        EntityRegistry entityRegistry)
    {
      _blockService = blockService;
      _terrainService = terrainService;
      _entityRegistry = entityRegistry;
    }

    public bool IsValid(BlockObject blockObject, out string errorMessage)
    {
      errorMessage = null;

      // Only validate our specific side platforms
      SidePlatform sidePlatform = blockObject.GetComponent<SidePlatform>();
      if (sidePlatform == null)
      {
        return true;
      }

      // The platform extends in the +X direction. 
      // Therefore, its anchor attachment point is directly adjacent at local X = -1.
      Vector3Int localAttachmentCoords = new Vector3Int(-1, 0, 0);

      // This safely translates the local coordinate into world space, 
      // perfectly accounting for the player rotating or flipping the blueprint.
      Vector3Int worldAttachmentCoords = blockObject.TransformCoordinates(localAttachmentCoords);

      if (IsValidAttachmentTarget(worldAttachmentCoords, blockObject.IsPreview))
      {
        return true;
      }

      errorMessage = "Must be attached to a solid wall or building.";
      return false;
    }

    private bool IsValidAttachmentTarget(Vector3Int coordinates, bool isPreview)
    {
      // 1. Check if the adjacent block is solid terrain
      if (_terrainService.Underground(coordinates))
      {
        return true;
      }

      // 2. Check if the adjacent block is a valid building in BlockService
      // A valid attachment target must have structural body (Top, Corners, or Middle).
      // Excludes paths, thin floors, and air blocks.
      var objectsAt = _blockService.GetObjectsAt(coordinates);
      for (int i = 0; i < objectsAt.Count; i++)
      {
        BlockObject obj = objectsAt[i];
        if (obj == null || obj.GetComponent<SidePlatform>() != null)
          continue;
        if (obj.PositionedBlocks != null && obj.PositionedBlocks.TryGetBlock(coordinates, out Block block))
        {
          if (IsStructuralBlock(block))
          {
            return true;
          }
        }
      }

      // If we are currently placing/previewing, all existing world objects are already in BlockService.
      // Skip the expensive entity scan during placement to maintain high performance.
      if (isPreview)
      {
        return false;
      }

      // 3. Fallback for Save Loading: Check instantiated BlockObjects that haven't been added to BlockService yet
      var allEntities = _entityRegistry.Entities;
      for (int i = 0; i < allEntities.Count; i++)
      {
        EntityComponent entity = allEntities[i];
        if (entity != null && entity.Enabled)
        {
          BlockObject obj = entity.GetComponent<BlockObject>();
          if (obj != null && obj.GetComponent<SidePlatform>() == null && obj.Positioned && obj.PositionedBlocks != null && obj.PositionedBlocks.TryGetBlock(coordinates, out Block block))
          {
            if (IsStructuralBlock(block))
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    private static bool IsStructuralBlock(Block block)
    {
      if (!block.IsOccupied)
        return false;

      if (block.Stackable != BlockStackable.None)
        return true;

      if ((block.Occupation & BlockOccupations.Floor) != BlockOccupations.None)
        return false;

      return (block.Occupation & (BlockOccupations.Top | BlockOccupations.Corners | BlockOccupations.Middle)) != BlockOccupations.None;
    }
  }
}