using HarmonyLib;
using Timberborn.BlockSystem;

namespace Tobbert.MorePlatforms
{
  [HarmonyPatch(typeof(BlockObject), nameof(BlockObject.IsValid))]
  public static class BlockObjectIsValidPatch
  {
    public static void Postfix(BlockObject __instance, ref bool __result)
    {
      if (__result)
      {
        return;
      }

      // Force non-preview SidePlatforms to pass load validation
      if (!__instance.IsPreview && __instance.GetComponent<SidePlatform>() != null)
      {
        __result = true;
      }
    }
  }
}