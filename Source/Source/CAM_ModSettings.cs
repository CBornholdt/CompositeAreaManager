using System;
using Verse;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace CompositeAreaManager
{
	public enum HoldingStyle { Aggressively, Smart, Normal, Avoid };

	public class CAM_ModSettings : ModSettings
	{
		public HoldingStyle holdingStyle = HoldingStyle.Normal;
		public bool displayDetailedOpNames = true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.displayDetailedOpNames, "DisplayDetailedOpNames", true);
			Scribe_Values.Look<HoldingStyle>(ref this.holdingStyle, "HoldingStyle", HoldingStyle.Normal);
		}
	}

	public class CAM_Mod : Mod
	{
		static public CAM_ModSettings settings;

		public CAM_Mod(ModContentPack contentPack) : base(contentPack)
		{
			settings = GetSettings<CAM_ModSettings>();
		}

		public override string SettingsCategory() => "CompositeAreaManager.Settings.CategoryLabel".Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			var listing = new Listing_Standard();
			listing.Begin(inRect);
			listing.ColumnWidth = inRect.width / 2;
			listing.CheckboxLabeled("CAM_DetailedOpNames.Label".Translate(), ref settings.displayDetailedOpNames
					, "CAM_DetailedOpNames.Desc".Translate());
			listing.NewColumn();
			listing.EnumButtonWithTooltip<HoldingStyle>("CAM_HoldingStyle.Label".Translate(), () => settings.holdingStyle
					, value => settings.holdingStyle = value,  "CAM_HoldingStyle_" , "CAM_HoldingStyle.Desc".Translate());
			listing.End();
		}
	}
}
