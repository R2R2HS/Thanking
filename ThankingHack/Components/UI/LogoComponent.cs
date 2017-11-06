﻿using SDG.Unturned;
using Thanking.Attributes;
using Thanking.Options;
using Thanking.Utilities;
using Thanking.Variables;
using UnityEngine;

namespace Thanking.Components.UI
{
	[Component]
	[SpyComponent]
	public class LogoComponent : MonoBehaviour
	{
		public void OnGUI()
		{
			if (MiscOptions.LogoEnabled)
			{
				DrawUtilities.DrawLabel(ESPComponent.ESPFont, LabelLocation.BottomRight, new Vector2(20, 40),
					"Thanking v2.0.5 Alpha", Color.black, Color.cyan, 3);
				if (Provider.isConnected && !Provider.isLoading)
					DrawUtilities.DrawLabel(ESPComponent.ESPFont, LabelLocation.BottomRight, new Vector2(20, 80),
						$"PlayerMovement: {PlayerMovement.forceTrustClient}", Color.black, Color.red, 3);
			}
		}
	}
}
