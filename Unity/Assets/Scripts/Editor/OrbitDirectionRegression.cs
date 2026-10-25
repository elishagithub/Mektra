using System;
using UnityEngine;
namespace Mektra.Editor
{
    static class OrbitDirectionRegression
    {
        public static void Run()
        {
            Check(10,30,100,1,1);Check(30,10,100,1,-1);
            Check(95,5,100,1,1);Check(5,95,100,1,-1);
            Check(10,60,100,-1,-1);Check(10,10,100,-1,-1);
            Debug.Log("MEKTRA_ORBIT_DIRECTION PASS: both directions, wraparound, ties and arrival stability.");
        }
        static void Check(double a,double b,double length,int tie,int expected)
        {
            if(Simulation.ShorterArcDirection(a,b,length,tie)!=expected)throw new Exception("Shortest landing arc failed");
        }
    }
}
