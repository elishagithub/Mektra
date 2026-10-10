using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public static class UiStyles
    {
        public static readonly Color Background=VectorArt.ColorOf("#081222"),Ink=VectorArt.ColorOf("#EAF2FF"),Muted=VectorArt.ColorOf("#899CB8"),Blue=VectorArt.ColorOf("#46B5FF");
        public static void Round(VisualElement e,float radius)
        {
            e.style.borderTopLeftRadius=radius;
            e.style.borderTopRightRadius=radius;
            e.style.borderBottomLeftRadius=radius;
            e.style.borderBottomRightRadius=radius;
        }
        public static void Pad(VisualElement e,float p)
        {
            e.style.paddingTop=p;
            e.style.paddingBottom=p;
            e.style.paddingLeft=p;
            e.style.paddingRight=p;
        }
        public static void Absolute(VisualElement e)
        {
            e.style.position=Position.Absolute;
            e.style.left=0;
            e.style.top=0;
            e.style.right=0;
            e.style.bottom=0;
        }
        public static Label Text(string text,int size=14,Color? color=null,bool bold=false)
        {
            var l=new Label(text);
            l.style.fontSize=size;
            l.style.color=color??Ink;
            l.style.whiteSpace=WhiteSpace.Normal;
            l.style.unityFontStyleAndWeight=bold?FontStyle.Bold:FontStyle.Normal;
            l.pickingMode=PickingMode.Ignore;
            return l;
        }
        public static Button Button(string text,Action click,bool secondary=false)
        {
            var b=new Button(click)
            {
                text=text
            }
            ;
            b.style.height=48;
            b.style.marginTop=6;
            b.style.marginBottom=6;
            b.style.marginLeft=0;
            b.style.marginRight=0;
            b.style.borderTopWidth=0;
            b.style.borderBottomWidth=0;
            b.style.borderLeftWidth=0;
            b.style.borderRightWidth=0;
            b.style.backgroundColor=secondary?VectorArt.ColorOf("#1B3151"):Ink;
            b.style.color=secondary?Ink:Background;
            b.style.fontSize=14;
            b.style.unityFontStyleAndWeight=FontStyle.Bold;
            Round(b,24);
            b.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());
            return b;
        }
        public static void Center(VisualElement e)
        {
            e.style.alignItems=Align.Center;
            e.style.justifyContent=Justify.Center;
        }
        public static void Border(VisualElement e,Color color,float width)
        {
            e.style.borderTopColor=color;
            e.style.borderBottomColor=color;
            e.style.borderLeftColor=color;
            e.style.borderRightColor=color;
            e.style.borderTopWidth=width;
            e.style.borderBottomWidth=width;
            e.style.borderLeftWidth=width;
            e.style.borderRightWidth=width;
        }
    }
}
