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

namespace Archipelago
{
    public class AP_Item
    {
        public string name;
        public long id;
        public bool isGroup;
        public int itemCount;
        public AP_Item(string name, long id, bool isGroup=false, int itemCount = 0)
        {
            this.id = id;
            this.name = name;
            this.isGroup = isGroup;
            this.itemCount = itemCount;
        }
    }

    public class AP_Location
    {
        public string name;
        public long id;
        public bool isFound;
        public AP_Location(string name, long id, bool isFound=false)
        {
            this.name = name;
            this.id = id;
            this.isFound = isFound;
        }

        public override string ToString()
        {
            return name + ":" + id + ":" + isFound;
        }
    }

    public class AP_Items : Dictionary<string, AP_Item>
    {
    }

    public class AP_Locations : Dictionary<string, AP_Location>
    {
    }

    public class AP_Items_Locations_Filler
    {
        public static long STARTING_ITEM_ID = 38800;
        public static long STARTING_LOCATION_ID = 3883300;

        public static Dictionary<string, long> allowedItems = new Dictionary<string, long>()
        {
            {"Light Armor Block", 38800},
            {"Heavy Armor Block", 38801},
            {"Round Armor Slope", 38802},
            {"Heavy Armor Round Slope", 38803},
            {"Light Armor Ramps", 38804},
            {"Light Armor Ramp Corners", 38805},
            {"Heavy Armor Ramps", 38806},
            {"Heavy Armor Ramp Corners", 38807},
            {"Light Armor Sloped Corners", 38808},
            {"Heavy Armor Sloped Corners", 38809},
            {"DisplayName_BlockGroup_LightArmorTransitionBlocks", 38810},
            {"DisplayName_BlockGroup_HeavyArmorTransitionBlocks", 38811},
            {"DisplayName_Block_LightArmorPanel", 38812},
            {"DisplayName_Block_heavyArmorPanel", 38813},
            {"Projector", 38814},
            {"Target Dummy", 38815},
            {"Sound Block", 38816},
            {"Button Panel", 38817},
            {"Automation Blocks", 38818},
            {"AI Flight {Move}", 38819},
            {"Communication Blocks", 38820},
            {"Remote Control", 38821},
            {"Control Station", 38822},
            {"Gyroscope", 38823},
            {"Control Seat", 38824},
            {"Door", 38825},
            {"Airtight Hangar Door", 38826},
            {"Blast Doors", 38827},
            {"Store", 38828},
            {"Battery", 38829},
            {"Fueled Energy Sources", 38830},
            {"Renewable Energy Sources", 38831},
            {"Engineer Plushie", 38832},
            {"Saberoid Plushie", 38833},
            {"Anniversary Statue", 38834},
            {"Gravity Blocks", 38835},
            {"Passage", 38836},
            {"Steel Catwalk", 38837},
            {"Stairs", 38838},
            {"DisplayName_Block_AirDucts", 38839},
            {"Corner LCD Screens", 38840},
            {"LCD Screens", 38841},
            {"Lighting", 38842},
            {"Gas Tanks", 38843},
            {"Air Vent", 38844},
            {"Cargo Containers", 38845},
            {"Small Conveyor Tube", 38846},
            {"Conveyor Junction", 38847},
            {"Inputs/Outputs", 38848},
            {"Piston", 38849},
            {"Rotor", 38850},
            {"DisplayName_Block_Hinge", 38851},
            {"Medical Blocks", 38852},
            {"Refinery", 38853},
            {"O2/H2 Generator", 38854},
            {"Assembler", 38855},
            {"Oxygen Farm", 38856},
            {"Upgrade Modules", 38857},
            {"letters A to H", 38858},
            {"DisplayName_BlockGroup_Numbers", 38859},
            {"Symbols", 38860},
            {"Large Ion Thruster", 38861},
            {"Large Hydrogen Thruster", 38862},
            {"Large Atmospheric Thruster", 38863},
            {"Ship Tools", 38864},
            {"Ore Detector", 38865},
            {"Landing Gear", 38866},
            {"Jump Drive", 38867},
            {"Parachute Hatch", 38868},
            {"Warhead", 38869},
            {"Decoy", 38870},
            {"Turreted Weapons", 38871},
            {"Stationary Weapons", 38872},
            {"Wheel Suspension 3x3 Right", 38873},
            {"Wheel Suspension 3x3 Left", 38874},
            {"Static Wheels", 38875},
            {"Shutters", 38876},
            {"Medium Corner Windows", 38877},
            {"Small Corner Windows", 38878},
            {"Medium Windows", 38879},
            {"Small Windows", 38880},
            {"Large Windows", 38881},
            {"Round Windows", 38882},
            {"Oxygen Bottle", 38883},
            {"Hydrogen Bottle", 38884},
            {"Iron Ingot", 38885},
            {"Nickel Ingot", 38886},
            {"Silicon Ingot", 38887},
            {"Gravel", 38888},
            {"Progressive Space Size", 38889},
        };

