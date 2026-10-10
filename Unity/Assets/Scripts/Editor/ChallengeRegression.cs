using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class ChallengeRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("Challenge opening: "+message);}
        static Simulation Predict(Simulation live)
        {
            var sim=new Simulation(live.level,live.character,live.sequence,live.platform.section);
            sim.platform=sim.platforms.Find(p=>p.section==live.platform.section&&p.index==live.platform.index);
            sim.time=live.time;sim.distance=live.distance;sim.gearAngle=live.gearAngle;sim.Select(live.character);sim.SetViewport(live.viewport);sim.Start();
            foreach(var p in sim.platforms)
            {
                var actual=live.platforms.Find(v=>v.section==p.section&&v.index==p.index);if(actual==null)continue;
                p.landedAt=actual.landedAt;p.vanished=actual.vanished;
            }
            sim.Jump();
            for(int i=0;i<480&&sim.flying&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);
            return sim;
        }
        [Serializable] class RunReport {public string character,route;public int courses,jumps,stars,forkStars;public double seconds,minimumCrumbleRemaining;}
        [Serializable] class Report {public string method="Continuous fixed-step runs with real lava, motion, moving gates and first-contact crumble clocks; both fork policies; no teleporting between jumps.";public RunReport[] runs;}
        public static void Run(LevelPack pack)
        {
            Check(pack.levels.Length==5,"expected five distinct challenges");
            var courses=pack.Courses();var chars=CharacterCatalog.Load();
            foreach(var level in courses)MotionRegression.GeometryChecks(level);
            int routeChecks=0;
            // Every authored connection, including both sides of each fork, is physically reachable.
            foreach(var course in pack.levels)foreach(var edge in course.routes)
            {
                foreach(var c in chars)
                {
                    bool reached=false;
                    for(int phase=0;phase<16&&!reached;phase++)
                    {
                        var sim=MotionRegression.Launch(courses,c,course.number-1,edge.from,edge.distance,phase*.2);
                        reached=(sim.phase==Phase.Playing||sim.phase==Phase.Complete)&&!sim.flying&&sim.platform.section==course.number-1&&sim.platform.index==edge.to;
                    }
                    Check(reached,"no authored connection "+course.number+" / "+edge.from+" -> "+edge.to+" / "+c.id);routeChecks++;
                }
            }
            // Explicitly reject straight skips through each route-choice blocker.
            foreach(var hazard in courses[1].hazards)
            {
                var center=new Point((hazard.bounds.minX+hazard.bounds.maxX)*.5,(hazard.bounds.minY+hazard.bounds.maxY)*.5);
                Check(Geometry.SweptTouches(center+new Point(0,100),center-new Point(0,100),hazard,0),"fork blocker tunnelling");
            }
            var crumble=new Simulation(courses[0],chars[0],courses,3);crumble.platform=crumble.platforms.Find(p=>p.section==3&&p.index==1);crumble.distance=0;crumble.Select(chars[0]);crumble.SetViewport(816);crumble.Start();
            crumble.Pause();double clock=crumble.time;crumble.Advance(5);Check(crumble.time==clock&&!crumble.platform.vanished,"paused crumble advanced");crumble.Resume();
            for(int step=0;step<254;step++)crumble.Advance(GameConfig.Step);
            Check(crumble.platform.vanished&&crumble.flying,"crumble did not remove support");
            Check(crumble.platform.Remaining(crumble.time)==0,"crumble ring clock disagrees");
            // A moving bar can sweep through a stationary airborne player; use its relative sweep.
            var gate=courses[4].hazards[0];double gateTime=gate.motion.periodSeconds*.5;
            var hit=new Simulation(courses[0],chars[0],courses,4);hit.time=gateTime;hit.Start();hit.flying=true;hit.velocity=new Point();
            hit.position=new Point((gate.bounds.minX+gate.bounds.maxX)*.5,(gate.bounds.minY+gate.bounds.maxY)*.5-4*courses[4].stride)+gate.motion.At(gateTime);hit.SetViewport(816);hit.Advance(GameConfig.Step);
            Check(hit.phase==Phase.Exploding&&hit.reason=="obstacle","moving gate contact missing");
            var reports=new List<RunReport>();
            foreach(var c in chars)foreach(bool bonus in new[]{false,true})
            {
                var sim=new Simulation(courses[0],c,courses);sim.SetViewport(816);sim.Start();int jumps=0;double minCrumble=1;int lastSection=-1,forkStart=0,forkEnd=0;int[] landed=new int[5];
                for(int step=0;step<120*600&&sim.phase==Phase.Playing&&sim.platform.section<5;step++)
                {
                    int section=sim.platform.section;
                    if(section!=lastSection){Debug.Log("MEKTRA_CHALLENGE_PROGRESS "+c.id+" "+(bonus?"bonus":"safe")+" course "+(section+1)+" at "+sim.time.ToString("F1")+"s");if(section==1)forkStart=sim.score;if(section==2)forkEnd=sim.score;lastSection=section;}
                    if(!sim.flying&&sim.normal.y<-.08&&step%3==0)
                    {
                        int from=sim.platform.index,to=0,targetSection=section;
                        if(from==courses[section].platforms.Length-1)targetSection++;
                        else
                        {
                            var choices=Array.FindAll(courses[section].connections,r=>r.from==from);Check(choices.Length>0,"missing exit "+section+" / "+from);
                            to=choices[bonus&&choices.Length>1?1:0].to;
                        }
                        var prediction=Predict(sim);
                        if((prediction.phase==Phase.Playing||prediction.phase==Phase.Complete)&&!prediction.flying&&prediction.platform.section==targetSection&&prediction.platform.index==to&&(!(bonus&&section==1&&(to==2||to==5||to==8))||prediction.score>=3))
                        {
                            if(sim.platform.data.crumbleSeconds>0)minCrumble=Math.Min(minCrumble,sim.platform.Remaining(sim.time));
                            sim.Jump();jumps++;landed[section]++;
                        }
                    }
                    sim.Advance(GameConfig.Step);sim.events.Clear();
                }
                Check(sim.phase==Phase.Complete&&sim.platform.section==4,"continuous "+c.id+" / "+(bonus?"bonus":"safe")+" stopped in "+(sim.platform.section+1)+" at platform "+sim.platform.index+" reason "+sim.reason+" / "+sim.time.ToString("F1")+"s");
                for(int i=0;i<5;i++)Check(landed[i]>=6,"challenge skipped by test policy "+i);
                reports.Add(new RunReport{character=c.id,route=bonus?"bonus":"safe",courses=5,jumps=jumps,stars=sim.score,forkStars=forkEnd-forkStart,seconds=sim.time,minimumCrumbleRemaining=minCrumble});
                Debug.Log("MEKTRA_CHALLENGE_CAMPAIGN "+c.id+" "+(bonus?"bonus":"safe")+" completed in "+sim.time.ToString("F1")+"s, "+sim.score+" stars");
            }
            for(int i=0;i<reports.Count;i+=2)Check(reports[i+1].forkStars>reports[i].forkStars,"bonus route did not pay more for "+reports[i].character);
            Directory.CreateDirectory("../assets/levels/challenges");File.WriteAllText("../assets/levels/challenges/campaign.json",JsonUtility.ToJson(new Report{runs=reports.ToArray()},true));
            Debug.Log("MEKTRA_CHALLENGE_VALIDATION PASS: "+routeChecks+" branch/landing checks, 16 continuous campaigns, live crumble, pause, moving-gate collision, fork blockers.");
        }
    }
}
