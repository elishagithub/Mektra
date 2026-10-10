using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class WorldView:VisualElement
    {
        readonly Simulation sim;
        readonly VisualElement world;
        readonly VisualElement effects;
        readonly VisualElement connections;
        readonly RobotView robot;
        readonly Dictionary<string,VisualElement> shapes=new Dictionary<string,VisualElement>();
        readonly HashSet<string> visibleKeys=new HashSet<string>();
        readonly List<string> removedKeys=new List<string>();
        readonly List<Point> connectionPoints=new List<Point>();
        readonly Dictionary<string,Point[]> guidePaths=new Dictionary<string,Point[]>();
        readonly Dictionary<string,double> guideTimes=new Dictionary<string,double>();
        public WorldView(Simulation sim)
        {
            this.sim=sim;
            style.position=Position.Absolute;
            style.left=0;
            style.top=0;
            style.right=0;
            style.bottom=0;
            style.overflow=Overflow.Hidden;
            pickingMode=PickingMode.Ignore;
            connections=new VisualElement();
            connections.style.position=Position.Absolute;
            connections.style.width=390;
            connections.pickingMode=PickingMode.Ignore;
            connections.generateVisualContent+=DrawConnection;
            Add(connections);
            world=new VisualElement();
            world.style.position=Position.Absolute;
            world.style.width=390;
            world.style.height=1;
            world.usageHints=UsageHints.DynamicTransform;
            Add(world);
            effects=new VisualElement();
            effects.style.position=Position.Absolute;
            effects.style.left=0;
            effects.style.top=0;
            effects.style.width=390;
            effects.style.height=1000;
            effects.pickingMode=PickingMode.Ignore;
            effects.generateVisualContent+=DrawEffects;
            Add(effects);
            robot=new RobotView(sim.character);Add(robot);
        }
        public void Refresh()
        {
            double camera=sim.RenderCamera;
            double origin=sim.platform.offset;
            world.style.translate=new Translate(0,(float)(origin-camera));
            visibleKeys.Clear();
            int count=0;
            foreach(var instance in sim.platforms)
            {
                var data=instance.data;
                if(instance.vanished)continue;
                if(data.bounds.maxY+instance.offset<camera-160||data.bounds.minY+instance.offset>camera+sim.viewport+160)continue;
                if(count++>=24)break;
                visibleKeys.Add(instance.key);
                if(!shapes.TryGetValue(instance.key,out var shape))
                {
                    shape=new VisualElement();
                    shape.usageHints=UsageHints.DynamicTransform;
                    shape.style.position=Position.Absolute;
                    shape.style.left=(float)(instance.mirrored?390-data.bounds.maxX:data.bounds.minX)-2;
                    if(instance.mirrored)shape.style.scale=new Scale(new Vector3(-1,1,1));
                    shape.style.top=(float)(data.bounds.minY+instance.offset-origin)-2;
                    shape.style.width=(float)(data.bounds.maxX-data.bounds.minX)+4;
                    shape.style.height=(float)(data.bounds.maxY-data.bounds.minY)+4;
                    shape.pickingMode=PickingMode.Ignore;
                    var shift=new Vector2((float)-data.bounds.minX+2,(float)-data.bounds.minY+2);
                    shape.generateVisualContent+=ctx=>
                    {
                        var p=ctx.painter2D;
                        VectorArt.Path(p,data.paths,shift);
                        p.fillColor=VectorArt.ColorOf("#03060B");
                        p.Fill(FillRule.OddEven);
                        p.strokeColor=data.sourceShapeId=="wedge"?new Color(.4f,1,.85f,1):data.crumbleSeconds>0?new Color(1,.55f,.25f,1):VectorArt.ColorOf("#03060B");
                        p.lineWidth=data.crumbleSeconds>0||data.sourceShapeId=="wedge"?2:1;
                        p.Stroke();
                    }
                    ;
                    world.Add(shape);
                    shapes.Add(instance.key,shape);
                }
                shape.style.top=(float)(data.bounds.minY+instance.offset-origin)-2;
                var motion=instance.MotionAt(sim.RenderTime);
                shape.style.translate=new Translate((float)motion.x,(float)motion.y);
                bool active=instance==sim.platform;
                if(!(shape.userData is bool was)||was!=active){shape.userData=active;shape.MarkDirtyRepaint();}
            }
            removedKeys.Clear();
            foreach(var pair in shapes)if(!visibleKeys.Contains(pair.Key))
            {
                pair.Value.RemoveFromHierarchy();
                removedKeys.Add(pair.Key);
            }
            foreach(var key in removedKeys)shapes.Remove(key);
            effects.style.height=(float)sim.viewport;
            connections.style.height=(float)sim.viewport;
            connections.MarkDirtyRepaint();
            effects.MarkDirtyRepaint();
            robot.Refresh(sim);
        }
        Vector2 ScreenPoint(Point p)=>new Vector2((float)p.x,(float)(p.y-sim.RenderCamera));
        Point PlatformCenter(PlatformInstance platform)
        {
            var bounds=platform.data.bounds;
            double x=(bounds.minX+bounds.maxX)*.5;
            return new Point(platform.mirrored?390-x:x,(bounds.minY+bounds.maxY)*.5+platform.offset)+platform.MotionAt(sim.RenderTime);
        }
        Point ConnectionEdge(PlatformInstance platform,Point target)
        {
            var motion=platform.MotionAt(sim.RenderTime);
            Point closest=PlatformCenter(platform);double best=double.MaxValue;
            foreach(var vertex in platform.data.outline)
            {
                var point=new Point(platform.mirrored?390-vertex.x:vertex.x,vertex.y+platform.offset)+motion;
                double distance=(point-target).Length;
                if(distance<best){best=distance;closest=point;}
            }
            return closest;
        }
        Point WorldVertex(PlatformInstance platform,Point point)
        {
            return new Point(platform.mirrored?390-point.x:point.x,point.y+platform.offset)+platform.MotionAt(sim.RenderTime);
        }
        Point[] ChooseGuide(PlatformInstance source,PlatformInstance target)
        {
            var start=PlatformCenter(source);var end=PlatformCenter(target);var axis=end-start;
            double squared=axis.x*axis.x+axis.y*axis.y;
            connectionPoints.Clear();
            foreach(var star in sim.stars)
            {
                if(star.section!=source.section&&star.section!=target.section)continue;
                var point=star.At(sim.RenderTime);var relative=point-start;
                double progress=(relative.x*axis.x+relative.y*axis.y)/Math.Max(1,squared);
                if(progress>0&&progress<1)connectionPoints.Add(point);
            }
            Point bestFrom=ConnectionEdge(source,end),bestTo=ConnectionEdge(target,start);
            double bestScore=double.MaxValue;
            // Choose a single outward-normal launch ray that lands on the next shape.
            // Prefer rays passing through the collectible lane; never bend a guide.
            for(int sample=0;sample<360;sample++)
            {
                source.orbit.Pose(sample*source.orbit.length/360,out var position,out var normal);
                position=WorldVertex(source,position);
                if(source.mirrored)normal.x=-normal.x;
                if(normal.x*axis.x+normal.y*axis.y<=0)continue;
                double hit=double.MaxValue;
                var outline=target.data.outline;
                for(int edge=0;edge<outline.Length;edge++)
                {
                    double distance=Geometry.Ray(position,normal,WorldVertex(target,outline[edge]),WorldVertex(target,outline[(edge+1)%outline.Length]));
                    if(distance>0&&distance<hit)hit=distance;
                }
                if(hit==double.MaxValue)continue;
                var landing=position+normal*hit;
                double score=0;
                foreach(var point in connectionPoints)
                {
                    double miss=Geometry.Segment(point,position,landing).distance;
                    score+=miss*miss;
                }
                if(connectionPoints.Count==0)score=hit;
                if(score<bestScore){bestScore=score;bestFrom=position;bestTo=landing;}
            }
            return new[]{bestFrom,bestTo};
        }
        void DrawConnection(MeshGenerationContext ctx)
        {
            if(GameConfig.GuideOpacity(sim.RenderTime)<=0)return;
            foreach(var source in sim.platforms)
            {
                if(source.vanished)continue;
                var course=sim.CourseAt(source.section);
                if(course.connections!=null)
                {
                    foreach(var link in course.connections)if(link.from==source.index)
                    {
                        var target=sim.platforms.Find(p=>p.section==source.section&&p.index==link.to);
                        if(target!=null)GuidePair(ctx,source,target);
                    }
                    if(source.index==course.platforms.Length-1)
                    {
                        var next=sim.platforms.Find(p=>p.section==source.section+1&&p.index==0);if(next!=null)GuidePair(ctx,source,next);
                    }
                }
                else
                {
                    int index=sim.platforms.IndexOf(source);if(index+1<sim.platforms.Count)GuidePair(ctx,source,sim.platforms[index+1]);
                }
            }
            // Bound the cache as endless sections leave the scene.
            removedKeys.Clear();
            foreach(var key in guidePaths.Keys)if(!sim.platforms.Exists(platform=>key.StartsWith(platform.key+"/")))removedKeys.Add(key);
            foreach(var key in removedKeys){guidePaths.Remove(key);guideTimes.Remove(key);}
        }
        void GuidePair(MeshGenerationContext ctx,PlatformInstance source,PlatformInstance target)
        {
            if(target.vanished)return;
            var start=PlatformCenter(source);var end=PlatformCenter(target);
            if(Math.Max(start.y,end.y)<sim.RenderCamera-160||Math.Min(start.y,end.y)>sim.RenderCamera+sim.viewport+160)return;
            string key=source.key+"/"+target.key;
            bool moving=source.data.motion!=null||target.data.motion!=null;
            if(!guidePaths.TryGetValue(key,out var path)||moving&&sim.RenderTime-guideTimes[key]>.25)
            {
                path=ChooseGuide(source,target);guidePaths[key]=path;guideTimes[key]=sim.RenderTime;
            }
            DrawConnectionSegment(ctx.painter2D,path[0],path[1]);
        }
        void DrawConnectionSegment(Painter2D painter,Point from,Point to)
        {
            var delta=to-from;double length=delta.Length;
            if(length<16)return;
            var direction=delta.Normal;
            // Leave room around each collectible so its silhouette stays clear.
            var color=new Color(.72f,.86f,1,.13f*GameConfig.GuideOpacity(sim.RenderTime));
            for(double along=8;along<length-8;along+=9)
                VectorArt.Circle(painter,ScreenPoint(from+direction*along),1.15f,color);
        }
        void DrawLava(Painter2D p)
        {
            if(sim.lava==null||!sim.lava.Active(sim.RenderTime))return;
            float y=(float)(sim.RenderLava-sim.RenderCamera),height=(float)sim.viewport;
            if(y>height+30)return;
            // Keep the bright contact line exact; ripples and glow sit below the lethal boundary.
            float top=Math.Max(y,-30);
            for(int band=7;band>=0;band--)
                VectorArt.RoundRect(p,0,Math.Max(0,top-4-band*5),390,12+band*5,0,new Color(1,.22f,.03f,.015f));
            VectorArt.RoundRect(p,0,top,390,height-top+30,0,new Color(.52f,.055f,.025f,.98f));
            for(int band=0;band<10;band++)
                VectorArt.RoundRect(p,0,top+band*6,390,6,0,new Color(1,.3f-band*.02f,.035f,.65f-band*.045f));
            p.BeginPath();p.MoveTo(new Vector2(0,top));
            for(int i=1;i<=48;i++)
            {
                float x=i*390f/48;
                float ripple=2+(float)(Math.Sin(x*.075+sim.RenderTime*2.3)+Math.Sin(x*.031-sim.RenderTime*1.4))*1.1f;
                p.LineTo(new Vector2(x,top+Math.Max(0,ripple)));
            }
            p.strokeColor=new Color(1,.78f,.22f,.95f);p.lineWidth=2;p.Stroke();
        }
        void DrawEffects(MeshGenerationContext ctx)
        {
            var p=ctx.painter2D;
            DrawLava(p);
            foreach(var hazard in sim.hazards)
            {
                var motion=hazard.MotionAt(sim.RenderTime);
                if(hazard.data.bounds.maxY+hazard.offset+motion.y<sim.RenderCamera||hazard.data.bounds.minY+hazard.offset+motion.y>sim.RenderCamera+sim.viewport)continue;
                VectorArt.Path(p,hazard.data.paths,new Vector2((float)motion.x,(float)(hazard.offset+motion.y-sim.RenderCamera)));
                p.fillColor=VectorArt.ColorOf("#03060B");p.Fill();
                p.strokeColor=new Color(1,.52f,.28f,.8f);p.lineWidth=1.5f;p.Stroke();
            }
            foreach(var hole in sim.blackHoles)
            {
                var center=ScreenPoint(hole.center);if(center.y<-100||center.y>sim.viewport+100)continue;
                float radius=(float)hole.data.radius;
                p.BeginPath();p.Arc(center,(float)hole.data.pullRadius,0,360);p.strokeColor=new Color(.73f,.5f,1,.2f);p.lineWidth=1;p.Stroke();
                VectorArt.Circle(p,center,radius,Color.black);
                for(int arm=0;arm<3;arm++)
                {
                    p.BeginPath();for(int i=0;i<=40;i++)
                    {
                        double t=i/40.0,a=t*7+arm*Math.PI*2/3-sim.RenderTime*1.4;float r=radius*(float)(1-t)*.92f;
                        var point=center+new Vector2((float)Math.Cos(a)*r,(float)Math.Sin(a)*r);if(i==0)p.MoveTo(point);else p.LineTo(point);
                    }
                    p.strokeColor=new Color(.75f,.62f,1,.8f);p.lineWidth=1.2f;p.Stroke();
                }
            }
            foreach(var support in sim.platforms)
            {
                if(support.vanished||support.data.crumbleSeconds<=0||double.IsPositiveInfinity(support.landedAt))continue;
                var center=ScreenPoint(PlatformCenter(support));
                float radius=(float)Math.Max(support.data.bounds.maxX-support.data.bounds.minX,support.data.bounds.maxY-support.data.bounds.minY)*.5f+5;
                int ticks=(int)Math.Ceiling(support.Remaining(sim.RenderTime)*24);
                for(int tick=0;tick<ticks;tick++)
                {
                    float a=-90+tick*15;p.BeginPath();p.Arc(center,radius,a,a+9);p.lineWidth=2;p.strokeColor=new Color(1,.55f,.25f,1);p.Stroke();
                }
            }
            if(sim.energyEnabled)foreach(var bloom in sim.energyBlooms)
            {
                if(bloom.collected||bloom.parent.vanished)continue;
                var center=ScreenPoint(bloom.At(sim.RenderTime));if(center.y<-25||center.y>sim.viewport+25)continue;
                VectorArt.Circle(p,center,12,new Color(.2f,.65f,1,.16f));
                VectorArt.Circle(p,center,7,new Color(.2f,.7f,1,1));
                VectorArt.Circle(p,center,3,new Color(.75f,.95f,1,1));
            }
            var ink=VectorArt.ColorOf("#EAF2FF");
            foreach(var magnet in sim.magnets)
            {
                if(magnet.collected||magnet.position.y<sim.RenderCamera-20||magnet.position.y>sim.RenderCamera+sim.viewport+20)continue;
                var center=ScreenPoint(magnet.position);
                p.BeginPath();p.Arc(center,10,0,360);
                p.strokeColor=Color.white;p.lineWidth=2;p.Stroke();
            }

            // The trail and bursts are rendered by native Unity particle systems.
            int index=0;
            foreach(var star in sim.stars)
            {
                var starPosition=star.At(sim.RenderTime);
                if(star.collected||starPosition.y<sim.RenderCamera-160||starPosition.y>sim.RenderCamera+sim.viewport+160)continue;
                VectorArt.Star(p,ScreenPoint(star.At(sim.RenderTime)),7.7f*(1+.04f*(float)Math.Sin(sim.time*2.8+index)),ink);
                index++;
            }
            if(sim.phase!=Phase.Exploding&&sim.phase!=Phase.Over)
            {
                var center=ScreenPoint(sim.RenderPosition);
                string expression=sim.time<sim.happyUntil?"happy":sim.flying?(sim.position.x<45||sim.position.x>345?"surprised":"focused"):"idle";

                if((sim.phase==Phase.Playing||sim.phase==Phase.Paused)&&GameConfig.GuideOpacity(sim.RenderTime)>0)
                {
                    var aim=sim.flying?sim.velocity.Normal:sim.JumpDirection();
                    for(int i=0;i<6;i++)VectorArt.Circle(p,center+aim.V*(21+i*8.5f),1.5f-i*.18f,new Color(.4f,.72f,1,.55f*GameConfig.GuideOpacity(sim.RenderTime)));
                }
            }
        }
    }
}
