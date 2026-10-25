using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Mektra.Editor
{
    public static class DiscoveryPackBuilder
    {
        static RouteLink Link(int from,int to)=>new RouteLink{from=from,to=to};
        static PlatformData Shape(int index,double x,double y,double radius,int family=0)=>EndlessPackBuilder.Shape(family,index,x,y,radius,0);
        static MotionData Motion(double phase=0)=>new MotionData{type="horizontal",amplitudeX=18,periodSeconds=6.5,phase=phase};
        static AuthoredCourse Make(int index)
        {
            string[] names={"First launch","Across the screen","A worthwhile choice","Lead the landing","Rain crossing","First black hole"};
            string[] rules={"Orbit, then tap toward the broad landing.","Read the diagonal. Cross, then come back.","Choose the broad route or the smaller bonus landing.","Watch the moving target before committing.","Cross the moving rows. Follow a few clear rewards.","Stay outside the vortex ring. Take the broad left route."};
            PlatformData[] shapes;RouteLink[] links;
            if(index==0){shapes=new[]{Shape(0,195,850,56),Shape(1,175,560,60),Shape(2,205,270,60)};links=new[]{Link(0,1),Link(1,2)};}
            else if(index==1){shapes=new[]{Shape(0,195,850,54),Shape(1,105,610,60),Shape(2,270,370,74,5),Shape(3,195,130,66)};links=new[]{Link(0,1),Link(1,2),Link(2,3)};}
            else if(index==2){shapes=new[]{Shape(0,195,850,56),Shape(1,145,640,50),Shape(2,100,390,56),Shape(3,290,430,34),Shape(4,195,170,60)};links=new[]{Link(0,1),Link(1,2),Link(1,3),Link(2,4),Link(3,4)};}
            else if(index==3){shapes=new[]{Shape(0,195,850,56),Shape(1,130,610,54),Shape(2,260,375,74,5),Shape(3,195,130,66)};shapes[1].motion=Motion();links=new[]{Link(0,1),Link(1,2),Link(2,3)};}
            else if(index==4)
            {
                shapes=new[]{Shape(0,195,850,58),Shape(1,112,620,60),Shape(2,278,385,60),Shape(3,278,640,60),Shape(4,112,375,60),Shape(5,195,140,60)};
                for(int i=1;i<=4;i++){shapes[i].motion=Motion(i%2==0?.75:.25);shapes[i].motion.amplitudeX=12;shapes[i].motion.periodSeconds=7;}
                links=new[]{Link(0,1),Link(0,3),Link(1,2),Link(1,4),Link(3,2),Link(3,4),Link(2,5),Link(4,5)};
            }
            else{shapes=new[]{Shape(0,195,850,58),Shape(1,95,610,54),Shape(2,115,360,56),Shape(3,195,110,60)};links=new[]{Link(0,1),Link(1,2),Link(2,3)};}
            var level=new LevelData{id="discovery-a-"+index,name=names[index],source="Prototype A authored discovery encounters",width=390,height=1000,stride=1000,authoredRadius=GameConfig.Radius,discovery=true,sparseRewards=true,instruction=rules[index],segmentKind=index,platforms=shapes,connections=links,hazards=Array.Empty<PlatformData>(),blackHoles=index==5?new[]{new BlackHoleData{id="intro-vortex",x=285,y=470,radius=20,pullRadius=58}}:Array.Empty<BlackHoleData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true};
            var routes=new List<AuthoredRoute>();var rewards=new List<StarData>();
            foreach(var link in links)
            {
                var review=CourseReview.Review(level,link.from,CharacterCatalog.Load(),link.to);
                routes.Add(new AuthoredRoute{from=link.from,to=link.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                bool reward=index==4?(link.from==0&&link.to==1||link.from==1&&link.to==2||link.from==2&&link.to==5):index==2?link.from==1&&link.to==3:link.from<3;
                int count=index==2?3:1;
                if(reward)for(int n=0;n<count;n++)
                {
                    double f=count==1?.5:.35+n*.15;var at=review.launch+(review.landing-review.launch)*f;
                    rewards.Add(new StarData{id="dot-"+rewards.Count,x=at.x,y=at.y,guideFrom=link.from,guideTo=link.to,guideFraction=f});
                }
            }
            level.stars=rewards.ToArray();
            return new AuthoredCourse{number=index+1,title=names[index],difficulty="Discovery",mood=rules[index],points=rewards.Count,level=level,routes=routes.ToArray()};
        }
        // Keep the established editor entry point on the current prototype pack.
        public static void Build()=>CampaignWaveBuilder.Build();
    }
}
