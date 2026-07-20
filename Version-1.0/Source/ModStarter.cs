using HarmonyLib;
using UnityEngine;
using Timberborn.ModManagerScene;

namespace Tobbert.MorePlatforms
{
  public class ModStarter : IModStarter
  {
    public void StartMod(IModEnvironment modEnvironment)
    {

      var harmony = new Harmony("tobbert.moreplatforms");
      harmony.PatchAll();
      Debug.Log("[MorePlatforms] Harmony patching complete.");
    }
  }
}
