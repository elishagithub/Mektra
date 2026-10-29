using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public sealed class ForestSettings:VisualElement
    {
        public ForestSettings(GameAudio audio,Action close)
        {
            UiStyles.Absolute(this);ForestUi.Background(this);ForestUi.Back(this,close);ForestUi.Header(this,"SETTINGS");
            var audioTitle=ForestUi.Text("AUDIO",13,4);audioTitle.style.unityTextAlign=TextAnchor.MiddleLeft;ForestUi.At(audioTitle,7,21.2f,86,3);Add(audioTitle);
            AudioRow("Music","music",audio.MusicVolume,audio.SetMusicVolume,24.5f);AudioRow("Sound Effects","sound",audio.SoundVolume,audio.SetSoundVolume,32.3f);
            var haptic=ForestUi.Glass();ForestUi.At(haptic,6,40.1f,88,6.9f);Add(haptic);var hIcon=new ForestIcon("haptics",ForestUi.White);ForestUi.At(hIcon,4,21,10,58);haptic.Add(hIcon);var hLabel=ForestUi.Text("Haptics",14,2);hLabel.style.unityTextAlign=TextAnchor.MiddleLeft;ForestUi.At(hLabel,18,0,45,100);haptic.Add(hLabel);
            var toggle=new Button();toggle.text="";toggle.style.backgroundColor=Color.clear;toggle.style.paddingLeft=0;toggle.style.paddingRight=0;toggle.style.paddingTop=0;toggle.style.paddingBottom=0;toggle.style.marginTop=0;toggle.style.marginBottom=0;UiStyles.Border(toggle,Color.clear,0);ForestUi.At(toggle,72,20,15,60);haptic.Add(toggle);var status=ForestUi.Text("",10,2);ForestUi.At(status,89,20,10,60);haptic.Add(status);
            toggle.generateVisualContent+=ctx=>{var p=ctx.painter2D;float w=toggle.contentRect.width,h=toggle.contentRect.height;bool on=GameHaptics.Enabled;VectorArt.RoundRect(p,0,h*.2f,w,h*.6f,h*.3f,new Color(.03f,.5f,.65f,.6f));VectorArt.Circle(p,new Vector2(on?w-h*.3f:h*.3f,h/2),h*.3f,ForestUi.White);};Action update=()=>{status.text=GameHaptics.Enabled?"ON":"OFF";toggle.MarkDirtyRepaint();};toggle.clicked+=()=>{GameHaptics.Enabled=!GameHaptics.Enabled;update();};update();
            var themes=ForestUi.Text("BACKGROUND THEME",12,3);themes.style.unityTextAlign=TextAnchor.MiddleLeft;ForestUi.At(themes,7,49.7f,88,3.2f);Add(themes);
            for(int i=0;i<3;i++){int theme=i;var b=new Button(()=>{PlayerPrefs.SetInt("mektra.background.theme",theme);PlayerPrefs.Save();NativeEffects.RefreshMenuTheme();RepaintThemes();});b.text="";b.style.backgroundColor=Color.clear;b.style.overflow=Overflow.Hidden;UiStyles.Round(b,16);UiStyles.Border(b,ForestUi.Cyan,PlayerPrefs.GetInt("mektra.background.theme",0)==i?2:0.7f);ForestUi.At(b,6+i*30.1f,53.3f,28,16.5f);
                var image=new Image{image=NativeEffects.ThemeTexture(i),scaleMode=ScaleMode.ScaleAndCrop,pickingMode=PickingMode.Ignore};UiStyles.Absolute(image);b.Add(image);b.userData=i;themeButtons.Add(b);Add(b);
                var name=ForestUi.Text(new[]{"MOONLIT JUNGLE","SUNSET","AURORA"}[i],8,1.5f);ForestUi.At(name,4.5f+i*30.1f,70.5f,31,2.8f);Add(name);}
            var presets=ForestUi.Text("COLOUR PRESET",12,3);presets.style.unityTextAlign=TextAnchor.MiddleLeft;ForestUi.At(presets,7,74.3f,88,2.8f);Add(presets);
            for(int i=0;i<ColorPresetPreview.Options.Length;i++){
                int choice=i;var button=ForestUi.Button(ColorPresetPreview.Options[i],()=>{ColorPresetPreview.Select(choice);RepaintPresets();});
                ForestUi.At(button,6+(i%2)*45,78+(i/2)*5,43,4.2f);
                foreach(var text in button.Query<TextElement>().ToList()){text.style.fontSize=11;text.style.letterSpacing=1;}
                presetButtons.Add(button);Add(button);
            }
            RepaintPresets();
            var done=ForestUi.Button("DONE",close,true);ForestUi.At(done,19,90,62,6.8f);Add(done);
        }
        readonly List<Button> presetButtons=new List<Button>();
        void RepaintPresets(){for(int i=0;i<presetButtons.Count;i++){bool active=i==ColorPresetPreview.CurrentIndex;UiStyles.Border(presetButtons[i],active?ForestUi.Cyan:new Color(.2f,.5f,.6f,.5f),active?2:1);presetButtons[i].style.backgroundColor=active?new Color(.08f,.32f,.42f,.85f):new Color(.02f,.12f,.2f,.65f);}}
        readonly List<Button> themeButtons=new List<Button>();
        void RepaintThemes(){foreach(var b in themeButtons)UiStyles.Border(b,ForestUi.Cyan,(int)b.userData==PlayerPrefs.GetInt("mektra.background.theme",0)?2:0.7f);}
        void AudioRow(string name,string kind,float initial,Action<float> change,float y)
        {
            var row=ForestUi.Glass();ForestUi.At(row,6,y,88,6.9f);Add(row);var icon=new ForestIcon(kind,ForestUi.White);ForestUi.At(icon,4,22,10,56);row.Add(icon);var text=ForestUi.Text(name,14,2);text.style.unityTextAlign=TextAnchor.MiddleLeft;ForestUi.At(text,18,0,36,100);row.Add(text);
            var value=ForestUi.Text(Mathf.RoundToInt(initial*100)+"%",10,1);ForestUi.At(value,89,0,10,100);row.Add(value);var slider=new Slider(0,1){value=initial};ForestUi.At(slider,49,24,38,52);row.Add(slider);
            var tracker=slider.Q("unity-tracker");if(tracker!=null){tracker.style.backgroundColor=Color.clear;UiStyles.Border(tracker,Color.clear,0);}
            var dragger=slider.Q("unity-dragger");if(dragger!=null){dragger.style.width=11;dragger.style.height=11;dragger.style.backgroundColor=ForestUi.White;UiStyles.Round(dragger,6);UiStyles.Border(dragger,ForestUi.Cyan,1);}
            slider.generateVisualContent+=ctx=>{var p=ctx.painter2D;float w=slider.contentRect.width,h=slider.contentRect.height;p.lineCap=LineCap.Round;p.lineWidth=3;p.BeginPath();p.MoveTo(new Vector2(0,h/2));p.LineTo(new Vector2(w,h/2));p.strokeColor=new Color(.16f,.47f,.59f,.5f);p.Stroke();p.BeginPath();p.MoveTo(new Vector2(0,h/2));p.LineTo(new Vector2(w*slider.value,h/2));p.strokeColor=ForestUi.Cyan;p.Stroke();};
            slider.RegisterValueChangedCallback(e=>{change(e.newValue);value.text=Mathf.RoundToInt(e.newValue*100)+"%";slider.MarkDirtyRepaint();});
        }
    }
}
