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
    partial class Program
    {
        public class PistonData
        {
            public IMyPistonBase Piston { get; private set; }
            public string Label { get; private set; }

            public float CurrentLength { get { return Piston.CurrentPosition - Piston.MinLimit; } }
            public float MaxLength { get { return Piston.MaxLimit - Piston.MinLimit; } }

            public PistonData(IMyPistonBase piston, string label)
            {
                this.Piston = piston;
                this.Label = label;
            }
        }

        public class PistonStatusDisplay : MyGridProgram
        {
            public static void PrintPistonStatusShort(IMyTextSurface textSurface, List<PistonData> pistons)
            {
                PrintHeader(textSurface);
                PrintTotalSpeedAndVelocity(textSurface, pistons);
            }

            public static void PrintPistonsStatusLong(IMyTextSurface textSurface, List<PistonData> pistons)
            {
                PrintHeader(textSurface);
                PrintPistonStatus(textSurface, pistons);
                PrintTotalSpeedAndVelocity(textSurface, pistons);
            }

            private static void PrintHeader(IMyTextSurface textSurface)
            {
                textSurface.WriteText("╔═══════════════╗\n", true);
                textSurface.WriteText("║ Piston Status ║\n", true);
                textSurface.WriteText("╚═══════════════╝\n", true);
            }

            public static void PrintPistonStatus(IMyTextSurface textSurface, List<PistonData> pistons)
            {
                pistons.ForEach(p => PrintStatus(textSurface, p));
                textSurface.WriteText("─────────────────\n", true);
            }

            private static void PrintTotalSpeedAndVelocity(IMyTextSurface textSurface, List<PistonData> pistons)
            {
                var totalVelocity = pistons.Sum(p => p.Piston.Velocity);
                var totalLength = pistons.Sum(p => p.Piston.CurrentPosition);
                var maxLenght = pistons.Sum(p => p.MaxLength);

                textSurface.WriteText($"Total velocity: {totalVelocity,7:F4}m/s\n", true);
                textSurface.WriteText($"Total length:   {totalLength,7:F4}m\n", true);
                textSurface.WriteText($"Max length:     {maxLenght,7:F4}m\n", true);
            }

            private static void PrintStatus(IMyTextSurface textSurface, PistonData pistonData)
            {
                textSurface.WriteText($"{pistonData.Label}: ", true);

                var velocity = pistonData.Piston.Velocity;
                var currPos = pistonData.Piston.CurrentPosition;

                PrintOnOffStatus(textSurface, pistonData.Piston);
                textSurface.WriteText($"{currPos,7:F4}m @ ", true);
                textSurface.WriteText($"{velocity,6:F4}m/s ", true);
                PrintExtensionStatus(textSurface, pistonData.Piston);
                textSurface.WriteText("\n", true);
            }

            private static void PrintOnOffStatus(IMyTextSurface textSurface, IMyPistonBase piston)
            {
                if (piston.Enabled)
                {
                    textSurface.WriteText("On  ", true);
                }
                else
                {
                    textSurface.WriteText("Off ", true);
                }
            }

            private static void PrintExtensionStatus(IMyTextSurface textSurface, IMyPistonBase piston)
            {
                var status = piston.Status;
                var level = GetExtensionLevel(piston);

                if (status == PistonStatus.Retracting)
                {
                    textSurface.WriteText("◄", true);
                }
                for (var i = 0; i < level; i++)
                {
                    textSurface.WriteText("█", true);
                }
                if (status == PistonStatus.Extending)
                {
                    textSurface.WriteText("►", true);
                }
            }

            private static int GetExtensionLevel(IMyPistonBase piston)
            {
                var lowPos = piston.LowestPosition;
                var highPos = piston.HighestPosition;
                var currPos = piston.CurrentPosition;

                if (currPos == lowPos)
                {
                    return 0;
                }
                else
                {
                    return (int)((currPos / ((highPos - lowPos) / 4)) + 1);
                }
            }
        }
    }
}
