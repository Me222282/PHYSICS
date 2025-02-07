using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Zene.Graphics;
using Zene.Structs;
using Zene.Windowing;

namespace PHYSICS
{
    public class Test : Window
    {
        // public static ConcurrentQueue<(Vector2, Box, Box, Box)> Issues = new ConcurrentQueue<(Vector2, Box, Box, Box)>();
        
        // private static void LoadString(string[] lines, out List<PBox> a, out List<PBox> b, out List<PBox> c)
        // {
        //     a = new List<PBox>();
        //     b = new List<PBox>();
        //     c = new List<PBox>();
            
        //     Vector2 v = 0d;
        //     Box aBox = new Box();
        //     Box bBox = new Box();
        //     Box cBox = new Box();
            
        //     int state = 0;
            
        //     for (int i = 0; i < lines.Length; i++)
        //     {
        //         string line = lines[i];
        //         if (line == null || line.Length == 0)
        //         {
        //             a.Add(new PBox()
        //             {
        //                 Location = aBox.Centre,
        //                 Size = aBox.Size,
        //                 Velocity = v
        //             });
        //             b.Add(new PBox()
        //             {
        //                 Location = bBox.Centre,
        //                 Size = bBox.Size,
        //                 Static = true
        //             });
        //             c.Add(new PBox()
        //             {
        //                 Location = cBox.Centre,
        //                 Size = cBox.Size,
        //                 Static = true
        //             });
        //             state = 0;
        //             continue;
        //         }
                
        //         if (line[0] == 'X')
        //         {
        //             int i1 = line.IndexOf('Y');
        //             string x = line.Substring(2, i1 - 4).Trim();
        //             string y = line.Substring(i1 + 2).Trim();
                    
        //             v = (double.Parse(x), double.Parse(y));
        //             continue;
        //         }
                
        //         if (state == 1)
        //         {
        //             int i1 = line.IndexOf('R');
        //             int i2 = line.IndexOf('T');
        //             int i3 = line.IndexOf('B');
        //             string l = line.Substring(5, i1 - 7).Trim();
        //             string r = line.Substring(i1 + 6, i2 - i1 - 8).Trim();
        //             string t = line.Substring(i2 + 4, i3 - i2 - 6).Trim();
        //             string bo = line.Substring(i3 + 7).Trim();
                    
        //             bBox = new Box(
        //                 double.Parse(l),
        //                 double.Parse(r),
        //                 double.Parse(t),
        //                 double.Parse(bo)
        //             );
        //             state = 2;
        //             continue;
        //         }
        //         if (state == 2)
        //         {
        //             int i1 = line.IndexOf('R');
        //             int i2 = line.IndexOf('T');
        //             int i3 = line.IndexOf('B');
        //             string l = line.Substring(5, i1 - 7).Trim();
        //             string r = line.Substring(i1 + 6, i2 - i1 - 8).Trim();
        //             string t = line.Substring(i2 + 4, i3 - i2 - 6).Trim();
        //             string bo = line.Substring(i3 + 7).Trim();
                    
        //             cBox = new Box(
        //                 double.Parse(l),
        //                 double.Parse(r),
        //                 double.Parse(t),
        //                 double.Parse(bo)
        //             );
        //             continue;
        //         }
        //         else
        //         {
        //             int i1 = line.IndexOf('R');
        //             int i2 = line.IndexOf('T');
        //             int i3 = line.IndexOf('B');
        //             string l = line.Substring(5, i1 - 7).Trim();
        //             string r = line.Substring(i1 + 6, i2 - i1 - 8).Trim();
        //             string t = line.Substring(i2 + 4, i3 - i2 - 6).Trim();
        //             string bo = line.Substring(i3 + 7).Trim();
                    
        //             aBox = new Box(
        //                 double.Parse(l),
        //                 double.Parse(r),
        //                 double.Parse(t),
        //                 double.Parse(bo)
        //             );
        //             state = 1;
        //         }
        //     }
        // }
        
