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
        public class InventoryStatusDisplay
        {
            public static void PrintInventoryStatus(IMyTextSurface textSurface, List<IMyInventory> inventories)
            {
                textSurface.WriteText("╔══════════════════╗\n", true);
                textSurface.WriteText("║ Inventory Status ║\n", true);
                textSurface.WriteText("╚══════════════════╝\n", true);

                var currentVolume = inventories.Sum(i => i.CurrentVolume.RawValue);
                var maxVolume = inventories.Sum(i => i.MaxVolume.RawValue);

                textSurface.WriteText($"Current: {currentVolume,11:0#,0}l\n".Replace(",", "\'"), true);
                textSurface.WriteText($"Max:     {maxVolume,11:0#,0}l\n".Replace(",", "\'"), true);
            }
        }
    }
}
