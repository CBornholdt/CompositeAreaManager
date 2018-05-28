using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CompositeAreaManager
{
	public class CompositeAreaOp_Area : CompositeAreaOp
	{
		public Area areaRef = null;

		public CompositeAreaOp_Area() { }

		public override void ExposeData()
		{
			base.ExposeData ();
			Scribe_References.Look<Area> (ref this.areaRef, "AreaRef");
		}

		public override bool this [int cellIndex] { 
			get {
				return areaRef [cellIndex];
			}
		}

		public override IEnumerable<Area> AllAreaReferences {
			get { 
				if (areaRef != null)
					yield return areaRef;
			}
		}

		public override bool IsValid { 
			get {
				return areaRef != null && areaRef.areaManager.AllAreas.Contains (areaRef);
			}
		}
		public override string Label {
			get { return areaRef.Label; }
		}
	}
}

