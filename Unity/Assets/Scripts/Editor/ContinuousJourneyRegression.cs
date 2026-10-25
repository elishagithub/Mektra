using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class ContinuousJourneyRegression
    {
        static Simulation Shot(LevelData[] courses,Character character,int section,double fraction,double clock)
        {
            var sim=new Simulation(courses[0],character,courses,section);
            sim.platform=sim.platforms.Find(p=>p.section==section&&p.index==courses[section].platforms.Length-1);
            sim.distance=sim.platform.orbit.length*fraction;sim.time=clock;sim.energyEnabled=false;
            sim.Select(character);sim.bestY=sim.startY=sim.position.y;sim.SetViewport(816);sim.Start();sim.Jump();
            for(int tick=0;tick<240&&sim.flying&&sim.phase==Phase.Playing;tick++)sim.Advance(GameConfig.Step);
            return sim;
        }
        static bool Arrived(Simulation sim,int section)=>sim.phase==Phase.Playing&&!sim.flying&&sim.platform.section==section+1&&sim.platform.index==0;
        public static void Run()
        {
            var courses=ContinuousJourney.Courses(LevelPack.Load().JourneyCourses());var characters=CharacterCatalog.Load();int checkedRoutes=0;
            int organic=0;
            foreach(var course in courses)foreach(var shape in course.platforms){
                if(OrganicPlatformShapes.IsRound(shape))throw new Exception("Perfect circular platform remains: "+course.id+" / "+shape.id);
                if(shape.sourceShapeId.StartsWith("organic-"))organic++;
            }
            Debug.Log("MEKTRA_ORGANIC_SHAPES_PASS: "+organic+" formerly round platforms replaced; no perfect circular platforms remain.");
            for(int section=0;section<courses.Length-1;section++)
            {
                foreach(double clock in new[]{0.0,30.0})
                {
                    double winning=-1;
                    for(int sample=0;sample<120;sample++)
                    {
                        double fraction=sample/120.0;
                        if(Arrived(Shot(courses,characters[0],section,fraction,clock),section)){winning=fraction;break;}
                    }
                    if(winning<0)throw new Exception("Continuous join unreachable: "+section+" at "+clock);
                    foreach(var character in characters)
                    {
                        if(!Arrived(Shot(courses,character,section,winning,clock),section))throw new Exception("Continuous join fails character: "+section+" / "+character.id);
                        checkedRoutes++;
                    }
                }
            }
            var firefly=new StarInstance{firefly=true,position=new Point(190,400),id="firefly-check"};
            var initial=firefly.At(0);bool moved=false;
            for(int i=0;i<1200;i++){
                double t=i/60.0;var point=firefly.At(t);
                if((point-firefly.position).Length>15.01)throw new Exception("Firefly exceeds drift bounds");
                if((firefly.At(t+GameConfig.Step)-point).Length/GameConfig.Step>20)throw new Exception("Firefly changes too fast");
                moved|=(point-initial).Length>2;
            }
            if(!moved)throw new Exception("Firefly remains stationary");
            var pickup=new Simulation(courses[0],characters[0],courses);pickup.Start();pickup.energyEnabled=true;pickup.energy=50;pickup.stars.Clear();
            var close=new StarInstance{firefly=true,id="moving-contact"};for(int i=0;i<20;i++)close.position=pickup.position-close.Drift(GameConfig.Step);pickup.stars.Add(close);
            var far=new StarInstance{firefly=true,id="moving-distant",position=pickup.position+new Point(60,0)};pickup.stars.Add(far);
            pickup.Advance(GameConfig.Step);
            if(Math.Abs(pickup.energy-50)>.001)throw new Exception("Firefly unexpectedly changes health");
            if(!close.collected||far.collected)throw new Exception("Moving point contact does not match its rendered position");
            var hunger=new Simulation(courses[0],characters[0],courses);hunger.energyEnabled=true;hunger.energy=50;hunger.energyAge=31;hunger.Start();hunger.Advance(GameConfig.Step);
            if(hunger.energyEnabled||Math.Abs(hunger.energy-50)>.001||Math.Abs(hunger.EnergySpeed-1)>.001)throw new Exception("Forest health remains active or changes movement");
            hunger.energy=0;hunger.Advance(GameConfig.Step);if(hunger.phase!=Phase.Playing)throw new Exception("Empty legacy energy ends the forest run");
            double frozen=hunger.energy;hunger.Pause();hunger.Advance(.1);if(Math.Abs(frozen-hunger.energy)>.001)throw new Exception("Glow drains during pause");
            var safeForest=new Simulation(courses[0],characters[0],courses);safeForest.time=60;safeForest.lava=new LavaHazard(safeForest.position.y-200);double front=safeForest.lava.Front;safeForest.Start();safeForest.Advance(GameConfig.Step);
            if(safeForest.LavaEnabled||safeForest.phase!=Phase.Playing||safeForest.lava.Front!=front)throw new Exception("Lava remains active in the forest game");
            Debug.Log("MEKTRA_FOREST_NO_LAVA_PASS: forced lava contact cannot kill and lava front does not advance.");
            Debug.Log("MEKTRA_FIREFLY_PASS: bounded smooth motion, real contact collects, distant point does not.");
            foreach(string art in new[]{"moonlit-forest","emerald-forest"})if(Resources.Load<Texture2D>("Art/FireflyForest/"+art)==null)throw new Exception("Missing forest art: "+art);
            foreach(string name in ColorPresetPreview.Options)
            {
                if(name=="Black & White"||name=="Invert")continue;
                var lut=Resources.Load<Texture2D>("ColorGrades/"+name);
                if(lut==null||!lut.isReadable||lut.width!=lut.height*lut.height)throw new Exception("Colour preset unavailable: "+name);
            }
            if(Resources.Load<AudioClip>("Audio/mektra_music_menu")==null||Resources.Load<AudioClip>("Audio/mektra_music_game")==null)throw new Exception("Missing music loops");
            Debug.Log("MEKTRA_CONTINUOUS_JOURNEY_PASS: "+checkedRoutes+" physical boundary flights, all characters at two clocks; colour LUTs and music assets loaded.");
        }
    }
}
