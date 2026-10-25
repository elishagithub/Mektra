using System;
using UnityEngine;
namespace Mektra
{
    [Serializable] public struct Point
    {
        public double x,y;
        public Point(double x,double y)
        {
            this.x=x;
            this.y=y;
        }
        public static Point operator +(Point a,Point b)=>new Point(a.x+b.x,a.y+b.y);
        public static Point operator -(Point a,Point b)=>new Point(a.x-b.x,a.y-b.y);
        public static Point operator *(Point a,double n)=>new Point(a.x*n,a.y*n);
        public double Length=>Math.Sqrt(x*x+y*y);
        public Point Normal=>this*(1/Math.Max(Length,1e-12));
        public Vector2 V=>new Vector2((float)x,(float)y);
    }
    [Serializable] public class BoundsData
    {
        public double minX,maxX,minY,maxY;
    }
    [Serializable] public class PathCommand
    {
        public string op;
        public double[] v;
    }
    [Serializable] public class MotionData
    {
        public string type="static";
        public double amplitudeX,amplitudeY,periodSeconds=6,phase,speedVariation;
        // Sine motion has continuous velocity at the turnarounds; phase is in cycles.
        public Point At(double time)
        {
            if(type=="static"||periodSeconds<=0)return new Point();
            if(type=="stream"){double cycle=time/periodSeconds+phase;cycle-=Math.Floor(cycle);return new Point(-amplitudeX*cycle,amplitudeY*cycle);}
            if(type=="rain")
            {
                // Capsules in a row share a clock; separate row periods create independent speeds.
                // Recycling happens beyond both screen edges.
                double clock=time/periodSeconds;double travel=clock+Math.Clamp(speedVariation,0,.9)*Math.Sin(2*Math.PI*clock)/(2*Math.PI);
                double cycle=travel+phase;cycle-=Math.Floor(cycle);double shift=cycle-phase;
                return new Point(-amplitudeX*shift,amplitudeY*shift);
            }
            double angle=2*Math.PI*(time/periodSeconds+phase);
            if(type=="figure-eight")return new Point(amplitudeX*Math.Sin(angle),amplitudeY*Math.Sin(2*angle));
            return new Point(type=="vertical"?0:amplitudeX*Math.Sin(angle),
                type=="horizontal"?0:amplitudeY*(type=="orbit"?Math.Cos(angle):Math.Sin(angle)));
        }
    }
    [Serializable] public class RotationData
    {
        public bool enabled,continuous;
        public double amplitudeRadians=.35,periodSeconds=16,phase;
        public double At(double time)=>!enabled||periodSeconds<=0?0:continuous?2*Math.PI*(time/periodSeconds+phase):amplitudeRadians*Math.Sin(2*Math.PI*(time/periodSeconds+phase));
    }
    [Serializable] public class PlatformData
    {
        public int orbitDirection=1;
        public string id,d,sourceShapeId;
        public Point[] outline;
        public BoundsData bounds;
        public PathCommand[] paths;
        public MotionData motion;
        public double crumbleSeconds;
        public string label;
        public RotationData rotation;
        public bool switchSupport;
        public string openedBy;
    }
    [Serializable] public class BlackHoleData { public string id; public double x,y,radius=23,pullRadius=53; }
    [Serializable] public class BounceRunData { public int entry,exit; public int minimumBounces=1; public double launchDistance; public bool livePreview; public Point[] points; public double secondsPerLeg=.32; }
    [Serializable] public class RouteLink { public int from,to; }
    [Serializable] public class StarData
    {
        public string id;
        public double x,y;
        public string parentPlatformId;
        public int guideFrom=-1,guideTo=-1;public double guideFraction=.5;
    }
    [Serializable] public class LevelData
    {
        public bool discovery,sparseRewards,streamingRain;
        public string id,name,source;
        public string worldName,stageName;
        public string instruction;
        public bool suppressMagnets;
        public RouteLink[] connections;
        public int worldNumber,worldStage;
        public int segmentKind,variant;
        public double width,height,stride,authoredRadius;
        public PlatformData[] platforms;
        public PlatformData[] hazards;
        public StarData[] stars,challengeStars;
        public BlackHoleData[] blackHoles;
        public BounceRunData[] bounceRuns;
        public int[] checkpointPlatforms;
        public string[] chapterTitles;
        public static LevelData Load()=>JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("level01").text);
    }
    [Serializable] public class Character
    {
        public string id,name,tagline,color,eyeColor,faceColor,shellStroke,shape,eyeStyle,legStyle;
        public int cost,legCount;
        public PathCommand[] shellPath;
    }
    [Serializable] public class CharacterCatalog
    {
        public Character[] characters;
        public static Character[] Load()=>JsonUtility.FromJson<CharacterCatalog>(Resources.Load<TextAsset>("characters").text).characters;
    }
    public static class GameConfig
    {
        public const double GuideHoldSeconds=20,GuideFadeSeconds=5,GuideEndSeconds=GuideHoldSeconds+GuideFadeSeconds;
        public static float GuideOpacity(double time){
#if MEKTRA_INVERT_PREVIEW
            return 1;
#else
            return (float)Math.Clamp((GuideEndSeconds-time)/GuideFadeSeconds,0,1);
#endif
        }
#if MEKTRA_INVERT_PREVIEW
        public const double ContactRadius=3.375;
#else
        public const double ContactRadius=6.06436875;
#endif
        // Pickups touch the visible circle and solid dot core, excluding the glow halo.
        public const double CollectibleRadius=2.2,PickupRadius=Radius+CollectibleRadius;
        // Hold the authored starting pose until both guides vanish, then ease the motion clock in.
        public static double GuidedMotionTime(double time)
        {
            double elapsed=Math.Max(0,time-GuideEndSeconds);
            return elapsed-1+Math.Exp(-elapsed);
        }
        public static int StreakAward(bool close,ref int streak){streak=close?streak+1:0;return streak*5;}
        public static int PrecisionPoints(double angleDegrees,double miss)=>angleDegrees<=4&&miss<=12?5:angleDegrees<=10&&miss<=20?3:angleDegrees<=18&&miss<=28?1:0;
        public const float ViewZoom=1.08f;
        public const double Width=390,Radius=8.085825,OrbitSpeed=69.7,JumpSpeed=734.4,Step=1.0/120,MaxDelta=.05,CollectionRadius=PickupRadius,CameraAnchor=.7;
    }
}
