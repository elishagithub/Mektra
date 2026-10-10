using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    // Sample movement phases with the real collision/scoring simulation, not geometric guesses.
    public static class MotionRegression
    {
        public static Simulation Launch(LevelData[] courses,Character c,int section,int index,double distance,double time)
        {
            var sim=new Simulation(courses[0],c,courses,section);
            sim.platform=sim.platforms.Find(p=>p.section==section&&p.index==index);
            sim.time=time;sim.distance=distance;sim.Select(c);sim.SetViewport(816);sim.Start();sim.Jump();
            for(int i=0;i<240&&sim.flying&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);
            return sim;
        }
        public static bool Arrived(Simulation sim,int section,int index)=>sim.phase==Phase.Playing&&!sim.flying&&sim.platform.section==section&&sim.platform.index==index&&sim.score>0;
        public static double FindRoute(LevelData[] courses,Character[] characters,int section,int from,double time,HashSet<string> reachable=null)
        {
            var orbit=new Orbit(courses[section].platforms[from].outline);var candidates=new List<double>();
            for(int sample=0;sample<240;sample++)
            {
                double d=sample*orbit.length/240;orbit.Pose(d,out var p,out var n);if(n.y<-.2)candidates.Add(d);
            }
            candidates.Sort((a,b)=>{orbit.Pose(a,out var p,out var n);orbit.Pose(b,out var q,out var m);return Math.Abs(n.x).CompareTo(Math.Abs(m.x));});
            double best=-1;
            foreach(double d in candidates)
            {
                var sim=Launch(courses,characters[0],section,from,d,time);if(!Arrived(sim,section,from+1))continue;
                if(reachable!=null)foreach(var star in sim.stars)if(star.section==section&&star.collected)reachable.Add(star.id);
                if(best>=0)continue;
                bool valid=true;
                for(int c=1;c<characters.Length;c++)if(!Arrived(Launch(courses,characters[c],section,from,d,time),section,from+1)){valid=false;break;}
                if(valid){best=d;if(reachable==null)return best;}
            }
            if(best<0)throw new Exception("No safe jump/star route: level "+(section+1)+", platform "+from+", motion time "+time);
            return best;
        }
        public static void GeometryChecks(LevelData level)
        {
            var ids=new HashSet<string>();
            if(level.hazards!=null)foreach(var hazard in level.hazards)
            {
                if(!ids.Add(hazard.id)||hazard.outline==null||hazard.outline.Length<3)throw new Exception("Invalid corridor rail: "+hazard.id);
                foreach(var vertex in hazard.outline)if(double.IsNaN(vertex.x)||double.IsInfinity(vertex.x)||double.IsNaN(vertex.y)||double.IsInfinity(vertex.y)||vertex.x<0||vertex.x>390||vertex.y<0||vertex.y>level.height)throw new Exception("Corridor rail leaves playfield: "+hazard.id);
            }
            if(level.blackHoles!=null)foreach(var hole in level.blackHoles)
            {
                if(!ids.Add(hole.id)||!double.IsFinite(hole.x)||!double.IsFinite(hole.y)||!double.IsFinite(hole.radius)||!double.IsFinite(hole.pullRadius)||hole.radius<=0||hole.pullRadius<=hole.radius||hole.x-hole.pullRadius<0||hole.x+hole.pullRadius>390||hole.y-hole.pullRadius<0||hole.y+hole.pullRadius>level.height)throw new Exception("Invalid black-hole field: "+hole.id);
            }
            if(level.bounceRuns!=null)foreach(var run in level.bounceRuns)
            {
                if(run.entry<0||run.exit<=run.entry||run.exit>=level.platforms.Length||run.points==null||run.points.Length<3||run.secondsPerLeg<=0)throw new Exception("Invalid wedge run");
                foreach(var point in run.points)if(!double.IsFinite(point.x)||!double.IsFinite(point.y)||point.x<25||point.x>365||point.y<0||point.y>level.height)throw new Exception("Bounce waypoint leaves safe playfield");
            }
            foreach(var p in level.platforms)
            {
                if(!ids.Add(p.id))throw new Exception("Duplicate platform ID: "+p.id);
                var m=p.motion;double ax=0,ay=0;
                if(m!=null&&m.type!="static")
                {
                    if(m.type!="horizontal"&&m.type!="vertical"&&m.type!="orbit"&&m.type!="rain")throw new Exception("Unknown motion: "+m.type);
                    foreach(double value in new[]{m.amplitudeX,m.amplitudeY,m.periodSeconds,m.phase})if(double.IsNaN(value)||double.IsInfinity(value))throw new Exception("Non-finite motion setting");
                    if(m.periodSeconds<2)throw new Exception("Motion cycles must be at least 2 seconds");
                    ax=m.type=="vertical"?0:Math.Abs(m.amplitudeX);ay=m.type=="horizontal"?0:Math.Abs(m.amplitudeY);
                }
                if(m?.type=="rain")
                {
                    if(m.amplitudeX<450||m.amplitudeY<0||m.speedVariation<0||m.speedVariation>.9||p.bounds.minY-m.amplitudeY<0||p.bounds.maxY+m.amplitudeY>level.height)throw new Exception("Invalid rain stream: "+p.id);
                    continue; // These carriers deliberately enter and leave the horizontal viewport.
                }
                if(p.sourceShapeId.StartsWith("giant-curve-"))
                {
                    if(p.outline.Length<120||p.bounds.minY-ay<0||p.bounds.maxY+ay>level.height||Math.Max(p.bounds.maxX-p.bounds.minX,p.bounds.maxY-p.bounds.minY)<250)throw new Exception("Invalid giant curve");
                    foreach(var point in p.outline)if(!double.IsFinite(point.x)||!double.IsFinite(point.y))throw new Exception("Non-finite giant curve");
                    continue; // Deliberate offscreen surfaces; player wall collision remains lethal.
                }
                if(p.sourceShapeId=="wedge")
                {
                    if(p.bounds.minX<0||p.bounds.maxX>390||p.bounds.minY<0||p.bounds.maxY>level.height)throw new Exception("Wedge bank leaves its level");
                    continue; // Side banks touch the wall; the automatic route stays inside the corridor.
                }
                if(p.bounds.minX-ax<GameConfig.Radius+2||p.bounds.maxX+ax>390-GameConfig.Radius-2||p.bounds.minY-ay<0||p.bounds.maxY+ay>level.height)throw new Exception("Platform movement enters a wall or leaves its level: "+p.id);
            }
            foreach(var s in level.stars)
            {
                if(!string.IsNullOrEmpty(s.parentPlatformId)&&!ids.Contains(s.parentPlatformId))throw new Exception("Star parent missing: "+s.id);
                var parent=Array.Find(level.platforms,p=>p.id==s.parentPlatformId);var m=parent?.motion;
                double ax=m==null||m.type=="vertical"||m.type=="static"?0:Math.Abs(m.amplitudeX),ay=m==null||m.type=="horizontal"||m.type=="static"?0:Math.Abs(m.amplitudeY);
                if(m?.type=="rain"){if(s.y-m.amplitudeY<0||s.y+m.amplitudeY>level.height)throw new Exception("Rain star leaves vertical bounds");continue;}
                if(s.x-ax<25||s.x+ax>365||s.y-ay<0||s.y+ay>level.height)throw new Exception("Star enters wall danger or leaves the level: "+s.id);
            }
        }
        public static void Run(LevelPack pack)
        {
            var courses=pack.Courses();var characters=CharacterCatalog.Load();int checks=0;
            for(int section=0;section<courses.Length;section++)
            {
                var level=courses[section];GeometryChecks(level);double period=0;
                foreach(var p in level.platforms)if(p.motion!=null&&p.motion.type!="static")period=Math.Max(period,p.motion.periodSeconds);
                if(period==0)continue;
                int moving=Array.FindIndex(level.platforms,p=>p.motion!=null&&p.motion.type!="static");
                var riding=new Simulation(courses[0],characters[0],courses,section);
                riding.platform=riding.platforms.Find(p=>p.section==section&&p.index==moving);riding.distance=0;riding.Select(characters[0]);riding.SetViewport(816);riding.Start();
                for(int frame=0;frame<288;frame++)
                {
                    riding.Advance(1.0/144);riding.platform.orbit.Pose(riding.distance,out var contact,out var normal);
                    var expected=Orbit.Ground(contact,normal,riding.gearAngle,riding.character)+riding.platform.MotionAt(riding.time);expected.y+=riding.platform.offset;
                    if((expected-riding.position).Length>1e-7)throw new Exception("Moving-platform attachment drift");
                }
                riding.Jump();if(Math.Abs(riding.velocity.Length-GameConfig.JumpSpeed)>1e-7)throw new Exception("Platform velocity changed jump speed");
                riding.Pause();var frozen=riding.RenderPosition;double frozenTime=riding.RenderTime;riding.Advance(5);
                if((riding.RenderPosition-frozen).Length>0||riding.RenderTime!=frozenTime)throw new Exception("Paused motion advanced");
                var movement=level.platforms[moving].motion;
                foreach(double t in new[]{0.0,period*.25,period*.5,period})
                {
                    var left=(movement.At(t)-movement.At(t-.0001))*10000;var right=(movement.At(t+.0001)-movement.At(t))*10000;
                    if((left-right).Length>.05)throw new Exception("Platform velocity discontinuity");
                }
                for(int phase=0;phase<12;phase++)foreach(var route in pack.levels[section].routes)
                {
                    double time=phase*period/12;
                    // Keep authored launch angles where possible; find an alternate window otherwise.
                    double distance=route.distance;
                    bool valid=true;foreach(var c in characters)if(!Arrived(Launch(courses,c,section,route.from,distance,time),section,route.to)){valid=false;break;}
                    if(!valid)distance=FindRoute(courses,characters,section,route.from,time);
                    foreach(var c in characters){if(!Arrived(Launch(courses,c,section,route.from,distance,time),section,route.to))throw new Exception("Motion phase route failed");checks++;}
                }
            }
            Debug.Log("MEKTRA_MOTION_VALIDATION PASS: "+checks+" phased jump/star checks; fixed jump speed, moving collision and star attachments.");
        }
    }
}
