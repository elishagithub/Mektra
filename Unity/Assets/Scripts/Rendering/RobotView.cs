using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class RobotView:VisualElement
    {
        float flightBlend;Character character;
        public RobotView(Character character)
        {
            this.character=character;
            style.position=Position.Absolute;style.width=64;style.height=64;
            usageHints=UsageHints.DynamicTransform;pickingMode=PickingMode.Ignore;
            generateVisualContent+=Draw;
        }
        void Draw(MeshGenerationContext ctx)
        {
            var p=ctx.painter2D;var center=new Vector2(32,32);
            float radius=(float)GameConfig.Radius;
            DrawBulb(p,center,radius,Tint(character));
        }
        public static Color Tint(Character c)=>c==null||c.id=="mektra"?Color.white:AvatarRig.Tint(c.id);
        public static void DrawBulb(Painter2D p,Vector2 center,float radius,Color? tint=null)
        {
            var glow=tint??Color.white;
            // Layered translucent halos keep the white core crisp at every scale.
            for(int i=12;i>=1;i--){float t=i/12f;VectorArt.Circle(p,center,radius*(1+1.75f*t),new Color(glow.r,glow.g,glow.b,.045f*(1-t)+.012f));}
            VectorArt.Circle(p,center,radius,Color.Lerp(glow,Color.white,.5f));
            VectorArt.Circle(p,center,radius*.48f,Color.Lerp(glow,Color.white,.88f));
        }

        public void Refresh(Simulation sim)
        {
            if(character!=sim.character){character=sim.character;MarkDirtyRepaint();}
            bool shown=sim.phase!=Phase.Exploding&&sim.phase!=Phase.Over;
            style.display=shown?DisplayStyle.Flex:DisplayStyle.None;if(!shown)return;
            var normal=sim.flying?sim.velocity.Normal:sim.normal;
            style.rotate=new Rotate(Angle.Degrees((float)(System.Math.Atan2(normal.x,-normal.y)*Mathf.Rad2Deg)));
            if(sim.phase!=Phase.Paused)flightBlend=Mathf.MoveTowards(flightBlend,sim.flying?1:0,Time.unscaledDeltaTime*10);
            float stretch=Mathf.Lerp(1,1.5f,flightBlend);
            float width=Mathf.Lerp(1,.78f,flightBlend);
            style.scale=new Scale(new Vector3((float)sim.BodyScale*width,(float)sim.BodyScale*stretch,1));
            var point=sim.RenderPosition;style.translate=new Translate((float)point.x-32,(float)(point.y-sim.RenderCamera)-32);
        }
    }
}
