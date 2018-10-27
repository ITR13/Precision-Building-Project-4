using System;
using Modding;
using PBP4.DefaultPlugins;
using UnityEngine;

namespace PBP4
{
	public class Mod : ModEntryPoint
	{
		public override void OnLoad()
		{
            var manager = PluginManager.Instance;
            manager.Register(new DisplayManager());
            manager.Register(new Offset());
            manager.Register(new Scale());
            manager.Register(new BlockID());
            manager.Register(new TransformMachine());
		}
	}
}
