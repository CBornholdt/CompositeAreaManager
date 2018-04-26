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
	static class Dialog_ManageAreas_DoWindowContents
	{
		private static Vector2 scrollViewPosition;
		public static readonly float footerHeight = 150;
		private static Rect inRectHolder;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
		//	MethodInfo listingEnd = AccessTools.Method (typeof(Verse.Listing), "End");
			MethodInfo getCount = AccessTools.Method (typeof(List<Area>), "get_Count");
			MethodInfo buttonHelper = AccessTools.Method (typeof(Dialog_ManageAreas_DoWindowContents), "ButtonHelper");
			MethodInfo scrollViewHelperBegin = AccessTools.Method(typeof(Dialog_ManageAreas_DoWindowContents), "ScrollViewHelperBegin");
			MethodInfo scrollViewHelperEnd = AccessTools.Method(typeof(Dialog_ManageAreas_DoWindowContents), "ScrollViewHelperEnd");
			FieldInfo mapField = AccessTools.Field (typeof(Dialog_ManageAreas), "map");

			List<CodeInstruction> codes = instructions.ToList ();

			yield return new CodeInstruction (OpCodes.Ldarg_1);	//Load Rect onto stack
			yield return new CodeInstruction (OpCodes.Call, scrollViewHelperBegin);	//Consume 0
			yield return new CodeInstruction (OpCodes.Starg_S, 1);	//Consume 1

			for (int i = 0; i < codes.Count; i++) {
				if(i > 1 && codes[i-2].opcode == OpCodes.Callvirt && codes[i-2].operand == getCount) {
					yield return codes [i];	//End loop, place Listing_Standard on stack
					yield return new CodeInstruction (OpCodes.Dup);
					yield return new CodeInstruction (OpCodes.Call, scrollViewHelperEnd); //Consume listing
					yield return codes [i + 1];
					yield return codes [i + 2];	//get_width()
					yield return codes [i + 3];
					yield return codes [i + 4];
					yield return codes [i + 5]; //set_columnWidth
					yield return new CodeInstruction (OpCodes.Ldarg_0);	//Leave Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Dup); //Leave Dialog_ManageAreas, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Ldfld, mapField);	//Consume 1, Leave Map, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Ldloc_0);	//Leave Listing_Standard, Map, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Call, buttonHelper);	//Consume 3

					i += 5;
					continue;
				}	
				yield return codes [i];
			}
		}

		static void ButtonHelper(Dialog_ManageAreas dialog, Map map, Listing_Standard listing) {
			if (listing.ButtonText ("ManageCompositeAreas".Translate (), null)) {
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

		static void ScrollViewHelperEnd(Listing_Standard listing)
		{
			listing.End ();
			Widgets.EndScrollView ();
			Rect footerRect = new Rect (inRectHolder.xMin, inRectHolder.yMax - footerHeight, inRectHolder.width, footerHeight);
			listing.Begin (footerRect);
		}
	}

	[HarmonyPatch(typeof(Verse.AreaUtility))]
	[HarmonyPatch("MakeAllowedAreaListFloatMenu")]
	static class AreaUtility_MakeAllowedAreaListFloatMenu
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
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
			list.Add (new FloatMenuOption ("ManageCompositeAreas".Translate (), 
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

