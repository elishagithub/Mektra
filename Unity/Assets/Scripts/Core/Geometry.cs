using System;
namespace Mektra
{
    public static class Geometry
    {
        static readonly int[] AimAngles={0,-3,3,-6,6,-9,9,-12,12,-16,16};
        public struct Nearest
        {
            public double distance,t,along;
            public Point point;
        }
        public static Nearest Segment(Point p,Point a,Point b)
        {
            var d=b-a;
            var t=Math.Clamp(((p.x-a.x)*d.x+(p.y-a.y)*d.y)/Math.Max(d.x*d.x+d.y*d.y,1e-12),0,1);
            var q=a+d*t;
            return new Nearest
            {
                point=q,t=t,distance=(p-q).Length
            }
            ;
        }
        public static bool Inside(Point p,Point[] v)
        {
            bool inside=false;
            for(int i=0,j=v.Length-1;i<v.Length;j=i++)
            {
                var a=v[i];
                var b=v[j];
                if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
            }
            return inside;
        }
        public static double Ray(Point p,Point n,Point a,Point b)
        {
            var s=b-a;
            double den=n.x*s.y-n.y*s.x;
            if(Math.Abs(den)<1e-8)return -1;
            var d=a-p;
            double t=(d.x*s.y-d.y*s.x)/den,u=(d.x*n.y-d.y*n.x)/den;
            return t>0&&u>=0&&u<=1?t:-1;
        }
        public static bool Touches(Point p,PlatformData d,double offset,bool mirrored=false)
        {
            p.y-=offset;
            if(mirrored)p.x=390-p.x;
            var b=d.bounds;
            double r=GameConfig.ContactRadius;
            if(p.x<b.minX-r||p.x>b.maxX+r||p.y<b.minY-r||p.y>b.maxY+r)return false;
            if(Inside(p,d.outline))return true;
            for(int i=0;i<d.outline.Length;i++)if(Segment(p,d.outline[i],d.outline[(i+1)%d.outline.Length]).distance<=r)return true;
            return false;
        }
        public static bool SweptTouches(Point before,Point after,PlatformData data,double offset)
        {
            before.y-=offset;after.y-=offset;
            var bounds=data.bounds;double radius=GameConfig.ContactRadius;
            if(Math.Max(before.x,after.x)<bounds.minX-radius||Math.Min(before.x,after.x)>bounds.maxX+radius||Math.Max(before.y,after.y)<bounds.minY-radius||Math.Min(before.y,after.y)>bounds.maxY+radius)return false;
            if(Inside(before,data.outline)||Inside(after,data.outline))return true;
            var delta=after-before;
            for(int edge=0;edge<data.outline.Length;edge++)
            {
                var a=data.outline[edge];var b=data.outline[(edge+1)%data.outline.Length];
                double hit=Ray(before,delta.Normal,a,b);
                if(hit>0&&hit<=delta.Length)return true;
                if(Segment(before,a,b).distance<=radius||Segment(after,a,b).distance<=radius||Segment(a,before,after).distance<=radius||Segment(b,before,after).distance<=radius)return true;
            }
            return false;
        }
        public static Point Aim(LevelData l,int index,Point p,Point n,double offset,bool mirrored=false,bool nextMirrored=false)
        {
            if(n.y>-.2)return n;
            int next=(index+1)%l.platforms.Length;
            var target=l.platforms[next];
            double targetOffset=offset-(next==0?l.stride:0);
            return AimTarget(target,p,n,targetOffset,next==0?nextMirrored:mirrored);
        }
        public static Point AimTarget(PlatformData target,Point p,Point n,double targetOffset,bool mirrored,Point motion=default)
        {
            if(n.y>-.2)return n;
            foreach(int degrees in AimAngles)
            {
                double a=degrees*Math.PI/180;
                var dir=new Point(n.x*Math.Cos(a)-n.y*Math.Sin(a),n.x*Math.Sin(a)+n.y*Math.Cos(a));
                for(int j=0;j<target.outline.Length;j++)
                {
                    var v=target.outline[j];
                    var w=target.outline[(j+1)%target.outline.Length];
                    if(mirrored){v.x=390-v.x;w.x=390-w.x;}
                    v+=motion;w+=motion;
                    v.y+=targetOffset;
                    w.y+=targetOffset;
                    double hit=Ray(p,dir,v,w);
                    if(hit>0&&hit<480)return dir;
                }
            }
            return n;
        }
    }
    public class Orbit
    {
        public Point[] points,normals;
        public double[] cumulative;
        public double length;
        public Orbit(Point[] outline,double radius=GameConfig.Radius)
        {
            int n=outline.Length;
            points=new Point[n];
            normals=new Point[n];
            cumulative=new double[n+1];
            for(int i=0;i<n;i++)
            {
                var a=outline[(i-3+n)%n];
                var b=outline[(i+3)%n];
                normals[i]=new Point(b.y-a.y,a.x-b.x).Normal;
                points[i]=outline[i]+normals[i]*radius;
            }
            for(int i=0;i<n;i++)cumulative[i+1]=cumulative[i]+(points[(i+1)%n]-points[i]).Length;
            length=cumulative[n];
        }
        public void Pose(double distance,out Point p,out Point n)
        {
            double d=(distance%length+length)%length;
            int lo=0,hi=points.Length-1;
            while(lo<hi)
            {
                int mid=(lo+hi+1)/2;
                if(cumulative[mid]<=d)lo=mid;
                else hi=mid-1;
            }
            int j=(lo+1)%points.Length;
            double t=(d-cumulative[lo])/Math.Max(cumulative[lo+1]-cumulative[lo],1e-12);
            p=points[lo]+(points[j]-points[lo])*t;
            n=(normals[lo]+(normals[j]-normals[lo])*t).Normal;
        }
        public double Nearest(Point p)
        {
            double best=double.PositiveInfinity,along=0;
            for(int i=0;i<points.Length;i++)
            {
                var q=Geometry.Segment(p,points[i],points[(i+1)%points.Length]);
                if(q.distance<best)
                {
                    best=q.distance;
                    along=cumulative[i]+q.t*(cumulative[i+1]-cumulative[i]);
                }
            }
            return along;
        }
        // Place the inward-most tooth on the shape; the upright shell never rotates.
        public static Point Ground(Point p,Point n,double angle,Character c)
        {
            double pitch=2*Math.PI/c.legCount,inward=Math.Atan2(-n.y,-n.x),support;
            if(c.legStyle=="paddle")
            {
                double left=angle-.06+Math.Round((inward-angle+.06)/pitch,MidpointRounding.AwayFromZero)*pitch;
                double right=angle+.06+Math.Round((inward-angle-.06)/pitch,MidpointRounding.AwayFromZero)*pitch;
                support=GameConfig.Radius*Math.Max(Math.Cos(left-inward),Math.Cos(right-inward));
            }
            else
            {
                angle+=c.legStyle=="blade"?.045:0;
                double tooth=angle+Math.Round((inward-angle)/pitch,MidpointRounding.AwayFromZero)*pitch;
                support=GameConfig.Radius*Math.Cos(tooth-inward);
            }
            return p+n*(support-GameConfig.Radius);
        }
    }
}
