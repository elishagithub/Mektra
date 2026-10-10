using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    // Reposition the user's actual paths. Never substitute generated circles/diamonds for them.
    public static class CustomCourseBuilder
    {
        [Serializable] class Catalog {public PlatformData[] shapes;}
        [Serializable] class Pack {public int screenHeight=816,screensPerLevel=4;public AuthoredCourse[] levels;}
        static readonly string[] Titles={"First Spark","Custom Crossing","Hook & Hollow","Playful Detour","Edge Timing","Sideways Rhythm","Narrow Passage","Three-Star Chase","Bent Metal","Tension Steps","Shape Carousel","Crossing Sparks","Precision Run","Breathing Room","Wall Whisper","Mechanical Maze","Critical Timing","Star Festival","Final Pressure","Mektra Master"};
        static readonly string[] Moods={"Easy","Easy","Intermediate","Fun","Tension","Intermediate","Tough","Fun","Intermediate","Tension","Tough","Tough","Critical","Fun","Tension","Tough","Critical","Fun","Critical","Critical"};
        static readonly int[] Unique={4,5,8,17,19,21,22,23,24,25,26,27,28,3,6,7,13,16,0,1,2,9,10,11,12,14,15,18,20};
        static string F(double n)=>n.ToString("0.#####",CultureInfo.InvariantCulture);
        static PlatformData Place(PlatformData source,int slot,double x,double y)
        {
            var p=JsonUtility.FromJson<PlatformData>(JsonUtility.ToJson(source));
            double dx=x-(source.bounds.maxX-source.bounds.minX)/2,dy=y-(source.bounds.maxY-source.bounds.minY)/2;
            foreach(var q in p.paths)for(int i=0;i<q.v.Length;i+=2){q.v[i]+=dx;q.v[i+1]+=dy;}
            for(int i=0;i<p.outline.Length;i++)p.outline[i]+=new Point(dx,dy);
            p.bounds=new BoundsData{minX=dx,maxX=source.bounds.maxX+dx,minY=dy,maxY=source.bounds.maxY+dy};
            string d="";foreach(var cmd in p.paths){d+=cmd.op;foreach(double v in cmd.v)d+=F(v)+" ";}
            p.d=d;p.id="platform-"+(slot+1)+"-"+source.id;p.sourceShapeId=source.id;p.motion=null;return p;
        }
        static Simulation Launch(LevelData level,Character c,int index,double distance)
        {
            var s=new Simulation(level,c);s.platform=s.platforms[index];s.distance=distance;s.Select(c);s.SetViewport(816);s.Start();s.Jump();
            for(int step=0;step<240&&s.flying&&s.phase==Phase.Playing;step++)s.Advance(GameConfig.Step);
            return s;
        }
        static AuthoredRoute Route(LevelData level,Character[] characters,int from)
        {
            var orbit=new Orbit(level.platforms[from].outline);var target=level.platforms[from+1];var candidates=new List<double>();
            for(int sample=0;sample<1440;sample++)
            {
                double d=sample*orbit.length/1440;orbit.Pose(d,out var p,out var n);if(n.y>=-.15)continue;
                double hit=double.PositiveInfinity;for(int edge=0;edge<target.outline.Length;edge++)
                {double ray=Geometry.Ray(p,n,target.outline[edge],target.outline[(edge+1)%target.outline.Length]);if(ray>0)hit=Math.Min(hit,ray);}
                if(hit>20&&hit<480)candidates.Add(d);
            }
            candidates.Sort((a,b)=>{orbit.Pose(a,out var p,out var n);orbit.Pose(b,out var q,out var m);return Math.Abs(n.x).CompareTo(Math.Abs(m.x));});
            foreach(double d in candidates)
            {
                bool valid=true;foreach(var c in characters){var s=Launch(level,c,from,d);if(s.phase!=Phase.Playing||s.flying||s.platform.index!=from+1){valid=false;break;}}
                if(!valid)continue;
                var start=new Simulation(level,characters[0]);start.platform=start.platforms[from];start.distance=d;start.Select(characters[0]);
                var landing=Launch(level,characters[0],from,d);
                return new AuthoredRoute{from=from,to=from+1,distance=d,windowSeconds=candidates.Count/1440.0*orbit.length/GameConfig.OrbitSpeed,launch=start.position,landing=landing.position};
            }
            throw new Exception("No perpendicular jump for custom level "+level.id+" platform "+from);
        }
        public static void Build()
        {
            var catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText("../assets/shapes/custom/catalog.json"));
            var characters=CharacterCatalog.Load();var designs=new List<AuthoredCourse>();
            for(int number=1;number<=20;number++)
            {
                string mood=Moods[number-1];int count=mood=="Critical"?10:mood=="Easy"||mood=="Fun"?12:11;
                var platforms=new List<PlatformData>();double spread=mood=="Easy"?65:mood=="Tension"?112:mood=="Critical"?105:90;
                for(int i=0;i<count;i++)
                {
                    int shape=i==0||i==count-1?0:Unique[((number-1)*5+i-1)%Unique.Length];var template=catalog.shapes[shape];
                    // Four deliberate lane rhythms: zigzag, crossing, two-step clusters, and wave.
                    double lane=(number-1)%4==0?Math.Sin(i*1.62+number*.4):(number-1)%4==1?(i%2==0?-.85:.85):(number-1)%4==2?Math.Sin(Math.Floor(i/2.0)*2.1+number*.3):Math.Sin(i*1.15+number*.5);
                    double x=i==0||i==count-1?195:195+lane*spread;
                    double half=(template.bounds.maxX-template.bounds.minX)/2;x=Math.Clamp(x,half+25,365-half);
                    double baseY=3144-i*(3024.0/(count-1));
                    double variation=i==0||i==count-1?0:Math.Sin(i*1.7+number)*24;
                    platforms.Add(Place(template,i,x,baseY+variation));
                }
                var level=new LevelData{id="mektra-custom-"+number.ToString("00"),name=Titles[number-1],source="29 original current-level custom SVG objects",width=390,height=3264,stride=3264,platforms=platforms.ToArray(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>()};
                var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
                for(int i=0;i<count-1;i++)
                {
                    var route=Route(level,characters,i);routes.Add(route);
                    int reward=mood=="Fun"?3:(i%4==0?2:1);var direction=(route.landing-route.launch).Normal;double flight=(route.landing-route.launch).Length;
                    for(int k=0;k<reward;k++)
                    {
                        double fraction=reward==1?.48:.28+k*(.44/(reward-1));var p=route.launch+direction*(flight*fraction);
                        stars.Add(new StarData{id="star-"+(i+1)+"-"+(k+1),x=p.x,y=p.y});
                    }
                }
                level.stars=stars.ToArray();
                foreach(var r in routes)foreach(var c in characters)
                {
                    var sim=Launch(level,c,r.from,r.distance);if(sim.phase!=Phase.Playing||sim.flying||sim.platform.index!=r.to||sim.score==0)throw new Exception("Custom star route failed "+number+" / "+r.from+" / "+c.id);
                }
                designs.Add(new AuthoredCourse{number=number,title=Titles[number-1],difficulty=mood,mood=mood,theme=ThemeJourney.Names[(number-1)/4%6],points=stars.Count,level=level,routes=routes.ToArray()});
                Debug.Log("MEKTRA_CUSTOM_COURSE "+number+" built from original paths");
            }
            var pack=new LevelPack{levels=designs.ToArray()};LevelPackRegression.Run(pack);MotionRegression.Run(pack);
            string folder="../assets/levels/pack-20";Directory.CreateDirectory(folder);
            File.WriteAllText(folder+"/pack.json",JsonUtility.ToJson(new Pack{levels=pack.levels},true));
            foreach(var c in pack.levels)File.WriteAllText(folder+"/Mektra_level_"+c.number+".json",JsonUtility.ToJson(c,true));
            File.WriteAllText("Assets/Resources/level-pack-20.json",JsonUtility.ToJson(new Pack{levels=pack.levels}));
            Debug.Log("MEKTRA_CUSTOM_PACK_SUCCESS: 20 courses; original custom shapes only.");
        }
    }
}
