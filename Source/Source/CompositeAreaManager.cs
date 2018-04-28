using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CompositeAreaManager
{
	public class CompositeAreaManager : MapComponent
	{
		//List Area IDs as the Dictionary cannot be exposed if the Key is abstract ...
		private List<CompositeArea> compositeAreas;

		public List<CompositeArea> AllCompositeAreas {
			get {
				return this.compositeAreas;
			}
		}

		public IEnumerable<Area> AllCompositableAreas {
			get {
				return this.map.areaManager.AllAreas.Where (a => a.AssignableAsAllowed (AllowedAreaMode.Any));
			}
		}

		public IEnumerable<Area> AllPotentialNewCompositeAreas {
			get {
				return map.areaManager.AllAreas.Where (a => a.AssignableAsAllowed(AllowedAreaMode.Any) 
					&& !compositeAreas.Any (ca => ca.area == a));
			}
		}

		public CompositeAreaManager (Map map) : base(map)
		{
			this.compositeAreas = new List<CompositeArea> ();
		}

		public CompositeArea CompositeAreaForOrNull(Area area)
		{
			return this.compositeAreas.FirstOrDefault (a => a.area == area);
		}

		public void LaunchDialog_ManageCompositeAreas()
		{
			Dialog_ManageCompositeAreas dialog = new Dialog_ManageCompositeAreas (this.map);
			Find.WindowStack.Add (dialog);
		}

		public void RecalculateAll()
		{
			foreach (var compositeArea in compositeAreas)
				compositeArea.Recalculate ();
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<CompositeArea> (ref this.compositeAreas, "CompositeAreas");
		}

		public override void MapComponentTick()
		{
			if((Find.TickManager.TicksGame % 1000) != 873)
				return;

			RecalculateAll();
		}
	}
}

