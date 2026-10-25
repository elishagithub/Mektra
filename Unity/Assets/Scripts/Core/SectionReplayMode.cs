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
        var next=new Simulation(live.level,live.character,live.sequence,0,live.endless,InitialSeed());
            next.SetViewport(live.viewport);
            return next;
        }
    }
}
