using System;
using UnityEngine;
namespace Mektra
{
    [Serializable] public class AuthoredRoute
    {
        public int from,to;public double distance,windowSeconds,clock;public Point launch,landing;
    }
    [Serializable] public class AuthoredCourse
    {
        public int number,points;public string title,difficulty,mood,theme;public LevelData level;public AuthoredRoute[] routes;
    }
    [Serializable] public class LevelPack
    {
        public AuthoredCourse[] levels;
        public bool challengeOpening,endless;
        public static LevelPack Load()
        {
            var prototype=Resources.Load<TextAsset>("discovery-pack");
            return JsonUtility.FromJson<LevelPack>((prototype??Resources.Load<TextAsset>("endless-pack")).text);
        }
        // Preserve the edited opening without limiting the rest of the journey.
        public LevelData[] JourneyCourses()
        {
            var result=Courses();var opening=Resources.Load<TextAsset>("figma-four-test");
            if(!endless||opening==null)return result;
            var edited=JsonUtility.FromJson<LevelPack>(opening.text).Courses();
            for(int i=0;i<Math.Min(4,Math.Min(edited.Length,result.Length));i++)
            {
                edited[i].segmentKind=result[i].segmentKind;edited[i].variant=result[i].variant;
                result[i]=edited[i];
            }
            return result;
        }
        public LevelData[] Courses()
        {
            var result=new LevelData[levels.Length];
            for(int i=0;i<result.Length;i++){result[i]=levels[i].level;result[i].name=levels[i].title;}
            return result;
        }
    }
}
