using System;
using Verse;
using System.Collections.Generic;

namespace CompositeAreaManager
{
	public enum CompositeAreaOperationType { Null, Union, Intersect, Invert, Area };

	public class CompositeAreaOperation : IExposable
	{
		public CompositeAreaOperationType opType = CompositeAreaOperationType.Null;
		public CompositeAreaOperation arg1 = null;
		public CompositeAreaOperation arg2 = null;
		public Area areaArg = null;

		public bool this [IntVec3 c] {
			get {
				switch (this.opType) {
				case CompositeAreaOperationType.Area:
					return areaArg [c];
				case CompositeAreaOperationType.Invert:
					return !arg1 [c];
				case CompositeAreaOperationType.Intersect:
					return arg1 [c] && arg2 [c];
				case CompositeAreaOperationType.Union:
					return arg1 [c] || arg2 [c];
				}
				return false;
			}
		}

		public IEnumerable<Area> AllAreaReferences {
			get {
				if (areaArg != null)
					yield return areaArg;
				if (arg1 != null)
					foreach (var area in arg1.AllAreaReferences)
						yield return area;
				if (arg2 != null)
					foreach (var area in arg2.AllAreaReferences)
						yield return area;
			}
		}

		public bool IsValid {
			get {
				switch (this.opType) {
				case CompositeAreaOperationType.Area:
					return areaArg != null;
				case CompositeAreaOperationType.Invert:
					return arg1?.IsValid ?? false;
				case CompositeAreaOperationType.Intersect:
					return (arg1?.IsValid ?? false) && (arg2?.IsValid ?? false);
				case CompositeAreaOperationType.Union:
					return (arg1?.IsValid ?? false) && (arg2?.IsValid ?? false);
				}
				return false;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<CompositeAreaOperationType> (ref this.opType, "OperationType");
			Scribe_Deep.Look<CompositeAreaOperation> (ref this.arg1, "Arg1");
			Scribe_Deep.Look<CompositeAreaOperation> (ref this.arg2, "Arg2");
			Scribe_References.Look<Area> (ref this.areaArg, "AreaArg");
		}

		public static CompositeAreaOperation CreateIntersect(CompositeAreaOperation op1, CompositeAreaOperation op2)
		{
			return new CompositeAreaOperation {
				opType = CompositeAreaOperationType.Intersect,
				arg1 = op1,
				arg2 = op2
			};
		}

		public static CompositeAreaOperation CreateUnion(CompositeAreaOperation op1, CompositeAreaOperation op2)
		{
			return new CompositeAreaOperation {
				opType = CompositeAreaOperationType.Union,
				arg1 = op1,
				arg2 = op2
			};
		}

		public static CompositeAreaOperation CreateInvert(CompositeAreaOperation op1)
		{
			return new CompositeAreaOperation {
				opType = CompositeAreaOperationType.Invert,
				arg1 = op1
			};
		}

		public static CompositeAreaOperation CreateFromArea(Area area)
		{
			return new CompositeAreaOperation {
				opType = CompositeAreaOperationType.Area,
				areaArg = area
			};
		}
	}
}

