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
  public enum LevelChoice {
    level_0 = 0, level_1 = 1, level_2 = 2,
    level_3 = 3, level_4 = 4, level_5 = 5,
    level_6 = 6, level_7 = 7, level_8 = 8,
    level_9 = 9, level_10 = 10, level_11 = 11,
    level_12 = 12, level_13 = 13, daily = 14
  }

  internal static new ManualLogSource Logger;
  internal static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
  public static ConfigEntry<LevelChoice> levelConfig;
  public static LevelChoice level; //0 to 13
  public static Vector2 overlayPos = new Vector2(1710, 10);

  public static bool timerActive;

  public static int versionSmall;

  private void Awake() {
    Logger = base.Logger;

    levelConfig = Config.Bind("General", "Seed", LevelChoice.daily, "Seed that gets loaded");
    level = levelConfig.Value;

    levelConfig.SettingChanged += delegate { //update value when config changes
      level = levelConfig.Value;
    };

    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
  }
  
  private void Start() {
    if(Chainloader.PluginInfos.ContainsKey("com.turtledsr.peaktimer")) {
      Logger.LogInfo("PeakTimer Found. Moving UI");
      overlayPos = new Vector2(1710, 40);
      timerActive = true;
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

    if(!timerActive) style.alignment = TextAnchor.UpperRight;

    style.normal.textColor = Color.white;

    GUI.Label(new Rect(overlayPos.x, overlayPos.y, 200, 30),(level != LevelChoice.daily) ? $"level_{(int) level}" : "Daily", style);
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
      if(level == LevelChoice.daily) {
        return true;
      }
      Logger.LogInfo("Using v1.6.x+ Level-Loader");
      __result = PathUtil.WithoutExtensions(PathUtil.GetFileName(__instance.AllLevels[(int) level]));
      return false;
    }
  }

  [HarmonyPatch(typeof(MapBaker), "GetLevel")]
  static class MapBaker_GetLevel_PRE_v1dot6_Patch {
    static bool Prefix(MapBaker __instance, ref string __result) {
      if(level == LevelChoice.daily) {
        return true;
      }
      Logger.LogInfo("Using PRE-v1.6.x Level-Loader");
      SceneReference[] scenes = (SceneReference[]) __instance.GetType().GetField("AllLevels").GetValue(__instance);
      __result = PathUtil.WithoutExtensions(PathUtil.GetFileName(scenes[(int) level].ScenePath));
      return false;
    }
  }
}
