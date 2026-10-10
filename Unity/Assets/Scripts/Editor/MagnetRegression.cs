using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class MagnetRegression
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception("Magnet: "+message);}
        public static void Run()
        {
            var plans=JumpPlans.Load();
            foreach(var character in CharacterCatalog.Load())
            {
                var lane=new Simulation(LevelData.Load(),character);
                var plan=plans[0];lane.distance=plan.distance;lane.Select(character);lane.Start();lane.Jump();
                for(int step=0;step<240&&lane.flying&&lane.phase==Phase.Playing;step++)lane.Advance(GameConfig.Step);
                Check(lane.magnets[0].collected,"opening pickup unreachable for "+character.id);
            }
            var sim=new Simulation(LevelData.Load(),CharacterCatalog.Load()[0]);
            Check(sim.magnets.Count>0,"no pickups spawned");
            sim.stars.Clear();sim.magnets.Clear();sim.Start();
            var left=new StarInstance{position=new Point(65,sim.position.y-120)};
            var right=new StarInstance{position=new Point(325,sim.position.y-180)};
            var outside=new StarInstance{position=new Point(195,sim.cameraY-80)};
            sim.stars.Add(left);sim.stars.Add(right);sim.stars.Add(outside);
            var ring=new MagnetInstance{position=sim.position};sim.magnets.Add(ring);
            sim.Advance(GameConfig.Step);
            Check(ring.collected&&left.attracted&&right.attracted&&!outside.attracted,"visible-screen selection");
            Check(sim.score==0,"stars awarded before arrival");
            var previous=left.magnetPosition;sim.Pause();sim.Advance(1);
            Check((left.magnetPosition-previous).Length==0,"attraction moves on pause");sim.Resume();
            for(int step=0;step<240;step++)sim.Advance(GameConfig.Step);
            Check(left.collected&&right.collected&&sim.score==2,"arrival rewards exactly once");
            Check(!outside.collected&&!outside.attracted,"offscreen star attracted");
            for(int step=0;step<60;step++)sim.Advance(GameConfig.Step);
            Check(sim.score==2,"duplicate reward");
            var restart=new Simulation(LevelData.Load(),CharacterCatalog.Load()[0]);
            Check(restart.magnets.TrueForAll(pickup=>!pickup.collected),"restart state");
            Debug.Log("MEKTRA_MAGNET_VALIDATION PASS: spawned pickup, visible stars only, animated arrival, pause, single rewards and restart.");
        }
    }
}
