using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class LevelPackBuilder
    {
        [Serializable] public class Route {public int from,to;public double distance,windowSeconds;public Point launch,landing;}
        [Serializable] public class Design {public int number;public string title,difficulty,mood,theme;public int points;public LevelData level;public Route[] routes;}
        [Serializable] class Pack {public int screenHeight=816,screensPerLevel=4;public Design[] levels;}
        static readonly string[] Titles={"First Orbit","Soft Steps","Gentle Zigzag","Petal Playground","First Squeeze","Crosswind","Diamond Dance","Capsule Coast","Clovers & Coins","Narrow Passage","Pebble Hop","Gear Garden","Twisted Rhythm","Star Carnival","Edge of Night","Small Targets","Storm Crossing","Hex Circuit","Breathing Space","Pressure Run","Critical Path","Precision Peaks","Final Squeeze","Victory Playground","Mektra Master"};
        static readonly string[] Moods={"Easy","Easy","Easy","Fun","Tension","Intermediate","Intermediate","Intermediate","Fun","Tension","Intermediate","Tough","Tough","Fun","Tension","Tough","Tough","Tough","Fun","Tension","Critical","Critical","Critical","Fun","Critical"};
        static readonly string[] ShapeNames={"Circle","Oval","Capsule","Rounded block","Diamond","Hexagon","Pebble","Clover","Soft gear","Squircle"};
        static string F(double v)=>v.ToString("0.###",CultureInfo.InvariantCulture);
        static double SignPow(double v,double power)=>Math.Sign(v)*Math.Pow(Math.Abs(v),power);
        public static Point[] Shape(int kind,double x,double y,double rx,double ry)
        {
            const int count=96;var points=new Point[count];
            for(int i=0;i<count;i++)
            {
                double a=i*Math.PI*2/count,px=Math.Cos(a),py=Math.Sin(a),radius=1;
                if(kind==2){px=SignPow(px,.65);py=SignPow(py,.65);}
                if(kind==3||kind==9){px=SignPow(px,kind==3?.5:.7);py=SignPow(py,kind==3?.5:.7);}
                if(kind==4){double r=1/(Math.Abs(px)+Math.Abs(py));px*=r;py*=r;}
                if(kind==5){double segment=Math.PI/3;double r=Math.Cos(Math.PI/6)/Math.Cos((a+Math.PI/6)%segment-Math.PI/6);px*=r;py*=r;}
                if(kind==6)radius=.9+.06*Math.Sin(a*3)+.04*Math.Cos(a*2);
                if(kind==7)radius=.87+.13*Math.Cos(a*4);
                if(kind==8)radius=.93+.07*Math.Cos(a*8);
                points[i]=new Point(x+rx*px*radius,y+ry*py*radius);
            }
            return points;
        }
        static PlatformData Platform(int kind,int index,double x,double y,double rx,double ry)
        {
            var points=Shape(kind,x,y,rx,ry);var commands=new List<PathCommand>();string d="";
            double minX=1e9,minY=1e9,maxX=-1e9,maxY=-1e9;
            for(int i=0;i<points.Length;i++){var p=points[i];minX=Math.Min(minX,p.x);maxX=Math.Max(maxX,p.x);minY=Math.Min(minY,p.y);maxY=Math.Max(maxY,p.y);string op=i==0?"M":"L";d+=op+F(p.x)+" "+F(p.y)+" ";commands.Add(new PathCommand{op=op,v=new[]{p.x,p.y}});}
            commands.Add(new PathCommand{op="Z",v=Array.Empty<double>()});
            return new PlatformData{id="platform-"+(index+1)+"-"+ShapeNames[kind].Replace(" ","-"),outline=points,d=d+"Z",paths=commands.ToArray(),bounds=new BoundsData{minX=minX,minY=minY,maxX=maxX,maxY=maxY}};
        }
        public static void Build()
        {
            var designs=new List<Design>();var characters=CharacterCatalog.Load();int checkedRoutes=0;string root=Path.GetFullPath("../assets/levels/pack-25");Directory.CreateDirectory(root);
            for(int number=1;number<=25;number++)
            {
                string mood=Moods[number-1];double difficulty=mood=="Easy"?.05:mood=="Intermediate"?.35:mood=="Tough"?.65:mood=="Critical"?.95:mood=="Tension"?.72:.25;
                int count=mood=="Fun"?12:mood=="Easy"?12:mood=="Critical"?10:11;
                var platforms=new List<PlatformData>();
                for(int i=0;i<count;i++)
                {
                    double recovery=i==0||i==count-1||i%4==0?1:0;
                    double rx=86-difficulty*34+recovery*12,ry=rx*(i%3==1?.72:1);
                    int kind=number<=3?(i+number)%3:(number*3+i*7)%10;
                    if(kind==1){rx*=1.1;ry*=.68;}if(kind==2){rx*=1.12;ry*=.66;}
                    // Half-size platforms and fewer targets create longer, tighter jumps.
                    rx*=.5;ry*=.5;
                    double spread=mood=="Tension"?110:35+difficulty*63;
                    double x=195+Math.Sin((i+number*.43)*1.55)*spread;
                    x=Math.Clamp(x,rx+31,359-rx);
                    double y=3144-i*(3024.0/(count-1));
                    platforms.Add(Platform(kind,i,x,y,rx,ry));
                }
                var level=new LevelData{id="mektra-level-"+number.ToString("00"),name="Mektra_level_"+number,width=390,height=3264,stride=3264,platforms=platforms.ToArray(),stars=Array.Empty<StarData>(),challengeStars=Array.Empty<StarData>()};
                var routes=new List<Route>();var stars=new List<StarData>();
                for(int i=0;i<count-1;i++)
                {
                    var orbit=new Orbit(level.platforms[i].outline);var candidates=new List<double>();
                    for(int sample=0;sample<240;sample++)
                    {
                        double along=orbit.length*sample/240;orbit.Pose(along,out var p,out var normal);
                        p=Orbit.Ground(p,normal,0,characters[0]);if(normal.y>=-.2)continue;
                        var direction=Geometry.Aim(level,i,p,normal,0);double hit=1e9;
                        var target=level.platforms[i+1].outline;
                        for(int edge=0;edge<target.Length;edge++){double ray=Geometry.Ray(p,direction,target[edge],target[(edge+1)%target.Length]);if(ray>0)hit=Math.Min(hit,ray);}
                        var end=p+direction*hit;
                        if(hit<480&&hit>30&&end.x>20&&end.x<370)candidates.Add(along);
                    }
                    if(candidates.Count==0)throw new Exception("No jump window: "+number+" / "+i);
                    double best=candidates[0],bestCost=1e9;
                    foreach(double along in candidates){orbit.Pose(along,out var p,out var n);double cost=Math.Abs(n.x)*100+Math.Abs(p.x-195)*.03;if(cost<bestCost){bestCost=cost;best=along;}}
                    Point launch=new Point(),landing=new Point(),directionFinal=new Point();
                    foreach(var c in characters)
                    {
                        var sim=new Simulation(level,c);sim.platform=sim.platforms[i];sim.distance=best;sim.Select(c);sim.SetViewport(816);sim.Start();launch=sim.position;sim.Jump();directionFinal=sim.velocity.Normal;
                        for(int step=0;step<180&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);
                        if(sim.flying||sim.phase!=Phase.Playing||sim.platform.index!=i+1)throw new Exception("Route failed: "+number+" / "+i+" / "+c.id);
                        landing=sim.position;checkedRoutes++;
                    }
                    routes.Add(new Route{from=i,to=i+1,distance=best,windowSeconds=candidates.Count/240.0*orbit.length/GameConfig.OrbitSpeed,launch=launch,landing=landing});
                    double flight=(landing-launch).Length;
                    int reward=mood=="Fun"?3:(i%3==0?2:1);
                    for(int star=0;star<reward;star++){double t=reward==1?.48:.25+star*.24;var point=launch+directionFinal*(flight*t);stars.Add(new StarData{id="star-"+(i+1)+"-"+(star+1),x=point.x,y=point.y});}
                }
                level.stars=stars.ToArray();
                // Verify star collection on every authored jump with the final reward coordinates.
                foreach(var route in routes){var sim=new Simulation(level,characters[0]);sim.platform=sim.platforms[route.from];sim.distance=route.distance;sim.Select(characters[0]);sim.SetViewport(816);sim.Start();sim.Jump();for(int step=0;step<180&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);if(sim.score==0)throw new Exception("No point catch on "+number+" / "+route.from);}
                var design=new Design{number=number,title=Titles[number-1],difficulty=mood=="Fun"?"Intermediate":mood=="Tension"?"Tough":mood,mood=mood,theme=ThemeJourney.Names[(number-1)/4%6],points=stars.Count,level=level,routes=routes.ToArray()};designs.Add(design);
                File.WriteAllText(Path.Combine(root,level.id+".json"),JsonUtility.ToJson(design,true));
            }
            File.WriteAllText(Path.Combine(root,"pack.json"),JsonUtility.ToJson(new Pack{levels=designs.ToArray()},true));
            File.WriteAllText(Path.Combine(root,"validation.txt"),"PASS: 25 four-screen levels; "+checkedRoutes+" jump routes across 8 characters; a collectible star on every route.\n");
            Debug.Log("MEKTRA_PACK_SUCCESS "+checkedRoutes);
        }
    }
}
