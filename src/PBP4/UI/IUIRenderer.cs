using System;
using UnityEngine;

namespace PBP4.UI
{
    public interface IUIRenderer
    {
        bool IsDrawnInGUI { get; }

        IUIWindow CreateWindow(
            Vector2 position, 
            int lines, 
            string key, 
            string title
        );
    }
}