using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Drawing = System.Drawing;
using Awesomium;
using Awesomium.Core;
using WaveEngine.Framework;
using WaveEngine.Components.UI;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework.Graphics;
using System.Runtime.InteropServices;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
using WaveEngine.Components.Gestures;
using System.Reflection;
using WaveEngine.Framework.Services;

namespace HumbleGuns
{
    [DataContract]
    public class UIconnector : Behavior
    {
        // Public Variables
        public static int width = 512;
        public static int height = 512;
        public string initialURL = "http://google.com";

        //private UIgrid ug;
        private Entity ui;
        private static Component webCoreHelper;
        
        private bool isFocused = false;
        private bool isScrollable = false;
        private static List<WebView> webViewList = new List<WebView>();
        private WebView webView;
        private Texture2D texture;
        //private Color[] Pixels;
        //private GCHandle PixelsHandle;

        public Scene CurrentScene { get; private set; }

        public UIconnector(Scene currentScene) : base()
        {
            CurrentScene = currentScene;
            var vm = currentScene.VirtualScreenManager;
            //this.width = (int)vm.VirtualWidth;
            //this.height = (int)vm.VirtualHeight;


            ////ic = new ImageControl(Color.Black,100,25);
            ui = CurrentScene.EntityManager.Find<Entity>("UserInterface");
            var ug = new UIgrid(currentScene).Entity;
            ui.AddChild(ug);

            // Initialize the WebCore if it's not active
            if (!WebCore.IsInitialized)
            {
                WebConfig conf = new WebConfig();
                WebCore.Initialize(conf);
                webCoreHelper = new WebCoreHelper();
                ui.AddComponent(webCoreHelper);
                //WebCore.Run();
            }

            
            webView = WebCore.CreateWebView(width, height, WebViewType.Offscreen);
            webViewList.Add(webView);
            LoadURL(initialURL);

            texture = new Texture2D() { Width = width, Height = height, Format = PixelFormat.R8G8B8A8, Faces = 1, Levels = 1 };
            //Pixels = texture.GetData();
            //PixelsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);

            var ic = ug.FindComponent<Sprite>();
            if (ic != null)
            {
                ic = new Sprite(texture);
            }
            else
            {
                throw new Exception("Game Object has no Material or GUI Texture, we cannot render a web-page to this object!");
            }

            RegisterEvents();
        }

        // Start loading a certain URL
        public void LoadURL(string url)
        {
            if (WebCore.IsInitialized)
                webView.Source = new Uri(url);
        }
        // Start loading a string of HTML
        public void LoadHTML(string html)
        {
            if (WebCore.IsInitialized)
                webView.LoadHTML(html);
        }               
                        
        public void GoBack()
        {               
            if (WebCore.IsInitialized)
                webView.GoToHistoryOffset(-1);
        }               
                        
        public void GoForward()
        {               
            if (WebCore.IsInitialized)
                webView.GoToHistoryOffset(1);
        }               
                        
        public void Reload()
        {               
            if (WebCore.IsInitialized)
                webView.Reload(true);
        }

        
        protected override void Update(TimeSpan gameTime)
        {
            if(ui.IsDisposed)
            {
                // We shutdown the WebCore only once
                if (WebCore.IsInitialized) { ui.RemoveComponent<WebCoreHelper>(); WebCore.Shutdown(); }
            }

            if (WebCore.IsRunning)
            {
                BitmapSurface surface = (BitmapSurface)webView.Surface;
                //RenderTarget rt = new RenderTarget(this.width, this.height);

                if (surface != null)
                {
                    //surface.CopyTo(PixelsHandle.AddrOfPinnedObject(), surface.RowSpan, 4, true, true);
                    CreateData(surface, texture.Data);
                    WaveServices.GraphicsDevice.Textures.UploadTexture(texture);
                    //texture.SetPixels32(Pixels, 0);
                    //texture.Apply(false, false);
                }
            }
        }

        private static void CreateData(BitmapSurface surface, byte[][][] data)
        {
            // Copy to byte array
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            Drawing.Bitmap myBitmap = new Drawing.Bitmap(width, height, Drawing.Imaging.PixelFormat.Format32bppArgb);
            Drawing.Imaging.BitmapData bmpData = myBitmap.LockBits(
                                                    new Drawing.Rectangle(0, 0, myBitmap.Width, myBitmap.Height), 
                                                    Drawing.Imaging.ImageLockMode.WriteOnly, 
                                                    myBitmap.PixelFormat);
            surface.CopyTo(bmpData.Scan0, surface.RowSpan, 4, true, true);
            myBitmap.UnlockBits(bmpData);
            myBitmap.Save(ms, Drawing.Imaging.ImageFormat.Bmp);

            
            byte[] fromStream = ms.ToArray();

            // Build texture data
            data = new byte[1][][];
            data[0] = new byte[1][];
            data[0][0] = new byte[width * height * 4];

            // Copy to texture data
            int offset = fromStream.Length - data[0][0].Length;
            int limit = width * 4;
            for (int i = 0; i < data[0][0].Length; i++)
            {

                int row = width - (int)(Math.Round((double)(i / limit), MidpointRounding.AwayFromZero) - 1) - 2;
                int index = row * limit + i % limit;
                data[0][0][index] = fromStream[offset + i];
            }

            ms.Dispose();
            // Return
            //return data;
        }

        public void OnGUI(/*string name, object sender, EventArgs args*/)
        {
            if (!WebCore.IsInitialized)
                return;

            

            //// We only inject keyboard input when the GameObject has focus
            //if (e.isKey == true && isFocused == true)
            //{
            //    if (e.type == EventType.KeyDown)
            //    {
            //        if (e.character == 0)
            //        {
            //            WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            //            keyEvent.Type = WebKeyType.KeyDown;
            //            keyEvent.VirtualKeyCode = MapKeys(e);
            //            keyEvent.Modifiers = MapModifiers(e);
            //            webView.InjectKeyboardEvent(keyEvent);
            //        }
            //        else
            //        {
            //            WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            //            keyEvent.Type = WebKeyType.Char;
            //            keyEvent.Text = new ushort[] { e.character, 0, 0, 0 };
            //            keyEvent.Modifiers = MapModifiers(e);
            //            webView.InjectKeyboardEvent(keyEvent);
            //        }
            //    }

            //    if (e.type == EventType.KeyUp)
            //    {
            //        WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            //        keyEvent.Type = WebKeyType.KeyUp;
            //        keyEvent.VirtualKeyCode = MapKeys(e);
            //        keyEvent.Modifiers = MapModifiers(e);
            //        webView.InjectKeyboardEvent(keyEvent);
            //    }
            //}

            //// We unfocus each WebView whenever a MouseDown event is encountered in OnGUI.
            //// The actual focusing of a specific element occurs in OnMouseDown
            //if (e.type == EventType.MouseDown)
            //{
            //    Unfocus();
            //}

            //if (e.type == EventType.ScrollWheel && isScrollable == true)
            //{
            //    webView.InjectMouseWheel((int)e.delta.y * -10);
            //}
        }

        private void RegisterEvents()
        {
            //var eventList = typeof(TouchGestures).GetEvents();
            //foreach (EventInfo ei in eventList)
            //{
            //    //var eventInfo = typeof(TouchGestures).GetEvent(ei.Name);
            //    EventHandler delegateForMethod = (sender, args) => OnGUI(ei.Name, sender, args);
            //    ei.AddEventHandler(this, delegateForMethod);
            //}
        }
    }
}
