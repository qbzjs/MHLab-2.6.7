using UnityEditor;
using System.Collections.Generic;

public class BuildScript
{
    static string[] GetEnabledScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        List<string> enabledScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }
        return enabledScenes.ToArray();
    }

    [MenuItem("Custom/Build/Windows")]
    static void BuildWindows()
    {
        string projectName = "Terron"; // Set the name of your project here
        string outputPath = $"Builds/Windows64/{projectName}64.exe";
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = outputPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Player;
        
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "WINDOWS64");
        
        EditorUserBuildSettings.development = false;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Custom/Build/Linux Server")]
    static void BuildLinuxServer()
    {
        string projectName = "Terron"; // Set the name of your project here
        string outputPath = $"Builds/Linux64DedicatedServer/{projectName}64DedicatedServer.x86_64";
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = outputPath;
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup Server, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup Server, "DEDICATED_SERVER");
        
        EditorUserBuildSettings.development = false;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
