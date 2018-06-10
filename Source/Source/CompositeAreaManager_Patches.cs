using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;

namespace CompositeAreaManager
{
	[HarmonyPatch(typeof(RimWorld.Dialog_ManageAreas))]
	[HarmonyPatch("DoWindowContents")]
	[HarmonyBefore("rimworld.cbornholdt.workareaprioritymanager")]
	static class Dialog_ManageAreas_DoWindowContents
	{
		private static Rect inRectHolder;
		private static Vector2 scrollViewPosition;
		public static readonly float footerHeight = 150;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//	MethodInfo listingEnd = AccessTools.Method (typeof(Verse.Listing), "End");
			MethodInfo getCount = AccessTools.Method (typeof(List<Area>), "get_Count");
			MethodInfo buttonHelper = AccessTools.Method (typeof(Dialog_ManageAreas_DoWindowContents), "ButtonHelper");
			MethodInfo scrollViewHelperBegin = AccessTools.Method (typeof(Dialog_ManageAreas_DoWindowContents), "ScrollViewHelperBegin");
			MethodInfo scrollViewHelperEnd = AccessTools.Method (typeof(Dialog_ManageAreas_DoWindowContents), "ScrollViewHelperEnd");
			MethodInfo listingEnd = AccessTools.Method (typeof(Listing), "End");

			List<CodeInstruction> codes = instructions.ToList ();

			yield return new CodeInstruction (OpCodes.Ldarg_1);	//Load Rect onto stack
			yield return new CodeInstruction (OpCodes.Call, scrollViewHelperBegin);	//Consume 1, leave 1
			yield return new CodeInstruction (OpCodes.Starg_S, 1);	//Consume 1

			bool closedScrollView = false;
			bool placedButton = false;

			for (int i = 0; i < codes.Count; i++) {
				if (!closedScrollView && i > 1 && codes [i - 2].opcode == OpCodes.Callvirt && codes [i - 2].operand == getCount) {
					yield return codes [i];	//End loop, place Listing_Standard on stack
					yield return new CodeInstruction (OpCodes.Dup); //Listing_Standard on stack
					yield return new CodeInstruction (OpCodes.Call, scrollViewHelperEnd); //Consume 1, leave Rect
					yield return new CodeInstruction (OpCodes.Starg_S, 1);	//Consume Rect
					closedScrollView = true;
					continue;
				}	
				if (closedScrollView && !placedButton 
					&& codes [i].opcode == OpCodes.Callvirt && codes [i].operand == listingEnd) {
					yield return new CodeInstruction(OpCodes.Dup);	//duplicate argument to listingEnd, Listing_Standard on stack
					yield return new CodeInstruction (OpCodes.Ldarg_0);	//Leave Listing_standard, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Call, buttonHelper);	//Consume 2
					placedButton = true;
				}
				yield return codes [i];
			}
		}

		static void ButtonHelper(Listing_Standard listing, Dialog_ManageAreas dialog) {
			FieldInfo mapField = AccessTools.Field (typeof(Dialog_ManageAreas), "map");
			Map map = (Map)mapField.GetValue(dialog);
			listing.NewColumn ();
			if (listing.ButtonText ("CAM_ManageCompositeAreas".Translate (), null)) {
				map.GetComponent<CompositeAreaManager> ().LaunchDialog_ManageCompositeAreas ();
				dialog.Close (false);
			}
		}

		static Rect ScrollViewHelperBegin(Rect inRect)
		{
			inRectHolder = inRect;
			Rect scrollRect = new Rect (inRect.xMin, inRect.yMin, inRect.width, inRect.height - footerHeight);
			Rect scrollViewRect = new Rect (0, 0, inRect.width - 2 * GUI.skin.verticalScrollbar.fixedWidth, 1000);
			Widgets.BeginScrollView (scrollRect, ref scrollViewPosition, scrollViewRect, true);
			return scrollViewRect;
		}

		static Rect ScrollViewHelperEnd(Listing_Standard listing)
		{
			listing.End ();
			Widgets.EndScrollView ();
			Rect footerRect = new Rect (inRectHolder);
			footerRect.yMin = footerRect.yMax - footerHeight;
			listing.Begin (footerRect);
			listing.ColumnWidth = footerRect.width / 2;
			return inRectHolder;
		}
	}

	[HarmonyPatch(typeof(Verse.AreaUtility))]
	[HarmonyPatch("MakeAllowedAreaListFloatMenu")]
	static class AreaUtility_MakeAllowedAreaListFloatMenu
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Log.Message("Patching!!");
			MethodInfo getWindowStack = AccessTools.Method (typeof(Verse.Find), "get_WindowStack");
			MethodInfo listAddHelper = AccessTools.Method (typeof(AreaUtility_MakeAllowedAreaListFloatMenu), "ListAddHelper");

			List<CodeInstruction> codes = instructions.ToList ();
			for(int i = 0; i < codes.Count; i++){
				if (codes [i].opcode == OpCodes.Call && codes [i].operand == getWindowStack) {
					yield return new CodeInstruction (OpCodes.Ldarg_S, 4);	//Map on stack
					yield return new CodeInstruction (OpCodes.Ldloc_1);	//Leave List<FloatMenuOption>, Map on stack
					yield return new CodeInstruction (OpCodes.Call, listAddHelper);	//Consume 1
				}
				yield return codes[i];
			}
		}

		static void ListAddHelper(Map map, List<FloatMenuOption> list)
		{
			list.Add (new FloatMenuOption ("CAM_ManageCompositeAreas".Translate (), 
				() => map.GetComponent<CompositeAreaManager> ().LaunchDialog_ManageCompositeAreas (), 
				MenuOptionPriority.Low, null, null, 0, null, null));
		}
	}

	[HarmonyPatch(typeof(Verse.AreaManager))]
	[HarmonyPatch("CanMakeNewAllowed")]
	static class AreaManager_CanMakeNewAllowed
	{
		public static readonly int maxAreasPerCategory = 12;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var code in instructions)
				if (code.opcode == OpCodes.Ldc_I4_5)
					yield return new CodeInstruction (OpCodes.Ldc_I4, maxAreasPerCategory);
				else
					yield return code;
		}
	}
}

