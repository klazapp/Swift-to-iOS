#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class SwiftToiOSPostBuilder
{
    private const string MODULE_MAP_FILE_PATH = "Packages/com.klazapp.swifttoios/Editor/UnityFramework.modulemap";

    private const string MODULE_MAP_FILE_NAME = "MODULEMAP_FILE";
    
    private const string MODULE_MAP_FRAMEWORK_PATH = "UnityFramework/UnityFramework.modulemap";
    private const string MODULE_MAP_ROOT_PATH = "$(SRCROOT)/UnityFramework/UnityFramework.modulemap";
    
    private const string UNITY_INTERFACE_PATH = "Classes/Unity/UnityInterface.h";
    private const string UNITY_FORWARD_DECLS_PATH = "Classes/Unity/UnityForwardDecls.h";
    private const string UNITY_RENDERING_PATH = "Classes/Unity/UnityRendering.h";
    private const string UNITY_SHARED_DECLS_PATH = "Classes/Unity/UnitySharedDecls.h";

    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltTarget)
    {
        Debug.Log("SwiftToiOSPostBuilder, On Post Process Build Begun");
        if (target != BuildTarget.iOS) 
            return;

        Debug.Log("SwiftToiOSPostBuilder, Setting Up Swift To Unity Started"); 
        SetUpSwiftToUnity(pathToBuiltTarget);
        Debug.Log("SwiftToiOSPostBuilder, Setting Up Swift To Unity Finished"); 

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetUpSwiftToUnity(string pathToBuiltProject)
    {
        //Get Xcode project path
        var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        
        //Set up new Xcode project, for modification
        var project = new PBXProject();
        
        //Read Xcode project file
        project.ReadFromFile(projectPath);

        //Get Unity Framework Target Guid
        var unityFrameworkGuid = project.GetUnityFrameworkTargetGuid();

        //Set DEFINES_MODULE to YES
        //Target will now define a module
        project.AddBuildProperty(unityFrameworkGuid, "DEFINES_MODULE", "YES");

        //Construct module file path
        var moduleFilePath = pathToBuiltProject + "/" + MODULE_MAP_FRAMEWORK_PATH;
        
        //Check for existing module map file and copy if necessary
        if (!File.Exists(moduleFilePath))
        {
            FileUtil.CopyFileOrDirectory(MODULE_MAP_FILE_PATH, moduleFilePath);
            project.AddFile(moduleFilePath, MODULE_MAP_FRAMEWORK_PATH);
            project.AddBuildProperty(unityFrameworkGuid, MODULE_MAP_FILE_NAME, MODULE_MAP_ROOT_PATH);
        }

        //Add public headers to UnityFramework target
        var unityInterfaceGuid = project.FindFileGuidByProjectPath(UNITY_INTERFACE_PATH);
        project.AddPublicHeaderToBuild(unityFrameworkGuid, unityInterfaceGuid);

        var unityForwardDeclsGuid = project.FindFileGuidByProjectPath(UNITY_FORWARD_DECLS_PATH);
        project.AddPublicHeaderToBuild(unityFrameworkGuid, unityForwardDeclsGuid);

        var unityRenderingGuid = project.FindFileGuidByProjectPath(UNITY_RENDERING_PATH);
        project.AddPublicHeaderToBuild(unityFrameworkGuid, unityRenderingGuid);

        var unitySharedDeclsGuid = project.FindFileGuidByProjectPath(UNITY_SHARED_DECLS_PATH);
        project.AddPublicHeaderToBuild(unityFrameworkGuid, unitySharedDeclsGuid);

        //Save project
        project.WriteToFile(projectPath);
    }
}
#endif
