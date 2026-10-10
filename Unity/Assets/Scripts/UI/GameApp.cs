using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class GameApp:MonoBehaviour
    {
        Simulation sim;
        LevelData level;
        LevelData[] courses;
        Character[] characters;
        ProgressStore store;
        GameAudio gameAudio;
        VisualElement root,scene,overlay;
        WorldView world;
        NativeEffects effects;
        Label distance,score,pickupFeedback,themeName,energyLabel;
        EnergyMeter energyMeter;
        double lastPickup=-10;
        Button pause,mute;
        bool shopOpen,banked;
        Phase shownPhase;
        float viewport;
        bool endless;
        int reviewSection,testWorldOrdinal;
        VisualElement testReveal; double testRevealAt;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Bootstrap()
        {
            if(FindFirstObjectByType<GameApp>()==null)new GameObject("Mektra").AddComponent<GameApp>();
        }
        void Awake()
        {
            Application.targetFrameRate=120;
            QualitySettings.vSyncCount=1;
            Screen.SetResolution(430,900,FullScreenMode.Windowed);
            // One endless journey; mechanics and recovery beats are selected from a tested library.
            var pack=LevelPack.Load();endless=pack.endless;courses=pack.Courses();level=courses[0];
            var args=Environment.GetCommandLineArgs();int review=Array.IndexOf(args,"-mektraReviewSection");
            if(review>=0&&review+1<args.Length)int.TryParse(args[review+1],out reviewSection);
            characters=CharacterCatalog.Load();
            store=new ProgressStore();
            gameAudio=gameObject.AddComponent<GameAudio>();
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
                if(shopOpen||sim==null||e.target is Button)return;
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
        void Replay()=>Reset(true);
        void Reset(bool play,Simulation replay=null)
        {
            shopOpen=false;
            banked=false;
            viewport=0;
            lastPickup=-10;
            testReveal=null;
            sim=replay??new Simulation(level,Current,courses,Math.Max(0,reviewSection),endless,reviewSection>0?17:SectionReplayMode.InitialSeed());
            sim.energyEnabled=true;
            if(reviewSection>0){sim.time=30;play=true;}
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
            world.Refresh();
            effects.Refresh(sim);
        }
        void Hud()
        {
            var hud=new VisualElement();
            hud.style.position=Position.Absolute;
            hud.style.top=8;
            hud.style.left=12;
            hud.style.right=12;
            hud.style.flexDirection=FlexDirection.Row;
            hud.style.alignItems=Align.Center;
            var left=new VisualElement();
            left.style.flexGrow=1;
            distance=UiStyles.Text("0 m",24,null,true);
            distance.style.whiteSpace=WhiteSpace.NoWrap;
            distance.style.flexShrink=0;
            left.Add(distance);
            themeName=UiStyles.Text(ThemeJourney.Names[0],8,UiStyles.Blue);
            themeName.style.whiteSpace=WhiteSpace.Normal;themeName.style.maxWidth=220;themeName.style.fontSize=10;
            themeName.style.display=DisplayStyle.None;left.Add(themeName);
            hud.Add(left);
            var energyColumn=new VisualElement();energyColumn.style.width=156;energyColumn.style.marginRight=8;
            energyLabel=UiStyles.Text("",8,UiStyles.Blue);energyLabel.style.display=DisplayStyle.None;energyColumn.Add(energyLabel);
            energyMeter=new EnergyMeter();energyColumn.Add(energyMeter);hud.Add(energyColumn);
            mute=UiStyles.Button(gameAudio.Enabled?"♪":"×",()=>
            {
                gameAudio.Toggle();mute.text=gameAudio.Enabled?"♪":"×";
            }
            ,true);
            mute.style.width=28;
            mute.style.height=28;
            mute.style.marginTop=0;mute.style.marginBottom=0;
            hud.Add(mute);
            score=UiStyles.Text("✦ "+store.Data.bank,14,null,true);
            score.style.marginLeft=6;
            score.style.marginRight=6;
            hud.Add(score);
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
            hud.Add(pause);
            scene.Add(hud);
            pickupFeedback=UiStyles.Text("+1 STAR",11,UiStyles.Blue,true);
            pickupFeedback.style.position=Position.Absolute;pickupFeedback.style.top=60;
            pickupFeedback.style.width=new Length(100,LengthUnit.Percent);pickupFeedback.style.unityTextAlign=TextAnchor.MiddleCenter;
            pickupFeedback.style.opacity=0;scene.Add(pickupFeedback);
        }
        void Update()
        {
            if(sim!=null)gameAudio.SetPhase(shopOpen?Phase.Ready:sim.phase);
            if(sim==null||shopOpen)return;
            sim.Advance(Time.unscaledDeltaTime);
            if(reviewSection==0&&WorldTestMode.ShouldEnter(sim))
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
                var gameEvent=sim.events.Dequeue();gameAudio.Play(gameEvent.kind,sim.CourseAt(sim.platform.section).segmentKind==17);effects.GameEvent(gameEvent.kind,sim,gameEvent.position);
                if(gameEvent.kind=="star")lastPickup=sim.time;
            }
            float pickupAge=(float)(sim.time-lastPickup);
            pickupFeedback.style.opacity=Mathf.Clamp01(1-pickupAge/.55f);
            pickupFeedback.style.translate=new Translate(0,-Mathf.Min(pickupAge,.55f)*16);
            score.style.scale=new Scale(Vector3.one*(1+Mathf.Sin(Mathf.Clamp01(pickupAge/.25f)*Mathf.PI)*.12f));
            score.style.color=pickupAge<.3f?UiStyles.Blue:UiStyles.Ink;
            if((sim.phase==Phase.Over||sim.phase==Phase.Complete)&&!banked)
            {
                store.Finish(sim.score,sim.Distance);
                banked=true;
            }
            if(sim.phase!=shownPhase)ShowOverlay();
            if(sim.phase==Phase.Playing&&overlay!=null)
            {
                overlay.style.opacity=(float)Math.Clamp(9-sim.RenderTime,0,1);
                if(sim.RenderTime>=9){overlay.RemoveFromHierarchy();overlay=null;}
            }
            distance.text=sim.Distance+" m";
            energyLabel.text=(sim.energy<=20?"LOW ENERGY ":"ENERGY ")+(int)Math.Ceiling(sim.energy)+"%";
            energyMeter.Refresh(sim.energy,sim.RenderTime);
            energyLabel.style.color=sim.energy<=20?new Color(1,.25f,.3f):new Color(.94f,.98f,1);
            themeName.text=sim.phase==Phase.Playing&&sim.lava.Active(sim.time)&&sim.RenderLava-sim.RenderPosition.y<180?"LAVA RISING · KEEP CLIMBING":(sim.Instruction??ThemeJourney.Name(sim.ThemeDistance));
            themeName.style.color=ThemeJourney.Sample(sim.ThemeDistance).accent;
            score.text="✦ "+(sim.phase==Phase.Ready?store.Data.bank:sim.score);
            pause.style.display=sim.phase==Phase.Playing?DisplayStyle.Flex:DisplayStyle.None;
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
            if(sim!=null&&!banked&&sim.score>0)store.Finish(sim.score,sim.Distance);
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
            if(overlay!=null)overlay.RemoveFromHierarchy();
            overlay=null;
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
            overlay.style.backgroundColor=new Color(.025f,.05f,.10f,.72f);
            UiStyles.Center(overlay);
            UiStyles.Pad(overlay,26);
            overlay.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());
            scene.Add(overlay);
            var card=new VisualElement();
            card.style.width=new Length(100,LengthUnit.Percent);
            card.style.maxWidth=370;
            card.style.backgroundColor=VectorArt.ColorOf("#10213A");
            UiStyles.Round(card,32);
            UiStyles.Pad(card,28);
            overlay.Add(card);
            if(sim.phase==Phase.Ready)
            {
                CenterLabel(card,"ONE ENDLESS RUN",9,UiStyles.Muted,true);
                CenterLabel(card,"Make your move",38,null,true);
                CenterLabel(card,"Tap or press Space to launch.\nRead the next landing. Keep ahead of the lava.",14,UiStyles.Muted);
                card.Add(UiStyles.Button("Let’s play",()=>
                {
                    sim.Start();gameAudio.Play("hello");ShowOverlay();
                }
                ));
                card.Add(UiStyles.Button("Characters",OpenShop,true));
                CenterLabel(card,"BEST "+store.Data.best+"m · "+store.Data.bank+" STARS",9,UiStyles.Muted);
            }
            else if(sim.phase==Phase.Paused)
            {
                CenterLabel(card,"TAKE A BREATH",10,UiStyles.Muted,true);
                CenterLabel(card,"Paused",38,null,true);
                CenterLabel(card,"Your orbit is waiting for you.",14,UiStyles.Muted);
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
                CenterLabel(card,sim.reason=="energy"?"Out of energy. Collect the blue blooms.":sim.reason=="lava"?"The lava caught you. Keep climbing.":sim.reason=="black hole"?"The vortex pulled you in. Stay outside its purple ring.":sim.reason=="wall"?"You touched the edge. Find your next opening.":sim.reason=="obstacle"?"You hit an orange hazard. Watch its opening.":"You drifted away. Time your next jump.",14,UiStyles.Muted);
                CenterLabel(card,sim.Distance+" m      ✦ "+sim.score,28,null,true);
                CenterLabel(card,"Best distance · "+store.Data.best+"m",12,UiStyles.Muted);
                card.Add(UiStyles.Button("Play again",Replay));
                card.Add(UiStyles.Button("Back to home",()=>Reset(false),true));
            }
        }
        void BankRun()
        {
            if(!banked)
            {
                store.Finish(sim.score,sim.Distance);
                banked=true;
            }
        }
        void OpenShop()
        {
            shopOpen=true;
            root.Add(new CharacterShop(characters,store,c=>sim.Select(c),()=>Reset(false)));
        }
    }
}
