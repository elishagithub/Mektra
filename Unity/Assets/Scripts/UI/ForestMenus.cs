using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public static class ForestMenus
    {
        public static VisualElement Home(Character character,Action play,Action characters,Action ranks,Action share,Action settings)
        {
            var root=ForestUi.Screen();ForestUi.Header(root,"MEKTRA",11,34);
            var art=new VisualElement();ForestUi.At(art,0,0,100,100);art.pickingMode=PickingMode.Ignore;art.generateVisualContent+=ctx=>{var p=ctx.painter2D;var glow=RobotView.Tint(character);float w=art.contentRect.width,h=art.contentRect.height;Blob(p,new Vector2(w*.54f,-h*.005f),w*.096f,.4f);Blob(p,new Vector2(w*.46f,h*.335f),w*.095f,1.2f);Blob(p,new Vector2(w*.51f,h*.65f),w*.095f,2.4f);
                var from=new Vector2(w*.51f,h*.59f);var to=new Vector2(w*.47f,h*.48f);p.BeginPath();p.MoveTo(from);p.LineTo(to);p.strokeColor=new Color(glow.r,glow.g,glow.b,.14f);p.lineWidth=10;p.Stroke();p.strokeColor=glow;p.lineWidth=2;p.Stroke();for(int i=1;i<3;i++){float t=i/3f;var spark=Vector2.Lerp(from,to,t)+new Vector2(Mathf.Sin(i*7f)*9,Mathf.Cos(i*5f)*6);VectorArt.Circle(p,spark,1.2f,glow);}VectorArt.Circle(p,new Vector2(w*.48f,h*.212f),2.5f,new Color(.8f,1,.2f));};root.Add(art);
            var avatar=new AvatarView(character,192,true){Flying=true};ForestUi.At(avatar,42,44,12,8);root.Add(avatar);
            var start=ForestUi.Button("PLAY",play,true,"play");ForestUi.At(start,21.5f,72.8f,57,7.2f);root.Add(start);
            string[] icons={"person","rank","share-graph","settings"},labels={"CHARACTERS","RANKS","SHARE","SETTINGS"};Action[] actions={characters,ranks,share,settings};for(int i=0;i<4;i++){var b=ForestUi.Circle(icons[i],actions[i]);ForestUi.At(b,10.5f+i*21.5f,83,15.2f,7);root.Add(b);var label=ForestUi.Text(labels[i],9,1.6f);ForestUi.At(label,7+i*21.5f,90.8f,22,2.5f);root.Add(label);}return root;
        }
        static void Blob(Painter2D p,Vector2 center,float radius,float seed){var points=new Vector2[100];for(int i=0;i<points.Length;i++){float a=i*Mathf.PI*2/points.Length,r=radius*(1+.07f*Mathf.Sin(3*a+seed)+.025f*Mathf.Sin(2*a));points[i]=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;}VectorArt.Polygon(p,points,Color.black);}
        public static VisualElement Results(Character character,int distance,int best,int jumps,Action retry,Action home,Action share)
        {
            var root=ForestUi.Screen();ForestUi.Header(root,"GAME OVER",18.4f,29);
            var hero=new AvatarView(character,384,true);ForestUi.At(hero,37,25.5f,35,15);root.Add(hero);
            var perch=ForestUi.Glass();ForestUi.At(perch,42,38,58,3);root.Add(perch);
            var label=ForestUi.Text("YOUR SCORE",13,4);ForestUi.At(label,12,48,76,3);root.Add(label);var score=ForestUi.Text(distance.ToString(),65,1);ForestUi.At(score,12,51.5f,76,9);root.Add(score);
            var message=ForestUi.Text(distance>=best?"A NEW HIGH":"SO CLOSE!",11,4,ForestUi.Cyan);ForestUi.At(message,12,60.2f,76,3);root.Add(message);
            string[] titles={"BEST","JUMPS"},icons={"rank","jump"};int[] values={best,jumps};for(int i=0;i<2;i++){var box=ForestUi.Glass();ForestUi.At(box,12+i*39.5f,63,36.5f,6.5f);var icon=new ForestIcon(icons[i],ForestUi.White);ForestUi.At(icon,9,24,24,52);box.Add(icon);var title=ForestUi.Text(titles[i],9,3);ForestUi.At(title,39,14,55,28);box.Add(title);var value=ForestUi.Text(values[i].ToString(),19,2);ForestUi.At(value,37,43,58,42);box.Add(value);root.Add(box);}
            var again=ForestUi.Button("RETRY",retry,true,"retry");ForestUi.At(again,19,73,62,7);root.Add(again);Action[] actions={home,share};string[] names={"HOME","SHARE"};for(int i=0;i<2;i++){var b=ForestUi.Circle(i==0?"home":"share",actions[i]);ForestUi.At(b,29+i*25.5f,82.5f,15,7);root.Add(b);var title=ForestUi.Text(names[i],11,3);ForestUi.At(title,23+i*25.5f,90.5f,27,3);root.Add(title);}return root;
        }
        public static VisualElement Pause(Character character,Action resume,Action restart,Action settings,Action home)
        {
            var root=ForestUi.Screen();ForestUi.Header(root,"PAUSED",25,30);
            var glyph=new ForestIcon("pause",ForestUi.White);ForestUi.At(glyph,46,19.5f,8,4.5f);root.Add(glyph);
            var subtitle=ForestUi.Text("TAKE A BREATH",11,4);ForestUi.At(subtitle,14,32.4f,72,3);root.Add(subtitle);
            var hero=new AvatarView(character,256,true){Flying=true};ForestUi.At(hero,37.5f,36,25,14);root.Add(hero);
            var play=ForestUi.Button("RESUME",resume,true,"play");ForestUi.At(play,19,54,62,7);root.Add(play);
            var again=ForestUi.Button("RESTART",restart,false,"retry");ForestUi.At(again,20.5f,63,59,6.3f);root.Add(again);
            var options=ForestUi.Button("SETTINGS",settings,false,"settings");ForestUi.At(options,20.5f,70.5f,59,6.3f);root.Add(options);
            var back=ForestUi.Circle("home",home);ForestUi.At(back,42.5f,79.5f,15,7);root.Add(back);var label=ForestUi.Text("HOME",11,3);ForestUi.At(label,33,87.5f,34,3);root.Add(label);return root;
        }
        public static VisualElement Share(Character character,int score,int best,Action close)
        {
            var root=ForestUi.Screen();ForestUi.Back(root,close);ForestUi.Header(root,"SHARE");var avatar=new AvatarView(character,384,true);ForestUi.At(avatar,27,26,46,22);root.Add(avatar);var panel=ForestUi.Glass();ForestUi.At(panel,12,50,76,18);root.Add(panel);var game=ForestUi.Text("MEKTRA",24,7);ForestUi.At(game,5,10,90,25);panel.Add(game);var distance=ForestUi.Text(score+" DISTANCE",28,3);ForestUi.At(distance,5,39,90,27);panel.Add(distance);var caption=ForestUi.Text("BEST "+best,12,3);ForestUi.At(caption,5,73,90,17);panel.Add(caption);var button=ForestUi.Button("SHARE",()=>ScoreShare.Share(score,best),true,"share");ForestUi.At(button,19,80,62,7);root.Add(button);return root;
        }
    }
}
