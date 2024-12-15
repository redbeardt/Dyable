using System.Collections;
using System.Diagnostics;
using UnityEngine;
using static SDCSUtils;

namespace Dyable;

public static class Patches
{
	static readonly int PropIdColor     = Shader.PropertyToID("_Color");
	static readonly int PropIdInvColor  = Shader.PropertyToID("_InversionColor");
	static readonly int PropIdMaskRange = Shader.PropertyToID("_MaskRange");
	static readonly int PropIdFuzziness = Shader.PropertyToID("_MaskFuzziness");

	// Used by Game/Character
	static readonly int PropIdAlbedo    = Shader.PropertyToID("_Albedo");
	static readonly int PropIdNormal    = Shader.PropertyToID("_Normal");
	static readonly int PropIdRMOE      = Shader.PropertyToID("_RMOE");

	// Used by Game/CharacterCloth
	static readonly int PropIdAlbedo1 = Shader.PropertyToID("_Albedo1");
	static readonly int PropIdNormal1 = Shader.PropertyToID("_Normal1");
	static readonly int PropIdRMO     = Shader.PropertyToID("_RMO");

	// Used by Game/CharacterPlayerOutfit
	static readonly int PropIdAlbedo2 = Shader.PropertyToID("_Albedo2");
	static readonly int PropIdNormal2 = Shader.PropertyToID("_Normal2");
	static readonly int PropIdRMOT    = Shader.PropertyToID("_RMOT");

	static IEnumerator ApplyDyes(
			EntityPlayer equippedPlayer,
			Transform targetTransform,
			bool waitOneFrame = true)
	{
		if(waitOneFrame){
			yield return new WaitForEndOfFrame();
		}

		LogDbg($"Applying dyes for player with entity id {equippedPlayer.entityId}...");

		int slotCount = equippedPlayer.equipment.GetSlotCount();
		string? headColour = null;
		string? bodyColour = null;
		string? handsColour = null;
		string? feetColour = null;

		for(int i = 0; i < slotCount; i++){
			ItemValue iv = equippedPlayer.equipment.GetSlotItem(i);

			if(iv?.ItemClass?.SDCSData == null){
				continue;
			}

			ItemClassArmor armour = (ItemClassArmor)iv.ItemClass;
			SlotData data = iv.ItemClass.SDCSData;
			LogDbg($"Found armour item for slot {armour.EquipSlot}.");
			
			switch(armour.EquipSlot){
				case EquipmentSlots.Head:
					headColour = iv.GetPropertyOverride("TintColor", "0,0,0");
					break;
				case EquipmentSlots.Chest:
					bodyColour = iv.GetPropertyOverride("TintColor", "0,0,0");
					break;
				case EquipmentSlots.Hands:
					handsColour = iv.GetPropertyOverride("TintColor", "0,0,0");
					break;
				case EquipmentSlots.Feet:
					feetColour = iv.GetPropertyOverride("TintColor", "0,0,0");
					break;
				default:
					break;
			};
		}

		var smrs = targetTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

		foreach(SkinnedMeshRenderer smr in smrs){
			if(smr == null || smr.material == null){
				continue;
			}

			Material mat = smr.material;

			if(mat.shader.name == "Game/SDCS/Hair"){
				LogDbg("Hair encounterd. Skipping...");
				continue;
			}

			if(mat.shader.name == "Game/SDCS/Skin"){
				LogDbg("Skin encounterd. Skipping...");
				continue;
			}

			GameObject go = smr.gameObject;
			string name = smr.name;
			string matName = mat.name;
			string? colour = null;

			if(matName.ContainsCaseInsensitive("Body") && bodyColour is not null){
				LogDbg("Selecting BODY colour...");
				colour = bodyColour;
			} else if((matName.ContainsCaseInsensitive("head")
						|| matName.ContainsCaseInsensitive("helmet"))
					&& headColour is not null){
				LogDbg("Selecting HEAD colour...");
				colour = headColour;
			} else if(matName.ContainsCaseInsensitive("hands") && handsColour is not null){
				LogDbg("Selecting HANDS colour...");
				colour = handsColour;
			} else if(matName.Contains("feet") && feetColour is not null){
				LogDbg("Selecting FEET colour...");
				colour = feetColour;
			}

			ApplyColourProps(mat, colour);
		}
	}

