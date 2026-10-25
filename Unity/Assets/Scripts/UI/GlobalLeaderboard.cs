using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public sealed class GlobalLeaderboard
    {
        const string Board="mektra_distance";
        readonly ProgressStore store;
        Task connecting;
        bool submitting;
        public GlobalLeaderboard(ProgressStore store){this.store=store;}
        async Task Connect()
        {
            if(connecting!=null){await connecting;return;}
            connecting=ConnectCore();
            try{await connecting;}finally{connecting=null;}
        }
        async Task ConnectCore()
        {
            if(string.IsNullOrEmpty(Application.cloudProjectId))throw new InvalidOperationException("Leaderboard setup incomplete: no Unity cloud project is linked.");
            if(UnityServices.State!=ServicesInitializationState.Initialized)await UnityServices.InitializeAsync();
            if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        public async void SubmitBest()
        {
            if(string.IsNullOrEmpty(store.Data.nickname)||submitting)return;
            submitting=true;
            try{await Connect();await LeaderboardsService.Instance.AddPlayerScoreAsync(Board,store.Best);}
            catch(Exception e){Debug.LogWarning("Mektra leaderboard submission failed: "+e.Message);}
            finally{submitting=false;}
        }
        public VisualElement View(Action close)
        {
            var screen=new VisualElement();UiStyles.Absolute(screen);screen.style.backgroundColor=Color.clear;UiStyles.Pad(screen,24);ForestUi.Background(screen);
            var eyebrow=ForestUi.Text("WORLD RANKING",11,3);eyebrow.style.marginTop=40;screen.Add(eyebrow);
            var title=ForestUi.Text("RANKS",28,8);title.style.marginTop=20;title.style.marginBottom=24;screen.Add(title);
            var status=UiStyles.Text("Your best journey, among players worldwide.",12,UiStyles.Muted);status.style.marginBottom=16;screen.Add(status);
            var body=new VisualElement();body.style.flexGrow=1;screen.Add(body);
            var back=ForestUi.Button("BACK",()=>{screen.RemoveFromHierarchy();close();},true);back.style.height=54;back.style.marginTop=12;screen.Add(back);
            bool loading=false,privacyOpen=false;
            async Task Refresh()
            {
                if(loading||screen.panel==null)return;loading=true;status.text="Updating world rankings…";
                try
                {
                    await Connect();
                    bool joined=!string.IsNullOrEmpty(store.Data.nickname);
                    if(joined)await LeaderboardsService.Instance.AddPlayerScoreAsync(Board,store.Best);
                    var scores=await LeaderboardsService.Instance.GetScoresAsync(Board,new GetScoresOptions{Limit=25});
                    var own=joined?await LeaderboardsService.Instance.GetPlayerScoreAsync(Board):null;
                    if(screen.panel==null||privacyOpen)return;
                    body.Clear();
                    if(own!=null){var summary=UiStyles.Text("YOUR RANK  #"+(own.Rank+1)+"   ·   BEST  "+(int)own.Score,15,Color.white,true);summary.style.marginBottom=14;body.Add(summary);}
                    var list=new ScrollView(ScrollViewMode.Vertical);list.verticalScrollerVisibility=ScrollerVisibility.Hidden;list.horizontalScrollerVisibility=ScrollerVisibility.Hidden;list.style.flexGrow=1;body.Add(list);
                    foreach(var entry in scores.Results)
                    {
                        var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.alignItems=Align.Center;row.style.height=48;row.style.borderBottomWidth=1;row.style.borderBottomColor=new Color(1.0000f,1.0000f,1.0000f,.06f);
                        bool me=entry.PlayerId==AuthenticationService.Instance.PlayerId;
                        row.style.backgroundColor=me?new Color(0.4043f,0.4043f,0.4043f,.18f):Color.clear;
                        var rank=UiStyles.Text((entry.Rank+1).ToString(),13,me?UiStyles.Blue:UiStyles.Muted,true);rank.style.width=34;row.Add(rank);
                        var name=UiStyles.Text(string.IsNullOrEmpty(entry.PlayerName)?"Climber":entry.PlayerName,13,null,me);name.style.flexGrow=1;name.style.whiteSpace=WhiteSpace.NoWrap;name.style.overflow=Overflow.Hidden;name.style.textOverflow=TextOverflow.Ellipsis;row.Add(name);
                        row.Add(UiStyles.Text(((int)entry.Score).ToString(),17,Color.white,true));list.Add(row);
                    }
                    status.text="Live global scores · updated "+DateTime.Now.ToString("HH:mm");
                }
                catch(Exception e){Debug.LogWarning("Mektra leaderboard load failed: "+e.Message);if(screen.panel!=null)status.text=string.IsNullOrEmpty(Application.cloudProjectId)?"World rankings are not configured yet. Your score is saved on this device.":"Can’t connect right now. Your best score is saved. Tap refresh to retry.";}
                finally{loading=false;}
            }
            if(string.IsNullOrEmpty(store.Data.nickname))
            {
                var joinForm=new VisualElement();screen.Insert(screen.IndexOf(body),joinForm);
                joinForm.Add(UiStyles.Text("Choose your nickname",22,null,true));
                var input=new TextField();input.maxLength=16;input.style.height=48;input.style.marginTop=16;input.style.fontSize=18;input.style.color=Color.black;input.style.backgroundColor=Color.white;joinForm.Add(input);
                var textInput=input.Q("unity-text-input");if(textInput!=null){textInput.style.color=Color.black;textInput.style.backgroundColor=Color.white;}
                joinForm.Add(UiStyles.Text("Use 3–16 letters, numbers or underscores. Your nickname and best score will be public. Unity stores a player ID to keep your ranking.",12,UiStyles.Muted));
                var join=UiStyles.Button("Join the global board",async ()=>
                {
                    string nickname=input.value.Trim();
                    if(nickname.Length<3||!nickname.All(c=>char.IsLetterOrDigit(c)||c=='_')){status.text="Choose 3–16 letters, numbers or underscores.";return;}
                    input.SetEnabled(false);status.text="Creating your climber profile…";
                    try
                    {
                        await Connect();await AuthenticationService.Instance.UpdatePlayerNameAsync(nickname);
                        store.Data.nickname=nickname;store.Data.leaderboardPlayerId=AuthenticationService.Instance.PlayerId;store.Save();joinForm.RemoveFromHierarchy();await Refresh();
                    }
                    catch(Exception e){Debug.LogWarning("Mektra leaderboard profile failed: "+e.GetType().Name+": "+e.Message);status.text="Can’t join right now. "+e.Message;input.SetEnabled(true);}
                });joinForm.Add(join);
            }
            screen.RegisterCallback<AttachToPanelEvent>(async _=>await Refresh());
            screen.Insert(screen.childCount-1,UiStyles.Button("Refresh rankings",async ()=>{privacyOpen=false;await Refresh();},true));
            var privacy=UiStyles.Button("Profile & privacy",()=>
            {
                privacyOpen=true;body.Clear();body.Add(UiStyles.Text("Your climber profile",24,null,true));
                body.Add(UiStyles.Text(string.IsNullOrEmpty(store.Data.nickname)?"You haven’t joined yet.":store.Data.nickname,16,null,true));
                body.Add(UiStyles.Text("Your public nickname and best score are stored with Unity. Your local stars and characters stay on this device.",13,UiStyles.Muted));
                body.Add(UiStyles.Button("Request profile removal",()=>Application.OpenURL("mailto:sureshc5551@gmail.com?subject=Mektra%20leaderboard%20removal&body="+Uri.EscapeDataString("Please remove my Mektra leaderboard profile and player account. Player ID: "+store.Data.leaderboardPlayerId)),true));
                body.Add(UiStyles.Button("Privacy policy",()=>Application.OpenURL("https://elishagithub.github.io/Mektra/privacy/"),true));
                status.text="Removal requests cover your ranking and Unity player account.";
            },true);
            screen.Insert(screen.childCount-1,privacy);
            screen.schedule.Execute(async ()=>{if(!privacyOpen)await Refresh();}).Every(15000);
            return screen;
        }
    }
}
