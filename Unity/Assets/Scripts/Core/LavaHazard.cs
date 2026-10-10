using System;
namespace Mektra
{
    // World Y points downward. The front never teleports or retreats; pause freezes its clock.
    public sealed class LavaHazard
    {
        public const double GraceSeconds=8,BaseSpeed=18,CatchupSpeed=52,EntryRampSeconds=6,StartBelowScreen=90;
        public double Front {get;private set;}
        double previous;
        readonly double startedAt;
        public LavaHazard(double bottom,double startedAt=0){Front=previous=bottom+StartBelowScreen;this.startedAt=startedAt;}
        public double Render(double alpha)=>previous+(Front-previous)*alpha;
        public bool Active(double time)=>time-startedAt>=GraceSeconds;
        public void Step(double dt,double time,double bestY,double viewport)
        {
            previous=Front;
            if(!Active(time))return;
            // Gently catch up after successful climbing, without suddenly trapping the player.
            double gap=Front-(bestY+viewport*.45);
            double catchup=Math.Clamp(gap/300,0,1);
            double speed=BaseSpeed+(CatchupSpeed-BaseSpeed)*catchup;
            double ramp=Math.Clamp((time-startedAt-GraceSeconds)/EntryRampSeconds,0,1);
            Front-=speed*(ramp*ramp*(3-2*ramp))*dt;
        }
        public bool Touches(Point player)=>player.y+GameConfig.Radius>=Front;
    }
}
