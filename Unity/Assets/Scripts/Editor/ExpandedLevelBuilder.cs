using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Mektra.Editor
{
    public static class ExpandedLevelBuilder
    {
        static PlatformData S(int i,double x,double y,double r,int family=0)=>EndlessPackBuilder.Shape(family,i,x,y,r,0);
        static RouteLink L(int a,int b)=>new RouteLink{from=a,to=b};
        static MotionData Motion(string type,double x,double y,double phase=0,double period=8)=>new MotionData{type=type,amplitudeX=x,amplitudeY=y,phase=phase,periodSeconds=period};
        static LevelData Flow(bool arc)
        {
            double[] ys=arc?new[]{3800d,3500,3200,2900,2600,2540,2200,1900,1900,1600,1300,1000,700,400}:new[]{3800d,3500,3200,2900,2600,2300,2000,1700,1700,1400,1100,800,500,200};
            double[] xs={195,120,270,195,110,280,195,110,285,195,195,115,275,195};
            var shapes=new PlatformData[14];
            for(int i=0;i<shapes.Length;i++)
            {
                double r=i==8?42:i==13?76:i==3||i==6||i==10||i==9?72:64;
                shapes[i]=!arc&&(i==1||i==2||i==4||i==5||i==7||i==11||i==12)?JourneyExpansionBuilder.Capsule(i,xs[i],ys[i],i==4||i==5?128:96,i==4||i==5?100:126,0):S(i,xs[i],ys[i],r);
            }
            shapes[8].label="bonus";
            if(arc)
            {
                foreach(int i in new[]{1,2,4,5,7,8,11,12})shapes[i].motion=Motion("orbit",i==8?10:18,i==8?12:16,i%2==0?.5:0,9);
                shapes[4]=S(4,100,ys[4],60);shapes[5]=S(5,290,ys[5],60);
                shapes[4].motion=Motion("orbit",14,18,.25,8);shapes[5].motion=Motion("orbit",14,18,.75,8);
            }
            else
            {
                shapes[1].motion=Motion("vertical",0,14);shapes[2].motion=Motion("vertical",0,14,.3);
                shapes[4].motion=Motion("horizontal",20,0);shapes[5].motion=Motion("horizontal",20,0,.5);
                shapes[7].motion=Motion("vertical",0,16,.25);shapes[11].motion=Motion("orbit",12,14);shapes[12].motion=Motion("horizontal",18,0,.25);
            }
            var links=new[]{L(0,1),L(1,2),L(2,3),L(3,4),L(4,5),L(5,6),L(6,7),L(7,9),L(6,8),L(8,9),L(9,10),L(10,11),L(11,12),L(12,13)};
            return new LevelData{id=arc?"expanded-arcs":"expanded-rain",name=arc?"Circles in motion":"A journey through rain",source="Expanded four-part authored prototype",width=390,height=4000,stride=4000,authoredRadius=GameConfig.Radius,
                discovery=true,sparseRewards=true,instruction=arc?"Read the arc before transferring. Each broad checkpoint gives you room to settle.":"Find the falling rhythm, cross the opposing rows, then choose a reward route. Broad rings save your progress.",
                platforms=shapes,connections=links,hazards=Array.Empty<PlatformData>(),blackHoles=Array.Empty<BlackHoleData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true,
                checkpointPlatforms=new[]{0,3,6,10},chapterTitles=arc?new[]{"Follow the arc","Meeting circles","Choose the transfer","Find the flow"}:new[]{"Falling rhythm","Crosscurrents","A choice in the stream","The final crossing"}};
        }
        static LevelData Rebounds()
        {
            var shapes=new List<PlatformData>{S(0,195,3800,58)};var links=new List<RouteLink>();var checkpoints=new List<int>{0};
            for(int part=0;part<4;part++)
            {
                int from=shapes.Count-1;double shift=2900-part*790;bool mirror=part%2==1;
                Point P(double x,double y)=>new Point(mirror?390-x:x,y+shift);
                Point[] points=part==2?new[]{P(261,430),P(351,700),P(335,700),P(245,430)}:part<2?new[]{P(375,300),P(375,740),P(350,740),P(310,300)}:new[]{P(326,300),P(366,740),P(350,740),P(310,300)};
                if(mirror)Array.Reverse(points);
                shapes.Add(WorldPackBuilder.Polygon("bank-"+part,points,"wedge"));
                if(part==2)shapes.Add(WorldPackBuilder.Polygon("return-plate",new[]{P(30,400),P(120,650),P(104,650),P(14,400)},"wedge"));
                int catcher=shapes.Count;
                shapes.Add(JourneyExpansionBuilder.Capsule(catcher,mirror?240:part==2?155:150,350+shift,part==2?250:part==1?220:240,part==2?110:100,0));
                int exit=shapes.Count;shapes.Add(S(exit,195,110+shift,part==3?76:part==1?64:58));
                links.Add(L(from,catcher));links.Add(L(catcher,exit));if(part<3)checkpoints.Add(exit);
            }
            return new LevelData{id="expanded-rebounds",name="The rebound passage",source="Expanded four-part authored prototype",width=390,height=4000,stride=4000,authoredRadius=GameConfig.Radius,
                discovery=true,sparseRewards=true,instruction="Follow the measured rebound dots. The bank changes sides, then two narrow plates form a rebound chain. Broad rings save each part.",platforms=shapes.ToArray(),connections=links.ToArray(),
                hazards=Array.Empty<PlatformData>(),blackHoles=Array.Empty<BlackHoleData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true,
                checkpointPlatforms=checkpoints.ToArray(),chapterTitles=new[]{"Read the bank","The return rebound","Two narrow plates","Bring it together"}};
        }
        public static Simulation Shot(LevelData level,Character c,int from,double distance,double clock=0)
        {
            var sim=new Simulation(level,c);sim.time=clock;sim.platform=sim.platforms[from];sim.distance=distance;sim.Select(c);sim.SetViewport(816);sim.energyEnabled=false;sim.Start();sim.Jump();
            for(int tick=0;tick<720&&sim.flying&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);return sim;
        }
        static AuthoredRoute Bounce(LevelData level,int from,int to,int minimumBounces,List<BounceRunData> runs,List<StarData> rewards)
        {
            var orbit=new Orbit(level.platforms[from].outline);var chars=CharacterCatalog.Load();var safe=new bool[360];
            for(int i=0;i<360;i++)
            {
                bool valid=true;foreach(var c in chars){var sim=Shot(level,c,from,orbit.length*i/360);if(sim.flying||sim.wedgeBounces<minimumBounces||sim.platform.index!=to||sim.phase!=Phase.Playing){valid=false;break;}}safe[i]=valid;
            }
            int longest=0,start=0,run=0;for(int i=0;i<720;i++){run=safe[i%360]?Math.Min(360,run+1):0;if(run>longest){longest=run;start=i-run+1;}}
            double window=longest*orbit.length/360/GameConfig.OrbitSpeed;
            if(window<.28)throw new Exception("Expanded rebound window too narrow at "+from+": "+window);
            double distance=((start+(longest-1)/2.0)%360)*orbit.length/360;
            var trace=new Simulation(level,chars[0]);trace.platform=trace.platforms[from];trace.distance=distance;trace.Select(chars[0]);trace.SetViewport(816);trace.energyEnabled=false;trace.Start();trace.Jump();
            var points=new List<Point>{trace.position};double next=.04;
            while(trace.flying&&trace.phase==Phase.Playing&&trace.time<6){trace.Advance(GameConfig.Step);if(trace.time>=next){points.Add(trace.position);next+=.04;}}
            points.Add(trace.position);if(trace.flying||trace.platform.index!=to||trace.wedgeBounces<minimumBounces)throw new Exception("Expanded rebound trace missed catcher");
            runs.Add(new BounceRunData{entry=from,exit=to,minimumBounces=minimumBounces,points=points.ToArray()});
            for(int i=1;i<=2;i++){var p=points[Math.Min(points.Count-1,points.Count*i/3)];rewards.Add(new StarData{id="bank-dot-"+rewards.Count,x=p.x,y=p.y});}
            Debug.Log("MEKTRA_EXPANDED_REBOUND_PASS "+from+" window "+window.ToString("0.000"));
            return new AuthoredRoute{from=from,to=to,distance=distance,windowSeconds=window,launch=points[0],landing=trace.position};
        }
        static AuthoredCourse Make(int index)
        {
            if(index==30)return RainStreamBuilder.Make();
            var level=index==31?Rebounds():Flow(true);var routes=new List<AuthoredRoute>();var rewards=new List<StarData>();var runs=new List<BounceRunData>();
            foreach(var link in level.connections)
            {
                if(index==31&&Array.IndexOf(level.checkpointPlatforms,link.from)>=0){routes.Add(Bounce(level,link.from,link.to,link.from==6?2:1,runs,rewards));continue;}
                var review=CourseReview.Review(level,link.from,CharacterCatalog.Load(),link.to);
                routes.Add(new AuthoredRoute{from=link.from,to=link.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                if(index==31)continue;
                bool bonus=level.platforms[link.to].label=="bonus";
                bool reward=bonus||link.from==0||link.from==1||link.from==3||link.from==4||link.from==6&&link.to==7||link.from==9||link.from==10||link.from==11;
                if(reward)for(int i=0;i<(bonus?3:1);i++){double f=bonus?.35+i*.15:.5;var at=review.launch+(review.landing-review.launch)*f;rewards.Add(new StarData{id="flow-dot-"+rewards.Count,x=at.x,y=at.y,guideFrom=link.from,guideTo=link.to,guideFraction=f});}
            }
            level.stars=rewards.ToArray();level.bounceRuns=runs.ToArray();
            return new AuthoredCourse{number=index+1,title=level.name,difficulty="Expanded",mood=level.instruction,points=rewards.Count,level=level,routes=routes.ToArray()};
        }
        public static void Build()
        {
            var approved=JsonUtility.FromJson<LevelPack>(File.ReadAllText("../assets/levels/discovery/prototype-c.json"));
            if(approved.levels.Length!=30)throw new Exception("Expanded prototype requires the accepted thirty");
            var entries=new List<AuthoredCourse>(approved.levels);for(int i=30;i<33;i++)entries.Add(Make(i));
            var pack=new LevelPack{levels=entries.ToArray()};DiscoveryRegression.Run(pack);
            string json=JsonUtility.ToJson(pack,true);File.WriteAllText("../assets/levels/discovery/expanded-prototype.json",json);File.WriteAllText("Assets/Resources/discovery-pack.json",json);
            AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
    }
}
