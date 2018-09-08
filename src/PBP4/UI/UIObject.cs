using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Modding;

namespace PBP4.UI
{

    public struct UIObject
    {
        /// <summary>
        /// Value of UIObject
        /// </summary>
        public object Value;

        /// <summary>
        /// Text to display if applicable
        /// </summary>
        public string Text;

        /// <summary>
        /// Called if Value is changed
        /// </summary>
        public Action<object> Callback;

        /// <summary>
        /// Set this to true if you want a new line
        ///  after drawing the ui element for this
        /// </summary>
        public bool NewLine;

        public UIObject(
            object value, 
            string text = null, 
            Action<object> callback = null, 
            bool newLine = false
        ) : this()
        {
            Value = value;
            Text = text;
            Callback = callback;
            NewLine = newLine;
        }
    }
}