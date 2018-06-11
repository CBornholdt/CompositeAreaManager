using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Verse
{
    static class Listing_Standard_Ext
    {
		static public void EnumButtonWithTooltip<T>(this Listing_Standard listing, string label, Func<T> valueGetter
                            , Action<T> valueSetter, string enumTranslationPrefix, string tooltip = null) where T : struct, IConvertible
		{
			Rect rect = listing.GetRect(30f);
			Widgets.Label(rect.LeftHalf(), label);
			FloatMenuButtonOverEnum<T>(rect.RightHalf(), (enumTranslationPrefix + Enum.GetName(typeof(T), valueGetter())).Translate()
                    , v => valueSetter(v), enumTranslationPrefix);
			if(!tooltip.NullOrEmpty()) {
				if(Mouse.IsOver(rect))
					Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, tooltip);
			}
			listing.Gap(listing.verticalSpacing);  
		}
        
        public static void FloatMenuButtonOverEnum<T>(Rect rect, string buttonLabel, Action<T> selectedAction, string enumTranslationPrefix 
            , Func<T, bool> displayPredicate = null, bool drawBackground = true, bool isActive = true) where T : struct, IConvertible
        {
            if(Widgets.ButtonText(rect, buttonLabel, drawBackground, false, isActive)) {
                List<FloatMenuOption> list = new List<FloatMenuOption> ();
                foreach(T value in Enum.GetValues(typeof(T)).Cast<T>().Where(v => displayPredicate?.Invoke(v) ?? true))
                    list.Add(new FloatMenuOption((enumTranslationPrefix + Enum.GetName(typeof(T), value)).Translate()
                        , () => selectedAction(value), MenuOptionPriority.Default, null, null, 0, null, null));
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
    }
}
