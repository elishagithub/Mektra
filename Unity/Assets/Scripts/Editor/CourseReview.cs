using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Mektra.Editor
{
    public static class CourseReview
    {
        [Serializable] public class JumpReview {public int from,to;public double distance,windowSeconds;public Point launch,landing;}
        [Serializable] public class CourseReport {public int number;public string title,difficulty;public double minimumWindow,averageWindow;public JumpReview[] jumps;}
        [Serializable] public class Report {public string method="360 orbit samples; contiguous landing window shared by all eight characters; fixed-speed perpendicular jumps; magnets disabled during checks";public CourseReport[] levels;}
        public static double RouteDistance(LevelData level,int from,double distance)
        {
            double reference=level.authoredRadius>0?level.authoredRadius:8.91;
            if(Math.Abs(reference-GameConfig.Radius)<1e-10)return distance;
            var old=new Orbit(level.platforms[from].outline,reference);var current=new Orbit(level.platforms[from].outline);
            double d=(distance%old.length+old.length)%old.length;int edge=0;
            while(edge+1<old.points.Length&&old.cumulative[edge+1]<=d)edge++;
            double fraction=(d-old.cumulative[edge])/(old.cumulative[edge+1]-old.cumulative[edge]);
            return current.cumulative[edge]+fraction*(current.cumulative[edge+1]-current.cumulative[edge]);
        }
        static Simulation Trial(LevelData level,Character c,int from,double distance)
        {
            var sim=new Simulation(level,c);sim.magnets.Clear();sim.platform=sim.platforms[from];sim.distance=distance;sim.Select(c);sim.SetViewport(816);sim.Start();sim.Jump();
            for(int step=0;step<240&&sim.flying&&sim.phase==Phase.Playing;step++)sim.Advance(GameConfig.Step);
            return sim;
        }
        static bool Arrived(Simulation sim,int to)=>sim.phase==Phase.Playing&&!sim.flying&&sim.platform.section==0&&sim.platform.index==to;
        public static JumpReview Review(LevelData level,int from,Character[] characters,int to=-1)
        {
            if(to<0)to=from+1;
            const int samples=360;var orbit=new Orbit(level.platforms[from].outline);var target=level.platforms[to];
            var safe=new bool[samples];
            for(int sample=0;sample<samples;sample++)
            {
                double distance=sample*orbit.length/samples;orbit.Pose(distance,out var p,out var n);var axis=new Point((target.bounds.minX+target.bounds.maxX)*.5-p.x,(target.bounds.minY+target.bounds.maxY)*.5-p.y);if(n.x*axis.x+n.y*axis.y<=0)continue;
                bool ray=false;
                for(int edge=0;edge<target.outline.Length;edge++)if(Geometry.Ray(p,n,target.outline[edge],target.outline[(edge+1)%target.outline.Length])>0){ray=true;break;}
                if(!ray)continue;
                bool valid=true;foreach(var c in characters)if(!Arrived(Trial(level,c,from,distance),to)){valid=false;break;}
                safe[sample]=valid;
            }
            int longest=0,bestStart=0,run=0;
            for(int sample=0;sample<samples*2;sample++)
            {
                run=safe[sample%samples]?Math.Min(samples,run+1):0;
                if(run>longest){longest=run;bestStart=sample-run+1;}
            }
            if(longest==0)throw new Exception("No shared landing window: "+level.id+" / "+from);
            double chosen=((bestStart+(longest-1)/2)%samples)*orbit.length/samples;
            var start=new Simulation(level,characters[0]);start.platform=start.platforms[from];start.distance=chosen;start.Select(characters[0]);
            var landed=Trial(level,characters[0],from,chosen);
            return new JumpReview{from=from,to=to,distance=chosen,windowSeconds=longest*orbit.length/samples/GameConfig.OrbitSpeed,launch=start.position,landing=landed.position};
        }
        static double Goal(AuthoredCourse course)=>course.number<=2?.30:course.mood=="Fun"?.23:course.mood=="Intermediate"?.21:course.mood=="Critical"?.17:.19;
        static double GeometricWindow(LevelData level,int from)
        {
            const int samples=360;var orbit=new Orbit(level.platforms[from].outline);var target=level.platforms[from+1];var safe=new bool[samples];
            for(int i=0;i<samples;i++)
            {
                orbit.Pose(i*orbit.length/samples,out var p,out var normal);if(normal.y>=-.1)continue;
                for(int edge=0;edge<target.outline.Length;edge++)
                {
                    double hit=Geometry.Ray(p,normal,target.outline[edge],target.outline[(edge+1)%target.outline.Length]);
                    if(hit>20&&hit<480){safe[i]=true;break;}
                }
            }
            int longest=0,run=0;
            for(int i=0;i<samples*2;i++){run=safe[i%samples]?Math.Min(samples,run+1):0;longest=Math.Max(longest,run);}
            return longest*orbit.length/samples/GameConfig.OrbitSpeed;
        }
        static void Shift(PlatformData platform,double dx)
        {
            for(int i=0;i<platform.outline.Length;i++)platform.outline[i].x+=dx;
            foreach(var command in platform.paths)for(int i=0;i<command.v.Length;i+=2)command.v[i]+=dx;
            platform.bounds.minX+=dx;platform.bounds.maxX+=dx;
            var path=new System.Text.StringBuilder();
            foreach(var command in platform.paths){path.Append(command.op);foreach(var value in command.v)path.Append(value.ToString("0.#####",System.Globalization.CultureInfo.InvariantCulture)).Append(' ');}
            platform.d=path.ToString();
        }
        static double Quality(double a,double b,double goal)=>Math.Min(a,goal)+Math.Min(b,goal)+Math.Min(a,b)*.5;
        public static void Improve()
        {
            var pack=LevelPack.Load();var characters=CharacterCatalog.Load();var reports=new List<CourseReport>();
            string folder="../assets/levels/design-review";Directory.CreateDirectory(folder);
            string backup=folder+"/pack-before.json";if(!File.Exists(backup))File.WriteAllText(backup,JsonUtility.ToJson(pack,true));
            foreach(var course in pack.levels)
            {
                var level=course.level;var reviews=new JumpReview[level.platforms.Length-1];
                for(int from=0;from<reviews.Length;from++)reviews[from]=Review(level,from,characters);
                double goal=Goal(course);int shifted=0;
                for(int index=1;index+1<level.platforms.Length;index++)
                {
                    double beforeA=reviews[index-1].windowSeconds,beforeB=reviews[index].windowSeconds;
                    if(beforeA>=goal&&beforeB>=goal)continue;
                    var original=JsonUtility.ToJson(level.platforms[index]);var platform=level.platforms[index];
                    double baseline=Quality(GeometricWindow(level,index-1),GeometricWindow(level,index),goal),best=baseline,bestDx=0;
                    foreach(double dx in new[]{-48.0,-36,-24,-12,12,24,36,48})
                    {
                        if(platform.bounds.minX+dx<25||platform.bounds.maxX+dx>365)continue;
                        Shift(platform,dx);
                        double quality=Quality(GeometricWindow(level,index-1),GeometricWindow(level,index),goal)-Math.Abs(dx)*.00015;
                        Shift(platform,-dx);
                        if(quality>best+.005){best=quality;bestDx=dx;}
                    }
                    if(bestDx==0)continue;
                    Shift(platform,bestDx);
                    var a=Review(level,index-1,characters);var b=Review(level,index,characters);
                    if(a.windowSeconds+.005<beforeA||b.windowSeconds+.005<beforeB||Quality(a.windowSeconds,b.windowSeconds,goal)<Quality(beforeA,beforeB,goal)+.008)
                        level.platforms[index]=JsonUtility.FromJson<PlatformData>(original);
                    else{reviews[index-1]=a;reviews[index]=b;shifted++;}
                }
                var routes=new List<AuthoredRoute>();double minimum=double.MaxValue,total=0;
                foreach(var review in reviews)
                {
                    var start=new Simulation(level,characters[0]);start.platform=start.platforms[review.from];start.distance=review.distance;start.Select(characters[0]);
                    var direction=start.JumpDirection();double length=(review.landing-review.launch).Length;
                    var lane=new List<StarData>();foreach(var star in level.stars)if(star.id.StartsWith("star-"+(review.from+1)+"-",StringComparison.Ordinal))lane.Add(star);
                    lane.Sort((a,b)=>string.CompareOrdinal(a.id,b.id));
                    for(int point=0;point<lane.Count;point++)
                    {
                        double fraction=lane.Count==1?.48:.32+point*.34/(lane.Count-1);
                        var position=review.launch+direction*(length*fraction);
                        if(position.x<25||position.x>365)throw new Exception("Reward lane enters wall");
                        lane[point].x=position.x;lane[point].y=position.y;
                    }
                    routes.Add(new AuthoredRoute{from=review.from,to=review.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});
                    minimum=Math.Min(minimum,review.windowSeconds);total+=review.windowSeconds;
                }
                course.routes=routes.ToArray();
                MotionRegression.GeometryChecks(level);
                foreach(var route in course.routes)foreach(var c in characters)
                {
                    var sim=Trial(level,c,route.from,route.distance);
                    if(!Arrived(sim,route.to)||sim.score==0)throw new Exception("Reward path failed: "+course.number+" / "+route.from+" / "+c.id);
                }
                reports.Add(new CourseReport{number=course.number,title=course.title,difficulty=course.difficulty,minimumWindow=minimum,averageWindow=total/reviews.Length,jumps=reviews});
                Debug.Log("MEKTRA_IMPROVED level "+course.number+": "+shifted+" placements refined; min "+minimum.ToString("F3")+"s; mean "+(total/reviews.Length).ToString("F3")+"s");
            }
            LevelPackRegression.Run(pack);MotionRegression.Run(pack);
            File.WriteAllText(folder+"/after.json",JsonUtility.ToJson(new Report{levels=reports.ToArray()},true));
            File.WriteAllText(folder+"/candidate-pack.json",JsonUtility.ToJson(pack,true));
            Debug.Log("MEKTRA_IMPROVEMENT_SUCCESS: staged candidate validated, active pack unchanged.");
        }
        static void Scale(PlatformData platform,double scale)
        {
            double x=(platform.bounds.minX+platform.bounds.maxX)*.5,y=(platform.bounds.minY+platform.bounds.maxY)*.5;
            for(int i=0;i<platform.outline.Length;i++)platform.outline[i]=new Point(x+(platform.outline[i].x-x)*scale,y+(platform.outline[i].y-y)*scale);
            foreach(var command in platform.paths)for(int i=0;i<command.v.Length;i+=2){command.v[i]=x+(command.v[i]-x)*scale;command.v[i+1]=y+(command.v[i+1]-y)*scale;}
            platform.bounds.minX=x+(platform.bounds.minX-x)*scale;platform.bounds.maxX=x+(platform.bounds.maxX-x)*scale;
            platform.bounds.minY=y+(platform.bounds.minY-y)*scale;platform.bounds.maxY=y+(platform.bounds.maxY-y)*scale;
            var path=new System.Text.StringBuilder();foreach(var command in platform.paths){path.Append(command.op);foreach(var value in command.v)path.Append(value.ToString("0.#####",System.Globalization.CultureInfo.InvariantCulture)).Append(' ');}platform.d=path.ToString();
        }
        public static void Refine()
        {
            string folder="../assets/levels/design-review";
            var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(folder+"/candidate-pack.json"));var characters=CharacterCatalog.Load();var reports=new List<CourseReport>();
            foreach(var course in pack.levels)
            {
                var level=course.level;var reviews=new JumpReview[level.platforms.Length-1];for(int i=0;i<reviews.Length;i++)reviews[i]=Review(level,i,characters);
                double floor=course.number<=2?.22:course.mood=="Fun"?.18:course.mood=="Critical"?.13:.16;int enlarged=0;
                for(int index=1;index+1<level.platforms.Length;index++)
                {
                    if(reviews[index-1].windowSeconds>=floor)continue;
                    var original=JsonUtility.ToJson(level.platforms[index]);
                    foreach(double scale in course.number<=2?new[]{1.7,1.45,1.25}:new[]{1.45,1.25})
                    {
                        var platform=JsonUtility.FromJson<PlatformData>(original);level.platforms[index]=platform;Scale(platform,scale);
                        if(platform.bounds.minX<25||platform.bounds.maxX>365||level.platforms[index-1].bounds.minY-platform.bounds.maxY<45||platform.bounds.minY-level.platforms[index+1].bounds.maxY<45)continue;
                        var a=Review(level,index-1,characters);var b=Review(level,index,characters);
                        if(a.windowSeconds<reviews[index-1].windowSeconds+.012||b.windowSeconds+.005<reviews[index].windowSeconds)continue;
                        reviews[index-1]=a;reviews[index]=b;enlarged++;original=null;break;
                    }
                    if(original!=null)level.platforms[index]=JsonUtility.FromJson<PlatformData>(original);
                }
                var routes=new List<AuthoredRoute>();double minimum=double.MaxValue,total=0;
                foreach(var review in reviews)
                {
                    var start=new Simulation(level,characters[0]);start.platform=start.platforms[review.from];start.distance=review.distance;start.Select(characters[0]);
                    var direction=start.JumpDirection();double length=(review.landing-review.launch).Length;
                    var lane=new List<StarData>();foreach(var star in level.stars)if(star.id.StartsWith("star-"+(review.from+1)+"-",StringComparison.Ordinal))lane.Add(star);
                    lane.Sort((a,b)=>string.CompareOrdinal(a.id,b.id));
                    for(int point=0;point<lane.Count;point++)
                    {
                        var position=review.launch+direction*(length*(lane.Count==1?.48:.32+point*.34/(lane.Count-1)));
                        if(position.x<25||position.x>365)throw new Exception("Reward lane enters wall");lane[point].x=position.x;lane[point].y=position.y;
                    }
                    routes.Add(new AuthoredRoute{from=review.from,to=review.to,distance=review.distance,windowSeconds=review.windowSeconds,launch=review.launch,landing=review.landing});minimum=Math.Min(minimum,review.windowSeconds);total+=review.windowSeconds;
                }
                course.routes=routes.ToArray();MotionRegression.GeometryChecks(level);
                foreach(var route in course.routes)foreach(var c in characters)
                {
                    var sim=Trial(level,c,route.from,route.distance);if(!Arrived(sim,route.to)||sim.score==0)throw new Exception("Refined reward path failed: "+course.number+" / "+route.from+" / "+c.id);
                }
                reports.Add(new CourseReport{number=course.number,title=course.title,difficulty=course.difficulty,minimumWindow=minimum,averageWindow=total/reviews.Length,jumps=reviews});
                Debug.Log("MEKTRA_REFINED level "+course.number+": "+enlarged+" target sizes refined; min "+minimum.ToString("F3")+"s; mean "+(total/reviews.Length).ToString("F3")+"s");
            }
            LevelPackRegression.Run(pack);MotionRegression.Run(pack);
            File.WriteAllText(folder+"/after.json",JsonUtility.ToJson(new Report{levels=reports.ToArray()},true));File.WriteAllText(folder+"/candidate-pack.json",JsonUtility.ToJson(pack,true));
            Debug.Log("MEKTRA_REFINEMENT_SUCCESS");
        }
        [Serializable] class Playthrough {public string character;public int completedCourses,jumps,stars;public double seconds,minimumLavaClearance;public string outcome;}
        [Serializable] class PlaythroughReport {public Playthrough[] runs;}
        static double Bridge(LevelData[] courses,Character[] characters,int section)
        {
            int from=courses[section].platforms.Length-1;var orbit=new Orbit(courses[section].platforms[from].outline);var candidates=new List<double>();
            for(int sample=0;sample<360;sample++)
            {
                double distance=sample*orbit.length/360;orbit.Pose(distance,out var p,out var n);if(n.y>=-.1)continue;
                bool valid=true;foreach(var c in characters)
                {
                    var sim=MotionRegression.Launch(courses,c,section,from,distance,0);
                    if(sim.phase!=Phase.Playing||sim.flying||sim.platform.section!=section+1||sim.platform.index!=0){valid=false;break;}
                }
                if(valid)candidates.Add(distance);
            }
            if(candidates.Count==0)throw new Exception("No campaign bridge: "+section);
            // Stay inside the contiguous window rather than taking its first valid sample.
            int bestStart=0,bestCount=1,runStart=0;
            double increment=orbit.length/360;
            for(int i=1;i<=candidates.Count;i++)
            {
                if(i<candidates.Count&&candidates[i]-candidates[i-1]<increment*1.5)continue;
                if(i-runStart>bestCount){bestStart=runStart;bestCount=i-runStart;}runStart=i;
            }
            return candidates[bestStart+(bestCount-1)/2];
        }
        public static void PlayCampaign()
        {
            string folder="../assets/levels/design-review";
            var args=Environment.GetCommandLineArgs();int option=Array.IndexOf(args,"-mektraCandidate");
            string candidate=option>=0&&option+1<args.Length?args[option+1]:folder+"/candidate-pack.json";
            folder=Path.GetDirectoryName(candidate);
            var pack=JsonUtility.FromJson<LevelPack>(File.ReadAllText(candidate));var courses=pack.Courses();var characters=CharacterCatalog.Load();var bridges=new double[courses.Length];
            for(int i=0;i<courses.Length;i++)bridges[i]=Bridge(courses,characters,i);
            var reports=new List<Playthrough>();
            foreach(var c in characters)
            {
                var sim=new Simulation(courses[0],c,courses);sim.SetViewport(816);sim.Start();int jumps=0;double clearance=double.MaxValue;
                for(int step=0;step<120*1800&&sim.phase==Phase.Playing&&sim.platform.section<courses.Length;step++)
                {
                    if(!sim.flying)
                    {
                        int section=sim.platform.section,index=sim.platform.index;
                        double target=index==courses[section].platforms.Length-1?bridges[section]:pack.levels[section].routes[index].distance;
                        double forward=(target-sim.distance+sim.platform.orbit.length)%sim.platform.orbit.length;
                        if(forward<=sim.Pace*GameConfig.Step*1.1){sim.Jump();jumps++;}
                    }
                    sim.Advance(GameConfig.Step);sim.events.Clear();
                    if(sim.lava.Active(sim.time))clearance=Math.Min(clearance,sim.lava.Front-sim.position.y-GameConfig.Radius);
                }
                var report=new Playthrough{character=c.id,completedCourses=sim.platform.section,jumps=jumps,stars=sim.score,seconds=sim.time,minimumLavaClearance=clearance,outcome=sim.platform.section>=courses.Length?"completed":"failed: "+sim.reason};reports.Add(report);
                Debug.Log("MEKTRA_CAMPAIGN "+c.id+": "+report.outcome+" after "+report.completedCourses+" courses, "+report.seconds.ToString("F1")+"s, "+report.stars+" stars");
            }
            File.WriteAllText(folder+"/campaign.json",JsonUtility.ToJson(new PlaythroughReport{runs=reports.ToArray()},true));
            foreach(var report in reports)if(report.outcome!="completed")throw new Exception("Campaign failed for "+report.character+": "+report.outcome);
            Debug.Log("MEKTRA_CAMPAIGN_SUCCESS: continuous traversal through all "+courses.Length+" courses and wraparound with all eight characters, active lava and magnets.");
        }
        public static void Run()
        {
            var pack=LevelPack.Load();var characters=CharacterCatalog.Load();var reports=new List<CourseReport>();
            foreach(var course in pack.levels)
            {
                var jumps=new List<JumpReview>();double min=double.MaxValue,total=0;
                for(int from=0;from+1<course.level.platforms.Length;from++)
                {
                    var review=Review(course.level,from,characters);jumps.Add(review);min=Math.Min(min,review.windowSeconds);total+=review.windowSeconds;
                }
                reports.Add(new CourseReport{number=course.number,title=course.title,difficulty=course.difficulty,minimumWindow=min,averageWindow=total/jumps.Count,jumps=jumps.ToArray()});
                Debug.Log("MEKTRA_REVIEW level "+course.number+": min "+min.ToString("F3")+"s; mean "+(total/jumps.Count).ToString("F3")+"s");
            }
            Directory.CreateDirectory("../assets/levels/design-review");
            File.WriteAllText("../assets/levels/design-review/before.json",JsonUtility.ToJson(new Report{levels=reports.ToArray()},true));
            Debug.Log("MEKTRA_REVIEW_SUCCESS");
        }
    }
}
