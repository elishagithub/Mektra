using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    public static class CrewRegression
    {
        public static void ValidateAndBuild()
        {
            var chars=CharacterCatalog.Load();var ids=new HashSet<string>();var legs=new HashSet<string>();
            foreach(var character in chars){if(!ids.Add(character.id))throw new Exception("Duplicate character");legs.Add(character.legStyle);}
            if(chars.Length<10||legs.Count<10)throw new Exception("Expected ten characters with distinct legs");
            var pack=LevelPack.Load();var courses=pack.Courses();int checks=0;
            foreach(var course in pack.levels)foreach(var route in course.routes)foreach(var character in chars)
            {
                bool safe=false;
                for(int phase=0;phase<24&&!safe;phase++)
                {
                    var sim=MotionRegression.Launch(courses,character,course.number-1,route.from,CourseReview.RouteDistance(course.level,route.from,route.distance),phase*.2);
                    safe=(sim.phase==Phase.Playing||sim.phase==Phase.Complete)&&!sim.flying&&sim.platform.section==course.number-1&&sim.platform.index==route.to;
                }
                if(!safe)throw new Exception("Crew route: "+character.id+" / "+course.level.id+" / "+route.from+" -> "+route.to);
                checks++;
            }
            Debug.Log("MEKTRA_CREW_VALIDATED: "+chars.Length+" characters; "+legs.Count+" distinct leg styles; "+checks+" authored route checks.");
            MektraBuild.BuildCharacterPreview();
        }
    }
}
