using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Mektra.Editor
{
    // Cache authoring searches only. The complete physical regression suite still
    // runs on every build, including every route loaded from this cache.
    public static class CampaignAuthorCache
    {
        static string physics;
        public static string Key(LevelData level)
        {
            if(physics==null)physics=string.Join("\n",Directory.GetFiles("Assets/Scripts/Core","*.cs").OrderBy(p=>p).Select(File.ReadAllText));
            using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(level)+physics+(level.bounceRuns!=null&&Array.Exists(level.bounceRuns,b=>b.minimumBounces>1)?"double-bank-v2":"")))).Replace("-","").ToLowerInvariant();
        }
        static string PathFor(string key)=>"/private/tmp/mektra-campaign-author-cache/"+key+".json";
        public static AuthoredCourse Read(string key)
        {
            string path=PathFor(key);if(!File.Exists(path))return null;try{var course=JsonUtility.FromJson<AuthoredCourse>(File.ReadAllText(path));foreach(var route in course.routes){bool bonus=course.level.platforms[route.from].label=="bonus"||course.level.platforms[route.to].label=="bonus";double required=bonus?CampaignShapeBalance.BonusWindow(course.number-1):CampaignShapeBalance.MainWindow(course.number-1);if(route.windowSeconds<required)return null;}return course;}catch{return null;}
        }
        public static void Write(string key,AuthoredCourse course){Directory.CreateDirectory("/private/tmp/mektra-campaign-author-cache");File.WriteAllText(PathFor(key),JsonUtility.ToJson(course));}
    }
}
