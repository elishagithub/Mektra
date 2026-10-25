using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public sealed class ActionIcon:VisualElement
    {
        public ActionIcon(string kind,Color color)
        {
            style.width=18;style.height=18;style.marginRight=8;pickingMode=PickingMode.Ignore;
            generateVisualContent+=ctx=>
            {
                var p=ctx.painter2D;p.strokeColor=color;p.lineWidth=1.7f;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
                Action<Vector2[]> line=points=>{p.BeginPath();p.MoveTo(points[0]);for(int i=1;i<points.Length;i++)p.LineTo(points[i]);p.Stroke();};
                if(kind=="share")
                {
                    line(new[]{new Vector2(3,9),new Vector2(3,15),new Vector2(15,15),new Vector2(15,9)});
                    line(new[]{new Vector2(9,11),new Vector2(9,2)});line(new[]{new Vector2(5,6),new Vector2(9,2),new Vector2(13,6)});
                }
                else if(kind=="rank")
                {
                    line(new[]{new Vector2(2,15),new Vector2(2,9),new Vector2(6,9),new Vector2(6,15)});
                    line(new[]{new Vector2(7,15),new Vector2(7,3),new Vector2(11,3),new Vector2(11,15)});
                    line(new[]{new Vector2(12,15),new Vector2(12,7),new Vector2(16,7),new Vector2(16,15)});
                }
                else if(kind=="home")
                {
                    line(new[]{new Vector2(2,8),new Vector2(9,2),new Vector2(16,8)});
                    line(new[]{new Vector2(4,7),new Vector2(4,16),new Vector2(8,16),new Vector2(8,11),new Vector2(11,11),new Vector2(11,16),new Vector2(14,16),new Vector2(14,7)});
                }
                else if(kind=="replay")
                {
                    var points=new Vector2[25];for(int i=0;i<25;i++){float a=(-55+i*270f/24)*Mathf.Deg2Rad;points[i]=new Vector2(9,9)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*6;};line(points);
                    line(new[]{new Vector2(2,2),new Vector2(2,7),new Vector2(7,7)});
                }
                else VectorArt.Polygon(p,new[]{new Vector2(5,3),new Vector2(15,9),new Vector2(5,15)},color);
            };
        }
    }
    public sealed class PanelGlow:VisualElement
    {
        public PanelGlow()
        {
            style.position=Position.Absolute;style.left=-20;style.right=-20;style.top=-20;style.bottom=-20;pickingMode=PickingMode.Ignore;
            generateVisualContent+=ctx=>
            {
                float w=contentRect.width,h=contentRect.height;
                for(int i=8;i>=1;i--){float inset=20-i*2;VectorArt.RoundRect(ctx.painter2D,inset,inset,w-inset*2,h-inset*2,32+i*2,new Color(0.5394f,0.5394f,0.5394f,.007f));}
            };
        }
    }
}
