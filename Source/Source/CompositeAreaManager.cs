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
		private Dictionary<int, CompositeAreaOperation> compositeAreas;
		private List<int> compositeAreasKeyHelper;
		private List<CompositeAreaOperation> compositeAreasValueHelper;

		public Dictionary<int, CompositeAreaOperation> AllCompositeAreas {
			get {
				return this.compositeAreas;
			}
		}

		public IEnumerable<Area> AllCompositableAreas {
			get {
				return this.map.areaManager.AllAreas.Where (a => a.AssignableAsAllowed (AllowedAreaMode.Any));
			}
		}

		public List<Area> AllValidAdditionalAreaReferences(int areaID)
		{
			HashSet<Area> alreadyIncluded = new HashSet<Area> ();
			alreadyIncluded.AddRange (compositeAreas [areaID].AllAreaReferences.ToList());
			int lastCount = 0;
			while (lastCount < alreadyIncluded.Count) {
				lastCount = alreadyIncluded.Count;
				for(int i = alreadyIncluded.Count; --i >= 0; )
					if(compositeAreas.TryGetValue(alreadyIncluded.ElementAt(i).ID, out var op))
						alreadyIncluded.AddRange(op.AllAreaReferences.ToList());
			}
			return AllCompositableAreas.Where (a => !alreadyIncluded.Contains (a)).ToList ();
		}

		public CompositeAreaManager (Map map) : base(map)
		{
			this.compositeAreas = new Dictionary<int, CompositeAreaOperation> ();
		}

		private Area AreaForID(int id)
		{
			return map.areaManager.AllAreas.FirstOrDefault (a => a.ID == id);
		}

		public void LaunchDialog_ManageCompositeAreas()
		{
			Dialog_ManageCompositeAreas dialog = new Dialog_ManageCompositeAreas (this.map);
			Find.WindowStack.Add (dialog);
		}

		public void RecalculateAll()
		{
			foreach (int areaID in compositeAreas.Keys)
				Recalculate (areaID);
		}

		public void Recalculate(int areaID)
		{
			if (!compositeAreas [areaID].IsValid)
				return;

			Area area = AreaForID (areaID);
			foreach (IntVec3 cell in area.Map.AllCells)
				area [cell] = compositeAreas [areaID] [cell];
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<int, CompositeAreaOperation> (ref this.compositeAreas, "CompositeAreas", 
				LookMode.Value, LookMode.Deep, ref this.compositeAreasKeyHelper, ref this.compositeAreasValueHelper);
		}

		public override void MapComponentTick()
		{
			if((Find.TickManager.TicksGame % 1000) != 873)
				return;

			RecalculateAll();
		}
	}
}

