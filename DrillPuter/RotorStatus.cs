using Sandbox.ModAPI.Ingame;
using System;

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

                if (stator == null)
                {
                    textSurface.WriteText("Main stator not configured.\n", true);
                    return;
                }

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
