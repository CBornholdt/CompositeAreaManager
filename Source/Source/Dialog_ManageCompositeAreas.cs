using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

namespace CompositeAreaManager
{
	public class Dialog_ManageCompositeAreas : Window
	{
		private readonly Map map;

		private readonly float footerHeight = 100;
		private Vector2 scrollPosition = Vector2.zero;
		private float viewHeight = 800;
		public static readonly float cellSpacing = 2f;

		public Map Map {
			get { return this.map; }
		}

		public override Vector2 InitialSize {
			get { return new Vector2 (700, 700); }
		}

		public float LineHeight {
			get { return Text.LineHeight + CellSpacing; }
		}

		public float CellSpacing {
			get { return cellSpacing; }
		}

		public float RowSpacing {
			get { return LineHeight * 0.75f; }
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

		private float DoCompositeAreaRowElement(CompositeArea compositeArea, Rect inRect)
		{
			Rect rect = inRect.ContractedBy (CellSpacing);
			Area area = compositeArea.area;

			CompositeAreaOp_DisplayNode rootNode = CompositeAreaOp_DisplayNode.GenerateFromCompositeArea (compositeArea, this);
			rootNode.ResetToAllHeld ();
			float usedHeight = (float)rootNode.DrawSubTree (rect) * LineHeight;
			rect.yMin += usedHeight;
			Widgets.Label (rect, area.Label);
			usedHeight += LineHeight;

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

		private void DoFooterContents(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard ();
			listing.Begin (inRect);
			listing.ColumnWidth = inRect.width / 2;

			//Must have at least 1 non composite area ...
			CompositeAreaManager cAM = map.GetComponent<CompositeAreaManager> ();
			if (listing.ButtonText ("NewCompositeArea".Translate ()) && cAM.AllCompositeAreas.Count () < cAM.AllCompositableAreas.Count () - 1) {
				List<FloatMenuOption> newAreaList = new List<FloatMenuOption> ();
				foreach (Area area in cAM.AllPotentialNewCompositeAreas)
					newAreaList.Add (new FloatMenuOption ("Compose".Translate () + ": " + area.Label, 
						() => cAM.AllCompositeAreas.Add (new CompositeArea (area))));
				Find.WindowStack.Add (new FloatMenu (newAreaList));
			}
			if (listing.ButtonText ("RemoveCompositeArea".Translate ()) && cAM.AllCompositeAreas.Any()) {
				List<FloatMenuOption> newAreaList = new List<FloatMenuOption> ();
				foreach (var compositeArea in cAM.AllCompositeAreas)
					newAreaList.Add (new FloatMenuOption ("Remove".Translate () + ": " + compositeArea.area.Label, 
						() => cAM.AllCompositeAreas.Remove (compositeArea)));
				Find.WindowStack.Add (new FloatMenu (newAreaList));
			}
			if (listing.ButtonText ("ManageAreas".Translate ()))
				Find.WindowStack.Add (new Dialog_ManageAreas (map));

			listing.End ();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect scrollRect = new Rect (inRect.xMin + 2, inRect.yMin + 2, inRect.width - 4, inRect.height - footerHeight - 4);
			Rect scrollViewRect = new Rect (0, 0, scrollRect.width - 2 * GUI.skin.verticalScrollbar.fixedWidth, this.viewHeight);
			Widgets.BeginScrollView (scrollRect, ref this.scrollPosition, scrollViewRect, true);

			CompositeAreaManager cAM = map.GetComponent<CompositeAreaManager> ();
			Rect drawRect = new Rect (scrollViewRect);
			foreach (var compositeArea in cAM.AllCompositeAreas) {
				drawRect.yMin += DoCompositeAreaRowElement (compositeArea, drawRect) + RowSpacing;
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


		//Node for combined operations ... really terrible name I know

	}
}

