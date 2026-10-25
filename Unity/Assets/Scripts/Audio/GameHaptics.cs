using UnityEngine;
namespace Mektra
{
    public static class GameHaptics
    {
        public static bool Enabled{get=>PlayerPrefs.GetInt("mektra.haptics",1)==1;set{PlayerPrefs.SetInt("mektra.haptics",value?1:0);PlayerPrefs.Save();}}
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]static extern void MektraHapticImpact();
#endif
        public static void Play(string kind)
        {
            if(!Enabled||(kind!="land"&&kind!="perfect"))return;
#if UNITY_IOS && !UNITY_EDITOR
            MektraHapticImpact();
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