        public Test(int width, int height, string title)
            : base(width, height, title)
        {
            DrawContext.RenderState.PostMatrixMods = false;
            DrawContext.RenderState.Blending = true;
            DrawContext.RenderState.SourceScaleBlending = BlendFunction.SourceAlpha;
            DrawContext.RenderState.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;
            
            // LoadString(File.ReadAllLines(title), out _tests, out _cols, out _cols2);
            
            _text = new TextRenderer();
            // _ph = new Physics();
            // _ph.Add(_test);
            // _ph.Add(_col);
        }
        
        private TextRenderer _text;
        
        private Vector2 _viewPan;
        private floatv _viewScale = 20;
        
        // private Physics _ph;
        private Ball _test = new Ball() { Radius = 0.5f };
        private Ball _col = new Ball() { Radius = 0.5f };
        // private List<PBox> _tests;
        // private List<PBox> _cols;
        // private List<PBox> _cols2;
        // private int _index = 0;

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);
            
            e.Context.Framebuffer.Clear(BufferBit.Depth);
            e.Context.Framebuffer.Clear(ColourF.White);
            
            Vector2 s = Size;
            e.Context.Projection = Matrix4.CreateOrthographic(s.X, s.Y, 0f, 1f);
            
            if (this[Keys.Up])
            {
                _viewPan -= new Vector2(0, 3) / _viewScale;
            }
            if (this[Keys.Down])
            {
                _viewPan += new Vector2(0, 3) / _viewScale;
            }
            if (this[Keys.Right])
            {
                _viewPan -= new Vector2(3, 0) / _viewScale;
            }
            if (this[Keys.Left])
            {
                _viewPan += new Vector2(3, 0) / _viewScale;
            }
            e.Context.View = Matrix4.CreateTranslation(_viewPan) * Matrix4.CreateScale(_viewScale);
            
