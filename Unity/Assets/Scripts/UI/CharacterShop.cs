using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class CharacterShop:VisualElement
    {
        readonly Character[] characters;
        readonly ProgressStore store;
        readonly Action<Character> selected;
        readonly Action close;
        public CharacterShop(Character[] catalog,ProgressStore store,Action<Character> selected,Action close)
        {
            characters=catalog;
            this.store=store;
            this.selected=selected;
            this.close=close;
            UiStyles.Absolute(this);
            style.backgroundColor=UiStyles.Background;
            Build();
        }
        void Build()
        {
            Clear();
            var header=new VisualElement();
            header.style.flexDirection=FlexDirection.Row;
            header.style.alignItems=Align.Center;
            header.style.height=72;
            UiStyles.Pad(header,20);
            var back=UiStyles.Button("‹",close,true);
            back.style.width=42;
            back.style.height=42;
            header.Add(back);
            var brand=UiStyles.Text("mektra",22,null,true);
            brand.style.marginLeft=12;
            brand.style.flexGrow=1;
            header.Add(brand);
            var wallet=UiStyles.Text("✦  "+store.Data.bank,16,null,true);
            UiStyles.Pad(wallet,12);
            wallet.style.backgroundColor=VectorArt.ColorOf("#1B3151");
            UiStyles.Round(wallet,22);
            header.Add(wallet);
            Add(header);
            var scroll=new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow=1;
            scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;
            scroll.verticalScrollerVisibility=ScrollerVisibility.Auto;
            scroll.RegisterCallback<PointerDownEvent>(e=>e.StopPropagation());
            UiStyles.Pad(scroll.contentContainer,20);
            var eyebrow=UiStyles.Text("YOUR LITTLE MECHANICAL CREW",9,UiStyles.Muted,true);
            eyebrow.style.marginBottom=9;
            scroll.Add(eyebrow);
            scroll.Add(UiStyles.Text("Your characters",32,null,true));
            var desc=UiStyles.Text("Collect stars. Find your kind of robot.",13,UiStyles.Muted);
            desc.style.marginTop=8;
            desc.style.marginBottom=22;
            scroll.Add(desc);
            var grid=new VisualElement();
            grid.style.flexDirection=FlexDirection.Row;
            grid.style.flexWrap=Wrap.Wrap;
            grid.style.justifyContent=Justify.SpaceBetween;
            scroll.Add(grid);
            foreach(var c in characters)
            {
                bool active=store.Data.selected==c.id,owned=store.Data.unlocked.Contains(c.id),canBuy=store.Data.bank>=c.cost;
                var card=new VisualElement();
                card.style.width=new Length(48,LengthUnit.Percent);
                card.style.marginBottom=14;
                card.style.backgroundColor=VectorArt.ColorOf("#14243A");
                UiStyles.Round(card,24);
                UiStyles.Pad(card,12);
                UiStyles.Border(card,active?UiStyles.Blue:Color.clear,2);
                var badge=UiStyles.Text(active?"YOUR CHARACTER":owned?"UNLOCKED":c.cost+" STARS",8,active?UiStyles.Blue:UiStyles.Muted,true);
                badge.style.unityTextAlign=TextAnchor.MiddleCenter;
                card.Add(badge);
                card.Add(new RobotPortrait(c,active));
                card.Add(UiStyles.Text(c.name,18,null,true));
                var tagline=UiStyles.Text(c.tagline,10,UiStyles.Muted);
                tagline.style.height=36;
                tagline.style.marginTop=4;
                card.Add(tagline);
                var button=UiStyles.Button(active?"Selected":owned?"Use character":"Unlock · ✦ "+c.cost,()=>
                {
                    if(store.Choose(c))
                    {
                        selected(c);Build();
                    }
                }
                ,!active);
                button.style.height=37;
                button.style.fontSize=11;
                if(active){button.style.backgroundColor=UiStyles.Blue;button.style.color=Color.white;}
                button.SetEnabled(active||owned||canBuy);
                card.Add(button);
                if(!owned&&!canBuy)
                {
                    var missing=UiStyles.Text((c.cost-store.Data.bank)+" more stars to go",8,UiStyles.Muted);
                    missing.style.unityTextAlign=TextAnchor.MiddleCenter;
                    card.Add(missing);
                }
                grid.Add(card);
            }
            Add(scroll);
            var footer=new VisualElement();
            UiStyles.Pad(footer,20);
            footer.style.backgroundColor=UiStyles.Background;
            footer.Add(UiStyles.Button("Ready to roll",close));
            Add(footer);
        }
    }
}
