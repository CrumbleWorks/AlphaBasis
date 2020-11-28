using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public class RotorStatusDisplay
        {
            public static void PrintRotorStatus(IMyTextSurface textSurface, IMyMotorStator stator)
            {
                textSurface.WriteText("╔══════════════╗\n", true);
                textSurface.WriteText("║ Rotor Status ║\n", true);
                textSurface.WriteText("╚══════════════╝\n", true);

                var velocity = stator.TargetVelocityRPM;
                var angleDeg = RadianToDegree(stator.Angle);

                textSurface.WriteText($"Velocity: {velocity:F3}rpm\n", true);
                textSurface.WriteText($"Angle:    {angleDeg:F0}°\n", true);
            }

            private static double RadianToDegree(double angle)
            {
                return angle * (180.0f / Math.PI);
            }
        }
    }
}
