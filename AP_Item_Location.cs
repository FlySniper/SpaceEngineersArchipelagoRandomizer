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
            {"Light Armor Block", 3883300},
            {"Heavy Armor Block", 3883301},
            {"Round Armor Slope", 3883302},
            {"Heavy Armor Round Slope", 3883303},
            {"Light Armor Ramps", 3883304},
            {"Light Armor Ramp Corners", 3883305},
            {"Heavy Armor Ramps", 3883306},
            {"Heavy Armor Ramp Corners", 3883307},
            {"Light Armor Sloped Corners", 3883308},
            {"Heavy Armor Sloped Corners", 3883309},
            {"DisplayName_BlockGroup_LightArmorTransitionBlocks", 3883310},
            {"DisplayName_BlockGroup_HeavyArmorTransitionBlocks", 3883311},
            {"DisplayName_Block_LightArmorPanel", 3883312},
            {"DisplayName_Block_heavyArmorPanel", 3883313},
            {"Projector", 3883314},
            {"Target Dummy", 3883315},
            {"Sound Block", 3883316},
            {"Button Panel", 3883317},
            {"Automation Blocks", 3883318},
            {"AI Flight {Move}", 3883319},
            {"Communication Blocks", 3883320},
            {"Remote Control", 3883321},
            {"Control Station", 3883322},
            {"Gyroscope", 3883323},
            {"Control Seat", 3883324},
            {"Door", 3883325},
            {"Airtight Hangar Door", 3883326},
            {"Blast Doors", 3883327},
            {"Store", 3883328},
            {"Battery", 3883329},
            {"Fueled Energy Sources", 3883330},
            {"Renewable Energy Sources", 3883331},
            {"Engineer Plushie", 3883332},
            {"Saberoid Plushie", 3883333},
            {"Anniversary Statue", 3883334},
            {"Gravity Blocks", 3883335},
            {"Passage", 3883336},
            {"Steel Catwalk", 3883337},
            {"Stairs", 3883338},
            {"DisplayName_Block_AirDucts", 3883339},
            {"Corner LCD Screens", 3883340},
            {"LCD Screens", 3883341},
            {"Lighting", 3883342},
            {"Gas Tanks", 3883343},
            {"Air Vent", 3883344},
            {"Cargo Containers", 3883345},
            {"Small Conveyor Tube", 3883346},
            {"Conveyor Junction", 3883347},
            {"Inputs/Outputs", 3883348},
            {"Piston", 3883349},
            {"Rotor", 3883350},
            {"DisplayName_Block_Hinge", 3883351},
            {"Medical Blocks", 3883352},
            {"Refinery", 3883353},
            {"O2/H2 Generator", 3883354},
            {"Assembler", 3883355},
            {"Oxygen Farm", 3883356},
            {"Upgrade Modules", 3883357},
            {"letters A to H", 3883358},
            {"DisplayName_BlockGroup_Numbers", 3883359},
            {"Symbols", 3883360},
            {"Large Ion Thruster", 3883361},
            {"Large Hydrogen Thruster", 3883362},
            {"Large Atmospheric Thruster", 3883363},
            {"Ship Tools", 3883364},
            {"Ore Detector", 3883365},
            {"Landing Gear", 3883366},
            {"Jump Drive", 3883367},
            {"Parachute Hatch", 3883368},
            {"Warhead", 3883369},
            {"Decoy", 3883370},
            {"Turreted Weapons", 3883371},
            {"Stationary Weapons", 3883372},
            {"Wheel Suspension 3x3 Right", 3883373},
            {"Wheel Suspension 3x3 Left", 3883374},
            {"Static Wheels", 3883375},
            {"Shutters", 3883376},
            {"Medium Corner Windows", 3883377},
            {"Small Corner Windows", 3883378},
            {"Medium Windows", 3883379},
            {"Small Windows", 3883380},
            {"Large Windows", 3883381},
            {"Round Windows", 3883382},
            {"Stone", 3883383},
            {"Iron", 3883384},
            {"Nickel", 3883385},
            {"Cobalt", 3883386},
            {"Magnesium", 3883387},
            {"Silicon", 3883388},
            {"Silver", 3883389},
            {"Gold", 3883390},
            {"Platinum", 3883391},
            {"Uranium", 3883392},
            {"Ice", 3883393},
            {"Visited Earth", 3883394},
            {"Visited Moon", 3883395},
            {"Visited Mars", 3883396},
            {"Visited Europa", 3883397},
            {"Visited Alien", 3883398},
            {"Visited Titan", 3883399},
            {"Visited Pertam", 3883400},
            {"Visited Space", 3883401},
            {"Visited Triton", 3883402}
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
        }
    }
}
