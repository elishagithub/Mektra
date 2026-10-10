using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class CustomResizeValidator
    {
        [Serializable] class Plan {public JumpPlan[] jumps;}
        static Simulation Trial(LevelData level,Character c,int section,int index,double distance)
        {
            var sim=new Simulation(level,c);sim.EnsureSection(section+1);
            sim.platform=sim.platforms.Find(p=>p.section==section&&p.index==index);sim.distance=distance;sim.Select(c);sim.SetViewport(750);sim.Start();sim.Jump();
            for(int i=0;i<360&&sim.flying&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);
            return sim;
        }
        public static void Build()
        {
            var level=LevelData.Load();var chars=CharacterCatalog.Load();var plans=new List<JumpPlan>();
            for(int index=0;index<level.platforms.Length;index++)
            {
                var orbit=new Orbit(level.platforms[index].outline);double best=-1;
                for(int sample=0;sample<1440;sample++)
                {
                    double along=sample*orbit.length/1440;orbit.Pose(along,out var p,out var n);if(n.y>=-.1)continue;
                    bool valid=true;
                    for(int section=0;section<3&&valid;section++)foreach(var c in chars)
                    {
                        var sim=Trial(level,c,section,index,along);int next=(index+1)%level.platforms.Length;
                        if(sim.phase!=Phase.Playing||sim.flying||sim.platform.index!=next||sim.platform.section!=section+(next==0?1:0)||sim.score==0){valid=false;break;}
                    }
                    if(valid){best=along;break;}
                }
                if(best<0)throw new Exception("Resize has no safe jump/star lane for custom platform "+(index+1));
                plans.Add(new JumpPlan{from=index,to=(index+1)%level.platforms.Length,distance=best});
            }
            File.WriteAllText("Assets/Resources/jump-plan.json",JsonUtility.ToJson(new Plan{jumps=plans.ToArray()}));
            Debug.Log("MEKTRA_RESIZE_SUCCESS: original shapes preserved, launch windows recalculated.");
            UnityEditor.AssetDatabase.Refresh();MektraBuild.Build();
        }
    }
}
