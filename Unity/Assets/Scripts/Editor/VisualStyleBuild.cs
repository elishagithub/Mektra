namespace Mektra.Editor
{
    public static class VisualStyleBuild
    {
        public static void Build(){ForestMenuRegression.Run();FrostedForestRegression.Run();PlatformScaleRegression.Run(ContinuousJourney.Courses(LevelPack.Load().JourneyCourses()));ContinuousJourneyRegression.Run();MektraBuild.BuildValidatedOpening(LevelPack.Load(),true);}
    }
}
