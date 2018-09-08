using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PBP4.UI
{

    public class UIContainer
    {
        /// <summary>
        /// Screen position of the container
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The unique identifier of the container
        /// </summary>
        public string Key;

        /// <summary>
        /// The title to show on top of the window of the container
        /// </summary>
        public string Title;


        private UIObject[] _uiObjects;

        /// <summary>
        /// UI-Elements that are children of this container
        /// </summary>
        public UIObject[] UIObjects {
            get {
                return _uiObjects;
            }
            set {
                if (_uiObjects != value)
                {
                    _uiObjects = value;
                    Lines = _uiObjects.Length > 0 ? 1 : 0;
                    for (int i = 0; i < _uiObjects.Length - 1; i++)
                    {
                        if (_uiObjects[i].NewLine)
                        {
                            Lines++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether or not to display this UIContainer
        /// </summary>
        public bool Display;

        /// <summary>
        /// How many lines the UI has
        /// </summary>
        public int Lines { get; private set; }

        public UIContainer(
            Vector2 position,
            string key,
            string title,
            UIObject[] uiObjects,
            bool display = false
        )
        {
            Position = position;
            Key = key;
            Title = title;
            UIObjects = uiObjects;
            Display = display;
        }
    }

}