	static void ApplyColourProps(Material mat, string? colourText)
	{
		if(colourText is null){
			LogDbg("No colour was selected so colour application was aborted.");
			return;
		}

		Color32 c = StringParsers.ParseColor32(colourText);

		if(mat.shader.name is "Game/CharacterPlayerOutfit"){
			LogDbg("Applying colour for shader Game/CharacterPlayerOutfit...");
			SetPlayerOutfitProps(mat, c);
			return;
		}
	
		if(mat.shader.name is "Game/Character"){
			LogDbg("Applying colour for shader Game/Character...");
			SetCharacterProps(mat, c);
			return;
		}

		if(mat.shader.name is "Game/CharacterCloth"){
			LogDbg("Applying colour for shader Game/CharacterCloth...");
			SetClothProps(mat, c);
			return;
		}
	}

	static void SetPlayerOutfitProps(Material mat, Color32 c)
	{
		Texture? albedo = mat.GetTexture(PropIdAlbedo2);
		Texture? normal = mat.GetTexture(PropIdNormal2);
		Texture? rmoe = mat.GetTexture(PropIdRMOT);

		mat.shader = Assets.DyableGameCharShader;
		mat.SetTexture(PropIdAlbedo, albedo);
		mat.SetTexture(PropIdNormal, normal);
		mat.SetTexture(PropIdRMOE, rmoe);
		mat.SetFloat(PropIdMaskRange, 1f);
		mat.SetFloat(PropIdFuzziness, 1f);
		mat.SetColor(PropIdColor, c);
	}

	static void SetCharacterProps(Material mat, Color32 c)
	{
		Texture? albedo = mat.GetTexture(PropIdAlbedo);
		Texture? normal = mat.GetTexture(PropIdNormal);
		Texture? rmoe = mat.GetTexture(PropIdRMOE);

		mat.shader = Assets.DyableGameCharShader;
		mat.SetTexture(PropIdAlbedo, albedo);
		mat.SetTexture(PropIdNormal, normal);
		mat.SetTexture(PropIdRMOE, rmoe);
		mat.SetFloat(PropIdMaskRange, 1f);
		mat.SetFloat(PropIdFuzziness, 1f);
		mat.SetColor(PropIdColor, c);
	}

	static void SetClothProps(Material mat, Color32 c)
	{
		Texture albedo = mat.GetTexture(PropIdAlbedo1);
		Texture normal = mat.GetTexture(PropIdNormal1);
		Texture rmo    = mat.GetTexture(PropIdRMO);

		mat.shader = Assets.DyableGameCharShader;
		mat.SetTexture(PropIdAlbedo, albedo);
		mat.SetTexture(PropIdNormal, normal);
		mat.SetTexture(PropIdRMOE, rmo);
		mat.SetFloat(PropIdMaskRange, 1f);
		mat.SetFloat(PropIdFuzziness, 1f);
		mat.SetColor(PropIdColor, c);
	}

	[Conditional("DEBUG")]
	static void LogDbg(string str)
	{
		Log.Out($"[DYABLE] {str}");
	}

	[HarmonyPatch]
	static class PatchesEModelSDCS
	{
		static MethodBase TargetMethod() =>
			typeof(EModelSDCS).GetMethod(nameof(EModelSDCS.generateMeshes));

		static void Postfix(EModelSDCS __instance)
		{
			GameManager.Instance.StartCoroutine(ApplyDyes(
					(EntityPlayer)__instance.entity,
					__instance.entity.emodel.transform));
		}
	}

	[HarmonyPatch]
	static class PatchesXUiC_CharacterFrameWindow
	{
		static MethodBase TargetMethod() =>
			typeof(XUiC_CharacterFrameWindow).GetMethod(
					nameof(XUiC_CharacterFrameWindow.MakePreview));

		static void Postfix(XUiC_CharacterFrameWindow __instance)
		{
			LogDbg("PatchesSDCSUtils.CreateVizUI.Postfix");

			if(GameManager.Instance.World.GetPrimaryPlayer() is not { } epl){
				return;
			}

			Transform targetT = __instance.previewSDCSObj.transform;

			GameManager.Instance.StartCoroutine(ApplyDyes(epl, targetT));
		}
	}
}