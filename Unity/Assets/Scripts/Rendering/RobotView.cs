using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    // Keep the ring and shell meshes cached. GPU transforms move/rotate them;
    // only the tiny LED face repaints during a blink or expression change.
    public class RobotView : VisualElement
    {
        Character character;
        readonly VisualElement ring, shell, face;
        string expression="idle";
        int lastBlink=-1;
        public RobotView(Character c)
        {
            character=c;
            style.position=Position.Absolute;style.width=40;style.height=40;
            usageHints=UsageHints.DynamicTransform;pickingMode=PickingMode.Ignore;
            ring=Layer();ring.usageHints=UsageHints.DynamicTransform;
            ring.generateVisualContent+=ctx=>VectorArt.Teeth(ctx.painter2D,new Vector2(20,20),.556875f,character,0);
            shell=Layer();shell.generateVisualContent+=ctx=>VectorArt.Shell(ctx.painter2D,new Vector2(20,20),.556875f,character);
            face=Layer();face.generateVisualContent+=ctx=>VectorArt.Face(ctx.painter2D,new Vector2(20,20),.556875f,character,expression,Time.unscaledTimeAsDouble);
            Add(ring);Add(shell);Add(face);
        }
        VisualElement Layer()
        {
            var e=new VisualElement();e.style.position=Position.Absolute;e.style.width=40;e.style.height=40;e.pickingMode=PickingMode.Ignore;return e;
        }
        public void Refresh(Simulation sim)
        {
            bool shown=sim.phase!=Phase.Exploding&&sim.phase!=Phase.Over;
            style.display=shown?DisplayStyle.Flex:DisplayStyle.None;
            if(!shown)return;
            if(character!=sim.character){character=sim.character;ring.MarkDirtyRepaint();shell.MarkDirtyRepaint();face.MarkDirtyRepaint();}
            style.scale=new Scale(new Vector3((float)sim.BodyScale,(float)sim.BodyScale,1));
            var p=sim.RenderPosition;
            style.translate=new Translate((float)p.x-20,(float)(p.y-sim.RenderCamera)-20);
            ring.style.rotate=new Rotate((float)(sim.RenderGear*180/System.Math.PI));
            // A restrained landing compression returns smoothly to the crystal-clear body.
            double age=.55-System.Math.Max(0,sim.happyUntil-sim.RenderTime);
            float settle=age>=0&&age<.24?(float)(System.Math.Sin(age/.24*System.Math.PI)*System.Math.Exp(-age*8))*.045f:0;
            shell.style.scale=new Scale(new Vector3(1+settle,1-settle,1));
            face.style.scale=new Scale(new Vector3(1+settle,1-settle,1));
            string next=sim.time<sim.happyUntil?"happy":sim.flying?(sim.position.x<45||sim.position.x>345?"surprised":"focused"):"idle";
            double t=Time.unscaledTimeAsDouble%4.1;
            int blink=t>3.92?(int)((t-3.92)*120):-1;
            if(next!=expression||blink!=lastBlink){expression=next;lastBlink=blink;face.MarkDirtyRepaint();}
        }
    }
}
