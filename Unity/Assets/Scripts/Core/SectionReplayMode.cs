namespace Mektra
{
        // Normal runs always restart from the opening.
    public static class SectionReplayMode
    {
        public static bool Enabled=false;
        public static bool CycleAllLayoutsForTesting=false;
        public static int InitialSeed()=>17;
        public static Simulation Restart(Simulation live)
        {
            var next=new Simulation(live.level,live.character,live.sequence,live.platform.section,live.endless,live.seed);
            next.SetViewport(live.viewport);
            next.startY=live.startY;
            return next;
        }
    }
}
