using System;
using Verse;
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
}

