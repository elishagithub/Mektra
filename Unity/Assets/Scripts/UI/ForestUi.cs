using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public static class ForestUi
    {
        public static readonly Color Cyan=new Color(.2f,.86f,1),White=new Color(.94f,.99f,1),Navy=new Color(.025f,.12f,.21f);
        static Font light,medium;
        public static void At(VisualElement e,float x,float y,float width,float height){e.style.position=Position.Absolute;e.style.left=Length.Percent(x);e.style.top=Length.Percent(y);e.style.width=Length.Percent(width);e.style.height=Length.Percent(height);}
        public static Label Text(string text,int size=16,float spacing=2,Color? colour=null,bool strong=false)
        {
            var l=UiStyles.Text(text,size,colour??White);l.style.letterSpacing=spacing;l.style.unityTextAlign=TextAnchor.MiddleCenter;l.style.marginTop=0;l.style.marginBottom=0;
            if(light==null)light=Resources.Load<Font>("Fonts/MenuLight");if(medium==null)medium=Resources.Load<Font>("Fonts/MenuMedium");
            l.style.unityFont=strong?medium:light;l.style.unityFontDefinition=new FontDefinition{font=strong?medium:light};l.style.textShadow=new TextShadow{blurRadius=4,color=new Color(.12f,.8f,1,.35f)};return l;
        }
        public static VisualElement Screen(){var e=new VisualElement();UiStyles.Absolute(e);e.pickingMode=PickingMode.Position;e.RegisterCallback<PointerDownEvent>(ev=>ev.StopPropagation());Background(e);return e;}
        public static void Background(VisualElement e)
        {
            var image=new Image{image=NativeEffects.MenuBackdrop??Resources.Load<Texture2D>("Art/FireflyForest/moonlit-forest"),scaleMode=ScaleMode.ScaleAndCrop,pickingMode=PickingMode.Ignore};UiStyles.Absolute(image);e.Add(image);image.schedule.Execute(()=>{var texture=NativeEffects.MenuBackdrop;if(texture!=null&&image.image!=texture)image.image=texture;}).Every(150);
            var veil=new VisualElement();UiStyles.Absolute(veil);veil.style.backgroundColor=new Color(0,.035f,.06f,.12f);veil.pickingMode=PickingMode.Ignore;e.Add(veil);
        }
        public static void Header(VisualElement parent,string title,float y=12.2f,int size=26)
        {
            var l=Text(title,size,title=="MEKTRA"?13:title=="CHARACTERS"?15:title=="PAUSED"||title=="GAME OVER"?18:13);At(l,6,y,88,5);parent.Add(l);
            var divider=new VisualElement();At(divider,34,y+5.3f,32,3);divider.pickingMode=PickingMode.Ignore;divider.generateVisualContent+=ctx=>{var p=ctx.painter2D;float w=divider.contentRect.width,h=divider.contentRect.height;for(int n=10;n>=1;n--)VectorArt.Circle(p,new Vector2(w/2,h/2),2+n*.5f,new Color(.1f,.85f,1,.012f));VectorArt.Circle(p,new Vector2(w/2,h/2),2.8f,White);p.lineWidth=.8f;for(int side=-1;side<=1;side+=2)for(int n=0;n<20;n++){float a=8+n*(w/2-8)/20,b=8+(n+1)*(w/2-8)/20;p.BeginPath();p.MoveTo(new Vector2(w/2+side*a,h/2));p.LineTo(new Vector2(w/2+side*b,h/2));p.strokeColor=new Color(.6f,.94f,1,(1-n/20f)*.9f);p.Stroke();}};parent.Add(divider);
        }
        public static Button Button(string label,Action click,bool primary=false,string icon=null)
        {
            var b=new Button(click);b.text="";b.style.backgroundColor=Color.clear;b.style.paddingLeft=0;b.style.paddingRight=0;b.style.paddingTop=0;b.style.paddingBottom=0;b.style.marginLeft=0;b.style.marginRight=0;b.style.marginTop=0;b.style.marginBottom=0;UiStyles.Border(b,Color.clear,0);b.style.overflow=Overflow.Visible;
            var paint=new VisualElement();paint.style.position=Position.Absolute;paint.style.left=-16;paint.style.top=-16;paint.style.right=-16;paint.style.bottom=-16;paint.pickingMode=PickingMode.Ignore;paint.generateVisualContent+=ctx=>DrawSurface(ctx.painter2D,paint.contentRect.width,paint.contentRect.height,primary,16,false);b.Add(paint);
            if(icon!=null){var glyph=new ForestIcon(icon,primary?Navy:White);At(glyph,primary?20:10,20,12,60);b.Add(glyph);}
            var text=Text(label,primary?(label.Length>9?14:19):14,primary?(label.Length>9?2:7):2,primary?Navy:White,true);At(text,icon==null?0:10,0,icon==null?100:85,100);b.Add(text);b.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());b.RegisterCallback<PointerEnterEvent>(e=>b.style.scale=new Scale(new Vector3(1.018f,1.018f,1)));b.RegisterCallback<PointerLeaveEvent>(e=>b.style.scale=new Scale(Vector3.one));return b;
        }
        public static Button Circle(string icon,Action click)
        {
            var b=new Button(click);b.text="";b.style.backgroundColor=Color.clear;UiStyles.Border(b,Color.clear,0);b.style.marginTop=0;b.style.marginBottom=0;b.style.marginLeft=0;b.style.marginRight=0;b.style.paddingLeft=0;b.style.paddingRight=0;b.style.paddingTop=0;b.style.paddingBottom=0;b.generateVisualContent+=ctx=>DrawSurface(ctx.painter2D,b.contentRect.width,b.contentRect.height,false,0,true);
            var glyph=new ForestIcon(icon,White);At(glyph,27,24,46,52);b.Add(glyph);b.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());return b;
        }
        public static Button Arrow(string icon,Action click)
        {
            var b=new Button(click);b.text="";b.style.backgroundColor=Color.clear;b.style.paddingLeft=0;b.style.paddingRight=0;b.style.paddingTop=0;b.style.paddingBottom=0;b.style.marginTop=0;b.style.marginBottom=0;UiStyles.Border(b,Color.clear,0);var glyph=new ForestIcon(icon,White);UiStyles.Absolute(glyph);b.Add(glyph);return b;
        }
        public static void Back(VisualElement parent,Action close){var b=Circle("back",close);At(b,3.5f,4.3f,11.2f,5.2f);parent.Add(b);}
        public static VisualElement Glass(){var e=new VisualElement();e.generateVisualContent+=ctx=>DrawSurface(ctx.painter2D,e.contentRect.width,e.contentRect.height,false,0,false);return e;}
        static void DrawSurface(Painter2D p,float w,float h,bool cream,float inset,bool circle)
        {
            float x=inset,y=inset,bw=w-inset*2,bh=h-inset*2,r=circle?Mathf.Min(bw,bh)/2:cream?bh/2:13;
            if(cream){for(int i=10;i>=1;i--)VectorArt.RoundRect(p,x-i*1.5f,y-i*1.5f,bw+i*3,bh+i*3,r+i*1.5f,new Color(.1f,.9f,1,.038f));VectorArt.RoundRect(p,x,y,bw,bh,r,new Color(.93f,.94f,.87f));for(int i=0;i<12;i++){float t=i/12f;VectorArt.RoundRect(p,x+1+t*2,y+1+t,bw-2-t*4,bh*.75f-t*2,r,new Color(1,.99f,.95f,.035f));}p.strokeColor=new Color(1,1,.96f,.9f);p.lineWidth=1.2f;RoundPath(p,x,y,bw,bh,r);p.Stroke();}
            else if(circle){VectorArt.Circle(p,new Vector2(w/2,h/2),r,new Color(.045f,.22f,.34f,.68f));for(int i=12;i>=1;i--)VectorArt.Circle(p,new Vector2(w*.45f,h*.42f),r*i/12,new Color(.2f,.65f,.85f,.013f));p.BeginPath();p.Arc(new Vector2(w/2,h/2),r-.7f,0,360);p.strokeColor=new Color(.45f,.85f,1,.8f);p.lineWidth=.9f;p.Stroke();}
            else{VectorArt.RoundRect(p,x,y,bw,bh,r,new Color(.025f,.14f,.24f,.76f));for(int i=0;i<10;i++)VectorArt.RoundRect(p,x+1,y+1+i*.5f,bw-2,bh*.6f-i,r,new Color(.25f,.64f,.8f,.011f));RoundPath(p,x+.5f,y+.5f,bw-1,bh-1,r);p.strokeColor=new Color(.3f,.72f,.91f,.7f);p.lineWidth=.8f;p.Stroke();}
        }
        static void RoundPath(Painter2D p,float x,float y,float w,float h,float r){p.BeginPath();p.MoveTo(new Vector2(x+r,y));p.LineTo(new Vector2(x+w-r,y));p.QuadraticCurveTo(new Vector2(x+w,y),new Vector2(x+w,y+r));p.LineTo(new Vector2(x+w,y+h-r));p.QuadraticCurveTo(new Vector2(x+w,y+h),new Vector2(x+w-r,y+h));p.LineTo(new Vector2(x+r,y+h));p.QuadraticCurveTo(new Vector2(x,y+h),new Vector2(x,y+h-r));p.LineTo(new Vector2(x,y+r));p.QuadraticCurveTo(new Vector2(x,y),new Vector2(x+r,y));p.ClosePath();}
    }
    public sealed class ForestIcon:VisualElement
    {
        public ForestIcon(string kind,Color colour){pickingMode=PickingMode.Ignore;generateVisualContent+=ctx=>{
            var p=ctx.painter2D;float s=Mathf.Min(contentRect.width,contentRect.height)/24;var shift=new Vector2((contentRect.width-24*s)/2,(contentRect.height-24*s)/2);p.lineWidth=1.8f*s;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;p.strokeColor=colour;
            Action<Vector2[]> line=pts=>{p.BeginPath();p.MoveTo(pts[0]*s+shift);for(int i=1;i<pts.Length;i++)p.LineTo(pts[i]*s+shift);p.Stroke();};Action<float,float,float> dot=(x,y,r)=>VectorArt.Circle(p,new Vector2(x,y)*s+shift,r*s,colour);
            if(kind=="play"){VectorArt.Polygon(p,new[]{new Vector2(6,2)*s+shift,new Vector2(22,12)*s+shift,new Vector2(6,22)*s+shift},colour);}
            else if(kind=="back"||kind=="next"){float sign=kind=="back"?1:-1;line(new[]{new Vector2(12+sign*4,3),new Vector2(12-sign*4,12),new Vector2(12+sign*4,21)});}
            else if(kind=="person"){dot(12,6,4);VectorArt.RoundRect(p,5*s+shift.x,12*s+shift.y,14*s,10*s,5*s,colour);}
            else if(kind=="rank"){VectorArt.Polygon(p,new[]{new Vector2(7,3)*s+shift,new Vector2(17,3)*s+shift,new Vector2(16,12)*s+shift,new Vector2(12,15)*s+shift,new Vector2(8,12)*s+shift},colour);line(new[]{new Vector2(7,5),new Vector2(3,5),new Vector2(4,11),new Vector2(9,13)});line(new[]{new Vector2(17,5),new Vector2(21,5),new Vector2(20,11),new Vector2(15,13)});line(new[]{new Vector2(12,14),new Vector2(12,21)});line(new[]{new Vector2(7,22),new Vector2(17,22)});}
            else if(kind=="share-graph"){line(new[]{new Vector2(5,12),new Vector2(18,5)});line(new[]{new Vector2(5,12),new Vector2(18,20)});dot(5,12,3);dot(18,5,3);dot(18,20,3);}
            else if(kind=="share"){line(new[]{new Vector2(7,10),new Vector2(4,10),new Vector2(4,23),new Vector2(20,23),new Vector2(20,10),new Vector2(17,10)});line(new[]{new Vector2(12,17),new Vector2(12,2)});line(new[]{new Vector2(7,7),new Vector2(12,2),new Vector2(17,7)});}
            else if(kind=="settings"){var points=new Vector2[48];for(int n=0;n<48;n++){float a=n*Mathf.PI/24,r=n%4<2?10:8;points[n]=new Vector2(12+Mathf.Cos(a)*r,12+Mathf.Sin(a)*r)*s+shift;}VectorArt.Polygon(p,points,colour);VectorArt.Circle(p,new Vector2(12,12)*s+shift,4*s,ForestUi.Navy);}
            else if(kind=="music"){line(new[]{new Vector2(7,17),new Vector2(7,5),new Vector2(19,2),new Vector2(19,15)});line(new[]{new Vector2(7,8),new Vector2(19,5)});dot(4,18,3);dot(16,16,3);}
            else if(kind=="sound"){VectorArt.Polygon(p,new[]{new Vector2(3,9)*s+shift,new Vector2(7,9)*s+shift,new Vector2(13,3)*s+shift,new Vector2(13,21)*s+shift,new Vector2(7,15)*s+shift,new Vector2(3,15)*s+shift},colour);for(int i=0;i<2;i++){p.BeginPath();p.Arc(new Vector2(12,12)*s+shift,(6+i*4)*s,-50,50);p.Stroke();}}
            else if(kind=="haptics"){line(new[]{new Vector2(8,3),new Vector2(16,3),new Vector2(16,21),new Vector2(8,21),new Vector2(8,3)});line(new[]{new Vector2(4,6),new Vector2(2,12),new Vector2(4,18)});line(new[]{new Vector2(20,6),new Vector2(22,12),new Vector2(20,18)});}
            else if(kind=="pause"){VectorArt.RoundRect(p,6*s+shift.x,2*s+shift.y,4*s,20*s,s,colour);VectorArt.RoundRect(p,15*s+shift.x,2*s+shift.y,4*s,20*s,s,colour);}
            else if(kind=="home"){VectorArt.Polygon(p,new[]{new Vector2(2,11)*s+shift,new Vector2(12,2)*s+shift,new Vector2(22,11)*s+shift,new Vector2(19,11)*s+shift,new Vector2(19,22)*s+shift,new Vector2(14,22)*s+shift,new Vector2(14,15)*s+shift,new Vector2(10,15)*s+shift,new Vector2(10,22)*s+shift,new Vector2(5,22)*s+shift,new Vector2(5,11)*s+shift},colour);}
            else if(kind=="retry"){p.BeginPath();p.Arc(new Vector2(12,12)*s+shift,9*s,-110,165);p.Stroke();VectorArt.Polygon(p,new[]{new Vector2(1,8)*s+shift,new Vector2(8,9)*s+shift,new Vector2(4,15)*s+shift},colour);}
            else if(kind=="check"){line(new[]{new Vector2(4,12),new Vector2(10,18),new Vector2(21,5)});}
            else if(kind=="lock"){p.BeginPath();p.Arc(new Vector2(12,10)*s+shift,5*s,180,360);p.Stroke();VectorArt.RoundRect(p,6*s+shift.x,10*s+shift.y,12*s,11*s,2*s,colour);}
            else{line(new[]{new Vector2(2,21),new Vector2(6,14),new Vector2(12,10),new Vector2(21,8)});dot(20,7,3);}
        };}
    }
    public sealed class AvatarView:VisualElement
    {
        AvatarRig rig;readonly Character character;readonly int resolution;public Vector3[] GroundFeet;public bool Walking;public bool Flying;public float Happy;public float Look;public bool Animated=true;
        public AvatarView(Character c,int resolution=384,bool glowingHero=false){
            if(glowingHero){pickingMode=PickingMode.Ignore;generateVisualContent+=ctx=>RobotView.DrawBulb(ctx.painter2D,new Vector2(contentRect.width*.5f,contentRect.height*.5f),Mathf.Min(contentRect.width,contentRect.height)*.23f,RobotView.Tint(c));return;}
            character=c;this.resolution=resolution;pickingMode=PickingMode.Ignore;RegisterCallback<AttachToPanelEvent>(_=>{if(rig!=null)return;rig=new AvatarRig(character,this.resolution);var image=new Image{image=rig.Texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};UiStyles.Absolute(image);Add(image);});RegisterCallback<DetachFromPanelEvent>(_=>{rig?.Dispose();rig=null;Clear();});schedule.Execute(()=>{if(Animated&&rig!=null){rig.GroundFeet=GroundFeet;rig.Render(Time.unscaledTimeAsDouble,Walking,Flying,Happy,false,Look);}}).Every(33);}
    }
}
