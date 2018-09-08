using Modding;
using PBP4.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PBP4.DefaultPlugins
{
    class DisplayManager : Plugin
    {
        private static Plugin _plugin;

        private static XDataHolder _defaultInput = new XDataHolder();
        private static HashSet<UIContainer> _containers =
            new HashSet<UIContainer>();
        internal static void RegisterPlugin(PluginData pluginData)
        {
            if (
                pluginData.Plugin is DisplayManager ||
                _containers.Contains(pluginData.UIContainer)
            )
            {
                return;
            }
            _containers.Add(pluginData.UIContainer);
            _defaultInput.Write(
                pluginData.UIContainer.Title,
                pluginData.UIContainer.Display
            );

            if (_plugin != null)
            {
                PluginManager.Instance.UnRegister(_plugin);
                PluginManager.Instance.Register(_plugin);
            }
        }

        internal static void UnregisterPlugin(UIContainer uiContainer)
        {
            if (_containers.Contains(uiContainer))
            {
                _containers.Remove(uiContainer);
                PluginManager.Instance.UnRegister(_plugin);
                PluginManager.Instance.Register(_plugin);
            }
        }

        public override string Name => "DisplayManager";

        public override ApplyRule ApplyRule => 
            ApplyRule.OnChange | ApplyRule.NoEnable;

        public override XDataHolder DefaultInput => _defaultInput.Clone();

        public override void Apply(XDataHolder input)
        {
            input.Write(_defaultInput);
            foreach (var uiContainer in _containers)
            {
                if (_defaultInput.HasKey(uiContainer.Title))
                {
                    uiContainer.Display = 
                        _defaultInput.ReadBool(uiContainer.Title);
                }
            }
        }

        public DisplayManager()
        {
            _plugin = this;
        }
    }
}
