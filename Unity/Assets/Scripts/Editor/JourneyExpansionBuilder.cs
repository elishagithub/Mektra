using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Mektra.Editor
{
    public static class JourneyExpansionBuilder
    {
        static readonly string[] Names={"Hollow passages","Connected hexagons","Organic curves","Star chimes","Counterflow","Zigzag lanes","Giant curves"};
        static PlatformData Poly(string id,Point[] points,string family)=>WorldPackBuilder.Polygon(id,points,family);
        static PlatformData Bank(string id,Point[] corners)
        {
            var points=new Point[corners.Length*48];for(int e=0;e<corners.Length;e++)for(int j=0;j<48;j++)points[e*48+j]=corners[e]+(corners[(e+1)%corners.Length]-corners[e])*(j/48.0);
            return Poly(id,points,"wedge");
        }
        public static PlatformData Capsule(int index,double x,double y,double width,double height,double angle)
        {
            var points=new Point[96];double radius=height/2,straight=(width-height)/2;
            for(int j=0;j<96;j++){bool right=j<48;double a=(right?-Math.PI/2:Math.PI/2)+(j%48)*Math.PI/47;double dx=(right?straight:-straight)+radius*Math.Cos(a),dy=radius*Math.Sin(a);points[j]=new Point(x+dx*Math.Cos(angle)-dy*Math.Sin(angle),y+dx*Math.Sin(angle)+dy*Math.Cos(angle));}
            return Poly("platform-"+(index+1),points,"rain-capsule");
        }
        static LevelData Base(int kind,int variant,PlatformData[] platforms,RouteLink[] links)=>new LevelData{id="mektra-endless-"+kind+"-"+variant,name=Names[kind-14],worldName=Names[kind-14],width=390,height=1080,stride=1080,segmentKind=kind,variant=variant,authoredRadius=GameConfig.Radius,source="Mektra expanded journey",platforms=platforms,connections=links,hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=kind==17};
        static AuthoredCourse Finish(LevelData level,int number,List<AuthoredRoute> routes,List<StarData> stars)
        {
            level.stars=stars.ToArray();MotionRegression.GeometryChecks(level);return new AuthoredCourse{number=number,title=level.name,difficulty=level.variant==0?"Light":level.variant==3?"Precision":"Medium",mood=level.instruction,points=stars.Count,level=level,routes=routes.ToArray()};
        }
        static AuthoredCourse Bounce(int variant,int number,Character[] chars)
        {
            // Four different rebound rhythms: taper, offset elbow, hourglass and alternating banks.
            double[][] lanes={new double[]{195,195,195},new double[]{174,214,224},new double[]{205,184,205,205},new double[]{170,205,178,212,212}};
            double[][] heights={new double[]{850,570,250},new double[]{850,600,250},new double[]{850,650,450,250},new double[]{850,700,550,400,250}};
            double[][] gaps={new double[]{112,94,108},new double[]{100,92,110},new double[]{114,90,102,116},new double[]{96,94,92,98,112}};
            var center=lanes[variant];var ys=heights[variant];var widths=gaps[variant];
            var left=new List<Point>{new Point(0,250)};var right=new List<Point>{new Point(390,250),new Point(390,850)};
            for(int j=center.Length-1;j>=0;j--)left.Add(new Point(center[j]-widths[j]/2,ys[j]));left.Add(new Point(0,850));
            for(int j=0;j<center.Length;j++)right.Add(new Point(center[j]+widths[j]/2,ys[j]));
            var exit=variant==0?EndlessPackBuilder.Shape(0,3,center[center.Length-1],120,48,0):variant==1?Capsule(3,center[center.Length-1],120,110,60,0):variant==2?OriginalOrganic(3,1,center[center.Length-1],120,54):EndlessPackBuilder.Shape(1,3,center[center.Length-1],120,50,0);
            var platforms=new[]{EndlessPackBuilder.Shape(0,0,center[0],960,48,0),Bank("platform-2",left.ToArray()),Bank("platform-3",right.ToArray()),exit};
            var level=Base(17,variant,platforms,new[]{new RouteLink{from=0,to=3}});level.instruction="Find the spring passage entrance. Rebounds collect the trail and carry you to the exit surface.";
            double chosen=-1;var orbit=new Orbit(platforms[0].outline);
            for(int sample=0;sample<1440&&chosen<0;sample++)
            {
                double d=sample*orbit.length/1440;bool valid=true;
                foreach(var character in chars)
                {
                    var sim=new Simulation(level,character);sim.distance=d;sim.Select(character);sim.SetViewport(816);sim.Start();sim.Jump();
                    for(int step=0;step<720&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);
                    if(sim.phase!=Phase.Playing||sim.flying||sim.platform.index!=3||sim.wedgeBounces<2){valid=false;break;}
                }
                if(valid)chosen=d;
            }
            if(chosen<0)throw new Exception("No narrow bounce route / "+variant);
            var trace=new Simulation(level,chars[0]);trace.distance=chosen;trace.Select(chars[0]);trace.SetViewport(816);trace.Start();var launch=trace.position;trace.Jump();var stars=new List<StarData>();double next=.18;
            while(trace.flying&&trace.phase==Phase.Playing){trace.Advance(GameConfig.Step);if(trace.time>=next){stars.Add(new StarData{id="star-"+(stars.Count+1),x=trace.position.x,y=trace.position.y});next+=.09;}}
            var routes=new List<AuthoredRoute>{new AuthoredRoute{from=0,to=3,distance=chosen,launch=launch,landing=trace.position}};
            level.stars=stars.ToArray();foreach(var character in chars)
            {
                var sim=new Simulation(level,character);sim.distance=chosen;sim.Select(character);sim.SetViewport(816);sim.Start();sim.Jump();for(int i=0;i<720&&sim.flying&&sim.phase==Phase.Playing;i++)sim.Advance(GameConfig.Step);
                if(sim.phase!=Phase.Playing||sim.platform.index!=3||sim.flying||sim.score!=stars.Count)throw new Exception("Bounce misses stars/catcher / "+variant+" / "+character.id);
            }
            Debug.Log("MEKTRA_CHIMES_PASS "+variant+" / "+stars.Count+" stars / "+trace.wedgeBounces+" bounces");return Finish(level,number,routes,stars);
        }
        static AuthoredCourse Counterflow(int variant,int number,Character[] chars)
        {
            const int rows=4,columns=3;var platforms=new PlatformData[14];var links=new List<RouteLink>();platforms[0]=EndlessPackBuilder.Shape(0,0,195,960,48,0);platforms[13]=EndlessPackBuilder.Shape(0,13,195,120,48,0);
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){int i=1+row*columns+col;platforms[i]=Capsule(i,80+col*115,new double[][]{new double[]{790,615,440,265},new double[]{805,625,460,270},new double[]{790,610,435,260},new double[]{805,635,445,265}}[variant][row],new double[]{94,90,92,84}[variant],new double[]{36,40,30,34}[variant],0);}
            for(int col=0;col<columns;col++)links.Add(new RouteLink{from=0,to=1+col});
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){int from=1+row*columns+col;if(row==3)links.Add(new RouteLink{from=from,to=13});else for(int next=Math.Max(0,col-1);next<=Math.Min(2,col+1);next++)links.Add(new RouteLink{from=from,to=1+(row+1)*columns+next});}
            var level=Base(18,variant,platforms,links.ToArray());level.instruction="Neighbouring rows flow in opposite directions. Cross with their rhythm.";var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
            foreach(var link in links){var r=CourseReview.Review(level,link.from,chars,link.to);routes.Add(new AuthoredRoute{from=r.from,to=r.to,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing});}
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){int i=1+row*columns+col;platforms[i].motion=new MotionData{type="horizontal",amplitudeX=12,periodSeconds=new double[]{8,6.5,9,7}[row]+variant*.4,phase=(row+variant)%2==0?.5:0};}
            for(int row=0;row<rows;row++){int i=1+row*3+(row+variant)%3;var b=platforms[i].bounds;stars.Add(new StarData{id="star-"+row,x=(b.minX+b.maxX)/2,y=b.minY-10,parentPlatformId=platforms[i].id});}
            return Finish(level,number,routes,stars);
        }
        static PlatformData OriginalOrganic(int index,int variant,double x,double y,double radius)
        {
            var source=JsonUtility.FromJson<LevelData>(File.ReadAllText("../assets/levels/original-custom-level-before-resize.json"));
            int[] choices={4,5,8,16,18};var original=source.platforms[choices[(index+variant)%choices.Length]];var b=original.bounds;
            double cx=(b.minX+b.maxX)/2,cy=(b.minY+b.maxY)/2,scale=radius*2/Math.Max(b.maxX-b.minX,b.maxY-b.minY);
            var points=new Point[original.outline.Length];for(int j=0;j<points.Length;j++)points[j]=new Point(x+(original.outline[j].x-cx)*scale,y+(original.outline[j].y-cy)*scale);
            return Poly("platform-"+(index+1),points,"original-organic");
        }
        internal static Point[] RoutePositions(int variant,int count,bool recovery=false,int kind=0)
        {
            // Unequal gaps create approach / commitment / recovery beats within each section.
            double[][] xs={new double[]{145,235,175,250,185},new double[]{110,125,270,240,170},new double[]{260,130,145,260,205},new double[]{118,270,155,255,175}};
            double[][] ys={new double[]{790,590,405,250,175},new double[]{805,675,420,245,175},new double[]{750,535,385,240,175},new double[]{795,560,440,260,175}};
            var result=new Point[count];result[0]=new Point(195,960);result[count-1]=new Point(195,120);
            for(int i=1;i<count-1;i++){int j=i-1;double x=xs[variant][j];if(recovery)x=195+(x-195)*.55;double y=ys[variant][j];
                switch(kind)
                {
                    case 2:x+=i%2==1?-15:20;y+=i%2==1?-15:10;break;
                    case 3:x=195+(x-195)*.70;y+=i==2?-22:i==3?12:0;break;
                    case 4:if(i==1)x=195;else x+=i%2==0?-10:10;break;
                    case 6:x+=i%2==1?-18:12;y=new double[]{760,530,310}[j]+variant*(i==2?-8:10);break;
                    case 7:x+=i%2==1?15:-15;y+=i==2?15:-8;break;
                    case 8:x+=i%2==1?-10:20;y+=i%2==1?10:-15;break;
                    case 10:x+=i<3?12:-20;y+=i%2==1?-12:8;break;
                    case 14:x+=i%2==1?-10:15;y+=i%2==1?-15:5;break;
                    case 15:x+=i%2==1?20:-10;y+=i%2==1?15:-5;break;
                    case 16:x+=i%2==1?-20:20;y+=i%2==1?15:-10;break;
                    case 19:x=195+(x-195)*1.2;y+=i%2==1?-10:10;break;
                }
                result[i]=new Point(Math.Clamp(x,85,305),y);}
            return result;
        }
        static AuthoredCourse Traversal(int kind,int variant,int number,Character[] chars)
        {
            var centers=RoutePositions(variant,variant==0?5:6,false,kind);var platforms=new PlatformData[centers.Length];var links=new RouteLink[centers.Length-1];
            for(int i=0;i<platforms.Length;i++)
            {
                bool seam=i==0||i==platforms.Length-1;int family=seam?0:kind==15?2:kind==16?4+(i+variant)%3:new[]{1,7,9,3}[(i+variant)%4];
                double radius=seam?48:kind==19?(i%2==0?32:40):variant==0?44:i%3==0?46:36;
                platforms[i]=EndlessPackBuilder.Shape(family,i,centers[i].x,centers[i].y,radius,0);
                if(kind==16&&!seam)platforms[i]=OriginalOrganic(i,variant,centers[i].x,centers[i].y,radius);
                if(i<links.Length)links[i]=new RouteLink{from=i,to=i+1};
            }
            var level=Base(kind,variant,platforms,links);level.instruction=kind==19?"Cross the zigzag pockets. Stars mark the precision route.":kind==14?"Travel through the hollow turns using the varied landings.":kind==15?"Thread the connected hexagon lattice.":"Read the curved surfaces and alternate your launch side.";
            var routes=new List<AuthoredRoute>();var stars=new List<StarData>();foreach(var link in links){var r=CourseReview.Review(level,link.from,chars,link.to);routes.Add(new AuthoredRoute{from=r.from,to=r.to,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing});var p=r.launch+(r.landing-r.launch)*.55;stars.Add(new StarData{id="star-"+stars.Count,x=p.x,y=p.y});}
            var hazards=new List<PlatformData>();
            if(kind==14||kind==19)
            {
                foreach(int side in new[]{-1,1})
                {
                    var inner=new List<Point>();for(int j=0;j<=168;j++)
                    {
                        double y=120+j*5;int i=0;while(i<platforms.Length-2&&y<((platforms[i+1].bounds.minY+platforms[i+1].bounds.maxY)/2))i++;
                        double ay=(platforms[i].bounds.minY+platforms[i].bounds.maxY)/2,by=(platforms[i+1].bounds.minY+platforms[i+1].bounds.maxY)/2;double ax=(platforms[i].bounds.minX+platforms[i].bounds.maxX)/2,bx=(platforms[i+1].bounds.minX+platforms[i+1].bounds.maxX)/2;
                        double t=Math.Clamp((ay-y)/(ay-by),0,1),edge=ax+(bx-ax)*t+side*(kind==19?64:78);
                        foreach(var support in platforms){var b=support.bounds;if(y>=b.minY-22&&y<=b.maxY+22)edge=side<0?Math.Min(edge,b.minX-22):Math.Max(edge,b.maxX+22);}
                        // Preserve every validated straight jump ray with player clearance.
                        foreach(var route in routes)if(y>=Math.Min(route.launch.y,route.landing.y)&&y<=Math.Max(route.launch.y,route.landing.y)){double u=(y-route.launch.y)/(route.landing.y-route.launch.y);double x=route.launch.x+(route.landing.x-route.launch.x)*u;edge=side<0?Math.Min(edge,x-27):Math.Max(edge,x+27);}
                        inner.Add(new Point(Math.Clamp(edge,2,388),y));
                    }
                    var outline=new List<Point>(inner);outline.Add(new Point(side<0?0:390,960));outline.Add(new Point(side<0?0:390,120));hazards.Add(Poly("hazard-channel-"+side,outline.ToArray(),"corridor"));
                }
            }
            if(kind==15)foreach(var route in routes)
            {
                var axis=(route.landing-route.launch).Normal;var perpendicular=new Point(-axis.y,axis.x);var center=(route.landing+route.launch)*.5;
                foreach(int sign in new[]{-1,1})for(int cell=0;cell<2;cell++){var c=center+perpendicular*(sign*65)+axis*(cell*43-21.5);if(c.x<27||c.x>363)continue;bool clear=true;foreach(var support in platforms){if(Geometry.Inside(c,support.outline)){clear=false;break;}for(int edge=0;edge<support.outline.Length;edge++)if(Geometry.Segment(c,support.outline[edge],support.outline[(edge+1)%support.outline.Length]).distance<45){clear=false;break;}if(!clear)break;}if(!clear)continue;var hex=EndlessPackBuilder.Shape(2,0,c.x,c.y,25,0);hex.id="hazard-connected-"+hazards.Count;hazards.Add(hex);}
            }
            level.hazards=hazards.ToArray();return Finish(level,number,routes,stars);
        }
        [Serializable] class LobeShape {public int lobes;public string sourceId;public Point[] outline;}
        [Serializable] class LobeLibrary {public LobeShape[] shapes;}
        static LobeLibrary giantLobes;
        static PlatformData GiantCurve(int index,int style,int side,double y,int variant)
        {
            if(giantLobes==null)giantLobes=JsonUtility.FromJson<LobeLibrary>(File.ReadAllText("../assets/shapes/custom/giant-lobe-library.json"));
            int lobes=2+(index-1+variant*2)%6;if(lobes==4)lobes=variant==1?2:variant==3?3:6;var source=giantLobes.shapes[lobes-2];
            double minX=double.MaxValue,maxX=double.MinValue,minY=double.MaxValue,maxY=double.MinValue;
            foreach(var point in source.outline){minX=Math.Min(minX,point.x);maxX=Math.Max(maxX,point.x);minY=Math.Min(minY,point.y);maxY=Math.Max(maxY,point.y);}
            double cx=(minX+maxX)/2,cy=(minY+maxY)/2,turn=lobes>=6?Math.PI/4+variant*Math.PI/2:variant*.25+index*.15;
            var points=new Point[source.outline.Length];minX=minY=double.MaxValue;maxX=maxY=double.MinValue;
            for(int j=0;j<points.Length;j++)
            {
                var point=source.outline[j]-new Point(cx,cy);points[j]=new Point(point.x*Math.Cos(turn)-point.y*Math.Sin(turn),point.x*Math.Sin(turn)+point.y*Math.Cos(turn));
                minX=Math.Min(minX,points[j].x);maxX=Math.Max(maxX,points[j].x);minY=Math.Min(minY,points[j].y);maxY=Math.Max(maxY,points[j].y);
            }
            double scale=300/Math.Max(maxX-minX,maxY-minY);cx=(minX+maxX)/2;cy=(minY+maxY)/2;
            for(int j=0;j<points.Length;j++){double x=-5+(points[j].x-cx)*scale;points[j]=new Point(side<0?x:390-x,y+(points[j].y-cy)*scale);}
            double area=0;for(int j=0;j<points.Length;j++)area+=points[j].x*points[(j+1)%points.Length].y-points[(j+1)%points.Length].x*points[j].y;if(area<0)Array.Reverse(points);
            var p=Poly("platform-"+(index+1),points,"giant-curve-"+lobes+"-lobes-"+source.sourceId);p.orbitDirection=side<0?-1:1;
            if(variant>=2)p.motion=new MotionData{type=index%2==0?"horizontal":"vertical",amplitudeX=index%2==0?5:0,amplitudeY=index%2==0?0:5,periodSeconds=10+index,phase=index*.13};
            return p;
        }
        static AuthoredCourse GiantCurves(int variant,int number,Character[] chars)
        {
            var platforms=new PlatformData[6];platforms[0]=EndlessPackBuilder.Shape(0,0,195,970,38,0);platforms[5]=EndlessPackBuilder.Shape(0,5,195,65,38,0);
            for(int i=1;i<5;i++)platforms[i]=GiantCurve(i,(i-1+variant)%4,(i+variant)%2==0?1:-1,790-(i-1)*185,variant);
            var links=new RouteLink[5];for(int i=0;i<5;i++)links[i]=new RouteLink{from=i,to=i+1};
            var level=Base(20,variant,platforms,links);level.instruction="Cross between the giant curves. Jump before your orbit reaches the screen edge.";
            var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
            for(int i=0;i<5;i++)
            {
                var r=CourseReview.Review(level,i,chars,i+1);routes.Add(new AuthoredRoute{from=i,to=i+1,distance=r.distance,windowSeconds=r.windowSeconds,launch=r.launch,landing=r.landing});
                foreach(double t in new[]{.32,.55,.77}){var point=r.launch+(r.landing-r.launch)*t;stars.Add(new StarData{id="star-"+stars.Count,x=point.x,y=point.y});}
            }
            return Finish(level,number,routes,stars);
        }
        public static void ValidateGiantCurvesAndBuild()
        {
            var pack=LevelPack.Load();var courses=pack.Courses();var chars=CharacterCatalog.Load();
            foreach(var course in pack.levels)if(course.level.segmentKind==20)foreach(var character in chars)
            {
                var collected=new HashSet<string>();
                foreach(var route in course.routes)
                {
                    var sim=MotionRegression.Launch(courses,character,course.number-1,route.from,route.distance,0);
                    foreach(var star in sim.stars)if(star.section==course.number-1&&star.collected)collected.Add(star.id);
                }
                if(collected.Count!=course.level.stars.Length)throw new Exception("Giant curve unreachable star / "+course.number+" / "+character.id+" / "+collected.Count);
                foreach(double edge in new[]{GameConfig.Radius*.5,390-GameConfig.Radius*.5})
                {
                    var sim=new Simulation(course.level,character);sim.SetViewport(816);sim.Start();sim.flying=true;sim.position=new Point(edge,970);sim.velocity=new Point();sim.Advance(GameConfig.Step);
                    if(sim.reason!="wall")throw new Exception("Giant curve wall lost lethality");
                }
            }
            Debug.Log("MEKTRA_GIANT_CURVE_VALIDATION PASS: all 15 stars reachable in four layouts across eight characters; both screen edges remain lethal.");
            MektraBuild.BuildValidatedOpening(pack);
        }
        public static void PublishGiantCurves()
        {
            var pack=LevelPack.Load();var chars=CharacterCatalog.Load();var entries=new List<AuthoredCourse>(pack.levels);
            if(entries.Count>80)entries.RemoveRange(80,entries.Count-80);
            for(int variant=0;variant<4;variant++)entries.Add(GiantCurves(variant,81+variant,chars));
            pack.levels=entries.ToArray();File.WriteAllText("../assets/levels/endless/giant-curves-preview.json",JsonUtility.ToJson(pack,true));
            EndlessRegression.Run(pack);
            File.WriteAllText("Assets/Resources/endless-pack.json",JsonUtility.ToJson(pack,true));File.WriteAllText("../assets/levels/endless/pack.json",JsonUtility.ToJson(pack,true));
            foreach(var course in entries)File.WriteAllText("../assets/levels/endless/Mektra_level_"+course.number+".json",JsonUtility.ToJson(course,true));
            AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
        public static void Publish()
        {
            var pack=LevelPack.Load();var chars=CharacterCatalog.Load();var entries=new List<AuthoredCourse>(pack.levels);
            for(int kind=0;kind<14;kind++)for(int variant=0;variant<4;variant++)entries[kind*4+variant]=kind<7?EndlessPackBuilder.Make(kind,variant,kind*4+variant+1,chars):ExperiencePackBuilder.Make(kind,variant,kind*4+variant+1,chars);
            if(entries.Count>56)entries.RemoveRange(56,entries.Count-56);
            for(int kind=14;kind<21;kind++)for(int variant=0;variant<4;variant++)entries.Add(kind==20?GiantCurves(variant,entries.Count+1,chars):kind==17?Bounce(variant,entries.Count+1,chars):kind==18?Counterflow(variant,entries.Count+1,chars):Traversal(kind,variant,entries.Count+1,chars));
            pack.levels=entries.ToArray();StarJumpDesigner.Apply(pack,chars);File.WriteAllText("../assets/levels/endless/expansion-preview.json",JsonUtility.ToJson(pack,true));EndlessRegression.Run(pack);
            File.WriteAllText("Assets/Resources/endless-pack.json",JsonUtility.ToJson(pack,true));File.WriteAllText("../assets/levels/endless/pack.json",JsonUtility.ToJson(pack,true));foreach(var course in entries)File.WriteAllText("../assets/levels/endless/Mektra_level_"+course.number+".json",JsonUtility.ToJson(course,true));AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
    }
}
