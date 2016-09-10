using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework.Graphics;

namespace HumbleGuns
{
    public class UIgrid : Grid
    {
        private Scene cs;
        private VirtualScreenManager vm;
        private ImageControl ic;
        public ImageControl Interface
        {
            get
            {   // Singleton ic
                ImageControl Sprite = this.entity.FindComponent<ImageControl>();
                if (Sprite == null)
                {
                    ic = new ImageControl(new Color("333333"), (int)this.Width, (int)this.Height);
                    //ic.Texture = new Texture2D() { Width = (int)this.Width, Height = (int)this.Height, };
                    this.entity.AddComponent(ic)
                               .AddComponent(new ImageControlRenderer());
                }
                return ic;
            }
            set
            {
                ic = value;
            }
        }

        public new Color BackgroundColor
        {
            get
            {
                Color color = Color.Transparent;
                ImageControl Sprite = this.entity.FindComponent<ImageControl>();
                if (Sprite != null)
                {
                    color = Sprite.TintColor;
                }
                return color;
            }

            set
            {
                ImageControl Sprite = this.entity.FindComponent<ImageControl>();
                if (Sprite != null)
                {
                    Sprite.TintColor = value;
                }
                else
                {
                    ic = new ImageControl(value, (int)this.Width, (int)this.Height);
                    //ic.Texture = new Texture2D() { Width = (int)this.Width, Height = (int)this.Height };
                    this.entity.AddComponent(ic)
                               .AddComponent(new ImageControlRenderer());
                }
            }
        }

        public UIgrid(Scene CurrentScene)
        {
            this.cs = CurrentScene;
            vm = cs.VirtualScreenManager;
            vm.Stretch = StretchMode.Uniform;
            SetUpGrid();
            BackgroundColor = new Color("#333333");
            //Entity ui = CurrentScene.EntityManager.Find<Entity>("UserInterface");

            //var button = new Button()   // debug only
            //{
            //    Text = string.Format("W: {0}, H: {1}", vm.VirtualWidth, vm.VirtualHeight),
            //    HorizontalAlignment = HorizontalAlignment.Left,
            //    VerticalAlignment = VerticalAlignment.Top,
            //    IsBorder = true,
            //    Margin = new Thickness(10),
            //    DrawOrder = -1
            //};
            //button.Width = this.Width;
            //button.Height = this.Height;

            //this.Add(button);
        }

        private void SetUpGrid()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.Width = vm.VirtualWidth;
            this.Height = vm.VirtualHeight;
            this.DrawOrder = -1;
            this.IsBorder = true;
            this.IsVisible = true;
            this.Margin = new Thickness(10);
        }
        public void Show() { this.IsVisible = true; }
        public void Hide() { this.IsVisible = false; }
    }
}
