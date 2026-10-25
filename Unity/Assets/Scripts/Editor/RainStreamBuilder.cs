using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Mektra.Editor
{
    public static class RainStreamBuilder
    {
        public static AuthoredCourse Make()
        {
            // Preserve the original rain's 74x40 capsules, slant, 520-unit wrap and diagonal row flow.
            var supports=new List<PlatformData>{EndlessPackBuilder.Shape(0,0,195,3800,64,0)};
            var links=new List<RouteLink>();var rewards=new List<StarData>();var checkpoints=new List<int>{0};
            double[][] periods={new[]{24d,20,27},new[]{22d,26,20},new[]{24d,19,27},new[]{22d,25,20}};
            for(int part=0;part<4;part++)
            {
                int entry=supports.Count-1;double origin=3800-part*750;
                for(int row=0;row<3;row++)for(int col=0;col<5;col++)
                {
                    int index=supports.Count;double x=29+col*104,y=origin-200-row*175-(x-195)*.1;
                    var capsule=JourneyExpansionBuilder.Capsule(index,x,y,74,40,-Math.PI/12);
                    capsule.motion=new MotionData{type="rain",amplitudeX=520,amplitudeY=52,periodSeconds=periods[part][row],phase=(445-x)/520,speedVariation=.18};
                    capsule.label="rain-row-"+(part*3+row);supports.Add(capsule);
                    if(row<2&&col==(row==0?1:2))rewards.Add(new StarData{id="rain-dot-"+rewards.Count,x=x,y=capsule.bounds.minY-10,parentPlatformId=capsule.id});
                }
                int exit=supports.Count;supports.Add(EndlessPackBuilder.Shape(0,exit,195,origin-750,72,0));
                for(int col=0;col<5;col++)links.Add(new RouteLink{from=entry,to=entry+1+col});
                for(int row=0;row<2;row++)for(int col=0;col<5;col++)for(int next=0;next<5;next++)links.Add(new RouteLink{from=entry+1+row*5+col,to=entry+1+(row+1)*5+next});
                for(int col=0;col<5;col++)links.Add(new RouteLink{from=entry+11+col,to=exit});
                if(part<3)checkpoints.Add(exit);
            }
            var level=new LevelData{id="expanded-rain",name="A journey through rain",source="Expanded original diagonal capsule rain",width=390,height=4000,stride=4000,authoredRadius=GameConfig.Radius,
                discovery=true,sparseRewards=true,streamingRain=true,instruction="Ride the slanted capsule rows. Jump to the next stream before your drop carries you off the left edge. Broad rings save each part.",
                platforms=supports.ToArray(),connections=links.ToArray(),hazards=Array.Empty<PlatformData>(),blackHoles=Array.Empty<BlackHoleData>(),stars=rewards.ToArray(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true,
                checkpointPlatforms=checkpoints.ToArray(),chapterTitles=new[]{"The original rain","Different currents","Read the faster row","Across the final stream"}};
            return new AuthoredCourse{number=31,title=level.name,difficulty="Expanded",mood=level.instruction,points=rewards.Count,level=level,routes=Array.Empty<AuthoredRoute>()};
        }
        public static void Build()
        {
            const string path="../assets/levels/discovery/expanded-prototype.json";
            if(!File.Exists("../assets/levels/discovery/expanded-prototype-v1.json"))File.Copy(path,"../assets/levels/discovery/expanded-prototype-v1.json");
            var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(path));
            var accepted=JsonUtility.FromJson<LevelPack>(File.ReadAllText("../assets/levels/discovery/prototype-c.json"));
            for(int i=0;i<30;i++)pack.levels[i]=accepted.levels[i];
            pack.levels[30]=Make();
            DiscoveryRegression.Run(pack);string json=JsonUtility.ToJson(pack,true);File.WriteAllText(path,json);File.WriteAllText("Assets/Resources/discovery-pack.json",json);
            AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
    }
}
