using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class CharacterShop:VisualElement
    {
        static Sprite pedestalSprite;
        readonly Character[] characters;readonly ProgressStore store;readonly Action<Character> selected;readonly Action close;int index;
        public CharacterShop(Character[] catalog,ProgressStore store,Action<Character> selected,Action close)
        {
            string[] order={"mektra","tide","ember","aurora","frost","volt","nova","bloom","onyx","comet"};
            characters=catalog.OrderBy(c=>Array.IndexOf(order,c.id)<0?99:Array.IndexOf(order,c.id)).ToArray();this.store=store;this.selected=selected;this.close=close;
            index=Math.Max(0,Array.FindIndex(characters,c=>c.id==store.Data.selected));UiStyles.Absolute(this);focusable=true;RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==KeyCode.LeftArrow)Move(-1);else if(e.keyCode==KeyCode.RightArrow)Move(1);else if(e.keyCode==KeyCode.Escape)close();else if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.Space){if(store.Choose(characters[index])){selected(characters[index]);close();}}else return;e.StopPropagation();});Build();
        }
        void Move(int delta){index=(index+delta+characters.Length)%characters.Length;Build();Focus();}
        public static string Name(Character c)=>c.id=="mektra"?"ORBIT":c.id=="tide"?"RIPPLE":c.name.ToUpperInvariant();
        void Build()
        {
            Clear();ForestUi.Background(this);ForestUi.Back(this,close);ForestUi.Header(this,"CHARACTERS");var c=characters[index];bool owned=store.Data.unlocked.Contains(c.id),active=store.Data.selected==c.id;
            var hero=new AvatarView(c,512){Walking=true};ForestUi.At(hero,30.5f,30,39,19);Add(hero);
            var pedestal=new VisualElement();ForestUi.At(pedestal,22,49.5f,56,7);pedestal.pickingMode=PickingMode.Ignore;pedestal.generateVisualContent+=ctx=>{var p=ctx.painter2D;float w=pedestal.contentRect.width,h=pedestal.contentRect.height;
                VectorArt.RoundRect(p,1,h*.28f,w-2,h*.6f,7,new Color(.005f,.025f,.04f,.97f));
                var pts=new Vector2[97];for(int i=0;i<96;i++){float a=i*Mathf.PI*2/96;pts[i]=new Vector2(w/2+Mathf.Cos(a)*(w/2-1),h*.28f+Mathf.Sin(a)*h*.2f);}pts[96]=pts[0];VectorArt.Polygon(p,pts,new Color(.025f,.14f,.21f));
                for(int j=5;j>=0;j--){p.BeginPath();for(int i=0;i<=96;i++){float a=i*Mathf.PI*2/96;var at=new Vector2(w/2+Mathf.Cos(a)*(w/2-9),h*.26f+Mathf.Sin(a)*h*.17f);if(i==0)p.MoveTo(at);else p.LineTo(at);}p.strokeColor=new Color(.2f,.85f,1,j==0?1:.045f);p.lineWidth=j==0?1.3f:j*2;p.Stroke();}};Add(pedestal);
            var atlas=Resources.Load<Texture2D>("Art/CharacterDesign/character-atlas");if(atlas!=null){if(pedestalSprite==null)pedestalSprite=Sprite.Create(atlas,new Rect(atlas.width*2f/3+atlas.width/90f,atlas.height*.12f,atlas.width/3f-atlas.width/45f,atlas.height*.19f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);var stone=new Image{sprite=pedestalSprite,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};ForestUi.At(stone,22,46.2f,56,11.2f);pedestal.style.display=DisplayStyle.None;Add(stone);}
            var prev=ForestUi.Arrow("back",()=>Move(-1));ForestUi.At(prev,4,39.5f,10,5.2f);Add(prev);var next=ForestUi.Arrow("next",()=>Move(1));ForestUi.At(next,86,39.5f,10,5.2f);Add(next);
            var pages=ForestUi.Text((index+1)+" / "+characters.Length,12,3);ForestUi.At(pages,30,56.5f,40,3);Add(pages);
            var name=ForestUi.Text(Name(c),27,8);ForestUi.At(name,10,60,80,4);Add(name);
            var tag=ForestUi.Text(c.id=="mektra"?"The Original":c.tagline,12,3);ForestUi.At(tag,8,64,84,3);Add(tag);
            int first=index/5*5;for(int n=0;n<5&&first+n<characters.Length;n++){
                int choice=first+n;var character=characters[choice];var b=ForestUi.Circle("",()=>{index=choice;Build();});ForestUi.At(b,3.5f+n*19,69,17,8.2f);b.Clear();
                b.generateVisualContent+=ctx=>{var p=ctx.painter2D;float w=b.contentRect.width,h=b.contentRect.height,r=Mathf.Min(w,h)/2-1;if(choice==index){p.BeginPath();p.Arc(new Vector2(w/2,h/2),r,0,360);p.strokeColor=ForestUi.Cyan;p.lineWidth=2;p.Stroke();}};
                var avatar=new AvatarView(character,96){Animated=false};UiStyles.Absolute(avatar);b.Add(avatar);
                if(!store.Data.unlocked.Contains(character.id)){var lockIcon=new ForestIcon("lock",ForestUi.White);ForestUi.At(lockIcon,57,65,30,30);b.Add(lockIcon);}Add(b);
            }
            string text=owned?"SELECT":"UNLOCK · "+c.cost;
            var use=ForestUi.Button(text,()=>{if(store.Choose(c)){selected(c);close();}},true,"play");ForestUi.At(use,19,81.5f,62,7);use.SetEnabled(owned||store.Data.bank>=c.cost);Add(use);
        }
    }
}
