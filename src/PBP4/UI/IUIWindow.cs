using System;
using UnityEngine;

namespace PBP4.UI
{
    public interface IUIWindow
    {
        Vector2 StartRender(Action callback);
        object Draw(int id, bool field, object value, string text = null);
        void NewLine();
    }
}