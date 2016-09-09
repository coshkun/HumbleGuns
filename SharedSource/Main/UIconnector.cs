using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Awesomium.Core;
using WaveEngine.Framework;
using WaveEngine.Components.UI;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework.Graphics;
using System.Runtime.InteropServices;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;

namespace HumbleGuns
{
    [DataContract]
    public class UIconnector : Behavior
    {
        // Public Variables
        public int width = 512;
        public int height = 512;
        public string initialURL = "http://google.com";

        //private UIgrid ug;
        private Entity ui;

        private ImageControl ic; // this will be our render target on UI Entity.
        private Transform2D uiLocation;
        private static Entity webCoreHelper;
        
        private bool isFocused = false;
        private bool isScrollable = false;
        private static List<WebView> webViewList = new List<WebView>();
        private WebView webView;
        private Texture2D texture;
        private byte[] Pixels;
        private GCHandle PixelsHandle;

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
            //CurrentScene.EntityManager.Add(new UIgrid(CurrentScene));

            //EntityManager.Add(ug);


            //CurrentScene.EntityManager.Add(ug);
            //ui.AddComponent(ic);
            //var sp = ui.FindComponent<Sprite>(true);
            //sp.TexturePath = WaveContent.Assets.Textures.DefaultTexture_png;
            ////ui.AddComponent(new SpriteRenderer(DefaultLayers.GUI));
            //this.uiLocation = ui.FindComponent<Transform2D>(true);
            //uiLocation.Position = new WaveEngine.Common.Math.Vector2(0, 0);
            //ui = new UIgrid(currentScene).Entity;
        }

        protected override void Update(TimeSpan gameTime)
        {
        }
    }
}
