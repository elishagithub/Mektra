using System;
using UnityEngine;
namespace Mektra
{
    [Serializable] public class AuthoredRoute
    {
        public int from,to;public double distance,windowSeconds;public Point launch,landing;
    }
    [Serializable] public class AuthoredCourse
    {
        public int number,points;public string title,difficulty,mood,theme;public LevelData level;public AuthoredRoute[] routes;
    }
    [Serializable] public class LevelPack
    {
        public AuthoredCourse[] levels;
        public bool challengeOpening,endless;
        public static LevelPack Load()=>JsonUtility.FromJson<LevelPack>(Resources.Load<TextAsset>("endless-pack").text);
        public LevelData[] Courses()
        {
            var result=new LevelData[levels.Length];
            for(int i=0;i<result.Length;i++){result[i]=levels[i].level;result[i].name=levels[i].title;}
            return result;
        }
    }
}
