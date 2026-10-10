using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class EnergyRegression
    {
        public static void BuildUi()=>MektraBuild.BuildValidatedOpening(LevelPack.Load());
        static void Check(bool ok,string why){if(!ok)throw new Exception("Energy: "+why);}
        public static void Run(LevelPack pack)
        {
            var chars=CharacterCatalog.Load();
            var library=Array.ConvertAll(pack.levels,course=>course.level);
            var journey=new Simulation(library[0],chars[0],library,0,true,17);
            Check(journey.energyBlooms.Count==0,"recovery blooms appear at the opening");
            journey.EnsureSection(12);
            Check(journey.energyBlooms.Count==2,"recovery blooms must appear once every six sections");
            foreach(var bloom in journey.energyBlooms)
                Check(bloom.parent.section%6==4&&bloom.parent.index==journey.CourseAt(bloom.parent.section).platforms.Length-1,"recovery bloom misses common final landing");
            foreach(var course in pack.levels)
            {
                var sim=new Simulation(course.level,chars[0]);
                Check(sim.energyBlooms.Count>0,"course has no refill "+course.number);
                foreach(var bloom in sim.energyBlooms)
                {
                    Check(bloom.position.x>20&&bloom.position.x<370,"refill outside lane");
                    Check(bloom.parent.orbit.points.Length>0,"refill lacks a traversable support");
                }
            }
            foreach(var character in chars)
            {
                var pickup=new Simulation(pack.levels[0].level,character);pickup.energyEnabled=true;pickup.Start();pickup.energy=10;
                pickup.stars.Clear();pickup.energyBlooms.Clear();
                pickup.stars.Add(new StarInstance{position=pickup.position,section=0});
                pickup.Step(GameConfig.Step);
                Check(pickup.score==1&&pickup.energy==14,"star must award both currency and four energy");
                pickup.Step(GameConfig.Step);Check(pickup.score==1&&pickup.energy==14,"star rewarded twice");
                pickup.energy=99;pickup.stars.Add(new StarInstance{position=pickup.position,section=0});pickup.Step(GameConfig.Step);
                Check(pickup.energy==100&&pickup.score==2,"star refill must cap at 100 without losing currency");
                var sim=new Simulation(pack.levels[0].level,character);sim.energyEnabled=true;sim.Start();
                double normalPace=sim.Pace;sim.energy=10;Check(sim.Pace<normalPace&&sim.Pace>=normalPace*.8,"critical slowdown range");
                sim.Jump();Check(Math.Abs(sim.velocity.Length-GameConfig.JumpSpeed)<.0001,"energy changes jump reach");
                var bloom=sim.energyBlooms[0];sim.platform=bloom.parent;sim.flying=false;
                int top=0;for(int i=1;i<sim.platform.orbit.points.Length;i++)if(sim.platform.orbit.points[i].y<sim.platform.orbit.points[top].y)top=i;
                sim.distance=sim.platform.orbit.cumulative[top];sim.platform.orbit.Pose(sim.distance,out sim.position,out sim.normal);
                sim.Step(GameConfig.Step);Check(bloom.collected&&sim.energy>=49,"critical refill cannot be collected");Check(sim.Pace==normalPace,"refill fails to restore speed");
                sim.energy=100;sim.Pause();sim.Advance(.1);Check(sim.energy==100,"pause drains energy");
                sim.Resume();sim.energyAge=26;sim.Step(GameConfig.Step);Check(sim.energy<100,"active energy does not drain");
            }
            Debug.Log("MEKTRA_ENERGY_VALIDATION PASS: refill supports in all courses; critical slowdown, full jump reach, collection, speed recovery, pause and drain across eight characters.");
        }
    }
}
