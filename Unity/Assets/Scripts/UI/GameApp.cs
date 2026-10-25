using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class GameApp:MonoBehaviour
    {
        Simulation sim;
        DiscoveryProgress discovery;
        int currentEncounter,discoveryDistance;
        bool IsDiscovery=>discovery!=null;
        int RunDistance=>sim.Distance+(IsDiscovery?discoveryDistance:0);
        LevelData level;
        LevelData[] courses;
        Character[] characters;
        ProgressStore store;
        GameAudio gameAudio;
        RewardedVideo ads;
        GlobalLeaderboard leaderboard;
        int bankedStars;
        Button reviveButton;
        VisualElement root,scene,overlay,journeyPanel;
        WorldView world;
        NativeEffects effects;
        RollingDistance distance;
        Label accuracyAward;int displayedAccuracy,pendingTotal;float awardStarted=-10;Vector2 awardOrigin;
        Label score,pickupFeedback,themeName,streakLabel,gameTitle;
        double lastPickup=-10;
        Button pause,mute;
        bool shopOpen,banked,journeyOpen,settingsOpen;
        System.Action closeSettings;
        Phase shownPhase;
        float viewport;
        bool endless;
        int reviewSection,testWorldOrdinal;
        VisualElement testReveal; double testRevealAt;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Bootstrap()
        {
            if(Environment.GetCommandLineArgs().Contains("-mektraLeaderboardProbe"))return;
            if(Environment.GetCommandLineArgs().Contains("-mektraLevelTour"))
            {
                new GameObject("Mektra level preview").AddComponent<LevelTour>();
                return;
            }
            if(FindFirstObjectByType<GameApp>()==null)new GameObject("Mektra").AddComponent<GameApp>();
        }
        void Awake()
        {
            Application.targetFrameRate=120;
            QualitySettings.vSyncCount=1;
            Screen.SetResolution(430,900,FullScreenMode.Windowed);
            // One endless journey; mechanics and recovery beats are selected from a tested library.
            var pack=LevelPack.Load();endless=pack.endless;courses=pack.JourneyCourses();
            level=courses[0];
            if(level.discovery){courses=ContinuousJourney.Courses(courses);level=courses[0];}
            var args=Environment.GetCommandLineArgs();int review=Array.IndexOf(args,"-mektraReviewSection");
            if(review>=0&&review+1<args.Length)int.TryParse(args[review+1],out reviewSection);
            characters=CharacterCatalog.Load();
            store=new ProgressStore();
            leaderboard=new GlobalLeaderboard(store);
            gameAudio=gameObject.AddComponent<GameAudio>();
            ads=gameObject.AddComponent<RewardedVideo>();
            ads.Changed+=()=>{if(sim!=null&&sim.phase==Phase.Over)ShowOverlay();};
            effects=gameObject.AddComponent<NativeEffects>();
            var settings=ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode=PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution=new Vector2Int(390,816);
            settings.screenMatchMode=PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match=0;
            settings.themeStyleSheet=Resources.Load<ThemeStyleSheet>("MektraTheme");
            var doc=gameObject.AddComponent<UIDocument>();
            doc.panelSettings=settings;
            root=doc.rootVisualElement;
            root.focusable=true;
            root.RegisterCallback<KeyDownEvent>(e=>
            {
                if(settingsOpen){if(e.keyCode==KeyCode.Escape)closeSettings?.Invoke();e.StopPropagation();return;}
                if(!shopOpen&&sim!=null&&(sim.phase==Phase.Ready||sim.phase==Phase.Paused)){
                    if(e.keyCode==KeyCode.C&&sim.phase==Phase.Ready){OpenShop();e.StopPropagation();return;}
                    if(e.keyCode==KeyCode.S){OpenPresetSettings();e.StopPropagation();return;}
                    if(e.keyCode==KeyCode.L&&sim.phase==Phase.Ready){OpenLeaderboard();e.StopPropagation();return;}
                }
                if(shopOpen||sim==null||e.target is Button)return;
                if(journeyOpen){if(e.keyCode==KeyCode.Escape){journeyPanel?.RemoveFromHierarchy();journeyOpen=false;root.Focus();}e.StopPropagation();return;}
                if(e.keyCode==KeyCode.Space||e.keyCode==KeyCode.Return)
                {
                    if(sim.phase==Phase.Ready){sim.Start();gameAudio.Play("hello");ShowOverlay();}
                    else if(sim.phase==Phase.Playing)sim.Jump();
                    e.StopPropagation();e.PreventDefault();
                }
                else if(e.keyCode==KeyCode.Escape)
                {
                    if(sim.phase==Phase.Playing){sim.Pause();gameAudio.Stop();}
                    else if(sim.phase==Phase.Paused)sim.Resume();
                    ShowOverlay();e.StopPropagation();e.PreventDefault();
                }
            });
            root.style.flexGrow=1;
            root.style.backgroundColor=Color.clear;
            root.RegisterCallback<GeometryChangedEvent>(e=>Resize());
            Reset(false);
#if MEKTRA_OPENING_PREVIEW
            overlay.Clear();overlay.style.backgroundColor=Color.clear;
            var begin=UiStyles.Button("Tap to begin",()=>{sim.Start();gameAudio.Play("hello");ShowOverlay();});
            begin.style.position=Position.Absolute;begin.style.bottom=84;begin.style.left=100;begin.style.right=100;overlay.Add(begin);
#endif



#if MEKTRA_GAMEOVER_PREVIEW
            sim.bestY=sim.startY-2730;sim.phase=Phase.Over;ShowOverlay();
#endif
#if MEKTRA_CREW_PREVIEW
            OpenShop();
#endif
        }
        System.Collections.IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-mektraScreenshots");
            if(at<0||at+1>=args.Length)yield break;
            Screen.SetResolution(540,960,FullScreenMode.Windowed);
            string folder=args[at+1];System.IO.Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(1);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"01-home.png"),2);
            yield return new WaitForSecondsRealtime(.4f);
            sim.Start();ShowOverlay();yield return new WaitForSecondsRealtime(.5f);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"02-landscape.png"),2);
            yield return new WaitForSecondsRealtime(.4f);
            ColorPresetPreview.Step(1);yield return new WaitForSecondsRealtime(.5f);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"03-colour-preset.png"),2);
            yield return new WaitForSecondsRealtime(.4f);ColorPresetPreview.Step(-1);
            OpenPresetSettings();yield return new WaitForSecondsRealtime(.4f);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"04-settings.png"),2);
            yield return new WaitForSecondsRealtime(.4f);
            Debug.Log("MEKTRA_SCREENSHOTS_COMPLETE "+folder);
            Reset(false);
        }
        void OpenPresetSettings()
        {
            if(settingsOpen)return;settingsOpen=true;bool resume=sim.phase==Phase.Playing;if(resume)sim.Pause();
            ForestSettings panel=null;closeSettings=()=>{panel?.RemoveFromHierarchy();settingsOpen=false;closeSettings=null;if(resume)sim.Resume();root.Focus();};
            panel=new ForestSettings(gameAudio,()=>closeSettings?.Invoke());root.Add(panel);
        }
        void OpenShare()
        {
            if(shopOpen)return;shopOpen=true;VisualElement share=null;int shareDistance=sim.phase==Phase.Ready?store.Best:RunDistance;
            share=ForestMenus.Share(Current,shareDistance,Math.Max(store.Best,RunDistance),()=>{share.RemoveFromHierarchy();shopOpen=false;root.Focus();});root.Add(share);
        }
        Character Current=>characters.FirstOrDefault(c=>c.id==store.Data.selected)??characters[0];
        void Resize()
        {
            if(sim!=null)gameAudio.SetPhase(shopOpen?Phase.Ready:sim.phase);
            if(sim==null||shopOpen)return;
            float h=root.resolvedStyle.height;
            if(h>0&&Math.Abs(h-viewport)>.1)
            {
                viewport=h;
                sim.SetViewport(h);
            }
        }
        void Replay(){
            ColorPresetPreview.Replay();
            reviewSection=0;discoveryDistance=0;Reset(true);
        }
        void Reset(bool play,Simulation replay=null)
        {
            if(ads!=null&&ads.Showing)return;
            shopOpen=false;journeyOpen=false;journeyPanel=null;settingsOpen=false;closeSettings=null;
            banked=false;
            bankedStars=0;
            viewport=0;
            lastPickup=-10;displayedAccuracy=pendingTotal=0;awardStarted=-10;accuracyAward=null;
            testReveal=null;
            if(IsDiscovery)
            {
                currentEncounter=Math.Clamp(reviewSection>0?reviewSection:discovery.State.current,0,courses.Length-1);
                sim=new Simulation(courses[currentEncounter],Current);
                sim.RestoreCheckpoint(discovery.State.checkpoints[currentEncounter]);
            }
            else sim=replay??new Simulation(level,Current,courses,Math.Max(0,reviewSection),endless,reviewSection>0?17:SectionReplayMode.InitialSeed());
            sim.energyEnabled=false;
            if(reviewSection>0){if(!IsDiscovery)sim.time=30;play=true;}
            root.Clear();
            scene=new VisualElement();
            UiStyles.Absolute(scene);
            root.Add(scene);
            world=new WorldView(sim);
            scene.Add(world);
            scene.RegisterCallback<PointerDownEvent>(e=>
            {
                if(e.button==0)
                {
                    sim.Jump();e.StopPropagation();
                }
            }
            );
            Hud();
            Resize();
            if(play)
            {
                sim.Start();
                gameAudio.Play("hello");
            }
            ShowOverlay();
            root.Focus();
            score.style.color=ColorPresetPreview.HudInk;gameTitle.style.color=ColorPresetPreview.HudInk;
            world.Refresh();
            effects.Refresh(sim);
        }
        void Hud()
        {
            var hud=new VisualElement();
            hud.style.position=Position.Absolute;
            float safeTop=(Screen.height-Screen.safeArea.yMax)*390f/Math.Max(1,Screen.width);
            hud.style.top=Mathf.Max(32,safeTop+10);
            hud.style.left=12;
            hud.style.right=12;
            hud.style.flexDirection=FlexDirection.Row;
            hud.style.alignItems=Align.FlexStart;
            hud.style.justifyContent=Justify.SpaceBetween;
            var left=new VisualElement();
            left.style.minWidth=100;
            left.style.alignItems=Align.Center;
            gameTitle=UiStyles.Text("MEKTRA",12,null,true);
            gameTitle.style.letterSpacing=2;
            gameTitle.style.marginBottom=2;
            gameTitle.style.whiteSpace=WhiteSpace.NoWrap;
            left.Add(gameTitle);
            distance=new RollingDistance();
            distance.style.whiteSpace=WhiteSpace.NoWrap;
            distance.style.flexShrink=0;
            left.Add(distance);
            streakLabel=UiStyles.Text("",9,null,true);
            streakLabel.style.letterSpacing=1;streakLabel.style.height=16;
            left.Add(streakLabel);
            themeName=UiStyles.Text(ThemeJourney.Names[0],8,UiStyles.Blue);
            themeName.style.whiteSpace=WhiteSpace.Normal;themeName.style.maxWidth=220;themeName.style.fontSize=10;
            themeName.style.display=DisplayStyle.None;left.Add(themeName);
            hud.Add(left);
            var right=new VisualElement();right.style.alignItems=Align.FlexEnd;
            var controls=new VisualElement();controls.style.flexDirection=FlexDirection.Row;controls.style.alignItems=Align.Center;
            right.Add(controls);hud.Add(right);
            mute=UiStyles.Button(gameAudio.Enabled?"♪":"×",()=>
            {
                gameAudio.Toggle();mute.text=gameAudio.Enabled?"♪":"×";
            }
            ,true);
            mute.style.width=28;
            mute.style.height=28;
            mute.style.marginTop=0;mute.style.marginBottom=0;
            controls.Add(mute);
            var starBalance=new VisualElement();starBalance.style.flexDirection=FlexDirection.Row;starBalance.style.alignItems=Align.Center;starBalance.style.marginLeft=6;
            starBalance.Add(new FourPointStar());
            score=UiStyles.Text(store.Data.bank.ToString(),14,null,true);
            score.style.marginLeft=6;
            score.style.marginRight=6;
            starBalance.Add(score);controls.Add(starBalance);
            pause=UiStyles.Button("Ⅱ",()=>
            {
                if(sim.phase==Phase.Playing)
                {
                    sim.Pause();gameAudio.Stop();ShowOverlay();
                }
            }
            ,true);
            pause.style.height=28;
            pause.style.marginTop=0;pause.style.marginBottom=0;
            pause.style.width=28;
            controls.Add(pause);
            scene.Add(hud);
            pickupFeedback=UiStyles.Text("+1 STAR",11,UiStyles.Blue,true);
            pickupFeedback.style.position=Position.Absolute;pickupFeedback.style.top=Mathf.Max(32,safeTop+10)+48;
            pickupFeedback.style.width=new Length(100,LengthUnit.Percent);pickupFeedback.style.unityTextAlign=TextAnchor.MiddleCenter;
            pickupFeedback.style.opacity=0;scene.Add(pickupFeedback);
        }
        void Update()
        {
            if(sim!=null)gameAudio.SetPhase(shopOpen?Phase.Ready:sim.phase);
            if(sim==null||shopOpen)return;
            if(IsDiscovery&&sim.phase==Phase.Playing)discovery.Tick(Time.unscaledDeltaTime);
            sim.Advance(Time.unscaledDeltaTime);
            if(IsDiscovery&&sim.phase==Phase.Playing&&sim.CheckpointIndex>discovery.State.checkpoints[currentEncounter])
            {
                discovery.State.SetCheckpoint(currentEncounter,sim.CheckpointIndex);discovery.Save();
            }
            if(!IsDiscovery&&reviewSection==0&&WorldTestMode.ShouldEnter(sim))
            {
                sim=WorldTestMode.Enter(sim,testWorldOrdinal++);
                world.RemoveFromHierarchy();world=new WorldView(sim);scene.Insert(0,world);
                testReveal=new VisualElement();UiStyles.Absolute(testReveal);testReveal.pickingMode=PickingMode.Ignore;
                testReveal.style.backgroundColor=UiStyles.Background;scene.Insert(1,testReveal);testRevealAt=sim.time;
                if(overlay!=null){overlay.RemoveFromHierarchy();overlay=null;}
            }
            if(testReveal!=null)
            {
                testReveal.style.opacity=(float)Math.Clamp(1-(sim.RenderTime-testRevealAt)/.65,0,1);
                if(sim.time-testRevealAt>=.65){testReveal.RemoveFromHierarchy();testReveal=null;}
            }
            while(sim.events.Count>0)
            {
                var gameEvent=sim.events.Dequeue();GameHaptics.Play(gameEvent.kind);gameAudio.Play(gameEvent.kind,sim.CourseAt(sim.platform.section).segmentKind==17,gameEvent.streak);effects.GameEvent(gameEvent.kind,sim,gameEvent.position);
                if(gameEvent.kind=="switch"){pickupFeedback.text="GATE OPEN";lastPickup=sim.time;gameAudio.Play("land");}
                if(gameEvent.kind=="star"){pickupFeedback.text="+1 FIREFLY";lastPickup=sim.time;}
                if(gameEvent.kind=="checkpoint"){pickupFeedback.text="CHECKPOINT "+(sim.Chapter+1)+" / "+sim.level.checkpointPlatforms.Length;lastPickup=sim.time;gameAudio.Play("land");}
            }
            float pickupAge=(float)(sim.time-lastPickup);
            pickupFeedback.style.opacity=Mathf.Clamp01(1-pickupAge/(pickupFeedback.text.StartsWith("CHECKPOINT")?2:.55f));
            pickupFeedback.style.translate=new Translate(0,-Mathf.Min(pickupAge,.55f)*16);
            score.style.scale=new Scale(Vector3.one*(1+Mathf.Sin(Mathf.Clamp01(pickupAge/.25f)*Mathf.PI)*.12f));
            score.style.color=ColorPresetPreview.HudInk;
            gameTitle.style.color=ColorPresetPreview.HudInk;
            streakLabel.style.color=ColorPresetPreview.HudInk;
            streakLabel.text=world.PerfectStreak>1?world.PerfectStreak+" PERFECT":"";
            if(accuracyAward!=null)accuracyAward.style.color=ColorPresetPreview.HudInk;
            if((sim.phase==Phase.Over||sim.phase==Phase.Complete)&&!banked)
            {
                store.Finish(Math.Max(0,sim.score-bankedStars),RunDistance);
                if(IsDiscovery)
                {
                    discovery.State.best=Math.Max(discovery.State.best,RunDistance);
                    if(sim.phase==Phase.Complete)discovery.State.Complete(currentEncounter);
                    else discovery.State.Fail(currentEncounter);
                    discovery.Save();
                }
                bankedStars=sim.score;
                banked=true;
                leaderboard.SubmitBest();
            }
            if(sim.phase!=shownPhase)ShowOverlay();
            if(sim.phase==Phase.Playing&&overlay!=null)
            {
                overlay.style.opacity=(float)Math.Clamp(9-sim.RenderTime,0,1);
                if(sim.RenderTime>=9){overlay.RemoveFromHierarchy();overlay=null;}
            }
#if MEKTRA_INVERT_PREVIEW
            if(sim.precisionScore>pendingTotal){int gain=sim.precisionScore-pendingTotal;pendingTotal=sim.precisionScore;awardStarted=Time.unscaledTime;awardOrigin=new Vector2((float)sim.RenderPosition.x,(float)(sim.RenderPosition.y-sim.RenderCamera));if(accuracyAward==null){accuracyAward=UiStyles.Text("",18,new Color(0.9466f,0.9466f,0.9466f),true);accuracyAward.style.position=Position.Absolute;accuracyAward.pickingMode=PickingMode.Ignore;root.Add(accuracyAward);}accuracyAward.text="+"+gain;}
            float awardAge=Time.unscaledTime-awardStarted;float u=Mathf.Clamp01(awardAge/1.5f);u=1-Mathf.Pow(1-u,3);
            if(accuracyAward!=null){var at=awardOrigin+new Vector2(0,-42*u);accuracyAward.style.left=at.x;accuracyAward.style.top=at.y;accuracyAward.style.opacity=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((awardAge-.45f)/1.05f));}
            displayedAccuracy=pendingTotal;
            distance.style.scale=new Scale(Vector3.one*(1+Mathf.Sin(Mathf.Clamp01(awardAge/.25f)*Mathf.PI)*.15f));distance.Refresh(displayedAccuracy);
