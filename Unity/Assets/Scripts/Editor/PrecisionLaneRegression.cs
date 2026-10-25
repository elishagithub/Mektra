using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class PrecisionLaneRegression
    {
        static int RunShot(bool missOne)
        {
            var level=LevelData.Load();var character=CharacterCatalog.Load()[0];var plan=JumpPlans.Load()[0];
            var sim=new Simulation(level,character);sim.platform=sim.platforms.Find(p=>p.section==0&&p.index==plan.from);sim.distance=plan.distance;sim.Select(character);sim.SetViewport(750);sim.Start();sim.energyEnabled=false;sim.stars.Clear();sim.magnets.Clear();
            int to=sim.platform.index+1;if(level.connections!=null)foreach(var link in level.connections)if(link.from==sim.platform.index){to=link.to;break;}
            var direction=sim.JumpDirection();var perpendicular=new Point(-direction.y,direction.x);
            for(int i=0;i<3;i++)sim.stars.Add(new StarInstance{section=0,guideFrom=plan.from,guideTo=to,position=sim.position+direction*(30+i*22)+(missOne&&i==1?perpendicular*75:new Point())});
            sim.Jump();for(int tick=0;tick<600&&sim.flying&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);
            if(sim.phase!=Phase.Playing||sim.flying)throw new Exception("Scoring route did not land safely");
            return sim.precisionScore;
        }
        public static void Run()
        {
            int perfect=RunShot(false),partial=RunShot(true);
            if(perfect!=6||partial!=2)throw new Exception("Precision lane scores: expected 6/2; got "+perfect+"/"+partial);
            Debug.Log("MEKTRA_PRECISION_LANES_PASS full=6 partial=2; scoring independent of rendering");
        }
    }
}
