using System;
using System.Collections.Generic;
using System.Linq;
namespace Mektra
{
    public enum Phase
    {
        Ready,Playing,Paused,Exploding,Over,Complete
    }
    public class PlatformInstance
    {
        public int index,section;
        public double offset;
        public PlatformData data;
        public Orbit orbit;
        public bool reflected,lockMotionDuringGuides;
        public double landedAt=double.PositiveInfinity;
        public bool vanished;
        public double Remaining(double time)=>data.crumbleSeconds<=0?1:Math.Clamp(1-(time-landedAt)/data.crumbleSeconds,0,1);
        public bool mirrored=>reflected;
        public string key=>section+":"+index;
        public bool Rotates=>data.rotation!=null&&data.rotation.enabled;
        public double AngleAt(double time)=>(Rotates?data.rotation.At(time):0)*(mirrored?-1:1);
        public Point Pivot=>new Point(mirrored?390-(data.bounds.minX+data.bounds.maxX)*.5:(data.bounds.minX+data.bounds.maxX)*.5,(data.bounds.minY+data.bounds.maxY)*.5);
        static Point Turn(Point p,double angle)=>new Point(p.x*Math.Cos(angle)-p.y*Math.Sin(angle),p.x*Math.Sin(angle)+p.y*Math.Cos(angle));
        public Point World(Point local,double time)
        {
            if(mirrored)local.x=390-local.x;
            if(Rotates)local=Pivot+Turn(local-Pivot,AngleAt(time));
            local.y+=offset;return local+MotionAt(time);
        }
        public Point Local(Point world,double time)
        {
            world-=MotionAt(time);world.y-=offset;
            if(Rotates)world=Pivot+Turn(world-Pivot,-AngleAt(time));
            if(mirrored)world.x=390-world.x;return world;
        }
        public Point Direction(Point local,double time){if(mirrored)local.x=-local.x;return Rotates?Turn(local,AngleAt(time)):local;}
        public Point SurfaceVelocity(Point local,double time,double dt)=>(World(local,time)-World(local,time-dt))*(1/dt);
        public Point MotionAt(double time)
        {
            var p=data.motion==null?new Point():data.motion.At(lockMotionDuringGuides?GameConfig.GuidedMotionTime(time):time);
            if(mirrored)p.x=-p.x;
            return p;
        }
    }
    public class StarInstance
    {
        public int guideFrom=-1,guideTo=-1;public double guideFraction;
        public Point position;
        public string id;
        public int section;
        public bool collected;
        public PlatformInstance parent;
        public bool attracted;
        public Point magnetPosition,previousMagnetPosition;
        public double magnetTime,magnetAge;
        public bool firefly;
        public Point Drift(double time)
        {
            if(!firefly)return new Point();
            // Stable phases keep each point independent without randomness or sudden turns.
            double phase=section*2.399963+guideFrom*.71+guideFraction*4.1;
            if(id!=null)foreach(char c in id)phase=phase*1.17+c*.031;else phase+=position.x*.017;
            phase=Math.Sin(phase*78.233)*43758.5453;phase=(phase-Math.Floor(phase))*Math.PI*2;
            return new Point(9*Math.Sin(time*(.55+phase*.02)+phase)+3*Math.Sin(time*1.13+phase*1.7),
                7*Math.Sin(time*(.43+phase*.015)+phase*1.3)+2*Math.Sin(time*.97+phase*.8));
        }
        public Point At(double time)=>attracted?previousMagnetPosition+(magnetPosition-previousMagnetPosition)*Math.Clamp((time-magnetTime)/GameConfig.Step,0,1):position+(parent==null?new Point():parent.MotionAt(time))+Drift(time);
    }
    public class EnergyBloom
    {
        public PlatformInstance parent; public Point position; public bool collected;
        public Point At(double time)=>position+parent.MotionAt(time);
    }
    public class MagnetInstance
    {
        public Point position;
        public int section;
        public bool collected;
    }
    public class BlackHoleInstance { public BlackHoleData data; public int section; public double offset; public Point center=>new Point(data.x,data.y+offset); }
    public class Particle
    {
        public Point position,velocity;
        public double age,life,radius;
        public bool star;
    }
    public class Burst
    {
        public Point position;
        public double age;
    }
    public struct SimulationEvent { public string kind; public Point position; public int streak; }
    public class Simulation
    {
        public readonly LevelData level;
        public readonly Orbit[] orbits;
        public readonly LevelData[] sequence;
        public readonly bool endless;
        public readonly int seed;
        public int CourseIndex(int section)=>endless?EndlessJourney.Index(sequence,section,seed):section%sequence.Length;
        public LevelData CourseAt(int section)=>sequence==null?level:sequence[CourseIndex(section)];
        readonly Dictionary<LevelData,Orbit[]> orbitCache=new Dictionary<LevelData,Orbit[]>();
        public Character character;
        public Phase phase=Phase.Ready;
        public string reason;
        public Point position,normal,velocity,previousPosition;
        Point landingOffset,previousLandingOffset,landingVelocity;
        double cameraVelocity;
        int landingOrbitDirection;
        public double distance,flightTime,cameraY,previousCamera,gearAngle,previousGear,time,accumulator,viewport=700,bestY,startY,happyUntil;
        public int CheckpointIndex {get;private set;}
        double checkpointAt;
        public int Chapter=>level.checkpointPlatforms==null?0:Math.Max(0,Array.IndexOf(level.checkpointPlatforms,CheckpointIndex));
        public bool flying;
        public bool AutoBouncing=>flying&&wedgeBounces>0;
        public int wedgeBounces;
        public bool BeingPulled=>capturedHole!=null;

