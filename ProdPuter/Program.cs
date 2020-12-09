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

        private readonly IDictionary<string, MyItemType> blueprintDict = new Dictionary<string, MyItemType>()
        {
            { "Bulletproof Glass", new MyItemType("MyObjectBuilder_BlueprintDefinition", "BulletproofGlass") },
            { "Canvas", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Canvas") },
            { "Computer", new MyItemType("MyObjectBuilder_BlueprintDefinition", "ComputerComponent") },
            { "Construction Comp", new MyItemType("MyObjectBuilder_BlueprintDefinition", "ConstructionComponent") },
            { "Detector Comp", new MyItemType("MyObjectBuilder_BlueprintDefinition", "DetectorComponent") },
            { "Display", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Display") },
            { "Explosives", new MyItemType("MyObjectBuilder_BlueprintDefinition", "ExplosivesComponent") },
            { "Girder", new MyItemType("MyObjectBuilder_BlueprintDefinition", "GirderComponent") },
            { "Gravity Gen. Comp", new MyItemType("MyObjectBuilder_BlueprintDefinition", "GravityGeneratorComponent") },
            { "Interior Plate", new MyItemType("MyObjectBuilder_BlueprintDefinition", "InteriorPlate") },
            { "Large Steel Tube", new MyItemType("MyObjectBuilder_BlueprintDefinition", "LargeTube") },
            { "Medical Comp", new MyItemType("MyObjectBuilder_BlueprintDefinition", "MedicalComponent") },
            { "Metal Grid", new MyItemType("MyObjectBuilder_BlueprintDefinition", "MetalGrid") },
            { "Motor", new MyItemType("MyObjectBuilder_BlueprintDefinition", "MotorComponent") },
            { "Oxygen", new MyItemType("MyObjectBuilder_BlueprintDefinition", "IceToOxygen") },
            { "Power Cell", new MyItemType("MyObjectBuilder_BlueprintDefinition", "PowerCell") },
            { "Radio Comm. Comp", new MyItemType("MyObjectBuilder_BlueprintDefinition", "RadioCommunicationComponent") },
            { "Reactor Comp", new MyItemType("MyObjectBuilder_BlueprintDefinition", "ReactorComponent") },
            { "Small Steel Tube", new MyItemType("MyObjectBuilder_BlueprintDefinition", "SmallTube") },
            { "Solar Cell", new MyItemType("MyObjectBuilder_BlueprintDefinition", "SolarCell") },
            { "Steel Plate", new MyItemType("MyObjectBuilder_BlueprintDefinition", "SteelPlate") },
            { "Superconductor", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Superconductor") },
            { "Thruster Comp.", new MyItemType("MyObjectBuilder_BlueprintDefinition", "ThrustComponent") },
            { "Automatic Rifle", new MyItemType("MyObjectBuilder_BlueprintDefinition", "AutomaticRifle") },
            { "Precise Rifle", new MyItemType("MyObjectBuilder_BlueprintDefinition", "PreciseAutomaticRifle") },
            { "Rapid Fire Rifle", new MyItemType("MyObjectBuilder_BlueprintDefinition", "RapidFireAutomaticRifle") },
            { "Ultimate Rifle", new MyItemType("MyObjectBuilder_BlueprintDefinition", "UltimateAutomaticRifle") },
            { "Welder 1", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Welder") },
            { "Welder 2", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Welder2") },
            { "Welder 3", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Welder3") },
            { "Welder 4", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Welder4") },
            { "Grinder 1", new MyItemType("MyObjectBuilder_BlueprintDefinition", "AngleGrinder") },
            { "Grinder 2", new MyItemType("MyObjectBuilder_BlueprintDefinition", "AngleGrinder2") },
            { "Grinder 3", new MyItemType("MyObjectBuilder_BlueprintDefinition", "AngleGrinder3") },
            { "Grinder 4", new MyItemType("MyObjectBuilder_BlueprintDefinition", "AngleGrinder4") },
            { "Drill 1", new MyItemType("MyObjectBuilder_BlueprintDefinition", "HandDrill") },
            { "Drill 2", new MyItemType("MyObjectBuilder_BlueprintDefinition", "HandDrill2") },
            { "Drill 3", new MyItemType("MyObjectBuilder_BlueprintDefinition", "HandDrill3") },
            { "Drill 4", new MyItemType("MyObjectBuilder_BlueprintDefinition", "HandDrill4") },
            { "Oxygen Bottle", new MyItemType("MyObjectBuilder_BlueprintDefinition", "OxygenBottle") },
            { "Hydrogen Bottle", new MyItemType("MyObjectBuilder_BlueprintDefinition", "HydrogenBottle") },
            { "Missile 200mm", new MyItemType("MyObjectBuilder_BlueprintDefinition", "Missile200mm") },
            { "NATO 25x184mm", new MyItemType("MyObjectBuilder_BlueprintDefinition", "NATO_25x184mmMagazine") },
            { "NATO 5.56x45mm", new MyItemType("MyObjectBuilder_BlueprintDefinition", "NATO_5p56x45mmMagazine") }
        };

        public class ProductionConfig
        {
            public Dictionary<string, List<int>> ProductionGoals { get; internal set; }
            public List<IMyAssembler> Assemblers { get; internal set; }
            public List<IMyInventory> Containers { get; internal set; }

            public string CurrentlyProducedItem { get; internal set; }
            public int CurrentlyProducedLevel { get; internal set; }
            public int MaxLevel { get; internal set; }

            public ProductionConfig()
            {
                ProductionGoals = new Dictionary<string, List<int>>();
            }
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
            var customData = Me.CustomData;

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            var items = new Dictionary<string, List<string>>();
            var levels = new Dictionary<string, List<List<int>>>();
            var configs = new Dictionary<string, ProductionConfig>();

            var configKeys = new List<MyIniKey>();
            _ini.GetKeys(prodPuterConfigurationSection, configKeys);

            foreach (var key in configKeys)
            {
                var keySplit = key.Name.Split('.');

                if (!levels.ContainsKey(keySplit[0]))
                {
                    levels.Add(keySplit[0], new List<List<int>>());
                    configs.Add(keySplit[0], new ProductionConfig());
                }

                int level;
                if (int.TryParse(keySplit[1], out level))
                {
                    var value = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                    var goals = value.Split(',').ToList().Select(int.Parse).ToList();
                    levels[keySplit[0]].Insert(level, goals);
                }
                else
                {
                    if (keySplit[1].Equals(assemblerGroupConfigurationKey))
                    {
                        var assemblerGroupName = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                        var assemblerGroup = GridTerminalSystem.GetBlockGroupWithName(assemblerGroupName);
                        var assemblers = new List<IMyAssembler>();
                        assemblerGroup.GetBlocksOfType(assemblers);
                        configs[keySplit[0]].Assemblers = assemblers;
                    }
                    else if (keySplit[1].Equals(cargoGroupConfigurationKey))
                    {
                        var cargoGroupName = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                        var cargoGroup = GridTerminalSystem.GetBlockGroupWithName(cargoGroupName);
                        var blocks = new List<IMyTerminalBlock>();
                        cargoGroup.GetBlocks(blocks);
                        var inventories = blocks.FindAll(b => b.HasInventory).Select(b => b.GetInventory()).ToList();
                        configs[keySplit[0]].Containers = inventories;
                    }
                    else if (keySplit[1].Equals(itemsConfigurationKey))
                    {
                        var value = _ini.Get(prodPuterConfigurationSection, key.Name).ToString();
                        items[keySplit[0]] = value.Split(',').ToList();
                    }
                }
            }

            foreach (var item in configs)
            {
                for (int i = 0; i < items[item.Key].Count; i++)
                {
                    var levelList = new List<int>();
                    foreach (var jtem in levels[item.Key])
                    {
                        levelList.Add(jtem[i]);
                    }
                    item.Value.ProductionGoals.Add(items[item.Key][i], levelList);
                    item.Value.MaxLevel = levelList.Count - 1;
                }
            }

            return configs.Values.ToList();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var config in productionConfigs)
            {
                config.CurrentlyProducedLevel = int.MaxValue;
                CalculateProductionState(config);

                if (config.CurrentlyProducedLevel > config.MaxLevel)
                {
                    continue;
                }

                Echo("-------");
                Echo($"item: {config.CurrentlyProducedItem}");
                Echo($"level: {config.CurrentlyProducedLevel}");
                Echo("=======");

                foreach (var assembler in config.Assemblers)
                {
                    if (assembler.IsQueueEmpty)
                    {
                        assembler.ClearQueue();
                        assembler.AddQueueItem(blueprintDict[config.CurrentlyProducedItem], uftragsMängi);
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
                return items.Sum(eff => (int)eff.Amount);
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