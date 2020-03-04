﻿using Sandbox.Game.EntityComponents;
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

        List<MaterialDisplayConfiguration> _configs;
        List<IMyInventory> _inventories;
        List<MaterialDisplay> _displays;
        IEnumerable<MyItemType> _allItemTypes;

        IDictionary<string, ItemTuple> materialDict = new Dictionary<string, ItemTuple>()
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
            _configs = ReadConfiguration();

            _allItemTypes = _configs.SelectMany(config => config.Items).Distinct();
            _inventories = GetInventories();

            ConfigureDrawingSurfaces();
            ConfigureMaterialDisplays();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var totalItemAmounts = new Dictionary<MyItemType, long>();
            foreach (var itemType in _allItemTypes)
            {
                var amount = _inventories.Sum(i => i.GetItemAmount(itemType).RawValue);
                totalItemAmounts.Add(itemType, amount);
            }
            _displays.ForEach(d => d.PrintMaterialStatus(totalItemAmounts));
        }

        private List<MaterialDisplayConfiguration> ReadConfiguration()
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
                GridTerminalSystem.GetBlockGroupWithName(valueSplit[1]).GetBlocksOfType(textSurfaces);

                materialDisplayConfigs.Add(new MaterialDisplayConfiguration { TextSurfaces = textSurfaces, Items = items });
            }

            return materialDisplayConfigs;
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
                    textSurface.FontSize = 1.0f;
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
                _displays.Add(new MaterialDisplay(config.TextSurfaces, config.Items));
            }
        }
    }

    public class MaterialDisplayConfiguration
    {
        public List<IMyTextSurface> TextSurfaces { get; internal set; }
        public List<MyItemType> Items { get; internal set; }
    }

    public class ItemTuple
    {
        public string Type { get; internal set; }
        public string SubType { get; internal set; }
    }

    public class MaterialDisplay
    {
        private List<IMyTextSurface> _textSurfaces;
        private List<MyItemType> _items;

        public MaterialDisplay(List<IMyTextSurface> textSurfaces, List<MyItemType> items)
        {
            this._textSurfaces = textSurfaces;
            this._items = items;
        }

        public void PrintMaterialStatus(Dictionary<MyItemType, long> totalItemAmounts)
        {
            _textSurfaces.ForEach(ts => {
                ts.WriteText("", false);
                PrintHeader(ts);
            });

            foreach (var itemType in _items)
            {
                var itemAmount = 0L;
                totalItemAmounts.TryGetValue(itemType, out itemAmount);
                itemAmount /= 1000000;

                _textSurfaces.ForEach(ts => PrintSingleMaterialStatus(ts, itemType, itemAmount));
            }
        }

        private void PrintSingleMaterialStatus(IMyTextSurface textSurface, MyItemType itemType, long itemAmount)
        {
            textSurface.WriteText($"\n{itemType.SubtypeId}: {itemAmount,7:0#,0}Ml\n".Replace(",", "\'"), true);
        }

        private void PrintHeader(IMyTextSurface textSurface)
        {
            textSurface.WriteText("╔═════════════════╗", true);
            textSurface.WriteText("\n║ Material Status ║", true);
            textSurface.WriteText("\n╚═════════════════╝", true);
        }
    }
}
