using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class WorldPackBuilder
    {
        public static readonly string[] Worlds={"Circle Grove","Soft Square","Hex Circuit","Rhombus Ridge","Twin Lobe","Tri Lobe","Clover Core"};
        static readonly string[] Stages={"Learn","Remix","Motion","Mastery"};
        static string F(double value)=>value.ToString("0.#####",CultureInfo.InvariantCulture);
        public static PlatformData Polygon(string id,Point[] points,string family)
        {
            var commands=new List<PathCommand>();var path=new System.Text.StringBuilder();
            double minX=double.MaxValue,minY=double.MaxValue,maxX=double.MinValue,maxY=double.MinValue;
            for(int i=0;i<points.Length;i++)
            {
                var p=points[i];string op=i==0?"M":"L";commands.Add(new PathCommand{op=op,v=new[]{p.x,p.y}});path.Append(op).Append(F(p.x)).Append(' ').Append(F(p.y)).Append(' ');
                minX=Math.Min(minX,p.x);maxX=Math.Max(maxX,p.x);minY=Math.Min(minY,p.y);maxY=Math.Max(maxY,p.y);
            }
            commands.Add(new PathCommand{op="Z",v=Array.Empty<double>()});path.Append('Z');
            return new PlatformData{id=id,sourceShapeId=family,d=path.ToString(),outline=points,paths=commands.ToArray(),bounds=new BoundsData{minX=minX,maxX=maxX,minY=minY,maxY=maxY}};
        }
        static PlatformData Shape(int family,int index,double x,double y,double radius)
        {
            const int samples=144;var points=new Point[samples];
            for(int i=0;i<samples;i++)
            {
                double a=i*Math.PI*2/samples,px=Math.Cos(a),py=Math.Sin(a),r=1;
                if(family==1){px=Math.Sign(px)*Math.Pow(Math.Abs(px),.5);py=Math.Sign(py)*Math.Pow(Math.Abs(py),.5);}
                if(family==2)r=Math.Cos(Math.PI/6)/Math.Cos((a+Math.PI/6)%(Math.PI/3)-Math.PI/6);
                if(family==3)r=1/Math.Pow(Math.Pow(Math.Abs(px),1.35)+Math.Pow(Math.Abs(py),1.35),1/1.35);
                if(family>=4)
                {
                    double wave=family==4?.22:.16;
                    r=1-wave+wave*Math.Cos((family-2)*(a+Math.PI/2));
                }
                points[i]=new Point(x+radius*px*r,y+radius*py*r);
            }
            return Polygon("platform-"+(index+1)+"-world-"+(family+1),points,"world-family-"+(family+1));
        }
        static PlatformData CorridorRail(string id,Point center,Point along,Point side,double offset,double length)
        {
            const double thickness=9;
            return Polygon(id,new[]{center+along*(-length*.5)+side*offset,center+along*(length*.5)+side*offset,center+along*(length*.5)+side*(offset+thickness),center+along*(-length*.5)+side*(offset+thickness)},"corridor-rail");
        }
        public static void Build()
        {
            var chars=CharacterCatalog.Load();var courses=new List<AuthoredCourse>();
            for(int world=0;world<Worlds.Length;world++)for(int stage=0;stage<4;stage++)
            {
                int number=world*4+stage+1,count=16;
                double radius=stage==0?52:stage==1?48:46;
                double spread=stage==0?22:stage==1?42:stage==2?32:38;
                var platforms=new List<PlatformData>();
                for(int index=0;index<count;index++)
                {
                    bool bridge=index==0||index==count-1;
                    double x=bridge?195:195+Math.Sin(index*1.12+world*.4)*spread;
                    double y=3144-index*(3024.0/(count-1));
                    platforms.Add(Shape(bridge?0:world,index,x,y,bridge?48:radius));
                }
                if(stage==2||stage==3&&world>=4)
                {
                    string type=world>=5?"orbit":world>=3?"vertical":"horizontal";
                    platforms[5].motion=new MotionData{type=type,amplitudeX=type=="vertical"?0:10,amplitudeY=type=="horizontal"?0:8,periodSeconds=8,phase=0};
                }
                var level=new LevelData{id="mektra-world-"+(world+1)+"-"+(stage+1),name=Worlds[world]+" · "+Stages[stage],source="Mektra shape-world authored progression",width=390,height=3264,stride=3264,worldName=Worlds[world],worldNumber=world+1,worldStage=stage+1,stageName=Stages[stage],platforms=platforms.ToArray(),hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>()};
                var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
                for(int from=0;from<count-1;from++)
                {
                    var review=CourseReview.Review(level,from,chars);
                    var start=new Simulation(level,chars[0]);start.platform=start.platforms[from];start.distance=review.distance;start.Select(chars[0]);
                    var direction=start.JumpDirection();double length=(review.landing-review.launch).Length;
                    routes.Add(new AuthoredRoute{from=from,to=from+1,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                    int rewards=stage==1||from%4==0?2:1;
                    for(int point=0;point<rewards;point++)
                    {
                        var position=review.launch+direction*(length*(rewards==1?.48:.35+point*.28));
                        stars.Add(new StarData{id="star-"+(from+1)+"-"+(point+1),x=position.x,y=position.y});
                    }
                }
                level.stars=stars.ToArray();
                if(stage==3)
                {
                    // A single short squeeze is followed by several ordinary recovery jumps.
                    var route=routes[8];var direction=(route.landing-route.launch).Normal;var side=new Point(-direction.y,direction.x);
                    var center=route.launch+(route.landing-route.launch)*.5;double halfGap=(64-world*4)*.5;
                    level.hazards=new[]{CorridorRail("hazard-corridor-left",center,direction,side,halfGap,70),CorridorRail("hazard-corridor-right",center,direction,side*(-1),halfGap,70)};
                }
                MotionRegression.GeometryChecks(level);
                courses.Add(new AuthoredCourse{number=number,title=level.name,difficulty=stage==0?"Easy":stage==3?"Tough":"Intermediate",mood=Stages[stage],theme=ThemeJourney.Names[world%ThemeJourney.Names.Length],points=stars.Count,level=level,routes=routes.ToArray()});
                Debug.Log("MEKTRA_WORLD_CREATED "+number+" "+level.name);
            }
            var pack=new LevelPack{levels=courses.ToArray()};
            LevelPackRegression.Run(pack);MotionRegression.Run(pack);WorldRegression.Run(pack);
            string folder="../assets/levels/worlds";Directory.CreateDirectory(folder);
            File.WriteAllText(folder+"/candidate-pack.json",JsonUtility.ToJson(pack,true));
            Debug.Log("MEKTRA_WORLDS_SUCCESS: 7 worlds, 28 courses, phased movement and collision corridors validated.");
        }
        public static void Publish()
        {
            Build();CourseReview.PlayCampaign();
            string folder="../assets/levels/worlds";
            File.Copy(folder+"/candidate-pack.json",folder+"/pack.json",true);
            File.Copy(folder+"/candidate-pack.json","Assets/Resources/world-pack.json",true);
            var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(folder+"/pack.json"));
            foreach(var course in pack.levels)File.WriteAllText(folder+"/Mektra_level_"+course.number+".json",JsonUtility.ToJson(course,true));
            UnityEditor.AssetDatabase.Refresh();MektraBuild.Build();
        }
    }
}
