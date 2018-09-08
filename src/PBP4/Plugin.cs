using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PBP4
{
    public abstract class Plugin
    {
        private bool _enabled = true;
        /// <summary>
        /// Whether or not the mod is enabled
        /// </summary>
        public bool Enabled {
            get {
                return _enabled;
            }
            internal set {
                try
                {
                    if (value)
                    {
                        OnEnable(_enabled);
                    }
                    else
                    {
                        OnDisable(_enabled);
                    }
                }
                catch (Exception e)
                {
                    ModConsole.Log(
                        "Failed to set enabled state of {0} to {1}:\n{2}", 
                        Key, 
                        value, 
                        e
                    );
                }
                _enabled = value;
            }
        }


        private string _key;
        /// <summary>
        /// The unique identifier of the plugin
        /// </summary>
        public string Key {
            get {
                if (string.IsNullOrEmpty(_key))
                {
                    _key = GetType().FullName;
                }
                return _key;
            }
        }

        internal string GetKey(string key)
        {
            return string.Format("{0}-{1}", Key, key);
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// When to call Apply
        /// </summary>
        public abstract ApplyRule ApplyRule { get; }

        /// <summary>
        /// The default input of the Apply function
        /// This is used to generate the UI
        /// </summary>
        public abstract XDataHolder DefaultInput { get; }

        /// <summary>
        /// Apply the input for the UI
        /// </summary>
        /// <param name="input">
        /// The input in the format specified by <see cref="DefaultInput"/>
        /// </param>
        public abstract void Apply(XDataHolder input);

        /// <summary>
        /// Called whenever the mod is enabled
        /// </summary>
        /// <param name="prevState">True if set from enabled to enabled</param>
        protected virtual void OnEnable(bool prevState) { }
        /// <summary>
        /// Called whenever the mod is disabled
        /// </summary>
        /// <param name="prevState">False if set from disabled to disabled</param>
        protected virtual void OnDisable(bool prevState) { }
    }

    public enum ApplyRule
    {
        /// <summary>
        /// Literally just never applied
        /// </summary>
        Disabled,
        /// <summary>
        /// Apply whenever a field has been updated
        /// </summary>
        OnChange,
        /// <summary>
        /// Apply whenever the apply button has been pressed
        /// </summary>
        OnApply,
        /// <summary>
        /// Apply every frame (Update)
        /// </summary>
        Constant,

        //NB: Keep this a power of 2!
        /// <summary>
        /// Do a binary or with this to make the Enable button not appear
        /// </summary>
        NoEnable,
    }
}
