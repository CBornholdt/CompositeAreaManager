using System;
using UnityEngine;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace CompositeAreaManager
{
	public class Dialog_ManageCompositeAreas : Window
	{
		private Map map;

		private readonly float footerHeight = 100;
		private Vector2 scrollPosition = Vector2.zero;
		private float viewHeight = 800;
		static private Vector2 opButtonSize = new Vector2(20, 20);
		static public readonly float RowSpacing = 10;
		public static readonly string HolderString = "(*)";
		public static readonly float CellSpacing = 2f;

		public override Vector2 InitialSize {
			get {
				return new Vector2 (700, 700);
			}
		}

		public static float LineHeight {
			get {
				return Text.LineHeight + 1;
			}
		}

		public Dialog_ManageCompositeAreas (Map map) 
		{
			this.map = map;
			this.forcePause = true;
			this.doCloseX = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
		}

		private float DoCompositeAreaRowElement(int areaID, Rect inRect)
		{
			Rect rect = inRect.ContractedBy (CellSpacing);
			Area area = this.map.areaManager.AllAreas.First (a => a.ID == areaID);
			CompositeAreaManager cAM = this.map.GetComponent<CompositeAreaManager> ();

			CSONode rootNode = CSONode.GenerateFromCompositeAreaOperationNode (cAM.AllCompositeAreas [areaID]);
			rootNode.ResetToAllHeld ();
			float usedHeight = (float)rootNode.DrawSubTree (rect, areaID) * LineHeight;
			rect.yMin += usedHeight;
			Widgets.Label (rect, area.Label);
			usedHeight += Text.LineHeight;

			usedHeight += 2 * CellSpacing;

			Rect borderRect = new Rect (inRect.xMin, inRect.yMin, inRect.width, usedHeight);
			Widgets.DrawBox(borderRect);

			if (Mouse.IsOver (borderRect)) {
				area.MarkForDraw ();
				GUI.color = area.Color;
				Widgets.DrawHighlight (borderRect);
				GUI.color = Color.white;
			}

			return usedHeight;
		}

/*		private float DoOperationElement(Rect inRect, CompositeAreaOperation op, CompositeAreaOperation parentOp, int areaID)
		{
			Rect buttonRect = new Rect (new Vector2 (inRect.center.x - op.opButtonSize.x, inRect.yMin), opButtonSize);
			string text = op == null ? "0" : ((int)op.opType).ToString ();

			if (Widgets.ButtonText (buttonRect, text)) {
				List<FloatMenuOption> buttonMenuList = this.MakeOpButtonMenuList (parentOp);
				if (parentOp == null) //Only for root node
					foreach (var option in buttonMenuList)
						option.action = delegate {
						};
			}
							
		}	*/

		private void DoFooterContents(Rect inRect)
		{
			Vector2 size = Text.CalcSize ("NewCompositeArea".Translate ());
			Rect rect = new Rect (0, 0, size.x + 12, size.y + 6);
			CompositeAreaManager cAM = this.map.GetComponent<CompositeAreaManager> ();
			GUI.BeginGroup (inRect);
			//Must have at least 1 non composite area ...
			if (Widgets.ButtonText (rect, "NewCompositeArea".Translate (), true, false, cAM.AllCompositeAreas.Count () < cAM.AllCompositableAreas.Count () - 1)) {
				List<FloatMenuOption> newAreaList = new List<FloatMenuOption> ();
				foreach (Area area in cAM.AllCompositableAreas.Where(a => !cAM.AllCompositeAreas.Keys.Contains(a.ID)))
					newAreaList.Add (new FloatMenuOption ("Compose".Translate () + ": " + area.Label, 
									() => cAM.AllCompositeAreas.Add (area.ID, new CompositeAreaOperation())));
				Find.WindowStack.Add (new FloatMenu (newAreaList));
			}
			size = Text.CalcSize ("RemoveCompositeArea".Translate ());
			rect = new Rect (0, rect.height + 2 * CellSpacing, size.x + 12, size.y + 6);
			if (Widgets.ButtonText (rect, "RemoveCompositeArea".Translate (), true, false, cAM.AllCompositeAreas.Count() > 0)) {
				List<FloatMenuOption> newAreaList = new List<FloatMenuOption> ();
				foreach (Area area in cAM.AllCompositeAreas.Keys.Select(aID => map.areaManager.AllAreas.First(a => a.ID == aID)))
					newAreaList.Add (new FloatMenuOption ("Remove".Translate () + ": " + area.Label, 
						() => cAM.AllCompositeAreas.Remove (area.ID)));
				Find.WindowStack.Add (new FloatMenu (newAreaList));
			}
			GUI.EndGroup ();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect scrollRect = new Rect (inRect.xMin + 2, inRect.yMin + 2, inRect.width - 4, inRect.height - footerHeight - 4);
			Rect scrollViewRect = new Rect (0, 0, scrollRect.width - 2 * GUI.skin.verticalScrollbar.fixedWidth, this.viewHeight);
			Widgets.BeginScrollView (scrollRect, ref this.scrollPosition, scrollViewRect, true);

			CompositeAreaManager cAM = map.GetComponent<CompositeAreaManager> ();
			Rect drawRect = new Rect (scrollViewRect);
			foreach (int areaID in cAM.AllCompositeAreas.Keys) {
				drawRect.yMin += DoCompositeAreaRowElement (areaID, drawRect) + RowSpacing;
	//			Log.Message ("ViewHeight: " + drawRect.yMin.ToString ());
			}
			this.viewHeight = drawRect.yMin;

			Widgets.EndScrollView ();
			Rect footerRect = new Rect (inRect);
			footerRect.yMin = footerRect.yMax - this.footerHeight;
			this.DoFooterContents (footerRect);
		}

		public override void PreClose()
		{
			map.GetComponent<CompositeAreaManager> ().RecalculateAll();
		}

		public IEnumerable<FloatMenuOption> MakeOpButtonMenuOptions(CompositeAreaOperation op, int areaID)
		{
			yield return new FloatMenuOption ("Union".Translate (), delegate {
				op.opType = CompositeAreaOperationType.Union;
				op.arg1 = new CompositeAreaOperation ();
				op.arg2 = new CompositeAreaOperation ();
			});
			yield return new FloatMenuOption ("Intersect".Translate (), delegate {
				op.opType = CompositeAreaOperationType.Intersect;
				op.arg1 = new CompositeAreaOperation ();
				op.arg2 = new CompositeAreaOperation ();
			});
			yield return new FloatMenuOption ("Invert".Translate (), delegate {
				op.opType = CompositeAreaOperationType.Invert;
				op.arg1 = new CompositeAreaOperation ();
			});
			CompositeAreaManager cAM = map.GetComponent<CompositeAreaManager> ();
			List<Area> validLinkAreas = new List<Area>();
			foreach (Area area in cAM.AllCompositableAreas)
				if(cAM.AllCompositeAreas.TryGetValue(area.ID, out var op2)) {
					if(!op2.AllAreaReferences.Any(a => a.ID == area.ID))
						validLinkAreas.Add(area);
				} else 
					validLinkAreas.Add(area);

			foreach (Area area in validLinkAreas) {
				yield return new FloatMenuOption ("Area".Translate () + ": " + area.Label, delegate {
					op.opType = CompositeAreaOperationType.Area;
					op.areaArg = area;
				});
			}
		}

		//Node for combined operations ... really terrible name I know
		private	class CSONode
		{
			public List<CSONode> children = new List<CSONode>();
			public bool heldByParent = true;
			public CompositeAreaOperation op;

			private static int nextLineRectIndex = 0;

			public CSONode(CompositeAreaOperation op) {
				this.op = op;
			}

			public IEnumerable<CSONode> ThisLineNodes {
				get {
					yield return this;
					foreach (var child in children)
						if (child.heldByParent)
							foreach (var node in child.ThisLineNodes)
								yield return node;
				}
			}

			public IEnumerable<CSONode> NextLineNodes {
				get {
					foreach (var child in children)
						if (child.heldByParent)
							foreach (var node in child.NextLineNodes)
								yield return node;
						else
							yield return child;
				}
			}

			public int DrawSubTree(Rect inRect, int areaID)
			{					
				int totalLinesUsed = 0;
				List<CSONode> nextLineNodes = NextLineNodes.ToList();
				List<float> nextLineWidths = nextLineNodes.Select(n => n.MinSubTreeDisplayWidthNeeded()).ToList();
				float nextLineWidthsSum = nextLineWidths.Sum ();
				List<Rect> nextLineRects = new List<Rect>();
				float lastPartition = inRect.xMin;
				for(int i = 0; i < nextLineNodes.Count; i++) {
					float nextPartitionWidth = inRect.width * nextLineWidths [i] / nextLineWidthsSum;
					nextLineRects.Add (new Rect (lastPartition, inRect.yMin + LineHeight, nextPartitionWidth, inRect.height - LineHeight));
					lastPartition += nextPartitionWidth;
					int linesUsed = nextLineNodes[i].DrawSubTree(nextLineRects[i], areaID);
					if (linesUsed > totalLinesUsed)
						totalLinesUsed = linesUsed;
				}
										
				float totalLineWidth = 0;
				foreach (var node in ThisLineNodes) 
					totalLineWidth += node.RawWidth ();

				totalLineWidth += HolderWidth () * NextLineNodes.Count ();
				float centeringOffset = (inRect.width - totalLineWidth) / 2;
				nextLineRectIndex = 0;
				DrawSubTreeLine (new Rect (inRect.xMin + centeringOffset, inRect.yMin, inRect.width - centeringOffset, LineHeight), 
					areaID, nextLineRects);

				return totalLinesUsed + 1;
			}

			public float DrawSubTreeLine(Rect inRect, int areaID, List<Rect> nextLineRects)
			{
				if (children.Count == 0)
					return DrawRawElement (inRect, null, areaID);
					
				int childIndex = 0;
				Rect workingRect = new Rect(inRect);
				Func<Rect, float> drawChild = r => {
					if (children [childIndex].heldByParent)
						return children [childIndex].DrawSubTreeLine (r, areaID, nextLineRects);
					else {
						float adjustment = 0;
						if(op.opType != CompositeAreaOperationType.Invert)
							adjustment = DrawHoldingElement (r);
						Widgets.DrawLine(r.min + new Vector2(HolderWidth ()/2, LineHeight / 2), 
							new Vector2 (nextLineRects[nextLineRectIndex].center.x, nextLineRects[nextLineRectIndex].yMin + LineHeight / 2),
							Color.green, 1);
						nextLineRectIndex++;
						return adjustment;
					}
				};

				if (children.Count > 1) {
					workingRect.xMin += drawChild (workingRect);
					childIndex++;
				}

				for (; childIndex < children.Count; childIndex++) 
					workingRect.xMin += DrawRawElement (workingRect, drawChild);

				return workingRect.xMin - inRect.xMin;
			}

			public float DrawHoldingElement(Rect inRect)
			{
				Widgets.Label (inRect, HolderString);
				return Text.CalcSize (HolderString).x + CellSpacing;
			}

			public float DrawRawElement(Rect inRect, Func<Rect, float> drawChild = null, int areaID = -1)
			{
				float usedWidth = RawWidth();
				string rs = RawString ();

				Action drawRemoveElementAction = delegate {
					Rect rect = new Rect (inRect.min, new Vector2 (usedWidth, LineHeight)).ContractedBy(2);
					if (Mouse.IsOver (rect)) {
						Color currentColor = GUI.color;
						GUI.color = Color.red;
						Widgets.DrawBox (rect);
						GUI.color = currentColor;
					}
					if (Widgets.ButtonInvisible (rect))
						Find.WindowStack.Add (new FloatMenu (new List<FloatMenuOption> () { 
							new FloatMenuOption ("Remove".Translate (), delegate {
								op.opType = CompositeAreaOperationType.Null;
								op.arg1 = null;
								op.arg2 = null;
								op.areaArg = null;
							})
						}));
				};

				switch (op.opType) {
				case CompositeAreaOperationType.Area:
					Widgets.Label (inRect, op.areaArg.Label);
					drawRemoveElementAction ();
					return usedWidth;
				case CompositeAreaOperationType.Intersect:
					Widgets.Label (inRect, RawString ());
					drawRemoveElementAction ();
					usedWidth += drawChild (new Rect (inRect.xMin + usedWidth, inRect.yMin, inRect.width - usedWidth, inRect.height));
					return usedWidth;
				case CompositeAreaOperationType.Union:
					Widgets.Label (inRect, RawString ());
					drawRemoveElementAction ();
					usedWidth += drawChild (new Rect (inRect.xMin + usedWidth, inRect.yMin, inRect.width - usedWidth, inRect.height));
					return usedWidth;
				case CompositeAreaOperationType.Invert:
					Widgets.Label (inRect, rs.Substring (0, 3));
					usedWidth = Text.CalcSize (rs.Substring (0, 3)).x;
					float childWidth = drawChild (new Rect (inRect.xMin + usedWidth, inRect.yMin, inRect.width - usedWidth, inRect.height));
					usedWidth += childWidth;
					Widgets.Label (new Rect (inRect.xMin + usedWidth, inRect.yMin, inRect.width - usedWidth, inRect.height), rs.Substring (3));
					usedWidth = RawWidth () + childWidth;
					drawRemoveElementAction ();
					return usedWidth;
				case CompositeAreaOperationType.Null:
					DrawOpButton (inRect, areaID);
					return usedWidth;
				}	
				return RawWidth ();
			}

			public void DrawOpButton(Rect inRect, int areaID)
			{
				var dialog = Find.WindowStack.Windows.OfType<Dialog_ManageCompositeAreas> ().FirstOrDefault ();
				if (dialog == default(Dialog_ManageCompositeAreas))	//Can get called in frame when dialog is called ...
					return;
				if(Widgets.ButtonText (new Rect (inRect.min, opButtonSize), "+"))
					Find.WindowStack.Add(new FloatMenu(dialog.MakeOpButtonMenuOptions(op, areaID).ToList()));
			}

			public static CSONode GenerateFromCompositeAreaOperationNode(CompositeAreaOperation originalNode)
			{
				CSONode result = new CSONode(originalNode);
			
				Stack<CompositeAreaOperation> currentOps = new Stack<CompositeAreaOperation>();
				currentOps.Push (originalNode);
				while (currentOps.Count > 0) {
					CompositeAreaOperation op = currentOps.Pop ();
					if (op.opType == result.op.opType && op != originalNode) {
						if (op.arg2 != null)
							currentOps.Push (op.arg2);
						if (op.arg1 != null)
							currentOps.Push (op.arg1);
					} else {
						if (op.arg1 != null)
							result.children.Add (GenerateFromCompositeAreaOperationNode (op.arg1));
						if (op.arg2 != null)
							result.children.Add (GenerateFromCompositeAreaOperationNode (op.arg2));
					}
				}

				return result;
			}

			public float HolderWidth()
			{
				return Text.CalcSize (HolderString).x + CellSpacing;
			}

			public string RawString()
			{
				switch (op.opType) {
				case CompositeAreaOperationType.Area:
					return op.areaArg.Label;
				case CompositeAreaOperationType.Intersect:
					return " \u2229 ";
				case CompositeAreaOperationType.Union:
					return " \u222A ";
				case CompositeAreaOperationType.Invert:
					return " !( ) ";
				}

				return string.Empty;
			}

			public float RawWidth()
			{
				string rawString = RawString ();
				float multiple = 1;

				if (op.opType == CompositeAreaOperationType.Null)
					return opButtonSize.x + CellSpacing;
				if (op.opType == CompositeAreaOperationType.Union || op.opType == CompositeAreaOperationType.Intersect)
					multiple = (float)children.Count - 1;
				if (rawString.Length > 0)
					return multiple * (Text.CalcSize(rawString).x + CellSpacing);
				else
					return opButtonSize.x;
			}

			public void ResetToAllHeld()
			{
				foreach (var child in children) {
					child.heldByParent = !((child.op.opType == CompositeAreaOperationType.Intersect || child.op.opType == CompositeAreaOperationType.Union)
						&& op.opType != child.op.opType);
					child.ResetToAllHeld ();
				}
			}

			public float MinSubTreeDisplayWidthNeeded()
			{
				float minWidthThisLine = RawWidth();
				foreach (var node in ThisLineNodes)
					minWidthThisLine += node.RawWidth ();
				float minWidthNextLine = 0;
				foreach (var node in NextLineNodes) {
					minWidthThisLine += HolderWidth ();
					minWidthNextLine += node.MinSubTreeDisplayWidthNeeded ();
				}
				return (minWidthThisLine >= minWidthNextLine) ? minWidthThisLine : minWidthNextLine;
			}

			public float TotalSubTreeWidth()
			{
				float total = RawWidth ();
				foreach (var child in children)
					total += child.TotalSubTreeWidth ();
				return total;
			}
		}
	}
}

