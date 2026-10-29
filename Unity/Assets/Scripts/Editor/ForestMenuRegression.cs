using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class ForestMenuRegression
    {
        public static void Run()
        {
            foreach(string name in new[]{"MenuLight","MenuMedium"})if(Resources.Load<Font>("Fonts/"+name)==null)throw new Exception("Missing menu font: "+name);
            foreach(string name in new[]{"AvatarSurface","AvatarHalo","ForestTheme"}){var shader=Resources.Load<Shader>("Shaders/"+name);if(shader==null||!shader.isSupported)throw new Exception("Missing menu shader: "+name);}
            var avatar=new AvatarRig(CharacterCatalog.Load()[0],128);var old=RenderTexture.active;var readback=new Texture2D(128,128,TextureFormat.RGBA32,false);
            try{
                avatar.Render(3.265,false,false,0,true);if(avatar.EyeOpenness>.2f)throw new Exception("Avatar does not blink");
                avatar.Render(3.6,true,false,0,true);if(avatar.EyeOpenness<.9f)throw new Exception("Avatar eyes do not reopen");var foot=avatar.FootPosition(0);
                avatar.Render(3.9,true,false,0,true);if((avatar.FootPosition(0)-foot).magnitude<.01f)throw new Exception("Avatar feet do not walk");
                avatar.Render(4,true,true,1,true);if(avatar.FootPosition(0).y<=-.48f)throw new Exception("Avatar feet do not tuck during flight");
                RenderTexture.active=avatar.Texture;readback.ReadPixels(new Rect(0,0,128,128),0,0);readback.Apply();
                if(readback.GetPixel(60,60).a<.8f||readback.GetPixel(0,0).a>.01f)throw new Exception("Avatar does not render against transparent background");
            }finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(readback);avatar.Dispose();}
            foreach(var character in CharacterCatalog.Load())using(var bug=new AvatarRig(character,64)){
                if(bug.LegCount!=AvatarRig.FootCount(character.id))throw new Exception("Incorrect bug leg count: "+character.id);
                bug.Render(1,true,false,0,true);var walking=bug.FootPosition(0);bug.Render(1.2,true,false,0,true);
                if((bug.FootPosition(0)-walking).magnitude<.02f)throw new Exception("Bug gait is not visible: "+character.id);
                bug.Render(1.2,false,true,0,true);if(bug.FootPosition(0).y<walking.y)throw new Exception("Bug legs do not fold in flight: "+character.id);
            }
            Debug.Log("MEKTRA_FOREST_MENUS_PASS: light fonts and shaders loaded; independent eyes blink/reopen, feet walk/tuck, 3D avatar renders with transparent background.");
        }
    }
}
