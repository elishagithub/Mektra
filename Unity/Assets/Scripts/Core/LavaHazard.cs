using System;
namespace Mektra
{
    // World Y points downward. The front never teleports or retreats; pause freezes its clock.
    public sealed class LavaHazard
    {
        public const double GraceSeconds=4,BaseSpeed=18,CatchupSpeed=52,EntryRampSeconds=6,StartBelowScreen=90;
        public double Front {get;private set;}
        double previous,stalled,progressY=double.NaN;
        readonly double startedAt;
        public LavaHazard(double bottom,double startedAt=0){Front=previous=bottom+StartBelowScreen;this.startedAt=startedAt;}
        public double Render(double alpha)=>previous+(Front-previous)*alpha;
        public bool Active(double time)=>stalled>=3.5||time-startedAt>=GraceSeconds;
        public void Step(double dt,double time,double bestY,double viewport,double secondsSinceJump=double.NaN)
        {
            previous=Front;
            if(double.IsNaN(progressY)||bestY<progressY-12){progressY=bestY;stalled=0;}else stalled+=dt;
            if(!double.IsNaN(secondsSinceJump))stalled=secondsSinceJump;
            if(!Active(time))return;
            // Gently catch up after successful climbing, without suddenly trapping the player.
            double gap=Front-(bestY+viewport*.45);
            double catchup=Math.Clamp(gap/300,0,1);
            double speed=BaseSpeed+(CatchupSpeed-BaseSpeed)*catchup;
            double ramp=Math.Clamp((time-startedAt-GraceSeconds)/EntryRampSeconds,0,1);
            double rise=speed*(ramp*ramp*(3-2*ramp));
            if(stalled>=3.5)rise=Math.Max(rise,48*Math.Clamp((stalled-3.5)/.3,0,1));
            Front-=rise*dt;
        }
        public bool Touches(Point player)=>player.y+GameConfig.ContactRadius>=Front;
    }
}
