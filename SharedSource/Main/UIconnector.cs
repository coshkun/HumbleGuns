using System;
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
using WaveEngine.Common.Math;
using System.Runtime.InteropServices;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
using WaveEngine.Components.Gestures;
using System.Reflection;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.Physics2D;
using WaveEngine.Framework.Diagnostic;
using WaveEngine.Common.Input;

namespace HumbleGuns
{
    //public delegate void OnGuiEventHandler(object sender, EventArgs args, string name);

    [DataContract]
    public class UIconnector : Behavior
    {
        public event OnGuiEventHandler OnGUI;

        // Public Variables
        public static int width = 512;
        public static int height = 512;
        public string initialURL = "http://www.google.com.tr";

        //private UIgrid ug;
        private Entity ui;
        private static Component webCoreHelper;

        private bool isFocused = false;
        private bool isScrollable = false;
        private bool isColliding = false;

        private static List<WebView> webViewList = new List<WebView>();
        private static WebView webView;
        private static WebSession session;
        private static Texture2D texture;
        private static Sprite ic;
        private static Entity ug;
        private static string tmpFileBuffer;
        private static Vector2 cam2pos;
        //hold Inputs
        private static KeyboardState state;
        private static MouseState ms;
        private static int Wheel;

        public Scene CurrentScene { get; private set; }
        public KeyboardState ks { get; private set; }

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
            cam2pos = CurrentScene.EntityManager.Find<Entity>("defaultCamera2D")
                          .FindComponent<Transform2D>().ScreenPosition;

            // Initialize the WebCore if it's not active
            if (!WebCore.IsInitialized)
            {
                tmpFileBuffer = Path.GetTempPath() + Guid.NewGuid().ToString();// + ".jpg";
                Directory.CreateDirectory(tmpFileBuffer);

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
                session = WebCore.CreateWebSession(
                tmpFileBuffer, WebPreferences.Default);
            }


            webView = WebCore.CreateWebView(width, height, session, WebViewType.Offscreen);
            //webView.Surface = new BitmapSurface(width,height);
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

            this.OnGUI += OnGui;
            RegisterEvents();
            this.StartListener();
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

        public void Focus()
        {
            if (!WebCore.IsInitialized)
                return;

            // Unfocus all open webViews, then focus the webView that was just clicked
            foreach (WebView view in webViewList)
            { if (view != null) { view.UnfocusView(); } }

            webView.FocusView();
            isFocused = true;
        }

        public void Unfocus()
        {
            if (!WebCore.IsInitialized)
                return;

            // Unfocus all open webViews
            foreach (WebView view in webViewList)
            { if (view != null) { view.UnfocusView(); } }

            isFocused = false;
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
                    if (Directory.Exists(tmpFileBuffer)) { Directory.Delete(tmpFileBuffer,true); }
                }
            }

            if (WebCore.IsInitialized)
            {
                BitmapSurface surface = (BitmapSurface)webView.Surface;
                //RenderTarget
                if (surface == null) { return; }

                if (!webView.IsLoading)
                {
                    CreateData(surface);
                }
            }
            //hold inputs
            state = WaveServices.Input.KeyboardState;
            ms = WaveServices.Input.MouseState;
            Wheel = ms.Wheel;

            //if ((gameTime.Milliseconds % (int)(UIEventListener.TickDelay * 1.5)) == 0) { UIEventListener.UpdateEvents(this); }
            /* if ((DateTime.Now.Subtract(UIEventListener.counter).Milliseconds % (int)(UIEventListener.TickDelay * 1.5)) == 0) */
            { UIEventListener.UpdateEvents(this); }
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

