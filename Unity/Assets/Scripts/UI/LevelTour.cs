using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    // Opt-in layout viewer: no gameplay ticks, player, score, lava, or saved progress.
    public sealed class LevelTour:MonoBehaviour
    {
        LevelData[] courses,journey;
        Character character;
        Simulation sim;
        WorldView world;
        VisualElement root;
        Label caption;
        Button autoButton;
        SliderInt sectionPicker;
        double[] offsets,tops,bottoms;
        double camera,firstCamera,lastCamera;
        int section=-1;
        bool automatic=true;
        float viewport=816;
        void Awake()
        {
            Application.targetFrameRate=60;
            Screen.SetResolution(430,900,FullScreenMode.Windowed);
            var gameCamera=Camera.main;
            if(gameCamera==null)gameCamera=FindFirstObjectByType<Camera>();
            gameCamera.clearFlags=CameraClearFlags.SolidColor;gameCamera.backgroundColor=Color.black;
            courses=LevelPack.Load().JourneyCourses();character=CharacterCatalog.Load()[0];
            var reference=new Simulation(courses[0],character,courses,0,true,17);
            journey=new LevelData[courses.Length];
            offsets=new double[courses.Length];tops=new double[courses.Length];bottoms=new double[courses.Length];
            for(int i=0;i<courses.Length;i++)
            {
                offsets[i]=reference.SectionOffset(i);tops[i]=double.PositiveInfinity;bottoms[i]=double.NegativeInfinity;
                journey[i]=reference.CourseAt(i);
                foreach(var shape in journey[i].platforms)
                {
                    tops[i]=Math.Min(tops[i],shape.bounds.minY+offsets[i]);
                    bottoms[i]=Math.Max(bottoms[i],shape.bounds.maxY+offsets[i]);
                }
            }
            var settings=ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode=PanelScaleMode.ScaleWithScreenSize;settings.referenceResolution=new Vector2Int(390,816);
            settings.screenMatchMode=PanelScreenMatchMode.MatchWidthOrHeight;settings.match=0;
            settings.themeStyleSheet=Resources.Load<ThemeStyleSheet>("MektraTheme");
            var doc=gameObject.AddComponent<UIDocument>();doc.panelSettings=settings;root=doc.rootVisualElement;
            root.style.flexGrow=1;root.style.backgroundColor=Color.black;root.focusable=true;
            root.RegisterCallback<GeometryChangedEvent>(e=>{if(root.resolvedStyle.height>0)viewport=root.resolvedStyle.height;});
            root.RegisterCallback<WheelEvent>(e=>{automatic=false;Move(-e.delta.y*35);e.StopPropagation();});
            root.RegisterCallback<KeyDownEvent>(e=>
            {
                if(e.keyCode==KeyCode.Space)automatic=!automatic;
                else if(e.keyCode==KeyCode.RightArrow)JumpTo(section+1);
                else if(e.keyCode==KeyCode.LeftArrow)JumpTo(section-1);
                else if(e.keyCode==KeyCode.UpArrow){automatic=false;Move(-100);}
                else if(e.keyCode==KeyCode.DownArrow){automatic=false;Move(100);}
                else if(e.keyCode==KeyCode.PageUp){automatic=false;Move(-viewport*.8);}
                else if(e.keyCode==KeyCode.PageDown){automatic=false;Move(viewport*.8);}
                else if(e.keyCode==KeyCode.Home)JumpTo(0);
                else if(e.keyCode==KeyCode.End)JumpTo(courses.Length-1);
                else return;
                e.StopPropagation();
            });
            var header=new VisualElement();header.style.position=Position.Absolute;header.style.top=25;header.style.left=12;header.style.right=12;
            header.style.paddingLeft=10;header.style.paddingRight=10;header.style.paddingTop=7;header.style.paddingBottom=7;
            header.style.backgroundColor=new Color(0,0,0,.9f);
            header.Add(UiStyles.Text("LEVEL PREVIEW",12,Color.white,true));
            caption=UiStyles.Text("",11,new Color(.7f,.7f,.7f));header.Add(caption);
            sectionPicker=new SliderInt(1,courses.Length);sectionPicker.RegisterValueChangedCallback(e=>JumpTo(e.newValue-1));header.Add(sectionPicker);
            root.Add(header);
            var footer=new VisualElement();footer.style.position=Position.Absolute;footer.style.bottom=12;footer.style.left=12;footer.style.right=12;
            footer.style.backgroundColor=new Color(0,0,0,.9f);
            var buttons=new VisualElement();buttons.style.flexDirection=FlexDirection.Row;
            var previous=UiStyles.Button("Previous",()=>JumpTo(section-1));
            autoButton=UiStyles.Button("Pause scroll",()=>{automatic=!automatic;root.Focus();});
            var next=UiStyles.Button("Next",()=>JumpTo(section+1));
            foreach(var button in new[]{previous,autoButton,next}){button.style.flexGrow=1;button.style.fontSize=11;buttons.Add(button);}
            footer.Add(buttons);footer.Add(UiStyles.Text("SCROLL / ↑ ↓   ·   ← → SECTIONS   ·   SPACE PAUSE",9,new Color(.6f,.6f,.6f)));
            root.Add(footer);
            firstCamera=bottoms[0]-viewport*.8;lastCamera=tops[courses.Length-1]-120;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-mektraLevelTour"),initial=0;
            if(at>=0&&at+1<args.Length)int.TryParse(args[at+1],out initial);
            JumpTo(initial);root.Focus();
        }
        void JumpTo(int index)
        {
            index=Math.Clamp(index,0,courses.Length-1);
            camera=Math.Clamp(bottoms[index]-viewport*.8,lastCamera,firstCamera);
            Show(index);root.Focus();
        }
        void Move(double delta)
        {
            camera=Math.Clamp(camera+delta,lastCamera,firstCamera);
            int visible=section;
            double anchor=camera+viewport*.75;
            for(int i=0;i<courses.Length;i++)if(tops[i]<=anchor&&bottoms[i]>=anchor){visible=i;break;}
            if(visible!=section)Show(visible);
        }
        void Show(int index)
        {
            section=index;
            if(world!=null)world.RemoveFromHierarchy();
            sim=new Simulation(courses[0],character,courses,index,true,17);sim.energyEnabled=false;sim.SetViewport(viewport);
            world=new WorldView(sim,false);root.Insert(0,world);
            caption.text="SECTION "+(index+1)+" / "+courses.Length+" · "+journey[index].name;
            sectionPicker.SetValueWithoutNotify(index+1);
        }
        void Update()
        {
            if(sim==null)return;
            if(automatic){Move(-180*Math.Min(Time.unscaledDeltaTime,.05));if(camera<=lastCamera)automatic=false;}
            sim.SetPreviewCamera(camera);world.Refresh();
            autoButton.text=automatic?"Pause scroll":"Auto scroll";
        }
    }
}
