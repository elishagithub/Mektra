using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    public static class RainStreamRegression
    {
        static void Check(bool value,string message){if(!value)throw new Exception("Rain stream: "+message);}
        public static void Run(LevelData level,Character[] characters)
        {
            Check(level.streamingRain&&level.platforms.Length==65&&level.stars.Length==8,"four flowing parts with eight rewards");
            Check(Array.FindAll(level.platforms,p=>p.motion?.type=="rain").Length==60,"sixty streaming capsules");
            Check(level.connections.Length==240,"all next-row alternatives remain available");
            foreach(var platform in level.platforms)if(platform.motion!=null&&platform.motion.type!="static")
            {
                var m=platform.motion;Check(m.type=="rain"&&m.amplitudeX==520&&m.amplitudeY==52,"original diagonal streaming motion");
                // Speed variation changes the wrap time. Search the actual discontinuity, not the linear estimate.
                double previous=m.At(0).x;bool wrapped=false;
                for(double t=.01;t<=m.periodSeconds+.01;t+=.01)
                {
                    var now=m.At(t);if(now.x-previous>400)
                    {
                        var before=m.At(t-.01);Check(platform.bounds.maxX+before.x+GameConfig.Radius<0&&platform.bounds.minX+now.x-GameConfig.Radius>390,"recycle happens beyond both screen edges");wrapped=true;break;
                    }
                    Check(now.x<=previous+.001,"rain flows continuously to the left");previous=now.x;
                }
                Check(wrapped,"rain actually recycles");
            }
            var reward=level.stars[0];int parent=Array.FindIndex(level.platforms,p=>p.id==reward.parentPlatformId);var flow=level.platforms[parent].motion;
            double low=0,high=0;
            for(double t=.01;t<=flow.periodSeconds+.01;t+=.01)if(flow.At(t).x-flow.At(t-.01).x>400){low=t-.01;high=t;break;}
            double initial=flow.At(low).x;for(int i=0;i<45;i++){double middle=(low+high)*.5;if(flow.At(middle).x>initial+400)high=middle;else low=middle;}
            var pickup=new Simulation(level,characters[0]);pickup.time=(low+high)*.5-GameConfig.Step*.5;
            var dot=pickup.stars[0];pickup.position=(dot.At(pickup.time)+dot.At(pickup.time+GameConfig.Step))*.5;pickup.SetViewport(816);pickup.Start();pickup.flying=true;pickup.velocity=new Point();
            for(int i=1;i<pickup.stars.Count;i++)pickup.stars[i].collected=true;
            pickup.Advance(GameConfig.Step);Check(!dot.collected&&pickup.score==0,"off-screen recycling cannot award a phantom pickup");
            double minimum=double.PositiveInfinity;int windows=0;var narrow=new List<string>();
            for(int part=0;part<4;part++)foreach(double clock in new[]{0d,5,17,31})for(int group=0;group<4;group++)
            {
                int from=part*16;
                if(group>0)
                {
                    double closest=double.PositiveInfinity;
                    for(int col=0;col<5;col++)
                    {
                        int index=part*16+1+(group-1)*5+col;var p=level.platforms[index];double x=(p.bounds.minX+p.bounds.maxX)*.5+p.motion.At(clock).x;
                        if(Math.Abs(x-195)<closest){closest=Math.Abs(x-195);from=index;}
                    }
                }
                var orbit=new Orbit(level.platforms[from].outline);var valid=new bool[180];
                for(int sample=0;sample<180;sample++)
                {
                    orbit.Pose(orbit.length*sample/180,out var point,out var normal);if(normal.y>=-.05)continue;
                    bool safe=true;
                    foreach(var character in characters)
                    {
                        var sim=ExpandedLevelBuilder.Shot(level,character,from,orbit.length*sample/180,clock);
                        if(sim.flying||(sim.phase!=Phase.Playing&&sim.phase!=Phase.Complete)||!Array.Exists(level.connections,l=>l.from==from&&l.to==sim.platform.index)){safe=false;break;}
                    }
                    valid[sample]=safe;
                }
                int longest=0,run=0;for(int i=0;i<360;i++){run=valid[i%180]?Math.Min(180,run+1):0;longest=Math.Max(longest,run);}
                double seconds=longest*orbit.length/180/GameConfig.OrbitSpeed;
                if(seconds<.2)narrow.Add("part "+part+", group "+group+", clock "+clock+" / "+seconds.ToString("0.000"));
                minimum=Math.Min(minimum,seconds);windows++;
            }
            Check(narrow.Count==0,"row choice windows: "+string.Join("; ",narrow));
            Debug.Log("MEKTRA_RAIN_STREAM_PASS "+windows+" live row-choice windows across "+characters.Length+" characters; minimum "+minimum.ToString("0.000")+"s; off-screen recycling and original capsule motion");
        }
    }
}
