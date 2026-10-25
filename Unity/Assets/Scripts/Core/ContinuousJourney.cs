using UnityEngine;
namespace Mektra
{
    // Runtime copies preserve authored assets; scale platforms about their original centres.
    public static class ContinuousJourney
    {
        public static LevelData[] Courses(LevelData[] authored)
        {
            var result=new LevelData[authored.Length];
            for(int i=0;i<result.Length;i++)result[i]=JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(authored[i]));
            for(int i=1;i<result.Length;i++)
            {
                var exit=result[i-1].platforms[^1].bounds;
                var entry=result[i].platforms[0].bounds;
                result[i].stride=entry.maxY-exit.minY+160;
            }
            int courseIndex=0;
            foreach(var course in result)
            {
                var lengths=new double[course.platforms.Length];
                for(int i=0;i<course.platforms.Length;i++){
                    var shape=course.platforms[i];lengths[i]=new Orbit(shape.outline).length;
                    bool bank=course.bounceRuns!=null&&System.Array.Exists(course.bounceRuns,b=>b.entry==i||b.exit==i);
                    OrganicPlatformShapes.Apply(shape,courseIndex*31+i*7,bank);
                    var bounds=shape.bounds;var center=new Point((bounds.minX+bounds.maxX)*.5,(bounds.minY+bounds.maxY)*.5);
                    foreach(var command in shape.paths)if(command.v!=null)for(int n=0;n+1<command.v.Length;n+=2){command.v[n]=center.x+(command.v[n]-center.x)*.65;command.v[n+1]=center.y+(command.v[n+1]-center.y)*.65;}
                    for(int n=0;n<shape.outline.Length;n++)shape.outline[n]=center+(shape.outline[n]-center)*.65;
                    bounds.minX=center.x+(bounds.minX-center.x)*.65;bounds.maxX=center.x+(bounds.maxX-center.x)*.65;
                    bounds.minY=center.y+(bounds.minY-center.y)*.65;bounds.maxY=center.y+(bounds.maxY-center.y)*.65;
                }
                if(course.bounceRuns!=null)foreach(var bounce in course.bounceRuns)bounce.launchDistance*=new Orbit(course.platforms[bounce.entry].outline).length/lengths[bounce.entry];
                courseIndex++;
            }
            return result;
        }
    }
}
