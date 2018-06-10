using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CompositeAreaManager
{
    public class CompositeAreaOp_Building : CompositeAreaOp
    {
		public ThingDef building = null;
		public DesignationCategoryDef buildingCategory = null;

        public CompositeAreaOp_Building() : base(null) { }

		public CompositeAreaOp_Building(ThingDef building, Map map) : base(map)
		{
			this.building = building;
		}

		public CompositeAreaOp_Building(DesignationCategoryDef category, Map map) : base(map)
		{
			this.buildingCategory = category;
		}

        public override void ExposeData()
        {
            base.ExposeData ();
			Scribe_Defs.Look<ThingDef>(ref this.building,"Building");
			Scribe_Defs.Look<DesignationCategoryDef>(ref this.buildingCategory,"BuildingCategory");
        }

        public override bool this [int cellIndex] { 
            get {
				if (building != null)
					return map.thingGrid.ThingsListAtFast(cellIndex).Any(thing => thing.def == building);
				return map.thingGrid.ThingsListAtFast(cellIndex)
						.Any(thing => thing.def.designationCategory == buildingCategory);
            }
        }

        public override bool IsValid { 
            get {
				return building != null || buildingCategory != null;
			}
        }
        
        public override string Label {
            get {
				if(building != null)
					return "[" + "Building".Translate() + ": " + building.LabelCap + "]";
				return "[" + "BuildingCategory".Translate() + ": " + buildingCategory.LabelCap + "]";
			}
        }
    }
}
