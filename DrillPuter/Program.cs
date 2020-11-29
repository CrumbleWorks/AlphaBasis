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

    partial class Program : MyGridProgram
    {
        public class Drill
        {
            public IMyMotorStator Stator { get; internal set; }
            public List<PistonData> Pistons { get; internal set; }
            public List<IMyInventory> Inventories { get; internal set; }
            public List<IMyTextSurface> InformationDisplays { get; internal set; }
            public List<IMyTextSurface> DetailsDisplays { get; internal set; }
        }

        const string drillPuterConfigurationKey = "DrillPuterConfiguration";
        const string drillArmTag = "[A]";
        const string informationDisplayTag = "[I]";
        const string detailsDisplayTag = "[D]";
        const string mainMotorStatorTag = "[S]";

        readonly MyIni _ini;
        readonly List<Drill> _drills;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _ini = new MyIni();
            _drills = InitDrills();
            ConfigureDisplays();
        }

        private List<Drill> InitDrills()
        {
            var drillNames = GetDrillNames();
            var drills = new List<Drill>(drillNames.Count);
            foreach (var drillName in drillNames)
            {
                var drillGroup = GridTerminalSystem.GetBlockGroupWithName(drillName);

                if (drillGroup == null)
                {
                    Echo($"No group named '{drillName}' found. Skipping this entry.");
                    continue;
                }

                var blocks = new List<IMyTerminalBlock>();
                drillGroup.GetBlocks(blocks);

                var inventories = blocks.FindAll(b => b.HasInventory).Select(b => b.GetInventory()).ToList();
                var stators = new List<IMyMotorStator>();
                var allPistonBases = new List<IMyPistonBase>();
                var displays = new List<IMyTextSurface>();

                drillGroup.GetBlocksOfType(stators);
                drillGroup.GetBlocksOfType(allPistonBases);
                drillGroup.GetBlocksOfType(displays);

                var informationDisplays = FilterTextSurfacesByTag(displays, informationDisplayTag);
                var detailsDisplays = FilterTextSurfacesByTag(displays, detailsDisplayTag);
                var armPistons = GetArmPistons(allPistonBases);
                var drillPistons = allPistonBases.Except(armPistons).ToList();

                var pistons = InitPistonData(armPistons);
                var mainStator = GetMainStator(stators);

                drills.Add(new Drill { Stator = mainStator, Pistons = pistons, Inventories = inventories, InformationDisplays = informationDisplays, DetailsDisplays = detailsDisplays });
            }
            return drills;
        }

        private List<string> GetDrillNames()
        {
            var customData = Me.CustomData;
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }
            var drillNameKeys = new List<MyIniKey>();
            _ini.GetKeys(drillPuterConfigurationKey, drillNameKeys);
            var drillNames = new List<string>(drillNameKeys.Count);
            foreach (var drillNameKey in drillNameKeys)
            {
                var drillPrefix = _ini.Get(drillPuterConfigurationKey, drillNameKey.Name).ToString();
                drillNames.Add(drillPrefix);
            }
            return drillNames;
        }

        private List<IMyTextSurface> FilterTextSurfacesByTag(List<IMyTextSurface> textSurfaces, string tag)
        {
            return textSurfaces.FindAll(ts => (ts as IMyTerminalBlock).CustomName.Contains(tag));
        }

        private List<IMyPistonBase> GetArmPistons(List<IMyPistonBase> pistons)
        {
            return pistons.FindAll(pb => pb.CustomName.Contains(drillArmTag));
        }

        private IMyMotorStator GetMainStator(List<IMyMotorStator> stators)
        {
            var mainStators = stators.FindAll(s => s.CustomName.Contains(mainMotorStatorTag));
            if (!mainStators.Any())
            {
                Echo($"No rotor marked with {mainMotorStatorTag} found. No rotation information will be displayed.");
                return null;
            }
            else if (mainStators.Count > 1)
            {
                Echo($"Found more then one rotor marked with {mainMotorStatorTag}. Using first result.");
                return mainStators[0];
            }
            return mainStators[0];
        }

        private List<PistonData> InitPistonData(List<IMyPistonBase> pistonBases)
        {
            var pistons = new List<PistonData>(pistonBases.Count);
            for (int i = 0; i < pistonBases.Count; i++)
            {
                var pistonBase = pistonBases[i];
                pistons.Add(new PistonData(pistonBase, (i + 1).ToString(), pistonBase.HighestPosition / 2));
            }
            return pistons;
        }

        private void ConfigureDisplays()
        {
            foreach (var drill in _drills)
            {
                ConfigureTextSurfaces(drill.InformationDisplays);
                ConfigureTextSurfaces(drill.DetailsDisplays);
            }
        }

        private void ConfigureTextSurfaces(List<IMyTextSurface> textSurfaces)
        {
            foreach (var textSurface in textSurfaces)
            {
                textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                textSurface.BackgroundColor = Color.Black;
                textSurface.Font = "Monospace";
                textSurface.FontSize = 0.75f;
                textSurface.Alignment = TextAlignment.LEFT;
                textSurface.WriteText("", false);
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _drills.ForEach(d => PrintDrillStatus(d));
        }

        private void PrintDrillStatus(Drill drill)
        {
            drill.InformationDisplays.ForEach(id =>
            {
                id.WriteText("");

                PistonStatusDisplay.PrintPistonStatusShort(id, drill.Pistons);
                id.WriteText("\n", true);
                RotorStatusDisplay.PrintRotorStatus(id, drill.Stator);
                id.WriteText("\n", true);
                InventoryStatusDisplay.PrintInventoryStatus(id, drill.Inventories);
            });
            drill.DetailsDisplays.ForEach(dd =>
            {
                dd.WriteText("");

                PistonStatusDisplay.PrintPistonsStatusLong(dd, drill.Pistons);
            });

        }
    }
}
