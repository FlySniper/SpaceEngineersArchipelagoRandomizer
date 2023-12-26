using System;
using System.IO;
using System.Diagnostics;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using Sandbox.Game;
using VRage.ObjectBuilders;
using Sandbox.Definitions;
using System.Linq;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using VRageMath;
using VRage.Game.Entity;
using Sandbox.Common.ObjectBuilders.Definitions;

namespace Archipelago
{
    // This example is minimal code required for it to work and with comments so you can better understand what is going on.

    // The gist of it is: ini file is loaded/created that admin can edit, SetVariable is used to store that data in sandbox.sbc which gets automatically sent to joining clients.
    // Benefit of this is clients will be getting this data before they join, very good if you need it during LoadData()
    // This example does not support reloading config while server runs, you can however implement that by sending a packet to all online players with the ini data for them to parse.

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Archipelago_Session : MySessionComponentBase
    {
        public static Archipelago_Session Instance;
        private static Stopwatch stopwatch = Stopwatch.StartNew();
        public static AP_Items items = new AP_Items();
        public static AP_Locations locations = new AP_Locations();
        List<IMyVoxelBase> planets = new List<IMyVoxelBase>();
        bool justAfterLoad = true;
        const string APItemsFileName = "ArchipelagoMultiworldItems.txt";
        public static AP_Settings settings = new AP_Settings();
        BoundingSphereD worldLimit = new BoundingSphereD(Vector3D.Zero, 75000);
        int worldLimitProgressionLevel = -1;
        Random random = new Random();

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (stopwatch.ElapsedMilliseconds >= 1000 && MyAPIGateway.Multiplayer.IsServer)
                {
                    settings.readAPSettings();
                    if (justAfterLoad)
                    {
                        justAfterLoad = false;
                        bool planetsSpawned;
                        if (!(MyAPIGateway.Utilities.GetVariable("archipelago_planets_spawned", out planetsSpawned) && planetsSpawned))
                        {
                            var earthDistance = settings.getNumber("earth_like_distance", 200) * 1000;
                            var tritonDistance = settings.getNumber("triton_distance", 200) * 1000;
                            var marsDistance = settings.getNumber("mars_distance", 200) * 1000;
                            var alienDistance = settings.getNumber("alien_planet_distance", 200) * 1000;
                            var pertamDistance = settings.getNumber("pertam_distance", 200) * 1000;
                            switch (settings.getNumber("starting_planet_choice", 0))
                            {
                                case (0):
                                default:
                                    {
                                        earthDistance = 0;
                                        break;
                                    }
                                case (1):
                                    {
                                        tritonDistance = 0;
                                        break;
                                    }
                                case (2):
                                    {
                                        marsDistance = 0;
                                        break;
                                    }
                                case (3):
                                    {
                                        alienDistance = 0;
                                        break;
                                    }
                                case (4):
                                    {
                                        pertamDistance = 0;
                                        break;
                                    }
                            }
                            var planetSize = settings.getNumber("earth_like_size", 120) * 2000;
                            var planetPosition = calcPlanetPosition(planetSize, 67200.0 / 60000.0, earthDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("EarthLike", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("triton_size", 120) * 2000;
                            planetPosition = calcPlanetPosition(planetSize, 48151.8 / 40126.5, tritonDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Triton", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("mars_size", 120) * 2000;
                            planetPosition = calcPlanetPosition(planetSize, 67200.0 / 60000.0, marsDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Mars", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("alien_planet_size", 120) * 2000;
                            planetPosition = calcPlanetPosition(planetSize, 67200.0 / 60000.0, alienDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Alien", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("pertam_size", 120) * 2000;
                            planetPosition = calcPlanetPosition(planetSize, 30818.1621 / 30066.5, pertamDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Pertam", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("moon_size", 20) * 2000;
                            var moonDistance = settings.getNumber("moon_distance", 200) * 1000;
                            planetPosition = calcPlanetPosition(planetSize, 9785 / 9500, moonDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Moon", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("europa_size", 20) * 2000;
                            var europaDistance = settings.getNumber("europa_distance", 200) * 1000;
                            planetPosition = calcPlanetPosition(planetSize, 10070 / 9500, europaDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Europa", planetSize, random.Next(), planetPosition);
                            planetSize = settings.getNumber("titan_size", 20) * 2000;
                            var titanDistance = settings.getNumber("titan_distance", 200) * 1000;
                            planetPosition = calcPlanetPosition(planetSize, 9785 / 9500, europaDistance);
                            MyAPIGateway.Session.VoxelMaps.SpawnPlanet("Titan", planetSize, random.Next(), planetPosition);
                            MyAPIGateway.Utilities.SetVariable<bool>("archipelago_planets_spawned", true);
                        }
                        MyAPIGateway.Session.VoxelMaps.GetInstances(planets, (vox) => vox is MyPlanet);
                    }
                    stopwatch.Reset();
                    stopwatch.Start();
                    readAPItemsTXT();
                    checkExtraItemsCount();
                    checkPlayersOnPlanets();
                    setWorldLimit();
                    Outside_World_Limit();
                    checkVictory();
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} ]", 10000, MyFontEnum.Red);
            }
        }

        private void checkVictory()
        {
            // Build jump drive goal (aka 0) is handled immediatly when the block is built
            if (settings.getNumber("goal") == 1)
            {
                if (MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Earth"].id}") && 
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Moon"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Mars"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Europa"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Alien"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Titan"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Pertam"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Space"].id}") &&
                    MyAPIGateway.Utilities.FileExistsInGlobalStorage($"apSend{locations["Visited Triton"].id}"))
                {
                    string APVictoryFileName = "apVictory";
                    if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(APVictoryFileName))
                    {
                        using (TextWriter writeFile = MyAPIGateway.Utilities.WriteFileInGlobalStorage(APVictoryFileName))
                        {
                            writeFile.Write("");
                            writeFile.Flush();
                            writeFile.Close();
                        }
                    }
                }
            }
            if (settings.getNumber("goal") == 2)
            {
                // TODO: Defeat Boss Victory check
            }
        }

        private Vector3D calcPlanetPosition(int radius, double hillHeightRatio, double distance)
        {
            double maxHeight = radius * hillHeightRatio;
            double size = (maxHeight + maxHeight) / 2.0;
            long boxSize = 1;
            while (boxSize < size)
                boxSize *= 2;
            double halfBoxSize = boxSize / -2.0;
            var planetoffset = new Vector3D(halfBoxSize);
            if (distance == 0.0)
            {
                return planetoffset;
            }
            var a = random.Next((int) -distance, (int) distance) / Math.Sqrt(2.0);
            var b = random.Next((int)-distance, (int)distance) / Math.Sqrt(2.0);
            double x = 0.0;
            double y = 0.0;
            double z = 0.0;
            switch (random.Next(0,3))
            {
                case (0):
                default:
                    {
                        x = a;
                        y = b;
                        z = Math.Sqrt(x * x + y * y + distance*distance);
                        if (random.Next(0, 2) == 0)
                            z *= -1.0;
                        break;
                    }
                case (1):
                    {
                        x = a;
                        z = b;
                        y = Math.Sqrt(x * x + z * z + distance * distance);
                        if (random.Next(0, 2) == 0)
                            y *= -1.0;
                        break;
                    }
                case (2):
                    {
                        z = a;
                        y = b;
                        x = Math.Sqrt(y * y + z * z + distance * distance);
                        if (random.Next(0, 2) == 0)
                            x *= -1.0;
                        break;
                    }
            }
            var center = new Vector3D(x, y, z);
            return center + planetoffset;
        }

        public override void Draw()
        {
            if (worldLimitProgressionLevel < 3)
            {
                var wm = MatrixD.CreateTranslation(worldLimit.Center);
                var color = new Color(0, 255, 255, 255);
                MySimpleObjectDraw.DrawTransparentSphere(ref wm, (float)worldLimit.Radius, ref color, MySimpleObjectRasterizer.Wireframe, 360 / 5, MyStringId.GetOrCompute("Square"), MyStringId.GetOrCompute("Square"), 10.0f);
            }
        }
        private void setWorldLimit()
        {
            var item = items["Progressive Space Size"];
            if (item.itemCount != worldLimitProgressionLevel)
            {
                if (item.itemCount == 0)
                {
                    switch (settings.getNumber("starting_planet_choice", 0))
                    {
                        case (0):
                        default:
                        {
                            worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("earth_like_size", (int)(75/ 1.25)) * 1.25 * 1000);
                            break;
                        }
                        case (1):
                        {
                            worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("triton_size", (int)(75 / 1.25)) * 1.25 * 1000);
                            break;
                        }
                        case (2):
                        {
                            worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("mars_size", (int)(75 / 1.25)) * 1.25 * 1000);
                            break;
                        }
                        case (3):
                        {
                            worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("alien_planet_size", (int)(75 / 1.25)) * 1.25 * 1000);
                            break;
                        }
                        case (4):
                        {
                            worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("pertam_size", (int)(75 / 1.25)) * 1.25 * 1000);
                            break;
                        }
                    }
                    worldLimitProgressionLevel = 0;
                }
                if (item.itemCount == 1)
                {
                    worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("second_world_size", 2500) * 1000);
                    worldLimitProgressionLevel = 1;
                }
                if (item.itemCount == 2)
                {
                    worldLimit = new BoundingSphereD(Vector3D.Zero, settings.getNumber("third_world_size", 4000) * 1000);
                    worldLimitProgressionLevel = 2;
                }
                if (item.itemCount == 3)
                {
                    worldLimit = new BoundingSphereD(Vector3D.Zero, 100000000);
                    worldLimitProgressionLevel = 3;
                }
            }
        }

        private void checkPlayersOnPlanets()
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach(var player in players)
            {
                if (player.Character == null)
                {
                    continue;
                }
                var playerPos = player.Character.WorldMatrix.Translation;
                bool in0G = true;
                foreach (var planetEnt in planets)
                {
                    var planet = (MyPlanet)planetEnt;
                    if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("EarthLike"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Earth"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Moon"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Moon"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Mars"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Mars"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Europa"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Europa"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Alien"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Alien"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Titan"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Titan"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Pertam"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.01f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Pertam"];
                            writeAPLocationTXT(location);
                        }
                    }
                    else if (planet.Generator.GetObjectBuilder().Id.SubtypeName.Equals("Triton"))
                    {
                        var planetPos = planet.GetClosestSurfacePointGlobal(playerPos);
                        if (planet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(playerPos) > 0.0f)
                            in0G = false;
                        if ((playerPos - planetPos).LengthSquared() < 5 * 5)
                        {
                            var location = locations["Visited Triton"];
                            writeAPLocationTXT(location);
                        }
                    }
                }

                if (in0G)
                {
                    var location = locations["Visited Space"];
                    writeAPLocationTXT(location);
                }
            }
        }

        private void checkExtraItemsCount()
        {
            int worldOxygenBottleCount;
            int worldHydrogenBottleCount;
            int worldIronIngotCount;
            int worldNickelIngotCount;
            int worldSiliconIngotCount;
            int worldGravelCount;
            if (!MyAPIGateway.Utilities.GetVariable<int>("AP Oxygen Bottle", out worldOxygenBottleCount))
            {
                MyAPIGateway.Utilities.SetVariable<int>("AP Oxygen Bottle", 0);
                worldOxygenBottleCount = 0;
            }
            if (!MyAPIGateway.Utilities.GetVariable<int>("AP Hydrogen Bottle", out worldHydrogenBottleCount))
            {
                MyAPIGateway.Utilities.SetVariable<int>("AP Hydrogen Bottle", 0);
                worldHydrogenBottleCount = 0;
            }
            if (!MyAPIGateway.Utilities.GetVariable<int>("AP Iron Ingot", out worldIronIngotCount))
            {
                MyAPIGateway.Utilities.SetVariable<int>("AP Iron Ingot", 0);
                worldIronIngotCount = 0;
            }
            if (!MyAPIGateway.Utilities.GetVariable<int>("AP Nickel Ingot", out worldNickelIngotCount))
            {
                MyAPIGateway.Utilities.SetVariable<int>("AP Nickel Ingot", 0);
                worldNickelIngotCount = 0;
            }
            if (!MyAPIGateway.Utilities.GetVariable<int>("AP Silicon Ingot", out worldSiliconIngotCount))
            {
                MyAPIGateway.Utilities.SetVariable<int>("AP Silicon Ingot", 0);
                worldSiliconIngotCount = 0;
            }
            if (!MyAPIGateway.Utilities.GetVariable<int>("AP Gravel", out worldGravelCount))
            {
                MyAPIGateway.Utilities.SetVariable<int>("AP Gravel", 0);
                worldGravelCount = 0;
            }

            if (items.ContainsKey("Oxygen Bottle") && worldOxygenBottleCount < items["Oxygen Bottle"].itemCount)
            {
                for (int i = 0; i < items["Oxygen Bottle"].itemCount - worldOxygenBottleCount; ++i)
                {

                    var myItem = new MyPhysicalInventoryItem
                    {
                        Amount = 1,
                        Content = new MyObjectBuilder_OxygenContainerObject() { SubtypeName = "OxygenBottle", GasLevel = 1.0f },

                    };
                    spawnItemInInventory(myItem, 1);
                }
                MyAPIGateway.Utilities.SetVariable<int>("AP Oxygen Bottle", items["Oxygen Bottle"].itemCount);
            }
            if (items.ContainsKey("Hydrogen Bottle") && worldHydrogenBottleCount < items["Hydrogen Bottle"].itemCount)
            {
                for (int i = 0; i < items["Hydrogen Bottle"].itemCount - worldHydrogenBottleCount; ++i)
                {

                    var myItem = new MyPhysicalInventoryItem
                    {
                        Amount = 1,
                        Content = new MyObjectBuilder_GasContainerObject() { SubtypeName = "HydrogenBottle", GasLevel = 1.0f },

                    };
                    spawnItemInInventory(myItem, 1);
                }
                MyAPIGateway.Utilities.SetVariable<int>("AP Hydrogen Bottle", items["Hydrogen Bottle"].itemCount);
            }

            if (items.ContainsKey("Iron Ingot") && worldIronIngotCount < items["Iron Ingot"].itemCount)
            {
                for (int i = 0; i < items["Iron Ingot"].itemCount - worldIronIngotCount; ++i)
                {

                    var myItem = new MyPhysicalInventoryItem
                    {
                        Amount = 100,
                        Content = new MyObjectBuilder_Ingot { SubtypeName = "Iron" },

                    };
                    spawnItemInInventory(myItem, 100);
                }
                MyAPIGateway.Utilities.SetVariable<int>("AP Iron Ingot", items["Iron Ingot"].itemCount);
            }

            if (items.ContainsKey("Nickel Ingot") && worldNickelIngotCount < items["Nickel Ingot"].itemCount)
            {
                for (int i = 0; i < items["Nickel Ingot"].itemCount - worldNickelIngotCount; ++i)
                {

                    var myItem = new MyPhysicalInventoryItem
                    {
                        Amount = 100,
                        Content = new MyObjectBuilder_Ingot { SubtypeName = "Nickel" },

                    };
                    spawnItemInInventory(myItem, 100);
                }
                MyAPIGateway.Utilities.SetVariable<int>("AP Nickel Ingot", items["Nickel Ingot"].itemCount);
            }

            if (items.ContainsKey("Silicon Ingot") && worldSiliconIngotCount < items["Silicon Ingot"].itemCount)
            {
                for (int i = 0; i < items["Silicon Ingot"].itemCount - worldSiliconIngotCount; ++i)
                {

                    var myItem = new MyPhysicalInventoryItem
                    {
                        Amount = 100,
                        Content = new MyObjectBuilder_Ingot { SubtypeName = "Silicon" },

                    };
                    spawnItemInInventory(myItem, 100);
                }
                MyAPIGateway.Utilities.SetVariable<int>("AP Silicon Ingot", items["Silicon Ingot"].itemCount);
            }

            if (items.ContainsKey("Gravel") && worldGravelCount < items["Gravel"].itemCount)
            {
                for (int i = 0; i < items["Gravel"].itemCount - worldGravelCount; ++i)
                {

                    var myItem = new MyPhysicalInventoryItem
                    {
                        Amount = 100,
                        Content = new MyObjectBuilder_Ingot { SubtypeName = "Stone" },

                    };
                    spawnItemInInventory(myItem, 100);
                }
                MyAPIGateway.Utilities.SetVariable<int>("AP Gravel", items["Gravel"].itemCount);
            }
        }

        private void spawnItemInInventory(MyPhysicalInventoryItem item, VRage.MyFixedPoint amount)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach(var player in players)
            {
                var character = player.Character;
                var inventory = character.GetInventory();
                if (inventory.CanAddItemAmount(item, amount))
                {
                    inventory.AddItems(amount, item.GetObjectBuilder().PhysicalContent);
                }
                else
                {
                    MyFloatingObjects.Spawn(item, character.WorldMatrix.Translation + character.WorldMatrix.Forward, character.WorldMatrix.Forward, character.WorldMatrix.Up, motionInheritedFrom:character.Physics);
                }
            }
        }

        public override void LoadData()
        {
            Instance = this;
            if (!MyAPIGateway.Session.IsServer)
            {
                return;
            }
            AP_Items_Locations_Filler.fill(items, locations);

            HashSet<IMyEntity> characters_and_grids = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(characters_and_grids, (ent) => ent is IMyCharacter || ent is IMyCubeGrid);
            foreach(var ent in characters_and_grids)
            {
                if(ent is IMyCharacter)
                {
                    (ent as IMyCharacter).GetInventory().OnVolumeChanged += Character_OnVolumeChanged;
                }
                else if(ent is IMyCubeGrid)
                {
                    (ent as IMyCubeGrid).OnBlockAdded += Grid_OnBlockAdded;
                }
            }
            MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
            /*var plushieObjectBuilder = new MyObjectBuilder_CubeBlock() {SubtypeName = "EngineerPlushie"};
            var plushie = MyDefinitionManager.Static.GetCubeBlockDefinition(plushieObjectBuilder);
            var blockGroups = new Dictionary<string, List<MyCubeBlockDefinition>>();
            foreach (var definition in MyDefinitionManager.Static.GetDefinitionsOfType<MyCubeBlockDefinition>())
            {
                
                if ( (definition.DLCs == null || definition.DLCs.Length == 0) && (definition.Context.ModName == null || definition.Context.ModName.Equals(""))
                    && definition.Public && definition.GuiVisible)
                {
                    MyLog.Default.WriteLine("Type: " + definition.Id.TypeId + " Subtype: " + definition.Id.SubtypeName);
                    if (definition.BlockVariantsGroup == null || definition.BlockVariantsGroup.Blocks == null || definition.BlockVariantsGroup.Blocks.Length == 0)
                    {
                        blockGroups.Add(definition.Id.SubtypeName, new List<MyCubeBlockDefinition>() { definition });
                    }
                    else
                    {
                        if (!blockGroups.ContainsKey(definition.BlockVariantsGroup.DisplayNameText))
                            blockGroups.Add(definition.BlockVariantsGroup.DisplayNameText, new List<MyCubeBlockDefinition>(definition.BlockVariantsGroup.Blocks));
                    }
                }
            }
            foreach (MyResearchBlockDefinition definition in MyDefinitionManager.Static.GetResearchBlockDefinitions().ToList())
            {
                MyLog.Default.WriteLine("Research Block Definition: " + definition.Id.SubtypeName);
                definition.UnlockedByGroups = new string[0];
            }
            foreach (MyResearchGroupDefinition definition in MyDefinitionManager.Static.GetResearchGroupDefinitions().ToList())
            {
                MyLog.Default.WriteLine("Research Group Definition: " + definition.Id.SubtypeName);
                if (definition.Id.SubtypeId.ToString().Equals("0"))
                {
                    if (definition.Members != null && definition.Members.Length != 0)
                        MyLog.Default.WriteLine("Member Name: " + definition.Members[0].SubtypeName);
                    definition.Members = new SerializableDefinitionId[] {plushie.Id};
                    continue;
                }

                definition.Members = new SerializableDefinitionId[] {};
            }
            int count = 0;
            for (int builtInRGCount = 0; builtInRGCount < blockGroups.Keys.Count; ++builtInRGCount)
            {
                var groupKey = blockGroups.Keys.ToArray()[count];
                var groupValue = blockGroups[groupKey];
                MyResearchGroupDefinition RG = MyDefinitionManager.Static.GetResearchGroup("" + builtInRGCount);
                if (RG == null)
                {
                    MyLog.Default.WriteLine("ERROR: ResearchGroup (existing): " + builtInRGCount);
                    continue;
                }

                MyResearchGroupDefinition customRG = MyDefinitionManager.Static.GetResearchGroup("ResearchGroup" + (count*2));
                MyResearchGroupDefinition customRG2 = MyDefinitionManager.Static.GetResearchGroup("ResearchGroup" + (count*2 + 1));
                if (customRG == null || customRG2 == null)
                {
                    MyLog.Default.WriteLine("ERROR: ResearchGroup_" + count);
                    ++count;
                    continue;
                }
                MyLog.Default.WriteLine("Set UnlockGroup: " + groupKey + " Block Count: " + blockGroups.Keys.Count);
                MyDefinitionId unlockId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "Unlock_" + (count * 2));
                MyDefinitionId unlockId2 = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "Unlock_" + (count * 2 + 1));
                if (unlockId == null || unlockId2 == null)
                {
                    MyLog.Default.WriteLine("ERROR: Unlock_" + count);
                    ++count;
                    continue;
                }

                var RGMembers = new List<SerializableDefinitionId>();
                foreach (var cubeDefinition in groupValue)
                {
                    MyLog.Default.WriteLine("Block: " + cubeDefinition.Id.SubtypeName);
                    MyResearchBlockDefinition cubeResearchDefinition = MyDefinitionManager.Static.GetResearchBlock(cubeDefinition.Id);
                    MyResearchBlockDefinition unlockCubeResearchDefinition = MyDefinitionManager.Static.GetResearchBlock(MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_CubeBlock", "Unlock_" + (count * 2 + 1)));
                    if (cubeResearchDefinition != null)
                    {
                        cubeResearchDefinition.UnlockedByGroups = new string[] { "ResearchGroup" + (count * 2) };


                        unlockCubeResearchDefinition.UnlockedByGroups = new string[] { "ResearchGroup" + (count * 2 + 1) };
                        RGMembers.Add(cubeDefinition.Id);
                    }
                    else
                    {
                        MyLog.Default.WriteLine("Unable to add to progression tree: " + cubeDefinition.Id.SubtypeName);
                    }
                }
                //MyDefinitionManager.Static.GetResearchBlockDefinitions().Where((def) => def.Id.SubtypeName.StartsWith("ResearchGroup")).Select((def) => (SerializableDefinitionId)def.Id).ToArray();
                RG.Members = RGMembers.ToArray();
                customRG.Members = new SerializableDefinitionId[] { unlockId2, customRG2.Id };
                customRG2.Members = new SerializableDefinitionId[] { unlockId };
                ++count;

            }
            MyLog.Default.WriteLine("Unlock It");
            MyVisualScriptLogicProvider.PlayerResearchClearAll();*/
            //MyDefinitionId definitionId = MyVisualScriptLogicProvider.GetDefinitionId("MyObjectBuilder_CubeBlock", "Unlock_0");
            //MyVisualScriptLogicProvider.PlayerResearchUnlock(MyAPIGateway.Session.Player.IdentityId, definitionId);
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                Instance = null;
                return;
            }
            MyAPIGateway.Entities.OnEntityAdd -= EntityAdded;
            Instance = null;
        }


        private void EntityAdded(IMyEntity entity)
        {
            if (entity is IMyCubeGrid)
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                    grid.OnBlockAdded += Grid_OnBlockAdded;
                    grid.OnBlockIntegrityChanged += Grid_OnBlockIntegrityChanged;
                    grid.GetBlocks(blocks);
                    if (blocks.Count == 1)
                    {
                        Prevent_Block_Build(blocks[0]);
                    }
                }
            }
            
            if (entity is IMyCharacter)
            {
                var character = entity as IMyCharacter;
                character.GetInventory().OnVolumeChanged += Character_OnVolumeChanged;
            }
        }

        private void Outside_World_Limit()
        {
            if (worldLimitProgressionLevel == 3)
            {
                return;
            }
            HashSet<IMyEntity> characters_and_grids = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(characters_and_grids, (ent) => ent is IMyCharacter || ent is IMyCubeGrid);
            foreach (IMyEntity obj in characters_and_grids)
            {
                if (obj == null || obj.Physics == null)
                {
                    return;
                }
                var matrix = obj.Physics.GetWorldMatrix();
                if (matrix == null)
                {
                    return;
                }
                var pos = matrix.Translation;
                if (pos.Length() > worldLimit.Radius && worldLimitProgressionLevel < 3)
                {
                    if (obj is IMyCharacter)
                    {
                        (obj as IMyCharacter).Kill();
                    }
                    else if (obj is IMyCubeGrid)
                    {
                        obj.Close();
                    }
                }
            }
        }

        private void Character_OnVolumeChanged(IMyInventory inventory, float old, float current)
        {
            if (old < current)
            {
                var inventoryItem = inventory.GetItemAt(inventory.ItemCount - 1).Value;

                if (locations.ContainsKey(inventoryItem.Type.TypeId + ":" + inventoryItem.Type.SubtypeId))
                {
                    var location = locations[inventoryItem.Type.TypeId + ":" + inventoryItem.Type.SubtypeId];
                    writeAPLocationTXT(location);
                }
                if (inventoryItem.Type.SubtypeId.Equals("FireworksBoxBlue"))
                {
                    var location = locations["Assembled Fireworks"];
                    writeAPLocationTXT(location);
                }
                if (inventoryItem.Type.SubtypeId.Equals("FireworksBoxGreen"))
                {
                    var location = locations["Assembled Fireworks"];
                    writeAPLocationTXT(location);
                }
                if (inventoryItem.Type.SubtypeId.Equals("FireworksBoxRed"))
                {
                    var location = locations["Assembled Fireworks"];
                    writeAPLocationTXT(location);
                }
                if (inventoryItem.Type.SubtypeId.Equals("FireworksBoxPink"))
                {
                    var location = locations["Assembled Fireworks"];
                    writeAPLocationTXT(location);
                }
                if (inventoryItem.Type.SubtypeId.Equals("FireworksBoxYellow"))
                {
                    var location = locations["Assembled Fireworks"];
                    writeAPLocationTXT(location);
                }
                if (inventoryItem.Type.SubtypeId.Equals("FireworksBoxRainbow"))
                {
                    var location = locations["Assembled Fireworks"];
                    writeAPLocationTXT(location);
                }

            }
        }

        private void Grid_OnBlockIntegrityChanged(IMySlimBlock block)
        {
            if (block.IsFullIntegrity)
            {
                var blockBuilder = block.GetObjectBuilder();
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(blockBuilder);
                bool hasItem = false;
                if (blockDefinition.BlockVariantsGroup == null || blockDefinition.BlockVariantsGroup.Blocks == null || blockDefinition.BlockVariantsGroup.Blocks.Length == 0)
                {
                    if (locations.ContainsKey("Built " + blockDefinition.Id.TypeId + "-" + blockDefinition.Id.SubtypeName))
                    {
                        hasItem = items[blockDefinition.Id.TypeId + ":" + blockDefinition.Id.SubtypeName].itemCount > 0;
                        var location = locations["Built " + blockDefinition.Id.TypeId + "-" + blockDefinition.Id.SubtypeName];
                        if (!location.isFound && hasItem)
                        {
                            writeAPLocationTXT(location);
                        }
                    }
                }
                else
                {
                    if (locations.ContainsKey("Built " + blockDefinition.BlockVariantsGroup.DisplayNameText))
                    {
                        hasItem = items[blockDefinition.BlockVariantsGroup.DisplayNameText].itemCount > 0;
                        var location = locations["Built " + blockDefinition.BlockVariantsGroup.DisplayNameText];
                        if (!location.isFound && hasItem)
                        {
                            writeAPLocationTXT(location);
                        }
                    }
                }
                // Jump Drive Victory
                if (settings.getNumber("goal", -1) == 0 && block.FatBlock is IMyJumpDrive)
                {
                    string APVictoryFileName = "apVictory";
                    if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(APVictoryFileName))
                    {
                        using (TextWriter writeFile = MyAPIGateway.Utilities.WriteFileInGlobalStorage(APVictoryFileName))
                        {
                            writeFile.Write("");
                            writeFile.Flush();
                            writeFile.Close();
                        }
                    }
                }
            }
        }

        private void Grid_OnBlockAdded(IMySlimBlock block)
        {
            Prevent_Block_Build(block);
        }

        private static void Prevent_Block_Build(IMySlimBlock block)
        {
            var stackInfo = block.ComponentStack.GetComponentStackInfo(0);
            var stackSubtype = stackInfo.DefinitionId.SubtypeName;
            var grid = block.CubeGrid;
            var gridPos = grid.GetPosition();
            var blockPos = block.Position;
            var blockPosWorld = new Vector3D(blockPos.X * grid.GridSize + gridPos.X, blockPos.Y * grid.GridSize + gridPos.Y, blockPos.Z * grid.GridSize + gridPos.Z);
            var forward = new Vector3D((float)grid.WorldMatrix.Forward.X, (float)grid.WorldMatrix.Forward.Y, (float)grid.WorldMatrix.Forward.Z);
            var up = new Vector3D((float)grid.WorldMatrix.Up.X, (float)grid.WorldMatrix.Up.Y, (float)grid.WorldMatrix.Up.Z);
            var blockBuilder = block.GetObjectBuilder();
            var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(blockBuilder);
            bool isBuildable = false;
            if (blockDefinition.BlockVariantsGroup == null || blockDefinition.BlockVariantsGroup.Blocks == null || blockDefinition.BlockVariantsGroup.Blocks.Length == 0)
            {
                if (items.ContainsKey(blockDefinition.Id.TypeId + ":" + blockDefinition.Id.SubtypeName))
                    isBuildable = items[blockDefinition.Id.TypeId + ":" + blockDefinition.Id.SubtypeName].itemCount > 0;
            }
            else
            {
                if (items.ContainsKey(blockDefinition.BlockVariantsGroup.DisplayNameText))
                    isBuildable = items[blockDefinition.BlockVariantsGroup.DisplayNameText].itemCount > 0;
            }

            if (!isBuildable)
            {
                block.CubeGrid.RemoveBlock(block);
                MyFloatingObjects.Spawn(
                    MyDefinitionManager.Static.GetComponentDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Component), stackSubtype)),
                    blockPosWorld,
                    forward,
                    up
                    );
            }
        }

        public void readAPItemsTXT()
        {
            if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(APItemsFileName))
            {
                using (TextWriter writeFile = MyAPIGateway.Utilities.WriteFileInGlobalStorage(APItemsFileName))
                {
                    writeFile.Write("");
                    writeFile.Flush();
                    writeFile.Close();
                }
            }
            using (TextReader file = MyAPIGateway.Utilities.ReadFileInGlobalStorage(APItemsFileName))
            {
                string wholeFile = file.ReadToEnd();
                string[] lines = wholeFile.Split('\n');
                foreach(string line in lines)
                {
                    if (line == null || line.Equals(""))
                    {
                        continue;
                    }
                    string [] segments = line.Split(':');
                    bool isGroup = bool.Parse(segments[3]);
                    string kind = "";
                    if (isGroup)
                    {
                        kind = segments[0];
                    }
                    else
                    {
                        kind = segments[0] + ":" + segments[1];
                    }
                    long id = long.Parse(segments[2]);
                    int count = int.Parse(segments[4]);
                    items[kind].itemCount = count;
                }
            }
        }

        public void writeAPLocationTXT(AP_Location location)
        {
            if (location.isFound)
            {
                return;
            }
            string APItemsFileName = $"apSend{location.id}";
            if(!MyAPIGateway.Utilities.FileExistsInGlobalStorage(APItemsFileName))
            {
                using (TextWriter writeFile = MyAPIGateway.Utilities.WriteFileInGlobalStorage(APItemsFileName))
                {
                    writeFile.Write("");
                    writeFile.Flush();
                    writeFile.Close();
                }
            }
        }

    }
}