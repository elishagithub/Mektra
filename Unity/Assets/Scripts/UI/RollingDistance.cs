using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    // Each digit has its own clipped reel, so carries roll along with the units.
    public sealed class RollingDistance:VisualElement
    {
        sealed class Reel
        {
            public VisualElement window;
            public Label oldDigit,newDigit;
            public char digit;
            public float started;
            public bool rolling;
        }
        readonly System.Collections.Generic.List<Reel> reels=new();
        int previous=-1;
        const float Height=38,Duration=.18f;
        public RollingDistance()
        {
            pickingMode=PickingMode.Ignore;
            style.flexDirection=FlexDirection.Row;
            style.height=Height;
            style.flexShrink=0;
        }
        Label Digit(char value)
        {
            var label=UiStyles.Text(value.ToString(),32,Color.white,true);
            label.style.position=Position.Absolute;
            label.style.top=0;
            label.style.left=0;
            label.style.marginTop=0;
            label.style.marginBottom=0;
            label.style.paddingTop=0;
            label.style.paddingBottom=0;
            label.style.height=Height;
            label.style.unityTextAlign=TextAnchor.MiddleCenter;
            label.style.width=21;
            label.style.textShadow=default(TextShadow);
            return label;
        }
        public void Refresh(int value)
        {
            var digits=Math.Max(0,value).ToString();
            bool reset=previous<0||value<previous;
            while(reels.Count<digits.Length)
            {
                var reel=new Reel{window=new VisualElement(),digit='0'};
                reel.window.style.width=21;
                reel.window.style.height=Height;
                reel.window.style.overflow=Overflow.Hidden;
                reel.oldDigit=Digit('0');reel.newDigit=Digit('0');
                reel.window.Add(reel.oldDigit);reel.window.Add(reel.newDigit);
                Insert(0,reel.window);reels.Insert(0,reel);
            }
            while(reels.Count>digits.Length){reels[0].window.RemoveFromHierarchy();reels.RemoveAt(0);}
            for(int i=0;i<reels.Count;i++)
            {
                var reel=reels[i];
                reel.oldDigit.style.color=ColorPresetPreview.HudInk;reel.newDigit.style.color=ColorPresetPreview.HudInk;
                if(reset){reel.digit=digits[i];reel.newDigit.text=digits[i].ToString();reel.rolling=false;}
                else if(reel.digit!=digits[i])
                {
                    reel.oldDigit.text=reel.digit.ToString();
                    reel.digit=digits[i];reel.newDigit.text=digits[i].ToString();
                    reel.started=Time.unscaledTime;reel.rolling=true;
                }
                float t=reel.rolling?Mathf.Clamp01((Time.unscaledTime-reel.started)/Duration):1;
                float eased=1-Mathf.Pow(1-t,3);
                reel.oldDigit.style.top=-Height*eased;
                reel.newDigit.style.top=Height*(1-eased);
                if(t>=1)reel.rolling=false;
            }
            previous=value;
        }
    }
}
