using System;
using RimWorld;
using Verse;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace CompositeAreaManager
{
    public class CompositeAreaOp_OnFire : CompositeAreaOp
    {
        private List<Thing>[] thingGrid;
        
        public CompositeAreaOp_OnFire() : base(null) { }

        public CompositeAreaOp_OnFire(Map map) : base(map) { }

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
				return thingGrid[cellIndex].Any(thing => thing is Fire);
            }
        }
		public override bool IsValid => true;
           
        public override string Label {
            get { return "[" + "OnFire".Translate() + "]"; }
        }
    }
}
