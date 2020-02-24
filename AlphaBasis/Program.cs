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
    public class BoringMachineConfiguration
    {
        public string PistonGroupName { get; private set; }
        public string DrillGroupName { get; private set; }
        public string LcdName { get; private set; }
        public string LcdWideName { get; private set; }
        public string StatorName { get; private set; }

        public BoringMachineConfiguration(string pistonGroupName, string drillGroupName, string lcdName, string lcdWideName, string statorName)
        {
            this.PistonGroupName = pistonGroupName;
            this.DrillGroupName = drillGroupName;
            this.LcdName = lcdName;
            this.LcdWideName = lcdWideName;
            this.StatorName = statorName;
        }
    }

    public class PistonConfig
    {
        public string Name { get; private set; }
        public string Label { get; private set; }

        public PistonConfig(string name, string label)
        {
            this.Name = name;
            this.Label = label;
        }
    }

    partial class Program : MyGridProgram
    {
        MyIni _ini;

        IMyTextSurface _drawingSurface;
        //IMyTextSurface _wideDrawingSurface;

        PistonStatusDisplay _pistonStatus;
        //PistonExtensionStatusDisplay _pistonExtensionsStatus;
        RotorStatusDisplay _rotorStatus;
        DrillStatusDisplay _drillStatus;

        BoringMachineConfiguration _config;

        public Program()
        {
            _ini = new MyIni();

            _config = ReadConfiguration();

            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _drawingSurface = GridTerminalSystem.GetBlockWithName(_config.LcdName) as IMyTextSurface;
            _drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            _drawingSurface.BackgroundColor = Color.Black;
            _drawingSurface.Font = "Monospace";
            _drawingSurface.FontSize = 0.75f;
            _drawingSurface.Alignment = TextAlignment.LEFT;
            _drawingSurface.WriteText("", false);

            /*_wideDrawingSurface = GridTerminalSystem.GetBlockWithName(_config.LcdWideName) as IMyTextSurface;
            _wideDrawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            _wideDrawingSurface.BackgroundColor = Color.Black;
            _wideDrawingSurface.Font = "Monospace";
            _wideDrawingSurface.FontSize = 1.0f;
            _wideDrawingSurface.Alignment = TextAlignment.LEFT;
            _wideDrawingSurface.WriteText("", false);*/

            var pistons = GetPistons();
            var stator = GridTerminalSystem.GetBlockWithName(_config.StatorName) as IMyMotorStator;
            var drillInventories = GetDrillInventories();

            var pistonDataList = InitPistonData(pistons);

            _pistonStatus = new PistonStatusDisplay(pistonDataList);
            //_pistonExtensionsStatus = new PistonExtensionStatusDisplay(pistonDataList);
            _rotorStatus = new RotorStatusDisplay(stator);
            _drillStatus = new DrillStatusDisplay(drillInventories);
        }

        private BoringMachineConfiguration ReadConfiguration()
        {
            var customData = Me.CustomData;

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            var pistonGroupName = _ini.Get("BoringMachineConfig", "PistonGroup").ToString();
            var drillGroupName = _ini.Get("BoringMachineConfig", "DrillGroup").ToString();
            var lcdName = _ini.Get("BoringMachineConfig", "LCD").ToString();
            var lcdWideName = _ini.Get("BoringMachineConfig", "LCDWide").ToString();
            var statorName = _ini.Get("BoringMachineConfig", "Stator").ToString();

            return new BoringMachineConfiguration(pistonGroupName, drillGroupName, lcdName, lcdWideName, statorName);
        }

        private List<IMyPistonBase> GetPistons()
        {
            var pistonGroup = GridTerminalSystem.GetBlockGroupWithName(_config.PistonGroupName);
            var pistonBlocks = new List<IMyPistonBase>();
            pistonGroup.GetBlocksOfType(pistonBlocks);
            return pistonBlocks.ToList();
        }

        private List<IMyInventory> GetDrillInventories()
        {
            var drillGroup = GridTerminalSystem.GetBlockGroupWithName(_config.DrillGroupName);            
            var drillBlocks = new List<IMyTerminalBlock>();
            drillGroup.GetBlocks(drillBlocks);
            return drillBlocks.FindAll(b => b.HasInventory).Select(b => b.GetInventory()).ToList();
        }

        private List<PistonData> InitPistonData(List<IMyPistonBase> pistons)
        {
            List<PistonData> pistonDataList = new List<PistonData>(pistons.Count);
            for (int i = 0; i < pistons.Count; i++)
            {
                pistonDataList.Add(new PistonData(pistons[i], (i + 1).ToString()));
            }
            return pistonDataList;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _drawingSurface.WriteText("");
            //_wideDrawingSurface.WriteText("");
            
            _pistonStatus.PrintPistonStatus(_drawingSurface);
            _drawingSurface.WriteText("\n\n", true);
            _rotorStatus.PrintRotorStatus(_drawingSurface);
            _drawingSurface.WriteText("\n", true);
            _drillStatus.PrintDrillStatus(_drawingSurface);

            //_pistonExtensionsStatus.PrintPistonExtensionStatus(_wideDrawingSurface);
        }
        
    }
}
