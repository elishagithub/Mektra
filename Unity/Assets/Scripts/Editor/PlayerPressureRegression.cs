using System;
using UnityEngine;
namespace Mektra.Editor
{
    public static class PlayerPressureRegression
    {
        public static void Build(){Run();MektraAndroidRelease.Build();}
        public static void Run()
        {
            var lava=new LavaHazard(1000);
            if(lava.Active(3.99)||!lava.Active(4))throw new Exception("Lava delay must be four seconds");
            double front=lava.Front;lava.Step(GameConfig.Step,4,500,816);
            if(lava.Front!=front)throw new Exception("Lava must start with smooth zero-speed ramp");
            lava.Step(GameConfig.Step,5,500,816);
            if(lava.Front>=front||front-lava.Front>LavaHazard.CatchupSpeed*GameConfig.Step)throw new Exception("Lava rise discontinuity");
            Debug.Log("MEKTRA_PRESSURE_VALIDATION PASS: four-second delay and gradual bounded rise.");
        }
    }
}
