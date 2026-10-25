using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    // Familiarity, combinations and recovery; the accepted twenty remain a separate artifact.
    public static class DiscoveryDepthBuilder
    {
        static PlatformData S(int i,double x,double y,double r,int family=0)=>EndlessPackBuilder.Shape(family,i,x,y,r,0);
        static RouteLink L(int from,int to)=>new RouteLink{from=from,to=to};
        static RouteLink[] Chain(int count){var links=new RouteLink[count-1];for(int i=0;i<links.Length;i++)links[i]=L(i,i+1);return links;}
        static MotionData Drift(double phase=0)=>new MotionData{type="horizontal",amplitudeX=16,periodSeconds=7.5,phase=phase};
        static PlatformData Gate(int id,double y,double phase)
        {
            var gate=JourneyExpansionBuilder.Capsule(id,195,y,70,14,0);gate.id="depth-gate-"+id;gate.sourceShapeId="rounded-hazard";
            gate.motion=Drift(phase);gate.motion.amplitudeX=100;return gate;
        }
        public static AuthoredCourse Make(int index)
        {
            PlatformData[] shapes;RouteLink[] links;string name,instruction;
            PlatformData[] hazards=Array.Empty<PlatformData>();BlackHoleData[] holes=Array.Empty<BlackHoleData>();
            switch(index)
            {
                case 20:
                    name="The returning arc";instruction="Read the curve on the right, then cross back through the open centre.";
                    shapes=new[]{S(0,195,880,60),S(1,275,640,70,5),S(2,115,400,64),S(3,245,160,68)};links=Chain(4);break;
                case 21:
                    name="Rise and settle";instruction="Lead the gently rising landing. Settle on solid ground before the next crossing.";
                    shapes=new[]{S(0,195,900,60),S(1,145,670,62),S(2,260,430,70),S(3,155,190,68)};
                    shapes[1].motion=new MotionData{type="vertical",amplitudeY=14,periodSeconds=8};links=Chain(4);break;
                case 22:
                    name="Beside the vortex";instruction="The left route stays clear of the ring. Take the smaller right landing for the reward, then rejoin.";
                    shapes=new[]{S(0,195,900,60),S(1,105,670,62),S(2,105,425,64),S(3,265,655,42),S(4,195,170,68)};
                    shapes[3].label="bonus";links=new[]{L(0,1),L(1,2),L(2,4),L(0,3),L(3,2)};
                    holes=new[]{new BlackHoleData{id="depth-vortex",x=285,y=400,radius=20,pullRadius=55}};break;
                case 23:
                    name="Fade, then focus";instruction="Leave the fading ring. The next landing is stable, so take time to read its departure.";
                    shapes=new[]{S(0,195,900,62),S(1,125,660,62),S(2,260,420,54),S(3,155,160,72)};
                    shapes[1].crumbleSeconds=6.5;links=Chain(4);break;
                case 24:
                    name="Two quiet openings";instruction="Read one gate at a time. Rest on the broad supports between the two openings.";
                    shapes=new[]{S(0,195,930,62),S(1,145,720,64),S(2,245,500,68),S(3,145,280,64),S(4,195,70,70)};
                    hazards=new[]{Gate(0,825,.25),Gate(1,390,.25)};hazards[1].motion.periodSeconds=8.5;links=Chain(5);break;
                case 25:
                    name="A wider rhythm";instruction="Room to recover. Ride the broad curve and follow the open crossing.";
                    shapes=new[]{S(0,195,900,66),S(1,195,550,110,5),S(2,195,150,76)};links=Chain(3);break;
                case 26:
                    name="Choose your crossing";instruction="The broad hexagon is the safe route. The smaller landing offers three reward dots before both paths rejoin.";
                    shapes=new[]{S(0,195,900,62),S(1,105,650,70,2),S(2,285,650,40),S(3,195,360,72),S(4,195,100,70)};
                    shapes[2].label="bonus";links=new[]{L(0,1),L(1,3),L(0,2),L(2,3),L(3,4)};break;
                case 27:
                    name="Crossing the tide";instruction="The two landings drift in opposite rhythms. Read each crossing separately.";
                    shapes=new[]{S(0,195,900,62),S(1,115,650,64),S(2,275,400,66),S(3,155,130,72)};
                    shapes[1].motion=Drift();shapes[2].motion=Drift(.5);links=Chain(4);break;
                case 28:
                    name="Wait, then leave";instruction="Wait safely for the gate. The later fading support asks for a timely departure, with no gate in its way.";
                    shapes=new[]{S(0,195,900,62),S(1,145,650,66),S(2,250,420,62),S(3,155,180,72)};
                    hazards=new[]{Gate(0,775,.25)};shapes[2].crumbleSeconds=6.5;links=Chain(4);break;
                case 29:
                    name="Find your flow";instruction="Curve, moving landing, then a choice. Take the broad finish or earn the smaller route's reward.";
                    shapes=new[]{S(0,195,930,64),S(1,110,710,78,5),S(2,265,480,66),S(3,115,265,70),S(4,300,245,42),S(5,195,50,72)};
                    shapes[2].motion=Drift();shapes[2].motion.amplitudeX=12;shapes[4].label="bonus";
                    links=new[]{L(0,1),L(1,2),L(2,3),L(2,4),L(3,5),L(4,5)};break;
                default:throw new ArgumentOutOfRangeException(nameof(index));
            }
            var level=new LevelData{id="discovery-c-"+index,name=name,source="Prototype C authored depth encounters",width=390,height=1000,stride=1000,
                authoredRadius=GameConfig.Radius,discovery=true,sparseRewards=true,instruction=instruction,segmentKind=index,platforms=shapes,connections=links,
                hazards=hazards,blackHoles=holes,stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true};
            var routes=new List<AuthoredRoute>();var rewards=new List<StarData>();var chars=CharacterCatalog.Load();
            foreach(var link in links)
            {
                var r=CourseReview.Review(level,link.from,chars,link.to);
                routes.Add(new AuthoredRoute{from=link.from,to=link.to,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing});
                bool bonus=shapes[link.to].label=="bonus";
                bool reward=index==25?link.from==1:index==22||index==26?bonus:index==29?bonus||link.from==0:routes.Count<=3;
                int count=bonus?3:1;
                if(reward)for(int n=0;n<count;n++)
                {
                    double f=count==1?.5:.35+n*.15;var at=r.launch+(r.landing-r.launch)*f;
                    rewards.Add(new StarData{id="depth-dot-"+rewards.Count,x=at.x,y=at.y,guideFrom=link.from,guideTo=link.to,guideFraction=f});
                }
            }
            level.stars=rewards.ToArray();Debug.Log("MEKTRA_DEPTH_AUTHORED "+(index+1)+" "+name);
            return new AuthoredCourse{number=index+1,title=name,difficulty=index==25?"Recovery":index<24?"Familiarity":"Combination",mood=instruction,points=rewards.Count,level=level,routes=routes.ToArray()};
        }
    }
}
