using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Zorro.Core.Editor;

namespace peakseedpicker;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
  internal static new ManualLogSource Logger;
  
  public static ConfigEntry<int> levelNumConfig;
  public static int levelNum; //0 to 13

  private void Awake() {
    Logger = base.Logger;

    levelNumConfig = Config.Bind("General", "Seed", -1, "Seed that gets loaded. (-1 = daily, 0 to 13 for valid levels)");
    levelNum = levelNumConfig.Value;
    
    Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    harmony.PatchAll();

    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
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
