#pragma warning disable CS8981
global using floatv =
#if DOUBLE
    System.Double;
#else
	System.Single;
#endif

global using Maths =
#if DOUBLE
    System.Math;
#else
    System.MathF;
#endif
#pragma warning restore CS8981

using System;
using Zene.Windowing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Graphics;
using Zene.Structs;

namespace PHYSICS
{
    class Program : Window
    {
        static void Main(string[] args)
        {
            Core.Init();
            
            Manager m = new Manager(new Bounds(-150, 150, 100, -100), 1);
            m.AddWall(new Wall((-150, 70), (-120, 100), 1));
            m.AddWall(new Wall((-150, -70), (-120, -100), 1));
            m.AddWall(new Wall((150, 70), (120, 100), 1));
            m.AddWall(new Wall((150, -70), (120, -100), 1));
            
            // m.AddBall(new Ball(0f, (20f, 5f), 0.8f, 10f, 1f));
            // m.AddBall(new Ball((30, 0), (-10f, 10f), 0.8f, 10f, 1f));
            // m.AddBall(new Ball((-30, 0), (-5f, 15f), 0.8f, 10f, 1f));
            // m.AddBall(new Ball((0, -30), (10f, 10f), 0.8f, 10f, 1f));
            
            m.AddBall(new Ball((-70, 0), (500f, 5f), 0.8f, 10f, 1f));
            m.AddBall(new Ball((30, 0), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((55, 0), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((80, 0), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((55, 25), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((55, -25), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((80, 25), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((80, -25), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((80, 50), 0, 0.8f, 10f, 1f));
            m.AddBall(new Ball((80, -50), 0, 0.8f, 10f, 1f));
            m.ElapseTo(1000f);
            
            Program p = new Program(800, 500, "WEEEEEEEEE", m);
            p.Run();
            p.Dispose();
            
            // Test t = new Test(800, 500, "sdrtfgyh");
            // t.Run();
            // t.Dispose();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, Manager m)
            : base(width, height, title)
        {
            _total = m.Fill(_renders);
            _walls = m.Walls;
            _tr = new TextRenderer();
            _et = m.Elapsed;
        }
        
        private floatv _time = 0;
        private readonly floatv _et;
        private bool _playing = false;
        private bool _reversed = false;
        private int _total;
        private List<ObjectRender> _renders = new List<ObjectRender>(20);
        private List<Wall> _walls;
        private TextRenderer _tr;
        
        private bool ElapseTime(floatv time)
        {
            time += _time;
            if (time < 0 || time > _et) { return false; }
            
            Span<ObjectRender> span = CollectionsMarshal.AsSpan(_renders);
            for (int i = 0; i < span.Length; i++)
            {
                span[i].SetTime(time);
            }
            _time = time;
            
            return true;
        }

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);
            
            e.Context.Framebuffer.Clear(BufferBit.Colour);
            
            if (_playing)
            {
                floatv dt = e.DeltaTime;
                if (_reversed) { dt = -dt; }
                if (this[Keys.LeftShift])
                {
                    dt *= 2;
                }
                if (this[Keys.LeftAlt])
                {
                    dt *= 4;
                }
                if (this[Keys.Q])
                {
                    dt *= 10;
                }
                _playing = ElapseTime(dt);
            }
            
            Vector2I s = Size;
            e.Context.Projection = Matrix4.CreateOrthographic(s.X, s.Y, 0f, 1f);
            e.Context.Model = Matrix.Identity;
            
            Span<ObjectRender> span = CollectionsMarshal.AsSpan(_renders);
            for (int i = 0; i < span.Length; i++)
            {
                e.Context.Render(span[i]);
            }
            
            Span<Wall> walls = CollectionsMarshal.AsSpan(_walls);
            for (int i = 0; i < walls.Length; i++)
            {
                Wall w = walls[i];
                DrawLine(e.Context, w.A, w.B, 5);
            }
            
            e.Context.Model = new STMatrix(15, (0, s.Y * 0.5f - 15));
            if (this[Keys.T])
            {
                _tr.DrawCentred(e.Context, $"{_total}", Shapes.SampleFont, 0, 0);
            }
            else
            {
                _tr.DrawCentred(e.Context, $"{_time}", Shapes.SampleFont, 0, 0);
            }
        }
        
        private static void DrawLine(IDrawingContext dc, Vector2 a, Vector2 b, floatv thick)
        {
            Vector2 diff = b - a;
            floatv length = diff.Length;
            
            floatv div = 1 / length;
            floatv cos = diff.X * div;
            floatv sin = diff.Y * div;
            Matrix4 mat = new Matrix4(
                (cos, sin, 0, 0),
                (-sin, cos, 0, 0),
                (0, 0, 1, 0),
                (0, 0, 0, 1)
            );
            dc.Model = mat * Matrix4.CreateTranslation((a + b) * 0.5f);
            dc.RenderState.PostMatrixMods = false;
            dc.DrawRoundedBox(
                new Box(0, (length + thick, thick)), ColourF.BlueViolet, 0.5f);
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.Space])
            {
                _playing = !_playing;
                return;
            }
            if (e[Keys.R])
            {
                _reversed = !_reversed;
                return;
            }
            if (e[Keys.Right])
            {
                ElapseTime(0.01f);
                return;
            }
            if (e[Keys.Left])
            {
                ElapseTime(-0.01f);
                return;
            }
            if (e[Keys.Enter])
            {
                ElapseTime(-_time);
                return;
            }
        }
    }
}
