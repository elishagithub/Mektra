using UnityEngine;
namespace Mektra
{
    public static class ScoreShare
    {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("MektraShare")]
        static extern int MektraShareText(string text);
#endif
        public static string Message(int score,int best)=>"I climbed to "+score+" in Mektra! My best: "+best+". Can you beat it?\nhttps://play.google.com/store/apps/details?id=com.arkcodesolutions.mektra";
        public static bool Share(int score,int best)
        {
            var text=Message(score,best);
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity=unity.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent=new AndroidJavaObject("android.content.Intent","android.intent.action.SEND");
                intent.Call<AndroidJavaObject>("setType","text/plain");
                intent.Call<AndroidJavaObject>("putExtra","android.intent.extra.TEXT",text);
                using var cls=new AndroidJavaClass("android.content.Intent");
                using var chooser=cls.CallStatic<AndroidJavaObject>("createChooser",intent,"Share your climb");
                activity.Call("startActivity",chooser);return true;
            }
            catch(System.Exception){GUIUtility.systemCopyBuffer=text;return false;}
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            try{return MektraShareText(text)!=0;}
            catch(System.Exception){GUIUtility.systemCopyBuffer=text;return false;}
#else
            GUIUtility.systemCopyBuffer=text;return false;
#endif
        }
    }
}
