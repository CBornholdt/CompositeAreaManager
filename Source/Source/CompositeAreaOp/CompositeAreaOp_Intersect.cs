using System;
using Verse;

namespace CompositeAreaManager
{
	public class CompositeAreaOp_Intersect : CompositeAreaOp
	{
		public CompositeAreaOp_Intersect(): base(null) 
        {
			arg1 = new CompositeAreaOp_Empty ();
			arg2 = new CompositeAreaOp_Empty ();
		}
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
			get { return " " + "CAM_AND".Translate() + " "; }
		}
	}
}

