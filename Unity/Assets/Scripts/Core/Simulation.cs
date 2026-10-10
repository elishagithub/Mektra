using System;
using System.Collections.Generic;
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
        public Point MotionAt(double time)
        {
            var p=data.motion==null?new Point():data.motion.At(lockMotionDuringGuides?GameConfig.GuidedMotionTime(time):time);
            if(mirrored)p.x=-p.x;
            return p;
        }
    }
    public class StarInstance
    {
        public Point position;
        public string id;
        public int section;
        public bool collected;
        public PlatformInstance parent;
        public bool attracted;
        public Point magnetPosition,previousMagnetPosition;
        public double magnetTime,magnetAge;
        public Point At(double time)=>attracted?previousMagnetPosition+(magnetPosition-previousMagnetPosition)*Math.Clamp((time-magnetTime)/GameConfig.Step,0,1):position+(parent==null?new Point():parent.MotionAt(time));
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
    public struct SimulationEvent { public string kind; public Point position; }
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
        public double distance,flightTime,cameraY,previousCamera,gearAngle,previousGear,time,accumulator,viewport=700,bestY,startY,happyUntil;
        public bool flying;
        public bool AutoBouncing=>flying&&wedgeBounces>0;
        public int wedgeBounces;
        public bool BeingPulled=>capturedHole!=null;

        BlackHoleInstance capturedHole; Point captureStart; double captureAge;
        public double BodyScale=>capturedHole==null?1:Math.Max(.15,1-captureAge/.85);
        public LavaHazard lava;
        public double RenderLava=>lava.Render(Alpha);
        public PlatformInstance platform;
        public int score,highestSection,nextSection;
        public bool energyEnabled;
        public double energy=100,energyAge;
        public double EnergySpeed=>!energyEnabled?1:.8+.2*Math.Clamp(energy/20,0,1);
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
        public int Distance=>(int)Math.Floor((startY-bestY)/10);
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
            if(platform.mirrored){p.x=390-p.x;normal.x=-normal.x;}
            position=Orbit.Ground(p,normal,gearAngle,character);
            position.y+=platform.offset;
            position+=platform.MotionAt(time);
        }
        void AddSection()
        {
            int section=nextSection++;
            var course=CourseAt(section);
            if(!orbitCache.TryGetValue(course,out var courseOrbits))
            {
                courseOrbits=new Orbit[course.platforms.Length];
                for(int i=0;i<courseOrbits.Length;i++)courseOrbits[i]=new Orbit(course.platforms[i].outline);
                orbitCache.Add(course,courseOrbits);
            }
            bool reflected=sequence==null&&section%3==1;
            double offset=-section*level.stride;
            for(int i=0;i<course.platforms.Length;i++)platforms.Add(new PlatformInstance
            {
                index=i,section=section,offset=offset,data=course.platforms[i],orbit=courseOrbits[i],reflected=reflected,lockMotionDuringGuides=endless
            }
            );
            // Stars sustain regular climbing; a blue bloom is an occasional recovery opportunity.
            for(int i=sequence==null?1:course.platforms.Length-1;(sequence==null||section%6==4)&&i<course.platforms.Length;i+=2)
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
                position=new Point(reflected?390-s.x:s.x,s.y+offset),section=section,id=s.id,
                parent=string.IsNullOrEmpty(s.parentPlatformId)?null:platforms.Find(p=>p.section==section&&p.data.id==s.parentPlatformId)
            }
            );
            if(sequence==null&&section%3==2&&level.challengeStars!=null)foreach(var s in level.challengeStars)stars.Add(new StarInstance{position=new Point(s.x,s.y+offset),section=section});
            // In endless play, omit opening pickups and nominate just one rare section per wave.
            for(int index=0;!course.suppressMagnets&&(!endless||EndlessJourney.HasMagnet(section,seed))&&index+1<course.platforms.Length;index+=6)
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
        public void EnsureSection(int section){while(nextSection<=section)AddSection();}
        public void Start()
        {
            if(phase==Phase.Ready){phase=Phase.Playing;platform.landedAt=time;}
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
        public void Jump()
        {
            if(phase!=Phase.Playing||flying)return;
            if(energyEnabled&&energyAge>25)energy=Math.Max(0,energy-1);
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
            if(energyEnabled){energyAge+=dt;if(energyAge>25)energy=Math.Max(0,energy-.65*dt);}
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
                distance=(distance+Pace*dt*(platform.data.orbitDirection<0?-1:1)+platform.orbit.length)%platform.orbit.length;
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
                    if(!Geometry.Touches(position-motion,target.data,target.offset,target.mirrored))continue;
                    if(target.data.sourceShapeId=="wedge")
                    {
                        var localPoint=position-motion-new Point(0,target.offset);double nearest=double.PositiveInfinity;Point hit=new Point(),face=new Point();
                        for(int edge=0;edge<target.data.outline.Length;edge++)
                        {
                            var a=target.data.outline[edge];var b=target.data.outline[(edge+1)%target.data.outline.Length];
                            if(target.mirrored){a.x=390-a.x;b.x=390-b.x;}
                            var contact=Geometry.Segment(localPoint,a,b);var n=new Point(b.y-a.y,a.x-b.x).Normal;if(target.mirrored)n=n*-1;
                            if(contact.distance<nearest){nearest=contact.distance;hit=contact.point;face=n;}
                        }
                        double approach=velocity.x*face.x+velocity.y*face.y;
                        if(approach<0)
                        {
                            velocity=velocity-face*(2*approach); // Elastic reflection: angle in equals angle out; preserve speed.
                            position=hit+face*(GameConfig.Radius+.1)+motion+new Point(0,target.offset);
                            normal=face;wedgeBounces++;landingOffset=new Point();Emit(position,8,70);
                            events.Enqueue(new SimulationEvent{kind="land",position=position});
                        }
                        continue;
                    }
                    var local=position-motion;
                    distance=target.orbit.Nearest(new Point(target.mirrored?390-local.x:local.x,local.y-target.offset));
                    var contactPosition=position+landingOffset;
                    platform=target;
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
            if(position.x-GameConfig.Radius<=0||position.x+GameConfig.Radius>=390)
            {
                Die("wall");
                return;
            }
            foreach(var hazard in hazards)
            {
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
                // Relative swept pickup tests also catch a star carried across the player.
                if(s.collected||Geometry.Segment(new Point(),before-s.At(time-dt),position-s.At(time)).distance>GameConfig.CollectionRadius)continue;
                s.collected=true;
                score++;
                if(energyEnabled)energy=Math.Min(100,energy+4);
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
            lava.Step(dt,time,bestY,viewport);
            if(lava.Active(time)&&lava.Touches(position)){Die("lava");return;}
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
            if(!endless&&sequence!=null&&platform.section==sequence.Length-1&&sequence[platform.section].connections!=null&&platform.index==sequence[platform.section].platforms.Length-1&&!flying)
            {
                phase=Phase.Complete;events.Enqueue(new SimulationEvent{kind="land",position=position});
            }
        }
    }
}
