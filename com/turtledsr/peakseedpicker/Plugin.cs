using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Zorro.Core.Editor;

namespace peakseedpicker;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
  internal static new ManualLogSource Logger;
  public static ConfigEntry<int> levelNumConfig;
  public static int levelNum; //0 to 13
  public static Vector2 overlayPos = new Vector2(1710, 10);

  private void Awake() {
    Logger = base.Logger;

    levelNumConfig = Config.Bind("General", "Seed", -1, "Seed that gets loaded. (-1 = daily, 0 to 13 for valid levels)");
    levelNum = levelNumConfig.Value;
    
    Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    harmony.PatchAll();

    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
  }
  
  private void Start() {
    if(Chainloader.PluginInfos.ContainsKey("com.turtledsr.peaktimer")) {
      Logger.LogInfo("PeakTimer Found. Moving UI down");
      overlayPos.y = 40;
    }
  }

  private void OnGUI() {
    GUIStyle style = new() {
      fontSize = 30,
      fontStyle = FontStyle.Bold,
    };
    style.normal.textColor = Color.white;

    GUI.Label(new Rect(overlayPos.x, overlayPos.y, 200, 30),(levelNum >= 0 && levelNum <= 13) ? $"level_{levelNum}" : "Daily", style);
  }

  [HarmonyPatch(typeof(MapBaker), "GetLevel")]
  static class MapBaker_GetLevel_Patch {
    static bool Prefix(MapBaker __instance, ref string __result) {
      if(!(levelNum >= 0 && levelNum <= 13)) {
        return true;
      } else if(__instance?.AllLevels == null || __instance.AllLevels.Length == 0) {
        return true;
      } else {
		    __result = PathUtil.WithoutExtensions(PathUtil.GetFileName(__instance.AllLevels[levelNum % __instance.AllLevels.Length]));
        return false;
      }
    }
  }
}
