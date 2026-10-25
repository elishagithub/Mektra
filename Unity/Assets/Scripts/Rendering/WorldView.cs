using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Mektra
{
    public class WorldView:VisualElement
    {
        readonly Simulation sim;
        readonly bool showPlayer;
        struct TrailSample {public Point point;public double time;}
        readonly List<TrailSample> ribbon=new List<TrailSample>();
        sealed class Ripple {public Point[] points;public Point center;public double born;public int tier;}
        readonly List<Ripple> ripples=new List<Ripple>();
        public static readonly Dictionary<string,float> OutlineVisibility=new Dictionary<string,float>();
        int presetVersion=-1;bool lightInitialized;
        readonly Dictionary<string,double> lightPositions=new Dictionary<string,double>();
        readonly List<Point> travelledPath=new List<Point>();
        public int PerfectStreak=>perfectStreak;
        bool observedFlight;int pendingAccuracy,perfectStreak;string accuracyTarget,preparedSource;double accuracyGuideTime;Point[] preparedGuide;
        readonly Dictionary<string,double> lightDepartures=new Dictionary<string,double>();
        readonly Dictionary<string,double> lightArrivals=new Dictionary<string,double>();
        const float ShapePadding=20;
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
        public WorldView(Simulation sim,bool showPlayer=true)
        {
            this.showPlayer=showPlayer;
            this.sim=sim;OutlineVisibility.Clear();
            style.position=Position.Absolute;
            style.left=0;
            style.top=0;
            style.right=0;
            style.bottom=0;
            style.overflow=Overflow.Hidden;
            style.transformOrigin=new TransformOrigin(new Length(50,LengthUnit.Percent),new Length(70,LengthUnit.Percent));
            style.scale=new Scale(new Vector3(GameConfig.ViewZoom,GameConfig.ViewZoom,1));
            pickingMode=PickingMode.Ignore;
            connections=new VisualElement();
            connections.style.position=Position.Absolute;
            connections.style.width=390;
            connections.pickingMode=PickingMode.Ignore;
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
        // UI Toolkit shapes render after the camera. Match the camera's Invert LUT
        // in this preview without changing production artwork or HUD readability.
        static Color PreviewShapeColor(Color color)
        {
return ColorPresetPreview.Apply(color);
        }
        static void DrawLightOutline(Painter2D p,PlatformData shape,Vector2 shift,float reveal,float pulse)
        {
            int n=shape.outline.Length;if(reveal<=0||n<3)return;
            int top=0;for(int i=1;i<n;i++)if(shape.outline[i].y<shape.outline[top].y)top=i;
            for(int part=0;part<3;part++){
                int first=part*n/3,last=(part+1)*n/3;double total=0;
                for(int i=first;i<last;i++)total+=(shape.outline[(top+i+1)%n]-shape.outline[(top+i)%n]).Length;
                float fraction=Mathf.Clamp01(reveal*1.15f-part*.075f);double remaining=total*fraction;Vector2 tip=shape.outline[(top+first)%n].V+shift;
                for(int i=first;i<last&&remaining>0;i++){
                    var from=shape.outline[(top+i)%n];var to=shape.outline[(top+i+1)%n];double length=(to-from).Length,visible=System.Math.Min(length,remaining);int pieces=System.Math.Max(1,(int)System.Math.Ceiling(visible/5));
                    for(int piece=0;piece<pieces;piece++){double t0=visible/ System.Math.Max(.001,length)*piece/pieces,t1=visible/System.Math.Max(.001,length)*(piece+1)/pieces;var a=from+(to-from)*t0;var b=from+(to-from)*t1;var ink=new Color(52f/255f,54f/255f,54f/255f,1);p.lineCap=LineCap.Round;p.BeginPath();p.MoveTo(a.V+shift);p.LineTo(b.V+shift);p.strokeColor=ink;p.lineWidth=2.1f+pulse*.6f;p.Stroke();tip=b.V+shift;}
                    remaining-=visible;
                }
                if(fraction>0&&fraction<.999f)VectorArt.Circle(p,tip,1.5f,new Color(52f/255f,54f/255f,54f/255f,.7f));


            }
        }
        public void Refresh()
        {
            double trailTime=sim.RenderTime;
            ribbon.RemoveAll(s=>trailTime-s.time>1.05);
            if(sim.phase==Phase.Playing&&(ribbon.Count==0||(ribbon[ribbon.Count-1].point-sim.RenderPosition).Length>1.2))ribbon.Add(new TrailSample{point=sim.RenderPosition,time=trailTime});
            if(ribbon.Count>300)ribbon.RemoveRange(0,ribbon.Count-300);

#if MEKTRA_INVERT_PREVIEW
            if(!sim.flying&&!observedFlight&&(preparedSource!=sim.platform.key||sim.RenderTime-accuracyGuideTime>.25)){preparedSource=sim.platform.key;accuracyGuideTime=sim.RenderTime;preparedGuide=null;int targetIndex=sim.platform.index+1;var links=sim.CourseAt(sim.platform.section).connections;if(links!=null)foreach(var link in links)if(link.from==sim.platform.index){targetIndex=link.to;break;}var next=(sim.level.streamingRain?sim.SuggestedTarget(sim.platform,sim.RenderTime):sim.platforms.Find(p=>p.section==sim.platform.section&&p.index==targetIndex))??sim.platforms.Find(p=>p.section==sim.platform.section+1&&p.index==0);if(next!=null){string guideKey=sim.platform.key+"/"+next.key;if(!guidePaths.TryGetValue(guideKey,out preparedGuide)||sim.RenderTime-guideTimes[guideKey]>.25){preparedGuide=ChooseGuide(sim.platform,next);guidePaths[guideKey]=preparedGuide;guideTimes[guideKey]=sim.RenderTime;}accuracyTarget=next.key;}}
            if(sim.perfectChain>perfectStreak){perfectStreak=sim.perfectChain;AddRipple();}else perfectStreak=sim.perfectChain;
            if(sim.flying&&!observedFlight){travelledPath.Clear();travelledPath.Add(sim.RenderPosition);}

            observedFlight=sim.flying;
            if(sim.phase==Phase.Playing&&sim.flying){var now=sim.RenderPosition;if(travelledPath.Count==0)travelledPath.Add(now);var last=travelledPath[travelledPath.Count-1];double length=(now-last).Length;for(double d=8;d<=length;d+=8)travelledPath.Add(last+(now-last)*(d/length));if(travelledPath.Count>2000)travelledPath.RemoveRange(0,travelledPath.Count-2000);}

#endif

            if(presetVersion!=ColorPresetPreview.Version){presetVersion=ColorPresetPreview.Version;foreach(var item in shapes.Values)item.MarkDirtyRepaint();}
            double camera=sim.RenderCamera;
#if MEKTRA_INVERT_PREVIEW
            foreach(var candidate in sim.platforms){double bottom=candidate.data.bounds.maxY+candidate.offset+candidate.MotionAt(sim.RenderTime).y-camera;
                if(!lightInitialized&&bottom>0&&candidate.data.bounds.minY+candidate.offset-camera<sim.viewport)lightArrivals[candidate.key]=Time.unscaledTimeAsDouble-3;
                if(lightInitialized&&lightPositions.TryGetValue(candidate.key,out var previous)&&previous<=0&&bottom>0&&!lightArrivals.ContainsKey(candidate.key))lightArrivals[candidate.key]=Time.unscaledTimeAsDouble;
                lightPositions[candidate.key]=bottom;
            }
            lightInitialized=true;
#endif
            double origin=sim.platform.offset;
            world.style.translate=new Translate(0,(float)(origin-camera));
            visibleKeys.Clear();
            int count=0;
            foreach(var instance in sim.platforms)
            {
                var data=instance.data;
                if(instance.vanished)continue;
                if(data.motion?.type=="rain"){var drift=instance.MotionAt(sim.RenderTime);if(data.bounds.maxX+drift.x<0||data.bounds.minX+drift.x>390)continue;}
                double overscan=sim.CourseAt(instance.section).streamingRain?40:160;
                if(data.bounds.maxY+instance.offset<camera-overscan||data.bounds.minY+instance.offset>camera+sim.viewport+overscan)continue;
                if(count++>=24)break;
                visibleKeys.Add(instance.key);
                if(!shapes.TryGetValue(instance.key,out var shape))
                {
                    shape=new VisualElement();
                    shape.usageHints=UsageHints.DynamicTransform;
                    shape.style.position=Position.Absolute;
                    shape.style.left=(float)(instance.mirrored?390-data.bounds.maxX:data.bounds.minX)-ShapePadding;
                    if(instance.mirrored)shape.style.scale=new Scale(new Vector3(-1,1,1));
                    shape.style.top=(float)(data.bounds.minY+instance.offset-origin)-ShapePadding;
                    shape.style.width=(float)(data.bounds.maxX-data.bounds.minX)+ShapePadding*2;
                    shape.style.height=(float)(data.bounds.maxY-data.bounds.minY)+ShapePadding*2;
                    shape.pickingMode=PickingMode.Ignore;
                    var shift=new Vector2((float)-data.bounds.minX+ShapePadding,(float)-data.bounds.minY+ShapePadding);
                    shape.generateVisualContent+=ctx=>
                    {
                        var p=ctx.painter2D;
                        // Soft solid silhouettes: 20% less opaque than the former .75 outlines.
                        VectorArt.Path(p,data.paths,shift);
                        p.fillColor=Color.black;
                        p.Fill();
                        OutlineVisibility[instance.key]=1;

                    }
                    ;
                    world.Add(shape);
                    shapes.Add(instance.key,shape);
                }
                shape.style.top=(float)(data.bounds.minY+instance.offset-origin)-ShapePadding;
                var motion=instance.MotionAt(sim.RenderTime);
                shape.style.translate=new Translate((float)motion.x,(float)motion.y);
                if(instance.Rotates)shape.style.rotate=new Rotate(Angle.Radians((float)instance.AngleAt(sim.RenderTime)));
#if MEKTRA_INVERT_PREVIEW
                shape.MarkDirtyRepaint();
#endif
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
            if(!showPlayer)robot.style.display=DisplayStyle.None;
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
                var point=platform.World(vertex,sim.RenderTime);
                double distance=(point-target).Length;
                if(distance<best){best=distance;closest=point;}
            }
            return closest;
        }
        Point WorldVertex(PlatformInstance platform,Point point)
        {
            return platform.World(point,sim.RenderTime);
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
                normal=source.Direction(normal,sim.RenderTime);
                if(normal.x*axis.x+normal.y*axis.y<=0)continue;
                double hit=double.MaxValue;
                var outline=target.data.outline;
                for(int edge=0;edge<outline.Length;edge++)
                {
                    double distance=Geometry.Ray(position,normal,WorldVertex(target,outline[edge]),WorldVertex(target,outline[(edge+1)%outline.Length]));
                    if(distance>0&&distance<hit)hit=distance;
                }
                if(hit==double.MaxValue)continue;
                bool blocked=false;
                foreach(var hazard in sim.hazards){
                    if(hazard.vanished)continue;
                    var shift=new Point(0,hazard.offset)+hazard.MotionAt(sim.RenderTime);var boundary=hazard.data.outline;
                    for(int edge=0;edge<boundary.Length;edge++){double crossing=Geometry.Ray(position,normal,boundary[edge]+shift,boundary[(edge+1)%boundary.Length]+shift);if(crossing>0&&crossing<hit){blocked=true;break;}}
                    if(blocked)break;
                }
                if(blocked)continue;
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

            foreach(var source in sim.platforms)
            {
                if(source.vanished)continue;
                var course=sim.CourseAt(source.section);
                if(course.streamingRain)
                {
                    var center=PlatformCenter(source);
                    if(center.x>=24&&center.x<=366&&center.y>=sim.RenderCamera-120&&center.y<=sim.RenderCamera+sim.viewport+120)
                    {
                        var target=sim.SuggestedTarget(source,sim.RenderTime);if(target!=null)GuidePair(ctx,source,target);
                    }
                    continue;
                }
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
        static bool HasMotion(MotionData motion)
        {
            if(motion==null||motion.type=="static"||motion.periodSeconds<=0)return false;
            double x=motion.type=="vertical"?0:motion.amplitudeX;
            double y=motion.type=="horizontal"?0:motion.amplitudeY;
            return Math.Abs(x)>.001||Math.Abs(y)>.001;
        }
        void GuidePair(MeshGenerationContext ctx,PlatformInstance source,PlatformInstance target)
        {
            if(target.vanished)return;
            var start=PlatformCenter(source);var end=PlatformCenter(target);
            if(Math.Max(start.y,end.y)<sim.RenderCamera-160||Math.Min(start.y,end.y)>sim.RenderCamera+sim.viewport+160)return;
            if(sim.CourseAt(source.section).bounceRuns!=null)foreach(var bounce in sim.CourseAt(source.section).bounceRuns)
                if(bounce.entry==source.index&&bounce.exit==target.index&&bounce.points!=null)
                {
                    var points=bounce.points;
                    if(bounce.livePreview)
                    {
                        string liveKey=source.key+"/bounce-"+target.key;
                        if(!guidePaths.TryGetValue(liveKey,out points)||sim.RenderTime-guideTimes[liveKey]>.35)
                        {
                            var probe=new Simulation(sim.CourseAt(source.section),sim.character);probe.time=sim.RenderTime;probe.platform=probe.platforms[source.index];probe.distance=bounce.launchDistance;probe.gearAngle=sim.gearAngle;probe.Select(sim.character);probe.SetViewport(816);probe.Start();probe.Jump();
                            var trace=new List<Point>{probe.position};
                            for(int tick=0;tick<360&&probe.flying&&probe.phase==Phase.Playing;tick++){probe.Advance(GameConfig.Step);if(tick%4==0)trace.Add(probe.position);}
                            trace.Add(probe.position);points=trace.ToArray();guidePaths[liveKey]=points;guideTimes[liveKey]=sim.RenderTime;
                        }
                    }
                    for(int i=1;i<points.Length;i++)DrawConnectionSegment(ctx.painter2D,points[i-1]+new Point(0,source.offset),points[i]+new Point(0,source.offset));
                    return;
                }
            string key=source.key+"/"+target.key;
            bool moving=source.Rotates||target.Rotates||HasMotion(source.data.motion)||HasMotion(target.data.motion);
            if(!guidePaths.TryGetValue(key,out var path)||moving&&sim.RenderTime-guideTimes[key]>.25)
            {
                path=ChooseGuide(source,target);guidePaths[key]=path;guideTimes[key]=sim.RenderTime;
            }
            foreach(var collectible in sim.stars){if(collectible.collected||collectible.attracted||collectible.section!=source.section||collectible.guideFrom!=source.index||collectible.guideTo!=target.index)continue;var at=path[0]+(path[1]-path[0])*collectible.guideFraction;collectible.position=at-(collectible.parent==null?new Point():collectible.parent.MotionAt(sim.RenderTime));}
            // Cached moving routes refresh above; every visible connection keeps its dots.
            {
                var backward=(path[0]-path[1]).Normal;double edgeDistance=double.MaxValue;
                var outline=source.data.outline;
                for(int i=0;i<outline.Length;i++){double hit=Geometry.Ray(path[0],backward,WorldVertex(source,outline[i]),WorldVertex(source,outline[(i+1)%outline.Length]));if(hit>0&&hit<edgeDistance)edgeDistance=hit;}
                var edgeStart=edgeDistance==double.MaxValue?path[0]:path[0]+backward*edgeDistance;
                DrawConnectionSegment(ctx.painter2D,edgeStart,path[1]);
            }
        }
        void DrawConnectionSegment(Painter2D painter,Point from,Point to)
        {
            var delta=to-from;double length=delta.Length;
            if(length<16)return;
            var direction=delta.Normal;
            // Leave room around each collectible so its silhouette stays clear.
            var color=new Color(.55f,.55f,.55f,.4375f);
            for(double along=0;along<length;along+=9)
                VectorArt.Circle(painter,ScreenPoint(from+direction*along),.805f,color);
            VectorArt.Circle(painter,ScreenPoint(to),.805f,color);
        }
        void DrawLava(Painter2D p)
        {
            if(!sim.LavaEnabled||sim.lava==null||!sim.lava.Active(sim.RenderTime))return;
            float y=(float)(sim.RenderLava-sim.RenderCamera),height=(float)sim.viewport;
            if(y>height+30)return;
            // Keep the bright contact line exact; ripples and glow sit below the lethal boundary.
            float top=Math.Max(y,-30);
            for(int band=7;band>=0;band--)
                VectorArt.RoundRect(p,0,Math.Max(0,top-4-band*5),390,12+band*5,0,new Color(0.3721f,0.3721f,0.3721f,.015f));
            VectorArt.RoundRect(p,0,top,390,height-top+30,0,new Color(0.1517f,0.1517f,0.1517f,.98f));
            for(int band=0;band<10;band++)
                VectorArt.RoundRect(p,0,top+band*6,390,6,0,new Color(.55f,.55f,.55f,.65f-band*.045f));
            p.BeginPath();p.MoveTo(new Vector2(0,top));
            for(int i=1;i<=48;i++)
            {
                float x=i*390f/48;
                float ripple=2+(float)(Math.Sin(x*.075+sim.RenderTime*2.3)+Math.Sin(x*.031-sim.RenderTime*1.4))*1.1f;
                p.LineTo(new Vector2(x,top+Math.Max(0,ripple)));
            }
            p.strokeColor=new Color(0.7863f,0.7863f,0.7863f,.95f);p.lineWidth=2;p.Stroke();
        }
        void AddRipple()
        {
            var platform=sim.platform;var outline=platform.data.outline;var motion=platform.MotionAt(sim.RenderTime);var points=new Point[outline.Length];
            for(int i=0;i<points.Length;i++)points[i]=platform.World(outline[i],sim.RenderTime);
            var bounds=platform.data.bounds;var center=new Point((bounds.minX+bounds.maxX)*.5,(bounds.minY+bounds.maxY)*.5+platform.offset)+motion;
            if(platform.mirrored)center.x=390-(bounds.minX+bounds.maxX)*.5+motion.x;
            ripples.Add(new Ripple{points=points,center=center,born=Time.unscaledTimeAsDouble,tier=perfectStreak>=10?3:perfectStreak>=5?2:perfectStreak>=3?1:0});
        }
        void DrawRipples(Painter2D p)
        {
            double now=Time.unscaledTimeAsDouble; ripples.RemoveAll(r=>now-r.born>2.2);
            foreach(var ripple in ripples)for(int wave=0;wave<5+ripple.tier;wave++){
                float age=(float)(now-ripple.born)-wave*.11f;if(age<0||age>1.05f)continue;
                float t=age/1.05f,scale=1+(.16f+ripple.tier*.025f)*(1-Mathf.Pow(1-t,2));
                p.BeginPath();for(int i=0;i<ripple.points.Length;i++){var point=ScreenPoint(ripple.center+(ripple.points[i]-ripple.center)*scale);if(i==0)p.MoveTo(point);else p.LineTo(point);}p.ClosePath();
                p.lineJoin=LineJoin.Round;p.lineCap=LineCap.Round;p.lineWidth=2.2f+ripple.tier*.15f;p.strokeColor=new Color(1.0000f,1.0000f,1.0000f,(1-t)*(wave==0?.95f:.7f));p.Stroke();
            }
        }
        void DrawPlayerRibbon(MeshGenerationContext ctx)
        {
            if(ribbon.Count<2)return;
            var p=ctx.painter2D;var head=ScreenPoint(sim.RenderPosition);float radius=(float)(GameConfig.Radius*sim.BodyScale);
            Vector2 NormalAt(int index){var before=ScreenPoint(ribbon[Math.Max(0,index-1)].point);var after=ScreenPoint(ribbon[Math.Min(ribbon.Count-1,index+1)].point);var tangent=(after-before).normalized;return new Vector2(-tangent.y,tangent.x);}
            for(int i=1;i<ribbon.Count;i++){
                var from=ScreenPoint(ribbon[i-1].point);var to=ScreenPoint(ribbon[i].point);
                float d0=(from-head).magnitude,d1=(to-head).magnitude;
                if(d0<radius&&d1<radius)continue;
                if(d1<radius){var delta=to-from;var relative=from-head;float aa=Vector2.Dot(delta,delta),bb=2*Vector2.Dot(relative,delta),cc=Vector2.Dot(relative,relative)-radius*radius;float disc=bb*bb-4*aa*cc;if(aa>.0001f&&disc>=0)to=from+delta*Mathf.Clamp01((-bb-Mathf.Sqrt(disc))/(2*aa));else continue;}
                if(d0<radius)continue;
                float life0=Mathf.Clamp01(1-(float)(sim.RenderTime-ribbon[i-1].time)/1.05f),life1=Mathf.Clamp01(1-(float)(sim.RenderTime-ribbon[i].time)/1.05f);
                float width0=i==1?0:1.25f*life0,width1=1.25f*life1;
                var n0=NormalAt(i-1);var n1=NormalAt(i);
                p.BeginPath();p.MoveTo(from+n0*width0);p.LineTo(to+n1*width1);p.LineTo(to-n1*width1);p.LineTo(from-n0*width0);p.ClosePath();
                p.fillColor=ColorPresetPreview.Apply(new Color(.15f,.55f,1f,.85f*Mathf.Pow((life0+life1)*.5f,.85f)));p.Fill();
            }
        }
        void DrawEffects(MeshGenerationContext ctx)
        {
            if(showPlayer)DrawPlayerRibbon(ctx);
            var p=ctx.painter2D;
            DrawLava(p);
            DrawRipples(p);
            if(sim.sequence==null&&sim.level.checkpointPlatforms!=null)foreach(int checkpointIndex in sim.level.checkpointPlatforms)
            {
                var support=sim.platforms.Find(s=>s.section==0&&s.index==checkpointIndex);if(support==null)continue;
                var center=ScreenPoint(PlatformCenter(support));if(center.y<-100||center.y>sim.viewport+100)continue;
                float radius=(float)((support.data.bounds.maxX-support.data.bounds.minX)*.5+7);
                for(int tick=0;tick<12;tick++){p.BeginPath();p.Arc(center,radius,tick*30,tick*30+18);p.strokeColor=new Color(.65f,.65f,.65f,.55f);p.lineWidth=1;p.Stroke();}
            }
#if MEKTRA_INVERT_PREVIEW
#endif
            foreach(var support in sim.platforms)if(support.data.switchSupport)
            {
                var center=ScreenPoint(PlatformCenter(support));if(center.y<-100||center.y>sim.viewport+100)continue;
                p.BeginPath();p.MoveTo(center+new Vector2(-5,-5));p.LineTo(center+new Vector2(5,-5));p.LineTo(center+new Vector2(5,5));p.LineTo(center+new Vector2(-5,5));p.ClosePath();p.strokeColor=new Color(.8f,.8f,.8f,.8f);p.lineWidth=1.5f;p.Stroke();
                foreach(var gate in sim.hazards)if(!gate.vanished&&gate.data.openedBy==support.data.id)
                {
                    var end=ScreenPoint(PlatformCenter(gate));p.BeginPath();p.MoveTo(center);p.LineTo(end);p.strokeColor=new Color(.6f,.6f,.6f,.22f);p.lineWidth=1;p.Stroke();
                }
            }
            foreach(var hazard in sim.hazards)
            {
                if(hazard.vanished)continue;
                var motion=hazard.MotionAt(sim.RenderTime);
                if(hazard.data.bounds.maxY+hazard.offset+motion.y<sim.RenderCamera||hazard.data.bounds.minY+hazard.offset+motion.y>sim.RenderCamera+sim.viewport)continue;
                var hazardShift=new Vector2((float)motion.x,(float)(hazard.offset+motion.y-sim.RenderCamera));
                VectorArt.Path(p,hazard.data.paths,hazardShift);
                p.fillColor=VectorArt.ColorOf("#060606");p.Fill();
                p.strokeColor=new Color(0.3444f,0.3444f,0.3444f,.95f);p.lineWidth=1.8f;p.Stroke();
                var edgePoints=hazard.data.outline;
                var hazardCenter=new Point((hazard.data.bounds.minX+hazard.data.bounds.maxX)*.5,(hazard.data.bounds.minY+hazard.data.bounds.maxY)*.5);
                double spacing=0;
                for(int edge=0;edge<edgePoints.Length;edge++){
                    var a=edgePoints[edge];var b=edgePoints[(edge+1)%edgePoints.Length];double length=(b-a).Length;var tangent=(b-a).Normal;
                    for(double d=spacing;d<length;d+=18){
                        var middle=a+tangent*d;var normal=new Point(-tangent.y,tangent.x);
                        if(normal.x*(middle.x-hazardCenter.x)+normal.y*(middle.y-hazardCenter.y)<0)normal=normal*-1;
                        if(sim.level.discovery){p.BeginPath();p.MoveTo((middle-tangent*3-normal*3).V+hazardShift);p.LineTo((middle+tangent*3+normal*3).V+hazardShift);p.strokeColor=new Color(.65f,.65f,.65f,.7f);p.lineWidth=1;p.Stroke();continue;}
                        // Each spike follows the outward normal of the surface.
                        var tip=middle+normal*6;var basePoint=middle;
                        VectorArt.Polygon(p,new[]{(basePoint-tangent*4).V+hazardShift,tip.V+hazardShift,(basePoint+tangent*4).V+hazardShift},Color.black);
                    }
                    spacing=(spacing-length)%18;if(spacing<0)spacing+=18;
                }

            }
            foreach(var hole in sim.blackHoles)
            {
                var center=ScreenPoint(hole.center);if(center.y<-100||center.y>sim.viewport+100)continue;
                float radius=(float)hole.data.radius;
                p.BeginPath();p.Arc(center,(float)hole.data.pullRadius,0,360);p.strokeColor=new Color(0.5850f,0.5850f,0.5850f,sim.level.discovery?.7f:.2f);p.lineWidth=1;p.Stroke();
                if(sim.level.discovery){p.BeginPath();p.Arc(center,(float)hole.data.pullRadius-4,0,360);p.strokeColor=new Color(.65f,.65f,.65f,.3f);p.lineWidth=1;p.Stroke();}
                VectorArt.Circle(p,center,radius,Color.black);
                for(int arm=0;arm<3;arm++)
                {
                    p.BeginPath();for(int i=0;i<=40;i++)
                    {
                        double t=i/40.0,a=t*7+arm*Math.PI*2/3-sim.RenderTime*1.4;float r=radius*(float)(1-t)*.92f;
                        var point=center+new Vector2((float)Math.Cos(a)*r,(float)Math.Sin(a)*r);if(i==0)p.MoveTo(point);else p.LineTo(point);
                    }
                    p.strokeColor=new Color(0.6751f,0.6751f,0.6751f,.8f);p.lineWidth=1.2f;p.Stroke();
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
                    float a=-90+tick*15;p.BeginPath();p.Arc(center,radius,a,a+9);p.lineWidth=2;p.strokeColor=new Color(0.6240f,0.6240f,0.6240f,1);p.Stroke();
                }
            }
            if(sim.energyEnabled)foreach(var bloom in sim.energyBlooms)
            {
                if(bloom.collected||bloom.parent.vanished)continue;
                var center=ScreenPoint(bloom.At(sim.RenderTime));if(center.y<-25||center.y>sim.viewport+25)continue;
                VectorArt.Circle(p,center,12,new Color(0.5796f,0.5796f,0.5796f,.16f));
                VectorArt.Circle(p,center,7,new Color(0.6154f,0.6154f,0.6154f,1));
                VectorArt.Circle(p,center,3,new Color(0.9111f,0.9111f,0.9111f,1));
            }
            var ink=VectorArt.ColorOf("#F1F1F1");
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
                var at=ScreenPoint(star.At(sim.RenderTime));
                float pulse=.82f+.18f*(float)Math.Sin(sim.RenderTime*2.4+index*1.73);
                // Wings and a tiny dark thorax sit around the luminous collectible abdomen.
                for(int halo=8;halo>=1;halo--){float q=halo/8f;VectorArt.Circle(p,at,(5+q*13)*pulse,ColorPresetPreview.Apply(new Color(.8f,1f,.19f,(1-q)*.025f)));}
                float flutter=.5f+.5f*(float)Math.Sin(sim.RenderTime*27+index*2.1);
                var wing=ColorPresetPreview.Apply(new Color(.86f,1,.8f,.22f+.16f*flutter));
                VectorArt.Circle(p,at+new Vector2(-3.4f,-3.5f),1.5f+flutter,wing);
                VectorArt.Circle(p,at+new Vector2(3.4f,-3.5f),1.5f+flutter,wing);
                VectorArt.Circle(p,at+new Vector2(0,-3.2f),1.7f,new Color(.025f,.04f,.025f,1));
                VectorArt.Circle(p,at,4.4f*pulse,ColorPresetPreview.Apply(new Color(.78f,1,.24f,.94f)));
                VectorArt.Circle(p,at,2.3f*pulse,ColorPresetPreview.Apply(new Color(1,1,.72f,1)));
                index++;
            }
            if(showPlayer&&sim.phase!=Phase.Exploding&&sim.phase!=Phase.Over)
            {
                var center=ScreenPoint(sim.RenderPosition);
                string expression=sim.time<sim.happyUntil?"happy":sim.flying?(sim.position.x<45||sim.position.x>345?"surprised":"focused"):"idle";

                if(sim.time<7&&!sim.flying&&(sim.phase==Phase.Playing||sim.phase==Phase.Paused)&&PlayerPrefs.GetInt("mektra.aimGuide",1)==1)
                {
                    var aim=sim.flying?sim.velocity.Normal:sim.JumpDirection();
                    for(float distance=11;distance<=29;distance+=6)
                        VectorArt.Circle(p,center+aim.V*distance,.805f,new Color(.55f,.55f,.55f,.4375f));
                }
            }
        }
    }
}
