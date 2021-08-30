using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using SDPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace pr1
{
    internal class CGOLScene
    {
        public static float[] fullscreenverticies =
        {
            1f, 1f, 0.0f, 1.0f, 1.0f,
            1f, -1f, 0.0f, 1.0f, 0.0f,
            -1f, -1f, 0.0f, 0.0f, 0.0f,
            -1f, 1f, 0.0f, 0.0f, 1.0f
        };

        private bool HideUI = false;
        private Button b_clear;
        private Button b_play;
        private Button b_step;
        private Button b_stop;
        private readonly Window base_w;
        private CGOLCanvas canvas;
        public List<Plane> elements = new List<Plane>();

        private bool H_press_prev;
        private bool N_press_prev;
        private bool play;
        private bool space_press_prev;

        public CGOLScene(Window base_window)
        {
            base_w = base_window;
        }

        public void Render()
        {
            canvas.Render();
            if (!HideUI)
            {
                b_play.Render();
                b_stop.Render();
                b_step.Render();
                b_clear.Render();
            }
        }

        public void Load()
        {
            canvas = new CGOLCanvas(base_w);
            canvas.Load();

            canvas.tool2.SetFloat("brush_width", 1f);
            canvas.tool2.SetVector4("brush_col", new Vector4(1.0f, 1.0f, 1.0f, 1f));

            b_play = new Button("content/ui/play.png", Play, base_w);
            b_stop = new Button("content/ui/stop.png", Stop, base_w);
            b_step = new Button("content/ui/step.png", Step, base_w);
            b_clear = new Button("content/ui/clear.png", Clear, base_w);

            b_play.ReshapeWithCoords(0.98f, 0.96f, 0.76f, 0.86f);
            b_play.Load();

            b_step.ReshapeWithCoords(0.98f, 0.7f, 0.76f, 0.6f);
            b_step.Load();

            b_stop.ReshapeWithCoords(0.98f, 0.83f, 0.76f, 0.73f);
            b_stop.Load();

            b_clear.ReshapeWithCoords(0.98f, 0.57f, 0.76f, 0.47f);
            b_clear.Load();
        }

        public void Unload()
        {
            canvas.Unload();
            b_play.Unload();
            b_stop.Unload();
            b_step.Unload();
            b_clear.Unload();
        }

        public void Update(Window wnd, MouseState mouse, KeyboardState keyboard)
        {
            if (play) canvas.GOL();
            canvas.Update(wnd, mouse);
            if (!HideUI)
            {
                b_play.Update(mouse);
                b_stop.Update(mouse);
                b_step.Update(mouse);
                b_clear.Update(mouse);
            }

            if (keyboard.IsKeyDown(Key.D)) canvas.ResetZoom();
            if (keyboard.IsKeyDown(Key.Space) && !space_press_prev)
            {
                space_press_prev = true;
                play = !play;
            }
            else if (keyboard.IsKeyUp(Key.Space))
            {
                space_press_prev = false;
            }
            
            if (keyboard.IsKeyDown(Key.H) && !H_press_prev)
            {
                H_press_prev = true;
                HideUI = !HideUI;
            }
            else if (keyboard.IsKeyUp(Key.H))
            {
                H_press_prev = false;
            }

            if (keyboard.IsKeyDown(Key.N) && !N_press_prev)
            {
                N_press_prev = true;
                canvas.GOL();
            }
            else if (keyboard.IsKeyUp((Key.N)))
            {
                N_press_prev = false;
            }

            if (keyboard.IsKeyDown(Key.R)) canvas.Randomize();
            if (keyboard.IsKeyDown(Key.F12))
            {
                TakeScreenshot(new Size(wnd.Width, wnd.Height), "shot.png");
            }

            canvas.PassScroll(wnd, mouse);
        }

        //ui
        public bool Stop()
        {
            play = false;
            return true;
        }

        public bool Step()
        {
            canvas.GOL();
            return true;
        }

        public bool Play()
        {
            play = true;
            return true;
        }

        public bool Clear()
        {
            canvas.ClearCanvas();
            return true;
        }

        public void TakeScreenshot(Size resolution, string name)
        {
            GL.Flush();
            using (var bmp = new Bitmap(resolution.Width, resolution.Height, SDPixelFormat.Format32bppArgb))
            {
                var mem = bmp.LockBits(new Rectangle(0, 0, resolution.Width, resolution.Height), ImageLockMode.WriteOnly, SDPixelFormat.Format32bppArgb);
                GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
                GL.ReadPixels(0, 0, resolution.Width, resolution.Height, PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
                bmp.UnlockBits(mem);
                bmp.Save("screenshots/" + name, ImageFormat.Png);
            }
        }
    }
}