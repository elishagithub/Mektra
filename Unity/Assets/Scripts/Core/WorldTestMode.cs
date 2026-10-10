namespace Mektra
{
    // Temporary playtest shortcut. Set Enabled=false to restore the normal 16-section opening.
    public static class WorldTestMode
    {
        public static bool Enabled=false;
        public const double DelaySeconds=10;
        public static bool ShouldEnter(Simulation sim)=>Enabled&&sim.endless&&sim.sequence!=null&&sim.phase==Phase.Playing&&sim.time>=DelaySeconds&&!sim.flying&&sim.platform.section<16;
        public static Simulation Enter(Simulation live,int worldOrdinal)
        {
            int[] sections={32,48,16,24,40,56,64};
            int section=sections[worldOrdinal%sections.Length];
            var next=new Simulation(live.level,live.character,live.sequence,section,true,live.seed);
            next.time=live.time;next.gearAngle=live.gearAngle;next.score=live.score;
            next.Select(live.character);next.SetViewport(live.viewport);
            next.bestY=next.position.y;next.startY=next.bestY+(live.startY-live.bestY);
            next.Start();return next;
        }
    }
}
