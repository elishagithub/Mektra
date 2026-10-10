using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class LevelImportValidator
    {
        [Serializable] class Candidate {public bool challengeOpening,endless;public int screenHeight=816,screensPerLevel=4;public AuthoredCourse[] levels;public int[] updatedLevels;}
        public static void Validate()
        {
            var args=Environment.GetCommandLineArgs();int option=Array.IndexOf(args,"-mektraCandidate");
            if(option<0||option+1>=args.Length)throw new Exception("Missing -mektraCandidate file");
            string file=args[option+1];var candidate=JsonUtility.FromJson<Candidate>(File.ReadAllText(file));
            var pack=new LevelPack{challengeOpening=candidate.challengeOpening,endless=candidate.endless,levels=candidate.levels};var courses=pack.Courses();var characters=CharacterCatalog.Load();
            foreach(int number in candidate.updatedLevels)
            {
                int section=Array.FindIndex(candidate.levels,c=>c.number==number);var course=candidate.levels[section];
                MotionRegression.GeometryChecks(course.level);var reachable=new HashSet<string>();var routes=new List<AuthoredRoute>();
                if(pack.challengeOpening||pack.endless)
                {
                    var hazards=course.level.hazards;course.level.hazards=Array.Empty<PlatformData>();
                    try
                    {
                        foreach(var edge in course.level.connections)
                        {
                            if(course.level.platforms[edge.from].sourceShapeId=="wedge"||course.level.bounceRuns!=null&&Array.Exists(course.level.bounceRuns,r=>r.entry==edge.from))continue; // Outgoing wedge links are automatic ricochets, validated by ExperienceRegression.
                            var review=CourseReview.Review(course.level,edge.from,characters,edge.to);
                            routes.Add(new AuthoredRoute{from=edge.from,to=edge.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                        }
                    }
                    finally {course.level.hazards=hazards;}
                    course.level.authoredRadius=GameConfig.Radius;course.routes=routes.ToArray();continue;
                }
                for(int i=0;i<course.level.platforms.Length-1;i++)
                {
                    double distance=MotionRegression.FindRoute(courses,characters,section,i,0,reachable);
                    var sim=MotionRegression.Launch(courses,characters[0],section,i,distance,0);
                    var start=new Simulation(courses[0],characters[0],courses);start.EnsureSection(section);start.platform=start.platforms.Find(p=>p.section==section&&p.index==i);start.distance=distance;start.Select(characters[0]);
                    routes.Add(new AuthoredRoute{from=i,to=i+1,distance=distance,launch=start.position,landing=sim.position});
                }
                foreach(var star in course.level.stars)if(!reachable.Contains(star.id))throw new Exception("Unreachable placed star in level "+number+": "+star.id);
                course.routes=routes.ToArray();
            }
            if(pack.endless)EndlessRegression.Run(pack);
            else if(pack.challengeOpening)ChallengeRegression.Run(pack);
            else {LevelPackRegression.Run(pack);MotionRegression.Run(pack);if(pack.levels[0].level.worldNumber>0)WorldRegression.Run(pack);}
            File.WriteAllText(file,JsonUtility.ToJson(candidate,true));
            Debug.Log("MEKTRA_IMPORT_SUCCESS: staged levels validated; active levels unchanged until importer promotion.");
        }
    }
}