            //ProblemView(e);
            Col2Test(e);
            //ResolveTest(e);
            //DynamicTest(e);
        }
        // private void ProblemView(FrameEventArgs e)
        // {
        //     PBox test = _tests[_index];
        //     PBox col = _cols[_index];
        //     PBox col2 = _cols2[_index];
        //     double dt = 1d / 60d;
            
        //     e.Context.DrawBox(test.Box, ColourF.Blue);
        //     e.Context.DrawBox(col.Box, ColourF.Yellow);
        //     e.Context.DrawBox(col2.Box, ColourF.Gold);
            
        //     Vector2 pos = test.Location;
        //     Vector2 vel = test.Velocity;
            
        //     //Vector2 np = PBox.ResolveOverlap(_test, _col, 1d);
        //     //_test.Location = np;
        //     bool c = PBox.StaticCollision(test, col2, dt, out double k, out bool side);
        //     PBox.StaticCollision(test, col, dt, out double k2, out _);
        //     // _test.Location = pos;
        //     // pos = np;
        //     if (c && side)
        //     {
        //         vel.Y *= k;
        //     }
        //     else if (c)
        //     {
        //         vel.X *= k;
        //     }
            
        //     pos += vel * dt;
            
        //     if (_enter)
        //     {
        //         test.Location = pos;
        //         _enter = false;
        //     }
            
        //     ColourF colour = ColourF.Green;
        //     // if (side)
        //     // {
        //     //     colour = ColourF.Orange;
        //     // }
        //     colour.A = 0.5f;
        //     e.Context.DrawBox(test.MovementBounds(dt), colour);
            
        //     colour = ColourF.Red;
        //     colour.A = 0.5f;
        //     e.Context.DrawBox(new Box(pos, test.Size), colour);
        //     // colour = ColourF.Orange;
        //     // colour.A = 0.5f;
        //     // e.Context.DrawBox(new Box(np, test.Size), colour);
        // }
        private void Col2Test(FrameEventArgs e)
        {
            // e.Context.DrawBox(_test.Box, ColourF.Blue);
            // e.Context.DrawBox(_col.Box, ColourF.Yellow);
            e.Context.DrawCircle(_test.Location, 2f * _test.Radius, ColourF.Blue);
            e.Context.DrawCircle(_col.Location, 2f * _col.Radius, ColourF.Yellow);
            
            // PBox.DynamicCollision(_test, _col, 1d, out double k, out _, out _, out _);
            floatv k = Collisions.BallBallLinear(_test, _col);
            if (k < 0d)
            {
                k = 1f;
            }
            
            ColourF colour = ColourF.Green;
            colour.A = 0.5f;
            // e.Context.DrawBox(_test.MovementBounds(1d), colour);
            // e.Context.DrawBox(_col.MovementBounds(1d), colour);
            
            colour = ColourF.Red;
            colour.A = 0.5f;
            e.Context.DrawCircle(_test.Location + (_test.Velocity * k), 2f * _test.Radius, colour);
            e.Context.DrawCircle(_col.Location + (_col.Velocity * k), 2f * _col.Radius, colour);
        }
        // private void ResolveTest(FrameEventArgs e)
        // {
        //     e.Context.DrawBox(_test.Box, ColourF.Blue);
        //     e.Context.DrawBox(_col.Box, ColourF.Yellow);
            
        //     ColourF colour = ColourF.Orange;
        //     colour.A = 0.5f;
        //     e.Context.DrawBox(_test.MovementBounds(1d), colour);
        //     e.Context.DrawBox(_col.MovementBounds(1d), colour);
            
        //     Vector2 velA = _test.Velocity;
        //     Vector2 velB = _col.Velocity;
        //     Vector2 p = (velA * _test.Mass) + (velB * _col.Mass);
        //     PBox.ResolveDynamicCollision(_test, _col, 0d, 0.5d);
            
        //     Vector2 np = (_test.Velocity * _test.Mass) + (_col.Velocity * _col.Mass);
        //     colour = p == np ? ColourF.Green : ColourF.Red;
        //     colour.A = 0.5f;
        //     e.Context.DrawBox(_test.MovementBounds(1d), colour);
        //     e.Context.DrawBox(_col.MovementBounds(1d), colour);
        //     _test.Velocity =  velA;
        //     _col.Velocity =  velB;
        // }
        // private void DynamicTest(FrameEventArgs e)
        // {
        //     e.Context.DrawBox(_test.Box, ColourF.Blue);
        //     e.Context.DrawBox(_col.Box, ColourF.Yellow);
            
        //     ColourF colour = ColourF.Orange;
        //     colour.A = 0.5f;
        //     e.Context.DrawBox(_test.MovementBounds(1d), colour);
        //     e.Context.DrawBox(_col.MovementBounds(1d), colour);
            
        //     if (_enter)
        //     {
        //         _ph.Update(1d / 60d);
        //         _enter = false;
        //     }
        // }
        
        // private bool _enter = false;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.Escape])
            {
                Close();
                return;
            }
            // if (e[Keys.Enter])
            // {
            //     _enter = true;
            //     return;
            // }
            // if (e[Keys.N])
            // {
            //     _index++;
            //     if (_index >= _tests.Count)
            //     {
            //         _index = 0;
            //     }
            //     return;
            // }
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            Vector2 pos = e.Location - (Size * 0.5f);
            pos /= _viewScale;
            
            if (this[Mods.Shift])
            {
                _col.Location = pos;
                return;
            }
            
            _test.Location = pos;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            Vector2 pos = e.Location - (Size * 0.5f);
            pos /= _viewScale;
            pos -= _viewPan;
            
            if (!this[Keys.V]) { return; }
            if (this[Mods.Shift])
            {
                _col.Velocity = pos - _col.Location;
                return;
            }
            
            _test.Velocity = pos - _test.Location;
            return;
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            
            if (this[Mods.Shift])
            {
                _col.Radius += e.DeltaY * 5f;
                return;
            }
            if (!this[Mods.Control])
            {
                _col.Radius += e.DeltaX * 5f;
                return;
            }
            
            floatv newZoom = _viewScale + (e.DeltaY * 0.1f * _viewScale);
            if (newZoom < 0) { return; }
            _viewScale = newZoom;
        }
    }
}