        public void OnGui(object sender, EventArgs args, string name)
        {
            if (!WebCore.IsInitialized)
                return;

            ////hold inputs
            //ks = WaveServices.Input.KeyboardState;
            //ms = WaveServices.Input.MouseState;
            OnMouseOver();
            OnMouseDown();
            bool chk = CheckIfAnyKeyPressed(state);

            // We only inject keyboard input when the GameObject has focus
            if (isFocused == true)
            {
                if (chk)
                {
                    char? rslt = IsCharKeys(ks);
                    if (rslt == null)
                    {
                        WebKeyboardEvent keyEvent = new WebKeyboardEvent();
                        keyEvent.Type = WebKeyboardEventType.KeyDown;
                        keyEvent.VirtualKeyCode = MapKeys(state);
                        keyEvent.Modifiers = MapModifiers(state);
                        webView.InjectKeyboardEvent(keyEvent);
                    }
                    else
                    {
                        WebKeyboardEvent keyEvent = new WebKeyboardEvent();
                        keyEvent.Type = WebKeyboardEventType.Char;
                        keyEvent.Text = rslt.ToString();  // new ushort[] { ushort.Parse(rslt), 0, 0, 0 };
                        keyEvent.Modifiers = MapModifiers(ks);
                        webView.InjectKeyboardEvent(keyEvent);
                    }
                }
                else //if (e.type == WebKeyboardEventType.KeyUp)
                {
                    WebKeyboardEvent keyEvent = new WebKeyboardEvent();
                    keyEvent.Type = WebKeyboardEventType.KeyUp;
                    keyEvent.VirtualKeyCode = MapKeys(state);
                    keyEvent.Modifiers = MapModifiers(state);
                    webView.InjectKeyboardEvent(keyEvent);
                }
            }

            //int latency = UIEventListener.HitTime.Subtract(UIEventListener.counter).Milliseconds;
            Labels.Add("x: ", ms.X);
            Labels.Add("y :", ms.Y);
            Labels.Add("whl: ", chk.ToString());
        }

        private bool CheckIfAnyKeyPressed(KeyboardState state)
        {
            bool rslt = false; int k = 0;
            do {
                rslt = state.IsKeyPressed((Keys)k); if (rslt) { break; }
                k++; } while (k <= 250);
            return rslt;
        }

        private void OnMouseOver()
        {
            if (!WebCore.IsInitialized)
                return;

            if (!isFocused)
                Focus();

            //Ray r = new WaveEngine.Common.Math.Ray(new Vector3(x,y,1), new Vector3(x, y, -1));

            //var cldr = ug.FindComponent<RectangleCollider2D>();
            //isColliding = cldr.Intersects(ref r);
            //if (latency % 2 == 0)
            //{
                webView.InjectMouseMove(ms.X, ms.Y);
                webView.InjectMouseWheel((int)Wheel,0);
            //    return;
            //}
        }

        private void OnMouseDown()
        {
            if (!WebCore.IsInitialized)
                return;

            if (!isFocused)
                Focus();

            //Ray r = new WaveEngine.Common.Math.Ray(ms.Position.ToVector3(1), ms.Position.ToVector3(-1));
            //var cldr = ug.FindComponent<RectangleCollider2D>();
            //if (cldr.Intersects(ref r))
            //{

            webView.InjectMouseMove(ms.X, ms.Y);
            // Presed
            if (ms.LeftButton == ButtonState.Pressed)
            {
                webView.InjectMouseDown(MouseButton.Left); return;
            }
            if (ms.RightButton == ButtonState.Pressed)
            {
                webView.InjectMouseDown(MouseButton.Right); return;
            }
            if (ms.MiddleButton == ButtonState.Pressed)
            {
                webView.InjectMouseDown(MouseButton.Middle); return;
            }
            // Released
            if (ms.LeftButton == ButtonState.Release)
            {
                webView.InjectMouseUp(MouseButton.Left);
            }
            if (ms.RightButton == ButtonState.Release)
            {
                webView.InjectMouseUp(MouseButton.Right);
            }
            if (ms.MiddleButton == ButtonState.Release)
            {
                webView.InjectMouseUp(MouseButton.Middle);
            }
            //}
        }

        private void OnMouseEnter()
        {
            isScrollable = true;
        }

        private void OnMouseExit()
        {
            isScrollable = false;
        }