        BlackHoleInstance capturedHole; Point captureStart; double captureAge;
        public double BodyScale=>capturedHole==null?1:Math.Max(.15,1-captureAge/.85);
        public LavaHazard lava;
        public double RenderLava=>lava.Render(Alpha);
        public PlatformInstance platform;
        public int score,highestSection,nextSection,precisionScore,jumpCount;
        bool legacyEnergyEnabled;
        public bool energyEnabled { get=>legacyEnergyEnabled&&!FireflySurvival; set=>legacyEnergyEnabled=value; }
        public bool reviveUsed {get;private set;}
        public bool CanRevive=>phase==Phase.Over&&!reviveUsed&&platforms.Any(p=>!p.vanished&&p.data.sourceShapeId!="wedge");
        public bool Revive()
        {
            if(!CanRevive)return false;
            var support=platforms.Where(p=>!p.vanished&&p.data.sourceShapeId!="wedge")
                .OrderBy(p=>Math.Abs(p.offset+p.data.bounds.minY-position.y)).First();
            platform=support;
            int top=-1;
            for(int i=0;i<support.orbit.points.Length;i++)
            {
                var point=support.orbit.points[i];double x=(support.mirrored?390-point.x:point.x)+support.MotionAt(time).x;
                if(x<GameConfig.Radius*3||x>390-GameConfig.Radius*3)continue;
                if(top<0||point.y<support.orbit.points[top].y)top=i;
            }
            if(top<0)return false;
            reviveUsed=true;distance=support.orbit.cumulative[top];
            landingOrbitDirection=ChooseLandingDirection(support,distance);
            support.landedAt=time;capturedHole=null;captureAge=0;deathAge=0;
            flying=false;wedgeBounces=0;velocity=new Point();landingOffset=new Point();landingVelocity=new Point();
            energy=100;energyAge=0;reason=null;accumulator=0;
            trail.Clear();particles.Clear();bursts.Clear();events.Clear();Place();
            SetViewport(viewport);lava=new LavaHazard(cameraY+viewport,time);
            phase=Phase.Paused;pausedFrom=Phase.Playing;Capture();
            return true;
        }
        public double energy=100,energyAge;
        public bool FireflySurvival=>sequence!=null&&level.discovery;
        public bool LavaEnabled=>!FireflySurvival;
        public double EnergySpeed=>!energyEnabled||FireflySurvival?1:.8+.2*Math.Clamp(energy/20,0,1);
        public readonly List<EnergyBloom> energyBlooms=new List<EnergyBloom>();
        double deathAge,trailTime;
        double previousTime;
        Phase pausedFrom;
        public List<PlatformInstance> platforms=new List<PlatformInstance>();
        public List<StarInstance> stars=new List<StarInstance>();
        public List<MagnetInstance> magnets=new List<MagnetInstance>();
        public List<PlatformInstance> hazards=new List<PlatformInstance>();
        public List<BlackHoleInstance> blackHoles=new List<BlackHoleInstance>();
        public List<Particle> particles=new List<Particle>();
        public List<Burst> bursts=new List<Burst>();
        public List<Point> trail=new List<Point>();
        public Queue<SimulationEvent> events=new Queue<SimulationEvent>();
        public int Distance{
get{
#if MEKTRA_INVERT_PREVIEW
return precisionScore;
#else
return (int)Math.Floor((startY-bestY)/10);
#endif
}}

