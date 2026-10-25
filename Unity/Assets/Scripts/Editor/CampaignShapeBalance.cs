using System;
using UnityEngine;
namespace Mektra.Editor
{
    // Geometry-only tuning: keep the authored routes, motion, hazards and saves.
    public static class CampaignShapeBalance
    {
        public static double MainWindow(int index)=>index<55?.25:index<77?.22:.18;
        public static double BonusWindow(int index)=>index<55?.20:index<77?.18:.16;
        public static void Apply(LevelData level,int index)
        {
            var longJumps=LengthenDiagonals(level,index);
            double progression=Math.Clamp((index-33)/65.0,0,1);
            for(int i=0;i<level.platforms.Length;i++)
            {
                var old=level.platforms[i];
                if(old.sourceShapeId=="wedge"||old.motion?.type=="rain"||old.sourceShapeId=="rain-capsule")continue;
                bool checkpoint=Array.IndexOf(level.checkpointPlatforms??Array.Empty<int>(),i)>=0;
                bool goal=i==level.platforms.Length-1;
                bool bank=level.bounceRuns!=null&&Array.Exists(level.bounceRuns,b=>b.entry==i||b.exit==i);
                // Intro studies create rebound metadata later. Their bank entries
                // must retain the measured departure geometry as well.
                bank|=(index==34&&(i==0||i==3))||(index==43&&i==11);
                if(bank||goal||i==0)continue;
                int pattern=(index*3+i*5)%7;
                bool circle=old.sourceShapeId=="circle";
                if(checkpoint)
                {
                    if(circle&&i%2==1)Replace(level,i,1,3,.035,(index+i)*.37,"distorted-circle");
                    continue;
                }
                bool bonus=old.label=="bonus";
                double size=1-.16*progression;
                double[] variation={1,.96,.90,1,.94,.98,.92};
                size*=variation[(index+i)%variation.Length];
                if(longJumps.Contains(i))size=Math.Max(size,.98-.04*progression);
                double cx=(old.bounds.minX+old.bounds.maxX)*.5,cy=(old.bounds.minY+old.bounds.maxY)*.5;
                double radius=0;foreach(var p in old.outline)radius=Math.Max(radius,(p-new Point(cx,cy)).Length);
                bool ferryLanding=bonus&&Array.Exists(level.connections,l=>l.to==i&&level.platforms[l.from].sourceShapeId=="rain-capsule"&&level.platforms[l.from].motion!=null&&level.platforms[l.from].motion.type!="static");
                size=Math.Min(1,Math.Max(size,(ferryLanding?42:bonus?38:44)/Math.Max(1,radius)));
                // Reserve circles deliberately; other landings mix two/three lobes
                // and an asymmetric closed curve, with no spikes or C shapes.
                bool firstLongLaunch=i==1&&longJumps.Contains(i)&&Array.Exists(level.connections,l=>l.from==i&&longJumps.Contains(l.to));
                if(circle&&pattern<5&&!firstLongLaunch&&!ferryLanding)
                {
                    int lobes=pattern==0||pattern==3?2:3;
                    string family=pattern==2?"distorted-circle":lobes==2?"two-lobe":"three-lobe";
                    Replace(level,i,size,lobes,pattern==2?.075:lobes==2?.18:.11,(index+i)*.37,family);
                }
                else Scale(level,i,size);
            }
        }
        static System.Collections.Generic.HashSet<int> LengthenDiagonals(LevelData level,int index)
        {
            var endpoints=new System.Collections.Generic.HashSet<int>();
            if(index<55)return endpoints;
            var moved=new System.Collections.Generic.HashSet<int>();
            foreach(var link in level.connections)
            {
                var a=level.platforms[link.from];var b=level.platforms[link.to];
                if(moved.Contains(link.to)||b.label=="bonus"||a.label=="bonus"||link.to==level.platforms.Length-1||Array.IndexOf(level.checkpointPlatforms??Array.Empty<int>(),link.to)>=0)continue;
                if(a.sourceShapeId=="wedge"||b.sourceShapeId=="wedge"||a.sourceShapeId=="rain-capsule"||b.sourceShapeId=="rain-capsule"||b.motion!=null&&b.motion.type!="static")continue;
                if(level.bounceRuns!=null&&Array.Exists(level.bounceRuns,r=>r.entry==link.from||r.exit==link.from||r.entry==link.to||r.exit==link.to))continue;
                double ax=(a.bounds.minX+a.bounds.maxX)*.5,ay=(a.bounds.minY+a.bounds.maxY)*.5,bx=(b.bounds.minX+b.bounds.maxX)*.5,by=(b.bounds.minY+b.bounds.maxY)*.5;
                if(Math.Abs(bx-ax)<120||ay-by<220)continue;
                double rise=25+25*Math.Clamp((index-55)/43.0,0,1);
                double horizontal=Math.Sign(bx-ax)*Math.Min(10,Math.Max(0,Math.Min(b.bounds.minX-24,366-b.bounds.maxX)));
                var points=new Point[b.outline.Length];for(int j=0;j<points.Length;j++)points[j]=b.outline[j]+new Point(horizontal,-rise);
                level.platforms[link.to]=WithGeometry(b,points,b.sourceShapeId);moved.Add(link.to);endpoints.Add(link.from);endpoints.Add(link.to);
            }
            return endpoints;
        }
        static void Replace(LevelData level,int index,double size,int lobes,double amplitude,double turn,string family)
        {
            var old=level.platforms[index];double x=(old.bounds.minX+old.bounds.maxX)*.5,y=(old.bounds.minY+old.bounds.maxY)*.5;
            double rx=(old.bounds.maxX-old.bounds.minX)*.5*size,ry=(old.bounds.maxY-old.bounds.minY)*.5*size;
            var points=new Point[144];
            for(int j=0;j<points.Length;j++)
            {
                double a=j*2*Math.PI/points.Length;
                double r=1-amplitude+amplitude*Math.Cos(lobes*(a+turn));
                if(family=="distorted-circle")r-=amplitude*.45*(1+Math.Sin(2*a+turn));
                points[j]=new Point(x+rx*r*Math.Cos(a),y+ry*r*Math.Sin(a));
            }
            level.platforms[index]=WithGeometry(old,points,family);
        }
        static void Scale(LevelData level,int index,double size)
        {
            var old=level.platforms[index];var centre=new Point((old.bounds.minX+old.bounds.maxX)*.5,(old.bounds.minY+old.bounds.maxY)*.5);var points=new Point[old.outline.Length];
            for(int j=0;j<points.Length;j++)points[j]=centre+(old.outline[j]-centre)*size;
            level.platforms[index]=WithGeometry(old,points,old.sourceShapeId);
        }
        static PlatformData WithGeometry(PlatformData old,Point[] points,string family)
        {
            var shape=WorldPackBuilder.Polygon(old.id,points,family);shape.motion=old.motion;shape.rotation=old.rotation;shape.label=old.label;shape.switchSupport=old.switchSupport;shape.openedBy=old.openedBy;shape.crumbleSeconds=old.crumbleSeconds;shape.orbitDirection=old.orbitDirection;return shape;
        }
    }
}
