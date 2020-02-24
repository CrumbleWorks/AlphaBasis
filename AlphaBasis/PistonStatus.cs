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
            private List<PistonData> _pistons;

            public PistonStatusDisplay(List<PistonData> pistons)
            {
                this._pistons = pistons;
            }

            public void PrintPistonStatus(IMyTextSurface textSurface)
            {
                textSurface.WriteText("╔═══════════════╗\n", true);
                textSurface.WriteText("║ Piston Status ║\n", true);
                textSurface.WriteText("╚═══════════════╝\n", true);

                _pistons.ForEach(p => PrintStatus(textSurface, p));

                textSurface.WriteText("─────────────────\n", true);

                PrintTotalSpeedAndVelocity(textSurface);
            }

            private void PrintTotalSpeedAndVelocity(IMyTextSurface textSurface)
            {
                var totalVelocity = _pistons.Sum(p => p.Piston.Velocity);
                var totalLength = _pistons.Sum(p => p.Piston.CurrentPosition);

                textSurface.WriteText($"Total velocity: {totalVelocity,7:F4}m/s\n", true);
                textSurface.WriteText($"Total length:   {totalLength,7:F4}m", true);
            }

            private void PrintStatus(IMyTextSurface textSurface, PistonData pistonData)
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

            private void PrintOnOffStatus(IMyTextSurface textSurface, IMyPistonBase piston)
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

            private void PrintExtensionStatus(IMyTextSurface textSurface, IMyPistonBase piston)
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

            private int GetExtensionLevel(IMyPistonBase piston)
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
    
        private class PistonExtensionStatusDisplay
        {
            private List<PistonData> _pistons;

            public PistonExtensionStatusDisplay(List<PistonData> pistons)
            {
                this._pistons = pistons;
            }

            public void PrintPistonExtensionStatus(IMyTextSurface textSurface)
            {
                PrintHeader(textSurface);
                PrintTopLine(textSurface);
                PrintPistonLengths(textSurface);
                PrintBottomLine(textSurface);
                PrintLengthLabels(textSurface);
            }

            private void PrintHeader(IMyTextSurface textSurface)
            {
                textSurface.WriteText("╔══════════════════╗\n", true);
                textSurface.WriteText("║ Piston Extension ║\n", true);
                textSurface.WriteText("╚══════════════════╝\n", true);
            }

            private void PrintTopLine(IMyTextSurface textSurface)
            {
                textSurface.WriteText("┌", true);
                var it = _pistons.GetEnumerator();
                while(it.MoveNext())
                {
                    var data = it.Current;
                    var length = data.MaxLength;

                    for (int i = 0; i < length - 1; i++) {
                        textSurface.WriteText("─", true);
                    }
                    if (it.MoveNext())
                    {
                        textSurface.WriteText("┬", true);
                    }
                }
                textSurface.WriteText("┐\n", true);
            }

            private void PrintPistonLengths(IMyTextSurface textSurface)
            {
                textSurface.WriteText("│", true);
                _pistons.ForEach(p => PrintSinglePistonLength(textSurface, p));
                textSurface.WriteText("\n", true);
            }

            private void PrintSinglePistonLength(IMyTextSurface textSurface, PistonData data)
            {
                var length = data.CurrentLength;
                if (length > 0)
                {
                    var statusSymbol = GetPistonStatusSymbol(data);
                    for (int i = 0; i < length - 1; i++)
                    {
                        textSurface.WriteText(statusSymbol, true);
                    }
                    textSurface.WriteText("╣", true);
                }
            }

            private void PrintBottomLine(IMyTextSurface textSurface)
            {
                textSurface.WriteText("├", true);
                for (int i = 1; i < GetMaxTotalLength(); i++)
                {
                    if (i % 10 == 0)
                    {
                        textSurface.WriteText("┼", true);
                    }
                    else
                    {
                        textSurface.WriteText("─", true);
                    }
                }
                textSurface.WriteText("┤\n", true);
            }

            private void PrintLengthLabels(IMyTextSurface textSurface)
            {
                for (int i = 0; i <= GetMaxTotalLength(); i++)
                {
                    if (i % 10 == 0)
                    {
                        textSurface.WriteText(i.ToString(), true);
                        if (i >= 10) // TODO
                        {
                            i++;
                        }
                    }
                    else
                    {
                        textSurface.WriteText(" ", true);
                    }
                }
                textSurface.WriteText("\n", true);
            }
            
            private int GetMaxTotalLength()
            {
                return (int)_pistons.Sum(p => p.MaxLength);
            }

            private string GetPistonStatusSymbol(PistonData data)
            {
                switch (data.Piston.Status)
                {
                    case PistonStatus.Retracting:
                        return "<";
                    case PistonStatus.Extending:
                        return ">";
                    default:
                        return "═";
                }
            }
        }
    }
}
