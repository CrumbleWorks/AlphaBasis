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
        public class SolarArray
        {
            public string Label { get; internal set; }
            public List<IMySolarPanel> Panels { get; internal set; }
            public IMyMotorStator DrivingRotor { get; internal set; }
            public IMyMotorStator Bearing { get; internal set; }
        }

        const string solarPuterConfigurationKey = "SolarPuterConfiguration";
        const string solarArraysConfigurationKey = "SolarArrays";
        const string displayConfigurationKey = "DisplayGroup";
        const string bearingRotorTag = "[B]";
        const string drivingRotorTag = "[D]";

        readonly MyIni _ini;
        readonly List<SolarArray> _solarArrays;
        readonly List<IMyTextSurface> _displays;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _ini = new MyIni();

            ParseCustomData();

            _solarArrays = InitSolarArrays();
            _displays = GetDisplays();

            ConfigureTextSurfaces(_displays);
        }

        private void ParseCustomData()
        {
            var customData = Me.CustomData;
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }
        }

        private List<SolarArray> InitSolarArrays()
        {
            var solarArrayNames = GetSolarArrayNames();
            var solarArrays = new List<SolarArray>(solarArrayNames.Count);
            foreach (var solarArrayName in solarArrayNames)
            {
                var solarArrayGroup = GridTerminalSystem.GetBlockGroupWithName(solarArrayName);

                if (solarArrayGroup == null)
                {
                    Echo($"No group named '{solarArrayName}' found. Skipping this entry.");
                    continue;
                }

                var panels = new List<IMySolarPanel>();
                var stators = new List<IMyMotorStator>();

                solarArrayGroup.GetBlocksOfType(panels);
                solarArrayGroup.GetBlocksOfType(stators);

                var drivingRotor = GetSingleRotorByTag(stators, drivingRotorTag);
                var bearing = GetSingleRotorByTag(stators, bearingRotorTag);

                solarArrays.Add(new SolarArray { Label = solarArrayName, Panels = panels, DrivingRotor = drivingRotor, Bearing = bearing});
            }
            return solarArrays;
        }
        private List<string> GetSolarArrayNames()
        {
            var solarArrayConfigValues = _ini.Get(solarPuterConfigurationKey, solarArraysConfigurationKey).ToString();
            return solarArrayConfigValues.Split(';').ToList();
        }

        private IMyMotorStator GetSingleRotorByTag(List<IMyMotorStator> stators, string tag)
        {
            var filteredStators = stators.FindAll(s => s.CustomName.Contains(tag));
            if (!filteredStators.Any())
            {
                Echo($"No rotor marked with {tag} found.");
                throw new Exception($"No rotor marked with {tag} found.");
            }
            else if (filteredStators.Count > 1)
            {
                Echo($"Found more then one rotor marked with {tag}. Using first result.");
                return filteredStators[0];
            }
            return filteredStators[0];
        }

        private List<IMyTextSurface> GetDisplays()
        {
            var displayGroupName = GetDisplayGroupName();
            var displayGroup = GridTerminalSystem.GetBlockGroupWithName(displayGroupName);
            
            if (displayGroup == null)
            {
                Echo($"No group named '{displayGroupName}' found. Output unavailable.");
                return new List<IMyTextSurface>();
            }

            var displays = new List<IMyTextSurface>();
            displayGroup.GetBlocksOfType(displays);
            return displays;
        }

        private string GetDisplayGroupName()
        {
            return _ini.Get(solarPuterConfigurationKey, displayConfigurationKey).ToString();
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
            _displays.ForEach(d =>
            {
                d.WriteText("");
                SolarArrayStatus.PrintSolarArraysStatus(d, _solarArrays);
            });
        }
    }
}
