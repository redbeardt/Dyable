using System.IO;
using UnityEngine;

namespace Dyable;

public static class Assets
{
	public static Shader? DyableGameCharShader { get; private set; }

	public static void LoadAssets()
	{
		if(DyableGameCharShader != null){
			return;
		}

		FileInfo fi = new(Assembly.GetExecutingAssembly().Location);
		DirectoryInfo di = fi.Directory;
		AssetBundle asb = AssetBundle.LoadFromFile(
				Path.Combine(di.FullName, "assets"));
		DyableGameCharShader = asb.LoadAsset<Shader>(
				"Assets/Dyable/DyableGameChar.shader");
	}
}