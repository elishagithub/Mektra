using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    // Native vector paths retain the Figma curves at any window or Retina resolution.
    public static class VectorArt
    {
        public static Color ColorOf(string hex)
        {
            ColorUtility.TryParseHtmlString(hex,out var c);
            return c;
        }
        public static void Polygon(Painter2D p,Vector2[] points,Color color)
        {
            p.fillColor=color;
            p.BeginPath();
            p.MoveTo(points[0]);
            for(int i=1;i<points.Length;i++)p.LineTo(points[i]);
            p.ClosePath();
            p.Fill();
        }
        public static void Circle(Painter2D p,Vector2 center,float r,Color color)
        {
            p.fillColor=color;
            p.BeginPath();
            p.Arc(center,r,0,360);
            p.ClosePath();
            p.Fill();
        }
        public static void RoundRect(Painter2D p,float x,float y,float w,float h,float r,Color color)
        {
            r=Mathf.Min(r,Mathf.Min(w,h)/2);
            p.fillColor=color;
            p.BeginPath();
            p.MoveTo(new Vector2(x+r,y));
            p.LineTo(new Vector2(x+w-r,y));
            p.QuadraticCurveTo(new Vector2(x+w,y),new Vector2(x+w,y+r));
            p.LineTo(new Vector2(x+w,y+h-r));
            p.QuadraticCurveTo(new Vector2(x+w,y+h),new Vector2(x+w-r,y+h));
            p.LineTo(new Vector2(x+r,y+h));
            p.QuadraticCurveTo(new Vector2(x,y+h),new Vector2(x,y+h-r));
            p.LineTo(new Vector2(x,y+r));
            p.QuadraticCurveTo(new Vector2(x,y),new Vector2(x+r,y));
            p.ClosePath();
            p.Fill();
        }
        public static void Star(Painter2D p,Vector2 center,float r,Color color,float angle=0)
        {
            var points=new Vector2[10];
            for(int i=0;i<10;i++)
            {
                float a=-Mathf.PI/2+angle+i*Mathf.PI/5;
                points[i]=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(i%2==0?r:r*.44f);
            }
            Polygon(p,points,color);
        }
        public static void Path(Painter2D p,PathCommand[] commands,Vector2 shift,float scale=1)
        {
            p.BeginPath();
            foreach(var c in commands)
            {
                var v=c.v;
                switch(c.op)
                {
                    case "M":p.MoveTo(new Vector2((float)v[0],(float)v[1])*scale+shift);
                    break;
                    case "L":p.LineTo(new Vector2((float)v[0],(float)v[1])*scale+shift);
                    break;
                    case "C":p.BezierCurveTo(new Vector2((float)v[0],(float)v[1])*scale+shift,new Vector2((float)v[2],(float)v[3])*scale+shift,new Vector2((float)v[4],(float)v[5])*scale+shift);
                    break;
                    case "Q":p.QuadraticCurveTo(new Vector2((float)v[0],(float)v[1])*scale+shift,new Vector2((float)v[2],(float)v[3])*scale+shift);
                    break;
                    case "H":case "V":throw new InvalidOperationException("Export must normalize SVG H/V commands.");
                    case "Z":case "z":p.ClosePath();
                    break;
                }
            }
        }
        public static Vector2 Polar(Vector2 center,float r,double a,float scale)=>center+new Vector2((float)Math.Cos(a),(float)Math.Sin(a))*r*scale;
        public static void Teeth(Painter2D p,Vector2 center,float scale,Character c,double angle)
        {
            var shell=ColorOf(c.color);var stroke=ColorOf(c.shellStroke);
            for(int i=0;i<c.legCount;i++)
            {
                double a=angle+i*2*Math.PI/c.legCount;
                Vector2[] points;
                if(c.legStyle=="paddle")points=new[]
                {
                    Polar(center,13,a-.1,scale),Polar(center,17.6f,a-.06,scale),Polar(center,17.6f,a+.06,scale),Polar(center,13,a+.1,scale)
                }
                ;
                else
                {
                    double width=c.legStyle=="needle"?.04:c.legStyle=="blade"?.13:.095;
                    points=new[]
                    {
                        Polar(center,13,a-width,scale),Polar(center,17.6f,a+(c.legStyle=="blade"?.045:0),scale),Polar(center,13,a+(c.legStyle=="blade"?.095:width),scale)
                    }
                    ;
                }
                Polygon(p,points,shell);
                p.strokeColor=stroke;
                p.lineWidth=.5f*scale;
                p.Stroke();
            }
        }
        public static void Shell(Painter2D p,Vector2 center,float scale,Character c)
        {
            var shell=ColorOf(c.color);var stroke=ColorOf(c.shellStroke);
            Path(p,c.shellPath,center,scale);
            p.fillColor=shell;
            p.Fill();
            p.strokeColor=stroke;
            p.lineWidth=scale;
            p.Stroke();
            if(c.shape!="diamond")
            {
                p.strokeColor=c.id=="mektra"?ColorOf("#DAE8F8"):new Color(1,1,1,.7f);
                p.lineWidth=.8f*scale;
                p.BeginPath();
                p.MoveTo(center+new Vector2(-8,-10)*scale);
                p.QuadraticCurveTo(center+new Vector2(0,-14)*scale,center+new Vector2(8,-10)*scale);
                p.Stroke();
            }
        }
        public static void Face(Painter2D p,Vector2 center,float scale,Character c,string expression,double time)
        {
            var stroke=ColorOf(c.shellStroke);var face=ColorOf(c.faceColor);var eye=ColorOf(c.eyeColor);
            float fs=scale*(c.shape=="diamond"?.8f:1);
            RoundRect(p,center.x-10*fs,center.y-9*fs,20*fs,18*fs,5*fs,stroke);
            RoundRect(p,center.x-9.3f*fs,center.y-8.3f*fs,18.6f*fs,16.6f*fs,4.5f*fs,face);
            double t=time%4.1;
            float blink=t>3.92?(float)Math.Max(.12,Math.Abs(t-4.01)/.09):1;
            float h=(expression=="focused"?4:expression=="surprised"?9:c.eyeStyle=="visor"?3:7)*blink;
            float w=c.eyeStyle=="round"?6:c.eyeStyle=="visor"?7:5;
            if(c.eyeStyle=="mono")w=8;
            foreach(float x in c.eyeStyle=="mono"?new[]
            {
                0f
            }
            :new[]
            {
                -5.5f,5.5f
            }
            )
            {
                if(expression=="happy")
                {
                    p.strokeColor=eye;
                    p.lineWidth=2*fs;
                    p.lineCap=LineCap.Round;
                    p.BeginPath();
                    p.MoveTo(center+new Vector2(x-2.5f,0)*fs);
                    p.QuadraticCurveTo(center+new Vector2(x,-5)*fs,center+new Vector2(x+2.5f,0)*fs);
                    p.Stroke();
                }
                else RoundRect(p,center.x+(x-w/2)*fs,center.y-(h/2+.5f)*fs,w*fs,h*fs,Mathf.Min(w,h)/2*fs,eye);
            }
            if(expression=="surprised")Circle(p,center+new Vector2(0,5)*fs,1.5f*fs,eye);
            else RoundRect(p,center.x-2*fs,center.y+4.8f*fs,4*fs,1*fs,.5f*fs,eye);
            Circle(p,center+new Vector2(0,11)*fs,1*scale,eye);
        }
        public static void Robot(Painter2D p,Vector2 center,float scale,Character c,double angle,string expression,double time)
        {
            Teeth(p,center,scale,c,angle);Shell(p,center,scale,c);Face(p,center,scale,c,expression,time);

        }
    }
    public class RobotPortrait:VisualElement
    {
        public Character character;
        public bool selected;
        public RobotPortrait(Character c,bool active)
        {
            character=c;
            selected=active;
            style.height=120;
            generateVisualContent+=ctx=>VectorArt.Robot(ctx.painter2D,new Vector2(contentRect.width/2,60),2.65f,character,.12,selected?"happy":"idle",Time.unscaledTimeAsDouble);
            pickingMode=PickingMode.Ignore;
        }
    }
}
