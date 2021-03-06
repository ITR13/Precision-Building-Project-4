﻿using Modding;
using PBP4.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4
{
    public class PluginManager : SingleInstance<PluginManager>
    {
        public override string Name => "PBP4 Plugins";
        private List<PluginData> _plugins = new List<PluginData>();

        private List<KeyValuePair<Plugin, bool>> _pluginQueue =
            new List<KeyValuePair<Plugin, bool>>();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            ModConsole.RegisterCommand(
                "save_pbp4",
                (args) => { SaveAll(); },
                "Save the current PBP4 configuration"
            );
            ModConsole.RegisterCommand(
                "reload_pbp4",
                (args) => { ReloadAll(); },
                "Reload the current PBP4 configuration"
            );
        }

        private void OnApplicationQuit()
        {
            SaveAll();
        }

        public void SaveAll()
        {
            ModConsole.Log("Saving PBP4 Plugins..");
            foreach (var pluginData in _plugins)
            {
                try
                {
                    Save(pluginData);
                }
                catch (Exception e)
                {
                    ModConsole.Log("Failed to save \"{0}\":\n{1}", pluginData.Name, e);
                }
            }
            Configuration.Save();
        }

        private void Save(PluginData pluginData)
        {
            var config = Configuration.GetData();
            var plugin = pluginData.Plugin;

            config.Write("_" + plugin.GetKey("enabled"), plugin.Enabled);
            config.Write(
                "_" + plugin.GetKey("ui_position"),
                (Vector3)pluginData.UIContainer.Position
            );
            config.Write(
                "_" + plugin.GetKey("display"),
                pluginData.UIContainer.Display
            );

            foreach (var data in pluginData.Input.ReadAll())
            {
                config.Write(plugin.GetKey(data.Key), data.RawValue);
            }
        }

        private void ReloadAll()
        {
            //Tell modloader to reload configuration here
            for (int i = 0; i < _plugins.Count; i++)
            {
                try
                {
                    var data = _plugins[i];
                    data.Input = Load(_plugins[i].Plugin);
                    _plugins[i] = data;
                }
                catch (Exception e)
                {
                    ModConsole.Log("Failed to reload plugin \"{0}\":\n{1}", _plugins[i].Name, e);
                }
            }
        }

        private XDataHolder Load(Plugin plugin)
        {
            var config = Configuration.GetData();
            var enabledKey = "_" + plugin.GetKey("enabled");
            if (config.HasKey(enabledKey))
            {
                plugin.Enabled = config.ReadBool(enabledKey);
            }
            else
            {
                plugin.Enabled = false;
            }
            if (plugin is DefaultPlugins.DisplayManager)
            {
                plugin.Enabled = true;
            }

            var input = plugin.DefaultInput;
            foreach (var data in input.ReadAll())
            {
                if (config.HasKey(plugin.GetKey(data.Key)))
                {
                    input.Write(
                        data.Key,
                        config.Read(
                            plugin.GetKey(data.Key)
                        ).RawValue
                    );
                }
            }

            return input;
        }

        public void UpdateDataAndUI(Plugin plugin, XDataHolder holder)
        {
            for (int i = 0; i < _plugins.Count; i++)
            {
                if (_plugins[i].Plugin != plugin)
                {
                    continue;
                }
                holder.Write(_plugins[i].Input);
                var uiObjects = GetObjects(
                    plugin,
                    _plugins[i].Input,
                    _plugins[i].ValueInfo
                );
                _plugins[i].UIContainer.UIObjects = uiObjects;
            }

        }

        public void Register(Plugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException("plugin");
            }
            _pluginQueue.Add(new KeyValuePair<Plugin, bool>(plugin, true));
        }

        public void UnRegister(Plugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException("plugin");
            }
            _pluginQueue.Add(new KeyValuePair<Plugin, bool>(plugin, false));
        }

        private void RegisterInternal(Plugin plugin)
        {
            var input = Load(plugin);

            ///These two values are used as pointers!
            var valueInfo = new ValueInfo();
            var uiObjects = GetObjects(plugin, input, valueInfo);

            var config = Configuration.GetData();

            var position = new Vector2(300, 300);
            var positionKey = "_" + plugin.GetKey("ui_position");
            if (config.HasKey(positionKey))
            {
                position = config.ReadVector3(positionKey);
            }

            var display = true;
            var displayKey = "_" + plugin.GetKey("display");
            if (config.HasKey(displayKey))
            {
                display = config.ReadBool(displayKey);
            }

            var uiContainer = new UIContainer(
                position,
                plugin.Key,
                plugin.Name,
                uiObjects.ToArray(),
                display
            );

            var pluginData =
               new PluginData(
                   plugin.Key,
                   plugin,
                   input,
                   uiContainer,
                   valueInfo
               );
            DefaultPlugins.DisplayManager.RegisterPlugin(pluginData);
            _plugins.Add(pluginData);
        }

        private UIObject[] GetObjects(
            Plugin plugin, 
            XDataHolder input,
            ValueInfo valueInfo
        )
        {
            ///Create UI elements for each input
            var uiObjects = new List<UIObject>();
            foreach (var data in input.ReadAll())
            {
                switch (data.Type)
                {
                    case "Single":
                        uiObjects.Add(new UIObject(data.Key));
                        uiObjects.Add(
                            new UIObject(
                                data.RawValue,
                                null,
                                (single) =>
                                {
                                    input.Write(data.Key, (float)single);
                                    valueInfo.ValueChanged = true;
                                },
                                true
                            )
                        );
                        break;
                    case "Integer":
                        uiObjects.Add(new UIObject(data.Key));
                        uiObjects.Add(
                            new UIObject(
                                (float)(int)data.RawValue,
                                null,
                                (single) =>
                                {
                                    input.Write(data.Key, (int)(float)single);
                                    valueInfo.ValueChanged = true;
                                },
                                true
                            )
                        );
                        break;
                    case "Vector3":
                        void Put(object single, int i)
                        {
                            var v3 = input.ReadVector3(data.Key);
                            v3[i] = (float)single;
                            input.Write(data.Key, v3);
                            valueInfo.ValueChanged = true;
                        }


                        uiObjects.Add(new UIObject(data.Key));
                        uiObjects.Add(
                            new UIObject(
                                ((Vector3)data.RawValue).x,
                                null,
                                f => Put(f, 0)
                            )
                        );
                        uiObjects.Add(
                            new UIObject(
                                ((Vector3)data.RawValue).y,
                                null,
                                f => Put(f, 1)
                            )
                        );
                        uiObjects.Add(
                            new UIObject(
                                ((Vector3)data.RawValue).z,
                                null,
                                f => Put(f, 2),
                                true
                            )
                        );
                        break;
                    case "Boolean":
                        uiObjects.Add(new UIObject(data.Key));
                        uiObjects.Add(
                            new UIObject(
                                data.RawValue,
                                null,
                                (boolean) =>
                                {
                                    input.Write(data.Key, (bool)boolean);
                                    valueInfo.ValueChanged = true;
                                },
                                true
                            )
                        );
                        break;
                }
            }

            if (uiObjects.Count > 0)
            {
                var lastObj = uiObjects[uiObjects.Count - 1];
                lastObj.NewLine = true;
                uiObjects[uiObjects.Count - 1] = lastObj;
            }

            ///Create global UI elements
            ApplyRule applyRule =
                (ApplyRule)((int)plugin.ApplyRule % (int)ApplyRule.NoEnable);


            if (applyRule == ApplyRule.OnApply)
            {
                uiObjects.Add(
                    new UIObject(
                        null,
                        "Apply",
                        (b) =>
                        {
                            valueInfo.WasApplied |=
                                b is bool ? (bool)b : false;
                        }
                    )
                );
            }

            if ((plugin.ApplyRule & ApplyRule.NoEnable) == ApplyRule.Disabled)
            {
                uiObjects.Add(
                    new UIObject(
                        "Enabled:"
                    )
                );
                uiObjects.Add(
                    new UIObject(
                        plugin.Enabled,
                        null,
                        (b) =>
                        {
                            plugin.Enabled = b is bool
                                           ? (bool)b
                                           : plugin.Enabled;
                            valueInfo.ValueChanged = true;
                            valueInfo.WasApplied = true;
                        },
                        true
                    )
                );
            }

            return uiObjects.ToArray();
        }

        private void UnRegisterInternal(Plugin plugin)
        {
            for (int i = _plugins.Count - 1; i >= 0; i--)
            {
                if (_plugins[i].Plugin == plugin)
                {
                    DefaultPlugins.DisplayManager
                                  .UnregisterPlugin(_plugins[i].UIContainer);
                    Save(_plugins[i]);
                    _plugins.RemoveAt(i);
                }
            }
        }

        private void Update()
        {
            var pluginQueue =
                new List<KeyValuePair<Plugin, bool>>(_pluginQueue);
            _pluginQueue.Clear();
            foreach (var pair in pluginQueue)
            {
                try
                {
                    if (pair.Value)
                    {
                        RegisterInternal(pair.Key);
                    }
                    else
                    {
                        UnRegisterInternal(pair.Key);
                    }
                }
                catch (Exception e)
                {
                    ModConsole.Log(
                        "Failed to load plugin {0} ({1}):\n{2}",
                        pair.Key.Name,
                        pair.Key.Key,
                        e
                    );
                }
            }

            var manager = UIManager.Instance;
            foreach (var data in _plugins)
            {
                manager.AddInterface(
                    data.UIContainer
                );

                if (!data.Plugin.Enabled)
                {
                    data.ValueInfo.WasApplied = false;
                    continue;
                }
                var rule = (ApplyRule)(
                    (int)data.Plugin.ApplyRule % (int)ApplyRule.NoEnable
                );
                if (
                    (rule == ApplyRule.Constant) ||
                    (rule == ApplyRule.OnApply && data.ValueInfo.WasApplied) ||
                    (rule == ApplyRule.OnChange && data.ValueInfo.ValueChanged)
                )
                {
                    data.ValueInfo.ValueChanged = false;
                    data.Plugin.Apply(data.Input);
                }
                data.ValueInfo.WasApplied = false;
            }
        }
    }

    public struct PluginData
    {
        public string Name;
        public Plugin Plugin;
        public XDataHolder Input;
        public UIContainer UIContainer;
        public ValueInfo ValueInfo;

        public PluginData(
            string name,
            Plugin plugin,
            XDataHolder input,
            UIContainer container,
            ValueInfo valueInfo
        )
        {
            Name = name;
            Plugin = plugin;
            Input = input;
            UIContainer = container;
            ValueInfo = valueInfo;
        }
    }

    public class ValueInfo
    {
        public bool ValueChanged;
        public bool WasApplied;
    }
}
