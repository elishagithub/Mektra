using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class ReviveRegression
    {
        public static void Run()
        {
            var pack=LevelPack.Load();var characters=CharacterCatalog.Load();
            foreach(var course in pack.Courses())foreach(var character in characters)
            {
                var sim=new Simulation(course,character);sim.energyEnabled=true;sim.Start();
                sim.score=7;int distance=sim.Distance;sim.energy=0;sim.Die("energy");
                for(int tick=0;tick<120;tick++)sim.Step(GameConfig.Step);
                Check(sim.phase==Phase.Over,"death did not finish");
                Check(sim.Revive(),"no revive landing in "+course.name);
                Check(sim.phase==Phase.Paused&&sim.energy==100,"revive must wait for player with full energy");
                Check(sim.score==7&&sim.Distance==distance,"revive changed earned score");
                Check(!sim.CanRevive&&!sim.Revive(),"second revive allowed");
                sim.Resume();sim.Step(GameConfig.Step);Check(sim.phase==Phase.Playing,"revive landing immediately killed player");
                sim.Die("wall");for(int tick=0;tick<120;tick++)sim.Step(GameConfig.Step);
                Check(!sim.CanRevive,"second death allows another revive");
            }
            Debug.Log("MEKTRA_REVIVE_VALIDATION PASS: all courses and characters; one revive, safe landing, energy, score and pause.");
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception("Revive: "+message);}
    }
}
