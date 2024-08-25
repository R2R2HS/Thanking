using System;
using Thanking.Managers.Main;
using Thanking.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Thnkng
{
	public static class Ldr
    {
        public static GameObject HookObject;
        
        public static void Hook()
        {
#if DEBUG
			DebugUtilities.Log("Initializing Thanking...");
#endif        
    
			HookObject = new GameObject();
			Object.DontDestroyOnLoad(HookObject);
            try
            {
                AttributeManager.Init();
                AssetManager.Init();
                ConfigManager.Init();
            }
            catch (Exception e)
            {
                DebugUtilities.LogException(e);
            }
#if DEBUG
			DebugUtilities.Log("Thanking initialized!");
#endif
		}
    }
}
