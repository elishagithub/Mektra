using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Mektra.Editor
{
    public static class MektraAndroidRelease
    {
        public static void Build()=>BuildOutputs(true);
        public static void BuildApk()=>BuildOutputs(false);
        public static void BuildBundle()=>BuildOutputs(true,true);
        static void BuildOutputs(bool includeBundle,bool bundleOnly=false)
        {
            if(Application.cloudProjectId!="71cdb2f4-2bd5-4de6-97fa-fb18e864069a")throw new Exception("Production Unity leaderboard project is not linked");
            var courses=ContinuousJourney.Courses(LevelPack.Load().JourneyCourses());
            ContinuousJourneyRegression.Run();
            PlatformScaleRegression.Run(courses);
            Regression.Run();OrbitDirectionRegression.Run();PrecisionLaneRegression.Run();
            // Energy depletion and magnetic pickup are absent in precision mode.
            var root=Path.GetFullPath("..");
            var signing=Path.Combine(root,"release/signing");
            var output=Path.Combine(root,"release/android");
            Directory.CreateDirectory(output);
            UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library/Android/sdk");
            UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath=Path.Combine(UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath,"ndk/27.2.12479018");
            UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath="/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home";
            PlayerSettings.companyName="Ark Code Solutions";
            PlayerSettings.productName="Mektra";
            PlayerSettings.bundleVersion="1.5.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.arkcodesolutions.mektra");
            PlayerSettings.Android.bundleVersionCode=7;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false;
            PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.SplashScreen.show=false;
            PlayerSettings.Android.useCustomKeystore=true;
            PlayerSettings.Android.keystoreName=Path.Combine(signing,"mektra-upload.p12");
            PlayerSettings.Android.keyaliasName="mektra-upload";
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/MektraIcon.png");
            if(icon==null)throw new Exception("Mektra launcher icon is missing");
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
            PlayerSettings.Android.keystorePass=File.ReadAllText(Path.Combine(signing,"password.txt")).Trim();
            PlayerSettings.Android.keyaliasPass=PlayerSettings.Android.keystorePass;
            try
            {
                foreach(bool bundle in bundleOnly?new[]{true}:includeBundle?new[]{false,true}:new[]{false})
                {
                    EditorUserBuildSettings.buildAppBundle=bundle;
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                        scenes=new[]{"Assets/Scenes/Mektra.unity"},
                        locationPathName=Path.Combine(output,bundle?"Mektra-1.5.0.aab":"Mektra-1.5.0.apk"),
                        target=BuildTarget.Android,options=BuildOptions.None,extraScriptingDefines=Array.Empty<string>()
                    });
                    if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed: "+report.summary.result);
                    Debug.Log("MEKTRA_ANDROID_RELEASE_SUCCESS "+report.summary.outputPath+" "+report.summary.totalSize);
                }
            }
            finally
            {
                PlayerSettings.Android.keystorePass="";
                PlayerSettings.Android.keyaliasPass="";
                AssetDatabase.SaveAssets();
            }
        }
    }
}
