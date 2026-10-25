using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class PlatformScaleRegression
    {
        static bool Lands(LevelData level,Character character,int from,int to,double distance,double clock,int bounces)
        {
            var sim=new Simulation(level,character);sim.platform=sim.platforms[from];sim.distance=distance;sim.time=clock;sim.Select(character);sim.startY=sim.bestY=sim.position.y;sim.SetViewport(816);sim.Start();sim.Jump();
            for(int tick=0;tick<360&&sim.flying&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);
            return (sim.phase==Phase.Playing||sim.phase==Phase.Complete)&&!sim.flying&&sim.platform.index==to&&sim.wedgeBounces>=bounces;
        }
        public static void Run(LevelData[] courses)
        {
            var character=CharacterCatalog.Load()[0];int checkedLinks=0;var failures=new System.Collections.Generic.List<string>();
            foreach(var course in courses)
            {
                // Live rain rows have many alternative links at different recycling phases.
                if(course.streamingRain)continue;
                foreach(var link in course.connections)
                {
                    var orbit=new Orbit(course.platforms[link.from].outline);bool success=false;
                    var bounce=course.bounceRuns==null?null:Array.Find(course.bounceRuns,b=>b.entry==link.from&&b.exit==link.to);
                    foreach(double clock in new[]{0.0,5.0,17.0}){
                        if(bounce!=null&&Lands(course,character,link.from,link.to,bounce.launchDistance,clock,bounce.minimumBounces)){success=true;break;}
                        for(int sample=0;sample<360&&!success;sample++){
                            double distance=orbit.length*sample/360;
                            if(bounce==null&&(course.platforms[link.from].motion==null||course.platforms[link.from].motion.type=="static")&&course.platforms[link.from].rotation?.enabled!=true&&course.platforms[link.to].rotation?.enabled!=true){orbit.Pose(distance,out var point,out var normal);bool ray=false;var target=course.platforms[link.to];var motion=target.motion==null?new Point():target.motion.At(clock);for(int edge=0;edge<target.outline.Length&&!ray;edge++)ray=Geometry.Ray(point,normal,target.outline[edge]+motion,target.outline[(edge+1)%target.outline.Length]+motion)>0;if(!ray)continue;}
                            success=Lands(course,character,link.from,link.to,distance,clock,bounce?.minimumBounces??0);
                        }
                        if(success)break;
                    }
                    if(!success)failures.Add(course.id+" / "+link.from+" -> "+link.to);checkedLinks++;
                }
            }
            if(failures.Count>0)throw new Exception("Smaller platforms have unreachable routes ("+failures.Count+"): "+string.Join("; ",failures));
            Debug.Log("MEKTRA_PLATFORM_SCALE_PASS: "+checkedLinks+" non-rain links including physical rebounds remain reachable at 65% size.");
        }
    }
}
