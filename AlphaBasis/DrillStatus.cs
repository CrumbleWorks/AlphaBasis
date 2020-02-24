using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Globalization;
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
        public class DrillStatusDisplay
        {
            private List<IMyInventory> _drillInventories;

            public DrillStatusDisplay(List<IMyInventory> drillInventories)
            {
                this._drillInventories = drillInventories;
            }

            public void PrintDrillStatus(IMyTextSurface textSurface)
            {
                textSurface.WriteText("╔══════════════╗\n", true);
                textSurface.WriteText("║ Drill Status ║\n", true);
                textSurface.WriteText("╚══════════════╝\n", true);

                PrintDrillInventoryStatus(textSurface);
            }

            private void PrintDrillInventoryStatus(IMyTextSurface textSurface)
            {
                var currentVolume = _drillInventories.Sum(i => i.CurrentVolume.RawValue);
                var maxVolume = _drillInventories.Sum(i => i.MaxVolume.RawValue);

                textSurface.WriteText($"Current: {currentVolume,11:0#,0}l\n".Replace(",", "\'"), true);
                textSurface.WriteText($"Max:     {maxVolume,11:0#,0}l\n".Replace(",", "\'"), true);
            }
        }
    }
}
