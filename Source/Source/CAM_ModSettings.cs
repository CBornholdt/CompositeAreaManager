using System;
using Verse;
using HugsLib;
using HugsLib.Settings;
using System.Collections.Generic;
namespace CompositeAreaManager
{
	public enum HoldingStyle { Aggressively, Smart, Normal, Avoid };

    public class CAM_ModSettings : ModBase
    {
		public override string ModIdentifier => "CompositeAreaManager";

		static public SettingHandle<HoldingStyle> holdingStyle;

		static public SettingHandle<bool> displayDetailedOperationNames;

		public override void DefsLoaded()
		{
			holdingStyle = Settings.GetHandle<HoldingStyle>("holdingStyle"
								, "CAM_HoldingStyle.Label".Translate(), "CAM_HoldingStyle.Desc".Translate()
								, HoldingStyle.Normal, null, "CAM_HoldingStyle_");
			displayDetailedOperationNames = Settings.GetHandle<bool>("displayDetailedOperationNames"
								, "CAM_DisplayDetailedOperationNames.Label".Translate()
								, "CAM_DisplayDetailedOperationNames.Desc".Translate()
								, true);                                
		}

		protected override bool HarmonyAutoPatch => false;
	}
}
