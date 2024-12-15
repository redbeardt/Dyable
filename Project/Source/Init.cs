using static HarmonyLib.Tools.Logger;

namespace Dyable;

public class Init : IModApi
{
	public static Mod? ModInstance { get; set; }
	public static Harmony? HarmonyInstance { get; set; }

	public void InitMod(Mod _modInstance)
	{
		ModInstance = _modInstance;
		HarmonyInstance = new("dyable." + _modInstance.VersionString);

		ChannelFilter = LogChannel.None;
#if DEBUG
		ChannelFilter = LogChannel.Warn | LogChannel.Error | LogChannel.Debug;
#endif

		Dyable.Assets.LoadAssets();
		HarmonyInstance.PatchAll(typeof(Init).Assembly);
	}
}