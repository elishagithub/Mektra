using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Mektra.Editor
{
    // Authored rooms use existing controls. Each later wave changes routes or the
    // relation between moving surfaces, rather than scaling a finished level.
    public static class CampaignFinaleBuilder
    {
        static readonly string[] Titles={
            "Rain between islands","Bank and ferry","The stretched orbit","The second opening","Two elevators","Choose the door","The turning lobe","The crossing loop","The side doorway","A moving safe route","Meet, then commit",
            "The reward current","The extra rebound","Outer orbit, inner reward","Wide door, quick door","The ferry shortcut","Open your route","Choose the shoulder","Across the wave","Two ways through","The outside reward","Your way across",
            "Rain to bank","A bank in motion","Carousel and doorway","Wait, cross, recover","The passing meeting","The linked passage","A rocking plate","Wave and window","A connected pair","Orbit beside a vortex","A composed passage",
            "The rain archipelago","The rebound gallery","The carousel passage","The breathing corridor","A journey by ferry","The connected machine","The turning garden","The wave passage","Through the chambers","The vortex garden","The long crossing",
            "Read the rain","The quiet ricochet","The final rendezvous","Doors in sequence","The last ferry","The complete machine","The changing horizon","The final wave","The open labyrinth","Beyond the vortices","Mektra: the full journey"};
        static readonly string[] Instructions={"Read the thin slanted rain. Board a passing capsule, then rest on each broad island.","Let the bank reflect you into a broad catcher. The dotted route shows the measured rebound.","Watch the orbital meeting. Transfer when the next circle approaches your departure direction.","Wait on broad supports and read each rounded gate before committing.","Board a moving support, then read its approach to the next landing. A small marked branch offers a shortcut.","Land on a square-marked support to open its linked gate. Choose the visible route you want.","Read the rotating curved surface. Your launch follows its changing outward direction.","Watch the wave and the crossing loop. Depart toward the broad next landing.","Read the room's doorway. Smaller optional branches reconnect with the broad route.","Stay outside the marked capture rings and lead any moving landing.","Use what you have learned across these checkpointed rooms. Each broad island starts a fresh part."};
        static PlatformData S(int i,double x,double y,double r,int family=0)=>EndlessPackBuilder.Shape(family,i,x,y,r,0);
        static RouteLink L(int a,int b)=>new RouteLink{from=a,to=b};
        static MotionData M(string type,double x,double y,double phase,double period)=>new MotionData{type=type,amplitudeX=x,amplitudeY=y,phase=phase,periodSeconds=period};
        static T Copy<T>(T value)=>JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        static double X(PlatformData p)=>(p.bounds.minX+p.bounds.maxX)*.5;
        static double Y(PlatformData p)=>(p.bounds.minY+p.bounds.maxY)*.5;
        static PlatformData Shift(PlatformData p,double y,string id)
        {
            var moved=WorldPackBuilder.Polygon(id,p.outline.Select(v=>new Point(v.x,v.y+y)).ToArray(),p.sourceShapeId);
            moved.motion=Copy(p.motion??new MotionData());moved.rotation=Copy(p.rotation??new RotationData());moved.label=p.label;moved.switchSupport=p.switchSupport;moved.openedBy=p.openedBy;moved.crumbleSeconds=p.crumbleSeconds;moved.orbitDirection=p.orbitDirection;return moved;
        }
        static PlatformData Gate(int i,double x,double y,double width,string opened=null)
        {
            var g=JourneyExpansionBuilder.Capsule(i,x,y,width,12,0);g.id="door-"+i;g.sourceShapeId="rounded-hazard";g.openedBy=opened;return g;
        }
        static LevelData Simple(PlatformData[] p,RouteLink[] links=null)=>new LevelData{width=390,height=1800,stride=1800,platforms=p,connections=links??Enumerable.Range(0,p.Length-1).Select(i=>L(i,i+1)).ToArray(),hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),blackHoles=Array.Empty<BlackHoleData>(),bounceRuns=Array.Empty<BounceRunData>()};
        static LevelData FirstRoom(int family)
        {
            var source=Copy(CampaignWaveBuilder.MakeLevel(33+family));int last=source.checkpointPlatforms[1];
            source.platforms=source.platforms.Take(last+1).ToArray();source.connections=source.connections.Where(l=>l.from<=last&&l.to<=last).ToArray();
            double bottom=Y(source.platforms[0]),top=Y(source.platforms[last]);
            source.hazards=source.hazards.Where(h=>Y(h)<bottom&&Y(h)>top).ToArray();source.blackHoles=source.blackHoles.Where(h=>h.y<bottom&&h.y>top).ToArray();source.stars=Array.Empty<StarData>();source.bounceRuns=Array.Empty<BounceRunData>();return source;
        }
        static LevelData Room(int family,int wave,int part)
        {
            var room=FirstRoom(family);
            if(family==0&&wave==5){var approved=Copy(JsonUtility.FromJson<LevelPack>(System.IO.File.ReadAllText("../assets/levels/discovery/accepted-33.json")).levels[30].level);approved.platforms=approved.platforms.Take(17).ToArray();approved.connections=approved.connections.Where(l=>l.from<=16&&l.to<=16).ToArray();approved.stars=Array.Empty<StarData>();approved.bounceRuns=Array.Empty<BounceRunData>();room=approved;}
            double phase=(part%4)*.25,period=20+part*2+wave*2;
            if(family==0)
            {
                // Preserve the original thin, slanted, off-screen recycling rain.
                foreach(var p in room.platforms)if(p.motion?.type=="rain")
                {
                    bool second=p.label.EndsWith("-1");p.motion.periodSeconds=wave==5?p.motion.periodSeconds+part*2:(second?18:26)+part*2+(wave%2)*2;
                    
                }
                if(wave>=2){var old=room.platforms[3];var smaller=JourneyExpansionBuilder.Capsule(3,X(old),Y(old),74,34,-Math.PI/12);smaller.id=old.id;smaller.motion=old.motion;smaller.label=old.label;room.platforms[3]=smaller;}
                return room;
            }
            if(family==1)
            {
                if(wave>=2&&part%2==1)
                {
                    // The reviewed double bank is a different physical sequence.
                    var accepted=JsonUtility.FromJson<LevelPack>(System.IO.File.ReadAllText("../assets/levels/discovery/accepted-33.json")).levels[31].level;
                    room=Simple(accepted.platforms.Skip(6).Take(5).Select(Copy).ToArray(),new[]{L(0,3),L(3,4)});
                    room.bounceRuns=new[]{new BounceRunData{entry=0,exit=3,minimumBounces=2}};
                }
                else room.bounceRuns=new[]{new BounceRunData{entry=0,exit=2,minimumBounces=1}};
                if(wave==1){room.platforms[2].motion=M("horizontal",5+part,0,phase,period);}
                if(wave>=3&&part%2==0)room.platforms[1].motion=M("horizontal",3+part,0,phase,period);
                if(wave==5&&part%2==0){int catcher=room.bounceRuns[0].exit;int exit=room.platforms.Length-1;var bonus=S(room.platforms.Length,300,Y(room.platforms[catcher])-110,45);bonus.label="bonus";int branch=room.platforms.Length;room.platforms=room.platforms.Concat(new[]{bonus}).ToArray();room.connections=room.connections.Concat(new[]{L(catcher,branch),L(branch,exit)}).ToArray();}
                return room;
            }
            if(family==2)
            {
                if(wave==1)
                {
                    var pair=new[]{S(0,195,1500,74),S(1,195,1150,64),S(2,195,1150,64),S(3,195,800,80)};
                    pair[1].motion=M("orbit",80,110,phase,28+part*2);pair[2].motion=M("orbit",80,110,phase+.5,28+part*2);room=Simple(pair);
                }
                else
                {
                    var p=new[]{S(0,195,1500,74),S(1,125,1150,64),S(2,300,1150,50),S(3,195,800,76)};p[2].label="bonus";
                    p[1].motion=M("orbit",25,45,phase,period);p[2].motion=M("orbit",12,28,phase+.5,period);
                    room=Simple(p,new[]{L(0,1),L(0,2),L(1,3),L(2,3)});
                    if(wave>=4&&part%2==1)p[1].motion=M("orbit",30,65,phase,period);
                }
                return room;
            }
            var supports=new[]{S(0,195,1500,74),S(1,120,1200,66),S(2,270,900,70),S(3,195,600,76)};room=Simple(supports);
            switch(family)
            {
                case 3:
                    var door=Gate(0,195,1350,64+part*4);door.motion=M("horizontal",100,0,phase+.25,9+part+wave);
                    var other=Gate(1,195,750,58+part*3);other.motion=M("horizontal",100,0,phase+.75,12+part+wave);room.hazards=wave==5?new[]{door,other,Gate(2,195,1050,52)}:new[]{door,other};if(wave==5)room.hazards[2].motion=M("horizontal",100,0,phase+.5,14+part);
                    if(wave==3&&part%2==1)supports[2].crumbleSeconds=5;
                    if(wave==2){supports[2].label="bonus";var fallback=S(4,110,900,64);room.platforms=new[]{supports[0],supports[1],fallback,supports[3],supports[2]};room.platforms[4].label="bonus";room.connections=new[]{L(0,1),L(1,2),L(1,4),L(2,3),L(4,3)};}
                    break;
                case 4:
                    supports[1]=JourneyExpansionBuilder.Capsule(1,195,1200,132,92,0);supports[1].motion=M(wave==1?"vertical":"horizontal",wave==1?0:55,wave==1?42:0,phase,period);
                    supports[2]=S(2,240,900,66);supports[2].motion=M(wave==3?"orbit":"vertical",wave==3?35:0,38,phase+.5,period);
                    room.platforms=supports;
                    if(wave==2||wave==5){supports[2]=S(2,90,1000,62);var bonus=S(4,310,870,48);bonus.motion=M("vertical",0,25,phase,period);bonus.label="bonus";var fallback=S(5,100,780,54);room.platforms=supports.Concat(new[]{bonus,fallback}).ToArray();room.connections=new[]{L(0,1),L(1,2),L(1,4),L(4,3),L(2,5),L(5,3)};}
                    break;
                case 5:
                    if(wave==1||wave==2||wave==4&&part%2==1||wave==5&&part%2==0)
                    {
                        supports=new[]{S(0,195,1500,74),S(1,100,1200,60),S(2,290,1200,54),S(3,195,850,76)};
                        supports[1].switchSupport=supports[2].switchSupport=true;supports[2].label="bonus";if(wave==5){supports[1].motion=M("vertical",0,15,phase,period);supports[2].motion=M("vertical",0,15,phase+.5,period);}
                        room=Simple(supports,new[]{L(0,1),L(0,2),L(1,3),L(2,3)});room.hazards=new[]{Gate(0,95,1040,80,supports[1].id),Gate(1,295,1040,80,supports[2].id)};
                    }
                    else
                    {
                        supports[1].switchSupport=true;supports[2].switchSupport=true;room.hazards=new[]{Gate(0,195,1050,160,supports[1].id),Gate(1,195,750,160,supports[2].id)};
                    }
                    break;
                case 6:
                    supports[1]=S(1,120,1200,76,wave%2==1?4:7);supports[1].rotation=new RotationData{enabled=true,continuous=wave!=3,amplitudeRadians=.35,periodSeconds=period+8,phase=phase};
                    supports[2]=S(2,265,900,70,wave>=4?4:0);if(wave>=4)supports[2].rotation=new RotationData{enabled=true,continuous=true,periodSeconds=period+12,phase=phase+.5};
                    room.platforms=supports;
                    if(wave==2){var small=S(4,300,1180,42,4);small.rotation=new RotationData{enabled=true,continuous=true,periodSeconds=32};small.label="bonus";room.platforms=supports.Concat(new[]{small}).ToArray();room.connections=new[]{L(0,1),L(0,4),L(1,2),L(4,2),L(2,3)};}
                    break;
                case 7:
                    supports[1]=S(1,195,1200,62);supports[1].motion=M("figure-eight",55,35,phase,period);
                    supports[2].motion=M(wave==5&&part%2==1?"figure-eight":"vertical",wave==5&&part%2==1?24:0,30,phase+.5,period);room.platforms=supports;
                    if(wave==3){var opening=Gate(0,195,750,58);opening.motion=M("horizontal",100,0,.25+phase,12);room.hazards=new[]{opening};}
                    if(wave==2){var small=S(4,90,1050,42);small.label="bonus";small.motion=M("vertical",0,25,phase+.25,period);room.platforms=supports.Concat(new[]{small}).ToArray();room.connections=new[]{L(0,1),L(1,2),L(1,4),L(4,3),L(2,3)};}
                    break;
                case 8:
                    if(wave==1||wave>=3&&part%2==1)
                    {
                        supports=new[]{S(0,195,1500,74),S(1,95,1220,60),S(2,290,1220,60),S(3,195,920,76)};room=Simple(supports);
                        var wall=JourneyExpansionBuilder.Capsule(0,195,1325,100,12,Math.PI/2);wall.sourceShapeId="rounded-hazard";room.hazards=new[]{wall};
                    }
                    else
                    {
                        supports=new[]{S(0,195,1500,74),S(1,100,1220,60),S(2,290,1220,50),S(3,195,900,76),S(4,290,1040,46)};supports[2].label=supports[4].label="bonus";
                        room=Simple(supports,new[]{L(0,1),L(0,2),L(1,3),L(2,4),L(4,3)});
                        var wall=JourneyExpansionBuilder.Capsule(0,195,1200,170,12,Math.PI/2);wall.sourceShapeId="rounded-hazard";room.hazards=new[]{wall};
                    }
                    if(wave==5)room.connections=room.connections.Concat(new[]{room.platforms.Length>4?L(2,3):L(0,2)}).ToArray();
                    break;
                case 9:
                    supports=new[]{S(0,195,1500,74),S(1,110,1220,64),S(2,130,940,66),S(3,195,650,76)};room=Simple(supports);
                    room.blackHoles=new[]{new BlackHoleData{id="garden-ring",x=285,y=1080,radius=20,pullRadius=52}};
                    if(wave==1)supports[2].motion=M("horizontal",24,0,phase,period);
                    if(wave>=3)supports[1].motion=M("orbit",20,25,phase,period);
                    if(wave==2||wave==5){var bonus=S(4,270,790,44);bonus.label="bonus";room.platforms=supports.Concat(new[]{bonus}).ToArray();room.connections=new[]{L(0,1),L(1,2),L(2,3),L(2,4),L(4,3)};}
                    break;
            }
            return room;
        }
        public static AuthoredCourse Make(int index)
        {
            int offset=index-44,wave=1+offset/11,family=offset%11,parts=wave==1?2:wave==2?3:4;
            var platforms=new List<PlatformData>();var hazards=new List<PlatformData>();var holes=new List<BlackHoleData>();var links=new List<RouteLink>();var checkpoints=new List<int>();var runs=new List<BounceRunData>();var chapters=new List<string>();
            double previousY=0;int previous=-1;
            for(int part=0;part<parts;part++)
            {
                int kind=family;
                if(family==10){int[][] composition={new[]{2,3},new[]{4,8,2},new[]{5,6,1,8},new[]{0,4,7,1},new[]{0,5,2,1}};kind=composition[wave-1][part];}
                if(family==0&&wave==3&&part==parts-1)kind=1;
                if(family==2&&wave==3&&part==parts-1)kind=3;
                if(family==9&&wave==3&&part%2==1)kind=2;
                var room=Room(kind,wave,part);if(family==6&&wave==3&&part==2){room=Room(1,wave,part);kind=1;room.platforms[1].rotation=new RotationData{enabled=true,amplitudeRadians=.025,periodSeconds=24};}int exit=room.checkpointPlatforms!=null&&room.checkpointPlatforms.Length>1?room.checkpointPlatforms[1]:room.platforms.Length-1;
                // Optional branches are stored after the real exit in some rooms.
                if(kind!=0&&kind!=1&&kind!=2)exit=3;
                double shift=part==0?0:previousY-Y(room.platforms[0]);var map=new int[room.platforms.Length];var idMap=new Dictionary<string,string>();
                for(int i=0;i<room.platforms.Length;i++){map[i]=i==0&&part>0?previous:platforms.Count;if(!(i==0&&part>0)){string id="l"+(index+1)+"-p"+platforms.Count;idMap[room.platforms[i].id]=id;var p=Shift(room.platforms[i],shift,id);if(p.motion?.type=="rain")p.label="room-"+part+"-"+p.label;platforms.Add(p);}else idMap[room.platforms[i].id]=platforms[previous].id;}
                // Each room starts on a stable checkpoint, including branch rooms.
                checkpoints.Add(map[0]);chapters.Add(new[]{"Rain current","Rebound passage","Orbital meeting","Gate rhythm","Moving rendezvous","Linked mechanism","Turning surface","Wave crossing","Chamber route","Vortex garden"}[kind]);
                foreach(var link in room.connections)links.Add(L(map[link.from],map[link.to]));
                foreach(var h in room.hazards){var moved=Shift(h,shift,"l"+(index+1)+"-gate"+hazards.Count);if(!string.IsNullOrEmpty(h.openedBy))moved.openedBy=idMap[h.openedBy];hazards.Add(moved);}
                foreach(var h in room.blackHoles){var moved=Copy(h);moved.y+=shift;moved.id="l"+(index+1)+"-ring"+holes.Count;holes.Add(moved);}
                foreach(var run in room.bounceRuns??Array.Empty<BounceRunData>())runs.Add(new BounceRunData{entry=map[run.entry],exit=map[run.exit],minimumBounces=run.minimumBounces,livePreview=true});
                previous=map[exit];previousY=Y(platforms[previous]);
            }
            // Completion is defined by the final platform; optional branches must
            // never become the completion sentinel merely due to storage order.
            if(previous!=platforms.Count-1)
            {
                int last=platforms.Count-1;var swap=platforms[previous];platforms[previous]=platforms[last];platforms[last]=swap;
                int Swap(int i)=>i==previous?last:i==last?previous:i;
                foreach(var l in links){l.from=Swap(l.from);l.to=Swap(l.to);}for(int i=0;i<checkpoints.Count;i++)checkpoints[i]=Swap(checkpoints[i]);foreach(var b in runs){b.entry=Swap(b.entry);b.exit=Swap(b.exit);}previous=last;
            }
            double finalShift=300-Y(platforms[previous]);for(int i=0;i<platforms.Count;i++)platforms[i]=Shift(platforms[i],finalShift,platforms[i].id);for(int i=0;i<hazards.Count;i++)hazards[i]=Shift(hazards[i],finalShift,hazards[i].id);foreach(var h in holes)h.y+=finalShift;
            var level=new LevelData{id=CampaignIdentity.Id(index),name=Titles[offset],source="99-level authored campaign",width=390,height=Y(platforms[0])+300,stride=Y(platforms[0])+300,authoredRadius=GameConfig.Radius,discovery=true,sparseRewards=true,suppressMagnets=true,instruction=Instructions[family],platforms=platforms.ToArray(),connections=links.ToArray(),hazards=hazards.ToArray(),blackHoles=holes.ToArray(),checkpointPlatforms=checkpoints.ToArray(),chapterTitles=chapters.ToArray(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),bounceRuns=runs.ToArray(),streamingRain=platforms.Any(p=>p.motion?.type=="rain")};
            CampaignShapeBalance.Apply(level,index);string cacheKey=CampaignAuthorCache.Key(level);var cached=CampaignAuthorCache.Read(cacheKey);if(cached!=null){Debug.Log("MEKTRA_CAMPAIGN_CACHED "+(index+1));return cached;}
            var chars=CharacterCatalog.Load();var routes=new List<AuthoredRoute>();var rewards=new List<StarData>();
            foreach(var link in level.connections)
            {
                if(level.platforms[link.from].motion?.type=="rain"||level.platforms[link.to].motion?.type=="rain")continue;
                var bank=Array.Find(level.bounceRuns,b=>b.entry==link.from&&b.exit==link.to);var route=CampaignWaveRegression.Opportunity(level,link.from,link.to,chars,bank!=null);routes.Add(route);
                if(bank!=null){bank.launchDistance=route.distance;bank.points=CampaignWaveRegression.Trace(level,chars[0],link.from,route.distance,route.clock).ToArray();}
                bool bonus=level.platforms[link.to].label=="bonus";
                if((checkpoints.Contains(link.from)||bonus)&&bank==null){rewards.Add(new StarData{id="campaign-dot-"+rewards.Count,x=(route.launch.x+route.landing.x)*.5,y=(route.launch.y+route.landing.y)*.5,guideFrom=link.from,guideTo=link.to,guideFraction=.5});}
            }
            foreach(var p in level.platforms)if(p.motion?.type=="rain"&&p.id.EndsWith("p3"))rewards.Add(new StarData{id="rain-dot-"+rewards.Count,x=X(p),y=p.bounds.minY-10,parentPlatformId=p.id});
            level.stars=rewards.ToArray();Debug.Log("MEKTRA_CAMPAIGN_AUTHORED "+(index+1)+" "+level.name+" / "+parts+" parts / "+platforms.Count+" supports");
            var course=new AuthoredCourse{number=index+1,title=level.name,difficulty=wave<3?"Explore":"Deepen",mood=level.instruction,points=level.stars.Length,level=level,routes=routes.ToArray()};CampaignAuthorCache.Write(cacheKey,course);return course;
        }
    }
}
