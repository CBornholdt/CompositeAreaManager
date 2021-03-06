﻿using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace CompositeAreaManager
{
	public class CompositeAreaOp_DisplayNode
	{
		public CompositeAreaOp_DisplayNode parent;
		public bool heldByParent = true;
		public CompositeAreaOp op;
		public CompositeArea parentArea;
		public Dialog_ManageCompositeAreas dialog;
		public static readonly string HolderString = "( )";
		public CompositeAreaOp_DisplayNode childBefore = null;
		public CompositeAreaOp_DisplayNode childAfter = null;

		public CompositeAreaOp_DisplayNode(CompositeAreaOp op, CompositeArea parentArea, Dialog_ManageCompositeAreas dialog, 
			CompositeAreaOp_DisplayNode parent = null) {
			this.op = op;
			this.parentArea = parentArea;
			this.dialog = dialog;
			this.parent = parent;
		}

		public float LineHeight {
			get { return dialog.LineHeight; }
		}

		public float CellSpacing {
			get { return dialog.CellSpacing; }
		}

		public float RowSpacing {
			get { return dialog.RowSpacing; }
		}

		public Vector2 OpButtonSize {
			get { return new Vector2 (LineHeight * 0.9f, LineHeight * 0.9f); }
		}

		public IEnumerable<CompositeAreaOp_DisplayNode> ThisLineNodes {
			get {
				if (childBefore != null) {
					if (childBefore.heldByParent)
						foreach (var node in childBefore.ThisLineNodes)
							yield return node;
					else
						yield return childBefore;
				}
				yield return this;
				if (childAfter != null) {
					if (childAfter.heldByParent)
						foreach (var node in childAfter.ThisLineNodes)
							yield return node;
					else
						yield return childAfter;
				}
			}
		}

		public IEnumerable<CompositeAreaOp_DisplayNode> Children {
			get {
				if (childBefore != null)
					yield return childBefore;
				if (childAfter != null)
					yield return childAfter;
			}
		}

		public IEnumerable<CompositeAreaOp_DisplayNode> NextLineNodes {
			get {
				foreach (var child in Children)
					if (child.heldByParent)
						foreach (var node in child.NextLineNodes)
							yield return node;
					else
						yield return child;
			}
		}

		public bool ShouldDrawHolderElement {
			get {
				return !(op is CompositeAreaOp_Invert);
			}
		}

		public Vector2 HolderPointAdjustment {
			get {
				if (ShouldDrawHolderElement)
					return Text.CalcSize (HolderString) / 2;
				if (op is CompositeAreaOp_Invert)
					return new Vector2 (Text.CalcSize (" (!").x, LineHeight / 2);
				return Vector2.zero;
			}
		}

		public float TotalWidthOnItsLine {
			get {
				return RawWidth() + Children.Sum (child => {
					if (child.heldByParent)
						return child.TotalWidthOnItsLine;
					return HolderWidth ();
				});
			}
		}

		public float TotalWidthOnThisLine {
			get {
				if (heldByParent)
					return TotalWidthOnItsLine;
				else
					return HolderWidth ();
			}
		}

		public int DrawSubTree(Rect inRect, Queue<Vector2> prevHolderLocs = null)
		{		
			List<CompositeAreaOp_DisplayNode> nextLineNodes = NextLineNodes.ToList();
			float centeringXOffset = (inRect.width - TotalWidthOnItsLine) / 2;
			Queue<Vector2> newHolderLocs = new Queue<Vector2> ();
			DrawSubTreeLine (new Rect (inRect.xMin + centeringXOffset, inRect.yMin, inRect.width - centeringXOffset, LineHeight), prevHolderLocs, newHolderLocs);

			int totalLinesUsed = 0;
			var subTreeWidths = PartitionWidthForSubtrees (inRect.width).ToList ();
			Rect subTreeRect = new Rect (inRect.xMin, inRect.yMin + LineHeight, inRect.width, inRect.height - LineHeight);
			for(int i = 0; i < nextLineNodes.Count; i++) {
				subTreeRect.width = subTreeWidths [i];
				int linesUsed = nextLineNodes[i].DrawSubTree(subTreeRect, newHolderLocs);
				if (linesUsed > totalLinesUsed)
					totalLinesUsed = linesUsed;
				subTreeRect.xMin = subTreeRect.xMax;	
			}

			return totalLinesUsed + 1;
		}

		private class RectHelper {	//Will be a reference type ... needed for lambdas to access
			public Rect rect;
		};

		public void DrawSubTreeLine(Rect inRect, Queue<Vector2> prevHolderLocs, Queue<Vector2> newHolderLocs)
		{
			RectHelper helper = new RectHelper(){ rect = new Rect (inRect) };	//should be called workingRectHelper but mah fingers

			Action<CompositeAreaOp_DisplayNode> drawOpRecur = null;
			Action<CompositeAreaOp_DisplayNode, bool> drawChild = (child, drawHolder) => {
				if (child != null) {
					if (child.heldByParent)
						drawOpRecur (child);
					else {
						if(drawHolder) {
							this.DrawHolderElement(helper);
							newHolderLocs.Enqueue(helper.rect.min + new Vector2(-HolderWidth() / 2, LineHeight / 2));
						}
						else
                            newHolderLocs.Enqueue (new Vector2(helper.rect.min.x, helper.rect.min.y + LineHeight / 2));
					}
				}
			};

			drawOpRecur = operation => {
				drawChild(operation.childBefore, this.ShouldDrawHolderElement);
				if(!operation.heldByParent) //Can only happen for root op
					Widgets.DrawLine(prevHolderLocs.Dequeue(),helper.rect.min + operation.HolderTargetAdjustment(), Color.green, 2);
				operation.DrawElement(helper, prevHolderLocs, () => drawChild(operation.childAfter, this.ShouldDrawHolderElement)); 
			};

			drawOpRecur (this);
		}

		private void DrawHolderElement(RectHelper helper)
		{
			Widgets.Label (helper.rect, HolderString);
			helper.rect.xMin += Text.CalcSize (HolderString).x + CellSpacing;
		}

		void DrawElement(RectHelper helper, Queue<Vector2> prevHoldingPoints, Action drawChild = null)
		{
			float usedWidth = RawWidth();
			string label = op.Label;

			Action highlightAndDrawRemoveElementAction = delegate {
				Rect rect = new Rect (helper.rect.min, new Vector2 (usedWidth - CellSpacing, LineHeight));
				if (Mouse.IsOver (rect)) {
					Color currentColor = GUI.color;
					GUI.color = Color.red;
					Widgets.DrawBox (rect);
					GUI.color = currentColor;
				}
				if (Widgets.ButtonInvisible (rect))
					Find.WindowStack.Add (new FloatMenu (new List<FloatMenuOption> () { 
						new FloatMenuOption ("CAM_Remove".Translate (), () => this.ReplaceOperationWith(new CompositeAreaOp_Empty()))
					}));
			};

			if(op is CompositeAreaOp_Intersect || op is CompositeAreaOp_Union) {
				Widgets.Label (helper.rect, label);
				highlightAndDrawRemoveElementAction ();
				helper.rect.xMin += usedWidth;
				drawChild ();
				return;
			}
			CompositeAreaOp_Area areaOp = op as CompositeAreaOp_Area;
			if (areaOp != null) {
				Color currentColor = GUI.color;
				GUI.color = areaOp.areaRef.Color;
				Widgets.Label (helper.rect, areaOp.areaRef.Label);
				GUI.color = currentColor;
				highlightAndDrawRemoveElementAction ();
				helper.rect.xMin += usedWidth;
				return;
			}
			if (op is CompositeAreaOp_Invert) {
				Widgets.Label (helper.rect, label.Substring (0, 4));
				usedWidth = Text.CalcSize (label.Substring (0, 4)).x;
				highlightAndDrawRemoveElementAction ();
				helper.rect.xMin += usedWidth;
				drawChild ();
				Widgets.Label (helper.rect, label.Substring (4));
				helper.rect.xMin += Text.CalcSize(label.Substring(4)).x + CellSpacing;
				return;
			}
			if (op is CompositeAreaOp_Empty) {
				DrawOpButton (helper.rect);
				helper.rect.xMin += usedWidth;
				return;
			}
            
            Widgets.Label (helper.rect, label);
            highlightAndDrawRemoveElementAction ();
            helper.rect.xMin += usedWidth;
		}

		public void DrawOpButton(Rect inRect)
		{
			if(Widgets.ButtonText (new Rect (inRect.min, OpButtonSize), "+"))
				Find.WindowStack.Add(new FloatMenu(MakeOpButtonMenuOptions().ToList()));
		}

		public static CompositeAreaOp_DisplayNode GenerateFromCompositeArea(CompositeArea area, Dialog_ManageCompositeAreas dialog)
		{
			Func<CompositeAreaOp, CompositeAreaOp_DisplayNode, CompositeAreaOp_DisplayNode>	createAndSetupNodeRecur = null;
			createAndSetupNodeRecur = (op, parentNode) => {
				CompositeAreaOp_DisplayNode node = new CompositeAreaOp_DisplayNode (op, area, dialog, parentNode);
				if (op.arg2 != null) 
					node.childBefore = createAndSetupNodeRecur(node.op.arg2, node);
				if (op.arg1 != null) 
					node.childAfter = createAndSetupNodeRecur(node.op.arg1, node);
				return node;
			};

			return createAndSetupNodeRecur (area.rootOp, null);
		}

		public Vector2 HolderTargetAdjustment()
		{
			return new Vector2 (RawWidth () / 2, LineHeight / 2);
		}

		public float HolderWidth()
		{
			return Text.CalcSize (HolderString).x + CellSpacing;
		}

		public IEnumerable<FloatMenuOption> MakeOpButtonMenuOptions()
		{
			yield return new FloatMenuOption ("CAM_Union".Translate (), () => ReplaceOperationWith (
				new CompositeAreaOp_Union ()));
			yield return new FloatMenuOption ("CAM_Intersect".Translate (), () => ReplaceOperationWith (
				new CompositeAreaOp_Intersect ()));
			yield return new FloatMenuOption ("CAM_Invert".Translate (), () => ReplaceOperationWith (
				new CompositeAreaOp_Invert ()));
				
			List<FloatMenuOption> mapReferenceList = new List<FloatMenuOption> ();
			foreach (Area area in parentArea.AllValidAdditionalAreaReferences)
				mapReferenceList.Add (new FloatMenuOption (area.Label, () => ReplaceOperationWith (
					new CompositeAreaOp_Area () { areaRef = area })));
			yield return new FloatMenuOption ("CAM_AreaReference".Translate (), () =>
				Find.WindowStack.Add (new FloatMenu (mapReferenceList)));

			List<FloatMenuOption> roomRoleTypeList = new List<FloatMenuOption> ();
			roomRoleTypeList.Add (new FloatMenuOption ("CAM_AnyRoom".Translate (), () => ReplaceOperationWith (
				new CompositeAreaOp_AnyRoomType (dialog.Map))));
			foreach (RoomRoleDef roomRole in DefDatabase<RoomRoleDef>.AllDefsListForReading)
				roomRoleTypeList.Add (new FloatMenuOption (roomRole.LabelCap, 
					() => ReplaceOperationWith (new CompositeAreaOp_RoomRoleType (dialog.Map, roomRole))));
			yield return new FloatMenuOption ("CAM_RoomRole".Translate (), () =>
				Find.WindowStack.Add (new FloatMenu (roomRoleTypeList)));

			yield return new FloatMenuOption ("CAM_GrowingZone".Translate (), () => ReplaceOperationWith (
				new CompositeAreaOp_GrowingZone (dialog.Map)));

			List<FloatMenuOption> buildingDesignationOptions = new List<FloatMenuOption>();
			foreach (var designation in DefDatabase<DesignationCategoryDef>.AllDefsListForReading) {
				var buildingsInCat = DefDatabase<ThingDef>.AllDefs
										.Where(def => def.category == ThingCategory.Building
												&& def.designationCategory == designation);
				if (buildingsInCat.Any()) {
					List<FloatMenuOption> buildingOptions = new List<FloatMenuOption>();
					if(buildingsInCat.Count() > 1)
						buildingOptions.Add(new FloatMenuOption("CAM_AnyCategoryBuildings".Translate(designation.LabelCap)
							, () => ReplaceOperationWith(new CompositeAreaOp_Building(designation, dialog.Map))));

					foreach(var thingDef in buildingsInCat)
						buildingOptions.Add(new FloatMenuOption(thingDef.LabelCap
							, () => ReplaceOperationWith(new CompositeAreaOp_Building(thingDef, dialog.Map))));
					buildingDesignationOptions.Add(new FloatMenuOption
                        ("CAM_Category".Translate() + ": " + designation.LabelCap
						, () => Find.WindowStack.Add(new FloatMenu(buildingOptions))));
				}
			}
			yield return new FloatMenuOption("CAM_Buildings".Translate()
								, () => Find.WindowStack.Add(new FloatMenu(buildingDesignationOptions)));

			yield return new FloatMenuOption("CAM_Plants".Translate()
								, () => ReplaceOperationWith(new CompositeAreaOp_Plant(dialog.Map)));
                                                                
            yield return new FloatMenuOption("CAM_OnFire".Translate()
                                , () => ReplaceOperationWith(new CompositeAreaOp_OnFire(dialog.Map)));                                                                
		}

		public float RawWidth()
		{
			if (op is CompositeAreaOp_Empty)
				return OpButtonSize.x + CellSpacing;

			return Text.CalcSize(op.Label).x + CellSpacing;
		}

		public void ReplaceOperationWith(CompositeAreaOp newOp)
		{
			if (parent != null) {
				if (parent.op.arg1 == op)
					parent.op.arg1 = newOp;
				if (parent.op.arg2 == op)
					parent.op.arg2 = newOp;
			} else if (parentArea.rootOp == op)
				parentArea.rootOp = newOp;
		}

		public IEnumerable<float> PartitionWidthForSubtrees(float width)
		{
			float nextSubTreeWidthsSum = NextLineNodes.Sum (node => node.MinSubTreeDisplayWidthNeeded());
			foreach(var subTreeTotalWidth in NextLineNodes.Select(node => node.MinSubTreeDisplayWidthNeeded()))
				yield return width * subTreeTotalWidth / nextSubTreeWidthsSum;
		}

		public void ResetHoldingsForWidth(float width)
		{
			this.ResetToAllHeld ();

			Action<CompositeAreaOp_DisplayNode, float> resetHoldingsRecur = null;
			resetHoldingsRecur = (node, w) => {
				var lineWidth = node.TotalWidthOnItsLine;
				if(lineWidth > w) {
					var adjustedNode = node.Children.Where(child => child.heldByParent).OrderByDescending(child => child.TotalWidthOnThisLine).FirstOrDefault();
					if(adjustedNode == null)
						return;
					adjustedNode.heldByParent = false;
					resetHoldingsRecur(node, w);
				} else {
					var subTreeWidths = node.PartitionWidthForSubtrees(w).ToList();
					var subTrees = node.NextLineNodes.ToList();
					for(int i = 0; i < subTrees.Count(); i++)
						resetHoldingsRecur(subTrees[i], subTreeWidths[i]);
				}
			};

			resetHoldingsRecur(this, width);
		}
	

		public void ResetToAllHeld()
		{
			foreach (var child in Children) {
				switch(CAM_Mod.settings.holdingStyle) {
				case HoldingStyle.Aggressively:
					child.heldByParent = true;
                    break;
				case HoldingStyle.Smart:
                    child.heldByParent = 
                    op is CompositeAreaOp_Invert //Invert Ops hold their children
                    || !((child.op is CompositeAreaOp_Intersect || child.op is CompositeAreaOp_Union)
                        && op.GetType() != child.op.GetType()); //Intersect and Unions not held unless same as parent
                    break;
				case HoldingStyle.Normal:
                    child.heldByParent = 
                    op is CompositeAreaOp_Invert //Invert Ops hold their children
                    || !((child.op is CompositeAreaOp_Intersect || child.op is CompositeAreaOp_Union)
                        && op.GetType() != child.op.GetType()); //Intersect and Unions not held unless same as parent
					break;
				case HoldingStyle.Avoid:
					child.heldByParent = !ShouldDrawHolderElement;
					break;
				}
               
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
			foreach (var child in Children)
				total += child.TotalSubTreeWidth ();
			return total;
		}
	}
}