        private Modifiers MapModifiers(KeyboardState ks)
        {
            int modifiers = 0;

            if (ks.Control == ButtonState.Pressed)
                modifiers |= (int)Modifiers.ControlKey;

            if (ks.Shift == ButtonState.Pressed)
                modifiers |= (int)Modifiers.ShiftKey;

            if (ks.LeftAlt == ButtonState.Pressed || ks.RightAlt == ButtonState.Pressed)
                modifiers |= (int)Modifiers.AltKey;

            return (Modifiers)modifiers;
        }

        private VirtualKey MapKeys(KeyboardState ks)
        {

            if (ks.IsKeyPressed(Keys.Back)) return VirtualKey.BACK;
            if (ks.IsKeyPressed(Keys.Delete)) return VirtualKey.DELETE;
            if (ks.IsKeyPressed(Keys.Tab)) return VirtualKey.TAB;
            if (ks.IsKeyPressed(Keys.Clear)) return VirtualKey.CLEAR;
            if (ks.IsKeyPressed(Keys.Enter)) return VirtualKey.RETURN;
            if (ks.IsKeyPressed(Keys.Pause)) return VirtualKey.PAUSE;
            if (ks.IsKeyPressed(Keys.Escape)) return VirtualKey.ESCAPE;
            if (ks.IsKeyPressed(Keys.Space)) return VirtualKey.SPACE;
            if (ks.IsKeyPressed(Keys.NumberPad0)) return VirtualKey.NUMPAD0;
            if (ks.IsKeyPressed(Keys.NumberPad1)) return VirtualKey.NUMPAD1;
            if (ks.IsKeyPressed(Keys.NumberPad2)) return VirtualKey.NUMPAD2;
            if (ks.IsKeyPressed(Keys.NumberPad3)) return VirtualKey.NUMPAD3;
            if (ks.IsKeyPressed(Keys.NumberPad4)) return VirtualKey.NUMPAD4;
            if (ks.IsKeyPressed(Keys.NumberPad5)) return VirtualKey.NUMPAD5;
            if (ks.IsKeyPressed(Keys.NumberPad6)) return VirtualKey.NUMPAD6;
            if (ks.IsKeyPressed(Keys.NumberPad7)) return VirtualKey.NUMPAD7;
            if (ks.IsKeyPressed(Keys.NumberPad8)) return VirtualKey.NUMPAD8;
            if (ks.IsKeyPressed(Keys.NumberPad9)) return VirtualKey.NUMPAD9;
            if (ks.IsKeyPressed(Keys.Period)) return VirtualKey.DECIMAL;
            if (ks.IsKeyPressed(Keys.Divide)) return VirtualKey.DIVIDE;
            if (ks.IsKeyPressed(Keys.Multiply)) return VirtualKey.MULTIPLY;
            if (ks.IsKeyPressed(Keys.Subtract)) return VirtualKey.SUBTRACT;
            if (ks.IsKeyPressed(Keys.Add)) return VirtualKey.ADD;
            //case ks.KeypadEnter: return VirtualKey.SEPARATOR;
            //if (ks.IsKeyPressed(Keys.KeypadEquals)) return VirtualKey.UNKNOWN;
            if (ks.IsKeyPressed(Keys.Up)) return VirtualKey.UP;
            if (ks.IsKeyPressed(Keys.Down)) return VirtualKey.DOWN;
            if (ks.IsKeyPressed(Keys.Right)) return VirtualKey.RIGHT;
            if (ks.IsKeyPressed(Keys.Left)) return VirtualKey.LEFT;
            if (ks.IsKeyPressed(Keys.Insert)) return VirtualKey.INSERT;
            if (ks.IsKeyPressed(Keys.Home)) return VirtualKey.HOME;
            if (ks.IsKeyPressed(Keys.End)) return VirtualKey.END;
            if (ks.IsKeyPressed(Keys.PageUp)) return VirtualKey.PRIOR;
            if (ks.IsKeyPressed(Keys.PageDown)) return VirtualKey.NEXT;
            if (ks.IsKeyPressed(Keys.F1)) return VirtualKey.F1;
            if (ks.IsKeyPressed(Keys.F2)) return VirtualKey.F2;
            if (ks.IsKeyPressed(Keys.F3)) return VirtualKey.F3;
            if (ks.IsKeyPressed(Keys.F4)) return VirtualKey.F4;
            if (ks.IsKeyPressed(Keys.F5)) return VirtualKey.F5;
            if (ks.IsKeyPressed(Keys.F6)) return VirtualKey.F6;
            if (ks.IsKeyPressed(Keys.F7)) return VirtualKey.F7;
            if (ks.IsKeyPressed(Keys.F8)) return VirtualKey.F8;
            if (ks.IsKeyPressed(Keys.F9)) return VirtualKey.F9;
            if (ks.IsKeyPressed(Keys.F10)) return VirtualKey.F10;
            if (ks.IsKeyPressed(Keys.F11)) return VirtualKey.F11;
            if (ks.IsKeyPressed(Keys.F12)) return VirtualKey.F12;
            if (ks.IsKeyPressed(Keys.F13)) return VirtualKey.F13;
            if (ks.IsKeyPressed(Keys.F14)) return VirtualKey.F14;
            if (ks.IsKeyPressed(Keys.F15)) return VirtualKey.F15;
            if (ks.IsKeyPressed(Keys.Number0)) return VirtualKey.NUM_0;
            if (ks.IsKeyPressed(Keys.Number1)) return VirtualKey.NUM_1;
            if (ks.IsKeyPressed(Keys.Number2)) return VirtualKey.NUM_2;
            if (ks.IsKeyPressed(Keys.Number3)) return VirtualKey.NUM_3;
            if (ks.IsKeyPressed(Keys.Number4)) return VirtualKey.NUM_4;
            if (ks.IsKeyPressed(Keys.Number5)) return VirtualKey.NUM_5;
            if (ks.IsKeyPressed(Keys.Number6)) return VirtualKey.NUM_6;
            if (ks.IsKeyPressed(Keys.Number7)) return VirtualKey.NUM_7;
            if (ks.IsKeyPressed(Keys.Number8)) return VirtualKey.NUM_8;
            if (ks.IsKeyPressed(Keys.Number9)) return VirtualKey.NUM_9;
            //case ks.Exclaim: return VirtualKey.NUM_1;
            ////if (ks.IsKeyPressed(Keys.DoubleQuote)) return VirtualKey.OEM_7;
            ////if (ks.IsKeyPressed(Keys.Hash)) return VirtualKey.NUM_3;
            ////if (ks.IsKeyPressed(Keys.Dollar)) return VirtualKey.NUM_4;
            ////if (ks.IsKeyPressed(Keys.Ampersand)) return VirtualKey.NUM_7;
            ////if (ks.IsKeyPressed(Keys.Quote)) return VirtualKey.OEM_7;
            ////if (ks.IsKeyPressed(Keys.LeftParen)) return VirtualKey.NUM_9;
            ////if (ks.IsKeyPressed(Keys.RightParen)) return VirtualKey.NUM_0;
            ////if (ks.IsKeyPressed(Keys.Asterisk)) return VirtualKey.NUM_8;
            ////if (ks.IsKeyPressed(Keys.Plus)) return VirtualKey.OEM_PLUS;
            ////if (ks.IsKeyPressed(Keys.Comma)) return VirtualKey.OEM_COMMA;
            ////if (ks.IsKeyPressed(Keys.Minus)) return VirtualKey.OEM_MINUS;
            ////if (ks.IsKeyPressed(Keys.Period)) return VirtualKey.OEM_PERIOD;
            ////if (ks.IsKeyPressed(Keys.Slash)) return VirtualKey.OEM_2;
            ////if (ks.IsKeyPressed(Keys.Colon)) return VirtualKey.OEM_1;
            ////if (ks.IsKeyPressed(Keys.Semicolon)) return VirtualKey.OEM_1;
            ////if (ks.IsKeyPressed(Keys.Less)) return VirtualKey.OEM_COMMA;
            ////if (ks.IsKeyPressed(Keys.Equals)) return VirtualKey.OEM_PLUS;
            ////if (ks.IsKeyPressed(Keys.Greater)) return VirtualKey.OEM_PERIOD;
            ////if (ks.IsKeyPressed(Keys.Question)) return VirtualKey.OEM_2;
            ////if (ks.IsKeyPressed(Keys.At)) return VirtualKey.NUM_2;
            ////if (ks.IsKeyPressed(Keys.LeftBracket)) return VirtualKey.OEM_4;
            ////if (ks.IsKeyPressed(Keys.Backslash)) return VirtualKey.OEM_102;
            ////if (ks.IsKeyPressed(Keys.RightBracket)) return VirtualKey.OEM_6;
            ////if (ks.IsKeyPressed(Keys.Caret)) return VirtualKey.NUM_6;
            ////if (ks.IsKeyPressed(Keys.Underscore)) return VirtualKey.OEM_MINUS;
            ////if (ks.IsKeyPressed(Keys.BackQuote)) return VirtualKey.OEM_3;
            if (ks.IsKeyPressed(Keys.A)) return VirtualKey.A;
            if (ks.IsKeyPressed(Keys.B)) return VirtualKey.B;
            if (ks.IsKeyPressed(Keys.C)) return VirtualKey.C;
            if (ks.IsKeyPressed(Keys.D)) return VirtualKey.D;
            if (ks.IsKeyPressed(Keys.E)) return VirtualKey.E;
            if (ks.IsKeyPressed(Keys.F)) return VirtualKey.F;
            if (ks.IsKeyPressed(Keys.G)) return VirtualKey.G;
            if (ks.IsKeyPressed(Keys.H)) return VirtualKey.H;
            if (ks.IsKeyPressed(Keys.I)) return VirtualKey.I;
            if (ks.IsKeyPressed(Keys.J)) return VirtualKey.J;
            if (ks.IsKeyPressed(Keys.K)) return VirtualKey.K;
            if (ks.IsKeyPressed(Keys.L)) return VirtualKey.L;
            if (ks.IsKeyPressed(Keys.M)) return VirtualKey.M;
            if (ks.IsKeyPressed(Keys.N)) return VirtualKey.N;
            if (ks.IsKeyPressed(Keys.O)) return VirtualKey.O;
            if (ks.IsKeyPressed(Keys.P)) return VirtualKey.P;
            if (ks.IsKeyPressed(Keys.Q)) return VirtualKey.Q;
            if (ks.IsKeyPressed(Keys.R)) return VirtualKey.R;
            if (ks.IsKeyPressed(Keys.S)) return VirtualKey.S;
            if (ks.IsKeyPressed(Keys.T)) return VirtualKey.T;
            if (ks.IsKeyPressed(Keys.U)) return VirtualKey.U;
            if (ks.IsKeyPressed(Keys.V)) return VirtualKey.V;
            if (ks.IsKeyPressed(Keys.W)) return VirtualKey.W;
            if (ks.IsKeyPressed(Keys.X)) return VirtualKey.X;
            if (ks.IsKeyPressed(Keys.Y)) return VirtualKey.Y;
            if (ks.IsKeyPressed(Keys.Z)) return VirtualKey.Z;
            if (ks.IsKeyPressed(Keys.NumberKeyLock)) return VirtualKey.NUMLOCK;
            if (ks.IsKeyPressed(Keys.CapitalLock)) return VirtualKey.CAPITAL;
            if (ks.IsKeyPressed(Keys.Scroll)) return VirtualKey.SCROLL;
            if (ks.IsKeyPressed(Keys.RightShift)) return VirtualKey.RSHIFT;
            if (ks.IsKeyPressed(Keys.LeftShift)) return VirtualKey.LSHIFT;
            if (ks.IsKeyPressed(Keys.RightControl)) return VirtualKey.RCONTROL;
            if (ks.IsKeyPressed(Keys.LeftControl)) return VirtualKey.LCONTROL;
            if (ks.IsKeyPressed(Keys.RightAlt)) return VirtualKey.RMENU;
            if (ks.IsKeyPressed(Keys.LeftAlt)) return VirtualKey.LMENU;
            //case ks.LeftApple: return VirtualKey.LWIN;
            if (ks.IsKeyPressed(Keys.LeftWindows)) return VirtualKey.LWIN;
            //case ks.RightApple: return VirtualKey.RWIN;
            if (ks.IsKeyPressed(Keys.RightWindows)) return VirtualKey.RWIN;
            //case ks.AltGr: return VirtualKey.UNKNOWN;
            if (ks.IsKeyPressed(Keys.Help)) return VirtualKey.HELP;
            if (ks.IsKeyPressed(Keys.Print)) return VirtualKey.PRINT;
            //case ks.SysReq: return VirtualKey.UNKNOWN;
            if (ks.IsKeyPressed(Keys.Pause)) return VirtualKey.PAUSE;
            if (ks.IsKeyPressed(Keys.Menu)) return VirtualKey.MENU;
            //default: return 0;
            else return 0;
        }

