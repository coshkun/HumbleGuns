﻿using System;
using System.IO;
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
    //public delegate void OnGuiEventHandler(object sender, EventArgs args, string name);

    [DataContract]
    public class UIconnector : Behavior
    {
        //public event OnGuiEventHandler OnGUI;

        // Public Variables
        public static int width = 512;
        public static int height = 512;
        public string initialURL = "http://www.google.com.tr";

        //private UIgrid ug;
        private Entity ui;
        private static Component webCoreHelper;

        private bool isFocused = false;
        private bool isScrollable = false;
        private static List<WebView> webViewList = new List<WebView>();
        private static WebView webView;
        private static Texture2D texture;
        private static Sprite ic;
        private static Entity ug;
        private static string tmpFileBuffer;

        public Scene CurrentScene { get; private set; }

        public UIconnector(Scene currentScene) : base()
        {
            CurrentScene = currentScene;
            var vm = currentScene.VirtualScreenManager;
            width = (int)vm.VirtualWidth;
            height = (int)vm.VirtualHeight;


            // Create Entity Instances
            ui = CurrentScene.EntityManager.Find<Entity>("UserInterface");
            ug = new UIgrid(currentScene).Entity;
            ui.AddChild(ug);

            // Initialize the WebCore if it's not active
            if (!WebCore.IsInitialized)
            {
                WebConfig conf = new WebConfig();
                WebCore.Initialize(conf);
                webCoreHelper = new WebCoreHelper();
                ui.AddComponent(webCoreHelper);
                //WebCore.Run();
                texture = new Texture2D() { Width = width, Height = height,
                    Format = PixelFormat.R8G8B8A8,
                    Faces = 1, Levels = 1,
                    Usage = TextureUsage.Dynamic,
                    CpuAccess = TextureCpuAccess.Write,
                    Type = TextureType.TextureVideo };

                tmpFileBuffer = Path.GetTempPath() + Guid.NewGuid().ToString() + ".jpg";
            }


            webView = WebCore.CreateWebView(width, height, WebViewType.Offscreen);
            webViewList.Add(webView);
            LoadURL(initialURL);

            // Assing the target texture to its parent Entity
            ic = ug.FindComponent<Sprite>();
            if (ic != null)
            {
                ug.RemoveComponent<Sprite>(); ug.AddComponent(new Sprite(texture));
                WaveServices.GraphicsDevice.Textures.UploadTexture(texture);
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
            if (ui.IsDisposed)
            {
                // We shutdown the WebCore only once
                if (WebCore.IsInitialized)
                {
                    ui.RemoveComponent<WebCoreHelper>();
                    webView.Dispose(); WebCore.Shutdown();
                    if (File.Exists(tmpFileBuffer)) { File.Delete(tmpFileBuffer); }
                }
            }

            if (WebCore.IsRunning)
            {
                BitmapSurface surface = (BitmapSurface)webView.Surface;
                //RenderTarget
                if (surface == null) { return; }

                if (!webView.IsLoading)
                {
                    CreateData(surface);

                }
            }

            if ((gameTime.TotalMilliseconds % (int)(UIEventListener.TickDelay * 1.5)) == 0) { UIEventListener.UpdateEvents(this); }
        }

        private static void CreateData(BitmapSurface surface)
        {
            // Copy to byte array
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            Drawing.Bitmap myBitmap = new Drawing.Bitmap(width, height, Drawing.Imaging.PixelFormat.Format32bppRgb);
            Drawing.Imaging.BitmapData bmpData = myBitmap.LockBits(
                                                    new Drawing.Rectangle(0, 0, myBitmap.Width, myBitmap.Height),
                                                    Drawing.Imaging.ImageLockMode.WriteOnly,
                                                    myBitmap.PixelFormat);
            // Internal Awesomium method for export data to buffer 
            surface.CopyTo(bmpData.Scan0, surface.RowSpan, 4, false, false);
            myBitmap.UnlockBits(bmpData);

            myBitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX); // necessary for color correction
            myBitmap.Save(ms, Drawing.Imaging.ImageFormat.Bmp);

            // save to disk  (for debug purpose, you can safely remove this line)
            // surface.SaveToJPEG(tmpFileBuffer);

            byte[] fromStream = ms.ToArray();
            int j, k; j = k = fromStream.Length; byte[] bfr = new byte[j];

            // invert stream buffer (because WaveService->SetData() messing with write direction of pixels)
            while (j > 0) { bfr[fromStream.Length - j] = fromStream[j - 1]; j--; }

            // now apply the color correction for WaveService
            byte R, G, B, A;     // int offset = fromStream.Length - (width * height * 4);  // no need this anymore
            while (k > 0)
            {

                if (k % 4 == 0)
                {
                    A = bfr[k - 4];
                    B = bfr[k - 3];
                    G = bfr[k - 2];
                    R = bfr[k - 1];

                    bfr[k - 4] = R;
                    bfr[k - 3] = G;
                    bfr[k - 2] = B;
                    bfr[k - 1] = A;
                }
                k--;
            }
            // now we can safely update it
            WaveServices.GraphicsDevice.Textures.SetData(texture, bfr);

            ////// Build texture data
            //byte[][][] data = new byte[1][][];
            //data[0] = new byte[1][];
            ////data[0][0] = new byte[width * height * 4];
            //data[0][0] = new byte[fromStream.Length];
            //for (int i = 0; i < fromStream.Length; i++) { data[0][0][i] = fromStream[i]; }

            ////// Copy to texture data
            ////int offset = fromStream.Length - data[0][0].Length;
            ////int limit = width * 4;
            ////for (int i = 0; i < data[0][0].Length; i++)
            ////{
            ////    int row = width - (int)(Math.Round((double)(i / limit), MidpointRounding.AwayFromZero) - 1) - 2;
            ////    int index = row * limit + i % limit;
            ////    data[0][0][index] = fromStream[offset + i];
            ////}
            ms.Dispose();
        }

        public void OnGUI(object sender, EventArgs args, string name)
        {
            if (!WebCore.IsInitialized)
                return;
        }

        private void RegisterEvents()
        {
            var eventList = typeof(TouchGestures).GetEvents();
            foreach (EventInfo ei in eventList)
            {
                //var eventInfo = typeof(TouchGestures).GetEvent(ei.Name);
                //OnGuiEventHandler delegateForMethod = (sender, args, name) => OnGUI(sender, args, ei.Name);
                //ei.AddEventHandler(this, delegateForMethod);
                
            }
        }
    }
}
