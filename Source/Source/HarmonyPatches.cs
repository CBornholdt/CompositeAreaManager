﻿using Harmony;
using Verse;
using System.Reflection;

namespace CompositeAreaManager
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		static HarmonyPatches ()
		{
			HarmonyInstance.DEBUG = true;

			HarmonyInstance harmony = HarmonyInstance.Create ("rimworld.compositeareamanager");

			harmony.PatchAll (Assembly.GetExecutingAssembly ());
		}
	}
}

