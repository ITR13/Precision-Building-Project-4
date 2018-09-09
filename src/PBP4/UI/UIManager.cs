using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4.UI
{
    public class UIManager : SingleInstance<UIManager>
    {
        private static HashSet<string> _logged = new HashSet<string>();
        private static void ErrorOnce(string id, string format, params object[] args)
        {
            if (!_logged.Contains(id))
            {
                _logged.Add(id);
                Debug.LogErrorFormat(format, args);
            }
        }


        public override string Name => "PBP4 UI";
        private List<WeakReference> _interfaces = new List<WeakReference>();

        public IUIRenderer Renderer = new DefaultUIRenderer();


        public bool AddInterface(UIContainer uiObject)
        {
            ClearNull();
            if (uiObject == null)
            {
                throw new ArgumentNullException("uiObject");
            }
            if (_interfaces.Any(wr => wr.Target == uiObject))
            {
                return false;
            }
            _interfaces.Add(new WeakReference(uiObject));
            return true;
        }

        public void RemoveInterface(UIContainer uiObject)
        {
            if (uiObject == null)
            {
                throw new ArgumentNullException("uiObject");
            }
            _interfaces.RemoveAll(wr => wr.Target == null || wr.Target == uiObject);
        }

        private void ClearNull()
        {
            _interfaces.RemoveAll(wr => wr.Target == null);
        }

        private void Update()
        {
            ClearNull();
            if (Renderer != null && !Renderer.IsDrawnInGUI)
            {
                Render();
            }
        }

        private void OnGUI()
        {
            if (Renderer != null && Renderer.IsDrawnInGUI)
            {
                Render();
            }
        }

        private void Render()
        {
            if (StatMaster.hudHidden)
            {
                return;
            }
            foreach (var i in _interfaces)
            {
                if (
                    !(i.Target is UIContainer container) ||
                    !container.Display ||
                    container.UIObjects == null
                )
                {
                    continue;
                }
                var window = Renderer.CreateWindow(
                    container.Position,
                    container.Lines,
                    container.Key,
                    container.Title
                );
                if (window != null)
                {
                    container.Position = window.StartRender(
                        () => OnRender(container, window)
                    );
                }
            }
        }

        private void OnRender(UIContainer container, IUIWindow window)
        {
            if (container.UIObjects == null)
            {
                return;
            }
            for (int j = 0; j < container.UIObjects.Length; j++)
            {
                var uiObject = container.UIObjects[j];
                if (uiObject.Callback != null)
                {
                    var newValue = window.Draw(
                        j,
                        true,
                        uiObject.Value,
                        uiObject.Text
                    );

                    if (uiObject.Value == null)
                    {
                        try
                        {
                            uiObject.Callback(newValue);
                        }
                        catch (Exception e)
                        {
                            ErrorOnce(
                                j + ": " + container.Key,
                                "Callback for {0}-{1} (null) failed:\n{2}",
                                container.Title,
                                j,
                                e
                            );
                        }
                    }
                    ///Hahaha, this is ugly and can easily break, but whatever
                    else if (newValue.GetHashCode() != uiObject.Value.GetHashCode())
                    {
                        try
                        {
                            uiObject.Callback(newValue);
                        }
                        catch (Exception e)
                        {
                            ErrorOnce(
                                j + ": " + container.Key,
                                "Callback for {0}-{1} ({2}=>{3}) failed:\n{3}",
                                container.Title,
                                j,
                                uiObject.Value.GetType(),
                                newValue.GetType(),
                                e
                            );
                        }
                        uiObject.Value = newValue;
                    }
                }
                else
                {
                    window.Draw(
                        j,
                        false,
                        uiObject.Value,
                        uiObject.Text
                    );
                }
                if (uiObject.NewLine)
                {
                    window.NewLine();
                }
                container.UIObjects[j] = uiObject;
            }
        }
    }
}
