using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    public static class CampaignWaveRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("Campaign wave: "+message);}
        public static Simulation Shot(LevelData level,Character character,int from,double distance,double clock=0,double gear=0)
        {
            var sim=new Simulation(level,character);sim.time=clock;sim.gearAngle=gear;sim.platform=sim.platforms[from];sim.distance=distance;sim.Select(character);sim.SetViewport(816);sim.energyEnabled=false;sim.Start();sim.Jump();
            for(int i=0;i<480&&sim.flying&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);return sim;
        }
        static bool Arrived(Simulation sim,int target,bool bounce)=>(sim.phase==Phase.Playing||sim.phase==Phase.Complete)&&!sim.flying&&sim.platform.index==target&&(!bounce||sim.wedgeBounces>0);
        public static AuthoredRoute Review(LevelData level,int from,int to,Character[] characters,double clock=0,bool bounce=false)
        {
            int minimumBounces=bounce?Math.Max(1,Array.Find(level.bounceRuns??Array.Empty<BounceRunData>(),b=>b.entry==from&&b.exit==to)?.minimumBounces??1):0;
            var context=new Simulation(level,characters[0]);var source=context.platforms[from];var target=context.platforms[to];var orbit=source.orbit;var valid=new bool[360];
            var center=target.World(new Point((target.data.bounds.minX+target.data.bounds.maxX)*.5,(target.data.bounds.minY+target.data.bounds.maxY)*.5),clock);
            for(int i=0;i<360;i++)
            {
                double distance=orbit.length*i/360;orbit.Pose(distance,out var p,out var n);p=source.World(p,clock);n=source.Direction(n,clock);var axis=center-p;
                if(!bounce&&n.x*axis.x+n.y*axis.y<=0)continue;
                bool safe=true;foreach(var c in characters){var trial=Shot(level,c,from,distance,clock);if(!Arrived(trial,to,bounce)||trial.wedgeBounces<minimumBounces){safe=false;break;}}valid[i]=safe;
            }
            int longest=0,start=0,run=0;for(int i=0;i<720;i++){run=valid[i%360]?Math.Min(360,run+1):0;if(run>longest){longest=run;start=i-run+1;}}
            if(longest==0)throw new Exception("No shared wave route: "+level.name+" "+from+" -> "+to+" at "+clock);
            double selected=((start+(longest-1)*.5)%360)*orbit.length/360;var landed=Shot(level,characters[0],from,selected,clock);
            var launch=new Simulation(level,characters[0]);launch.time=clock;launch.platform=launch.platforms[from];launch.distance=selected;launch.Select(characters[0]);
            return new AuthoredRoute{from=from,to=to,distance=selected,clock=clock,windowSeconds=longest*orbit.length/360/GameConfig.OrbitSpeed,launch=launch.position,landing=landed.position};
        }
        public static AuthoredRoute Opportunity(LevelData level,int from,int to,Character[] characters,bool bounce=false)
        {
            bool bonus=level.platforms[from].label=="bonus"||level.platforms[to].label=="bonus";AuthoredRoute best=null;
            int index=CampaignIdentity.Index(level.id);double required=bonus?CampaignShapeBalance.BonusWindow(index):CampaignShapeBalance.MainWindow(index);
            for(int wait=0;wait<=6;wait+=2){try{var candidate=Review(level,from,to,characters,wait,bounce);if(best==null||candidate.windowSeconds>best.windowSeconds)best=candidate;if(best.windowSeconds>=required)return best;}catch(Exception){}}
            if(best==null)throw new Exception("No bounded-wait campaign route: "+level.name+" / "+from+" -> "+to);if(best.windowSeconds<required)throw new Exception("Campaign launch window below "+required+": "+level.name+" / "+from+" -> "+to+" / "+best.windowSeconds);return best;
        }
        public static List<Point> Trace(LevelData level,Character c,int from,double distance,double clock)
        {
            var sim=new Simulation(level,c);sim.time=clock;sim.platform=sim.platforms[from];sim.distance=distance;sim.Select(c);sim.SetViewport(816);sim.Start();sim.Jump();var points=new List<Point>{sim.position};double next=clock+.04;
            while(sim.flying&&sim.phase==Phase.Playing&&sim.time<clock+4){sim.Advance(GameConfig.Step);if(sim.time>=next){points.Add(sim.position);next+=.04;}}points.Add(sim.position);return points;
        }
        static int DynamicWindows(LevelData level,Character[] characters)
        {
            var groups=new Dictionary<string,List<int>>();
            foreach(var link in level.connections)if(level.platforms[link.from].motion?.type=="rain"||level.platforms[link.to].motion?.type=="rain")
            {
                string key=level.platforms[link.from].motion?.type=="rain"?level.platforms[link.from].label:"entry-"+link.from;
                if(!groups.TryGetValue(key,out var group)){group=new List<int>();groups.Add(key,group);}if(!group.Contains(link.from))group.Add(link.from);
            }
            int count=0;
            foreach(var group in groups.Values)foreach(double clock in new[]{0d,5,17,31})
            {
                int from=group[0];double score=double.PositiveInfinity;
                foreach(int i in group){var p=level.platforms[i];double x=(p.bounds.minX+p.bounds.maxX)*.5+(p.motion==null?0:p.motion.At(clock).x);if(Math.Abs(x-195)<score){from=i;score=Math.Abs(x-195);}}
                var orbit=new Orbit(level.platforms[from].outline);var valid=new bool[180];
                for(int sample=0;sample<180;sample++)
                {
                    orbit.Pose(orbit.length*sample/180,out var p,out var n);if(n.y>=-.05)continue;
                    bool safe=true;foreach(var c in characters){var trial=Shot(level,c,from,orbit.length*sample/180,clock);if((trial.phase!=Phase.Playing&&trial.phase!=Phase.Complete)||trial.flying||!Array.Exists(level.connections,l=>l.from==from&&l.to==trial.platform.index)){safe=false;break;}}valid[sample]=safe;
                }
                int best=0,run=0;for(int i=0;i<360;i++){run=valid[i%180]?Math.Min(180,run+1):0;best=Math.Max(best,run);}
                Check(best*orbit.length/180/GameConfig.OrbitSpeed>=.2,"live rain row choice "+level.name+" / "+from+" / "+clock);count++;
            }
            return count;
        }
        public static void Run(LevelPack pack)
        {
            var chars=CharacterCatalog.Load();int routes=0,phases=0,dynamic=0;double minimum=double.PositiveInfinity;var laterFailures=new List<string>();var courseFailures=new List<string>();
            for(int i=33;i<pack.levels.Length;i++)
            {
                var course=pack.levels[i];var level=course.level;try{Check(level.id==CampaignIdentity.Id(i),"stable level identity");
                Check(level.checkpointPlatforms.Length>=2&&level.checkpointPlatforms.Length<=4&&level.chapterTitles.Length==level.checkpointPlatforms.Length,"checkpointed connected parts");Check(level.stars.Length<=level.checkpointPlatforms.Length*3,"sparse wave rewards");
                foreach(var p in level.platforms)if(p.sourceShapeId!="wedge"&&p.motion?.type!="rain")
                {
                    double cx=(p.bounds.minX+p.bounds.maxX)*.5,rx=(p.bounds.maxX-p.bounds.minX)*.5;
                    if(p.rotation?.enabled==true)foreach(var v in p.outline)rx=Math.Max(rx,(v-new Point(cx,(p.bounds.minY+p.bounds.maxY)*.5)).Length);
                    double travel=p.motion==null?0:Math.Abs(p.motion.amplitudeX);Check(cx-rx-travel>=24&&cx+rx+travel<=366,"orbit clearance: "+level.name+" / "+p.id);
                }
                if(level.streamingRain)dynamic+=DynamicWindows(level,chars);
                foreach(var route in course.routes)
                {
                    bool bounce=level.bounceRuns!=null&&Array.Exists(level.bounceRuns,b=>b.entry==route.from&&b.exit==route.to);
                    bool optional=level.platforms[route.from].label=="bonus"||level.platforms[route.to].label=="bonus";
                    Check(route.windowSeconds>=(optional?CampaignShapeBalance.BonusWindow(i):CampaignShapeBalance.MainWindow(i)),"authored launch window "+level.name+" / "+route.from+" -> "+route.to+" / "+route.windowSeconds.ToString("0.000"));minimum=Math.Min(minimum,route.windowSeconds);routes++;
                    foreach(var c in chars){var flight=Shot(level,c,route.from,route.distance,route.clock);int bounces=bounce?Array.Find(level.bounceRuns,b=>b.entry==route.from&&b.exit==route.to).minimumBounces:0;Check(Arrived(flight,route.to,bounce)&&flight.wedgeBounces>=bounces,"authored route reaches target with its required reflections");}
                    bool moves=level.platforms[route.from].rotation?.enabled==true||level.platforms[route.to].rotation?.enabled==true||level.platforms[route.from].motion?.type!="static"&&level.platforms[route.from].motion!=null||level.platforms[route.to].motion?.type!="static"&&level.platforms[route.to].motion!=null||Array.Exists(level.hazards,h=>h.motion!=null&&h.motion.type!="static")||bounce;
                    if(moves)foreach(double clock in new[]{2d,5,17,31})
                    {
                        double best=0;for(int wait=0;wait<=6;wait+=2){try{best=Math.Max(best,Review(level,route.from,route.to,chars,clock+wait,bounce).windowSeconds);}catch(Exception){}if(best>=.15)break;}if(best<.15){string issue=level.name+" / "+route.from+" -> "+route.to+" from "+clock+" / "+best;laterFailures.Add(issue);Debug.Log("WAVE_WINDOW_REPAIR "+issue);}phases++;
                    }
                }
                foreach(var c in chars){DiscoveryRegression.Traverse(level,c);for(int cp=1;cp<level.checkpointPlatforms.Length;cp++)DiscoveryRegression.Traverse(level,c,level.checkpointPlatforms[cp]);}
                Debug.Log("MEKTRA_CAMPAIGN_VALIDATED "+(i+1)+" "+level.name);
                }catch(Exception ex){string issue=(i+1)+" "+level.name+": "+ex.Message;courseFailures.Add(issue);Debug.Log("MEKTRA_COURSE_REPAIR "+issue);}
            }
            Check(courseFailures.Count==0,"course validation: "+string.Join("; ",courseFailures));
            Check(laterFailures.Count==0,"later readable launch opportunities: "+string.Join("; ",laterFailures));
            var switches=pack.levels[38].level;var fresh=new Simulation(switches,chars[0]);Check(fresh.hazards.TrueForAll(h=>!h.vanished),"linked gates start closed");
            fresh.platform=fresh.platforms[1];fresh.Select(chars[0]);fresh.Start();Check(fresh.hazards[0].vanished&&!fresh.hazards[1].vanished,"landing support opens only its linked gate");
            var resumed=new Simulation(switches,chars[0]);resumed.RestoreCheckpoint(3);Check(resumed.hazards[0].vanished&&!resumed.hazards[1].vanished,"checkpoint restores prior linked gate without opening upcoming gate");
            var reset=new Simulation(switches,chars[0]);Check(!reset.hazards[0].vanished,"restart resets linked mechanism");
            var rotating=new Simulation(pack.levels[39].level,chars[0]);var oval=rotating.platforms[1];
            foreach(double clock in new[]{0d,3,11,27})foreach(var point in oval.data.outline)
            {
                var world=oval.World(point,clock);Check((oval.Local(world,clock)-point).Length<1e-7,"rotation transform inverse");Check(Geometry.Touches(oval.Local(world,clock),oval.data,0),"rotating render vertices agree with collision");
            }
            rotating.time=3;rotating.platform=oval;rotating.distance=oval.orbit.length*.2;rotating.Select(chars[0]);var normal=rotating.normal;rotating.Start();rotating.Jump();Check((rotating.velocity-normal*GameConfig.JumpSpeed).Length<1e-7,"rotating support preserves normal launch and launch speed");
            MovingReflection(pack.levels[34].level,chars[0]);
            var old33=new DiscoveryState{current=32,unlocked=32,completedBits=(1L<<33)-1,best=991,rainLayoutVersion=1};old33.Normalize();Check(old33.current==33&&old33.unlocked==33&&old33.completedIds.Count==33&&old33.best==991&&!old33.Completed(33),"33-level save migration preserves completion without opening false completions");
            var partial33=new DiscoveryState{current=32,unlocked=32,completedBits=(1L<<32)-1,rainLayoutVersion=1};partial33.checkpoints[32]=6;partial33.Normalize();Check(partial33.current==32&&!partial33.Completed(32)&&partial33.checkpointRecords.Exists(r=>r.levelId==CampaignIdentity.Id(32)&&r.platform==6),"in-progress 33rd-level checkpoint migrates by ID");
            var state=new DiscoveryState();state.Complete(62);state.Complete(63);state.Complete(98);state.SetCheckpoint(38,3);
            var saved=JsonUtility.FromJson<DiscoveryState>(JsonUtility.ToJson(state));saved.Normalize();Check(saved.Completed(62)&&saved.Completed(63)&&saved.Completed(98)&&!saved.Completed(0)&&!saved.Completed(31)&&!saved.Completed(32),"99-level completion IDs do not alias old bit positions");
            Check(saved.checkpointRecords.Exists(r=>r.levelId==CampaignIdentity.Id(38)&&r.platform==3),"checkpoint persists by stable level ID");
            Debug.Log("MEKTRA_WAVE_ONE_PASS "+routes+" new fixed routes across "+chars.Length+" characters, "+phases+" later motion windows, "+dynamic+" rain choice windows; min base window "+minimum.ToString("0.000")+"s; all new full and checkpoint-resume runs; linked gates, rotation transforms, moving-frame reflection and 99-ID saves");
        }
        static void MovingReflection(LevelData level,Character c)
        {
            var route=Array.Find(level.connections,l=>l.from==3);var measured=Review(level,3,route.to,new[]{c},2,true);
            var sim=new Simulation(level,c);sim.time=2;sim.platform=sim.platforms[3];sim.distance=measured.distance;sim.Select(c);sim.SetViewport(816);sim.Start();sim.Jump();
            for(int i=0;i<300&&sim.wedgeBounces==0&&sim.phase==Phase.Playing;i++)
            {
                var before=sim.velocity;sim.Advance(GameConfig.Step);if(sim.wedgeBounces==0)continue;
                var bank=sim.platforms[4];var velocity=(bank.MotionAt(sim.time)-bank.MotionAt(sim.time-GameConfig.Step))*(1/GameConfig.Step);var incoming=before-velocity;var outgoing=sim.velocity-velocity;
                Check(Math.Abs(incoming.Length-outgoing.Length)<1e-6,"moving-frame elastic reflection preserves relative speed");Check(Math.Abs(incoming.x*sim.normal.x+incoming.y*sim.normal.y+outgoing.x*sim.normal.x+outgoing.y*sim.normal.y)<1e-6,"moving-frame normal reverses");return;
            }
            Check(false,"moving plate test actually reaches a physical bounce");
        }
    }
}
