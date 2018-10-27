using Modding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4.UI
{
    class DefaultUIRenderer : IUIRenderer
    {
        public bool IsDrawnInGUI => true;
        Dictionary<string, DefaultUIWindow> _windows
            = new Dictionary<string, DefaultUIWindow>();

        public IUIWindow CreateWindow(
            Vector2 position,
            int lines,
            string key,
            string title
        )
        {
            var rect = new Rect(position, new Vector2(200, 20 + 25 * lines));
            if (_windows.ContainsKey(key))
            {
                _windows[key].Update(rect, title);
            }
            else
            {
                _windows.Add(key, new DefaultUIWindow(rect, title));
            }
            return _windows[key];
        }

        private class DefaultUIWindow : IUIWindow
        {
            private string _title;
            private Rect _rect;
            private readonly int _windowID = ModUtility.GetWindowId();
            private Dictionary<int, KeyValuePair<float, string>> _floats;

            public DefaultUIWindow(Rect rect, string title)
            {
                _title = title;
                _rect = rect;
                _floats = new Dictionary<int, KeyValuePair<float, string>>();
            }

            public object Draw(int id, bool field, object value, string text)
            {
                if (field)
                {
                    switch (value)
                    {
                        case null:
                            return GUILayout.Button(text);
                        case string s:
                            return GUILayout.TextField(s);
                        case bool toggle:
                            return GUILayout.Toggle(toggle, text);
                        case float f:
                            KeyValuePair<float, string> data;
                            if (_floats.ContainsKey(id))
                            {
                                if(_floats[id].Key == f)
                                {
                                    data = _floats[id];
                                }
                                else
                                {
                                    data = new KeyValuePair<float, string>(
                                        f,
                                        f.ToString()
                                    );
                                    _floats[id] = data;
                                }
                            }
                            else
                            {
                                data = new KeyValuePair<float, string>(
                                    f,
                                    f.ToString()
                                );
                                _floats.Add(id, data);
                            }

                            var newFloat = GUILayout.TextField(data.Value);
                            if (data.Value != newFloat)
                            {
                                data = new KeyValuePair<float, string>(
                                    f,
                                    newFloat
                                );
                                _floats[id] = data;
                                if (string.IsNullOrEmpty(newFloat))
                                {
                                    return 0f;
                                }
                                
                                try
                                {
                                    return float.Parse(
                                        newFloat,
                                        CultureInfo.InvariantCulture
                                );
                                }
                                catch { }
                            }
                            return f;
                    }
                }
                else
                {
                    GUILayout.Label(value?.ToString() ?? text ?? "");
                }
                return value;
            }

            public void Update(Rect rect, string title)
            {
                _title = title;
                _rect = rect;
            }

            public Vector2 StartRender(Action callback)
            {
                return GUI.Window(_windowID, _rect, (_) =>
                {
                    GUILayout.BeginHorizontal();
                    callback();
                    GUILayout.EndHorizontal();
                    GUI.DragWindow();
                }, _title).position;
            }

            public void NewLine()
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
        }
    }
}
