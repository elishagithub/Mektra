using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
namespace Mektra.Editor
{
    public static class DiscoveryRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("Discovery: "+message);}
        static bool Equivalent(JToken a,JToken b)
        {
            if(a.Type==JTokenType.Float||a.Type==JTokenType.Integer)
                return (b.Type==JTokenType.Float||b.Type==JTokenType.Integer)&&Math.Abs(a.Value<double>()-b.Value<double>())<=1e-9;
            if(a.Type!=b.Type)return false;
            if(a is JObject ao&&b is JObject bo)
            {
                if(ao.Count!=bo.Count)return false;
                foreach(var property in ao.Properties())if(bo[property.Name]==null||!Equivalent(property.Value,bo[property.Name]))return false;
                return true;
            }
            if(a is JArray aa&&b is JArray ba){if(aa.Count!=ba.Count)return false;for(int i=0;i<aa.Count;i++)if(!Equivalent(aa[i],ba[i]))return false;return true;}
            return JToken.DeepEquals(a,b);
        }
        static Simulation Shot(LevelData level,Character character,int from,double distance,double clock,double gear=0)
        {
            var sim=new Simulation(level,character);sim.time=clock;sim.platform=sim.platforms[from];sim.distance=distance;sim.gearAngle=gear;sim.Select(character);sim.SetViewport(816);sim.energyEnabled=false;sim.Start();sim.Jump();
            for(int tick=0;tick<480&&sim.flying&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);
            return sim;
        }
        public static void Traverse(LevelData level,Character character,int checkpoint=0)
        {
            var sim=new Simulation(level,character);sim.RestoreCheckpoint(checkpoint);sim.SetViewport(816);sim.energyEnabled=false;sim.Start();
            while(sim.phase==Phase.Playing&&sim.time<(level.checkpointPlatforms==null?60:180))
            {
                if(!sim.flying)
                {
                    int target=-1;
                    foreach(var link in level.connections)if(link.from==sim.platform.index&&level.platforms[link.to].label!="bonus"){target=link.to;break;}
                    Check(target>=0,"main route continues: "+level.id);
                    var probe=Shot(level,character,sim.platform.index,sim.distance,sim.time,sim.gearAngle);
                    var bounceRun=level.bounceRuns==null?null:Array.Find(level.bounceRuns,b=>b.entry==sim.platform.index&&b.exit==target);
                    var arrival=probe.platform;bool laterCampaign=level.id.StartsWith("mektra-level-");double arrivalX=(arrival.data.bounds.minX+arrival.data.bounds.maxX)*.5+arrival.MotionAt(probe.time).x;
                    if((!level.streamingRain||arrival.data.motion?.type!="rain"||arrivalX>=(laterCampaign?135:85)&&arrivalX<=(laterCampaign?285:350))&&(bounceRun==null||probe.wedgeBounces>=bounceRun.minimumBounces)&&(probe.phase==Phase.Playing||probe.phase==Phase.Complete)&&!probe.flying&&(level.streamingRain?Array.Exists(level.connections,l=>l.from==sim.platform.index&&l.to==probe.platform.index):probe.platform.index==target))sim.Jump();
                }
                // The real simulation waits, orbits and flies continuously; no teleporting between supports.
                for(int tick=0;tick<6&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);
            }
            Check(sim.phase==Phase.Complete,"continuous main-route traversal: "+level.id+" / "+character.id+" ("+sim.reason+", "+sim.time.ToString("0.0")+"s, support "+sim.platform.index+", checkpoint "+sim.CheckpointIndex+")");
            if(level.checkpointPlatforms!=null&&level.checkpointPlatforms.Length>0)Check(sim.CheckpointIndex==level.checkpointPlatforms[level.checkpointPlatforms.Length-1],"continuous traversal reaches final checkpoint: "+level.id+" / "+character.id+" / "+sim.CheckpointIndex);

        }
        public static void Run(LevelPack pack)
        {
            Check(pack.levels.Length==DiscoveryState.Count,"current campaign count");int routes=0,moving=0,dynamicOptions=0;double minimum=double.PositiveInfinity;
            var approved=JsonUtility.FromJson<LevelPack>(File.ReadAllText("../assets/levels/discovery/accepted-33.json"));
            for(int i=0;i<33;i++)Check(Equivalent(JToken.Parse(JsonUtility.ToJson(pack.levels[i])),JToken.Parse(JsonUtility.ToJson(approved.levels[i]))),"approved opening unchanged");
            var migrated=new DiscoveryState{current=5,unlocked=5,completedMask=63,failures=new[]{1,2,3,4,5,6}};migrated.Normalize();
            Check(migrated.current==6&&migrated.unlocked==6&&migrated.failures.Length==CampaignIdentity.Capacity&&migrated.failures[5]==6,"six encounter save migration");
            var oldTwenty=new DiscoveryState{current=19,unlocked=19,completedMask=(1<<20)-1,activeSeconds=570,failures=new int[20],best=123};oldTwenty.failures[19]=2;oldTwenty.Normalize();
            Check(oldTwenty.current==20&&oldTwenty.unlocked==20&&oldTwenty.failures[19]==2&&oldTwenty.best==123,"twenty encounter migration preserves progress and score");
            var oldThirty=new DiscoveryState{current=29,unlocked=29,completedMask=(1<<30)-1,failures=new int[30],best=777};oldThirty.Normalize();
            Check(oldThirty.current==30&&oldThirty.Completed(29)&&oldThirty.failures.Length==CampaignIdentity.Capacity&&oldThirty.best==777,"thirty encounter save migration");
            var highBit=new DiscoveryState();highBit.Complete(32);var savedBits=JsonUtility.FromJson<DiscoveryState>(JsonUtility.ToJson(highBit));savedBits.Normalize();
            Check(savedBits.Completed(32)&&!savedBits.Completed(0)&&!savedBits.Completed(30),"completion beyond 32 bits does not alias the opening");
            foreach(int old in new[]{3,6,10}){var oldRain=new DiscoveryState();oldRain.checkpoints[30]=old;oldRain.Normalize();Check(oldRain.checkpoints[30]==(old==3?16:old==6?32:48),"rain checkpoint part migration");}
            var timed=new DiscoveryState();timed.Tick(570);Check(timed.unlocked==19,"all variety available within ten active minutes");
            timed.Tick(570);Check(timed.unlocked==29,"depth encounters available by nineteen active minutes");
            var chars=CharacterCatalog.Load();var narrow=new List<string>();
            foreach(var course in pack.levels)
            {
                if(course.number>33)continue;
                var level=course.level;Check(level.discovery&&level.sparseRewards,"explicit authored rewards");
                Check(level.platforms.Length<=(level.streamingRain?65:course.number>30?16:6)&&level.stars.Length<=(course.number>30?12:4),"encounter size and reward budget");
                if(course.number>30)foreach(var support in level.platforms)if(support.sourceShapeId!="wedge"&&support.motion?.type!="rain")
                {
                    double travel=support.motion==null||support.motion.type=="vertical"?0:Math.Abs(support.motion.amplitudeX);
                    Check(support.bounds.minX-travel>=24&&support.bounds.maxX+travel<=366,"full orbit stays inside viewport: "+level.id+" / "+support.id);
                }
                var fresh=new Simulation(level,chars[0]);fresh.EnsureSection(100);
                Check(fresh.platforms.Count==level.platforms.Length&&fresh.stars.Count==level.stars.Length,"no duplicate sections or reward lanes");
                if(level.streamingRain){RainStreamRegression.Run(level,chars);dynamicOptions+=level.connections.Length;continue;}
                foreach(var link in level.connections)
                {
                    bool rebound=course.number>30&&level.bounceRuns!=null&&Array.Exists(level.bounceRuns,b=>b.entry==link.from&&b.exit==link.to);
                    var review=CourseReview.Review(level,link.from,chars,link.to);
                    if(rebound)
                    {
                        var authored=Array.Find(course.routes,r=>r.from==link.from&&r.to==link.to);
                        review=new CourseReview.JumpReview{from=link.from,to=link.to,distance=authored.distance,windowSeconds=authored.windowSeconds,launch=authored.launch,landing=authored.landing};
                        foreach(var c in chars){var bounce=Shot(level,c,link.from,review.distance,0);Check(bounce.wedgeBounces>=Array.Find(level.bounceRuns,b=>b.entry==link.from&&b.exit==link.to).minimumBounces&&!bounce.flying&&bounce.platform.index==link.to,"physical rebound route across all characters");}
                    }
                    minimum=Math.Min(minimum,review.windowSeconds);routes++;
                    Check(review.windowSeconds>=.2,"landing window too narrow: "+level.id+" "+link.from+" -> "+link.to+" ("+review.windowSeconds.ToString("0.000")+"s)");
                    bool bonus=course.number==3&&(link.from==3||link.to==3)||level.platforms[link.from].label=="bonus"||level.platforms[link.to].label=="bonus";
                    if(!bonus&&review.windowSeconds<(course.number<=20?.3:.25))narrow.Add(level.id+" "+link.from+" -> "+link.to+" ("+review.windowSeconds.ToString("0.000")+"s)");
                    var landed=Shot(level,chars[0],link.from,review.distance,0);
                    if(link.to==level.platforms.Length-1)Check(landed.phase==Phase.Complete,"exit completes encounter");
                    if(level.platforms[link.from].motion==null&&level.platforms[link.to].motion==null&&Array.TrueForAll(level.hazards,h=>h.motion==null))continue;
                    foreach(double clock in course.number>30?new[]{2.0,5.0,17.0,31.0}:new[]{2.0,5.0})
                    {
                        bool safe=false;var orbit=new Orbit(level.platforms[link.from].outline);
                        for(int sample=0;sample<360&&!safe;sample++)
                        {
                            var sim=Shot(level,chars[0],link.from,orbit.length*sample/360,clock);
                            safe=(sim.phase==Phase.Playing||sim.phase==Phase.Complete)&&!sim.flying&&sim.platform.index==link.to;
                        }
                        Check(safe,"moving route at clock "+clock+": "+level.id+" "+link.from+" -> "+link.to);moving++;
                    }
                }
            }
            foreach(var course in pack.levels)if(course.number>20&&course.number<=33)foreach(var character in chars)Traverse(course.level,character);
            Debug.Log("MEKTRA_DEPTH_CONTINUOUS_PASS all depth and expanded encounters across "+chars.Length+" characters");
            foreach(var course in pack.levels)if(course.number>30&&course.number<=33)
            {
                var level=course.level;Check(level.height==4000&&level.checkpointPlatforms.Length==4&&level.chapterTitles.Length==4,"four-part expanded journey");
                for(int part=1;part<4;part++)
                {
                    int cp=level.checkpointPlatforms[part];Check(cp>level.checkpointPlatforms[part-1]&&cp<level.platforms.Length-1,"ordered checkpoints before finish");
                    Check((level.platforms[cp].motion==null||level.platforms[cp].motion.type=="static")&&level.platforms[cp].crumbleSeconds==0&&level.platforms[cp].sourceShapeId!="wedge","stable checkpoint support");
                    var stateAtCheckpoint=new DiscoveryState{unlocked=32,current=course.number-1};stateAtCheckpoint.checkpoints[course.number-1]=cp;
                    var saved=JsonUtility.FromJson<DiscoveryState>(JsonUtility.ToJson(stateAtCheckpoint));saved.Normalize();Check(saved.checkpoints[course.number-1]==cp,"checkpoint survives restart");
                    var resumed=new Simulation(level,chars[0]);resumed.RestoreCheckpoint(cp);
                    Check(resumed.CheckpointIndex==cp&&resumed.platform.index==cp&&resumed.Distance==0&&resumed.score==0&&resumed.time==0&&resumed.phase==Phase.Ready,"safe fresh checkpoint; no skipped score or distance");
                    resumed.RestoreCheckpoint(-1);Check(resumed.platform.index==cp,"invalid checkpoint ignored");
                    foreach(var c in chars)Traverse(level,c,cp);
                }
            }
            Debug.Log("MEKTRA_EXPANDED_CHECKPOINT_PASS 90 resumed traversals plus 30 uninterrupted expanded traversals");
            var crumble=new Simulation(pack.levels[7].level,chars[0]);crumble.Start();crumble.platform=crumble.platforms[1];crumble.platform.landedAt=crumble.time;crumble.Select(chars[0]);
            crumble.Pause();double pausedTime=crumble.time;crumble.Advance(.05);Check(crumble.time==pausedTime&&!crumble.platform.vanished,"pause freezes crumble");crumble.Resume();
            for(int i=0;i<1000&&!crumble.platforms[1].vanished;i++)crumble.Advance(GameConfig.Step);
            Check(crumble.platforms[1].vanished,"crumble expires after landing");
            var reset=new Simulation(pack.levels[7].level,chars[0]);Check(!reset.platforms[1].vanished&&double.IsPositiveInfinity(reset.platforms[1].landedAt),"retry restores crumble");
            var hazardLevel=pack.levels[8].level;var struck=new Simulation(hazardLevel,chars[0]);struck.Start();struck.flying=true;struck.position=new Point(300,720);struck.velocity=new Point();struck.Advance(GameConfig.Step);Check(struck.reason=="obstacle","gate is a real collision hazard");
            var state=new DiscoveryState();state.Tick(150);Check(state.unlocked==5,"black hole available by 150 active seconds across attempts");
            state.Select(4);state.Fail(4);state.Fail(4);state.Fail(4);state.Complete(2);
            var restored=JsonUtility.FromJson<DiscoveryState>(JsonUtility.ToJson(state));
            Check(restored.current==4&&restored.unlocked==5&&restored.Completed(2)&&restored.failures[4]==3,"progress round trip");
            var repeated=new DiscoveryState();for(int i=0;i<3;i++)repeated.Fail(0);Check(repeated.unlocked==1,"failure offers next discovery without marking completion");
            Check(!repeated.Completed(0),"skip is not completion");
            var holeLevel=pack.levels[5].level;var trapped=new Simulation(holeLevel,chars[0]);trapped.Start();trapped.flying=true;trapped.position=new Point(285,470);trapped.velocity=new Point();
            for(int i=0;i<180&&trapped.phase==Phase.Playing;i++)trapped.Advance(GameConfig.Step);
            Check(trapped.reason=="black hole","vortex remains a real hazard");
            Check(narrow.Count==0,"main route windows below their pass target: "+string.Join("; ",narrow));
            string summary="PASS: "+routes+" fixed connections plus "+dynamicOptions+" streaming rain options across "+chars.Length+" characters, "+moving+" later motion samples; minimum shared window "+minimum.ToString("0.000")+"s; authored reward budgets, completion, bounded instances, persistent discovery and real vortex capture.";
            Directory.CreateDirectory("../assets/levels/discovery");File.WriteAllText("../assets/levels/discovery/validation.txt",summary);
            Debug.Log("MEKTRA_DISCOVERY_PASS "+summary);
            if(pack.levels.Length>33)CampaignWaveRegression.Run(pack);
        }
    }
}
