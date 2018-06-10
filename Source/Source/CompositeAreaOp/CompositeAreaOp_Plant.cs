using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System.Reflection;

namespace CompositeAreaManager
{
    public class CompositeAreaOp_Plant : CompositeAreaOp
    {
		private List<Thing>[] thingGrid;
    
        public CompositeAreaOp_Plant() : base(null) { }

		public CompositeAreaOp_Plant(Map map) : base(map) { }
        
        public override void PreRecalculate()
        {
			FieldInfo thingGridField = typeof(ThingGrid).GetField("thingGrid", BindingFlags.Instance | BindingFlags.NonPublic);
			this.thingGrid = (List<Thing>[])thingGridField.GetValue(map.thingGrid);
        }

        public override void PostRecalculate()
        {
			this.thingGrid = null;
        }

        public override bool this [int cellIndex] { 
            get {
				return thingGrid[cellIndex].Any(thing => thing is Plant); 
            }
        }

		public override bool IsValid => true;

		public override string Label {
            get { return "[" + "Plants".Translate() + "]"; }
        }
    }
}

