using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    // Rounded glyph cells glow white; critical energy blinks red without hiding the remaining charge.
    public sealed class EnergyMeter:VisualElement
    {
        double amount=100,time;
        public EnergyMeter()
        {
            pickingMode=PickingMode.Ignore;
            style.width=116;style.height=32;style.marginTop=0;
            style.backgroundColor=Color.clear;
            style.borderTopLeftRadius=16;style.borderTopRightRadius=16;
            style.borderBottomLeftRadius=16;style.borderBottomRightRadius=16;
            generateVisualContent+=Draw;
        }
        public void Refresh(double energy,double seconds){amount=energy;time=seconds;MarkDirtyRepaint();}
        void Cell(Painter2D p,float x,float y,float size,float radius,Color color)
        {
            const float k=.55228475f;float r=radius,c=r*k,hi=x+size,bottom=y+size;
            p.BeginPath();p.MoveTo(new Vector2(x+r,y));p.LineTo(new Vector2(hi-r,y));
            p.BezierCurveTo(new Vector2(hi-r+c,y),new Vector2(hi,y+r-c),new Vector2(hi,y+r));p.LineTo(new Vector2(hi,bottom-r));
            p.BezierCurveTo(new Vector2(hi,bottom-r+c),new Vector2(hi-r+c,bottom),new Vector2(hi-r,bottom));p.LineTo(new Vector2(x+r,bottom));
            p.BezierCurveTo(new Vector2(x+r-c,bottom),new Vector2(x,bottom-r+c),new Vector2(x,bottom-r));p.LineTo(new Vector2(x,y+r));
            p.BezierCurveTo(new Vector2(x,y+r-c),new Vector2(x+r-c,y),new Vector2(x+r,y));p.ClosePath();p.fillColor=color;p.Fill();
        }
        void Draw(MeshGenerationContext ctx)
        {
            var p=ctx.painter2D;bool critical=amount<=20;
            var bright=critical?new Color(1,.65f,.3f):new Color(.86f,1,.66f);
            float pulse=critical?.45f+.55f*(float)(.5+.5*Math.Sin(time*Math.PI*2)):1;
            Cell(p,10,9,14,4,new Color(bright.r,bright.g,bright.b,.22f*pulse));
            VectorArt.Circle(p,new Vector2(17,17),3,new Color(bright.r,bright.g,bright.b,pulse));
            VectorArt.Circle(p,new Vector2(13,12),2,new Color(bright.r,bright.g,bright.b,.3f));
            VectorArt.Circle(p,new Vector2(21,12),2,new Color(bright.r,bright.g,bright.b,.3f));
            for(int i=0;i<10;i++)
            {
                float x=32+i*8,y=12;float filled=(float)Math.Clamp(amount/10-i,0,1);
                Cell(p,x,y,6,2,new Color(bright.r,bright.g,bright.b,.12f+.88f*filled*pulse));
            }
        }
    }
}
