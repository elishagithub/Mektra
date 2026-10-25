using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public sealed class FourPointStar:VisualElement
    {
        public FourPointStar()
        {
            pickingMode=PickingMode.Ignore;
            style.width=18;style.height=18;style.marginRight=5;
            generateVisualContent+=ctx=>
            {
                var p=ctx.painter2D;
                VectorArt.Circle(p,new Vector2(9,9),5,new Color(.8f,1,.3f,.14f));
                VectorArt.Circle(p,new Vector2(6,5),2,new Color(.9f,1,.8f,.35f));
                VectorArt.Circle(p,new Vector2(12,5),2,new Color(.9f,1,.8f,.35f));
                VectorArt.Circle(p,new Vector2(9,9),2.5f,new Color(.9f,1,.5f));
            };
        }
    }
}
