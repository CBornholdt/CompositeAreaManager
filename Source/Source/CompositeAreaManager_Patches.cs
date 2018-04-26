using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Harmony;

namespace CompositeAreaManager
{
	[HarmonyPatch(typeof(RimWorld.Dialog_ManageAreas))]
	[HarmonyPatch("DoWindowContents")]
	static class Dialog_ManageAreas_DoWindowContents
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo listingEnd = AccessTools.Method (typeof(Verse.Listing), "End");
			MethodInfo buttonHelper = AccessTools.Method (typeof(Dialog_ManageAreas_DoWindowContents), "ButtonHelper");
			FieldInfo mapField = AccessTools.Field (typeof(Dialog_ManageAreas), "map");

			List<CodeInstruction> codes = instructions.ToList ();

			for (int i = 0; i < codes.Count; i++) {
				if (codes [i].opcode == OpCodes.Callvirt && codes [i].operand == listingEnd) {
					yield return new CodeInstruction (OpCodes.Ldarg_0);	//Leave Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Dup); //Leave Dialog_ManageAreas, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Ldfld, mapField);	//Consume 1, Leave Map, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Ldloc_0);	//Leave Listing_Standard, Map, Dialog_ManageAreas on stack
					yield return new CodeInstruction (OpCodes.Call, buttonHelper);	//Consume 3
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
}

