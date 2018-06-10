using System;
using Verse;
using RimWorld;

namespace CompositeAreaManager
{
	public class CompositeAreaOp_RoomRoleType : CompositeAreaOp
	{
		public RoomRoleDef roomRoleType = null;
		Region[] regionGrid = null;

		public CompositeAreaOp_RoomRoleType() : base(null) { }

		public CompositeAreaOp_RoomRoleType(Map map, RoomRoleDef roomRoleType) : base(map)
		{
			this.roomRoleType = roomRoleType;
			this.map = map;
		}

		public override void ExposeData()
		{
			base.ExposeData ();
			Scribe_Defs.Look<RoomRoleDef> (ref this.roomRoleType, "RoomRoleType");
		}

		public override void PreRecalculate()
		{
			this.regionGrid = map.regionGrid.DirectGrid;
		}

		public override void PostRecalculate()
		{
			this.regionGrid = null;
		}

		public override bool this [int cellIndex] { 
			get {
				return regionGrid[cellIndex]?.Room?.Role == roomRoleType;
			}
			//	return map != null && RegionAndRoomQuery.RoomAtFast(c, map).Role == roomRoleType;
		}
		public override bool IsValid { 
			get {
				return roomRoleType != null && map != null;
			}
		}
		public override string Label {
			get { return "[" + roomRoleType.LabelCap + "]"; }
		}
	}

	public class CompositeAreaOp_AnyRoomType : CompositeAreaOp
	{
		Region[] regionGrid = null;

		public CompositeAreaOp_AnyRoomType(Map map = null) : base(map) { }
	
		public override void ExposeData()
		{
			base.ExposeData ();
			Scribe_References.Look<Map> (ref this.map, "Map");
		}

		public override void PreRecalculate()
		{
			this.regionGrid = map.regionGrid.DirectGrid;
		}

		public override void PostRecalculate()
		{
			this.regionGrid = null;
		}

		public override bool this [int cellIndex] { 
			get {
				return (regionGrid[cellIndex]?.Room?.Role ?? RoomRoleDefOf.None) != RoomRoleDefOf.None;
			}
		}
		public override bool IsValid { 
			get {
				return map != null;
			}
		}
		public override string Label {
			get { return "[" + "AnyRoom".Translate() + "]"; }
		}
	}
}

