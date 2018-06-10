using Harmony;
using Verse;
using System.Reflection;

namespace CompositeAreaManager
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		static HarmonyPatches ()
		{
			HarmonyInstance harmony = HarmonyInstance.Create ("rimworld.cbornholdt.compositeareamanager");

			harmony.PatchAll (Assembly.GetExecutingAssembly ());
		}
	}
}

