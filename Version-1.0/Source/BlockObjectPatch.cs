using HarmonyLib;
using Timberborn.BlockSystem;

namespace Tobbert.MorePlatforms
{
  [HarmonyPatch(typeof(BlockObject), nameof(BlockObject.IsAlmostValid))]
  public static class BlockObjectPatch
  {
    public static void Postfix(BlockObject __instance, ref bool __result)
    {
      // If vanilla already considers it almost valid, we don't need to interfere.
      if (__result)
      {
        return;
      }

      // Check if the object being evaluated has our custom SidePlatform component
      SidePlatform sidePlatform = __instance.GetComponent<SidePlatform>();

      if (sidePlatform != null)
      {
        // Force the preview to be "almost valid".
        // This prevents the engine from dropping the preview entirely and 
        // forces it to render the red 'unbuildable' ghost model instead.
        __result = true;
      }
    }
  }
}