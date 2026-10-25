using System;
using UnityEditor;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
namespace Mektra.Editor
{
    public static class LeaderboardIntegration
    {
        static bool previousEnabled;
        static EnterPlayModeOptions previousOptions;
        static int result=1;static bool buildAfter=true;
        public static void Probe(){buildAfter=false;ValidateAndBuild();}
        public static void ValidateAndBuild()
        {
            previousEnabled=EditorSettings.enterPlayModeOptionsEnabled;
            previousOptions=EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled=true;
            EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged+=State;
            EditorApplication.EnterPlaymode();
        }
        static void State(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode)Validate();
            if(state==PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.playModeStateChanged-=State;
                EditorSettings.enterPlayModeOptionsEnabled=previousEnabled;EditorSettings.enterPlayModeOptions=previousOptions;
                if(result==0&&buildAfter){try{MektraBuild.BuildPreview();}catch(Exception e){Debug.LogError(e.Message);result=1;}}
                EditorApplication.Exit(result);
            }
        }
        // Read the real board using an isolated identity; never publish fabricated scores.
        static async void Validate()
        {
            try
            {
                await UnityServices.InitializeAsync(new InitializationOptions().SetProfile("mektra-backend-check"));
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                var page=await LeaderboardsService.Instance.GetScoresAsync("mektra_distance",new GetScoresOptions{Limit=5});
                if(page.Results==null)throw new Exception("Leaderboard returned no result collection");
                Debug.Log("MEKTRA_GLOBAL_LEADERBOARD_CONNECTED: production board read successfully; isolated identity retained; no test scores published.");
                result=0;
            }
            catch(Exception e){Debug.LogError("MEKTRA_GLOBAL_LEADERBOARD_CHECK_FAILED: "+e.Message);}
            finally{EditorApplication.ExitPlaymode();}
        }
    }
}
