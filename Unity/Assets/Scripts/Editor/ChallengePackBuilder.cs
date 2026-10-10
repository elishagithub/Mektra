using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Mektra.Editor
{
    // Five deliberately different problems. The old shape-world pack remains archived.
    public static class ChallengePackBuilder
    {
        static PlatformData Shape(int family,int index,double x,double y,double radius)
        {
            const int samples=144;var points=new Point[samples];
            for(int i=0;i<samples;i++)
            {
                double a=i*Math.PI*2/samples,px=Math.Cos(a),py=Math.Sin(a),r=1;
                if(family==1){px=Math.Sign(px)*Math.Pow(Math.Abs(px),.5);py=Math.Sign(py)*Math.Pow(Math.Abs(py),.5);}
                if(family==2)r=Math.Cos(Math.PI/6)/Math.Cos((a+Math.PI/6)%(Math.PI/3)-Math.PI/6);
                if(family==3)r=.78+.22*Math.Cos(2*(a+Math.PI/2));
                points[i]=new Point(x+radius*px*r,y+radius*py*r);
            }
            return WorldPackBuilder.Polygon("platform-"+(index+1),points,"challenge-shape-"+family);
        }
        static PlatformData Bar(string id,double x,double y,double halfWidth,double halfHeight)
            =>WorldPackBuilder.Polygon(id,new[]{new Point(x-halfWidth,y-halfHeight),new Point(x+halfWidth,y-halfHeight),new Point(x+halfWidth,y+halfHeight),new Point(x-halfWidth,y+halfHeight)},"hazard");
        static AuthoredCourse Course(int number,string name,string rule,double[] xs,double[] ys,double[] sizes,int[] families,RouteLink[] edges=null)
        {
            var platforms=new PlatformData[xs.Length];for(int i=0;i<xs.Length;i++)platforms[i]=Shape(families[i],i,xs[i],ys[i],sizes[i]);
            if(edges==null){edges=new RouteLink[platforms.Length-1];for(int i=0;i<edges.Length;i++)edges[i]=new RouteLink{from=i,to=i+1};}
            return new AuthoredCourse{number=number,title=name,difficulty=number<3?"Intermediate":"Tough",mood=rule,theme=ThemeJourney.Names[number-1],level=new LevelData{id="mektra-challenge-"+number,name=name,source="Mektra five-challenge playtest opening",width=390,height=3264,stride=3264,worldNumber=number,worldStage=1,worldName=name,stageName=name,instruction=rule,connections=edges,platforms=platforms,hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>()}};
        }
        public static void Publish()
        {
            var chars=CharacterCatalog.Load();var courses=new List<AuthoredCourse>();
            // Crossings alternate in direction and length, with broad rests between tight targets.
            courses.Add(Course(1,"Crossfire","Look across the screen. Launch toward the next landing.",
                new double[]{195,92,287,120,265,100,285,195,85,285,195},
                new double[]{3144,2920,2650,2410,2070,1840,1540,1260,950,640,120},
                new double[]{40,43,38,48,32,48,36,54,36,42,48},
                new[]{0,0,1,0,2,0,3,0,1,2,0}));
            // Three forks, not a decorative second lane. Both routes merge and stay traversable.
            courses.Add(Course(2,"Risk / Reward","Wide left landings are safer. Small right landings hold bonus stars.",
                new double[]{195,100,300,195,95,295,195,105,295,195,110,265,195},
                new double[]{3144,2860,2860,2540,2240,2240,1910,1580,1580,1220,880,540,120},
                new double[]{40,50,26,49,48,25,52,48,24,56,43,40,48},
                new[]{0,0,2,0,1,2,0,0,2,0,1,2,0},
                new[]{new RouteLink{from=0,to=1},new RouteLink{from=0,to=2},new RouteLink{from=1,to=3},new RouteLink{from=2,to=3},new RouteLink{from=3,to=4},new RouteLink{from=3,to=5},new RouteLink{from=4,to=6},new RouteLink{from=5,to=6},new RouteLink{from=6,to=7},new RouteLink{from=6,to=8},new RouteLink{from=7,to=9},new RouteLink{from=8,to=9},new RouteLink{from=9,to=10},new RouteLink{from=10,to=11},new RouteLink{from=11,to=12}}));
            courses.Add(Course(3,"On the Beat","Lead the moving target. Its current position is not its landing position.",
                new double[]{195,160,260,145,235,150,245,145,235,195,195},
                new double[]{3144,2920,2650,2330,2090,1770,1500,1170,900,590,120},
                new double[]{40,35,48,29,48,31,50,28,49,50,48},
                new[]{0,0,2,0,1,0,2,0,3,0,0}));
            foreach(int i in new[]{1,3,5,7})courses[2].level.platforms[i].motion=new MotionData{type="horizontal",amplitudeX=48,periodSeconds=3.6,phase=i*.19};
            courses.Add(Course(4,"No Second Lap","Orange platforms crumble. Find your exit before the ring runs out.",
                new double[]{195,150,240,155,195,245,150,240,195,135,250,195},
                new double[]{3144,2900,2660,2390,2110,1840,1580,1310,1040,780,500,120},
                new double[]{40,27,26,28,52,27,25,28,53,28,27,48},
                new[]{0,0,0,0,1,0,0,0,1,0,0,0}));
            foreach(int i in new[]{1,2,3,5,6,7,9,10})courses[3].level.platforms[i].crumbleSeconds=2.1;
            courses.Add(Course(5,"Gate Runner","Orange bars are lethal. Wait for an opening, then commit.",
                new double[]{195,195,115,255,195,195,115,260,195,195,195},
                new double[]{3144,2900,2670,2360,2050,1780,1510,1220,950,630,120},
                new double[]{40,46,35,42,52,46,33,40,52,48,48},
                new[]{0,0,2,3,0,0,2,3,0,0,0}));
            // Author the normal-launch windows before adding lethal gates. Validation uses real hazards.
            foreach(var course in courses)
            {
                var stars=new List<StarData>();var routes=new List<AuthoredRoute>();int edgeNumber=0;
                foreach(var edge in course.level.connections)
                {
                    var review=CourseReview.Review(course.level,edge.from,chars,edge.to);
                    routes.Add(new AuthoredRoute{from=edge.from,to=edge.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                    bool bonus=course.number==2&&(edge.to==2||edge.to==5||edge.to==8);
                    int count=bonus?3:1;
                    for(int k=0;k<count;k++)
                    {
                        var pos=review.launch+(review.landing-review.launch)*(count==1?.48:.28+k*.2);
                        stars.Add(new StarData{id="star-"+(++edgeNumber),x=pos.x,y=pos.y});
                    }
                }
                course.level.stars=stars.ToArray();course.routes=routes.ToArray();course.points=stars.Count;
            }
            // The fork's central blockers prevent a straight vertical skip from erasing the decision.
            var fork=courses[1].level;fork.suppressMagnets=true;fork.hazards=new[]{Bar("hazard-fork-1",195,2860,35,34),Bar("hazard-fork-2",195,2240,35,34),Bar("hazard-fork-3",195,1580,35,34)};
            foreach(int i in new[]{1,4,7})fork.platforms[i].label="SAFE";
            foreach(int i in new[]{2,5,8})fork.platforms[i].label="BONUS";
            var gates=new List<PlatformData>();
            foreach(int edge in new[]{0,4,8})
            {
                var r=courses[4].routes[edge];var mid=r.launch+(r.landing-r.launch)*.5;
                var gate=Bar("hazard-gate-"+edge,195,mid.y,48,7);
                gate.motion=new MotionData{type="horizontal",amplitudeX=110,periodSeconds=3.2,phase=.25+edge*.025};gates.Add(gate);
            }
            // One fixed precision tunnel after the first gate, with a broad recovery platform.
            var tunnel=courses[4].routes[2];var along=(tunnel.landing-tunnel.launch).Normal;var side=new Point(-along.y,along.x);var center=(tunnel.launch+tunnel.landing)*.5;
            foreach(int sign in new[]{-1,1})gates.Add(WorldPackBuilder.Polygon("hazard-tunnel-"+sign,new[]{center-along*35+side*(sign*24),center+along*35+side*(sign*24),center+along*35+side*(sign*33),center-along*35+side*(sign*33)},"precision-rail"));
            courses[4].level.hazards=gates.ToArray();
            courses[4].level.platforms[courses[4].level.platforms.Length-1].label="FINISH";
            var pack=new LevelPack{challengeOpening=true,levels=courses.ToArray()};
            ChallengeRegression.Run(pack);
            string folder="../assets/levels/challenges";Directory.CreateDirectory(folder);
            File.WriteAllText(folder+"/pack.json",JsonUtility.ToJson(pack,true));
            File.WriteAllText("Assets/Resources/challenge-pack.json",JsonUtility.ToJson(pack,true));
            foreach(var c in courses)File.WriteAllText(folder+"/Mektra_level_"+c.number+".json",JsonUtility.ToJson(c,true));
            AssetDatabase.Refresh();MektraBuild.BuildValidatedOpening(pack);
        }
    }
}
