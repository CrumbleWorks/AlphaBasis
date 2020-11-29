using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

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

                switch(status)
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
