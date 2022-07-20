using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

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

                if (!inventories.Any())
                {
                    textSurface.WriteText("No inventories configured.\n", true);
                    return;
                }

                var currentVolume = inventories.Sum(i => i.CurrentVolume.RawValue);
                var maxVolume = inventories.Sum(i => i.MaxVolume.RawValue);

                textSurface.WriteText($"Current: {currentVolume,11:0#,0}l\n".Replace(",", "\'"), true);
                textSurface.WriteText($"Max:     {maxVolume,11:0#,0}l\n".Replace(",", "\'"), true);
            }
        }
    }
}
