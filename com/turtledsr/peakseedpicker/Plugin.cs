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
    level_12 = 12, level_13 = 13, level_14 = 14, 
    level_15 = 15, level_16 = 16, level_17 = 17,
    level_18 = 18, level_19 = 19, level_20 = 20,
    daily = 21
  }

  public static Version version;

  public static Version v1dot7 = new Version("1.7.a");

  internal static new ManualLogSource Logger;
  internal static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
  public static ConfigEntry<LevelChoice> levelConfig;
  public static LevelChoice level; //0 to 13
  public static Vector2 overlayPos = new Vector2(1710, 10);

  public static bool timerActive;

  private void Awake() {
    Logger = base.Logger;

    levelConfig = Config.Bind("General", "Seed", LevelChoice.daily, "Seed that gets loaded, Versions prior to 1.7.a only have level_0 to level_13, any other value will be counted as daily");
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

    version = new Version(Application.version);
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

    GUI.Label(new Rect(overlayPos.x, overlayPos.y, 200, 30), (version < v1dot7 && (int) level > 13) ? "daily" : level.ToString(), style);
  }

  private static void PatchMapBaker() {
    if(version >= new Version("1.6.a")) {
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
      if(level == LevelChoice.daily || (version < v1dot7 && (int) level > 13)) {
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
      if(level == LevelChoice.daily || (version < v1dot7 && (int) level > 13)) {
        return true;
      }
      Logger.LogInfo("Using PRE-v1.6.x Level-Loader");
      SceneReference[] scenes = (SceneReference[]) __instance.GetType().GetField("AllLevels").GetValue(__instance);
      __result = PathUtil.WithoutExtensions(PathUtil.GetFileName(scenes[(int) level].ScenePath));
      return false;
    }
  }
}
