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
            _ini = new MyIni();            
            _config = ReadConfiguration();

            ConfigureDrawingSurfaces();

            var batteries = GetBatteries();
            _powerDisplay = new PowerDisplay(batteries);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _drawingSurfaces.ForEach(ds => _powerDisplay.PrintPowerStatus(ds));
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

            return new ResourcesDisplayConfiguration { LCDGroup = lcdGroupName };
        }

        private List<IMyBatteryBlock> GetBatteries()
        {
            var batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries);
            return batteries;
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

        public PowerDisplay(List<IMyBatteryBlock> batteries)
        {
            this._batteries = batteries;
        }

        public void PrintPowerStatus(IMyTextSurface textSurface)
        {
            PrintHeader(textSurface);

            var totalCurrentInput = _batteries.Sum(b => b.CurrentInput);
            var totalCurrentStored = _batteries.Sum(b => b.CurrentStoredPower);
            var totalCurrentOutput = _batteries.Sum(b => b.CurrentOutput);
            var totalMaxStored = _batteries.Sum(b => b.MaxStoredPower);

            var trend = totalCurrentInput - totalCurrentOutput;
            var trendString = GetTrendString(GetTrendDirection(trend), GetTrendLevel(trend));

            textSurface.WriteText($"\nInput : {totalCurrentInput:F3}MW", true);
            textSurface.WriteText($"\nStored: {totalCurrentStored:F3}MW / {totalMaxStored:F3}MW", true);
            textSurface.WriteText($"\nOutput: {totalCurrentOutput:F3}MW", true);
            textSurface.WriteText($"\nTrend : {trendString}", true);
        }

        private TrendLevel GetTrendLevel(float trend)
        {
            // TODO Calculate trend level relative to something
            var trendAbsolute = Math.Abs(trend);
            if (trendAbsolute > 0 && trendAbsolute <= 1)
            {
                return TrendLevel.Slow;
            }
            else if (trendAbsolute > 1 && trendAbsolute <= 2)
            {
                return TrendLevel.Medium;
            }
            else if (trendAbsolute > 2)
            {
                return TrendLevel.Fast;
            }
            return TrendLevel.None;
        }

        private TrendDirection GetTrendDirection(float trend)
        {
            if (trend < 0)
            {
                return TrendDirection.Down;
            }
            else if (trend > 0)
            {
                return TrendDirection.Up;
            }
            return TrendDirection.None;
        }

        private string GetTrendString(TrendDirection direction, TrendLevel level)
        {
            var symbol = GetTrendSymbol(direction);
            switch (direction)
            {
                case TrendDirection.Up:
                case TrendDirection.Down:
                    return string.Concat(Enumerable.Repeat(symbol, (int) level));
                default:
                    return string.Concat(Enumerable.Repeat(symbol, 3));
            }
        }

        private string GetTrendSymbol(TrendDirection direction)
        {
            switch (direction)
            {
                case TrendDirection.Down:
                    return "↓";
                case TrendDirection.Up:
                    return "↑";
                default:
                    return "═";
            }
        }

        private void PrintHeader(IMyTextSurface textSurface)
        {
            textSurface.WriteText("╔══════════════╗", true);
            textSurface.WriteText("\n║ Power Status ║", true);
            textSurface.WriteText("\n╚══════════════╝", true);
        }

        enum TrendDirection
        {
            Up,
            Down,
            None
        }

        enum TrendLevel
        {
            None = 0,
            Slow = 1,
            Medium = 2,
            Fast = 3
        }
    }

    public class ResourcesDisplayConfiguration
    {
        public string LCDGroup { get; internal set; }
    }

}
