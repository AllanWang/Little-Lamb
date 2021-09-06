using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildScript
{
  [MenuItem("Build/Build All")]
  public static void BuildAll()
  {
    // BuildWindowsServer();
    BuildWindowsClient();
    BuildOSXClient();
  }

  [MenuItem("Build/Build Server (Windows)")]
  public static void BuildWindowsServer()
  {
    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    buildPlayerOptions.scenes = new[] { "Assets/LittleLamb/Scenes/SampleScene.unity" };
    buildPlayerOptions.locationPathName = "Builds/Windows/Server.exe";
    buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
    buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;

    Console.WriteLine("Building Server (Windows)");
    BuildPipeline.BuildPlayer(buildPlayerOptions);
    Console.WriteLine("Built Server (Windows)");
  }

  [MenuItem("Build/Build Client (Windows)")]
  public static void BuildWindowsClient()
  {
    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    buildPlayerOptions.scenes = new[] { "Assets/LittleLamb/Scenes/SampleScene.unity" };
    buildPlayerOptions.locationPathName = "Builds/Windows/Client.exe";
    buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
    buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

    Console.WriteLine("Building Client (Windows)");
    BuildPipeline.BuildPlayer(buildPlayerOptions);
    Console.WriteLine("Built Client (Windows)");
  }

  [MenuItem("Build/Build Client (OSX)")]
  public static void BuildOSXClient()
  {
    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    buildPlayerOptions.scenes = new[] { "Assets/LittleLamb/Scenes/SampleScene.unity" };
    buildPlayerOptions.locationPathName = "Builds/OSX/Client.app";
    buildPlayerOptions.target = BuildTarget.StandaloneOSX;
    buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

    Console.WriteLine("Building Client (OSX)");
    BuildPipeline.BuildPlayer(buildPlayerOptions);
    Console.WriteLine("Built Client (OSX)");
  }
}
