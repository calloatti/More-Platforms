using HarmonyLib;
using UnityEngine;
using Timberborn.ModManagerScene;

namespace Tobbert.MorePlatforms
{
  public class ModStarter : IModStarter
  {
    public void StartMod(IModEnvironment modEnvironment)
    {

      new Harmony("Tobbert.MorePlatforms").PatchAll();
      Debug.Log("[MorePlatforms] Harmony patching complete.");
    }
  }
}
