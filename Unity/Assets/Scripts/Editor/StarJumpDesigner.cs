using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class StarJumpDesigner
    {
        [Serializable] public class RewardRoute
        {
            public int level,from,to,points;public string style;public double launchDistance,flightLength,shiftSeconds;public Point[] stars;
        }
        [Serializable] public class Report {public RewardRoute[] routes;public int levels,validatedCharacters;}
        static Simulation Launch(LevelData level,Character c,int from,double distance)
        {
            var sim=new Simulation(level,c);sim.magnets.Clear();sim.platform=sim.platforms[from];sim.distance=distance;sim.Select(c);sim.SetViewport(816);sim.Start();sim.Jump();return sim;
        }
        static bool Trace(LevelData level,Character c,int from,int to,double distance,out List<Point> points,out List<double> times)
        {
            var sim=Launch(level,c,from,distance);points=new List<Point>{sim.position};times=new List<double>{sim.time};
            for(int i=0;i<360&&sim.flying&&sim.phase==Phase.Playing;i++){sim.Advance(GameConfig.Step);points.Add(sim.position);times.Add(sim.time);}
            return sim.phase==Phase.Playing&&!sim.flying&&sim.platform.section==0&&sim.platform.index==to;
        }
        static bool Valid(LevelData level,Character[] chars,int from,int to,double distance)
        {
            foreach(var c in chars)if(!Trace(level,c,from,to,distance,out _,out _))return false;return true;
        }
        public static void Apply(LevelPack pack,Character[] chars)
        {
            var reports=new List<RewardRoute>();int changed=0;
            foreach(var course in pack.levels)
            {
                var level=course.level;
                // Bounce rewards already follow a measured physical rebound. Keep that special journey intact.
                if(level.segmentKind==11||level.segmentKind==9||level.segmentKind==17||level.segmentKind==18||level.segmentKind==20)continue;
                var rewards=new List<StarData>();var previous=level.stars;level.stars=Array.Empty<StarData>();
                int index=0;
                foreach(var route in course.routes)
                {
                    bool light=level.segmentKind==5||course.number<=2||index%4==0;
                    bool edge=!light&&(index+level.variant)%3!=0;
                    string style=light?"light":edge?"edge / risky":"medium / long";
                    var orbit=new Orbit(level.platforms[route.from].outline);double baseline=CourseReview.RouteDistance(level,route.from,route.distance),chosen=baseline,bestLength=-1,shift=0;
                    // Sample toward the edge of the known launch window, retaining all-character arrival checks.
                    double width=route.windowSeconds>0?route.windowSeconds*GameConfig.OrbitSpeed:orbit.length*.12;
                    foreach(double fraction in light?new[]{0.0}:edge?new[]{-.42,.42,-.32,.32,-.2,.2,0}:new[]{-.22,.22,-.12,.12,0})
                    {
                        double d=(baseline+width*fraction+orbit.length)%orbit.length;
                        if(!Valid(level,chars,route.from,route.to,d))continue;
                        Trace(level,chars[0],route.from,route.to,d,out var trace,out _);double length=0;for(int i=1;i<trace.Count;i++)length+=(trace[i]-trace[i-1]).Length;
                        if(length>bestLength){bestLength=length;chosen=d;shift=Math.Abs(width*fraction)/GameConfig.OrbitSpeed;}
                    }
                    if(bestLength<0){level.stars=previous;throw new Exception("No reward landing route: "+level.id+" / "+route.from+" -> "+route.to);}
                    Trace(level,chars[0],route.from,route.to,chosen,out var positions,out var times);
                    int count=level.segmentKind==1&&route.to==2?3:edge&&bestLength>240?2:1;
                    var lane=new List<StarData>();var coords=new List<Point>();
                    for(int point=0;point<count;point++)
                    {
                        double fraction=count==1?(light?.46:edge?.77:.64):.40+point*.34/(count-1);
                        int sample=Math.Min(positions.Count-2,Math.Max(1,(int)((positions.Count-1)*fraction)));var p=positions[sample];
                        if(p.x<22||p.x>368)continue;
                        string parent=level.segmentKind==9?level.platforms[route.to].id:null;
                        if(level.segmentKind==9&&route.to==level.platforms.Length-1)parent=null;
                        Point local=p;if(parent!=null)local-=level.platforms[route.to].motion.At(times[sample]);
                        lane.Add(new StarData{id="star-"+(rewards.Count+lane.Count+1),x=local.x,y=local.y,parentPlatformId=parent});coords.Add(p);
                    }
                    if(lane.Count==0)throw new Exception("Reward trajectory enters wall: "+level.id);
                    level.stars=lane.ToArray();
                    foreach(var c in chars)
                    {
                        var sim=Launch(level,c,route.from,chosen);for(int i=0;i<360&&sim.flying&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);
                        if(sim.phase!=Phase.Playing||sim.flying||sim.platform.index!=route.to||sim.score<lane.Count)throw new Exception("Unreachable reward: "+level.id+" / "+route.from+" / "+c.id);
                    }
                    rewards.AddRange(lane);reports.Add(new RewardRoute{level=course.number,from=route.from,to=route.to,style=style,points=lane.Count,launchDistance=chosen,flightLength=bestLength,shiftSeconds=shift,stars=coords.ToArray()});index++;
                }
                level.stars=rewards.ToArray();course.points=rewards.Count;MotionRegression.GeometryChecks(level);changed++;
            }
            Directory.CreateDirectory("../assets/levels/endless");File.WriteAllText("../assets/levels/endless/star-jump-review.json",JsonUtility.ToJson(new Report{routes=reports.ToArray(),levels=changed,validatedCharacters=chars.Length},true));
            Debug.Log("MEKTRA_STAR_JUMPS PASS: "+changed+" layouts / "+reports.Count+" reward routes validated across "+chars.Length+" characters; physical wedge trails preserved.");
        }
        public static void Publish()
        {
            var pack=LevelPack.Load();Apply(pack,CharacterCatalog.Load());EndlessRegression.Run(pack);
            File.WriteAllText("Assets/Resources/endless-pack.json",JsonUtility.ToJson(pack,true));File.WriteAllText("../assets/levels/endless/pack.json",JsonUtility.ToJson(pack,true));
            foreach(var course in pack.levels)File.WriteAllText("../assets/levels/endless/Mektra_level_"+course.number+".json",JsonUtility.ToJson(course,true));
            UnityEditor.AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
    }
}
