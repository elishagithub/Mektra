using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class CharacterShop:VisualElement
    {
        readonly Character[] characters;readonly ProgressStore store;readonly Action<Character> selected;readonly Action close;int index;
        public CharacterShop(Character[] catalog,ProgressStore store,Action<Character> selected,Action close)
        {
            string[] order={"mektra","tide","ember","aurora","frost","volt","nova","bloom","onyx","comet"};
            characters=catalog.OrderBy(c=>Array.IndexOf(order,c.id)<0?99:Array.IndexOf(order,c.id)).ToArray();this.store=store;this.selected=selected;this.close=close;
            index=Math.Max(0,Array.FindIndex(characters,c=>c.id==store.Data.selected));UiStyles.Absolute(this);focusable=true;RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==KeyCode.LeftArrow)Move(-1);else if(e.keyCode==KeyCode.RightArrow)Move(1);else if(e.keyCode==KeyCode.Escape)close();else if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.Space){if(store.Choose(characters[index])){selected(characters[index]);close();}}else return;e.StopPropagation();});Build();
        }
        void Move(int delta){index=(index+delta+characters.Length)%characters.Length;Build();Focus();}
        public static string Name(Character c)=>c.id=="mektra"?"ORBIT":c.id=="tide"?"RIPPLE":c.name.ToUpperInvariant();
        static string GlowName(Character c)
        {
            switch(c.id){case "tide":return "CYAN";case "ember":return "AMBER";case "aurora":return "VIOLET";case "frost":return "ICE";case "volt":return "LIME";case "nova":return "ROSE";case "bloom":return "BLUSH";case "onyx":return "PERIWINKLE";case "comet":return "MINT";default:return "WHITE";}
        }
        void Build()
        {
            Clear();ForestUi.Background(this);ForestUi.Back(this,close);ForestUi.Header(this,"CHARACTERS");var c=characters[index];bool owned=store.Data.unlocked.Contains(c.id),active=store.Data.selected==c.id;
            var subtitle=ForestUi.Text("CHOOSE YOUR GLOW",11,3);ForestUi.At(subtitle,10,20,80,3);Add(subtitle);
            var preview=new VisualElement{pickingMode=PickingMode.Ignore};ForestUi.At(preview,15,25,70,30);Add(preview);
            preview.generateVisualContent+=ctx=>{
                var p=ctx.painter2D;float w=preview.contentRect.width,h=preview.contentRect.height;
                var glow=RobotView.Tint(c);var center=new Vector2(w*.6f,h*.44f);
                for(int layer=3;layer>=0;layer--){p.BeginPath();p.MoveTo(new Vector2(w*.18f,h*.78f));p.BezierCurveTo(new Vector2(w*.05f,h*.35f),new Vector2(w*.42f,h*.14f),center);p.lineCap=LineCap.Round;p.lineWidth=layer==0?3:7+layer*6;p.strokeColor=new Color(glow.r,glow.g,glow.b,layer==0?.8f:.045f);p.Stroke();}
                foreach(var at in new[]{new Vector2(w*.2f,h*.38f),new Vector2(w*.42f,h*.27f)}){float r=2.2f;p.lineWidth=1.2f;p.strokeColor=glow;p.BeginPath();p.MoveTo(at-new Vector2(r*2,0));p.LineTo(at+new Vector2(r*2,0));p.MoveTo(at-new Vector2(0,r*2));p.LineTo(at+new Vector2(0,r*2));p.Stroke();}
                RobotView.DrawBulb(p,center,Mathf.Min(w,h)*.105f,glow);
            };
            var prev=ForestUi.Arrow("back",()=>Move(-1));ForestUi.At(prev,4,39.5f,10,5.2f);Add(prev);var next=ForestUi.Arrow("next",()=>Move(1));ForestUi.At(next,86,39.5f,10,5.2f);Add(next);
            var pages=ForestUi.Text((index+1)+" / "+characters.Length,12,3);ForestUi.At(pages,30,56.5f,40,3);Add(pages);
            var name=ForestUi.Text(Name(c),27,8);ForestUi.At(name,10,60,80,4);Add(name);
            var tag=ForestUi.Text(GlowName(c)+" · FIREFLY COLLECTOR",10,2);ForestUi.At(tag,8,64,84,3);Add(tag);
            int first=index/5*5;for(int n=0;n<5&&first+n<characters.Length;n++){
                int choice=first+n;var character=characters[choice];var b=ForestUi.Circle("",()=>{index=choice;Build();});ForestUi.At(b,3.5f+n*19,69,17,8.2f);b.Clear();
                b.generateVisualContent+=ctx=>{var p=ctx.painter2D;float w=b.contentRect.width,h=b.contentRect.height,r=Mathf.Min(w,h)/2-1;if(choice==index){p.BeginPath();p.Arc(new Vector2(w/2,h/2),r,0,360);p.strokeColor=ForestUi.Cyan;p.lineWidth=2;p.Stroke();}};
                var avatar=new AvatarView(character,96,true){Animated=false};UiStyles.Absolute(avatar);b.Add(avatar);
                if(!store.Data.unlocked.Contains(character.id)){var lockIcon=new ForestIcon("lock",ForestUi.White);ForestUi.At(lockIcon,57,65,30,30);b.Add(lockIcon);}Add(b);
            }
            string text=active?"SELECTED":owned?"SELECT":"UNLOCK · "+c.cost;
            var use=ForestUi.Button(text,()=>{if(store.Choose(c)){selected(c);close();}},true,"play");ForestUi.At(use,19,81.5f,62,7);use.SetEnabled(owned||store.Data.bank>=c.cost);Add(use);
        }
    }
}
