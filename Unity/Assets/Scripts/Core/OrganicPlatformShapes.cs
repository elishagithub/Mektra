using System;
namespace Mektra
{
    // Closed, smooth silhouettes keep the existing footprint and gameplay metadata.
    public static class OrganicPlatformShapes
    {
        public static bool IsRound(PlatformData shape)
        {
            if(shape.sourceShapeId=="circle")return true;
            var b=shape.bounds;double rx=(b.maxX-b.minX)*.5,ry=(b.maxY-b.minY)*.5;
            if(Math.Abs(rx-ry)>Math.Max(rx,ry)*.015||shape.outline.Length<24)return false;
            var center=new Point((b.minX+b.maxX)*.5,(b.minY+b.maxY)*.5);
            foreach(var point in shape.outline)if(Math.Abs((point-center).Length-rx)>rx*.015)return false;
            return true;
        }
        public static void Apply(PlatformData shape,int seed,bool measuredLaunch)
        {
            if(!IsRound(shape))return;
            var b=shape.bounds;double cx=(b.minX+b.maxX)*.5,cy=(b.minY+b.maxY)*.5;
            double rx=(b.maxX-b.minX)*.5,ry=(b.maxY-b.minY)*.5;
            int family=seed%3,lobes=!measuredLaunch&&family==0?2:3;
            double amplitude=measuredLaunch?.035:family==0?.105:family==1?.07:.045;
            double turn=seed*.61803398875;var points=new Point[144];
            double minX=double.PositiveInfinity,maxX=double.NegativeInfinity,minY=minX,maxY=maxX;
            for(int i=0;i<points.Length;i++){
                double angle=2*Math.PI*i/points.Length;
                double radius=1+amplitude*Math.Cos(lobes*angle+turn)+amplitude*.32*Math.Sin(2*angle-turn*.7)+amplitude*.38*Math.Sin(3*angle+turn*.53);
                points[i]=new Point(rx*radius*Math.Cos(angle),ry*radius*Math.Sin(angle));
                minX=Math.Min(minX,points[i].x);maxX=Math.Max(maxX,points[i].x);minY=Math.Min(minY,points[i].y);maxY=Math.Max(maxY,points[i].y);
            }
            // Normalize into the same bounding box: no hidden enlargement or moved centres.
            var paths=new PathCommand[points.Length+1];
            for(int i=0;i<points.Length;i++){
                points[i]=new Point(cx-rx+(points[i].x-minX)/(maxX-minX)*rx*2,cy-ry+(points[i].y-minY)/(maxY-minY)*ry*2);
                paths[i]=new PathCommand{op=i==0?"M":"L",v=new[]{points[i].x,points[i].y}};
            }
            paths[^1]=new PathCommand{op="Z",v=Array.Empty<double>()};shape.outline=points;shape.paths=paths;
            shape.sourceShapeId=measuredLaunch||family==2?"organic-oval":family==0?"organic-two-lobe":"organic-three-lobe";
        }
    }
}
