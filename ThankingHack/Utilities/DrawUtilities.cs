﻿using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thanking.Variables;
using UnityEngine;

namespace Thanking.Utilities
{
	public static class DrawUtilities
	{
		public static void PrepareRectangleLines(Camera cam, Bounds b, Color c)
		{
			Vector3[] pts = new Vector3[8];
			pts[0] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
			pts[1] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
			pts[2] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
			pts[3] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
			pts[4] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
			pts[5] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
			pts[6] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
			pts[7] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
			
			//Get them in GUI space
			for (int i = 0; i < pts.Length; i++)
				pts[i].y = Screen.height - pts[i].y;

			//Calculate the min and max positions
			Vector3 min = pts[0];
			Vector3 max = pts[0];
			for (int i = 1; i < pts.Length; i++)
			{
				min = Vector3.Min(min, pts[i]);
				max = Vector3.Max(max, pts[i]);
			}

			Vector2[] vectors = new Vector2[4];
			vectors[0] = new Vector2(min.x, min.y);
			vectors[1] = new Vector2(max.x, min.y);
			vectors[2] = new Vector2(min.x, max.y);
			vectors[3] = new Vector2(max.x, max.y);

			PrepareRectangleLines(vectors, c);
		}

		public static Bounds GetBoundsRecursively(GameObject go)
		{
			Bounds b = new Bounds();
			MeshRenderer[] mf = go.GetComponentsInChildren<MeshRenderer>();

			for (int i = 0; i < mf.Length; i++)
				b.Encapsulate(mf[i].bounds);

			return b;
		}

		public static Bounds TransformBounds(Transform _transform, Bounds _localBounds)
		{
			var center = _transform.TransformPoint(_localBounds.center);

			// transform the local extents' axes
			var extents = _localBounds.extents;
			var axisX = _transform.TransformVector(extents.x, 0, 0);
			var axisY = _transform.TransformVector(0, extents.y, 0);
			var axisZ = _transform.TransformVector(0, 0, extents.z);

			// sum their absolute value to get the world extents
			extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
			extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
			extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

			return new Bounds { center = center, extents = extents };
		}

		public static void DrawTextWithOutline(Rect centerRect, string text, GUIStyle style, Color borderColor, Color innerColor, int borderWidth)
		{
			// assign the border color
			style.normal.textColor = borderColor;

			// draw an outline color copy to the left and up from original
			Rect modRect = centerRect;
			modRect.x -= borderWidth;
			modRect.y -= borderWidth;
			GUI.Label(modRect, text, style);


			// stamp copies from the top left corner to the top right corner
			while (modRect.x <= centerRect.x + borderWidth)
			{
				modRect.x++;
				GUI.Label(modRect, text, style);
			}

			// stamp copies from the top right corner to the bottom right corner
			while (modRect.y <= centerRect.y + borderWidth)
			{
				modRect.y++;
				GUI.Label(modRect, text, style);
			}

			// stamp copies from the bottom right corner to the bottom left corner
			while (modRect.x >= centerRect.x - borderWidth)
			{
				modRect.x--;
				GUI.Label(modRect, text, style);
			}

			// stamp copies from the bottom left corner to the top left corner
			while (modRect.y >= centerRect.y - borderWidth)
			{
				modRect.y--;
				GUI.Label(modRect, text, style);
			}

			// draw the inner color version in the center
			style.normal.textColor = innerColor;
			GUI.Label(centerRect, text, style);
		}

		public static Vector2 InvertScreenSpace(Vector2 dim) =>
			new Vector2(dim.x, Screen.height - dim.y);

		public static string ColorToHex(Color32 color)
		{
			string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
			return hex;
		}

		public static void DrawLabel(LabelLocation location, Vector2 W2SVector, string content, Color bColor, Color iColor, int bWidth)
		{
			GUIContent gcontent = new GUIContent(content);
			GUIStyle LabelStyle = new GUIStyle();

			LabelStyle.font = AssetVariables.Roboto;
			LabelStyle.fontSize = 12;

			Vector2 dim = LabelStyle.CalcSize(gcontent);
			float width = dim.x;
			float height = dim.y;
			Rect rect = new Rect(0, 0, width, height);

			switch (location)
			{
				case LabelLocation.BottomLeft:
					rect.x = W2SVector.x - width;
					rect.y = W2SVector.y - height;
					LabelStyle.alignment = TextAnchor.LowerRight;
					break;
				case LabelLocation.BottomMiddle:
					rect.x = W2SVector.x - width / 2;
					rect.y = W2SVector.y;
					LabelStyle.alignment = TextAnchor.UpperCenter;
					break;
				case LabelLocation.BottomRight:
					rect.x = W2SVector.x;
					rect.y = W2SVector.y - height;
					LabelStyle.alignment = TextAnchor.LowerLeft;
					break;
				case LabelLocation.Center:
					rect.x = W2SVector.x - width / 2;
					rect.y = W2SVector.y - height / 2;
					LabelStyle.alignment = TextAnchor.MiddleCenter;
					break;
				case LabelLocation.MiddleLeft:
					rect.x = W2SVector.x - width;
					rect.y = W2SVector.y - height / 2;
					LabelStyle.alignment = TextAnchor.MiddleRight;
					break;
				case LabelLocation.MiddleRight:
					rect.x = W2SVector.x;
					rect.y = W2SVector.y - height / 2;
					LabelStyle.alignment = TextAnchor.MiddleLeft;
					break;
				case LabelLocation.TopLeft:
					rect.x = W2SVector.x - width;
					rect.y = W2SVector.y;
					LabelStyle.alignment = TextAnchor.UpperRight;
					break;
				case LabelLocation.TopMiddle:
					rect.x = W2SVector.x - width / 2;
					rect.y = W2SVector.y - height;
					LabelStyle.alignment = TextAnchor.LowerCenter;
					break;
				case LabelLocation.TopRight:
					rect.x = W2SVector.x;
					rect.y = W2SVector.y;
					LabelStyle.alignment = TextAnchor.UpperLeft;
					break;
			}

			if (rect.x - 10 < 0 || rect.y - 10 < 0)
				return;

			if (rect.x + 10 > Screen.width || rect.y + 10 > Screen.height)
				return;

			DrawTextWithOutline(rect, gcontent.text, LabelStyle, bColor, iColor, bWidth);
		}

