#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.Components.UI;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
#endregion

namespace HumbleGuns
{
    public class MyScene : Scene
    {
        public static Entity uiTarget;

        private Game _game;
        public MyScene() : base() { }
        public MyScene(Game game) : base() { _game = game; }

        protected override void CreateScene()
        {
            this.Load(WaveContent.Scenes.MyScene);
            var ui = EntityManager.Find<Entity>("UserInterface");
            var uic = new UIconnector(this);
            ui.AddComponent(uic);

            //var ui = EntityManager.Find<Entity>("UserInterface");
            //Entity uig = new UIgrid(this).Entity;
            //uig.AddComponent(new UIconnector(this));
            //uiTarget.AddComponent(new UIconnector(this));
            //uiTarget.AddChild(uig);
            //uiTarget.AddComponent(new Transform2D() { X=0,Y=0, TranformMode= Transform2D.TransformMode.Standard });
            //uiTarget.AddComponent(new Sprite(WaveContent.Assets.Textures.DefaultTexture_png));
            //uiTarget.AddComponent(new SpriteRenderer(DefaultLayers.GUI, AddressMode.LinearClamp));
            //EntityManager.Add(uiTarget);
        }

        private void OnUIClick(object sender, EventArgs e)
        {
        }
    }
}