#else
            distance.Refresh(RunDistance);
#endif
            if(IsDiscovery)gameTitle.text=sim.level.checkpointPlatforms!=null&&sim.level.checkpointPlatforms.Length>0?"LEVEL "+(currentEncounter+1)+" · PART "+(sim.Chapter+1)+" / "+sim.level.checkpointPlatforms.Length:"LEVEL "+(currentEncounter+1)+" / "+courses.Length;
            themeName.text=sim.LavaEnabled&&sim.phase==Phase.Playing&&sim.lava.Active(sim.time)&&sim.RenderLava-sim.RenderPosition.y<180?"LAVA RISING · KEEP CLIMBING":(sim.Instruction??ThemeJourney.Name((sim.startY-sim.bestY)/10));
            themeName.style.color=ThemeJourney.Sample((sim.startY-sim.bestY)/10).accent;
            score.text=(store.Data.bank+Math.Max(0,sim.score-bankedStars)).ToString();
            pause.style.display=sim.phase==Phase.Playing?DisplayStyle.Flex:DisplayStyle.None;
            score.style.color=ColorPresetPreview.HudInk;gameTitle.style.color=ColorPresetPreview.HudInk;
            world.Refresh();
            effects.Refresh(sim);
        }
        void OnApplicationFocus(bool focused)
        {
            if(!focused&&sim!=null)
            {
                sim.Pause();
                if(gameAudio!=null)gameAudio.Stop();
            }
        }
        void OnApplicationPause(bool paused)
        {
            if(paused&&sim!=null)
            {
                sim.Pause();
                gameAudio.Stop();
            }
        }
        void OnApplicationQuit()
        {
            if(IsDiscovery)discovery.Save();
            if(sim!=null&&!banked&&sim.score>0)store.Finish(Math.Max(0,sim.score-bankedStars),RunDistance);
        }
        void ChooseEncounter(int index,bool play,bool fromStart=false)
        {
            BankRun();discoveryDistance=0;reviewSection=0;
            if(fromStart)discovery.State.SetCheckpoint(index,0);
            discovery.State.Select(index);discovery.Save();Reset(play);
        }
        void OpenDiscoveryJourney()
        {
            if(journeyOpen)return;journeyOpen=true;
            var panel=new VisualElement();UiStyles.Absolute(panel);panel.style.backgroundColor=new Color(.03f,.03f,.03f,.98f);
            panel.style.paddingTop=70;panel.style.paddingLeft=26;panel.style.paddingRight=26;root.Add(panel);journeyPanel=panel;
            panel.Add(UiStyles.Text("YOUR JOURNEY",22,Color.white,true));
            var list=new ScrollView();list.style.flexGrow=1;list.style.minHeight=0;panel.Add(list);
            for(int i=0;i<courses.Length;i++)
            {
                int selected=i;bool unlocked=i<=discovery.State.unlocked;
                string prefix=discovery.State.Completed(i)?"✓ ":"";
                string checkpoint=discovery.State.checkpoints[i]>0?" · checkpoint":"";
                var button=UiStyles.Button(prefix+(i+1)+". "+courses[i].name+checkpoint+(unlocked?"":" · soon"),()=>ChooseEncounter(selected,false),true);
                button.SetEnabled(unlocked);list.Add(button);
            }
            panel.Add(UiStyles.Button("Back",()=>{panel.RemoveFromHierarchy();journeyOpen=false;journeyPanel=null;root.Focus();}));
        }
        void ShowDiscoveryOverlay()
        {
            if(overlay!=null)overlay.RemoveFromHierarchy();overlay=null;
            if(sim.phase==Phase.Playing||sim.phase==Phase.Exploding)return;
            overlay=new VisualElement();UiStyles.Absolute(overlay);UiStyles.Center(overlay);UiStyles.Pad(overlay,26);
            overlay.style.backgroundColor=new Color(0,0,0,.72f);overlay.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());scene.Add(overlay);
            var card=new VisualElement();card.style.width=new Length(100,LengthUnit.Percent);card.style.maxWidth=360;
            card.style.backgroundColor=new Color(.07f,.07f,.07f,.98f);UiStyles.Round(card,26);UiStyles.Pad(card,24);overlay.Add(card);
            CenterLabel(card,"LEVEL "+(currentEncounter+1)+" / "+courses.Length,10,UiStyles.Muted,true);
            if(sim.phase==Phase.Ready)
            {
                CenterLabel(card,courses[currentEncounter].name,28,null,true);
                CenterLabel(card,sim.Instruction,14,UiStyles.Muted);
                if(sim.level.chapterTitles!=null&&sim.level.chapterTitles.Length>sim.Chapter)CenterLabel(card,"PART "+(sim.Chapter+1)+" / "+sim.level.checkpointPlatforms.Length+" · "+sim.level.chapterTitles[sim.Chapter],12,Color.white);
                CenterLabel(card,"Tap or press Space to jump. Your checkpoints are saved.",11,UiStyles.Muted);
                card.Add(UiStyles.Button("Play",()=>{sim.Start();gameAudio.Play("hello");ShowOverlay();root.Focus();}));
            }
            else if(sim.phase==Phase.Complete)
            {
                CenterLabel(card,currentEncounter==courses.Length-1?"Journey complete":"Level complete",28,null,true);
                CenterLabel(card,courses[currentEncounter].name,14,UiStyles.Muted);
                CenterLabel(card,"RUN DISTANCE "+RunDistance,14,Color.white,true);
                if(currentEncounter+1<courses.Length)card.Add(UiStyles.Button("Next level",()=>
                {
                    discoveryDistance=RunDistance;discovery.State.Select(currentEncounter+1);discovery.Save();reviewSection=0;Reset(false);
                }));
                else CenterLabel(card,"You completed all "+courses.Length+" levels. Replay your favourites from Journey.",13,UiStyles.Muted);
                card.Add(UiStyles.Button("Play again",()=>ChooseEncounter(currentEncounter,true,true),true));
                SocialActions(card);
                card.Add(UiStyles.Button("Home",()=>Reset(false),true));
            }
            else if(sim.phase==Phase.Paused)
            {
                CenterLabel(card,"Take a breath",28,null,true);
                card.Add(UiStyles.Button("Continue",()=>{sim.Resume();ShowOverlay();root.Focus();}));
                card.Add(UiStyles.Button(sim.CheckpointIndex>0?"Retry from checkpoint":"Retry level",()=>ChooseEncounter(currentEncounter,true),true));
            }
            else
            {
                CenterLabel(card,sim.CheckpointIndex>0?"Your checkpoint is safe":"Try the opening again",28,null,true);
                CenterLabel(card,sim.reason=="black hole"?"The vortex captured you. Stay outside its ring.":sim.reason=="lava"?"The lava caught up. Your next attempt starts here.":"That jump missed. Try again when the angle is right.",14,UiStyles.Muted);
                CenterLabel(card,"RUN DISTANCE "+RunDistance,14,Color.white,true);
                card.Add(UiStyles.Button(sim.CheckpointIndex>0?"Retry from checkpoint":"Retry level",()=>ChooseEncounter(currentEncounter,true)));
                if(discovery.State.unlocked>currentEncounter)card.Add(UiStyles.Button("Next level",()=>ChooseEncounter(currentEncounter+1,false),true));
            }
            if(sim.CheckpointIndex>0&&sim.phase!=Phase.Complete)card.Add(UiStyles.Button("Restart level from beginning",()=>ChooseEncounter(currentEncounter,true,true),true));
            card.Add(UiStyles.Button("Journey",OpenDiscoveryJourney,true));
        }
        void CenterLabel(VisualElement card,string text,int size,Color? color=null,bool bold=false)
        {
            var l=UiStyles.Text(text,size,color,bold);
            l.style.unityTextAlign=TextAnchor.MiddleCenter;
            l.style.marginBottom=12;
            card.Add(l);
        }
        void ShowOverlay()
        {
            shownPhase=sim.phase;
            if(IsDiscovery&&sim.phase==Phase.Complete){ShowDiscoveryOverlay();return;}
            if(overlay!=null)overlay.RemoveFromHierarchy();
            overlay=null;
            if(sim.phase==Phase.Ready){overlay=ForestMenus.Home(Current,()=>{sim.Start();gameAudio.Play("hello");ShowOverlay();root.Focus();},OpenShop,OpenLeaderboard,OpenShare,OpenPresetSettings);scene.Add(overlay);return;}
            if(sim.phase==Phase.Over){overlay=ForestMenus.Results(Current,RunDistance,Math.Max(store.Best,RunDistance),sim.jumpCount,Replay,()=>Reset(false),OpenShare);scene.Add(overlay);return;}
            if(sim.phase==Phase.Paused){overlay=ForestMenus.Pause(Current,()=>{sim.Resume();ShowOverlay();root.Focus();},()=>{BankRun();Replay();},OpenPresetSettings,()=>{BankRun();Reset(false);});scene.Add(overlay);return;}
            if(sim.phase==Phase.Playing||sim.phase==Phase.Exploding)
            {
                if(sim.phase==Phase.Playing&&sim.time<9)
                {
                    overlay=UiStyles.Text("WAIT FOR YOUR OPENING · TAP TO JUMP",9,UiStyles.Muted,true);
                    overlay.style.position=Position.Absolute;
                    overlay.style.bottom=32;
                    overlay.style.width=new Length(100,LengthUnit.Percent);
                    overlay.style.unityTextAlign=TextAnchor.MiddleCenter;
                    scene.Add(overlay);
                }
                return;
            }
            overlay=new VisualElement();
            UiStyles.Absolute(overlay);
            overlay.style.backgroundColor=new Color(.005f,.018f,.025f,.38f);
            UiStyles.Center(overlay);
            UiStyles.Pad(overlay,26);
            overlay.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());
            scene.Add(overlay);
            var card=new VisualElement();
            card.style.width=new Length(100,LengthUnit.Percent);
            card.style.maxWidth=370;
            card.style.backgroundColor=new Color(.025f,.055f,.065f,.95f);
            UiStyles.Round(card,32);
            UiStyles.Pad(card,28);
            if(sim.phase==Phase.Over||sim.phase==Phase.Ready)
            {
                var frame=new VisualElement();frame.style.width=new Length(100,LengthUnit.Percent);frame.style.maxWidth=370;
                frame.Add(card);overlay.Add(frame);
                UiStyles.Border(card,new Color(0.6522f,0.6522f,0.6522f,.25f),1);
            }
            else overlay.Add(card);
            if(sim.phase==Phase.Ready)
            {
                CenterLabel(card,"ONE CONTINUOUS JOURNEY",9,UiStyles.Muted,true);
                CenterLabel(card,"MEKTRA",38,null,true);
                if(IsDiscovery)CenterLabel(card,courses[currentEncounter].name,15,UiStyles.Muted,true);
                CenterLabel(card,"Leap through the night forest.\nCatch fireflies and climb higher.",14,UiStyles.Muted);
                if(IsDiscovery&&sim.CheckpointIndex>0)CenterLabel(card,"Continue from your saved checkpoint.",11,UiStyles.Blue);
                card.Add(UiStyles.Button("Let’s play",()=>
                {
                    sim.Start();gameAudio.Play("hello");ShowOverlay();root.Focus();
                }
                ));
                card.Add(UiStyles.Button("Characters",OpenShop,true));
                card.Add(UiStyles.Button("Global leaderboard",OpenLeaderboard,true));
                if(IsDiscovery)card.Add(UiStyles.Button("Levels",OpenDiscoveryJourney,true));
                card.Add(UiStyles.Button("Settings",OpenPresetSettings,true));
                CenterLabel(card,"BEST DISTANCE "+store.Best+" · "+store.Data.bank+" FIREFLIES",9,UiStyles.Muted);
            }
            else if(sim.phase==Phase.Complete)
            {
                CenterLabel(card,"THE WHOLE JOURNEY",10,UiStyles.Blue,true);
                CenterLabel(card,"Beautifully done",34,null,true);
                CenterLabel(card,"You made it from the first jump to the final shape.",14,UiStyles.Muted);
                CenterLabel(card,RunDistance+" DISTANCE",28,Color.white,true);
                SocialActions(card);
                card.Add(IconButton("Play again","replay",Replay));
                card.Add(IconButton("Home","home",()=>Reset(false),true));
            }
            else if(sim.phase==Phase.Paused)
            {
                CenterLabel(card,"TAKE A BREATH",10,UiStyles.Muted,true);
                CenterLabel(card,"Paused",38,null,true);
                CenterLabel(card,"Your orbit is waiting for you.",14,UiStyles.Muted);
                card.Add(UiStyles.Button("Settings",OpenPresetSettings,true));
                card.Add(UiStyles.Button("Characters",OpenShop,true));
                SocialActions(card);
                card.Add(UiStyles.Button("Continue",()=>
                {
                    sim.Resume();ShowOverlay();
                }
                ));
                card.Add(UiStyles.Button("Restart",()=>
                {
                    BankRun();Replay();
                }
                ,true));
                card.Add(UiStyles.Button("Home",()=>
                {
                    BankRun();Reset(false);
                }
                ,true));
            }
            else
            {
                CenterLabel(card,"ONE MORE ORBIT",10,UiStyles.Muted,true);
                CenterLabel(card,"Game over",38,null,true);
                CenterLabel(card,sim.reason=="lava"?"The lava caught you. Keep climbing.":sim.reason=="black hole"?"The vortex pulled you in. Stay outside its ring.":sim.reason=="wall"?"You touched the edge. Find your next opening.":sim.reason=="obstacle"?"You hit a hazard. Watch its opening.":"You drifted away. Time your next jump.",14,UiStyles.Muted);
                var result=UiStyles.Text(RunDistance.ToString(),62,Color.white,true);result.style.unityTextAlign=TextAnchor.MiddleCenter;
                result.style.marginTop=4;result.style.marginBottom=12;result.style.textShadow=new TextShadow{offset=Vector2.zero,blurRadius=10,color=new Color(0.6260f,0.6260f,0.6260f,.32f)};card.Add(result);
                var high=UiStyles.Text("BEST DISTANCE",10,UiStyles.Muted,true);high.style.letterSpacing=3.5f;high.style.unityTextAlign=TextAnchor.MiddleCenter;card.Add(high);
                var best=UiStyles.Text(Math.Max(store.Best,RunDistance).ToString(),23,UiStyles.Ink,true);best.style.unityTextAlign=TextAnchor.MiddleCenter;best.style.marginTop=5;best.style.marginBottom=14;card.Add(best);
                var wallet=new VisualElement();wallet.style.flexDirection=FlexDirection.Row;wallet.style.justifyContent=Justify.Center;wallet.style.alignItems=Align.Center;wallet.style.marginBottom=12;
                wallet.Add(new FourPointStar());wallet.Add(UiStyles.Text((store.Data.bank+Math.Max(0,sim.score-bankedStars)).ToString(),20,Color.white,true));card.Add(wallet);
                SocialActions(card);
                if(sim.CanRevive&&ads.Supported)
                {
                    reviveButton=UiStyles.Button(!ads.Allowed?"Watch video · Revive once":ads.Ready?"Watch video · Revive once":ads.Loading?"Loading revive video…":"Revive video unavailable",()=>
                    {
                        if(!ads.Allowed){ShowAdsDisclosure(card);return;}
                        var run=sim;
                        ads.Show(()=>
                        {
                            if(sim!=run||!sim.Revive())return;
                            banked=false;ShowOverlay();world.Refresh();effects.Refresh(sim);
                        });
                    });
                    reviveButton.SetEnabled(!ads.Showing&&(!ads.Allowed||ads.Ready));card.Add(reviveButton);
                    CenterLabel(card,"Complete the video to return to a safe shape.",11,UiStyles.Muted);
                }
                card.Add(UiStyles.Button("Characters",OpenShop,true));
                card.Add(IconButton(IsDiscovery&&sim.CheckpointIndex>0?"Retry from checkpoint":"Play again","replay",Replay));
                card.Add(IconButton("Back to home","home",()=>Reset(false),true));
            }
        }
        void ShowAdsDisclosure(VisualElement card)
        {
            card.Clear();CenterLabel(card,"Optional revive videos",26,null,true);
            CenterLabel(card,"Unity loads videos using device identifiers, IP address and ad interaction data. We request contextual ads with personalization disabled. Playing without videos is always available.",14,UiStyles.Muted);
            card.Add(UiStyles.Button("Privacy policy",()=>Application.OpenURL("https://elishagithub.github.io/Mektra/privacy/"),true));
            card.Add(UiStyles.Button("Enable videos",()=>{ads.AllowVideos();ShowOverlay();}));
            card.Add(UiStyles.Button("Not now",ShowOverlay,true));
        }
        Button IconButton(string title,string icon,Action click,bool secondary=false)
        {
            var button=UiStyles.Button("",click,secondary);button.style.flexDirection=FlexDirection.Row;button.style.justifyContent=Justify.Center;button.style.alignItems=Align.Center;
            var color=secondary?UiStyles.Ink:UiStyles.Background;button.Add(new ActionIcon(icon,color));button.Add(UiStyles.Text(title,secondary?12:14,color,true));return button;
        }
        void SocialActions(VisualElement card)
        {
            var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.marginTop=8;row.style.marginBottom=8;
            var share=IconButton("Share climb","share",()=>
            {
                if(!ScoreShare.Share(RunDistance,Math.Max(store.Best,RunDistance)))
                {
                    var note=UiStyles.Text("Score copied — paste it in your favourite app.",11,UiStyles.Blue);
                    card.Add(note);note.schedule.Execute(()=>note.RemoveFromHierarchy()).StartingIn(3500);
                }
            },true);
            share.style.flexGrow=1;share.style.fontSize=12;share.style.marginRight=4;
            var ranks=IconButton("Leaderboard","rank",OpenLeaderboard,true);ranks.style.flexGrow=1;ranks.style.fontSize=12;ranks.style.marginLeft=4;
            row.Add(share);row.Add(ranks);card.Add(row);
        }
        void OpenLeaderboard()
        {
            shopOpen=true;
            root.Add(leaderboard.View(()=>{shopOpen=false;ShowOverlay();}));
        }
        void BankRun()
        {
            if(!banked)
            {
                store.Finish(Math.Max(0,sim.score-bankedStars),RunDistance);
                bankedStars=sim.score;
                banked=true;
            }
        }
        void OpenShop()
        {
            shopOpen=true;
            CharacterShop view=null;
            view=new CharacterShop(characters,store,c=>sim.Select(c),()=>{view.RemoveFromHierarchy();shopOpen=false;ShowOverlay();root.Focus();});
            root.Add(view);view.Focus();
        }
    }
}
