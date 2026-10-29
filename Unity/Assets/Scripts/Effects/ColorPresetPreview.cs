using UnityEngine;
namespace Mektra
{
    // Shared colour grading for camera effects and UI Toolkit gameplay artwork.
    public static class ColorPresetPreview
    {
        static readonly string[] Names={"Setting Sun","Invert","Black & White","Blue Hue"};
        const int DefaultIndex=0;
        static int index=DefaultIndex;public static int CurrentIndex=>index;
        public static bool Automatic=>PlayerPrefs.GetInt("mektra.preset.auto",0)==1;
        public static string Selection=>Automatic?"Change each replay":Names[index];
        public static string[] Options=>Names;
        public static void SetAutomatic(){PlayerPrefs.SetInt("mektra.preset.auto",1);PlayerPrefs.Save();}
        public static void Select(int choice){index=Mathf.Clamp(choice,0,Names.Length-1);PlayerPrefs.SetInt("mektra.preset.auto",0);PlayerPrefs.SetInt("mektra.preset.choice",index);PlayerPrefs.SetString("mektra.preset.name",Names[index]);PlayerPrefs.Save();Load();}
        public static void Replay(){if(Automatic)Step(1);}

        public static Color GradientTop=>IsInvert?new Color(.77f,.70f,.82f):IsMonochrome?new Color(.85f,.75f,.68f):new Color(.66f,.75f,.82f);
        public static Color HudInk=>IsInvert?new Color(.04f,.03f,0):new Color(.96f,.97f,1f);
        public static Color PlayerInk=>IsInvert?Color.black:Color.white;
        public static Color PlatformInk=>Color.black;
        public static int AbstractMode=>0;
        public static bool IsInvert=>Names[index]=="Invert";
        public static bool IsMonochrome=>Names[index]=="Black & White";
        public static Color ShapeFill=>Apply(IsInvert?Color.white:VectorArt.ColorOf("#03060B"));
        public static Color Backlight=>IsMonochrome?VectorArt.ColorOf("#EBA16F"):IsInvert?VectorArt.ColorOf("#68D9CA"):Names[index]=="Blue Hue"?VectorArt.ColorOf("#719DD8"):VectorArt.ColorOf("#BA8AFF");
        public static ThemeJourney.Palette Background
        {
            get
            {
                return new ThemeJourney.Palette("#191632","#03162F","#3A729C","#699BDC",0,.25f,1,.5f,0,0);

            }
        }
        public static int Version {get;private set;}
        public static string Caption=>$"{index+1}/{Names.Length} · {Names[index]}";
        public static void Initialize()
        {
            if(PlayerPrefs.GetInt("mektra.defaults.setting-sun.v1",0)==0){
                PlayerPrefs.SetString("mektra.preset.name","Setting Sun");PlayerPrefs.SetInt("mektra.preset.choice",DefaultIndex);
                PlayerPrefs.SetInt("mektra.preset.auto",0);PlayerPrefs.SetInt("mektra.background.theme",0);
                PlayerPrefs.SetInt("mektra.defaults.setting-sun.v1",1);PlayerPrefs.Save();
            }
            index=System.Array.IndexOf(Names,PlayerPrefs.GetString("mektra.preset.name","Setting Sun"));if(index<0)index=DefaultIndex;Load();
        }
        public static void Step(int direction){index=(index+direction+Names.Length)%Names.Length;Load();}
        static Texture2D lut;
        static void Load(){lut=Resources.Load<Texture2D>("ColorGrades/"+Names[index]);Version++;}
        // No LUT grading: shader output remains unchanged.
        public static Color Apply(Color color)
        {
            float alpha=color.a;
            if(IsInvert)return new Color(1-color.r,1-color.g,1-color.b,alpha);
            if(IsMonochrome){float grey=.2126f*color.r+.7152f*color.g+.0722f*color.b;return new Color(grey,grey,grey,alpha);}
            if(lut==null)return color;
            // Amplify's horizontal strip: one square slice for each blue value.
            int size=lut.height;float blue=Mathf.Clamp01(color.b)*(size-1);int lo=Mathf.FloorToInt(blue),hi=Mathf.Min(lo+1,size-1);
            float red=Mathf.Clamp01(color.r)*(size-1),green=Mathf.Clamp01(color.g)*(size-1);
            Color a=lut.GetPixelBilinear((lo*size+red+.5f)/lut.width,(green+.5f)/size);
            Color b=lut.GetPixelBilinear((hi*size+red+.5f)/lut.width,(green+.5f)/size);
            var result=Color.Lerp(a,b,blue-lo);result.a=alpha;return result;
        }
    }
}
