using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace CustomProvidence;

public static class TextureManager {
  public static Texture2D[] textures = new Texture2D[0];
  public static List<string> FileExtensions = new List<string> {".jpeg", ".jpg", ".png", ".bmp"};
  public static System.Random rand = new System.Random();

  public static void ReloadTextures() {
    string[] texturePaths = GetImagePaths(Plugin.textureFolder);
    textures = new Texture2D[texturePaths.Length];

    for (int i = 0; i < texturePaths.Length; i++) {
      textures[i] = LoadTextureFromFile(texturePaths[i]);
    }
  }

  public static Texture2D GetRandomTexture() {
    int randomIndex = Plugin.rand.Next(0, textures.Length);
    return textures[0];
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

