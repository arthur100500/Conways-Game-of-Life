using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace pr1
{
    internal class CGOLCanvas
    {
        public readonly Size resolution = new Size(960, 540);

        private GameWindow basewnd;
        private CGOLCanvasClearer clearer;
        private ConwayGOL gol;

        private readonly Shader outp_shader =
            new Shader("shaders/canvas/out_shader.vert", "shaders/canvas/out_shader.frag");

        private Plane plane;
        private bool pressed;

        private int prev;
        private float[] prev_drag_pos;
        private int prev_pos_x = -1;
        private int prev_pos_y = -1;
        private int tex_output;
        public CGOLTool tool;
        public CGOLTool tool2;
        public CGOLTool rndtool;

        public CGOLCanvas(GameWindow base_window)
        {
            basewnd = base_window;
        }

        public void Load()
        {
            tool = new CGOLPixelPicker(this);
            tool2 = new CGOLPencil(this);
            rndtool = new CGOLRandomizer(this);
            gol = new ConwayGOL(this);
            clearer = new CGOLCanvasClearer(this);
            tool.Load();
            tool2.Load();
            rndtool.Load();
            gol.Load();
            clearer.Load();
            TextureSetup();
            GeometrySetup();
            ClearCanvas();
        }

        private void GeometrySetup()
        {
            plane = new Plane(Misc.fullscreenverticies, outp_shader, tex_output);
            plane.Load();
            //plane.ReshapeWithCoords(0.05f, 0.95f, 0.95f, 0.05f);
        }

        private void TextureSetup()
        {
            tex_output = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex_output);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, resolution.Width * 2,
                resolution.Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.BindImageTexture(0, tex_output, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
        }

        public void Render()
        {
            plane.Render();
        }

        public void Unload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void Update(Window wnd, MouseState mouseinfo)
        {
            if (mouseinfo.IsButtonDown(MouseButton.Left) && !pressed)
            {
                pressed = true;
                var pos = plane.GetZoomRelativeCursorPosition(wnd, mouseinfo.X, mouseinfo.Y);
                var x_pos = (int) (pos[0] * resolution.Width);
                var y_pos = (int) (pos[1] * resolution.Height);
                if (prev_pos_x != -1 || prev_pos_y != -1)
                {
                    //tool.SetVector2("brush_pos1", new Vector2(prev_pos_x, prev_pos_y));
                }

                tool.SetVector2("brush_pos2", new Vector2(x_pos, y_pos));
                prev_pos_x = x_pos;
                prev_pos_y = y_pos;
                tool.Compute();
            }

            if (mouseinfo.IsButtonUp(MouseButton.Left)) pressed = false;
            if (mouseinfo.IsButtonDown(MouseButton.Right))
            {
                var pos = plane.GetZoomRelativeCursorPosition(wnd, mouseinfo.X, mouseinfo.Y);
                var x_pos = (int) (pos[0] * resolution.Width);
                var y_pos = (int) (pos[1] * resolution.Height);
                if (prev_pos_x != -1 || prev_pos_y != -1)
                    tool2.SetVector2("brush_pos1", new Vector2(prev_pos_x, prev_pos_y));
                else
                    tool2.SetVector2("brush_pos1", new Vector2(x_pos, y_pos));

                tool2.SetVector2("brush_pos2", new Vector2(x_pos, y_pos));
                prev_pos_x = x_pos;
                prev_pos_y = y_pos;
                tool2.Compute();
            }

            else
            {
                prev_pos_x = -1;
                prev_pos_y = -1;
            }
        }

        public Bitmap GetBitmap()
        {
            int fboId;
            GL.Ext.GenFramebuffers(1, out fboId);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboId);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                TextureTarget.Texture2D, tex_output, 0);

            var b = new Bitmap(resolution.Width * 2, resolution.Height);
            var bits = b.LockBits(new Rectangle(0, 0, resolution.Width * 2, resolution.Height), ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.ReadPixels(0, 0, resolution.Width * 2, resolution.Height, PixelFormat.Bgra, PixelType.UnsignedByte,
                bits.Scan0);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.Ext.DeleteFramebuffers(1, ref fboId);
            b.UnlockBits(bits);
            b.Save("canvasSave.png");
            return b;
        }

        public void ClearCanvas()
        {
            clearer.Compute();
        }

        public void GOL()
        {
            gol.Compute();
        }

        public void PassScroll(Window wnd, MouseState mouse)
        {
            if (mouse.ScrollWheelValue > prev) plane.ZoomIn(plane.GetZoomRelativeCursorPosition(wnd, mouse.X, mouse.Y));

            if (mouse.ScrollWheelValue < prev)
                plane.ZoomOut(plane.GetZoomRelativeCursorPosition(wnd, mouse.X, mouse.Y));

            if (mouse.IsButtonDown(MouseButton.Middle))
                plane.ZoomDrag(plane.GetZoomRelativeCursorPosition(wnd, mouse.X, mouse.Y), prev_drag_pos);

            prev_drag_pos = plane.GetZoomRelativeCursorPosition(wnd, mouse.X, mouse.Y);
            prev = mouse.ScrollWheelValue;
        }

        public void ResetZoom()
        {
            plane.ZoomReset();
        }

        public void Randomize()
        {
            rndtool.Compute();
        }
    }
}