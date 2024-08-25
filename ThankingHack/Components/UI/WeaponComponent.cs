using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Thanking.Attributes;
using Thanking.Coroutines;
using Thanking.Misc.Enums;
using Thanking.Options.AimOptions;
using Thanking.Utilities;
using Thanking.Variables;
using Thanking.Variables.UIVariables;
using UnityEngine;

namespace Thanking.Components.UI
{
	[Component]
	[SpyComponent]
	public class WeaponComponent : MonoBehaviour
	{
		public static Dictionary<ushort, float[]> AssetBackups = [];
        public static List<TracerLine> Tracers = [];
		public static Camera MainCamera;
		
		public static FieldInfo AmmoInfo = typeof(UseableGun).GetField("ammo", BindingFlags.NonPublic | BindingFlags.Instance);
		
		public static MethodInfo UpdateCrosshair  = typeof(UseableGun).GetMethod("updateCrosshair", BindingFlags.NonPublic | BindingFlags.Instance);

		public static byte Ammo() => 
			(byte)AmmoInfo.GetValue(Player.player.equipment.useable);

		[Initializer]
		public static void Initialize()
		{
			ColorUtilities.addColor(new ColorVariable("_BulletTracersHitColor", "Weapons - Bullet Tracers (Hit)", new Color32(255, 0, 0, 255)));
			ColorUtilities.addColor(new ColorVariable("_BulletTracersColor", "Weapons - Bullet Tracers", new Color32(255, 255, 255, 255)));
			ColorUtilities.addColor(new ColorVariable("_WeaponInfoColor", "Weapons - Information", new Color32(0, 255, 0, 255)));
			ColorUtilities.addColor(new ColorVariable("_WeaponInfoBorder", "Weapons - Information (Border)", new Color32(0, 0, 0, 255)));
		}
		
		public void Start()
		{	
			StartCoroutine(UpdateWeapon());
		}

		public void OnGUI()
		{
			if (MainCamera == null)
				MainCamera = Camera.main;

			if (WeaponOptions.NoSway)
				if (Player.player != null && Player.player.animator != null)
					Player.player.animator.scopeSway = Vector3.zero;

			if (Event.current.type != EventType.Repaint)
				return;

			if (!DrawUtilities.ShouldRun())
				return;

			if (WeaponOptions.Tracers)
			{
				ESPComponent.GLMat.SetPass(0);

				GL.PushMatrix();
				GL.LoadProjectionMatrix(MainCamera.projectionMatrix);
				GL.modelview = MainCamera.worldToCameraMatrix;
				GL.Begin(GL.LINES);
				
				for (int i = Tracers.Count - 1; i > -1; i--)
				{
					TracerLine t = Tracers[i];
					if (DateTime.Now - t.CreationTime > TimeSpan.FromSeconds(5))
					{
						Tracers.Remove(t);
						continue;
					}

					GL.Color(t.Hit
						? ColorUtilities.getColor("_BulletTracersHitColor")
						: ColorUtilities.getColor("_BulletTracersColor"));
					
					GL.Vertex(t.StartPosition);
					GL.Vertex(t.EndPosition);
				}

				GL.End();
				GL.PopMatrix();
			}
			if (WeaponOptions.ShowWeaponInfo)
			{
				if (!(Player.player.equipment.asset is ItemGunAsset))
					return;	

				GUI.depth = 0;
				ItemGunAsset PAsset = (ItemGunAsset) Player.player.equipment.asset;
				string text = $"<size=15>{PAsset.itemName}\nRange: {PAsset.range}</size>";

				DrawUtilities.DrawLabel(ESPComponent.ESPFont, LabelLocation.MiddleLeft,
					new Vector2(Screen.width - 20, Screen.height / 2), text,  ColorUtilities.getColor("_WeaponInfoColor"), ColorUtilities.getColor("_WeaponInfoBorder"), 1);
			}
		}

		public static IEnumerator UpdateWeapon()
		{
			while (true)
			{
				yield return new WaitForSeconds(0.1f);
				if (!DrawUtilities.ShouldRun())
					continue;

				if (!(Player.player.equipment.asset is ItemGunAsset PAsset))
					continue;
			
				if (!AssetBackups.ContainsKey(PAsset.id))
				{
					float[] Backups =
					[
						PAsset.aimingRecoilMultiplier,
						PAsset.recoilMax_x,
						PAsset.recoilMax_y,
						PAsset.recoilMin_x,
						PAsset.recoilMin_y,
						PAsset.spreadAim,
						PAsset.baseSpreadAngleRadians
					];
				
					Backups[6] = PAsset.baseSpreadAngleRadians;

					AssetBackups.Add(PAsset.id, Backups);
				}

				if (WeaponOptions.NoRecoil && !PlayerCoroutines.IsSpying)
				{
					PAsset.aimingRecoilMultiplier = 0;
					PAsset.recoilMax_x = 0;
					PAsset.recoilMax_y = 0;
					PAsset.recoilMin_x = 0;
					PAsset.recoilMin_y = 0;
				}
				else
				{
					PAsset.aimingRecoilMultiplier = AssetBackups[PAsset.id][0];
					PAsset.recoilMax_x = AssetBackups[PAsset.id][1];
					PAsset.recoilMax_y = AssetBackups[PAsset.id][2];
					PAsset.recoilMin_x = AssetBackups[PAsset.id][3];
					PAsset.recoilMin_y = AssetBackups[PAsset.id][4];
				}

				if (WeaponOptions.NoSpread && !PlayerCoroutines.IsSpying)
				{
					PAsset.spreadAim = 0;
					PAsset.baseSpreadAngleRadians = 0;

					PlayerUI.updateScope(false);
				}
				else
				{
					PAsset.spreadAim = AssetBackups[PAsset.id][5];
					PAsset.baseSpreadAngleRadians = AssetBackups[PAsset.id][6];

					UpdateCrosshair.Invoke(Player.player.equipment.useable, null);
				}
			
				Reload();
			}
		}

		public static void Reload()
		{
#if DEBUG
			DebugUtilities.Log($"Ammo: {Ammo()}");
#endif
			
			if (!WeaponOptions.AutoReload || Ammo() > 0) return;
			
#if DEBUG
			DebugUtilities.Log("Ammo less than or equal to 0");
#endif
			
			IEnumerable<InventorySearch> magazineSearch = 
				Player.player.inventory.search(EItemType.MAGAZINE,
					((ItemGunAsset) Player.player.equipment.asset).magazineCalibers, false)
					.Where(i => i.jar.item.amount > 0);

			var inventorySearches = magazineSearch.ToList();
			if (inventorySearches.Count == 0)
				return;
			
			InventorySearch search = inventorySearches
					.OrderByDescending(i => i.jar.item.amount)
					.First();

			#if DEBUG
			DebugUtilities.Log("Magazine reloaded");
				#endif

			Player.player.channel.send("askAttachMagazine", ESteamCall.SERVER,
				ESteamPacket.UPDATE_UNRELIABLE_BUFFER, search.page, search.jar.x,
				search.jar.y);
		}
	}
}