        public double Alpha=>(phase==Phase.Playing||phase==Phase.Exploding)?Math.Clamp(accumulator/GameConfig.Step,0,1):1;
        public Point RenderPosition=>previousPosition+(position-previousPosition)*Alpha+previousLandingOffset+(landingOffset-previousLandingOffset)*Alpha;
        // Critical energy slows surface travel only; jump reach remains physically unchanged.
        public double Pace=>GameConfig.OrbitSpeed*EnergySpeed;
        public int LevelNumber=>sequence==null?platform.section+1:platform.section%sequence.Length+1;
        public string Instruction=>CourseAt(platform.section).instruction;
        public string WorldName=>CourseAt(platform.section).name;
        public string SectionName=>sequence!=null?CourseAt(platform.section).name:platform.section%3==1?"MIRROR RUN":platform.section%3==2?"STAR RUSH":"CLASSIC";
        public double ThemeDistance
        {
            get
            {
                var course=CourseAt(platform.section);
                if(endless)return platform.section<16?(startY-bestY)/10:118.8+(((platform.section-16)/8)%6)*180;
                if(course.worldNumber==0)return (startY-bestY)/10;
                if(course.connections!=null)return (course.worldNumber-1)*180+118.8;
                int worldStart=platform.section-course.worldStage+1;
                double remaining=bestY-(3144-(worldStart+4)*course.stride);
                double blend=course.worldNumber==7?0:Math.Clamp((600-remaining)/600,0,1);
                return (course.worldNumber-1)*180+118.8+61.2*blend;
            }
        }
        public double RenderCamera=>previousCamera+(cameraY-previousCamera)*Alpha;
        // Layout inspection moves only the camera; gameplay and hazards never advance.
        public void SetPreviewCamera(double value){cameraY=previousCamera=value;cameraVelocity=0;}
        public double RenderTime=>previousTime+(time-previousTime)*Alpha;
        public double RenderGear=>previousGear+(((gearAngle-previousGear+Math.PI*3)%(Math.PI*2))-Math.PI)*Alpha;
        public Simulation(LevelData data,Character c,LevelData[] courses=null,int startSection=0,bool endless=false,int seed=0)
        {
            level=data;sequence=courses;this.endless=endless;this.seed=seed;
            character=c;
            orbits=new Orbit[data.platforms.Length];
            for(int i=0;i<orbits.Length;i++)orbits[i]=new Orbit(data.platforms[i].outline);
            orbitCache[data]=orbits;
            nextSection=Math.Max(0,startSection);highestSection=nextSection;
            AddSection();
            AddSection();
            platform=platforms[0];
            int top=0;
            for(int i=0;i<platform.orbit.points.Length;i++)if(platform.orbit.points[i].y<platform.orbit.points[top].y)top=i;
            distance=platform.orbit.cumulative[top];
            Place();
            startY=bestY=position.y;
            SetViewport(700);
            lava=new LavaHazard(cameraY+viewport);
        }
        // Only used on a fresh attempt; live checkpoint landings never reposition the player.
        public void RestoreCheckpoint(int index)
        {
            if(phase!=Phase.Ready||index<=0||level.checkpointPlatforms==null||Array.IndexOf(level.checkpointPlatforms,index)<0)return;
            platform=platforms.Find(p=>p.index==index&&p.section==0);
            int top=0;for(int i=1;i<platform.orbit.points.Length;i++)if(platform.orbit.points[i].y<platform.orbit.points[top].y)top=i;
            distance=platform.orbit.cumulative[top];landingOrbitDirection=ChooseLandingDirection(platform,distance);Place();
            CheckpointIndex=index;startY=bestY=position.y;SetViewport(viewport);lava=new LavaHazard(cameraY+viewport,time);
            foreach(var star in stars)if(star.position.y>platform.data.bounds.minY)star.collected=true;
            foreach(var support in platforms)if(support.index<=index&&support.data.switchSupport)OpenLinkedGate(support,false);
            Capture();
        }
        void OpenLinkedGate(PlatformInstance support,bool feedback)
        {
            bool changed=false;
            foreach(var hazard in hazards)if(!hazard.vanished&&hazard.data.openedBy==support.data.id){hazard.vanished=true;changed=true;}
            if(changed&&feedback)events.Enqueue(new SimulationEvent{kind="switch",position=position});
        }
        void ReachCheckpoint()
        {
            if(sequence!=null||flying||platform.index<=CheckpointIndex||level.checkpointPlatforms==null||Array.IndexOf(level.checkpointPlatforms,platform.index)<0)return;
            CheckpointIndex=platform.index;checkpointAt=time;lava=new LavaHazard(cameraY+viewport,time);
            events.Enqueue(new SimulationEvent{kind="checkpoint",position=position});
        }
        void Capture()
        {
            previousTime=time;
            previousPosition=position;
            previousLandingOffset=landingOffset;
            previousCamera=cameraY;
            previousGear=gearAngle;
        }
        public void SetViewport(double height)
        {
            viewport=height;
            cameraY=position.y-height*GameConfig.CameraAnchor;
            cameraVelocity=0;
            // Initial UI sizing must keep the lava below the actual viewport, never reveal it mid-screen.
            if(lava!=null&&phase==Phase.Ready)lava=new LavaHazard(cameraY+viewport,time);
            Capture();
        }
        public void Select(Character c)
        {
            character=c;
            if(!flying)Place();
            Capture();
        }
        void Place()
        {
            platform.orbit.Pose(distance,out var p,out normal);
            if(platform.Rotates){normal=platform.Direction(normal,time);position=Orbit.Ground(platform.World(p,time),normal,gearAngle,character);return;}
            if(platform.mirrored){p.x=390-p.x;normal.x=-normal.x;}
            position=Orbit.Ground(p,normal,gearAngle,character);
            position.y+=platform.offset;
            position+=platform.MotionAt(time);
        }
        // A section's bottom meets the preceding section's top even when their heights differ.
        public double SectionOffset(int section)
        {
            if(sequence==null)return -section*level.stride;
            int period=sequence.Length;double cycle=0,tail=0;
            for(int i=1;i<=period;i++)
            {
                double stride=CourseAt(i).stride;cycle+=stride;
                if(i<=section%period)tail+=stride;
            }
            return -(section/period*cycle+tail);
        }
        void AddSection()
        {
            if(level.discovery&&nextSection>=(sequence==null?1:sequence.Length))return;
            int section=nextSection++;
            var course=CourseAt(section);
            if(!orbitCache.TryGetValue(course,out var courseOrbits))
            {
                courseOrbits=new Orbit[course.platforms.Length];
                for(int i=0;i<courseOrbits.Length;i++)courseOrbits[i]=new Orbit(course.platforms[i].outline);
                orbitCache.Add(course,courseOrbits);
            }
            bool reflected=sequence==null&&section%3==1;
            double offset=SectionOffset(section);
            for(int i=0;i<course.platforms.Length;i++)platforms.Add(new PlatformInstance
            {
                index=i,section=section,offset=offset,data=course.platforms[i],orbit=courseOrbits[i],reflected=reflected,lockMotionDuringGuides=endless
            }
            );
            // Stars sustain regular climbing; a blue bloom is an occasional recovery opportunity.
            for(int i=sequence==null?1:course.platforms.Length-1;!FireflySurvival&&(sequence==null||section%6==4)&&i<course.platforms.Length;i+=2)
            {
                var support=platforms.Find(p=>p.section==section&&p.index==i);
                int top=0;for(int j=1;j<support.orbit.points.Length;j++)if(support.orbit.points[j].y<support.orbit.points[top].y)top=j;
                var at=support.orbit.points[top];if(reflected)at.x=390-at.x;at.y+=offset;
                if(at.x>20&&at.x<370)energyBlooms.Add(new EnergyBloom{parent=support,position=at});
            }
            if(course.blackHoles!=null)foreach(var hole in course.blackHoles)blackHoles.Add(new BlackHoleInstance{data=hole,section=section,offset=offset});
            if(course.hazards!=null)for(int i=0;i<course.hazards.Length;i++)hazards.Add(new PlatformInstance{index=i,section=section,offset=offset,data=course.hazards[i],lockMotionDuringGuides=endless});
            foreach(var s in course.stars)stars.Add(new StarInstance
            {
                position=new Point(reflected?390-s.x:s.x,s.y+offset),section=section,id=s.id,guideFrom=s.guideFrom,guideTo=s.guideTo,guideFraction=s.guideFraction,
                firefly=sequence!=null&&level.discovery,parent=string.IsNullOrEmpty(s.parentPlatformId)?null:platforms.Find(p=>p.section==section&&p.data.id==s.parentPlatformId)
            }
            );
            if(sequence==null&&section%3==2&&level.challengeStars!=null)foreach(var s in level.challengeStars)stars.Add(new StarInstance{position=new Point(s.x,s.y+offset),section=section});
            // Give every authored connection a precision lane; existing collectible placement remains the guide solver's seed.
            if(!course.sparseRewards&&course.connections!=null)foreach(var link in course.connections)
            {
                var source=platforms.Find(p=>p.section==section&&p.index==link.from);
                var target=platforms.Find(p=>p.section==section&&p.index==link.to);
                if(source==null||target==null)continue;
                var from=new Point((source.data.bounds.minX+source.data.bounds.maxX)*.5,(source.data.bounds.minY+source.data.bounds.maxY)*.5+offset);
                var to=new Point((target.data.bounds.minX+target.data.bounds.maxX)*.5,(target.data.bounds.minY+target.data.bounds.maxY)*.5+offset);
                if(reflected){from.x=390-from.x;to.x=390-to.x;}
                int count=2+(section+link.from)%3;
                stars.RemoveAll(point=>point.section==section&&point.guideFrom==link.from&&point.guideTo==link.to);
                for(int n=0;n<count;n++){double f=(n+1.0)/(count+1);stars.Add(new StarInstance{section=section,guideFrom=link.from,guideTo=link.to,guideFraction=f,firefly=sequence!=null&&level.discovery,position=from+(to-from)*f});}
            }
            magnets.Clear();
            // In endless play, omit opening pickups and nominate just one rare section per wave.
            for(int index=0;false&& !course.suppressMagnets&&(!endless||EndlessJourney.HasMagnet(section,seed))&&index+1<course.platforms.Length;index+=6)
            {
                var lower=course.platforms[index].bounds;var upper=course.platforms[index+1].bounds;
                double lowerY=(lower.minY+lower.maxY)*.5+offset,upperY=(upper.minY+upper.maxY)*.5+offset;
                var anchor=stars.Find(star=>star.section==section&&star.position.y<lowerY&&star.position.y>upperY);
                if(anchor==null)continue;
                double x=(lower.minX+lower.maxX)*.5;
                var center=new Point(reflected?390-x:x,lowerY);
                magnets.Add(new MagnetInstance{section=section,position=anchor.position+(center-anchor.position).Normal*32});
            }
        }
        // Three authored-section treatments provide variety without random unreachable geometry.
        public void EnsureSection(int section){while(nextSection<=section&&(!level.discovery||nextSection<(sequence==null?1:sequence.Length)))AddSection();}
        public void Start()
        {
            if(phase==Phase.Ready){phase=Phase.Playing;platform.landedAt=time;if(platform.data.switchSupport)OpenLinkedGate(platform,true);}
        }
        public void Pause()
        {
            if(phase==Phase.Playing||phase==Phase.Exploding)
            {
                pausedFrom=phase;
                phase=Phase.Paused;
                Capture();
            }
        }
        public void Resume()
        {
            if(phase==Phase.Paused)phase=pausedFrom;
        }
        // No aim assistance: launch perpendicular to the current surface tangent.
        public Point JumpDirection()=>normal.Normal;
        double lastJumpAt;
        readonly List<StarInstance> shotPoints=new List<StarInstance>();public int perfectChain;
        public void Jump()
        {
            if(phase!=Phase.Playing||flying)return;
            jumpCount++;
            if(energyEnabled&&!FireflySurvival&&energyAge>25)energy=Math.Max(0,energy-1);
            shotPoints.Clear();
            int to=platform.index+1;var links=CourseAt(platform.section).connections;
            if(links!=null)foreach(var link in links)if(link.from==platform.index){to=link.to;break;}
            foreach(var point in stars)if(!point.collected&&point.section==platform.section&&point.guideFrom==platform.index&&point.guideTo==to)shotPoints.Add(point);
            lastJumpAt=time;
            velocity=JumpDirection()*GameConfig.JumpSpeed;
            wedgeBounces=0;flying=true;
            flightTime=0;
            events.Enqueue(new SimulationEvent{kind="jump",position=position});
        }
        public void Advance(double delta)
        {
            if(phase!=Phase.Playing&&phase!=Phase.Exploding)return;
            accumulator+=Math.Clamp(delta,0,GameConfig.MaxDelta);
            while(accumulator>=GameConfig.Step)
            {
                Step(GameConfig.Step);
                accumulator-=GameConfig.Step;
            }
        }
        public void Die(string why)
        {
            if(phase!=Phase.Playing)return;
            reason=why;
            phase=Phase.Exploding;
            deathAge=0;
            trail.Clear();
            events.Enqueue(new SimulationEvent{kind="crash",position=position});
            Emit(position,32,180);
        }
        // Decide once on contact; shifting platforms must not reverse an active orbit.
        public static int ShorterArcDirection(double contact,double goal,double length,int tieDirection=1)
        {
            if(length<=0)return tieDirection;
            double forward=((goal-contact)%length+length)%length;
            double backward=length-forward;
            if(Math.Abs(forward-backward)<.001||forward<.001)return tieDirection;
            return forward<backward?1:-1;
        }
        public PlatformInstance SuggestedTarget(PlatformInstance source,double clock)
        {
            var course=CourseAt(source.section);if(course.connections==null)return null;
            PlatformInstance best=null;double score=double.PositiveInfinity;
            double sx=(source.data.bounds.minX+source.data.bounds.maxX)*.5+source.MotionAt(clock).x;
            foreach(var link in course.connections)if(link.from==source.index)
            {
                var target=platforms.Find(p=>p.section==source.section&&p.index==link.to);if(target==null||target.vanished)continue;
                double tx=(target.data.bounds.minX+target.data.bounds.maxX)*.5+target.MotionAt(clock+.3).x;
                if(tx<85||tx>340)continue;double value=Math.Abs(tx-sx);
                if(value<score){score=value;best=target;}
            }
            return best;
        }
        int ChooseLandingDirection(PlatformInstance landed,double contact)
        {
            bool rain=CourseAt(landed.section).streamingRain;
            PlatformInstance next=rain?SuggestedTarget(landed,time):null;
            if(!rain)foreach(var candidate in platforms)
            {
                if(candidate.vanished||candidate==landed)continue;
                if(candidate.section<landed.section||candidate.section==landed.section&&candidate.index<=landed.index)continue;
                if(next==null||candidate.section<next.section||candidate.section==next.section&&candidate.index<next.index)next=candidate;
            }
            int fallback=landed.data.orbitDirection<0?-1:1;
            if(next==null)return fallback;
            if(landed.Rotates){var nextCenter=next.World(new Point((next.data.bounds.minX+next.data.bounds.maxX)*.5,(next.data.bounds.minY+next.data.bounds.maxY)*.5),time);return ShorterArcDirection(contact,landed.orbit.Nearest(landed.Local(nextCenter,time)),landed.orbit.length,fallback);}
            var bounds=next.data.bounds;
            var center=new Point((bounds.minX+bounds.maxX)*.5,(bounds.minY+bounds.maxY)*.5+next.offset);
            if(next.mirrored)center.x=390-center.x;
            center+=next.MotionAt(time)-landed.MotionAt(time);
            center.y-=landed.offset;
            if(landed.mirrored)center.x=390-center.x;
            double exit=landed.orbit.Nearest(center);
            return ShorterArcDirection(contact,exit,landed.orbit.length,fallback);
        }
        void Emit(Point p,int count,double speed,bool star=false)
        {
            for(int i=0;i<count;i++)
            {
                double a=i*2*Math.PI/count,v=speed*(.45+(i%5)*.14);
                particles.Add(new Particle
                {
                    position=p,velocity=new Point(Math.Cos(a)*v,Math.Sin(a)*v),age=0,life=.6+(i%4)*.1,radius=(star?4:2)+i%3,star=star
                }
                );
            }
            if(particles.Count>48)particles.RemoveRange(0,particles.Count-48);
        }
        public void Step(double dt)
        {
            Capture();
            time+=dt;
            foreach(var b in bursts)b.age+=dt;
            bursts.RemoveAll(b=>b.age>=.45);
            foreach(var p in particles)
            {
                p.age+=dt;
                p.position+=p.velocity*dt;
                p.velocity*=Math.Exp(-3*dt);
            }
            particles.RemoveAll(p=>p.age>=p.life);
            if(phase==Phase.Exploding)
            {
                deathAge+=dt;
                if(deathAge>=.85)phase=Phase.Over;
                return;
            }
            if(phase!=Phase.Playing)return;
            if(energyEnabled){energyAge+=dt;if(energyAge>(FireflySurvival?30:25))energy=Math.Max(0,energy-(FireflySurvival?.4:.65)*dt);}
            // Smooth only the visual landing correction; collision and scoring remain fixed-step.
            // Critically damped contact correction: continuous displacement, no spring bounce.
            const double landingRate=28;
            var landingTerm=landingVelocity+landingOffset*landingRate;
            double decay=Math.Exp(-landingRate*dt);
            landingOffset=(landingOffset+landingTerm*dt)*decay;
            landingVelocity=(landingVelocity-landingTerm*(landingRate*dt))*decay;
            gearAngle=(gearAngle+Pace/GameConfig.Radius*dt*(platform.mirrored?-1:1)+Math.PI*2)%(Math.PI*2);
            foreach(var support in platforms)
            {
                if(support.vanished||support.data.crumbleSeconds<=0||time-support.landedAt<support.data.crumbleSeconds)continue;
                support.vanished=true;Emit((new Point((support.data.bounds.minX+support.data.bounds.maxX)*.5,(support.data.bounds.minY+support.data.bounds.maxY)*.5+support.offset))+support.MotionAt(time),12,100);
                if(support==platform&&!flying){flying=true;flightTime=0;velocity=new Point(0,180);events.Enqueue(new SimulationEvent{kind="crumble",position=position});}
            }
            var before=position;
            if(capturedHole!=null)
            {
                captureAge+=dt;double progress=Math.Clamp(captureAge/.85,0,1),angle=progress*Math.PI*4;
                var d=captureStart-capturedHole.center;double shrink=1-progress;
                position=capturedHole.center+new Point(d.x*Math.Cos(angle)-d.y*Math.Sin(angle),d.x*Math.Sin(angle)+d.y*Math.Cos(angle))*shrink;
                if(progress>=1){Die("black hole");return;}
            }
            else if(!flying)
            {
                distance=(distance+Pace*dt*(landingOrbitDirection!=0?landingOrbitDirection:(platform.data.orbitDirection<0?-1:1))+platform.orbit.length)%platform.orbit.length;
                Place();
            }
            else
            {
                    position+=velocity*dt;
                flightTime+=dt;
                foreach(var target in platforms)
                {
                    if(target.vanished||target==platform&&flightTime<.16)continue;
                    var motion=target.MotionAt(time);
                    if(target.Rotates?!Geometry.Touches(target.Local(position,time),target.data,0):!Geometry.Touches(position-motion,target.data,target.offset,target.mirrored))continue;
                    if(target.data.sourceShapeId=="wedge")
                    {
                        var localPoint=target.Rotates?target.Local(position,time):position-motion-new Point(0,target.offset);double nearest=double.PositiveInfinity;Point hit=new Point(),face=new Point();
                        for(int edge=0;edge<target.data.outline.Length;edge++)
                        {
                            var a=target.data.outline[edge];var b=target.data.outline[(edge+1)%target.data.outline.Length];
                            if(target.mirrored&&!target.Rotates){a.x=390-a.x;b.x=390-b.x;}
                            var contact=Geometry.Segment(localPoint,a,b);var n=new Point(b.y-a.y,a.x-b.x).Normal;if(target.mirrored&&!target.Rotates)n=n*-1;
                            if(contact.distance<nearest){nearest=contact.distance;hit=contact.point;face=n;}
                        }
                        var surfaceVelocity=target.Rotates||target.data.motion!=null&&target.data.motion.type!="static"?target.SurfaceVelocity(hit,time,dt):new Point();
                        if(target.Rotates)face=target.Direction(face,time);
                        var incoming=velocity-surfaceVelocity;double approach=incoming.x*face.x+incoming.y*face.y;
                        if(approach<0)
                        {
                            velocity=incoming-face*(2*approach)+surfaceVelocity; // Elastic reflection in the moving surface frame.
                            position=(target.Rotates?target.World(hit,time):hit+motion+new Point(0,target.offset))+face*(GameConfig.Radius+.1);
                            normal=face;wedgeBounces++;landingOffset=new Point();Emit(position,8,70);
                            events.Enqueue(new SimulationEvent{kind="land",position=position});
                        }
                        continue;
                    }
                    var local=position-motion;
                    distance=target.orbit.Nearest(target.Rotates?target.Local(position,time):new Point(target.mirrored?390-local.x:local.x,local.y-target.offset));
                    var contactPosition=position+landingOffset;
                    platform=target;
                    landingOrbitDirection=ChooseLandingDirection(target,distance);
                    if(double.IsPositiveInfinity(target.landedAt))target.landedAt=time;
                    Place();
                    landingOffset=contactPosition-position;
                    landingVelocity=new Point();
                    flying=false;
                    velocity=new Point();
                    highestSection=Math.Max(highestSection,target.section);
                    happyUntil=time+.55;
                    events.Enqueue(new SimulationEvent{kind="land",position=position});
                    Emit(position,8,70);
                    break;
                }
            }
            if(position.x-GameConfig.ContactRadius<=0||position.x+GameConfig.ContactRadius>=390)
            {
                Die("wall");
                return;
            }
            if(!flying&&platform.data.switchSupport)OpenLinkedGate(platform,true);
            foreach(var hazard in hazards)
            {
                if(hazard.vanished)continue;
                var oldMotion=hazard.MotionAt(time-dt);var newMotion=hazard.MotionAt(time);
                // A streamed shape resets outside the lane; never sweep its teleport through the player.
                if(hazard.data.motion?.type=="stream"&&(newMotion-oldMotion).Length>100)oldMotion=newMotion;
                if(Geometry.SweptTouches(before-oldMotion,position-newMotion,hazard.data,hazard.offset)){Die("obstacle");return;}
            }
            if(capturedHole==null)foreach(var hole in blackHoles)
            {
                if(Geometry.Segment(hole.center,before,position).distance>hole.data.pullRadius)continue;
                capturedHole=hole;captureStart=position;captureAge=0;flying=true;landingOffset=new Point();velocity=new Point();
                events.Enqueue(new SimulationEvent{kind="magnet",position=position});break;
            }
            if(position.y>cameraY+viewport+50||(flying&&flightTime>7))
            {
                Die("lost");
                return;
            }
            foreach(var magnet in magnets)
            {
                if(magnet.collected||Geometry.Segment(magnet.position,before,position).distance>GameConfig.Radius+10)continue;
                magnet.collected=true;
                foreach(var star in stars)
                {
                    var point=star.At(time);
                    if(star.collected||star.attracted||point.x<0||point.x>GameConfig.Width||point.y<RenderCamera||point.y>RenderCamera+viewport)continue;
                    star.attracted=true;star.magnetPosition=star.previousMagnetPosition=point;
                    star.magnetTime=time-dt;star.magnetAge=0;
                }
                happyUntil=time+.55;
                events.Enqueue(new SimulationEvent{kind="magnet",position=magnet.position});
            }
            foreach(var s in stars)
            {
                if(s.collected)continue;
                if(s.attracted)
                {
                    s.previousMagnetPosition=s.magnetPosition;s.magnetTime=time-dt;s.magnetAge+=dt;
                    var delta=position-s.magnetPosition;
                    double travel=Math.Min(delta.Length,(220+Math.Min(880,s.magnetAge*900))*dt);
                    s.magnetPosition+=delta.Normal*travel;
                }
                // Recycling is off-screen; its discontinuity must never sweep a reward across the player.
                if(s.parent?.data.motion?.type=="rain"&&(s.parent.MotionAt(time)-s.parent.MotionAt(time-dt)).Length>100)continue;
                // Relative swept pickup tests also catch a star carried across the player.
                if(s.collected||Geometry.Segment(new Point(),before-s.At(time-dt),position-s.At(time)).distance>GameConfig.PickupRadius)continue;
                s.collected=true;
                score++;if(shotPoints.Contains(s))precisionScore++;
                if(energyEnabled)energy=Math.Min(100,energy+(FireflySurvival?10:4));
                happyUntil=time+.55;
                events.Enqueue(new SimulationEvent{kind="star",position=s.At(time)});
                bursts.Add(new Burst
                {
                    position=s.At(time)
                }
                );
                if(bursts.Count>4)bursts.RemoveAt(0);
                Emit(s.At(time),12,170,true);
            }
            if(!flying&&shotPoints.Count>0)
            {
                int count=shotPoints.Count(point=>point.collected);bool perfect=count==shotPoints.Count;
                if(perfect)precisionScore+=count;perfectChain=perfect?perfectChain+1:0;
                if(perfect)events.Enqueue(new SimulationEvent{kind="perfect",position=position,streak=perfectChain});
                shotPoints.Clear();
            }
            if(energyEnabled)
            {
                foreach(var bloom in energyBlooms)
                {
                    if(bloom.collected||bloom.parent.vanished)continue;
                    if(Geometry.Segment(new Point(),before-bloom.At(time-dt),position-bloom.At(time)).distance>GameConfig.Radius+7)continue;
                    bloom.collected=true;energy=Math.Min(100,energy+40);happyUntil=time+.55;
                    events.Enqueue(new SimulationEvent{kind="energy",position=bloom.At(time)});
                }
                if(energy<=0){Die("energy");return;}
            }
            bestY=Math.Min(bestY,position.y);
            ReachCheckpoint();
            if(LavaEnabled&&(!level.discovery||time-checkpointAt>=12))lava.Step(dt,level.discovery?time-8:time,bestY,viewport,level.discovery?0:flying?0:time-lastJumpAt);
            if(LavaEnabled&&lava.Active(time)&&lava.Touches(position)){Die("lava");return;}
            double targetY=bestY-viewport*.7;
            // Carry camera velocity through target changes instead of snapping its scroll rate.
            const double cameraRate=12;
            double error=cameraY-targetY,term=cameraVelocity+cameraRate*error;
            double cameraDecay=Math.Exp(-cameraRate*dt);
            cameraY=targetY+(error+term*dt)*cameraDecay;
            cameraVelocity=(cameraVelocity-cameraRate*term*dt)*cameraDecay;
            int current=platform.section;
            if(nextSection<=current+2)AddSection();
            int oldest=Math.Max(0,current-1);
            // Instances are appended in section order; trim without per-tick capturing predicates.
            int remove=0;while(remove<platforms.Count&&platforms[remove].section<oldest)remove++;
            if(remove>0)platforms.RemoveRange(0,remove);
            remove=0;while(remove<stars.Count&&stars[remove].section<oldest)remove++;
            if(remove>0)stars.RemoveRange(0,remove);
            energyBlooms.RemoveAll(bloom=>bloom.parent.section<oldest);
            magnets.RemoveAll(magnet=>magnet.section<oldest);
            hazards.RemoveAll(hazard=>hazard.section<oldest);
            blackHoles.RemoveAll(hole=>hole.section<oldest);
            trailTime+=dt;
            if(trailTime>=1.0/60)
            {
                trailTime-=1.0/60;
                trail.Add(position);
                if(trail.Count>36)trail.RemoveAt(0);
            }
            if((level.discovery&&sequence==null&&platform.index==level.platforms.Length-1&&!flying)||(!endless&&sequence!=null&&platform.section==sequence.Length-1&&sequence[platform.section].connections!=null&&platform.index==sequence[platform.section].platforms.Length-1&&!flying))
            {
                phase=Phase.Complete;events.Enqueue(new SimulationEvent{kind="land",position=position});
            }
        }
    }
}
