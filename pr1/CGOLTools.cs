using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace pr1
{
    //Group of classes for drawing stuff on Canvas
    internal class CGOLTool
    {
        private Dictionary<string, int> _uniformLocations;
        public CGOLCanvas basecanvas;
        private int draw_shader;
        private int program;
        public string tool_shader_path;

        public virtual void Load()
        {
            var tool_shader = File.ReadAllText(tool_shader_path);
            draw_shader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(draw_shader, tool_shader);
            GL.CompileShader(draw_shader);
            GL.GetShader(draw_shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int) All.True)
            {
                Console.WriteLine(tool_shader);
                var infoLog = GL.GetShaderInfoLog(draw_shader);
                Console.WriteLine(infoLog);
                throw new Exception($"Error occurred whilst compiling Shader({draw_shader}).\n\n{infoLog}");
            }

            program = GL.CreateProgram();
            GL.AttachShader(program, draw_shader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var c2ode);
            if (c2ode != (int) All.True) throw new Exception($"Error occurred whilst linking Program({program})");
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(program, i, out _, out _);
                var location = GL.GetUniformLocation(program, key);
                _uniformLocations.Add(key, location);
            }

            if (_uniformLocations.ContainsKey("resolution_x")) SetInt("resolution_x", basecanvas.resolution.Width);
            if (_uniformLocations.ContainsKey("resolution_y")) SetInt("resolution_y", basecanvas.resolution.Height);

            GL.DetachShader(program, draw_shader);
            GL.DeleteShader(draw_shader);
        }

        public virtual void Compute()
        {
            GL.UseProgram(program);
            GL.DispatchCompute(basecanvas.resolution.Width * 2, basecanvas.resolution.Height, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        public virtual void SetFloat(string name, float data)
        {
            GL.UseProgram(program);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public virtual void SetInt(string name, int data)
        {
            GL.UseProgram(program);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public virtual void SetVector4(string name, Vector4 data)
        {
            GL.UseProgram(program);
            GL.Uniform4(_uniformLocations[name], data);
        }

        public virtual void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(program);
            GL.Uniform2(_uniformLocations[name], data);
        }
    }

    internal class CGOLPixelPicker : CGOLTool
    {
        public CGOLPixelPicker(CGOLCanvas canvas)
        {
            basecanvas = canvas;
        }

        public override void Load()
        {
            tool_shader_path = "shaders/tools/pixel_inverter.vert";
            base.Load();
        }
    }

    internal class CGOLPencil : CGOLTool
    {
        public CGOLPencil(CGOLCanvas canvas)
        {
            basecanvas = canvas;
        }

        public override void Load()
        {
            tool_shader_path = "shaders/tools/pencil.vert";
            base.Load();
        }
    }

    internal class CGOLCanvasClearer : CGOLTool
    {
        public CGOLCanvasClearer(CGOLCanvas canvas)
        {
            basecanvas = canvas;
        }

        public override void Load()
        {
            tool_shader_path = "shaders/tools/prank/canvas_clearer.vert";
            base.Load();
        }
    }

    internal class ConwayGOL : CGOLTool
    {
        public CGOLTool swapper;

        public ConwayGOL(CGOLCanvas canvas)
        {
            swapper = new CGOLTool();
            basecanvas = canvas;
            swapper.basecanvas = canvas;
        }

        public override void Load()
        {
            swapper.tool_shader_path = "shaders/tools/prank/swaphalves.vert";
            tool_shader_path = "shaders/tools/prank/conv_gol_grad.vert";
            base.Load();
            swapper.Load();
        }

        public override void Compute()
        {
            base.Compute();
            swapper.Compute();
        }
    }

    internal class CGOLRandomizer : CGOLTool
    {
        public CGOLRandomizer(CGOLCanvas canvas)
        {
            basecanvas = canvas;
        }

        public override void Load()
        {
            tool_shader_path = "shaders/tools/randomizer.vert";
            base.Load();
        }
        
        public override void Compute()
        {
            float seed = (float) new Random().NextDouble();
            base.SetFloat("seed", seed);
            base.Compute();
        }
    }
}