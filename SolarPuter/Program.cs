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

            public float CurrentOutput { get { return Panels.Sum(p => p.CurrentOutput); } }
            public float MaxOutput { get { return Panels.Sum(p => p.MaxOutput); } }
            public float PreviousMaxOutput { get; set; }

            public SolarArrayMovementStatus MovementStatus { get; set; }
        }

        public enum SolarArrayMovementStatus
        {
            Stopped,
            FollowingSun,
            ReturnToStartingPosition
        }

        const string solarPuterConfigurationKey = "SolarPuterConfiguration";
        const string solarArraysConfigurationKey = "SolarArrays";
        const string displayConfigurationKey = "DisplayGroup";
        const string bearingRotorTag = "[B]";
        const string drivingRotorTag = "[D]";

        const float followingVelocity = 0.01f;
        const float returnVelocity = -0.1f;

        const float epsilonPanelAngel = 0.00174533f; // 0.1 deg in rad

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

                solarArrays.Add(new SolarArray { Label = solarArrayName, Panels = panels, DrivingRotor = drivingRotor, Bearing = bearing, PreviousMaxOutput = 0, MovementStatus = SolarArrayMovementStatus.Stopped});
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
            PrintStatus();
            RotateSolarArrays();
            UpdatePreviousOutput();
        }

        private void PrintStatus()
        {
            _displays.ForEach(d =>
            {
                d.WriteText("");
                SolarArrayStatus.PrintSolarArraysStatus(d, _solarArrays);
            });
        }

        private void RotateSolarArrays()
        {
            foreach (var solarArray in _solarArrays)
            {
                var drivingRotor = solarArray.DrivingRotor;

                if (solarArray.MaxOutput == 0)
                {
                    // No sun.

                    if (NearlyEqual(drivingRotor.Angle, drivingRotor.LowerLimitRad, epsilonPanelAngel))
                    {
                        // Panel is at sunrise-angle. Stop rotating.
                        drivingRotor.TargetVelocityRPM = 0;
                        solarArray.MovementStatus = SolarArrayMovementStatus.Stopped;
                    }
                    else if (solarArray.MaxOutput == 0 && !NearlyEqual(drivingRotor.Angle, drivingRotor.LowerLimitRad, epsilonPanelAngel))
                    {
                        // It's dark. Rotate back to sunrise-angle.
                        drivingRotor.TargetVelocityRPM = returnVelocity;
                        solarArray.MovementStatus = SolarArrayMovementStatus.ReturnToStartingPosition;
                    }
                }
                else if (solarArray.MaxOutput < solarArray.PreviousMaxOutput)
                {
                    // Less sun. Turn towards upper limit.
                    drivingRotor.TargetVelocityRPM = followingVelocity;
                    solarArray.MovementStatus = SolarArrayMovementStatus.FollowingSun;
                }
                else
                {
                    // More sun. Stop turning.
                    drivingRotor.TargetVelocityRPM = 0f;
                    solarArray.MovementStatus = SolarArrayMovementStatus.Stopped;
                }
            }
        }

        private void UpdatePreviousOutput()
        {
            _solarArrays.ForEach(sa => sa.PreviousMaxOutput = sa.MaxOutput);
        }

        private static bool NearlyEqual(float a, float b, float epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }
    }
}
