using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Mektra.Editor
{
    public static class CampaignWaveBuilder
    {
        static PlatformData S(int i,double x,double y,double r,int family=0)=>EndlessPackBuilder.Shape(family,i,x,y,r,0);
        static RouteLink L(int a,int b)=>new RouteLink{from=a,to=b};
        static MotionData Move(string kind,double x,double y,double phase=0,double period=12)=>new MotionData{type=kind,amplitudeX=x,amplitudeY=y,phase=phase,periodSeconds=period};
        static PlatformData Gate(int i,double x,double y,double width,string openedBy=null)
        {
            var gate=JourneyExpansionBuilder.Capsule(i,x,y,width,14,0);gate.id="wave-gate-"+i;gate.sourceShapeId="rounded-hazard";gate.openedBy=openedBy;return gate;
        }
        static LevelData Base(int index,string name,string instruction,PlatformData[] supports,RouteLink[] links=null,int checkpoint=3)=>new LevelData
        {
            id=CampaignIdentity.Id(index),name=name,source="Campaign wave one authored prototype",width=390,height=2400,stride=2400,authoredRadius=GameConfig.Radius,
            discovery=true,sparseRewards=true,instruction=instruction,platforms=supports,connections=links??Chain(supports.Length),
            hazards=Array.Empty<PlatformData>(),blackHoles=Array.Empty<BlackHoleData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=true,
            checkpointPlatforms=new[]{0,checkpoint},chapterTitles=new[]{"Read the idea","Apply it, then recover"}
        };
        static RouteLink[] Chain(int n){var links=new RouteLink[n-1];for(int i=0;i<n-1;i++)links[i]=L(i,i+1);return links;}
        static PlatformData[] Crossing()=>new[]{S(0,195,2100,64),S(1,125,1800,66),S(2,265,1500,70),S(3,195,1200,74),S(4,115,900,66),S(5,275,600,70),S(6,195,300,76)};
        static LevelData Rain(bool combined)
        {
            var shapes=new List<PlatformData>{S(0,195,1450,64)};var links=new List<RouteLink>();var stars=new List<StarData>();
            int parts=combined?1:2;
            for(int part=0;part<parts;part++)
            {
                int entry=shapes.Count-1;double y0=1450-part*600;
                for(int row=0;row<2;row++)for(int col=0;col<5;col++)
                {
                    int i=shapes.Count;double x=29+col*104,y=y0-200-row*175-(x-195)*.1;
                    var p=JourneyExpansionBuilder.Capsule(i,x,y,74,40,-Math.PI/12);
                    p.label="wave-rain-"+part+"-"+row;p.motion=new MotionData{type="rain",amplitudeX=520,amplitudeY=52,periodSeconds=part==0?(row==0?26:18):(row==0?22:30),phase=(445-x)/520,speedVariation=.18};shapes.Add(p);
                    if(col==row+1)stars.Add(new StarData{id="wave-rain-dot-"+stars.Count,x=x,y=p.bounds.minY-10,parentPlatformId=p.id});
                }
                int exit=shapes.Count;shapes.Add(S(exit,195,y0-600,74));
                for(int col=0;col<5;col++){links.Add(L(entry,entry+1+col));links.Add(L(entry+6+col,exit));for(int next=0;next<5;next++)links.Add(L(entry+1+col,entry+6+next));}
            }
            if(combined)
            {
                AddBank(shapes,links,850,-50,false,false);
            }
            var level=Base(combined?43:33,combined?"A new current":"The divided stream",combined?"Cross the original rain, rest on the checkpoint, then follow the bank into a broad catcher.":"Read the slow and fast rows. Rest on the broad island before crossing the different current.",shapes.ToArray(),links.ToArray(),11);
            level.streamingRain=true;level.stars=stars.ToArray();level.height=level.stride=1800;level.chapterTitles=combined?new[]{"Cross the stream","A measured rebound"}:new[]{"Slow, then fast","Read the other current"};return level;
        }
        static void AddBank(List<PlatformData> shapes,List<RouteLink> links,double sourceY,double shift,bool mirror,bool moving)
        {
            int from=shapes.Count-1;Point P(double x,double y)=>new Point(mirror?390-x:x,y+shift);
            var points=new[]{P(375,300),P(375,740),P(350,740),P(310,300)};if(mirror)Array.Reverse(points);
            var bank=WorldPackBuilder.Polygon("wave-bank-"+shapes.Count,points,"wedge");if(moving)bank.motion=Move("horizontal",4,0,.25,14);shapes.Add(bank);
            int catcher=shapes.Count;shapes.Add(JourneyExpansionBuilder.Capsule(catcher,mirror?240:150,350+shift,240,100,0));
            int exit=shapes.Count;shapes.Add(S(exit,195,110+shift,76));links.Add(L(from,catcher));links.Add(L(catcher,exit));
        }
        internal static LevelData MakeLevel(int index)
        {
            if(index==33)return Rain(false);if(index==43)return Rain(true);
            var shapes=Crossing();LevelData level;
            switch(index)
            {
                case 34:
                    var banks=new List<PlatformData>{S(0,195,1900,62)};var bankLinks=new List<RouteLink>();AddBank(banks,bankLinks,1900,1000,false,false);AddBank(banks,bankLinks,1110,210,true,true);
                    level=Base(index,"The angled return","Read the first bank, then a slowly travelling plate. Let each rebound finish on its broad catcher.",banks.ToArray(),bankLinks.ToArray());break;
                case 35:
                    shapes=new[]{S(0,195,2050,64),S(1,195,1650,54),S(2,195,1650,54),S(3,195,1650,54),S(4,195,1350,74),S(5,195,1000,54),S(6,195,1000,54),S(7,195,650,76)};
                    for(int i=1;i<=3;i++)shapes[i].motion=Move("orbit",100,85,(i-1)/3.0,20);
                    shapes[5].motion=Move("orbit",90,70,0,20);shapes[6].motion=Move("orbit",90,70,.5,20);
                    level=Base(index,"Three travelling circles","Watch the shared orbit. Tap to transfer between circles; settle on the stable checkpoint before the next meeting.",shapes,null,4);break;
                case 36:
                    level=Base(index,"Alternating doors","Read one opening at a time. Broad supports give you room to watch the next gate.",shapes);
                    var a=Gate(0,195,1950,80);a.motion=Move("horizontal",100,0,.25,8);
                    var b=Gate(1,195,1650,70);b.motion=Move("horizontal",100,0,.75,10);
                    var c=Gate(2,195,1050,80);c.motion=Move("horizontal",100,0,.25,9);level.hazards=new[]{a,b,c};break;
                case 37:
                    shapes[1]=JourneyExpansionBuilder.Capsule(1,195,1800,140,92,0);shapes[1].motion=Move("horizontal",65,0,0,14);
                    shapes[4].motion=Move("vertical",0,45,0,12);
                    level=Base(index,"The patient ferry","Board the ferry and read its approach to solid ground. The next part carries you vertically.",shapes);break;
                case 38:
                    shapes[1].switchSupport=true;shapes[4].switchSupport=true;
                    level=Base(index,"A visible connection","Land on the square-marked support. Its thin link shows which striped gate will open.",shapes);
                    level.hazards=new[]{Gate(0,195,1650,180,shapes[1].id),Gate(1,195,750,180,shapes[4].id)};break;
                case 39:
                    shapes[1]=S(1,125,1800,74,7);shapes[4]=S(4,125,900,74,7);
                    shapes[1].rotation=new RotationData{enabled=true,continuous=true,periodSeconds=24};shapes[4].rotation=new RotationData{enabled=true,continuous=true,periodSeconds=30,phase=.25};
                    level=Base(index,"The turning oval","The oval turns beneath you. Read the changing surface direction, then tap toward the broad landing.",shapes);break;
                case 40:
                    shapes[1].motion=Move("vertical",0,30,0,12);shapes[2].motion=Move("vertical",0,30,.5,12);
                    shapes[4].motion=Move("vertical",0,40,.25,12);shapes[5].motion=Move("vertical",0,40,.75,12);
                    level=Base(index,"A travelling wave","Read the crests and troughs. The next landing follows the wave while your launch stays consistent.",shapes);break;
                case 41:
                    shapes=new[]{S(0,195,2100,64),S(1,100,1850,60),S(2,290,1850,54),S(3,195,1570,76),S(4,110,1300,66),S(5,275,1050,68),S(6,195,780,76)};shapes[2].label="bonus";
                    level=Base(index,"The open chamber","Choose a side of the striped divider. Both doorways rejoin at the broad checkpoint.",shapes,new[]{L(0,1),L(0,2),L(1,3),L(2,3),L(3,4),L(4,5),L(5,6)});
                    var divider=JourneyExpansionBuilder.Capsule(0,195,1830,220,14,Math.PI/2);divider.id="chamber-divider";divider.sourceShapeId="rounded-hazard";level.hazards=new[]{divider};break;
                case 42:
                    shapes=new[]{S(0,195,2100,64),S(1,110,1830,66),S(2,110,1560,70),S(3,195,1280,74),S(4,110,1010,66),S(5,230,740,70),S(6,195,450,76)};
                    level=Base(index,"Around the ring","Read the capture rings. The broad route passes outside them, with a stable checkpoint between the gardens.",shapes);
                    level.blackHoles=new[]{new BlackHoleData{id="wave-vortex-1",x=265,y=1660,radius=20,pullRadius=55},new BlackHoleData{id="wave-vortex-2",x=75,y=820,radius=20,pullRadius=55}};break;
                default:throw new ArgumentOutOfRangeException(nameof(index));
            }
            return level;
        }
        static AuthoredCourse Make(int index)
        {
            var level=MakeLevel(index);CampaignShapeBalance.Apply(level,index);string cacheKey=CampaignAuthorCache.Key(level);var cached=CampaignAuthorCache.Read(cacheKey);if(cached!=null){Debug.Log("MEKTRA_CAMPAIGN_CACHED "+(index+1));return cached;}var routes=new List<AuthoredRoute>();var rewards=new List<StarData>(level.stars);var bounces=new List<BounceRunData>();var chars=CharacterCatalog.Load();
            foreach(var link in level.connections)
            {
                if(level.platforms[link.from].motion?.type=="rain"||level.platforms[link.to].motion?.type=="rain")continue;
                bool bank=(index==34&&(link.from==0||link.from==3))||(index==43&&link.from==11);
                var r=CampaignWaveRegression.Opportunity(level,link.from,link.to,chars,bank);
                routes.Add(r);
                if(bank)
                {
                    var trace=CampaignWaveRegression.Trace(level,chars[0],link.from,r.distance,r.clock);bounces.Add(new BounceRunData{entry=link.from,exit=link.to,minimumBounces=1,launchDistance=r.distance,livePreview=link.from==3,points=trace.ToArray()});
                    if(link.from!=3)for(int n=1;n<=2;n++){var p=trace[Math.Min(trace.Count-1,trace.Count*n/3)];rewards.Add(new StarData{id="wave-bank-dot-"+rewards.Count,x=p.x,y=p.y});}
                    continue;
                }
                if(level.streamingRain||index==34)continue;
                bool bonus=level.platforms[link.to].label=="bonus";bool reward=bonus||link.from==0||link.from==3||link.from==4;
                if(reward)for(int n=0;n<(bonus?2:1);n++){double f=bonus?.4+n*.2:.5;var p=r.launch+(r.landing-r.launch)*f;rewards.Add(new StarData{id="wave-dot-"+rewards.Count,x=p.x,y=p.y,guideFrom=link.from,guideTo=link.to,guideFraction=f});}
            }
            level.stars=rewards.ToArray();level.bounceRuns=bounces.ToArray();Debug.Log("MEKTRA_WAVE_AUTHORED "+(index+1)+" "+level.name);
            var course=new AuthoredCourse{number=index+1,title=level.name,difficulty="New horizons",mood=level.instruction,points=rewards.Count,level=level,routes=routes.ToArray()};CampaignAuthorCache.Write(cacheKey,course);return course;
        }
        public static void Build()
        {
            const string frozen="../assets/levels/discovery/accepted-33.json";
            if(!File.Exists(frozen))File.Copy("../assets/levels/discovery/expanded-prototype.json",frozen);
            var accepted=JsonUtility.FromJson<LevelPack>(File.ReadAllText(frozen));if(accepted.levels.Length!=33)throw new Exception("Wave one requires the frozen 33-level baseline");
            var entries=new List<AuthoredCourse>(accepted.levels);var authorErrors=new List<string>();for(int i=33;i<99;i++){try{entries.Add(i<44?Make(i):CampaignFinaleBuilder.Make(i));}catch(Exception ex){string issue="Level "+(i+1)+": "+ex.Message;authorErrors.Add(issue);Debug.Log("MEKTRA_AUTHOR_REPAIR "+issue);}}if(authorErrors.Count>0)throw new Exception(string.Join("; ",authorErrors));
            var pack=JsonUtility.FromJson<LevelPack>(JsonUtility.ToJson(new LevelPack{levels=entries.ToArray()}));
            DiscoveryRegression.Run(pack);string json=JsonUtility.ToJson(pack,true);File.WriteAllText("../assets/levels/discovery/campaign-99.json",json);File.WriteAllText("Assets/Resources/discovery-pack.json",json);
            AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack,true);
        }
    }
}
