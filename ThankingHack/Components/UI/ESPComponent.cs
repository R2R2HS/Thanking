﻿using SDG.Unturned;
using System.Collections.Generic;
using Thanking.Misc;
using Thanking.Attributes;
using Thanking.Components.Basic;
using Thanking.Coroutines;
using Thanking.Options.VisualOptions;
using Thanking.Utilities;
using Thanking.Variables;
using UnityEngine;
using System;
using Thanking.Managers.Main;

namespace Thanking.Components.UI
{
	[SpyComponent]
	[Component]
	public class ESPComponent : MonoBehaviour
	{
		public static Material GLMat;
		public static Camera MainCam;
		public static Font ESPFont;

		public void Start() =>
			CoroutineComponent.ESPCoroutine = StartCoroutine(ESPCoroutines.UpdateObjectList());

		public void OnGUI()
		{
			if (Event.current.type != EventType.Repaint || !ESPOptions.Enabled)
				return;

			if (!Provider.isConnected || Provider.isLoading)
				return;
			
			for (int i = 0; i < ESPVariables.Objects.Count; i++)
			{
				ESPObject obj = ESPVariables.Objects[i];
				ESPVisual visual = ESPOptions.VisualOptions[obj.Target];

				Color c = visual.Color.ToColor();
				LabelLocation ll = visual.Location;

				GameObject go = obj.GObject;

				if (go == null)
					continue;

				Vector3 position = go.transform.position;
				float dist = VectorUtilities.GetDistance(position, Player.player.transform.position);

				if (dist > visual.Distance && !visual.InfiniteDistance)
					continue;

				Vector3 cpos = MainCam.WorldToScreenPoint(position);

				if (cpos.z <= 0)
					continue;


				string text = "";
				Bounds b = go.GetComponentInChildren<Renderer>().bounds;

				Debug.Log(obj.Target);
				switch (obj.Target)
				{
					#region Players
					case ESPTarget.Players:
						{
							Player p = (Player)obj.Object;
							text = "<size=" + DrawUtilities.GetTextSize(visual, dist) + ">";
							
							if (ESPOptions.ShowPlayerName)
								text += (p.name + "\n");
							if (ESPOptions.ShowPlayerWeapon)
								text += ((p.equipment.asset != null ? p.equipment.asset.itemName : "Fists") + "\n");
							if (ESPOptions.ShowPlayerDistance)
								text += Mathf.Round(dist);
							
							text += "</size>";
							
							b.size = b.size / 2;
							b.size = new Vector3(b.size.x, b.size.y * 1.25f, b.size.z);

							if (p.quests.isMemberOfSameGroupAs(Player.player) && ESPOptions.UsePlayerGroup)
								c = ESPOptions.SameGroupColor.ToColor();

							break;
						}
					#endregion
					#region Items
					case ESPTarget.Items:
						{
							InteractableItem item = (InteractableItem)obj.Object;

							text = string.Format("<size={2}>{0}\n{1}</size>", item.asset.itemName, Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
					#endregion
					#region Items
					case ESPTarget.Sentries:
						{
							InteractableSentry sentry = (InteractableSentry)obj.Object;

							text = string.Format("<size={3}>{0}\n{1}\n{2}</size>", "Sentry", sentry.displayItem != null ? Assets.find(EAssetType.ITEM, sentry.displayItem.id).name : "<color=#ff0000ff>No Item</color>", Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
					#endregion
					#region Items
					case ESPTarget.Beds:
						{
							InteractableBed bed = (InteractableBed)obj.Object;

							text = string.Format("<size={2}>{0}\n{1}</size>", "Bed", Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
					#endregion
					#region Items
					case ESPTarget.ClaimFlags:
						{
							InteractableClaim flag = (InteractableClaim)obj.Object;

							text = string.Format("<size={2}>{0}\n{1}</size>", "Claim Flag", Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
					#endregion
					#region Items
					case ESPTarget.Vehicles:
						{
							InteractableVehicle vehicle = (InteractableVehicle)obj.Object;

							text = string.Format("<size={3}>{0}\n{1}\n{2}</size>", vehicle.asset.name, vehicle.isLocked ? "<color=#ff0000ff>LOCKED</color>" : "<color=#00ff00ff>UNLOCKED</color>", Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
					#endregion
					#region Items
					case ESPTarget.Storage:
						{
							InteractableStorage stor = (InteractableStorage)obj.Object;

							text = string.Format("<size={2}>{0}\n{1}</size>", "Storage", Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
					#endregion
					#region Items
					case ESPTarget.Generators:
						{
							InteractableGenerator gen = (InteractableGenerator)obj.Object;

							text = string.Format("<size={4}>{0}\n{1}%\n{2}\n{3}</size>", "Generator", gen.fuel / gen.capacity, gen.isPowered ? "<color=#00ff00ff>ON</color>" : "<color=#ff0000ff>OFF</color>", Mathf.Round(dist), DrawUtilities.GetTextSize(visual, dist));
							break;
						}
						#endregion
				}

				Vector3[] vectors = DrawUtilities.GetBoxVectors(b);

				if (visual.Rectangle)
					DrawUtilities.PrepareRectangleLines(MainCam, b, c);
				else
					DrawUtilities.PrepareBoxLines(vectors, c);

				Vector3 LabelVector = DrawUtilities.GetW2SVector(MainCam, b, ll);
				DrawUtilities.DrawLabel(ESPFont, ll, LabelVector, text, Color.black, c, visual.BorderStrength);

				if (visual.LineToObject)
					ESPVariables.DrawBuffer2.Add(new ESPBox2()
					{
						Color = visual.Color.ToColor(),
						Vertices = new Vector2[]
						{
							new Vector2(Screen.width / 2, Screen.height),
							new Vector2(cpos.x, Screen.height - cpos.y)
						}
					});
			}
			
			GLMat.SetPass(0);

			GL.PushMatrix();
			GL.LoadProjectionMatrix(MainCam.projectionMatrix);
			GL.modelview = MainCam.worldToCameraMatrix;
			GL.Begin(GL.LINES);

			for (int i = 0; i < ESPVariables.DrawBuffer.Count; i++)
			{
				ESPBox box = ESPVariables.DrawBuffer[i];

				GL.Color(box.Color);

				Vector3[] vertices = box.Vertices;
				for (int j = 0; j < vertices.Length; j++)
					GL.Vertex(vertices[j]);

			}
			GL.End();
			GL.PopMatrix();

			GL.PushMatrix();
			GL.Begin(GL.LINES);

			for (int i = 0; i < ESPVariables.DrawBuffer2.Count; i++)
			{
				ESPBox2 box = ESPVariables.DrawBuffer2[i];

				GL.Color(box.Color);

				Vector2[] vertices = box.Vertices;
				for (int j = 0; j < vertices.Length; j++)
					GL.Vertex3(vertices[j].x, vertices[j].y, 0);

			}
			GL.End();
			GL.PopMatrix();


			ESPVariables.DrawBuffer.Clear();
			ESPVariables.DrawBuffer2.Clear();
		}
	}
}
