using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
using WaveEngine.Common.Graphics;

namespace HumbleGuns
{
    [DataContract]
    public class UIgrid : Grid
    {
        private Scene cs;
        private VirtualScreenManager vm;
        private ImageControl ic;
        public ImageControl Interface { get; set; }

        public new Color BackgroundColor
        {
            get
            {
                Color color = Color.White;

                ImageControl imageControl = this.entity.FindComponent<ImageControl>();
                if (imageControl != null)
                {
                    color = imageControl.TintColor;
                }
                else
                {
                    throw new Exception("This panel haven't background assigned");
                }

                return color;
            }

            set
            {
                ImageControl imageControl = this.entity.FindComponent<ImageControl>();
                if (imageControl != null)
                {
                    imageControl.TintColor = value;
                }
                else
                {
                    this.entity.AddComponent(new ImageControl(value, (int)this.Width, (int)this.Height))
                               .AddComponent(new ImageControlRenderer());
                }
            }
        }

        public UIgrid(Scene CurrentScene) : base()
        {
            this.cs = CurrentScene;
            vm = cs.VirtualScreenManager;
            vm.Stretch = StretchMode.UniformToFill;

            this.SetUpGrid();
            // Set image background
            ic = new ImageControl(WaveContent.Assets.Textures.DefaultTexture_png);
            this.entity.AddComponent(ic)
                       .AddComponent(new ImageControlRenderer());
            this.Interface = ic;

            this.IsVisible = false;
        }

        private void SetUpGrid()
        {
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.Width = vm.VirtualWidth;
            this.Height = vm.VirtualHeight;
            this.Margin = new Thickness(10);
            this.IsBorder = false;
        }

        public void Show() { this.IsVisible = true;  }
        public void Hide() { this.IsVisible = false; }
    }
}
