﻿using System;
using Verse;

namespace CompositeAreaManager
{
	public class CompositeAreaOp_Invert : CompositeAreaOp
	{
		public CompositeAreaOp_Invert() : base(null)
		{
			arg1 = new CompositeAreaOp_Empty();
		}

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
			get { return "CAM_NOT".Translate() + "() "; }
		}
	}
}

