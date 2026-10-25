using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public static class MiniRobotFace
    {
        public static void Draw(Painter2D p,Vector2 center,float radius,Character character,string expression,double time)
        {
            // A solid display inside the smooth shell. No glow or outline layers.
            Color screen=ColorPresetPreview.IsInvert?new Color(.88f,.88f,.88f):new Color(.12f,.13f,.13f);
            Color ink=ColorPresetPreview.IsInvert?Color.black:Color.white;
            float width=radius*1.64f,height=radius*1.40f;
            VectorArt.RoundRect(p,center.x-width/2,center.y-height/2,width,height,radius*.58f,screen);
            bool blink=time%4.1>3.94;
            float eyeWidth=radius*.25f,eyeHeight=radius*.35f;
            if(character.eyeStyle=="round")eyeWidth=eyeHeight=radius*.32f;
            if(character.eyeStyle=="visor"){eyeWidth=radius*.39f;eyeHeight=radius*.13f;}
            if(expression=="focused")eyeHeight=radius*.14f;
            if(expression=="surprised")eyeHeight=radius*.43f;
            bool single=character.eyeStyle=="mono";
            p.strokeColor=ink;p.lineWidth=radius*.12f;p.lineCap=LineCap.Round;
            foreach(float side in single?new[]{0f}:new[]{-1f,1f})
            {
                float x=center.x+side*radius*.35f,y=center.y-radius*.16f;
                if(blink)
                {
                    p.BeginPath();p.MoveTo(new Vector2(x-radius*.13f,y));p.LineTo(new Vector2(x+radius*.13f,y));p.Stroke();
                }
                else if(expression=="happy")
                {
                    p.BeginPath();p.MoveTo(new Vector2(x-radius*.15f,y+radius*.05f));
                    p.QuadraticCurveTo(new Vector2(x,y-radius*.21f),new Vector2(x+radius*.15f,y+radius*.05f));p.Stroke();
                }
                else VectorArt.RoundRect(p,x-eyeWidth/2,y-eyeHeight/2,eyeWidth,eyeHeight,Mathf.Min(eyeWidth,eyeHeight)/2,ink);
            }
            if(expression=="surprised")VectorArt.Circle(p,center+new Vector2(0,radius*.32f),radius*.11f,ink);
            else
            {
                float half=radius*(expression=="happy"?.22f:.16f),y=center.y+radius*.24f;
                p.BeginPath();p.MoveTo(new Vector2(center.x-half,y));
                p.QuadraticCurveTo(new Vector2(center.x,y+radius*(expression=="happy"?.25f:.15f)),new Vector2(center.x+half,y));p.Stroke();
            }
        }
    }
}
