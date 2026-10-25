using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class ExperienceRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("World experience: "+message);}
        public static void Run(LevelData[] library,Character[] characters)
        {
            var authored=new System.Collections.Generic.HashSet<string>();
            for(int section=0;section<library.Length;section++)
            {
                var course=library[EndlessJourney.Index(library,section,17)];
                authored.Add(course.id);
                Check(course.id==library[EndlessJourney.Index(library,section,713)].id,"authored order changes by seed");
            }
            Check(authored.Count==library.Length,"authored journey omits layouts");
            var opening=new Simulation(library[0],characters[0],library,0,true,17);opening.SetViewport(816);opening.Start();
            opening.time=9.99;Check(!WorldTestMode.ShouldEnter(opening),"world test starts before ten seconds");
            opening.time=10;opening.flying=true;Check(!WorldTestMode.ShouldEnter(opening),"world test interrupts a jump");opening.flying=false;
            if(WorldTestMode.Enabled)Check(WorldTestMode.ShouldEnter(opening),"world test does not start at ten seconds");
            else foreach(double elapsed in new[]{10.0,30,120,600}){opening.time=elapsed;Check(!WorldTestMode.ShouldEnter(opening),"disabled shortcut interrupts normal progression");}
            opening.time=10;
            opening.score=3;
            for(int world=0;world<7;world++)
            {
                var next=WorldTestMode.Enter(opening,world);
                Check(next.phase==Phase.Playing&&next.platform.section==new[]{32,48,16,24,40,56,64}[world]&&next.score==3&&next.time==10&&next.Distance==opening.Distance,"world test lost progress or selected the wrong chapter");
                Check(next.lava.Front-next.cameraY>=next.viewport+LavaHazard.StartBelowScreen-.001&&!next.lava.Active(next.time),"world test reveals lava suddenly");
                Check(!WorldTestMode.ShouldEnter(next),"world test repeats inside the later chapter");
            }
            foreach(int section in new[]{0,9,16,24,32,40,48,56,64,120})
            {
                var live=new Simulation(library[0],characters[0],library,section,true,713);live.time=80;live.SetViewport(816);live.Start();live.score=7;
                var retry=SectionReplayMode.Restart(live);
                Check(retry.platform.section==0&&retry.CourseAt(0).id==library[EndlessJourney.Index(library,0,17)].id,"replay did not return to opening");
                if(SectionReplayMode.CycleAllLayoutsForTesting)
                {
                    var seen=new System.Collections.Generic.HashSet<string>();
                    seen.Add(retry.CourseAt(section).id);
                    for(int attempt=1;attempt<library.Length;attempt++)seen.Add(SectionReplayMode.Restart(live).CourseAt(section).id);
                    Check(seen.Count==library.Length,"retry itinerary does not cover all layouts");
                }
                Check(retry.phase==Phase.Ready&&!retry.flying&&retry.platform.index==0&&retry.score==0&&retry.time==0&&retry.energy==100&&retry.energyAge==0,"section replay does not create a clean attempt");
                Check(retry.lava.Front-retry.cameraY>=retry.viewport+LavaHazard.StartBelowScreen-.001&&!retry.lava.Active(retry.time),"section replay exposes lava");
                Check(retry.Distance==0,"replay retains distance from previous run");
                foreach(var support in retry.platforms)Check(!support.vanished&&double.IsPositiveInfinity(support.landedAt),"section replay retains used platform state");
            }
            var resized=new Simulation(library[0],characters[0]);resized.SetViewport(950);
            Check(resized.lava.Front-resized.cameraY>resized.viewport,"initial sizing exposes lava");
            foreach(var c in library)
            {
                if(c.blackHoles!=null)foreach(var hole in c.blackHoles)
                {
                    var sim=new Simulation(c,characters[0],null,0,true);sim.SetViewport(816);sim.Start();sim.position=new Point(hole.x+hole.pullRadius-3,hole.y);sim.flying=true;sim.velocity=new Point();sim.Advance(GameConfig.Step);
                    Check(sim.BeingPulled,"proximity did not capture player");sim.Pause();var before=sim.position;sim.Advance(.05);Check((sim.position-before).Length==0,"paused vortex advanced");sim.Resume();
                    for(int i=0;i<120&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);
                    Check(sim.reason=="black hole"&&sim.phase==Phase.Exploding,"vortex did not implode and burst");
                }
                if(c.segmentKind==11)foreach(var character in characters)
                {
                    // The publishing builder checks all characters against the measured entry angle.
                    var orbit=new Orbit(c.platforms[0].outline);bool reached=false;
                    for(int sample=0;sample<360&&!reached;sample++)
                    {
                        var sim=new Simulation(c,character);sim.distance=sample*orbit.length/360;sim.Select(character);sim.SetViewport(816);sim.Start();sim.Jump();
                        double speed=sim.velocity.Length;
                        for(int step=0;step<480&&sim.flying&&sim.phase==Phase.Playing;step++)
                        {sim.Advance(GameConfig.Step);if(sim.flying&&sim.wedgeBounces>0)Check(Math.Abs(sim.velocity.Length-speed)<1e-6,"rebound changes speed");}
                        reached=sim.phase==Phase.Playing&&!sim.flying&&sim.platform.index==3&&sim.wedgeBounces>0;
                    }
                    Check(reached,"physical wedge path misses catcher / "+character.id);
                }
                if(c.segmentKind==9)
                {
                    Check(c.hazards.Length==0&&c.platforms.Length==18,"rain is not a field of landable drops");
                    Check(c.platforms[1].motion.periodSeconds!=c.platforms[5].motion.periodSeconds,"rain rows share a speed");
                    foreach(var drop in c.platforms)if(drop.motion?.type=="rain")
                    {
                        var m=drop.motion;var fast=(m.At(.101)-m.At(.1))*1000;var slow=(m.At(m.periodSeconds*.5+.001)-m.At(m.periodSeconds*.5))*1000;
                        Check(fast.x<0&&fast.y>0&&slow.x<0&&slow.y>0&&Math.Abs(fast.x)>Math.Abs(slow.x)*1.2,"rain direction/speed pulse is wrong");
                        Check(m.At(0).Length<1e-7,"rain starting pose is offset");
                        double center=(drop.bounds.minX+drop.bounds.maxX)*.5;bool reset=false;
                        for(double t=.01;t<m.periodSeconds*1.1;t+=.01)
                        {var a=m.At(t-.01);var b=m.At(t);if(b.x-a.x>100){Check(center+a.x<-60&&center+b.x>430,"rain resets inside visible screen");reset=true;break;}}
                        Check(reset,"rain never recycles");
                    }
                    var riding=new Simulation(c,characters[0],null,0,true);riding.time=25;riding.platform=riding.platforms[2];riding.distance=0;riding.Select(characters[0]);riding.SetViewport(816);riding.Start();
                    // Keep lava outside this focused carrier check; lava behavior is covered separately.
                    riding.lava=new LavaHazard(1000000,25);var support=riding.platform;
                    for(int i=0;i<60;i++)
                    {
                        riding.Advance(GameConfig.Step);support.orbit.Pose(riding.distance,out var contact,out var normal);
                        var expected=Orbit.Ground(contact,normal,riding.gearAngle,riding.character)+support.MotionAt(riding.time);
                        Check(!riding.flying&&(riding.position-expected).Length<1e-7,"rain does not carry its rider");
                    }
                    riding.Pause();var frozen=riding.position;var frozenTime=riding.time;riding.Advance(.05);Check((riding.position-frozen).Length==0&&riding.time==frozenTime,"paused rain carries player");riding.Resume();
                    for(int i=0;i<3000&&riding.phase==Phase.Playing;i++)riding.Advance(GameConfig.Step);
                    Check(riding.reason=="wall","idle rain rider is not swept offscreen");
                }
            }
            Debug.Log("MEKTRA_EXPERIENCE_VALIDATION PASS: fixed authored order and opening full-health retries, black-hole capture/pause/burst, physical wedge reflections preserve speed and reach the catcher for eight characters; landable rain carries riders, varies speed, pauses and recycles offscreen.");
        }
    }
}
