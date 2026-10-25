using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    // The fourteen encounters after the approved, frozen Prototype A opening.
    public static class DiscoveryExpansionBuilder
    {
        static RouteLink L(int a,int b)=>new RouteLink{from=a,to=b};
        static PlatformData S(int i,double x,double y,double r,int family=0)=>EndlessPackBuilder.Shape(family,i,x,y,r,0);
        static PlatformData Capsule(int i,double x,double y,double w,double h,double turn=0)=>JourneyExpansionBuilder.Capsule(i,x,y,w,h,turn);
        static RouteLink[] Chain(int count){var links=new RouteLink[count-1];for(int i=0;i<links.Length;i++)links[i]=L(i,i+1);return links;}
        static MotionData Drift(double phase=0,double amplitude=14)=>new MotionData{type="horizontal",amplitudeX=amplitude,periodSeconds=8,phase=phase};
        static PlatformData Rail(int id,double x,double y,double width,double height,double turn=0)
        {
            var p=Capsule(id,x,y,width,height,turn);p.id="hazard-"+id;p.sourceShapeId="rounded-hazard";return p;
        }
        static LevelData Base(int index,string name,string instruction,PlatformData[] shapes,RouteLink[] links)=>new LevelData
        {
            id="discovery-b-"+index,name=name,source="Prototype B authored discovery encounters",width=390,height=1000,stride=1000,
            authoredRadius=GameConfig.Radius,discovery=true,sparseRewards=true,instruction=instruction,segmentKind=index,
            platforms=shapes,connections=links,hazards=Array.Empty<PlatformData>(),blackHoles=Array.Empty<BlackHoleData>(),
            stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true
        };
        public static AuthoredCourse Make(int index)
        {
            PlatformData[] shapes;LevelData level;
            switch(index)
            {
                case 6:
                    var bank=WorldPackBuilder.Polygon("platform-2",new[]{new Point(375,300),new Point(375,740),new Point(350,740),new Point(310,300)},"wedge");
                    shapes=new[]{S(0,195,900,58),bank,Capsule(2,130,350,240,100),S(3,195,110,66)};
                    level=Base(index,"Rebound","Launch into the sloped bank. Let the rebound carry you to the broad catcher.",shapes,new[]{L(0,2),L(2,3)});break;
                case 7:
                    shapes=new[]{S(0,195,850,58),S(1,135,620,64),S(2,260,380,76,5),S(3,195,130,66)};shapes[1].crumbleSeconds=8;
                    level=Base(index,"Leave before it fades","The ring counts down after landing. Leave the fading support for a stable one.",shapes,Chain(4));break;
                case 8:
                    shapes=new[]{S(0,195,850,58),S(1,145,610,62),S(2,255,370,66),S(3,195,130,66)};
                    level=Base(index,"Read the opening","Striped bars are hazards. Watch the gate, then commit when the passage opens.",shapes,Chain(4));
                    var gate=Rail(0,195,720,90,16);gate.motion=Drift(.25,105);gate.motion.periodSeconds=6;level.hazards=new[]{gate};break;
                case 9:
                    shapes=new[]{S(0,195,850,60),S(1,115,620,70,2),S(2,260,370,76,2),S(3,115,120,66)};
                    level=Base(index,"Honeycomb","Read the broad hexagon faces. Follow their diagonal openings.",shapes,Chain(4));break;
                case 10:
                    shapes=new[]{S(0,195,870,60),Capsule(1,114,640,122,88,-.1),Capsule(2,276,420,122,88,.1),Capsule(3,276,650,122,88,-.1),Capsule(4,114,410,138,104,.1),S(5,195,130,74)};
                    shapes[1].motion=Drift(0);shapes[2].motion=Drift(.25);shapes[3].motion=Drift(.5);shapes[4].motion=Drift(.75);
                    level=Base(index,"Counterflow","The rounded rows move against one another. Choose a crossing, then find the exit.",shapes,new[]{L(0,1),L(1,2),L(2,5),L(0,3),L(3,4),L(4,5)});break;
                case 11:
                    shapes=new[]{S(0,195,850,60),S(1,150,610,64),S(2,235,365,76,5),S(3,195,130,68)};
                    level=Base(index,"Through the gap","The striped walls frame the route. Aim through the open space between them.",shapes,Chain(4));
                    level.hazards=new[]{Rail(0,55,470,200,18,Math.PI/2),Rail(1,335,470,200,18,Math.PI/2)};break;
                case 12:
                    shapes=new[]{S(0,195,900,64),S(1,245,500,82),S(2,155,120,76)};
                    level=Base(index,"The long jump","Commit to the open crossing. The large destination gives you room to arrive.",shapes,Chain(3));break;
                case 13:
                    shapes=new[]{S(0,195,900,60),S(1,110,680,62),S(2,280,470,70,5),S(3,110,270,64),S(4,195,70,68)};
                    level=Base(index,"Change direction","Cross left, then right. Read the next turn before departing.",shapes,Chain(5));break;
                case 14:
                    shapes=new[]{S(0,195,900,60),S(1,115,650,72,2),S(2,260,400,76,2),S(3,285,680,50),S(4,100,360,56),S(5,115,140,68)};
                    shapes[3].label=shapes[4].label="bonus";
                    level=Base(index,"Connected field","Take the broad hexagon route or the smaller crossing. Both rejoin at the exit.",shapes,new[]{L(0,1),L(1,2),L(2,5),L(0,3),L(3,4),L(4,5)});break;
                case 15:
                    shapes=new[]{S(0,195,900,62),S(1,195,560,112,5),S(2,110,290,60),S(3,230,90,68)};
                    level=Base(index,"Giant curves","Ride a larger curve, then step back onto the smaller landings.",shapes,Chain(4));break;
                case 16:
                    shapes=new[]{S(0,195,850,62),S(1,145,610,62),S(2,245,375,60),S(3,195,130,68)};
                    level=Base(index,"Quiet precision","One short framed crossing. Find its centre, then enjoy the broad recovery landing.",shapes,Chain(4));
                    level.hazards=new[]{Rail(0,120,495,70,14,Math.PI/2),Rail(1,280,495,70,14,Math.PI/2)};break;
                case 17:
                    shapes=new[]{S(0,205,850,66),S(1,155,560,74),S(2,225,260,74)};
                    level=Base(index,"Room to breathe","Broad landings. Settle into your rhythm before the next passage.",shapes,Chain(3));break;
                case 18:
                    shapes=new[]{S(0,195,900,62),S(1,125,680,62),S(2,245,450,70),S(3,140,240,66),S(4,230,50,68)};shapes[1].motion=Drift(0,12);
                    level=Base(index,"Familiar combination","Lead the moving landing, rest on solid ground, then read the gate. One decision at a time.",shapes,Chain(5));
                    var laterGate=Rail(0,195,345,70,14);laterGate.motion=Drift(.25,100);laterGate.motion.periodSeconds=7;level.hazards=new[]{laterGate};break;
                case 19:
                    shapes=new[]{S(0,195,900,62),S(1,125,690,78,5),S(2,110,445,68),S(3,285,460,46),S(4,195,230,72),S(5,195,20,72)};shapes[3].label="bonus";
                    level=Base(index,"First journey payoff","A familiar curve, a choice, and a broad final crossing. Finish your first discoveries.",shapes,new[]{L(0,1),L(1,2),L(1,3),L(2,4),L(3,4),L(4,5)});break;
                default:throw new ArgumentOutOfRangeException(nameof(index));
            }
            var characters=CharacterCatalog.Load();var routes=new List<AuthoredRoute>();var rewards=new List<StarData>();
            foreach(var link in level.connections)
            {
                var r=CourseReview.Review(level,link.from,characters,link.to);
                routes.Add(new AuthoredRoute{from=link.from,to=link.to,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing});
                bool reward=index==17?link.from==1:index==14?link.from==0&&link.to==3||link.from==3:index==19?link.from==1&&link.to==3:routes.Count<=3;
                int count=index==19?3:1;
                if(reward)for(int dot=0;dot<count;dot++)
                {
                    double f=count==1?.5:.35+dot*.15;var at=r.launch+(r.landing-r.launch)*f;
                    rewards.Add(new StarData{id="dot-"+rewards.Count,x=at.x,y=at.y,guideFrom=link.from,guideTo=link.to,guideFraction=f});
                }
            }
            if(index==6)AuthorRebound(level,rewards,characters);
            level.stars=rewards.ToArray();
            Debug.Log("MEKTRA_DISCOVERY_AUTHORED "+(index+1)+" "+level.name);
            return new AuthoredCourse{number=index+1,title=level.name,difficulty="Discovery",mood=level.instruction,points=rewards.Count,level=level,routes=routes.ToArray()};
        }
        static Simulation BounceShot(LevelData level,Character character,double distance)
        {
            var sim=new Simulation(level,character);sim.energyEnabled=false;sim.distance=distance;sim.Select(character);sim.SetViewport(816);sim.Start();sim.Jump();
            for(int tick=0;tick<720&&sim.flying&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);
            return sim;
        }
        static void AuthorRebound(LevelData level,List<StarData> rewards,Character[] characters)
        {
            var orbit=new Orbit(level.platforms[0].outline);var valid=new bool[360];
            for(int i=0;i<360;i++)
            {
                bool safe=true;foreach(var character in characters)
                {
                    var sim=BounceShot(level,character,orbit.length*i/360);
                    if(sim.phase!=Phase.Playing||sim.flying||sim.platform.index!=2||sim.wedgeBounces==0){safe=false;break;}
                }
                valid[i]=safe;
            }
            int longest=0,start=0,run=0;
            for(int i=0;i<720;i++){run=valid[i%360]?Math.Min(360,run+1):0;if(run>longest){longest=run;start=i-run+1;}}
            if(longest*orbit.length/360/GameConfig.OrbitSpeed<.3)throw new Exception("Discovery rebound shared physical window too narrow: "+(longest*orbit.length/360/GameConfig.OrbitSpeed));
            double distance=((start+(longest-1)/2.0)%360)*orbit.length/360;
            var trace=new Simulation(level,characters[0]);trace.energyEnabled=false;trace.distance=distance;trace.Select(characters[0]);trace.SetViewport(816);trace.Start();trace.Jump();
            var points=new List<Point>{trace.position};double next=.04;
            while(trace.flying&&trace.phase==Phase.Playing&&trace.time<6)
            {
                trace.Advance(GameConfig.Step);if(trace.time>=next){points.Add(trace.position);next+=.04;}
            }
            points.Add(trace.position);
            if(trace.flying||trace.wedgeBounces==0||trace.platform.index!=2)throw new Exception("Measured rebound trace missed catcher");
            level.bounceRuns=new[]{new BounceRunData{entry=0,exit=2,points=points.ToArray()}};
            rewards.Clear();for(int i=1;i<=3;i++){var at=points[Math.Min(points.Count-1,points.Count*i/4)];rewards.Add(new StarData{id="rebound-dot-"+i,x=at.x,y=at.y});}
            Debug.Log("MEKTRA_DISCOVERY_REBOUND_PASS shared physical window "+(longest*orbit.length/360/GameConfig.OrbitSpeed).ToString("0.000")+"s across "+characters.Length+" characters");
        }
    }
}
