using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class EndlessRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("Endless: "+message);}
        static Simulation Predict(Simulation live)
        {
            var sim=new Simulation(live.level,live.character,live.sequence,live.platform.section,true,live.seed);
            sim.platform=sim.platforms.Find(p=>p.section==live.platform.section&&p.index==live.platform.index);
            sim.energyEnabled=live.energyEnabled;sim.energy=live.energy;sim.energyAge=live.energyAge;
            foreach(var bloom in sim.energyBlooms){var actual=live.energyBlooms.Find(b=>b.parent.section==bloom.parent.section&&b.parent.index==bloom.parent.index);if(actual!=null)bloom.collected=actual.collected;}
            sim.time=live.time;sim.distance=live.distance;sim.gearAngle=live.gearAngle;sim.Select(live.character);sim.SetViewport(live.viewport);sim.Start();
            foreach(var p in sim.platforms)
            {
                var actual=live.platforms.Find(v=>v.section==p.section&&v.index==p.index);if(actual==null)continue;
                p.landedAt=actual.landedAt;p.vanished=actual.vanished;
            }
            sim.Jump();for(int step=0;step<480&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);return sim;
        }
        [Serializable] class RunReport {public string character,policy;public int seed,segments,jumps,stars;public double seconds;public int[] kinds;}
        [Serializable] class Report {public string method="Continuous real-clock play with seeded segment order, both fork policies, active lava, gates, moving targets, crumble timers and magnets. No teleports between landings.";public RunReport[] runs;}
        public static void ValidateSaved()=>Run(LevelPack.Load());
        public static void Run(LevelPack pack)
        {
            Check(pack.endless&&pack.levels.Length==84,"expected 84 endless segment variants");
            var courses=pack.Courses();int platformLimit=0,hazardLimit=0,starLimit=0;foreach(var level in courses){platformLimit=Math.Max(platformLimit,level.platforms.Length*4);hazardLimit=Math.Max(hazardLimit,level.hazards.Length*4);starLimit=Math.Max(starLimit,level.stars.Length*4);}
            var chars=CharacterCatalog.Load();var families=new HashSet<string>();int connections=0;
            foreach(var c in pack.levels)
            {
                MotionRegression.GeometryChecks(c.level);
                Check(c.level.height==1080&&c.level.stride==1080,"segment seam size");
                foreach(var p in c.level.platforms)families.Add(p.sourceShapeId);
                foreach(var route in c.routes)foreach(var character in chars)
                {
                    bool safe=false;
                    for(int phase=0;phase<24&&!safe;phase++)
                    {
                        var sim=MotionRegression.Launch(courses,character,c.number-1,route.from,CourseReview.RouteDistance(c.level,route.from,route.distance),phase*.2);
                        safe=(sim.phase==Phase.Playing||sim.phase==Phase.Complete)&&!sim.flying&&sim.platform.section==c.number-1&&sim.platform.index==route.to;
                    }
                    Check(safe,"connection "+c.level.id+" "+route.from+" -> "+route.to+" / "+character.id);connections++;
                }
            }
            Check(families.Count>=11,"not all ten shape families were used");
            // The authored planner covers every layout in the same order for every seed.
            bool different=false;var seen=new HashSet<int>();
            for(int section=0;section<1024;section++)
            {
                int a=EndlessJourney.Index(courses,section,17),b=EndlessJourney.Index(courses,section,713);
                Check(a==EndlessJourney.Index(courses,section,17),"unstable seeded selection");if(a!=b)different=true;seen.Add(a);
                Check(a==b,"authored order changes with seed");
            }
            Check(!different&&seen.Count==pack.levels.Length,"planner does not use the full library");
            ExperienceRegression.Run(courses,chars);
            foreach(int seed in new[]{17,713,9001})
            {
                var opening=new Simulation(courses[0],chars[0],courses,0,true,seed);
                Check(opening.magnets.Count==0,"opening magnet still present");
                int last=-100,count=0;
                for(int section=0;section<128;section++)if(EndlessJourney.HasMagnet(section,seed))
                {
                    Check(section>=8&&section-last>=4,"magnet appears too soon or too often");last=section;count++;
                }
                Check(count==15,"rare magnet wave spacing");
            }
            foreach(var course in courses)
            {
                foreach(var data in course.platforms)if(data.motion!=null&&data.motion.type!="static")
                {
                    var instance=new PlatformInstance{data=data,lockMotionDuringGuides=true};var start=instance.MotionAt(0);
                    foreach(double time in new[]{1.0,20,24.9,25})Check((instance.MotionAt(time)-start).Length<1e-10,"platform moves while guides are visible");
                    Check((instance.MotionAt(25.001)-start).Length<.001,"motion snapped on guide expiry");
                    Check((instance.MotionAt(28)-start).Length>.1,"platform motion never resumes");
                }
                foreach(var data in course.hazards)if(data.motion!=null&&data.motion.type!="static")
                {
                    var instance=new PlatformInstance{data=data,lockMotionDuringGuides=true};Check((instance.MotionAt(24.9)-instance.MotionAt(0)).Length<1e-10,"gate moves during guide practice");
                }
            }
            Check(GameConfig.GuideOpacity(20)==1&&GameConfig.GuideOpacity(24.9)>0&&GameConfig.GuideOpacity(25)==0,"shared guide lifetime");
            var reports=new List<RunReport>();
            foreach(var character in chars)foreach(bool bonus in new[]{false,true})
            {
                int seed=bonus?713:17;var sim=new Simulation(courses[0],character,courses,0,true,seed);sim.energyEnabled=true;sim.SetViewport(816);sim.Start();
                int jumps=0,lastSection=-1;var kinds=new int[EndlessJourney.KindCount(courses)];
                for(int step=0;step<120*1800&&sim.phase==Phase.Playing&&sim.platform.section<128;step++)
                {
                    int section=sim.platform.section;var course=sim.CourseAt(section);
                    if(lastSection!=section)
                    {
                        kinds[course.segmentKind]++;lastSection=section;
                        if(section%8==0)Debug.Log("MEKTRA_ENDLESS_PROGRESS "+character.id+" "+(bonus?"bonus":"safe")+" segment "+section+" at "+sim.time.ToString("F1")+"s");
                    }
                    if(!sim.flying&&step%3==0)
                    {
                        int targetSection=section,to=0;
                        if(sim.platform.index==course.platforms.Length-1)targetSection++;
                        else
                        {
                            var choices=Array.FindAll(course.connections,r=>r.from==sim.platform.index);Check(choices.Length>0,"missing platform exit");
                            to=choices[bonus&&choices.Length>1?1:0].to;
                        }
                        if(course.bounceRuns!=null){var bounce=Array.Find(course.bounceRuns,r=>r.entry==to);if(bounce!=null)to=bounce.exit;}
                        var prediction=Predict(sim);
                        if(prediction.phase==Phase.Playing&&!prediction.flying&&(prediction.platform.data.motion?.type!="rain"||prediction.position.x>=150)&&prediction.platform.section==targetSection&&(prediction.platform.index==to||course.segmentKind==9&&Array.Exists(course.connections,r=>r.from==sim.platform.index&&r.to==prediction.platform.index))&&(!(bonus&&course.segmentKind==1&&to==2)||prediction.score>=3))
                        {
                            sim.Jump();jumps++;
                        }
                    }
                    sim.Advance(GameConfig.Step);sim.events.Clear();
                    Check(sim.platforms.Count<=platformLimit&&sim.hazards.Count<=hazardLimit&&sim.stars.Count<=starLimit&&sim.magnets.Count<12,"endless instances grew without trimming");
                }
                Check(sim.phase==Phase.Playing&&sim.platform.section>=128,"run stopped: "+character.id+" "+(bonus?"bonus":"safe")+" at segment "+sim.platform.section+" ("+sim.CourseAt(sim.platform.section).id+") platform "+sim.platform.index+", "+sim.reason+", "+sim.time.ToString("F1")+"s");
                for(int kind=0;kind<EndlessJourney.KindCount(courses);kind++)if(kind!=6)Check(kinds[kind]>0,"world or mechanic missing from run "+kind);
                sim.Pause();double time=sim.time;var position=sim.position;sim.Advance(2);Check(time==sim.time&&(position-sim.position).Length==0,"pause changed endless state");sim.Resume();
                reports.Add(new RunReport{character=character.id,policy=bonus?"bonus":"safe",seed=seed,segments=128,jumps=jumps,stars=sim.score,seconds=sim.time,kinds=kinds});
                Debug.Log("MEKTRA_ENDLESS_RUN "+character.id+" "+(bonus?"bonus":"safe")+" passed 128 segments / "+sim.time.ToString("F1")+"s / "+jumps+" jumps");
            }
            // Fresh production runs begin at the opening with unused platforms.
            var restart=new Simulation(courses[0],chars[0],courses,0,true,9001);Check(restart.platform.section==0&&restart.phase==Phase.Ready,"restart failed");foreach(var p in restart.platforms)Check(!p.vanished&&double.IsPositiveInfinity(p.landedAt),"restart kept crumble state");
            Directory.CreateDirectory("../assets/levels/endless");File.WriteAllText("../assets/levels/endless/campaign.json",JsonUtility.ToJson(new Report{runs=reports.ToArray()},true));
            Debug.Log("MEKTRA_ENDLESS_VALIDATION PASS: "+connections+" connection checks, all shape families, 1024 planner samples, 16 continuous 128-segment runs, bounded instances, pause and restart.");
        }
    }
}
