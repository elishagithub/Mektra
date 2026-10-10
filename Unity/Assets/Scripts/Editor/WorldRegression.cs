using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    public static class WorldRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("Worlds: "+message);}
        public static void Run(LevelPack pack)
        {
            var courses=pack.Courses();var chars=CharacterCatalog.Load();int catches=0,corridors=0;
            foreach(var course in pack.levels)
            {
                int section=course.number-1;
                foreach(var character in chars)
                {
                    var reached=new HashSet<string>();
                    foreach(var route in course.routes)
                    {
                        var sim=MotionRegression.Launch(courses,character,section,route.from,route.distance,0);
                        Check(sim.phase==Phase.Playing&&!sim.flying&&sim.platform.section==section&&sim.platform.index==route.to,"authored landing "+course.number+" / "+route.from);
                        foreach(var star in sim.stars)if(star.section==section&&star.collected)reached.Add(star.id);
                    }
                    foreach(var star in course.level.stars)Check(reached.Contains(star.id),"unreachable reward "+course.number+" / "+character.id+" / "+star.id);
                    catches+=reached.Count;
                }
                if(course.level.hazards!=null)foreach(var hazard in course.level.hazards)
                {
                    var a=hazard.outline[0];var b=hazard.outline[1];var midpoint=(a+b)*.5;
                    Check(Geometry.SweptTouches(midpoint+new Point(-30,0),midpoint+new Point(30,0),hazard,0),"thin rail tunnelling");
                    var sim=new Simulation(courses[0],chars[0],courses,section);sim.Start();sim.flying=true;sim.velocity=new Point();sim.position=midpoint+new Point(0,-section*course.level.stride);sim.previousPosition=sim.position;sim.SetViewport(816);
                    sim.Advance(GameConfig.Step);Check(sim.phase==Phase.Exploding&&sim.reason=="obstacle","rail death");corridors++;
                }
            }
            var start=new Simulation(courses[0],chars[0],courses,24);
            Check(start.platform.section==24&&start.LevelNumber==25&&start.platform.index==0,"world selection starts at wrong course");
            start.Start();start.Advance(GameConfig.Step);start.Pause();var time=start.time;start.Advance(1);Check(start.time==time,"pause clock");
            Debug.Log("MEKTRA_WORLD_VALIDATION PASS: "+catches+" reachable reward checks, "+corridors+" lethal rail checks, swept collision, world selection and pause.");
        }
    }
}
