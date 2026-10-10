using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Mektra.Editor
{
    public static class EndlessPackBuilder
    {
        static readonly string[] Names={"Crossings","Star fork","Moving targets","Crumble chain","Gates","Open stretch","Pressure passage"};
        static readonly string[] Rules={"Read the next landing. Launch across the gap.","Wide targets are safer. Small targets carry bonus stars.","Lead the moving target before you launch.","Orange rings count down. Leave before the platform crumbles.","Watch the orange gate. Commit when it clears your path.","Room to breathe. Collect stars and find your next opening.","Read the moving gate, then leave the crumbling landing."};
        static readonly string[] Families={"circle","rounded-square","hexagon","rhombus","two-lobe","three-lobe","four-lobe","oval","pentagon","rounded-triangle"};
        internal static PlatformData Shape(int family,int index,double x,double y,double radius,double turn)
        {
            var points=new Point[144];
            for(int i=0;i<points.Length;i++)
            {
                double a=i*Math.PI*2/points.Length,px=Math.Cos(a),py=Math.Sin(a),r=1;
                if(family==1){px=Math.Sign(px)*Math.Pow(Math.Abs(px),.5);py=Math.Sign(py)*Math.Pow(Math.Abs(py),.5);}
                if(family==2||family==8)
                {
                    int sides=family==2?6:5;double h=Math.PI/sides,angle=(a-turn+h+Math.PI*8)%(2*h)-h;r=Math.Cos(h)/Math.Cos(angle);
                }
                if(family==3)r=1/Math.Pow(Math.Pow(Math.Abs(px),1.35)+Math.Pow(Math.Abs(py),1.35),1/1.35);
                if(family>=4&&family<=6){double wave=family==4?.22:.16;r=1-wave+wave*Math.Cos((family-2)*(a+Math.PI/2));}
                if(family==7){px*=.78;py*=1.08;}
                // A smooth, convex three-corner outline rather than a sharp tip.
                if(family==9)r=.95+.05*Math.Cos(3*(a-turn));
                points[i]=new Point(x+radius*px*r,y+radius*py*r);
            }
            return WorldPackBuilder.Polygon("platform-"+(index+1),points,Families[family]);
        }
        static PlatformData Bar(string id,double x,double y,double w,double h)
            =>WorldPackBuilder.Polygon(id,new[]{new Point(x-w,y-h),new Point(x+w,y-h),new Point(x+w,y+h),new Point(x-w,y+h)},"hazard");
        internal static AuthoredCourse Make(int kind,int variant,int number,Character[] chars)
        {
            double[] xs,ys,rs;RouteLink[] links;
            if(kind==1)
            {
                xs=new double[]{195,100,300,195};ys=new double[]{960,560,560,120};rs=new double[]{48,50,28,48};
                links=new[]{new RouteLink{from=0,to=1},new RouteLink{from=0,to=2},new RouteLink{from=1,to=3},new RouteLink{from=2,to=3}};
            }
            else
            {
                xs=kind==0?new double[]{195,92,287,140,195}:kind==2?new double[]{195,160,245,150,195}:kind==3?new double[]{195,150,240,155,195}:kind==5?new double[]{195,135,255,195}:new double[]{195,195,115,255,195};
                ys=kind==5?new double[]{960,690,405,120}:new double[]{960,745,520,300,120};
                rs=kind==0?new double[]{48,43,38,45,48}:kind==2?new double[]{48,35,46,31,48}:kind==3?new double[]{48,27,26,28,48}:kind==5?new double[]{48,54,52,48}:new double[]{48,46,35,42,48};
                links=new RouteLink[xs.Length-1];for(int i=0;i<links.Length;i++)links[i]=new RouteLink{from=i,to=i+1};
            }
            // Variants change cadence and crossing direction, rather than merely mirroring.
            if(kind!=1)
            {
                var positions=JourneyExpansionBuilder.RoutePositions(variant,xs.Length,kind==5,kind);
                for(int i=1;i<xs.Length-1;i++){xs[i]=positions[i].x;ys[i]=positions[i].y;rs[i]*=variant==0?1.06:variant==3&&i%2==0?.90:1;}
            }
            else
            {
                xs[1]=new double[]{100,118,90,128}[variant];xs[2]=new double[]{300,280,294,270}[variant];
                ys[1]=new double[]{560,640,490,610}[variant];ys[2]=new double[]{560,480,620,510}[variant];
            }
            var platforms=new PlatformData[xs.Length];
            for(int i=0;i<xs.Length;i++)
            {
                int family=i==0||i==xs.Length-1?0:(kind*3+variant*2+i)%Families.Length;
                if(kind==3&&variant==0)family=0;
                var exit=Array.Find(links,link=>link.from==i);
                double angle=exit==null?-Math.PI*.5:Math.Atan2(ys[exit.to]-ys[i],xs[exit.to]-xs[i]);
                platforms[i]=Shape(family,i,xs[i],ys[i],rs[i],angle);
            }
            if(kind==2)foreach(int i in new[]{1,3})platforms[i].motion=new MotionData{type=variant==3?"orbit":"horizontal",amplitudeX=variant<2?40:48,amplitudeY=variant==3?12:0,periodSeconds=variant%2==0?3.6:4.2,phase=i*.19+variant*.1};
            if(kind==3)for(int i=1;i<platforms.Length-1;i++)platforms[i].crumbleSeconds=Math.Max(2.1,new Orbit(platforms[i].outline).length/GameConfig.OrbitSpeed*.87);
            if(kind==6)
            {
                platforms[2].motion=new MotionData{type="vertical",amplitudeY=12,periodSeconds=4.2,phase=.2};
                platforms[3].crumbleSeconds=2.8;
            }
            
            var level=new LevelData{id="mektra-endless-"+kind+"-"+variant,name=Names[kind],source="Mektra endless mechanic library",authoredRadius=GameConfig.Radius,width=390,height=1080,stride=1080,segmentKind=kind,variant=variant,instruction=Rules[kind],connections=links,platforms=platforms,hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>(),suppressMagnets=kind==1};
            var routes=new List<AuthoredRoute>();var stars=new List<StarData>();
            foreach(var link in links)
            {
                var review=CourseReview.Review(level,link.from,chars,link.to);
                routes.Add(new AuthoredRoute{from=link.from,to=link.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                int count=kind==1&&link.to==2?3:1;
                for(int i=0;i<count;i++)
                {
                    var point=review.launch+(review.landing-review.launch)*(count==1?.48:.28+i*.2);
                    stars.Add(new StarData{id="star-"+(stars.Count+1),x=point.x,y=point.y});
                }
            }
            level.stars=stars.ToArray();
            if(kind==1)level.hazards=new[]{Bar("hazard-fork",195,(ys[1]+ys[2])/2,16,24)};
            if(kind==4||kind==6)
            {
                var first=routes[0];var gate=Bar("hazard-gate",195,(first.launch.y+first.landing.y)*.5,48,7);
                gate.motion=new MotionData{type="horizontal",amplitudeX=110,periodSeconds=3.2+variant*.2,phase=.25};
                var hazards=new List<PlatformData>{gate};
                if(kind==4)
                {
                    var route=routes[1];foreach(var candidate in routes)if((candidate.landing-candidate.launch).Length>(route.landing-route.launch).Length)route=candidate;var along=(route.landing-route.launch).Normal;var side=new Point(-along.y,along.x);var center=(route.launch+route.landing)*.5;
                    foreach(int sign in new[]{-1,1})hazards.Add(WorldPackBuilder.Polygon("hazard-tunnel-"+sign,new[]{center-along*Math.Min(30,(route.landing-route.launch).Length*.18)+side*(sign*26),center+along*Math.Min(30,(route.landing-route.launch).Length*.18)+side*(sign*26),center+along*Math.Min(30,(route.landing-route.launch).Length*.18)+side*(sign*35),center-along*Math.Min(30,(route.landing-route.launch).Length*.18)+side*(sign*35)},"precision-rail"));
                }
                level.hazards=hazards.ToArray();
            }
            MotionRegression.GeometryChecks(level);
            Debug.Log("MEKTRA_ENDLESS_AUTHORED "+level.id);
            return new AuthoredCourse{number=number,title=Names[kind],difficulty=kind==5?"Recovery":variant<2?"Intermediate":"Tough",mood=Rules[kind],points=stars.Count,level=level,routes=routes.ToArray()};
        }
        public static void Publish()
        {
            var chars=CharacterCatalog.Load();var entries=new List<AuthoredCourse>();
            for(int kind=0;kind<7;kind++)for(int variant=0;variant<4;variant++)entries.Add(Make(kind,variant,entries.Count+1,chars));
            for(int kind=7;kind<14;kind++)for(int variant=0;variant<4;variant++)entries.Add(ExperiencePackBuilder.Make(kind,variant,entries.Count+1,chars));
            var pack=new LevelPack{endless=true,levels=entries.ToArray()};
            StarJumpDesigner.Apply(pack,chars);
            EndlessRegression.Run(pack);
            string folder="../assets/levels/endless";Directory.CreateDirectory(folder);
            File.WriteAllText(folder+"/pack.json",JsonUtility.ToJson(pack,true));File.WriteAllText("Assets/Resources/endless-pack.json",JsonUtility.ToJson(pack,true));
            foreach(var entry in entries)File.WriteAllText(folder+"/Mektra_level_"+entry.number+".json",JsonUtility.ToJson(entry,true));
            AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
    }
}