		public static Vector2 GetW2SVector(Camera cam, Bounds b, Vector3[] vectors, LabelLocation location)
		{
			Vector2 vec = Vector2.zero;
			switch (location)
			{
				case LabelLocation.BottomLeft:
					vec = cam.WorldToScreenPoint(vectors[2]);
					break;
				case LabelLocation.BottomMiddle:
					vec = cam.WorldToScreenPoint(new Vector3(b.center.x, vectors[3].y, vectors[3].z));
					break;
				case LabelLocation.BottomRight:
					vec = cam.WorldToScreenPoint(vectors[3]);
					break;
				case LabelLocation.Center:
					vec = cam.WorldToScreenPoint(b.center);
					break;
				case LabelLocation.MiddleLeft:
					vec = cam.WorldToScreenPoint(new Vector3(vectors[0].x, b.center.y, vectors[0].z));
					break;
				case LabelLocation.MiddleRight:
					vec = cam.WorldToScreenPoint(new Vector3(vectors[1].x, b.center.y, vectors[1].z));
					break;
				case LabelLocation.TopLeft:
					vec = cam.WorldToScreenPoint(vectors[0]);
					break;
				case LabelLocation.TopMiddle:
					vec = cam.WorldToScreenPoint(new Vector3(b.center.x, vectors[1].y, vectors[1].z));
					break;
				case LabelLocation.TopRight:
					vec = cam.WorldToScreenPoint(vectors[1]);
					break;
			}

			return InvertScreenSpace(vec);
		}

		public static Vector3[] GetBoxVectors(Bounds b)
		{
			Vector3 v3Center = b.center;
			Vector3 v3Extents = b.extents;

			Vector3[] vectors = new Vector3[8];

			vectors[0] = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner; 2 to 0
			vectors[1] = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner; 3 to 1
			vectors[2] = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
			vectors[3] = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
			vectors[4] = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner; 6 to 4
			vectors[5] = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner; 7 to 5
			vectors[6] = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
			vectors[7] = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

			return vectors;
		}

		public static void PrepareRectangleLines(Vector2[] nvectors, Color c)
		{
			ESPBox2 box = new ESPBox2()
			{
				Color = c,
				Vertices = new Vector2[8]
				{
					nvectors[0],
					nvectors[1],
					nvectors[1],
					nvectors[3],
					nvectors[3],
					nvectors[2],
					nvectors[2],
					nvectors[0]
				}
			};

			GL.PushMatrix();
			AssetVariables.GLMaterial.SetPass(0);
			GL.Begin(GL.LINES);

			GL.Color(box.Color);

			Vector2[] vertices = box.Vertices;
			for (int j = 0; j < vertices.Length; j++)
				GL.Vertex(vertices[j]);

			GL.End();
			GL.PopMatrix();
		}

		public static void PrepareBoxLines(Vector3[] vectors, Color c)
		{
			ESPBox box = new ESPBox()
			{
				Color = c,
				Vertices = new Vector3[24]
				{
					vectors[0], //front top left to front right
					vectors[1],
					vectors[1], //front top right to front bottom right
					vectors[3],
					vectors[3], //front bottom right to front bottom left
					vectors[2],
					vectors[2], //front bottom left to front top left
					vectors[0],
					vectors[4], //back top left to back top right
					vectors[5],
					vectors[5],//back top right to back bottom right
					vectors[7],
					vectors[7], //back bottom right to back bottom left
					vectors[6],
					vectors[6], //front top left to back top left
					vectors[4],
					vectors[0], //front top right to back top right
					vectors[4],
					vectors[1], //front bottom left to back bottom left
					vectors[5],
					vectors[2], //front bottom left to back bottom left
					vectors[6],
					vectors[3], //front bottom right to back bottom right
					vectors[7]
				}
			};

			GL.PushMatrix();
			GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
			GL.modelview = Camera.main.worldToCameraMatrix;
			AssetVariables.GLMaterial.SetPass(0);
			GL.Begin(GL.LINES);

			GL.Color(box.Color);

			Vector3[] vertices = box.Vertices;
			for (int j = 0; j < vertices.Length; j++)
				GL.Vertex(vertices[j]);

			GL.End();
			GL.PopMatrix();
		}
	}
}
