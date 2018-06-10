using System;
using RimWorld;
using Verse;
using System.Reflection;

namespace CompositeAreaManager
{
	public class CompositeAreaOp_GrowingZone : CompositeAreaOp
	{
		Zone[] zoneGrid = null;

		public CompositeAreaOp_GrowingZone() : base(null) { }

		public CompositeAreaOp_GrowingZone(Map map) : base(map) { }

		public override void PreRecalculate()
		{
			FieldInfo zoneGrid = typeof(ZoneManager).GetField ("zoneGrid", BindingFlags.Instance | BindingFlags.NonPublic);
			this.zoneGrid = (Zone[])zoneGrid.GetValue (map.zoneManager);
		}

		public override void PostRecalculate()
		{
			this.zoneGrid = null;
		}

		public override bool this [int cellIndex] { 
			get {
				return zoneGrid [cellIndex] is Zone_Growing;
			}
			//	return map != null && RegionAndRoomQuery.RoomAtFast(c, map).Role == roomRoleType;
		}
		public override bool IsValid { 
			get {
				return map != null;
			}
		}
		public override string Label {
			get { return "[" + "CAM_GrowingZone".Translate() + "]"; }
		}
	}
}

