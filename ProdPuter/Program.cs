﻿using Sandbox.Game.EntityComponents;
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
        private readonly IDictionary<string, MyItemType> materialDict = new Dictionary<string, MyItemType>()
        {
            { "Bulletproof Glass", new MyItemType("MyObjectBuilder_Component", "BulletproofGlass") },
            { "Canvas", new MyItemType("MyObjectBuilder_Component", "Canvas") },
            { "Computer", new MyItemType("MyObjectBuilder_Component", "Computer") },
            { "Construction Comp", new MyItemType("MyObjectBuilder_Component", "Construction") },
            { "Detector Comp", new MyItemType("MyObjectBuilder_Component", "Detector") },
            { "Display", new MyItemType("MyObjectBuilder_Component", "Display") },
            { "Explosives", new MyItemType("MyObjectBuilder_Component", "Explosives") },
            { "Girder", new MyItemType("MyObjectBuilder_Component", "Girder") },
            { "Gravity Gen. Comp", new MyItemType("MyObjectBuilder_Component", "GravityGenerator") },
            { "Interior Plate", new MyItemType("MyObjectBuilder_Component", "InteriorPlate") },
            { "Large Steel Tube", new MyItemType("MyObjectBuilder_Component", "LargeTube") },
            { "Medical Comp", new MyItemType("MyObjectBuilder_Component", "Medical") },
            { "Metal Grid", new MyItemType("MyObjectBuilder_Component", "MetalGrid") },
            { "Motor", new MyItemType("MyObjectBuilder_Component", "Motor") },
            { "Power Cell", new MyItemType("MyObjectBuilder_Component", "PowerCell") },
            { "Radio Comm. Comp", new MyItemType("MyObjectBuilder_Component", "RadioCommunication") },
            { "Reactor Comp", new MyItemType("MyObjectBuilder_Component", "Reactor") },
            { "Small Steel Tube", new MyItemType("MyObjectBuilder_Component", "SmallTube") },
            { "Solar Cell", new MyItemType("MyObjectBuilder_Component", "SolarCell") },
            { "Steel Plate", new MyItemType("MyObjectBuilder_Component", "SteelPlate") },
            { "Superconductor", new MyItemType("MyObjectBuilder_Component", "Superconductor") },
            { "Thruster Comp", new MyItemType("MyObjectBuilder_Component", "Thrust") },
            { "Automatic Rifle", new MyItemType("MyObjectBuilder_PhysicalGunObject", "AutomaticRifleItem") },
            { "Precise Rifle", new MyItemType("MyObjectBuilder_PhysicalGunObject", "PreciseAutomaticRifleItem") },
            { "Rapid Fire Rifle", new MyItemType("MyObjectBuilder_PhysicalGunObject", "RapidFireAutomaticRifleItem") },
            { "Ultimate Rifle", new MyItemType("MyObjectBuilder_PhysicalGunObject", "UltimateAutomaticRifleItem") },
            { "Welder 1", new MyItemType("MyObjectBuilder_PhysicalGunObject", "WelderItem") },
            { "Welder 2", new MyItemType("MyObjectBuilder_PhysicalGunObject", "Welder2Item") },
            { "Welder 3", new MyItemType("MyObjectBuilder_PhysicalGunObject", "Welder3Item") },
            { "Welder 4", new MyItemType("MyObjectBuilder_PhysicalGunObject", "Welder4Item") },
            { "Grinder 1", new MyItemType("MyObjectBuilder_PhysicalGunObject", "AngleGrinderItem") },
            { "Grinder 2", new MyItemType("MyObjectBuilder_PhysicalGunObject", "AngleGrinder2Item") },
            { "Grinder 3", new MyItemType("MyObjectBuilder_PhysicalGunObject", "AngleGrinder3Item") },
            { "Grinder 4", new MyItemType("MyObjectBuilder_PhysicalGunObject", "AngleGrinder4Item") },
            { "Drill 1", new MyItemType("MyObjectBuilder_PhysicalGunObject", "HandDrillItem") },
            { "Drill 2", new MyItemType("MyObjectBuilder_PhysicalGunObject", "HandDrill2Item") },
            { "Drill 3", new MyItemType("MyObjectBuilder_PhysicalGunObject", "HandDrill3Item") },
            { "Drill 4", new MyItemType("MyObjectBuilder_PhysicalGunObject", "HandDrill4Item") },
            { "Oxygen Bottle", new MyItemType("MyObjectBuilder_OxygenContainerObject", "OxygenBottle") },
            { "Hydrogen Bottle", new MyItemType("MyObjectBuilder_GasContainerObject", "HydrogenBottle") },
            { "NATO 5.56x45mm", new MyItemType("MyObjectBuilder_AmmoMagazine", "NATO_5p56x45mm") },
            { "NATO 25x184mm", new MyItemType("MyObjectBuilder_AmmoMagazine", "NATO_25x184mm") },
            { "Missile 200mm", new MyItemType("MyObjectBuilder_AmmoMagazine", "Missile200mm") }
        };

        public class ProductionConfig
        {
            public Dictionary<string, List<int>> ProductionGoals { get; internal set; }
            public List<IMyAssembler> Assemblers { get; internal set; }
            public List<IMyInventory> Containers { get; internal set; }

            public string CurrentlyProducedItem { get; internal set; }
            public int CurrentlyProducedLevel { get; internal set; }
        }

        private static readonly string prodPuterConfigurationSection = "ProdPuterConfiguration";
        private static readonly string cargoGroupConfigurationKey = "CargoGroup";
        private static readonly string assemblerGroupConfigurationKey = "AssemblerGroup";
        private static readonly string itemsConfigurationKey = "Items";

        private static readonly decimal uftragsMängi = 100;

        private readonly MyIni _ini;

        private readonly List<ProductionConfig> productionConfigs;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _ini = new MyIni();
            productionConfigs = InitProductionConfigs();
        }

        private List<ProductionConfig> InitProductionConfigs()
        {
            var temp1 = new Dictionary<string, List<List<int>>>();
            var temp2 = new Dictionary<string, ProductionConfig>();
            var temp3 = new Dictionary<string, List<string>>();

            var configKeys = new List<MyIniKey>();
            _ini.GetKeys(prodPuterConfigurationSection, configKeys);

            foreach (var key in configKeys)
            {
                var keySplit = key.Name.Split('.');

                if (!temp1.ContainsKey(keySplit[0]))
                {
                    temp1.Add(keySplit[0], new List<List<int>>());
                    temp2.Add(keySplit[0], new ProductionConfig());
                }

                int level;
                if (int.TryParse(keySplit[1], out level))
                {
                    var value = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                    var goals = value.Split(',').ToList().Select(int.Parse).ToList();
                    temp1[keySplit[0]].Insert(level, goals);
                }
                else
                {
                    if (keySplit[1].Equals(cargoGroupConfigurationKey))
                    {
                        var assemblerGroupName = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                        var assemblerGroup = GridTerminalSystem.GetBlockGroupWithName(assemblerGroupName);
                        var assemblers = new List<IMyAssembler>();
                        assemblerGroup.GetBlocksOfType(assemblers);
                        temp2[keySplit[0]].Assemblers = assemblers;
                    }
                    else if (keySplit[1].Equals(assemblerGroupConfigurationKey))
                    {
                        var cargoGroupName = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                        var cargoGroup = GridTerminalSystem.GetBlockGroupWithName(cargoGroupName);
                        var blocks = new List<IMyTerminalBlock>();
                        cargoGroup.GetBlocks(blocks);
                        var inventories = blocks.FindAll(b => b.HasInventory).Select(b => b.GetInventory()).ToList();
                        temp2[keySplit[0]].Containers = inventories;
                    }
                    else if (keySplit[1].Equals(itemsConfigurationKey))
                    {
                        var value = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                        temp3[keySplit[0]] = value.Split(',').ToList();
                    }
                }
            }

            foreach (var item in temp2)
            {
                item.Value.ProductionGoals = new Dictionary<string, List<int>>();

                for (int i = 0; i < temp3.Count; i++)
                {
                    var levelList = new List<int>();
                    foreach (var jtem in temp1[item.Key])
                    {
                        levelList.Add(jtem[i]);
                    }
                    item.Value.ProductionGoals.Add(temp3[item.Key][i], levelList);
                }
            }

            return temp2.Values.ToList();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var config in productionConfigs)
            {
                config.CurrentlyProducedLevel = int.MaxValue;
                CalculateProductionState(config);

                foreach (var assembler in config.Assemblers)
                {
                    if (assembler.IsQueueEmpty)
                    {
                        assembler.ClearQueue();
                        assembler.AddQueueItem(materialDict[config.CurrentlyProducedItem], uftragsMängi);
                    }
                }
            }
        }

        private Dictionary<string, long> GetStoredComponents(ProductionConfig config)
        {
            var amountsPerMaterial = new Dictionary<string, long>();
            foreach (var material in config.ProductionGoals.Keys)
            {
                var itemType = materialDict[material];
                var amount = GetItemCount(itemType, config.Containers);
                amountsPerMaterial.Add(material, amount);
            }
            return amountsPerMaterial;
        }

        private long GetItemCount(MyItemType type, List<IMyInventory> inventories)
        {
            return inventories.Sum(i =>
            {
                var items = new List<MyInventoryItem>();
                i.GetItems(items, item => item.Type.Equals(type));
                return items.Count;
            });
        }

        private void CalculateProductionState(ProductionConfig config)
        {
            var storedComponents = GetStoredComponents(config);
            foreach (var productionGoals in config.ProductionGoals)
            {
                var amountOfThisComponentInStorage = storedComponents[productionGoals.Key];
                var productionLevel = 0;
                foreach (var level in productionGoals.Value)
                {
                    if (amountOfThisComponentInStorage >= level)
                    {
                        productionLevel += 1;
                    }
                }

                if (config.CurrentlyProducedLevel > productionLevel)
                {
                    config.CurrentlyProducedLevel = productionLevel;
                    config.CurrentlyProducedItem = productionGoals.Key;
                }
            }
        }
    }

    public class ItemTuple
    {
        public string Type { get; internal set; }
        public string SubType { get; internal set; }
    }
}