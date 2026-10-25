using System;
using System.Collections.Generic;
using UnityEngine;
namespace Mektra.Editor
{
    public static class FinalPolishReview
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception("Final polish: "+message);}
        static PlatformData Shift(PlatformData original,double dy)
        {
            var p=JsonUtility.FromJson<PlatformData>(JsonUtility.ToJson(original));
            for(int i=0;i<p.outline.Length;i++)p.outline[i].y+=dy;
            p.bounds.minY+=dy;p.bounds.maxY+=dy;return p;
        }
        public static void Run(){Validate();MektraBuild.BuildInvertPreview();}
        public static void Validate()
        {
            var courses=LevelPack.Load().JourneyCourses();var chars=CharacterCatalog.Load();
            Check(courses.Length==84,"full library missing");Check(courses[0].height==5400,"edited opening missing");
            var seen=new HashSet<int>();double expected=0;int seams=0;
            var sim=new Simulation(courses[0],chars[0],courses,0,true,17);
            for(int section=0;section<=168;section++)
            {
                seen.Add(sim.CourseIndex(section));
                if(section>0)expected-=sim.CourseAt(section).stride;
                Check(Math.Abs(sim.SectionOffset(section)-expected)<.001,"offset at "+section);
                if(section>=84)continue;
                var a=sim.CourseAt(section);var b=sim.CourseAt(section+1);
                var bridge=new LevelData{id="seam-"+section,width=390,height=a.height,stride=a.stride,
                    platforms=new[]{Shift(a.platforms[a.platforms.Length-1],0),Shift(b.platforms[0],-b.stride)},
                    hazards=Array.Empty<PlatformData>(),stars=Array.Empty<StarData>()};
                CourseReview.Review(bridge,0,chars,1);seams++;
            }
            Check(seen.Count==84,"journey omits courses");
            int streak=0;Check(GameConfig.StreakAward(true,ref streak)==5,"first perfect");
            Check(GameConfig.StreakAward(true,ref streak)==10,"second perfect");
            Check(GameConfig.StreakAward(true,ref streak)==15,"third perfect");
            Check(GameConfig.StreakAward(false,ref streak)==0&&streak==0,"streak reset");
            Debug.Log("MEKTRA_FINAL_POLISH_PASS courses="+seen.Count+" seams="+seams+" offsetChecks=169 streak=5/10/15/reset");
        }
    }
}
