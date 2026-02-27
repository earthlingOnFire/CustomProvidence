using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CustomProvidence;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {	
  public const string PLUGIN_GUID = "com.earthlingOnFire.CustomProvidence";
  public const string PLUGIN_NAME = "CustomProvidence";
  public const string PLUGIN_VERSION = "1.0.0";
  public static ManualLogSource logger;
  public static string modDir;
  public static List<string> FileExtensions = new List<string> {".jpeg", ".jpg", ".png", ".bmp"};
  public static System.Random rand = new System.Random();

  private void Awake() {
    gameObject.hideFlags = HideFlags.HideAndDontSave;
    Plugin.logger = Logger;
  }

  private void Start() {
    string modPath = Assembly.GetExecutingAssembly().Location.ToString();
    modDir = Path.GetDirectoryName(modPath);

    new Harmony(PLUGIN_GUID).PatchAll();
    Plugin.logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
  }
}

[HarmonyPatch]
public static class Patches {
  [HarmonyPostfix]
  [HarmonyPatch(typeof(EnemyIdentifier), "Start")]
  public static void ChangeEye(EnemyIdentifier __instance) {
    if (__instance.enemyType != EnemyType.Providence || __instance.puppet == true) return;

    Texture2D texture = LoadRandomTexture();
    
    if (texture == null) return;

    Transform eye = __instance.gameObject.transform.Find("Providence/Eye");
    Component.Destroy(eye.GetComponent<AnimatedTexture>());
    Component.Destroy(eye.GetComponent<BlinkAnimTex>());

    eye.GetComponent<SkinnedMeshRenderer>().material.mainTexture = texture;
  }

  public static Texture2D LoadRandomTexture() {
    string textureFolder = Path.Combine(Plugin.modDir, "textures");
    string[] texturePaths = GetImagePaths(textureFolder);

    if (texturePaths.Length == 0) {
      Plugin.logger.LogWarning("No images found in textures folder");
      return null;
    }

    int randomIndex = Plugin.rand.Next(0, texturePaths.Length);
    return LoadTextureFromFile(texturePaths[randomIndex]);
  }

  public static string[] GetImagePaths(string dirPath) {
    string[] filePaths = Directory.GetFiles(dirPath);
    return Array.FindAll(filePaths, x => IsImage(x));
  }

  public static bool IsImage(string filePath) {
    foreach (string fileExtension in Plugin.FileExtensions) {
      if (filePath.EndsWith(fileExtension)) return true;
    }
    return false;
  }

  public static Texture2D LoadTextureFromFile(string filePath) {
    byte[] fileData = File.ReadAllBytes(filePath);

    Texture2D tex = new Texture2D(2,2);
    ImageConversion.LoadImage(tex, fileData);

    return tex;
  }
}
