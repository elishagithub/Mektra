using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    // Later chapters use a different spatial grammar; the opening library remains intact.
    public static class ExperiencePackBuilder
    {
        static readonly string[] Names={"Spike canyon","Honeycomb passage","Rain of shapes","Black-hole crossing","Wedge springs","Lobed crossroads","Long leaps"};
        static readonly string[] Rules={"Thread the curved passage. Alternate sides to avoid the spikes.","Read the hexagon faces. Tight gaps reward a precise launch.","Land on the rain. Jump again before it carries you off the left edge.","Stay outside the purple pull ring. The vortex pulls you in and bursts.","Rebound from the wedge faces. The upper landing catches your exit.","Follow the lobes across horizontal and diagonal crossings.","Take the long leap, collect the trail, then rest on the wide landing."};
        static PlatformData Polygon(string id,Point[] p,string family)=>WorldPackBuilder.Polygon(id,p,family);
        static PlatformData Oval(string id,Point center,double radius,double tilt)
        {
            var points=new Point[48];for(int i=0;i<points.Length;i++){double a=i*Math.PI*2/points.Length,x=Math.Cos(a)*radius,y=Math.Sin(a)*radius*.38;points[i]=center+new Point(x*Math.Cos(tilt)-y*Math.Sin(tilt),x*Math.Sin(tilt)+y*Math.Cos(tilt));}
            return Polygon(id,points,"drifting-oval");
        }
        static LevelData Base(int kind,int variant,PlatformData[] platforms,RouteLink[] links)=>new LevelData{id="mektra-endless-"+kind+"-"+variant,name=Names[kind-7],worldName=Names[kind-7],worldNumber=kind-6,source="Mektra later-world library",authoredRadius=GameConfig.Radius,width=390,height=1080,stride=1080,segmentKind=kind,variant=variant,instruction=Rules[kind-7],platforms=platforms,connections=links,hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=kind==11};
        static AuthoredRoute Route(CourseReview.JumpReview r)=>new AuthoredRoute{from=r.from,to=r.to,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing};
        static AuthoredCourse Finish(LevelData level,int number,List<AuthoredRoute> routes,List<StarData> stars)
        {
            level.stars=stars.ToArray();MotionRegression.GeometryChecks(level);
            Debug.Log("MEKTRA_EXPERIENCE_AUTHORED "+level.id);
            return new AuthoredCourse{number=number,title=level.name,difficulty="Later world",mood=level.instruction,points=stars.Count,level=level,routes=routes.ToArray()};
        }
        static PlatformData Ramp(string id,Point[] corners,bool mirror)
        {
            // Dense samples retain correct face normals for the orbit/landing geometry.
            var points=new Point[corners.Length*48];for(int edge=0;edge<corners.Length;edge++)for(int sample=0;sample<48;sample++)
            {var p=corners[edge]+(corners[(edge+1)%corners.Length]-corners[edge])*(sample/48.0);points[edge*48+sample]=mirror?new Point(390-p.x,p.y):p;}
            if(mirror)Array.Reverse(points);var result=Polygon(id,points,"wedge");return result;
        }
        static Point Face(Point a,Point b,double y)
        {
            var p=a+(b-a)*((y-a.y)/(b.y-a.y));var normal=new Point(b.y-a.y,a.x-b.x).Normal;
            return p+normal*(GameConfig.Radius+1.5);
        }
        static AuthoredCourse Wedges(int variant,int number,Character[] chars)
        {
            Point[][] leftBanks={
                new[]{new Point(0,300),new Point(45,350),new Point(145,850),new Point(0,850)},
                new[]{new Point(0,300),new Point(72,350),new Point(104,580),new Point(150,850),new Point(0,850)},
                new[]{new Point(0,300),new Point(55,350),new Point(135,610),new Point(115,850),new Point(0,850)},
                new[]{new Point(0,300),new Point(40,350),new Point(110,520),new Point(80,670),new Point(148,850),new Point(0,850)}};
            Point[][] rightBanks={
                new[]{new Point(390,300),new Point(390,850),new Point(245,850),new Point(345,350)},
                new[]{new Point(390,300),new Point(390,850),new Point(255,850),new Point(305,580),new Point(340,350)},
                new[]{new Point(390,300),new Point(390,850),new Point(245,850),new Point(268,610),new Point(325,350)},
                new[]{new Point(390,300),new Point(390,850),new Point(255,850),new Point(295,670),new Point(260,520),new Point(335,350)}};
            // The bank silhouette and receiving surface both change with the rebound rhythm.
            var catcher=variant==0?JourneyExpansionBuilder.Capsule(3,195,237,320,26,0):variant==1?JourneyExpansionBuilder.Capsule(3,205,237,280,36,0):variant==2?JourneyExpansionBuilder.Capsule(3,180,237,250,44,0):JourneyExpansionBuilder.Capsule(3,195,237,210,48,0);
            var platforms=new[]{EndlessPackBuilder.Shape(0,0,195,960,48,0),Ramp("platform-2",leftBanks[variant],false),Ramp("platform-3",rightBanks[variant],false),catcher,EndlessPackBuilder.Shape(0,4,195,120,48,0)};
            var level=Base(11,variant,platforms,new[]{new RouteLink{from=0,to=3},new RouteLink{from=1,to=3},new RouteLink{from=2,to=3},new RouteLink{from=3,to=4}});
            var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
            var orbit=new Orbit(platforms[0].outline);double selected=-1;Simulation successful=null;
            for(int i=0;i<360&&selected<0;i++)
            {
                double d=i*orbit.length/360;bool valid=true;
                foreach(var character in chars)
                {
                    var sim=new Simulation(level,character);sim.distance=d;sim.Select(character);sim.SetViewport(816);sim.Start();sim.Jump();
                    for(int step=0;step<480&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);
                    if(sim.phase!=Phase.Playing||sim.flying||sim.platform.index!=3||sim.wedgeBounces==0){valid=false;break;}successful=sim;
                }
                if(valid)selected=d;
            }
            if(selected<0)throw new Exception("No physical wedge rebound reaches catcher / "+variant);
            var start=new Simulation(level,chars[0]);start.distance=selected;start.Select(chars[0]);
            routes.Add(new AuthoredRoute{from=0,to=3,distance=selected,launch=start.position,landing=successful.position});
            var jump=CourseReview.Review(level,3,chars,4);routes.Add(Route(jump));
            // Rewards follow a measured physical trajectory; they never steer the player.
            var trace=new Simulation(level,chars[0]);trace.distance=selected;trace.Select(chars[0]);trace.SetViewport(816);trace.Start();trace.Jump();double next=.32;
            while(trace.flying&&trace.phase==Phase.Playing&&trace.time<4)
            {trace.Advance(GameConfig.Step);if(trace.time>=next){stars.Add(new StarData{id="star-"+(stars.Count+1),x=trace.position.x,y=trace.position.y});next+=.3;}}
            return Finish(level,number,routes,stars);
        }
        static AuthoredCourse Rain(int variant,int number,Character[] chars)
        {
            // Equally spaced capsules form diagonal rows; each row has its own flow speed.
            const int rows=4,columns=4;var platforms=new PlatformData[rows*columns+2];var links=new List<RouteLink>();
            platforms[0]=EndlessPackBuilder.Shape(0,0,195,960,48,0);platforms[platforms.Length-1]=EndlessPackBuilder.Shape(0,platforms.Length-1,195,120,48,0);
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
            {
                int index=1+row*columns+col;double x=26+col*112,y=new double[][]{new double[]{790,615,440,265},new double[]{805,625,455,270},new double[]{795,615,435,250},new double[]{805,635,445,265}}[variant][row]-(x-195)*.10;
                double angle=-Math.PI/new double[]{12,10,15,11}[variant];
                // Radius equals half the thickness: fully rounded semicircular ends.
                var points=new Point[96];const double radius=20,halfStraight=17;
                for(int j=0;j<points.Length;j++)
                {
                    bool right=j<48;double a=(right?-Math.PI/2:Math.PI/2)+(j%48)*Math.PI/47;
                    double dx=(right?halfStraight:-halfStraight)+radius*Math.Cos(a),dy=radius*Math.Sin(a);
                    points[j]=new Point(x+dx*Math.Cos(angle)-dy*Math.Sin(angle),y+dx*Math.Sin(angle)+dy*Math.Cos(angle));
                }
                platforms[index]=Polygon("platform-"+(index+1),points,"rain-capsule");
            }
            for(int col=0;col<columns;col++)links.Add(new RouteLink{from=0,to=1+col});
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
            {int from=1+row*columns+col;if(row==rows-1)links.Add(new RouteLink{from=from,to=platforms.Length-1});else for(int next=0;next<columns;next++)links.Add(new RouteLink{from=from,to=from-col+columns+next});}
            var level=Base(9,variant,platforms,links.ToArray());var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
            foreach(var link in links)
            {
                var review=CourseReview.Review(level,link.from,chars,link.to);routes.Add(Route(review));

            }
            for(int i=1;i<platforms.Length-1;i++)
            {
                var bounds=platforms[i].bounds;double x=(bounds.minX+bounds.maxX)*.5;
                platforms[i].motion=new MotionData{type="rain",amplitudeX=520,amplitudeY=52,periodSeconds=new double[]{24,20,27,22}[((i-1)/columns+variant)%rows]+variant*.5,phase=(445-x)/520,speedVariation=.18};
            }
            for(int row=0;row<rows;row++)
            {
                int i=1+row*columns+(row%2==0?1:2);var b=platforms[i].bounds;
                stars.Add(new StarData{id="rain-star-"+row,x=(b.minX+b.maxX)/2,y=b.minY-10,parentPlatformId=platforms[i].id});
            }
            return Finish(level,number,routes,stars);
        }
        public static AuthoredCourse Make(int kind,int variant,int number,Character[] chars)
        {
            if(kind==9)return Rain(variant,number,chars);
            if(kind==11)return Wedges(variant,number,chars);
            int experience=kind-7;bool mirror=variant>=2;
            Point[] centers=kind==12?new[]{new Point(195,960),new Point(88,735),new Point(287,735),new Point(205,465),new Point(92,275),new Point(195,120)}:
                kind==13?new[]{new Point(195,960),new Point(115,535),new Point(275,535),new Point(195,120)}:
                kind==8?new[]{new Point(195,960),new Point(104,755),new Point(266,570),new Point(114,365),new Point(195,120)}:
                new[]{new Point(195,960),new Point(118,750),new Point(259,540),new Point(125,320),new Point(195,120)};
            if(kind!=12&&kind!=13)centers=JourneyExpansionBuilder.RoutePositions(variant,centers.Length,false,kind);
            else if(kind==12)
            {
                Point[][] crossroads={centers,new[]{new Point(195,960),new Point(112,785),new Point(278,620),new Point(220,425),new Point(105,280),new Point(195,120)},new[]{new Point(195,960),new Point(260,780),new Point(105,575),new Point(250,575),new Point(150,290),new Point(195,120)},new[]{new Point(195,960),new Point(100,780),new Point(255,650),new Point(125,445),new Point(280,275),new Point(195,120)}};centers=crossroads[variant];
            }
            else
            {
                centers[1]=new Point(new double[]{115,95,275,120}[variant],new double[]{535,650,600,570}[variant]);centers[2]=new Point(new double[]{275,245,125,270}[variant],new double[]{535,400,430,390}[variant]);
            }
            var platforms=new PlatformData[centers.Length];var links=new RouteLink[centers.Length-1];
            for(int i=0;i<centers.Length;i++)
            {
                bool seam=i==0||i==centers.Length-1;int family=seam?0:kind==8?2:kind==12?4+(variant+i)%3:kind==13?1:0;
                double angle=i==centers.Length-1?-Math.PI/2:Math.Atan2(centers[i+1].y-centers[i].y,centers[i+1].x-centers[i].x);
                platforms[i]=EndlessPackBuilder.Shape(family,i,centers[i].x,centers[i].y,seam?48:kind==8?43:kind==13?47:36,angle);
                if(i<links.Length)links[i]=new RouteLink{from=i,to=i+1};
            }
            var level=new LevelData{id="mektra-endless-"+kind+"-"+variant,name=Names[experience],worldName=Names[experience],worldNumber=experience+1,source="Mektra later-world library",authoredRadius=GameConfig.Radius,width=390,height=1080,stride=1080,segmentKind=kind,variant=variant,instruction=Rules[experience],connections=links,platforms=platforms,hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=kind==10||kind==11};
            var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
            foreach(var link in links)
            {
                var review=CourseReview.Review(level,link.from,chars,link.to);
                routes.Add(new AuthoredRoute{from=link.from,to=link.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                int count=kind==13?3:1;
                for(int i=0;i<count;i++){var p=review.launch+(review.landing-review.launch)*(count==1?.48:.25+i*.24);stars.Add(new StarData{id="star-"+(stars.Count+1),x=p.x,y=p.y});}
            }
            var hazards=new List<PlatformData>();
            if(kind==7)
            {
                // Sample a winding corridor around the authored route, widened at the rest pockets.
                foreach(int side in new[]{-1,1})
                {
                    var inner=new List<Point>();for(int j=0;j<=168;j++)
                    {
                        double y=120+j*5;int index=0;while(index+1<centers.Length-1&&y<centers[index+1].y)index++;
                        var a=centers[index];var b=centers[index+1];double t=Math.Clamp((a.y-y)/(a.y-b.y),0,1);
                        double x=a.x+(b.x-a.x)*t+Math.Sin(t*Math.PI*2)*7;
                        double width=62+24*Math.Pow(Math.Abs(2*t-1),3);
                        // A small pointed intrusion between pockets forces a deliberate side change.
                        double spike=Math.Max(0,1-Math.Abs(t-.5)/.045)*8*(index%2==0?side==1?1:0:side==-1?1:0);
                        double edge=x+side*(width-spike);
                        foreach(var support in platforms)
                        {
                            var bounds=support.bounds;
                            if(y<bounds.minY-18||y>bounds.maxY+18)continue;
                            edge=side<0?Math.Min(edge,bounds.minX-20):Math.Max(edge,bounds.maxX+20);
                        }
                        inner.Add(new Point(Math.Clamp(edge,4,386),y));
                    }
                    var outline=new List<Point>(inner);outline.Add(new Point(side<0?0:390,960));outline.Add(new Point(side<0?0:390,120));
                    hazards.Add(Polygon("hazard-curved-wall-"+side,outline.ToArray(),"spike-wall"));
                }
            }
            if(kind==8)
            {
                // Dense side hexagons leave a narrow, slanting central lane.
                foreach(var route in routes)
                {
                    var axis=(route.landing-route.launch).Normal;var side=new Point(-axis.y,axis.x);var center=(route.launch+route.landing)*.5;
                    foreach(int sign in new[]{-1,1})
                    {
                        var c=center+side*(sign*64);if(c.x<29||c.x>361)continue;
                        var hex=EndlessPackBuilder.Shape(2,0,c.x,c.y,25,Math.Atan2(axis.y,axis.x));hex.id="hazard-honeycomb-"+hazards.Count;hazards.Add(hex);
                    }
                }
            }
            if(kind==10)
            {
                // Place the danger beside, rather than directly on, the viable jump ray.
                var r=routes[1];var axis=(r.landing-r.launch).Normal;var side=new Point(-axis.y,axis.x);var mid=(r.launch+r.landing)*.5;
                var c=mid+side*((mid.x<195?1:-1)*98);c.x=Math.Clamp(c.x,62,328);
                level.blackHoles=new[]{new BlackHoleData{id="black-hole-1",x=c.x,y=c.y,radius=24,pullRadius=51}};
            }
            level.stars=stars.ToArray();level.hazards=hazards.ToArray();MotionRegression.GeometryChecks(level);
            Debug.Log("MEKTRA_EXPERIENCE_AUTHORED "+level.id);
            return new AuthoredCourse{number=number,title=level.name,difficulty="Later world",mood=level.instruction,points=stars.Count,level=level,routes=routes.ToArray()};
        }
    }
}
