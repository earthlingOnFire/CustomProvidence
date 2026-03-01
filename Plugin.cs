using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Configgy;

namespace CustomProvidence;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin {	
  public const string PLUGIN_GUID = "com.earthlingOnFire.CustomProvidence";
  public const string PLUGIN_NAME = "CustomProvidence";
  public const string PLUGIN_VERSION = "1.1.0";

  public static ManualLogSource logger;
  public static ConfigBuilder config;
  public static System.Random rand = new System.Random();

  public static string modAppdata;
  public static string textureFolder;
  public static string rareTextureFolder;
  public static string modDir;
  public static List<string> FileExtensions = new List<string> {".jpeg", ".jpg", ".png", ".bmp"};

  public static Texture2D[] textures;

	[Configgable("", "Enabled")]
	public static ConfigToggle EnabledToggle = new ConfigToggle(true);

	[Configgable("", "Reload Textures")]
	public static ConfigButton ReloadTexturesButton = new ConfigButton(delegate {
	     TextureManager.ReloadTextures();
	 });

	[Configgable("", "Open Textures Folder")]
	public static ConfigButton OpenTexturesFolderButton = new ConfigButton(delegate {
      Application.OpenURL(modAppdata);
  });

	[Configgable("", "Rare Texture Chance", 0, "Chance to use a texture from 'rare textures' folder.")]
	public static IntegerSlider RareTextureChanceSlider = new IntegerSlider(0, 0, 100);

  private void Awake() {
    gameObject.hideFlags = HideFlags.HideAndDontSave;
    Plugin.logger = Logger;
  }

  private void Start() {
    string modPath = Assembly.GetExecutingAssembly().Location.ToString();
    modDir = Path.GetDirectoryName(modPath);

    string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    modAppdata = Path.Combine(appdata, "CustomProvidence");
    textureFolder = Path.Combine(modAppdata, "textures");
    rareTextureFolder = Path.Combine(modAppdata, "rare textures");

    CreateFolders();
    TextureManager.ReloadTextures();

    new ConfigBuilder(PLUGIN_GUID, PLUGIN_NAME).BuildAll(); 
    new Harmony(PLUGIN_GUID).PatchAll();
    logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
  }

  public static void CreateFolders() {
    if (!Directory.Exists(modAppdata)) {
      Directory.CreateDirectory(modAppdata);
    }

    if (!Directory.Exists(textureFolder)) {
      Directory.CreateDirectory(textureFolder);
      File.Copy(Path.Combine(modDir, "shittyprovidence.png"),
          Path.Combine(textureFolder, "shittyprovidence.png"));
    }

    if (!Directory.Exists(rareTextureFolder)) {
      Directory.CreateDirectory(rareTextureFolder);
      File.Copy(Path.Combine(modDir, "rareshittyprovidence.png"),
          Path.Combine(rareTextureFolder, "rareshittyprovidence.png"));
    }
  }
}

[HarmonyPatch]
public static class Patches {
  [HarmonyPostfix]
  [HarmonyPatch(typeof(EnemyIdentifier), "Start")]
  public static void ChangeEye(EnemyIdentifier __instance) {
    if (Plugin.EnabledToggle.Value == false
        || __instance.enemyType != EnemyType.Providence
        || __instance.puppet == true) return;

    Texture2D texture = TextureManager.GetRandomTexture();
    
    if (texture == null) return;

    Transform eye = __instance.gameObject.transform.Find("Providence/Eye");
    Component.Destroy(eye.GetComponent<AnimatedTexture>());
    Component.Destroy(eye.GetComponent<BlinkAnimTex>());

    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture("_MainTex", texture);
    eye.GetComponent<SkinnedMeshRenderer>().SetPropertyBlock(propertyBlock);
  }
}