        public static Dictionary<string, long> allowedLocations = new Dictionary<string, long>()
        {
            {"Built Light Armor Block", 3883300},
            {"Built Heavy Armor Block", 3883301},
            {"Built Round Armor Slope", 3883302},
            {"Built Heavy Armor Round Slope", 3883303},
            {"Built Light Armor Ramps", 3883304},
            {"Built Light Armor Ramp Corners", 3883305},
            {"Built Heavy Armor Ramps", 3883306},
            {"Built Heavy Armor Ramp Corners", 3883307},
            {"Built Light Armor Sloped Corners", 3883308},
            {"Built Heavy Armor Sloped Corners", 3883309},
            {"Built DisplayName_BlockGroup_LightArmorTransitionBlocks", 3883310},
            {"Built DisplayName_BlockGroup_HeavyArmorTransitionBlocks", 3883311},
            {"Built DisplayName_Block_LightArmorPanel", 3883312},
            {"Built DisplayName_Block_heavyArmorPanel", 3883313},
            {"Built Projector", 3883314},
            {"Built Target Dummy", 3883315},
            {"Built Sound Block", 3883316},
            {"Built Button Panel", 3883317},
            {"Built Automation Blocks", 3883318},
            {"Built AI Flight {Move}", 3883319},
            {"Built Communication Blocks", 3883320},
            {"Built Remote Control", 3883321},
            {"Built Control Station", 3883322},
            {"Built Gyroscope", 3883323},
            {"Built Control Seat", 3883324},
            {"Built Door", 3883325},
            {"Built Airtight Hangar Door", 3883326},
            {"Built Blast Doors", 3883327},
            {"Built Store", 3883328},
            {"Built Battery", 3883329},
            {"Built Fueled Energy Sources", 3883330},
            {"Built Renewable Energy Sources", 3883331},
            {"Built Engineer Plushie", 3883332},
            {"Built Saberoid Plushie", 3883333},
            {"Built Anniversary Statue", 3883334},
            {"Built Gravity Blocks", 3883335},
            {"Built Passage", 3883336},
            {"Built Steel Catwalk", 3883337},
            {"Built Stairs", 3883338},
            {"Built DisplayName_Block_AirDucts", 3883339},
            {"Built Corner LCD Screens", 3883340},
            {"Built LCD Screens", 3883341},
            {"Built Lighting", 3883342},
            {"Built Gas Tanks", 3883343},
            {"Built Air Vent", 3883344},
            {"Built Cargo Containers", 3883345},
            {"Built Small Conveyor Tube", 3883346},
            {"Built Conveyor Junction", 3883347},
            {"Built Inputs/Outputs", 3883348},
            {"Built Piston", 3883349},
            {"Built Rotor", 3883350},
            {"Built DisplayName_Block_Hinge", 3883351},
            {"Built Medical Blocks", 3883352},
            {"Built Refinery", 3883353},
            {"Built O2/H2 Generator", 3883354},
            {"Built Assembler", 3883355},
            {"Built Oxygen Farm", 3883356},
            {"Built Upgrade Modules", 3883357},
            {"Built letters A to H", 3883358},
            {"Built DisplayName_BlockGroup_Numbers", 3883359},
            {"Built Symbols", 3883360},
            {"Built Large Ion Thruster", 3883361},
            {"Built Large Hydrogen Thruster", 3883362},
            {"Built Large Atmospheric Thruster", 3883363},
            {"Built Ship Tools", 3883364},
            {"Built Ore Detector", 3883365},
            {"Built Landing Gear", 3883366},
            {"Built Jump Drive", 3883367},
            {"Built Parachute Hatch", 3883368},
            {"Built Warhead", 3883369},
            {"Built Decoy", 3883370},
            {"Built Turreted Weapons", 3883371},
            {"Built Stationary Weapons", 3883372},
            {"Built Wheel Suspension 3x3 Right", 3883373},
            {"Built Wheel Suspension 3x3 Left", 3883374},
            {"Built Static Wheels", 3883375},
            {"Built Shutters", 3883376},
            {"Built Medium Corner Windows", 3883377},
            {"Built Small Corner Windows", 3883378},
            {"Built Medium Windows", 3883379},
            {"Built Small Windows", 3883380},
            {"Built Large Windows", 3883381},
            {"Built Round Windows", 3883382},
            {"MyObjectBuilder_Ore:Stone", 3883383},
            {"MyObjectBuilder_Ore:Iron", 3883384},
            {"MyObjectBuilder_Ore:Nickel", 3883385},
            {"MyObjectBuilder_Ore:Cobalt", 3883386},
            {"MyObjectBuilder_Ore:Magnesium", 3883387},
            {"MyObjectBuilder_Ore:Silicon", 3883388},
            {"MyObjectBuilder_Ore:Silver", 3883389},
            {"MyObjectBuilder_Ore:Gold", 3883390},
            {"MyObjectBuilder_Ore:Platinum", 3883391},
            {"MyObjectBuilder_Ore:Uranium", 3883392},
            {"MyObjectBuilder_Ore:Ice", 3883393},
            {"Visited Earth", 3883394},
            {"Visited Moon", 3883395},
            {"Visited Mars", 3883396},
            {"Visited Europa", 3883397},
            {"Visited Alien", 3883398},
            {"Visited Titan", 3883399},
            {"Visited Pertam", 3883400},
            {"Visited Space", 3883401},
            {"Visited Triton", 3883402},
            {"MyObjectBuilder_PhysicalGunObject:FlareGunItem", 3883403},
            {"MyObjectBuilder_AmmoMagazine:FlareClip", 3883404},
            {"MyObjectBuilder_Datapad:Datapad", 3883405},
            {"MyObjectBuilder_PhysicalGunObject:AngleGrinderItem", 3883406},
            {"MyObjectBuilder_PhysicalGunObject:HandDrillItem", 3883407},
            {"MyObjectBuilder_PhysicalGunObject:WelderItem", 3883408},
            {"MyObjectBuilder_PhysicalGunObject:SemiAutoPistolItem", 3883409},
            {"MyObjectBuilder_AmmoMagazine:SemiAutoPistolMagazine", 3883410},
            {"Assembled Fireworks", 3883411}, // There are multiple fireworks, therefore we need special code to merge them all into one check
            {"MyObjectBuilder_OxygenContainerObject:OxygenBottle", 3883412},
            {"MyObjectBuilder_GasContainerObject:HydrogenBottle", 3883413},
            {"MyObjectBuilder_PhysicalGunObject:AngleGrinder2Item", 3883414},
            {"MyObjectBuilder_PhysicalGunObject:AngleGrinder3Item", 3883415},
            {"MyObjectBuilder_PhysicalGunObject:AngleGrinder4Item", 3883416},
            {"MyObjectBuilder_PhysicalGunObject:Welder2Item", 3883417},
            {"MyObjectBuilder_PhysicalGunObject:Welder3Item", 3883418},
            {"MyObjectBuilder_PhysicalGunObject:Welder4Item", 3883419},
            {"MyObjectBuilder_PhysicalGunObject:HandDrill2Item", 3883420},
            {"MyObjectBuilder_PhysicalGunObject:HandDrill3Item", 3883421},
            {"MyObjectBuilder_PhysicalGunObject:HandDrill4Item", 3883422},
            {"MyObjectBuilder_PhysicalGunObject:FullAutoPistolItem", 3883423},
            {"MyObjectBuilder_PhysicalGunObject:ElitePistolItem", 3883424},
            {"MyObjectBuilder_PhysicalGunObject:AutomaticRifleItem", 3883425},
            {"MyObjectBuilder_PhysicalGunObject:RapidFireAutomaticRifleItem", 3883426},
            {"MyObjectBuilder_PhysicalGunObject:PreciseAutomaticRifleItem", 3883427},
            {"MyObjectBuilder_PhysicalGunObject:UltimateAutomaticRifleItem", 3883428},
            {"MyObjectBuilder_PhysicalGunObject:BasicHandHeldLauncherItem", 3883429},
            {"MyObjectBuilder_PhysicalGunObject:AdvancedHandHeldLauncherItem", 3883430},
            {"MyObjectBuilder_AmmoMagazine:FullAutoPistolMagazine", 3883431},
            {"MyObjectBuilder_AmmoMagazine:ElitePistolMagazine", 3883432},
            {"MyObjectBuilder_AmmoMagazine:AutomaticRifleGun_Mag_20rd", 3883433},
            {"MyObjectBuilder_AmmoMagazine:RapidFireAutomaticRifleGun_Mag_50rd", 3883434},
            {"MyObjectBuilder_AmmoMagazine:PreciseAutomaticRifleGun_Mag_5rd", 3883435},
            {"MyObjectBuilder_AmmoMagazine:UltimateAutomaticRifleGun_Mag_30rd", 3883436},
            {"MyObjectBuilder_AmmoMagazine:NATO_25x184mm", 3883437},
            {"MyObjectBuilder_AmmoMagazine:AutocannonClip", 3883438},
            {"MyObjectBuilder_AmmoMagazine:MediumCalibreAmmo", 3883439},
            {"MyObjectBuilder_AmmoMagazine:LargeCalibreAmmo", 3883440},
            {"MyObjectBuilder_AmmoMagazine:SmallRailgunAmmo", 3883441},
            {"MyObjectBuilder_AmmoMagazine:LargeRailgunAmmo", 3883442},
            {"MyObjectBuilder_AmmoMagazine:Missile200mm", 3883443}
        };
        public static void fill(AP_Items items, AP_Locations locations)
        {
            foreach(var itemPair in allowedItems)
            {
                items.Add(itemPair.Key, new AP_Item(itemPair.Key, itemPair.Value, isGroup:true));
            }
            foreach (var locationPair in allowedLocations)
            {
                locations.Add(locationPair.Key, new AP_Location(locationPair.Key, locationPair.Value));
            }
        }
        public static void gen(AP_Items items, AP_Locations locations)
        {
            long itemId = STARTING_ITEM_ID;
            long locationId = STARTING_LOCATION_ID;
            var definitions = MyDefinitionManager.Static.GetDefinitionsOfType<MyCubeBlockDefinition>();
            for (int i = 0; i < definitions.Count; ++i)
            {
                var definition = definitions[index: i];
                if ((definition.DLCs == null || definition.DLCs.Length == 0) && (definition.Context.ModName == null || definition.Context.ModName.Equals(""))
                    && definition.Public && definition.GuiVisible)
                {
                    if (definition.BlockVariantsGroup == null || definition.BlockVariantsGroup.Blocks == null || definition.BlockVariantsGroup.Blocks.Length == 0)
                    {
                        if (allowedItems.ContainsKey(definition.Id.TypeId + ":" + definition.Id.SubtypeName))
                            items.Add(definition.Id.TypeId + ":" + definition.Id.SubtypeName, new AP_Item(definition.Id.TypeId + ":" + definition.Id.SubtypeName, allowedItems[definition.Id.TypeId + ":" + definition.Id.SubtypeName]));

                        if (allowedLocations.ContainsKey(definition.Id.TypeId + ":" + definition.Id.SubtypeName))
                            locations.Add(definition.Id.TypeId + ":" + definition.Id.SubtypeName, new AP_Location("Welded Block " + definition.Id.TypeId + "-" + definition.Id.SubtypeName, allowedLocations[definition.Id.TypeId + ":" + definition.Id.SubtypeName]));
                        //MyLog.Default.WriteLine(definition.Id.TypeId + "-" + definition.Id.SubtypeName + ":" + locationId + ":false");
                        ++itemId;
                        ++locationId;
                    }
                    else
                    {
                        if (!items.ContainsKey(definition.BlockVariantsGroup.DisplayNameText))
                        {
                            if (allowedItems.ContainsKey(definition.BlockVariantsGroup.DisplayNameText))
                            {
                                items.Add(definition.BlockVariantsGroup.DisplayNameText, new AP_Item(definition.BlockVariantsGroup.DisplayNameText, allowedItems[definition.BlockVariantsGroup.DisplayNameText], true));
                                MyLog.Default.WriteLine(definition.BlockVariantsGroup.DisplayNameText + ":" + definition.Id.SubtypeName + ":" + allowedItems[definition.BlockVariantsGroup.DisplayNameText] + ":true:0");
                            }

                            if (allowedLocations.ContainsKey(definition.BlockVariantsGroup.DisplayNameText))
                                locations.Add(definition.BlockVariantsGroup.DisplayNameText, new AP_Location("Welded Block Group " + definition.BlockVariantsGroup.DisplayNameText, allowedLocations[definition.BlockVariantsGroup.DisplayNameText]));
                            //MyLog.Default.WriteLine(definition.BlockVariantsGroup.DisplayNameText + ":" + locationId + ":false");
                            //MyLog.Default.WriteLine("{\"" + definition.BlockVariantsGroup.DisplayNameText + "\", " + itemId + "},");
                            //MyLog.Default.WriteLine("Type: " + definition.Id.TypeId + " Subtype: " + definition.Id.SubtypeName + " AP_ID: " + itemId + " Group: " + definition.BlockVariantsGroup.DisplayNameText);
                            ++itemId;
                            ++locationId;
                        }
                    }
                }
            }

            var itemDefinitions = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
            foreach(var itemDefinition in itemDefinitions)
            {
                if (itemDefinition.IsOre)
                {
                    if (allowedLocations.ContainsKey(itemDefinition.Id.TypeId + ":" + itemDefinition.Id.SubtypeName))
                        locations.Add(itemDefinition.Id.TypeId + ":" + itemDefinition.Id.SubtypeName, new AP_Location("Found " + itemDefinition.Id.SubtypeName + " Ore", allowedLocations[itemDefinition.Id.TypeId + ":" + itemDefinition.Id.SubtypeName]));
                    //MyLog.Default.WriteLine(itemDefinition.Id.TypeId + "-" + itemDefinition.Id.SubtypeName + ":" + locationId + ":false");
                    MyLog.Default.WriteLine("{\"" + itemDefinition.Id.TypeId + ":" + itemDefinition.Id.SubtypeName + "\", " + locationId + "},");
                    ++locationId;
                }
            }
            foreach (var itemDefinition in itemDefinitions)
            {
                if (itemDefinition.CanPlayerOrder)
                {
                    MyLog.Default.WriteLine("{\"" + itemDefinition.Id.TypeId + ":" + itemDefinition.Id.SubtypeName + "\", " + locationId + "},");
                    ++locationId;
                }
            }
        }
    }
}
