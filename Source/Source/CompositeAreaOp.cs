using System;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;

namespace CompositeAreaManager
{
	public abstract class CompositeAreaOp : IExposable
	{
		public CompositeAreaOp arg1 = null;
		public CompositeAreaOp arg2 = null;

		public abstract bool this [int cellIndex] { get; }

		public virtual IEnumerable<Area> AllAreaReferences {
			get {
				if (arg1 != null)
					foreach (var area in arg1.AllAreaReferences)
						yield return area;
				if (arg2 != null)
					foreach (var area in arg2.AllAreaReferences)
						yield return area;
			}
		}

		IEnumerable<CompositeAreaOp> Children {
			get {
				if (arg1 != null)
					yield return arg1;
				if (arg2 != null)
					yield return arg2;
			}
		}

		public abstract bool IsValid { get; }

		public virtual void ExposeData()
		{
			Scribe_Deep.Look<CompositeAreaOp> (ref this.arg1, "Arg1");
			Scribe_Deep.Look<CompositeAreaOp> (ref this.arg2, "Arg2");
		}

		public void Traverse(Action<CompositeAreaOp> action)
		{
			action(this);
			foreach(var child in Children)
				child.Traverse(action);
		}

		public virtual string Label {
			get { return string.Empty; }
		}

		public virtual void PreRecalculate(){}
		public virtual void PostRecalculate(){}
	}

	public class CompositeAreaOp_Empty : CompositeAreaOp
	{
		public override bool this [int cellIndex] { 
			get { return false; }
		}
		public override bool IsValid { 
			get { return false; }
		}
	}

	public class CompositeAreaOp_Union : CompositeAreaOp
	{
		public override bool this [int cellIndex] { 
			get {
				return arg1 [cellIndex] || arg2 [cellIndex];
			}
		}
		public override bool IsValid { 
			get {
				return (arg1?.IsValid ?? false) && (arg2?.IsValid ?? false);
			}
		}
		public override string Label {
			//get { return " \u222A "; }
			get { return " OR "; }
		}
	}

	public class CompositeAreaOp_Intersect : CompositeAreaOp
	{
		public override bool this [int cellIndex] { 
			get {
				return arg1 [cellIndex] && arg2 [cellIndex];
			}
		}
		public override bool IsValid { 
			get {
				return (arg1?.IsValid ?? false) && (arg2?.IsValid ?? false);
			}
		}
		public override string Label {
			//get { return " \u2229 "; }
			get { return " AND "; }
		}
	}

	public class CompositeAreaOp_Invert : CompositeAreaOp
	{
		public override bool this [int cellIndex] { 
			get {
				return !arg1 [cellIndex];
			}
		}
		public override bool IsValid { 
			get {
				return (arg1?.IsValid ?? false);
			}
		}
		public override string Label {
			get { return " !() "; }
		}
	}

	public class CompositeAreaOp_Area : CompositeAreaOp
	{
		public Area areaRef = null;

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

	public class CompositeAreaOp_RoomRoleType : CompositeAreaOp
	{
		public RoomRoleDef roomRoleType = null;
		public Map map = null;
		Region[] regionGrid = null;

		public CompositeAreaOp_RoomRoleType(Map map, RoomRoleDef roomRoleType)
		{
			this.roomRoleType = roomRoleType;
			this.map = map;
		}

		public override void ExposeData()
		{
			base.ExposeData ();
			Scribe_Defs.Look<RoomRoleDef> (ref this.roomRoleType, "RoomRoleType");
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


	public class CompositeAreaOp_GrowingZone : CompositeAreaOp
	{
		public Map map = null;
		Zone[] zoneGrid = null;

		public CompositeAreaOp_GrowingZone(Map map)
		{
			this.map = map;
		}

		public override void PreRecalculate()
		{
			FieldInfo zoneGrid = typeof(ZoneManager).GetField ("zoneGrid", BindingFlags.Instance | BindingFlags.NonPublic);
			this.zoneGrid = (Zone[])zoneGrid.GetValue (map.zoneManager);
		}

		public override void PostRecalculate()
		{
			this.zoneGrid = null;
		}

		public override void ExposeData()
		{
			base.ExposeData ();
			Scribe_References.Look<Map> (ref this.map, "Map");
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
			get { return "[" + "GrowingZone".Translate() + "]"; }
		}
	}
}

