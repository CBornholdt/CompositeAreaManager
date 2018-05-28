using System;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace CompositeAreaManager
{
	public class CompositeArea : IExposable 
	{
		public CompositeAreaOp rootOp;
		public Area area;

		public CompositeArea (Area area, CompositeAreaOp op)
		{
			this.area = area;
			this.rootOp = op;
		}

		public CompositeArea(Area area)
		{
			this.area = area;
			this.rootOp = new CompositeAreaOp_Empty ();
		}

		public IEnumerable<Area> AllValidAdditionalAreaReferences {
			get {
				CompositeAreaManager cAM = area.Map.GetComponent<CompositeAreaManager> ();
				return area.areaManager.AllAreas.Where (a => !(cAM.CompositeAreaForOrNull(a)?.AllAreaReferences.Contains(this.area) ?? false));
			}
		}

		public IEnumerable<Area> AllAreaReferences {
			get {
				return rootOp.AllAreaReferences.Concat(this.area);
			}
		}

		public bool IsValid {
			get {
				return rootOp.IsValid;
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<CompositeAreaOp> (ref this.rootOp, "RootOP");
			Scribe_References.Look<Area> (ref this.area, "Area");
		}

		public void Recalculate()
		{
			if (!IsValid)
				return;
			Log.Message ("Rebuilding: " + area.Label);
			rootOp.Traverse (op => op.PreRecalculate ());
			for(int cellIndex = 0; cellIndex < area.Map.cellIndices.NumGridCells; cellIndex++) 
				area [cellIndex] = rootOp [cellIndex];
			rootOp.Traverse (op => op.PostRecalculate ());
		}
	}
}

