﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Thanking.Attributes;
using Thanking.Variables;

namespace Thanking.Managers
{
    public static class ComponentManager
    {
        public static void Load()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type tClass in asm.GetTypes())
                    if (tClass.IsClass)
						if (tClass.IsDefined(typeof(ComponentAttribute), false))
							Loader.HookObject.AddComponent(tClass);
        }
    }
}