        private char? IsCharKeys(KeyboardState ks)
        {
            char? rslt = null;
            if (ks.IsKeyPressed(Keys.Space)) rslt = ' ';
            if (ks.IsKeyPressed(Keys.NumberPad0)) rslt = '0'; 
            if (ks.IsKeyPressed(Keys.NumberPad1)) rslt = '1'; 
            if (ks.IsKeyPressed(Keys.NumberPad2)) rslt = '2'; 
            if (ks.IsKeyPressed(Keys.NumberPad3)) rslt = '3'; 
            if (ks.IsKeyPressed(Keys.NumberPad4)) rslt = '4'; 
            if (ks.IsKeyPressed(Keys.NumberPad5)) rslt = '5'; 
            if (ks.IsKeyPressed(Keys.NumberPad6)) rslt = '6'; 
            if (ks.IsKeyPressed(Keys.NumberPad7)) rslt = '7'; 
            if (ks.IsKeyPressed(Keys.NumberPad8)) rslt = '8'; 
            if (ks.IsKeyPressed(Keys.NumberPad9)) rslt = '9';

            if (ks.IsKeyPressed(Keys.Number0)) rslt = '0';
            if (ks.IsKeyPressed(Keys.Number1)) rslt = '1';
            if (ks.IsKeyPressed(Keys.Number2)) rslt = '2';
            if (ks.IsKeyPressed(Keys.Number3)) rslt = '3';
            if (ks.IsKeyPressed(Keys.Number4)) rslt = '4';
            if (ks.IsKeyPressed(Keys.Number5)) rslt = '5';
            if (ks.IsKeyPressed(Keys.Number6)) rslt = '6';
            if (ks.IsKeyPressed(Keys.Number7)) rslt = '7';
            if (ks.IsKeyPressed(Keys.Number8)) rslt = '8';
            if (ks.IsKeyPressed(Keys.Number9)) rslt = '9';
            //case ks.Exclaim: return VirtualKey.NUM_1;
            ////if (ks.IsKeyPressed(Keys.DoubleQuote)) return VirtualKey.OEM_7;
            ////if (ks.IsKeyPressed(Keys.Hash)) return VirtualKey.NUM_3;
            ////if (ks.IsKeyPressed(Keys.Dollar)) return VirtualKey.NUM_4;
            ////if (ks.IsKeyPressed(Keys.Ampersand)) return VirtualKey.NUM_7;
            ////if (ks.IsKeyPressed(Keys.Quote)) return VirtualKey.OEM_7;
            ////if (ks.IsKeyPressed(Keys.LeftParen)) return VirtualKey.NUM_9;
            ////if (ks.IsKeyPressed(Keys.RightParen)) return VirtualKey.NUM_0;
            ////if (ks.IsKeyPressed(Keys.Asterisk)) return VirtualKey.NUM_8;
            ////if (ks.IsKeyPressed(Keys.Plus)) return VirtualKey.OEM_PLUS;
            ////if (ks.IsKeyPressed(Keys.Comma)) return VirtualKey.OEM_COMMA;
            ////if (ks.IsKeyPressed(Keys.Minus)) return VirtualKey.OEM_MINUS;
            ////if (ks.IsKeyPressed(Keys.Period)) return VirtualKey.OEM_PERIOD;
            ////if (ks.IsKeyPressed(Keys.Slash)) return VirtualKey.OEM_2;
            ////if (ks.IsKeyPressed(Keys.Colon)) return VirtualKey.OEM_1;
            ////if (ks.IsKeyPressed(Keys.Semicolon)) return VirtualKey.OEM_1;
            ////if (ks.IsKeyPressed(Keys.Less)) return VirtualKey.OEM_COMMA;
            ////if (ks.IsKeyPressed(Keys.Equals)) return VirtualKey.OEM_PLUS;
            ////if (ks.IsKeyPressed(Keys.Greater)) return VirtualKey.OEM_PERIOD;
            ////if (ks.IsKeyPressed(Keys.Question)) return VirtualKey.OEM_2;
            ////if (ks.IsKeyPressed(Keys.At)) return VirtualKey.NUM_2;
            ////if (ks.IsKeyPressed(Keys.LeftBracket)) return VirtualKey.OEM_4;
            ////if (ks.IsKeyPressed(Keys.Backslash)) return VirtualKey.OEM_102;
            ////if (ks.IsKeyPressed(Keys.RightBracket)) return VirtualKey.OEM_6;
            ////if (ks.IsKeyPressed(Keys.Caret)) return VirtualKey.NUM_6;
            ////if (ks.IsKeyPressed(Keys.Underscore)) return VirtualKey.OEM_MINUS;
            ////if (ks.IsKeyPressed(Keys.BackQuote)) return VirtualKey.OEM_3;
            if (ks.IsKeyPressed(Keys.A)) rslt = 'a';
            if (ks.IsKeyPressed(Keys.B)) rslt = 'b';
            if (ks.IsKeyPressed(Keys.C)) rslt = 'c';
            if (ks.IsKeyPressed(Keys.D)) rslt = 'd';
            if (ks.IsKeyPressed(Keys.E)) rslt = 'e';
            if (ks.IsKeyPressed(Keys.F)) rslt = 'f';
            if (ks.IsKeyPressed(Keys.G)) rslt = 'g';
            if (ks.IsKeyPressed(Keys.H)) rslt = 'h';
            if (ks.IsKeyPressed(Keys.I)) rslt = 'i';
            if (ks.IsKeyPressed(Keys.J)) rslt = 'j';
            if (ks.IsKeyPressed(Keys.K)) rslt = 'k';
            if (ks.IsKeyPressed(Keys.L)) rslt = 'l';
            if (ks.IsKeyPressed(Keys.M)) rslt = 'm';
            if (ks.IsKeyPressed(Keys.N)) rslt = 'n';
            if (ks.IsKeyPressed(Keys.O)) rslt = 'o';
            if (ks.IsKeyPressed(Keys.P)) rslt = 'p';
            if (ks.IsKeyPressed(Keys.Q)) rslt = 'q';
            if (ks.IsKeyPressed(Keys.R)) rslt = 'r';
            if (ks.IsKeyPressed(Keys.S)) rslt = 's';
            if (ks.IsKeyPressed(Keys.T)) rslt = 't';
            if (ks.IsKeyPressed(Keys.U)) rslt = 'u';
            if (ks.IsKeyPressed(Keys.V)) rslt = 'v';
            if (ks.IsKeyPressed(Keys.W)) rslt = 'w';
            if (ks.IsKeyPressed(Keys.X)) rslt = 'x';
            if (ks.IsKeyPressed(Keys.Y)) rslt = 'y';
            if (ks.IsKeyPressed(Keys.Z)) rslt = 'z';
            return rslt;
        }

        private void RegisterEvents()
        {
            //var eventList = typeof(TouchGestures).GetEvents();
            //foreach (EventInfo ei in eventList)
            //{
            //    var eventInfo = typeof(TouchGestures).GetEvent(ei.Name);
            //    OnGuiEventHandler delegateForMethod = (sender, args, name) => OnGUI(sender, args, ei.Name);
            //    //ei.AddEventHandler(this, delegateForMethod);
            //    MethodInfo nm = ei.RaiseMethod;
            //}
        }
    }
}
