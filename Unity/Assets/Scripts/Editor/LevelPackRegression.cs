using System;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class LevelPackRegression
    {
        static Simulation Launch(LevelData[] courses,Character c,int section,int index,double distance)
        {
            var sim=new Simulation(courses[0],c,courses,section);
            sim.platform=sim.platforms.Find(p=>p.section==section&&p.index==index);
            sim.distance=distance;sim.Select(c);sim.SetViewport(816);sim.Start();sim.Jump();
            for(int step=0;step<180&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);
            return sim;
        }
        public static void Run()=>Run(LevelPack.Load());
        public static void Run(LevelPack pack)
        {
            var courses=pack.Courses();var characters=CharacterCatalog.Load();int routes=0,bridges=0;
            for(int section=0;section<courses.Length;section++)
            {
                foreach(var route in pack.levels[section].routes)
                {
                    double distance=route.distance;bool authored=true;
                    foreach(var c in characters)if(!MotionRegression.Arrived(Launch(courses,c,section,route.from,distance),section,route.to)){authored=false;break;}
                    if(!authored)distance=MotionRegression.FindRoute(courses,characters,section,route.from,0);
                    foreach(var c in characters)
                    {
                        var sim=Launch(courses,c,section,route.from,distance);
                        if(!MotionRegression.Arrived(sim,section,route.to))throw new Exception("Pack perpendicular route/star failed: "+section+" / "+route.from+" / "+c.id);
                        routes++;
                    }
                }
                int last=courses[section].platforms.Length-1;var orbit=new Orbit(courses[section].platforms[last].outline);bool found=false;
                for(int sample=0;sample<240&&!found;sample++)
                {
                    double distance=sample*orbit.length/240;orbit.Pose(distance,out var p,out var normal);if(normal.y>=-.2)continue;
                    var sim=Launch(courses,characters[0],section,last,distance);
                    if(sim.flying||sim.phase!=Phase.Playing||sim.platform.section!=section+1||sim.platform.index!=0)continue;
                    foreach(var c in characters)
                    {
                        sim=Launch(courses,c,section,last,distance);
                        if(sim.flying||sim.phase!=Phase.Playing||sim.platform.section!=section+1||sim.platform.index!=0)throw new Exception("Bridge failed for "+section+" / "+c.id);
                        bridges++;
                    }
                    found=true;
                }
                if(!found)throw new Exception("No bridge to next course from "+section);
            }
            string report="PASS: "+routes+" course jump/star checks, "+bridges+" level-to-level bridge checks across 8 characters, including level "+courses.Length+" to level 1.\n";
            File.WriteAllText("level-pack-validation.txt",report);Debug.Log("MEKTRA_SEQUENCE_VALIDATION "+report);
        }
    }
}
