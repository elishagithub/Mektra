using System;
using UnityEngine;
using Unity.Services.LevelPlay;

namespace Mektra
{
    [Serializable] public class AdsConfiguration
    {
        public string androidAppKey;
        public string rewardedAdUnitId;
    }

    // Gameplay never depends on ad fill. Only the SDK reward event grants a revive.
    public sealed class RewardedVideo:MonoBehaviour
    {
        public event Action Changed;
        LevelPlayRewardedAd rewarded;
        AdsConfiguration configuration;
        Action grant;
        bool initialized,consented,closed,rewardGranted;
        float retryAt;
        public bool Showing {get;private set;}
        public bool Loading {get;private set;}
        public bool Allowed=>consented;
        public bool Supported=>Application.platform==RuntimePlatform.Android&&configuration!=null&&
            !string.IsNullOrWhiteSpace(configuration.androidAppKey)&&!string.IsNullOrWhiteSpace(configuration.rewardedAdUnitId);
        public bool Ready=>rewarded!=null&&rewarded.IsAdReady()&&!Showing;

        void Awake()
        {
            var asset=Resources.Load<TextAsset>("ads-configuration");
            if(asset!=null)configuration=JsonUtility.FromJson<AdsConfiguration>(asset.text);
        }
        public void AllowVideos()
        {
            if(consented||!Supported)return;
            consented=true;
            Loading=true;
            // Contextual ads: do not request personalized advertising or sale of data.
            LevelPlay.SetConsent(false);
            LevelPlay.SetMetaData("do_not_sell","true");
            LevelPlay.OnInitSuccess+=Initialized;
            LevelPlay.OnInitFailed+=InitializationFailed;
            LevelPlay.Init(configuration.androidAppKey);
            Changed?.Invoke();
        }
        void Initialized(LevelPlayConfiguration _)
        {
            initialized=true;
            rewarded=new LevelPlayRewardedAd(configuration.rewardedAdUnitId);
            rewarded.OnAdLoaded+=_=>{Loading=false;Changed?.Invoke();};
            rewarded.OnAdLoadFailed+=_=>{Loading=false;retryAt=Time.unscaledTime+30;Changed?.Invoke();};
            rewarded.OnAdDisplayFailed+=(_,__)=>{Showing=false;grant=null;retryAt=Time.unscaledTime+10;Changed?.Invoke();};
            rewarded.OnAdRewarded+=(_,__)=>
            {
                if(grant==null||rewardGranted)return;
                rewardGranted=true;
                if(closed)Grant();
            };
            rewarded.OnAdClosed+=_=>
            {
                closed=true;Showing=false;
                if(rewardGranted)Grant();
                retryAt=Time.unscaledTime+1;Changed?.Invoke();
            };
            rewarded.LoadAd();
        }
        void Grant(){var callback=grant;grant=null;callback?.Invoke();}
        void InitializationFailed(LevelPlayInitError _){Loading=false;retryAt=Time.unscaledTime+30;Changed?.Invoke();}
        public bool Show(Action completed)
        {
            if(!Ready)return false;
            grant=completed;closed=false;rewardGranted=false;Showing=true;
            AudioListener.pause=true;rewarded.ShowAd();Changed?.Invoke();return true;
        }
        void Update()
        {
            if(!Showing)AudioListener.pause=false;
            if(retryAt<=0||Time.unscaledTime<retryAt||Showing)return;
            retryAt=0;
            Loading=true;
            Changed?.Invoke();
            if(initialized)rewarded.LoadAd();
            else if(consented)LevelPlay.Init(configuration.androidAppKey);
        }
        void OnDestroy()
        {
            LevelPlay.OnInitSuccess-=Initialized;LevelPlay.OnInitFailed-=InitializationFailed;
            rewarded?.DestroyAd();AudioListener.pause=false;
        }
    }
}
