using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Zorro.Core;
using Zorro.Core.Editor; 

namespace peakseedpicker;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
  internal static new ManualLogSource Logger;
  internal static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
  public static ConfigEntry<int> levelNumConfig;
  public static int levelNum; //0 to 13
  public static Vector2 overlayPos = new Vector2(1710, 10);

  public static int versionSmall;

  private void Awake() {
    Logger = base.Logger;

    levelNumConfig = Config.Bind("General", "Seed", -1, "Seed that gets loaded. (-1 = daily, 0 to 13 for valid levels)");
    levelNum = levelNumConfig.Value;

    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
  }
  
  private void Start() {
    if(Chainloader.PluginInfos.ContainsKey("com.turtledsr.peaktimer")) {
      Logger.LogInfo("PeakTimer Found. Moving UI down");
      overlayPos.y = 40;
    }

    versionSmall = Convert.ToInt32(Application.version.TrimStart('.').Split(".")[1]);
    PatchMapBaker();

    Logger.LogInfo($"Using version: {Application.version}");
  }

  private void OnGUI() {
    GUIStyle style = new() {
      fontSize = 30,
      fontStyle = FontStyle.Bold,
    };
    style.normal.textColor = Color.white;

    GUI.Label(new Rect(overlayPos.x, overlayPos.y, 200, 30),(levelNum >= 0 && levelNum <= 13) ? $"level_{levelNum}" : "Daily", style);
  }

  private static void PatchMapBaker() {
    if(versionSmall >= 6) {
      Logger.LogInfo("Patched for 1.6.x+");
      harmony.PatchAll(typeof(MapBaker_GetLevel_v1dot6_plus_Patch));
    } else {
      Logger.LogInfo("Patched for PRE-1.6.x");
      harmony.PatchAll(typeof(MapBaker_GetLevel_PRE_v1dot6_Patch));
    }
  }

  [HarmonyPatch(typeof(MapBaker), "GetLevel")]
  static class MapBaker_GetLevel_v1dot6_plus_Patch {
    static bool Prefix(MapBaker __instance, ref string __result) {
      if(levelNum == -1) {
        return true;
      }
      Logger.LogInfo("Using v1.6.x+ Level-Loader");
      __result = PathUtil.WithoutExtensions(PathUtil.GetFileName(__instance.AllLevels[levelNum % __instance.AllLevels.Length]));
      return false;
    }
  }

  [HarmonyPatch(typeof(MapBaker), "GetLevel")]
  static class MapBaker_GetLevel_PRE_v1dot6_Patch {
    static bool Prefix(MapBaker __instance, ref string __result) {
      if(levelNum == -1) {
        return true;
      }
      Logger.LogInfo("Using PRE-v1.6.x Level-Loader");
      SceneReference[] scenes = (SceneReference[]) __instance.GetType().GetField("AllLevels").GetValue(__instance);
      __result = PathUtil.WithoutExtensions(PathUtil.GetFileName(scenes[levelNum % scenes.Length].ScenePath));
      return false;
    }
  }
}
