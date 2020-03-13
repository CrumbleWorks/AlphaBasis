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
        MyIni _ini;

        ResourcesDisplayConfiguration _config;
        List<IMyTextSurface> _drawingSurfaces;
        
        PowerDisplay _powerDisplay;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _ini = new MyIni();            
            _config = ReadConfiguration();

            ConfigureDrawingSurfaces();

            var batteries = GetBatteries();
            var cargos = GetCargos();
            _powerDisplay = new PowerDisplay(batteries, cargos, _config.CargoGroups);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _drawingSurfaces.ForEach(ds => ds.WriteText("", false)); //reset displays
            _drawingSurfaces.ForEach(ds => _powerDisplay.PrintStatus(ds));
        }

        private ResourcesDisplayConfiguration ReadConfiguration()
        {
            var customData = Me.CustomData;

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            var lcdGroupName = _ini.Get("ResourcesDisplayConfig", "LCDGroup").ToString();
            var cargoGroups = _ini.Get("ResourcesDisplayConfig", "CargoGroups").ToString().Split(',');

            return new ResourcesDisplayConfiguration { LCDGroup = lcdGroupName, CargoGroups = getDiffCargoGroups(cargoGroups) };
        }

        private List<IMyBatteryBlock> GetBatteries()
        {
            var batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries);
            return batteries;
        }

        private List<IMyInventory> GetCargos()
        {
            var containers = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType(containers);

            var inventories = new List<IMyInventory>();
            foreach (var container in containers)
            {
                if (container.HasInventory)
                {
                    inventories.Add(container.GetInventory());
                }
            }
            return inventories;
        }

        public Dictionary<string, IEnumerable<IMyInventory>> getDiffCargoGroups(string[] cargoGroupNames)
        {
            var grops = new Dictionary<string, IEnumerable<IMyInventory>>(cargoGroupNames.Length);
            foreach (var cargoGroup in cargoGroupNames)
            {
                var group = GridTerminalSystem.GetBlockGroupWithName(cargoGroup);
                var containers = new List<IMyCargoContainer>();
                group.GetBlocksOfType(containers);
                var inventories = new List<IMyInventory>();
                foreach (var container in containers)
                {
                    if (container.HasInventory)
                    {
                        inventories.Add(container.GetInventory());
                    }
                }
                grops.Add(cargoGroup, inventories);
            }

            return grops;
        }

        private void ConfigureDrawingSurfaces()
        {
            var textSurfaces = new List<IMyTextSurface>();
            GridTerminalSystem.GetBlockGroupWithName(_config.LCDGroup).GetBlocksOfType(textSurfaces);
            _drawingSurfaces = textSurfaces;
            _drawingSurfaces.ForEach(drawingSurface =>
            {
                drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                drawingSurface.BackgroundColor = Color.Black;
                drawingSurface.Font = "Monospace";
                drawingSurface.FontSize = 1.0f;
                drawingSurface.FontColor = Color.DarkOrange;
                drawingSurface.Alignment = TextAlignment.LEFT;
                drawingSurface.WriteText("", false);
            });            
        }
    }

    public class PowerDisplay {
        private List<IMyBatteryBlock> _batteries;
        private List<IMyInventory> _cargos;
        private IDictionary<string, IEnumerable<IMyInventory>> _CargoCargos;

        public PowerDisplay(List<IMyBatteryBlock> batteries, List<IMyInventory> cargos, IDictionary<string, IEnumerable<IMyInventory>> cargogos)
        {
            _batteries = batteries;
            _cargos = cargos;
            _CargoCargos = cargogos;
        }

        public void PrintStatus(IMyTextSurface textSurface)
        {
            PrintHeader(textSurface);

            //POWER
            var totalCurrentInput = _batteries.Sum(b => b.CurrentInput);
            textSurface.WriteText($"\nIn:  {totalCurrentInput:F3}MW", true);

            var totalCurrentOutput = _batteries.Sum(b => b.CurrentOutput);
            textSurface.WriteText($"\nOut: {totalCurrentOutput:F3}MW", true);

            var trend = totalCurrentInput - totalCurrentOutput;
            textSurface.WriteText($"\nProduction: {(trend > 0 ? "↑" : trend < 0 ? "↓" : "=")}", true);

            var totalCurrentStored = _batteries.Sum(b => b.CurrentStoredPower);
            var totalMaxStored = _batteries.Sum(b => b.MaxStoredPower);
            textSurface.WriteText($"\nStore: {totalCurrentStored:F3} / {totalMaxStored:F3} MWh", true);

            //CARGO
            textSurface.WriteText("\n", true);
            textSurface.WriteText("\nAll Cargos:", true);
            var currentCargo = _cargos.Sum(c => c.CurrentVolume.RawValue / 1000); //m^3 to l
            var currentCargoPrint = makeNumbersReadable(currentCargo);
            var totalCargo = _cargos.Sum(c => c.MaxVolume.RawValue / 1000); //m^3 to l
            var totalCargoPrint = makeNumbersReadable(totalCargo);
            textSurface.WriteText($"\n{currentCargoPrint} / {totalCargoPrint}", true);

            foreach (var carandache in _CargoCargos)
            {
                textSurface.WriteText($"\n{carandache.Key}", true);
                var carandacheCargo = carandache.Value.Sum(c => c.CurrentVolume.RawValue / 1000); //m^3 to l
                var carandacheCargoPrint = makeNumbersReadable(carandacheCargo);
                var totaldacheCargo = carandache.Value.Sum(c => c.MaxVolume.RawValue / 1000); //m^3 to l
                var totaldacheCargoPrint = makeNumbersReadable(totaldacheCargo);
                textSurface.WriteText($"\n{carandacheCargoPrint} / {totaldacheCargoPrint}", true);
            }
        }

        private static string makeNumbersReadable(long itemAmount)
        {
            var itemUnit = " l";

            if (itemAmount > 9999999999L)
            {
                itemUnit = "Gl";
                itemAmount /= 1000000000;
            }
            else if (itemAmount > 9999999L)
            {
                itemUnit = "Ml";
                itemAmount /= 1000000;
            }
            else if (itemAmount > 9999L)
            {
                itemUnit = "kl";
                itemAmount /= 1000;
            }

            return $"{itemAmount,5:#,0} {itemUnit}";
        }

        private void PrintHeader(IMyTextSurface textSurface)
        {
            textSurface.WriteText("╔═════════════════╗", true);
            textSurface.WriteText("\n║ Resource Status ║", true);
            textSurface.WriteText("\n╚═════════════════╝", true);
        }
    }

    public class ResourcesDisplayConfiguration
    {
        public string LCDGroup { get; internal set; }
        public IDictionary<string, IEnumerable<IMyInventory>> CargoGroups { get; internal set;  }
    }
}
