using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class SolarArrayStatus
        {
            public static void PrintSolarArraysStatus(IMyTextSurface textSurface, List<SolarArray> solarArrays)
            {
                PrintHeader(textSurface);
                solarArrays.ForEach(sa => PrintStatus(textSurface, sa));
            }

            private static void PrintHeader(IMyTextSurface textSurface)
            {
                textSurface.WriteText("╔══════════════╗\n", true);
                textSurface.WriteText("║ Solar Arrays ║\n", true);
                textSurface.WriteText("╚══════════════╝\n", true);
            }

            private static void PrintStatus(IMyTextSurface textSurface, SolarArray solarArray)
            {
                textSurface.WriteText($"{solarArray.Label}: ", true);
                textSurface.WriteText($"{solarArray.CurrentOutput:F3}MW / {solarArray.MaxOutput:F3}MW ", true);
                PrintMovementStatus(textSurface, solarArray);
                textSurface.WriteText("\n", true);
            }

            private static void PrintMovementStatus(IMyTextSurface textSurface, SolarArray solarArray)
            {
                var status = solarArray.MovementStatus;

                switch (status)
                {
                    case SolarArrayMovementStatus.ReturnToStartingPosition:
                        textSurface.WriteText("◄◄◄", true);
                        break;
                    case SolarArrayMovementStatus.FollowingSun:
                        textSurface.WriteText("►►►", true);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
