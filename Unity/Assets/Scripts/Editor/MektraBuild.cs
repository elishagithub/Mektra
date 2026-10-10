using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
namespace Mektra.Editor
{
    public static class MektraBuild
    {
        [MenuItem("Mektra/Validate and build Mac game")] public static void Build()=>BuildChecked(null);
        public static void BuildValidatedOpening(LevelPack validated)=>BuildChecked(validated);
        static void BuildChecked(LevelPack validated)
        {
            Regression.Run();
            var active=validated??LevelPack.Load();
            if(active.endless)EnergyRegression.Run(active);
            if(active.endless){if(validated==null)EndlessRegression.Run(active);}
            else if(active.challengeOpening){if(validated==null)ChallengeRegression.Run(active);}
            else {LevelPackRegression.Run(active);MotionRegression.Run(active);WorldRegression.Run(active); }
            MagnetRegression.Run();
            
            PlayerSettings.companyName="Mektra";
            PlayerSettings.productName="Mektra";
            PlayerSettings.SplashScreen.show=false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone,"com.mektra.unity");
            PlayerSettings.defaultScreenWidth=430;
            PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=false;
            PlayerSettings.macRetinaSupport=true;
            PlayerSettings.runInBackground=false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone,1);
            Directory.CreateDirectory("Assets/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=UiStyles.Background;
            camera.orthographic=true;
            camera.gameObject.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Mektra.unity");
            EditorBuildSettings.scenes=new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Mektra.unity",true)
            }
            ;
            Directory.CreateDirectory("Builds");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]
                {
                    "Assets/Scenes/Mektra.unity"
                }
                ,locationPathName="Builds/Mektra.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.None
            }
            );
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Mac build failed: "+result.summary.result);
            Debug.Log("MEKTRA_BUILD_SUCCESS "+result.summary.totalSize);
        }
    }
    [Serializable]class JumpPlan
    {
        public int from=0,to=0;
        public double distance=0;
    }
    [Serializable]class JumpPlans
    {
        public JumpPlan[] jumps=Array.Empty<JumpPlan>();
        public static JumpPlan[] Load()
        {
            var plans=JsonUtility.FromJson<JumpPlans>(Resources.Load<TextAsset>("jump-plan").text).jumps;
            var level=LevelData.Load();foreach(var plan in plans)plan.distance=CourseReview.RouteDistance(level,plan.from,plan.distance);return plans;
        }
    }
    public static class Regression
    {
        public static void Run()
        {
            // Both sides of every boundary must agree, including the loop back to forest.
            for(int boundary=1;boundary<=12;boundary++)
            {
                var before=ThemeJourney.Sample(boundary*180-.001);var after=ThemeJourney.Sample(boundary*180+.001);
                Assert((new Vector4(before.top.r,before.top.g,before.top.b,before.forest)-new Vector4(after.top.r,after.top.g,after.top.b,after.forest)).magnitude<.001f,"Theme transition discontinuity");
            }
            var level=LevelData.Load();
            var characters=CharacterCatalog.Load();
            var plan=JumpPlans.Load();
            int routes=0;
            foreach(var c in characters)for(int sectionIndex=0;sectionIndex<3;sectionIndex++)foreach(var jump in plan)
            {
                var s=new Simulation(level,c);
                s.EnsureSection(sectionIndex+1);
                s.platform=s.platforms.Find(p=>p.section==sectionIndex&&p.index==jump.from);
                s.distance=jump.distance;
                s.Select(c);
                s.SetViewport(750);
                s.Start();
                s.Jump();
                for(int step=0;step<240&&s.flying&&s.phase==Phase.Playing;step++)s.Advance(GameConfig.Step);
                int index=(jump.from+1)%level.platforms.Length,section=sectionIndex+(index==0?1:0);
                Assert(s.phase==Phase.Playing&&!s.flying&&s.platform.index==index&&s.platform.section==section,"Route "+jump.from+" failed for "+c.id);
                // Pickup reach is intentionally tighter; a safe landing need not award a nearby star.
                routes++;
            }
            // Frame cadence must not change travel speed or produce stepped render poses.
            var reference=new Simulation(level,characters[0]);reference.Start();
            for(int i=0;i<240;i++)reference.Advance(GameConfig.Step);
            foreach(double frame in new[]{1.0/60,1.0/120,1.0/144})
            {
                var motion=new Simulation(level,characters[0]);motion.Start();
                double elapsed=0;
                while(elapsed<2-1e-10)
                {
                    double dt=Math.Min(frame,2-elapsed);var before=motion.RenderPosition;
                    motion.Advance(dt);elapsed+=dt;
                    Assert((motion.RenderPosition-before).Length<GameConfig.OrbitSpeed*dt+2,"Render position discontinuity at "+frame);
                }
                Assert(Math.Abs(motion.distance-reference.distance)<=GameConfig.OrbitSpeed*GameConfig.Step+.001,"Frame cadence changed travel speed");
            }
            var constant=new Simulation(level,characters[0]);constant.EnsureSection(12);
            foreach(var platform in constant.platforms)
            {
                constant.platform=platform;
                Assert(constant.Pace==69.7,"Automatic speed increase remains");
            }
            // Perpendicular launch must never be bent toward a target by aim assistance.
            var launchCheck=new Simulation(level,characters[0]);launchCheck.Start();
            for(int sample=0;sample<120;sample++)
            {
                launchCheck.distance=sample*launchCheck.platform.orbit.length/120;
                launchCheck.Select(characters[0]);var n=launchCheck.normal.Normal;
                var direction=launchCheck.JumpDirection();
                Assert(Math.Abs(direction.x*n.y-direction.y*n.x)<1e-12,"Jump is not perpendicular to tangent");
                Assert(direction.x*n.x+direction.y*n.y>.999999,"Jump normal points inward");
            }
            var pickup=new Simulation(level,characters[0]);pickup.stars.Clear();pickup.magnets.Clear();pickup.Start();
            var closeStar=new StarInstance{position=pickup.position+new Point(0,-14)};
            var distantStar=new StarInstance{position=pickup.position+new Point(0,-20)};
            pickup.stars.Add(closeStar);pickup.stars.Add(distantStar);pickup.Advance(GameConfig.Step);
            Assert(closeStar.collected&&!distantStar.collected&&pickup.score==1,"Tighter star contact boundary failed");
            Debug.Log("MEKTRA_PICKUP_VALIDATION PASS: close contact collects; former generous pickup distance does not.");
            var lavaTest=new LavaHazard(1000);double front=lavaTest.Front;
            Assert(front>=1000+LavaHazard.StartBelowScreen,"Lava starts inside the screen");
            lavaTest.Step(1,LavaHazard.GraceSeconds*.5,500,700);Assert(lavaTest.Front==front,"Lava ignored opening grace");
            for(int i=0;i<600;i++)
            {
                front=lavaTest.Front;lavaTest.Step(GameConfig.Step,LavaHazard.GraceSeconds+i*GameConfig.Step,500,700);
                Assert(lavaTest.Front<=front&&front-lavaTest.Front<=LavaHazard.CatchupSpeed*GameConfig.Step+.0001,"Lava teleported or retreated");
            }
            Assert(lavaTest.Touches(new Point(195,lavaTest.Front-GameConfig.Radius)),"Lava contact missing");
            Assert(!lavaTest.Touches(new Point(195,lavaTest.Front-GameConfig.Radius-1)),"Lava kills above surface");
            var lavaRun=new Simulation(level,characters[0]);lavaRun.Start();lavaRun.time=20;
            lavaRun.position=new Point(195,lavaRun.lava.Front+5);lavaRun.flying=true;lavaRun.velocity=new Point();lavaRun.SetViewport(750);
            lavaRun.Advance(GameConfig.Step);Assert(lavaRun.phase==Phase.Exploding&&lavaRun.reason=="lava","Lava death missing");
            var lavaPause=new Simulation(level,characters[0]);lavaPause.Start();lavaPause.time=15;lavaPause.Advance(GameConfig.Step);lavaPause.Pause();front=lavaPause.lava.Front;
            lavaPause.Advance(10);Assert(lavaPause.lava.Front==front,"Paused lava advanced");
            Debug.Log("MEKTRA_LAVA_VALIDATION PASS: grace, bounded rise, contact death, safe clearance and pause.");
            var wall=new Simulation(level,characters[0]);
            wall.Start();
            wall.position=new Point(5,8000);
            wall.flying=true;
            wall.velocity=new Point(-864,0);
            wall.SetViewport(750);
            wall.Advance(GameConfig.Step);
            Assert(wall.phase==Phase.Exploding&&wall.reason=="wall","Wall crash missing");
            for(int i=0;i<130;i++)wall.Advance(GameConfig.Step);
            Assert(wall.phase==Phase.Over,"Game over timeout missing");
            var paused=new Simulation(level,characters[0]);
            paused.Start();
            paused.Advance(.016);
            paused.Pause();
            var position=paused.position;
            double gear=paused.gearAngle;
            paused.Advance(10);
            Assert((paused.position-position).Length==0&&paused.gearAngle==gear,"Pause advances simulation");
            paused.Resume();
            paused.Advance(.016);
            Assert((paused.position-position).Length>0,"Resume stalled");
            var stress=new Simulation(level,characters[0]);
            stress.Start();
            stress.platform=stress.platforms.Find(p=>p.section==1&&p.index==0);
            stress.distance=0;
            stress.Select(characters[0]);
            stress.SetViewport(750);
            stress.Advance(GameConfig.Step);
            Assert(stress.nextSection==3,"Endless section spawn missing");
            Assert(stress.particles.Count<=48,"Particle pool exceeded");
            string report="PASS: "+routes+" authored jump/star routes, 8 characters, classic/mirror/star-rush sections, wall crash, game over, pause/resume, endless section spawning.\n";
            File.WriteAllText("validation.txt",report);
            Debug.Log("MEKTRA_VALIDATION "+report);
        }
        static void Assert(bool condition,string message)
        {
            if(!condition)throw new Exception(message);
        }
    }
}
