﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private MyIni _ini;

        private List<MaterialDisplayConfiguration> _configs;
        private List<IMyInventory> _inventories;
        private List<MaterialDisplay> _displays;
        private IEnumerable<MyItemType> _allItemTypes;
        private Dictionary<MyItemType, ItemAmount> _totalItemAmounts;

        private IDictionary<string, ItemTuple> materialDict = new Dictionary<string, ItemTuple>()
        {
            { "Bulletproof Glass", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "BulletproofGlass" } },
            { "Canvas", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Canvas" } },
            { "Computer", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Computer" } },
            { "Construction Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Construction" } },
            { "Detector Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Detector" } },
            { "Display", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Display" } },
            { "Explosives", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Explosives" } },
            { "Girder", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Girder" } },
            { "Gravity Gen. Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "GravityGenerator" } },
            { "Interior Plate", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "InteriorPlate" } },
            { "Large Steel Tube", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "LargeTube" } },
            { "Medical Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Medical" } },
            { "Metal Grid", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "MetalGrid" } },
            { "Motor", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Motor" } },
            { "Power Cell", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "PowerCell" } },
            { "Radio Comm. Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "RadioCommunication" } },
            { "Reactor Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Reactor" } },
            { "Small Steel Tube", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "SmallTube" } },
            { "Solar Cell", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "SolarCell" } },
            { "Steel Plate", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "SteelPlate" } },
            { "Superconductor", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Superconductor" } },
            { "Thruster Comp", new ItemTuple { Type = "MyObjectBuilder_Component", SubType = "Thrust" } },
            { "Automatic Rifle", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "AutomaticRifleItem" } },
            { "Precise Rifle", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "PreciseAutomaticRifleItem" } },
            { "Rapid Fire Rifle", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "RapidFireAutomaticRifleItem" } },
            { "Ultimate Rifle", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "UltimateAutomaticRifleItem" } },
            { "Welder 1", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "WelderItem" } },
            { "Welder 2", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "Welder2Item" } },
            { "Welder 3", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "Welder3Item" } },
            { "Welder 4", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "Welder4Item" } },
            { "Grinder 1", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "AngleGrinderItem" } },
            { "Grinder 2", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "AngleGrinder2Item" } },
            { "Grinder 3", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "AngleGrinder3Item" } },
            { "Grinder 4", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "AngleGrinder4Item" } },
            { "Drill 1", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "HandDrillItem" } },
            { "Drill 2", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "HandDrill2Item" } },
            { "Drill 3", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "HandDrill3Item" } },
            { "Drill 4", new ItemTuple { Type = "MyObjectBuilder_PhysicalGunObject", SubType = "HandDrill4Item" } },
            { "Oxygen Bottle", new ItemTuple { Type = "MyObjectBuilder_OxygenContainerObject", SubType = "OxygenBottle" } },
            { "Hydrogen Bottle", new ItemTuple { Type = "MyObjectBuilder_GasContainerObject", SubType = "HydrogenBottle" } },
            { "NATO 5.56x45mm", new ItemTuple { Type = "MyObjectBuilder_AmmoMagazine", SubType = "NATO_5p56x45mm" } },
            { "NATO 25x184mm", new ItemTuple { Type = "MyObjectBuilder_AmmoMagazine", SubType = "NATO_25x184mm" } },
            { "Missile 200mm", new ItemTuple { Type = "MyObjectBuilder_AmmoMagazine", SubType = "Missile200mm" } },
            { "Cobalt Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Cobalt" } },
            { "Gold Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Gold" } },
            { "Ice", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Ice" } },
            { "Iron Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Iron" } },
            { "Magnesium Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Magnesium" } },
            { "Nickel Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Nickel" } },
            { "Platinum Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Platinum" } },
            { "Scrap Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Scrap" } },
            { "Silicon Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Silicon" } },
            { "Silver Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Silver" } },
            { "Stone", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Stone" } },
            { "Uranium Ore", new ItemTuple { Type = "MyObjectBuilder_Ore", SubType = "Uranium" } },
            { "Cobalt Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Cobalt" } },
            { "Gold Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Gold" } },
            { "Gravel", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Stone" } },
            { "Iron Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Iron" } },
            { "Magnesium Powder", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Magnesium" } },
            { "Nickel Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Nickel" } },
            { "Platinum Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Platinum" } },
            { "Silicon Wafer", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Silicon" } },
            { "Silver Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Silver" } },
            { "Uranium Ingot", new ItemTuple { Type = "MyObjectBuilder_Ingot", SubType = "Uranium" } }
        };

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _ini = new MyIni();
            _configs = ReadMaterialDisplayConfiguration();

            _allItemTypes = _configs.SelectMany(config => config.Items).Distinct();

            _inventories = GetInventories();

            ConfigureDrawingSurfaces();
            ConfigureMaterialDisplays();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (_totalItemAmounts == null) //cannot init in ctor because source data is not available
            {
                _totalItemAmounts = new Dictionary<MyItemType, ItemAmount>();
                foreach (var itemType in _allItemTypes)
                {
                    _totalItemAmounts.Add(itemType, new ItemAmount { current = GetItemsMängi(itemType) });
                }
            }
            else
            {
                foreach (var itemType in _totalItemAmounts.Keys)
                {
                    _totalItemAmounts[itemType].current = GetItemsMängi(itemType);
                }
            }

            _displays.ForEach(d => d.PrintMaterialStatus(_totalItemAmounts));
        }

        public long GetItemsMängi(MyItemType type)
        {
            if (!type.TypeId.Equals("MyObjectBuilder_Ingot") && !type.TypeId.Equals("MyObjectBuilder_Ore"))
            {
                var mängi = _inventories.Sum(i => (int)i.GetItemAmount(type));
                return mängi;
            }

            var liter = _inventories.Sum(i => i.GetItemAmount(type).RawValue / 1000); //m^3 to l
            return Convert.ToInt64(Math.Truncate((double)liter * type.GetItemInfo().Volume));
        }

        private List<MaterialDisplayConfiguration> ReadMaterialDisplayConfiguration()
        {
            var customData = Me.CustomData;

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            return ParseMaterialDisplayConfigs();
        }

        private List<MaterialDisplayConfiguration> ParseMaterialDisplayConfigs()
        {
            var configKeys = new List<MyIniKey>();
            _ini.GetKeys("MaterialDisplayConfig", configKeys);

            var materialDisplayConfigs = new List<MaterialDisplayConfiguration>(configKeys.Count);
            foreach (var key in configKeys)
            {
                var value = _ini.Get("MaterialDisplayConfig", key.Name).ToString();

                // E.g. value = "Stone, Iron Ore; StIronDisplays"
                var valueSplit = value.Split(';');

                // E.g. valueSplit[0] = "Stone,Iron Ore"
                // E.g. valueSplit[1] = "StIronDisplays"
                // E,g, valueSplit[2] = "Super Geils Display"

                var itemKeys = valueSplit[0].Split(',');

                // E.g. itemKeys[0] = "Stone"
                // E.g. itemKeys[1] = "Iron Ore"

                var items = new List<MyItemType>(itemKeys.Length);
                foreach (var itemKey in itemKeys)
                {
                    ItemTuple itemTuple;
                    if (materialDict.TryGetValue(itemKey, out itemTuple))
                    {
                        items.Add(new MyItemType(itemTuple.Type, itemTuple.SubType));
                    }
                    else
                    {
                        Echo($"Item '${itemKey}' is not configured.");
                    }
                }

                var textSurfaces = new List<IMyTextSurface>();
                var blockGroup = GridTerminalSystem.GetBlockGroupWithName(valueSplit[1]);
                if (blockGroup != null)
                {
                    blockGroup.GetBlocksOfType(textSurfaces);
                }
                else
                {
                    Echo($"Display group '{valueSplit[1]}' does not exist in this grid.");
                }

                var panelConfiguration = ReadPanelConfiguration(key.Name);

                materialDisplayConfigs.Add(new MaterialDisplayConfiguration { TextSurfaces = textSurfaces, Items = items, Title = valueSplit[2] });
            }

            return materialDisplayConfigs;
        }

        private PanelConfiguration ReadPanelConfiguration(string key)
        {
            var configKeys = new List<MyIniKey>();
            _ini.GetKeys("PanelConfig", configKeys);

            var value = _ini.Get("PanelCOnfig", key).ToString();

            var panelConfiguration = new PanelConfiguration();

            if (String.IsNullOrEmpty(value))
            {
                return panelConfiguration;
            }

            // E.g. value = "9;1.0f"
            var valueSplit = value.Split(';');

            var labelLength = Convert.ToInt32(valueSplit[0]);
            var fontSize = (float)Convert.ToDouble(valueSplit[1]);

            panelConfiguration.LabelLength = labelLength;
            panelConfiguration.FontSize = fontSize;

            return panelConfiguration;
        }

        private List<IMyInventory> GetInventories()
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

        private void ConfigureDrawingSurfaces()
        {
            foreach (var config in _configs)
            {
                config.TextSurfaces.ForEach(textSurface =>
                {
                    textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                    textSurface.BackgroundColor = Color.Black;
                    textSurface.Font = "Monospace";
                    textSurface.FontSize = 0.7f;
                    textSurface.FontColor = Color.Teal;
                    textSurface.Alignment = TextAlignment.LEFT;
                    textSurface.WriteText("", false);
                });
            }
        }

        private void ConfigureMaterialDisplays()
        {
            _displays = new List<MaterialDisplay>(_configs.Count);
            foreach (var config in _configs)
            {
                _displays.Add(new MaterialDisplay(config));
            }
        }
    }

    public class MaterialDisplayConfiguration
    {
        public List<IMyTextSurface> TextSurfaces { get; internal set; }
        public List<MyItemType> Items { get; internal set; }
        public string Title { get; internal set; }
    }

    public class PanelConfiguration
    {
        public int LabelLength { get; internal set; } = 9;
        public Color BackgroundColor { get; internal set; } = Color.Black;
        public Color FontColor { get; internal set; } = Color.Teal;
        public float FontSize { get; internal set; } = 1.0f;
        public TextAlignment TextAlignment { get; internal set; } = TextAlignment.LEFT;
    }

    public class ItemTuple
    {
        public string Type { get; internal set; }
        public string SubType { get; internal set; }
    }

    public class ItemAmount
    {
        private long _current;
        private Queue<long> _history = new Queue<long>(25);

        /// <summary>
        /// Current amount in liters
        /// </summary>
        public long current { get { return _current; } set { _current = value; _history.Enqueue(value); } }

        /// <summary>
        /// Historic amount in liters
        /// </summary>
        public long historic { get { return Convert.ToInt64(_history.Average()); } }
    }

    public class MaterialDisplay
    {
        private MaterialDisplayConfiguration _config;

        public MaterialDisplay(MaterialDisplayConfiguration config)
        {
            _config = config;
        }

        public void PrintMaterialStatus(Dictionary<MyItemType, ItemAmount> totalItemAmounts)
        {
            _config.TextSurfaces.ForEach(ts =>
            {
                ts.WriteText("", false);
                PrintHeader(ts, _config.Title);
            });

            foreach (var itemType in _config.Items)
            {
                char[] itemUnit;
                if (!itemType.TypeId.Equals("MyObjectBuilder_Ingot") && !itemType.TypeId.Equals("MyObjectBuilder_Ore"))
                {
                    itemUnit = new char[] { ' ', 's', 't', 'k' };
                }
                else
                {
                    itemUnit = new char[] { ' ', 'l' };
                }

                var itemAmount = totalItemAmounts[itemType].current;

                var historicFactor = totalItemAmounts[itemType].current - totalItemAmounts[itemType].historic;
                string trend = historicFactor > 0 ? "↑" : historicFactor < 0 ? "↓" : "=";

                if (itemAmount > 9999999999L)
                {
                    itemUnit[0] = 'G';
                    itemAmount /= 1000000000;
                }
                else if (itemAmount > 9999999L)
                {
                    itemUnit[0] = 'M';
                    itemAmount /= 1000000;
                }
                else if (itemAmount > 9999L)
                {
                    itemUnit[0] = 'k';
                    itemAmount /= 1000;
                }

                _config.TextSurfaces.ForEach(ts => PrintSingleMaterialStatus(ts, itemType, itemAmount, new string(itemUnit), trend));
            }
        }

        private void PrintSingleMaterialStatus(IMyTextSurface textSurface, MyItemType itemType, long itemAmount, string unit, string trend)
        {
            var label = itemType.SubtypeId;
            textSurface.WriteText($"{$"{(label.Length > 9 ? label.Substring(0, 9) : label)}:".PadRight(10)} {itemAmount,5:#,0} {unit} {trend}\n".Replace(",", "\'"), true);
        }

        private void PrintHeader(IMyTextSurface textSurface, string title)
        {
            textSurface.WriteText($"{title}\n\n", true);
        }
    }
}