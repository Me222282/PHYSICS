using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Structs;

namespace PHYSICS
{
    public enum ObjType
    {
        None = 0,
        Ball,
        Wall
    }
    
    public class Manager
    {
        public Manager(Bounds b, floatv e)
        {
            _walls.Add(new Wall(b.TopLeft, b.BottomLeft, e));
            _walls.Add(new Wall(b.TopRight, b.BottomRight, e));
            _walls.Add(new Wall(b.TopLeft, b.TopRight, e));
            _walls.Add(new Wall(b.BottomLeft, b.BottomRight, e));
        }
        
        private List<Wrap<Ball>> _balls = new List<Wrap<Ball>>(20);
        private List<Path> _paths = new List<Path>(20);
        private List<Wall> _walls = new List<Wall>(8);
        // private Bounds _bounds;
        // private floatv _wallElas;
        
        public void AddBall(Ball b)
        {
            _balls.Add(new Wrap<Ball>(b));
            _paths.Add(new Path(b.Location, b.Velocity));
        }
        
        public void Fill(List<ObjectRender> or)
        {
            Span<Wrap<Ball>> span1 = CollectionsMarshal.AsSpan(_balls);
            Span<Path> span2 = CollectionsMarshal.AsSpan(_paths);
            for (int i = 0; i < span1.Length; i++)
            {
                or.Add(new ObjectRender(span1[i].Obj, span2[i]));
            }
        }
        
        public void ElapseTo(floatv time)
        {
            RefObj e1 = new RefObj();
            RefObj e2 = new RefObj();
            floatv minT = floatv.MaxValue;
            
            // Find all init collisions
            Span<Wrap<Ball>> span = CollectionsMarshal.AsSpan(_balls);
            Span<Wall> walls = CollectionsMarshal.AsSpan(_walls);
            for (int i = 0; i < span.Length; i++)
            {
                Wrap<Ball> o1 = span[i];
                RefObj ro = new RefObj(i, ObjType.Ball);
                
                (RefObj r1, RefObj r2, floatv mt) = FindAllCol(o1, i, span, walls, time, ro);
                if (mt < minT)
                {
                    minT = mt;
                    e1 = r1;
                    e2 = r2;
                }
                
                // span[i] = o1;
            }
            
            while (true)
            {
                // No collisions in time
                if (e1.IsNone()) { return; }
                
                if (!e2.IsWall())
                {
                    // Manage earliest
                    Wrap<Ball> c1 = span[e1.Index];
                    Wrap<Ball> c2 = span[e2.Index];
                    floatv et = c1.ColTime;
                    
                    // update pos
                    c1.Obj.Location = Resolve.Linear(c1.Obj, et - c1.ElapsedTime);
                    c2.Obj.Location = Resolve.Linear(c2.Obj, et - c2.ElapsedTime);
                    c1.ElapsedTime = et;
                    c2.ElapsedTime = et;
                    (c1.Obj.Velocity, c2.Obj.Velocity) = Resolve.Find(c1.Obj, c2.Obj);
                    _paths[e1.Index].Add(c1);
                    _paths[e2.Index].Add(c2);
                    c1.LastCollide = e2;
                    c2.LastCollide = e1;
                    c1.ColTime = floatv.MaxValue;
                    c2.ColTime = floatv.MaxValue;
                    c1.Collide = new RefObj();
                    c2.Collide = new RefObj();
                    // span[e1.Index] = c1;
                    // span[e2.Index] = c2;
                }
                else
                {
                    // Resolve wall collision
                    Wrap<Ball> c1 = span[e1.Index];
                    Wall w = walls[e2.Index];
                    floatv et = c1.ColTime;
                    c1.Obj.Location = Resolve.Linear(c1.Obj, et - c1.ElapsedTime);
                    c1.ElapsedTime = et;
                    c1.Obj.Velocity = Resolve.Find(c1.Obj, w);
                    _paths[e1.Index].Add(c1);
                    c1.LastCollide = e2;
                    c1.ColTime = floatv.MaxValue;
                    c1.Collide = new RefObj();
                    // span[e1.Index] = c1;
                }
                
                RefObj change1 = e1;
                RefObj change2 = e2;
                e1 = new RefObj();
                e2 = new RefObj();
                minT = floatv.MaxValue;
                
                // Update collisions with the changes to the 2 objects
                // Find missing collisions (ones who lost their earilest collide)
                for (int i = 0; i < span.Length; i++)
                {
                    Wrap<Ball> o1 = span[i];
                    RefObj ro = new RefObj(i, ObjType.Ball);
                    
                    if (o1.CollideTaken || i == change1.Index || i == change2.Index)
                    {
                        o1.CollideTaken = false;
                        (RefObj r1, RefObj r2, floatv mt) =
                            FindAllCol(o1, i, span, walls, time, ro);
                        if (mt < minT)
                        {
                            minT = mt;
                            e1 = r1;
                            e2 = r2;
                        }
                    }
                    else
                    {
                        Wrap<Ball> o2 = span[change1.Index];
                        floatv t = FindCol(o1, o2, time, span, ro, change1);
                        
                        if (t >= 0 && t < minT)
                        {
                            minT = t;
                            e1 = ro;
                            e2 = change1;
                        }
                        
                        if (!change2.IsWall())
                        {
                            Wrap<Ball> o3 = span[change2.Index];
                            t = FindCol(o1, o3, time, span, ro, change2);
                            
                            if (t >= 0 && t < minT)
                            {
                                minT = t;
                                e1 = ro;
                                e2 = change2;
                            }
                        }
                    }
                    
                    // span[i] = o1;
                }
                
                // remove old last collides
                // span[change1.Index].LastCollide = new RefObj();
                // span[change2.Index].LastCollide = new RefObj();
            }
            
            // repeat
        }
        
        private static floatv FindCol(
            Wrap<Ball> o1, Wrap<Ball> o2, floatv time,
            Span<Wrap<Ball>> span, RefObj r1, RefObj r2)
        {
            if (o1.LastCollide == r2) { return -1; }
            
            // Too far out of sync
            if (o2.ColTime < o1.ElapsedTime || o1.ColTime < o2.ElapsedTime) { return -1; }
            floatv t = Collisions.BallBallLinearOffset(o1.Obj, o2.Obj, o1.ElapsedTime, o2.ElapsedTime);
            if (t < 0 || o2.ColTime < t || o1.ColTime < t ||
                time < t || t < o1.ElapsedTime || t < o2.ElapsedTime) { return -1; }
            
            // Remove old collide
            if (!o2.Collide.IsNone() && !o2.Collide.IsWall())
            {
                int i = o2.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            if (!o1.Collide.IsNone() && !o1.Collide.IsWall())
            {
                int i = o1.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            o2.Collide = r1;
            o2.ColTime = t;
            o1.Collide = r2;
            o1.ColTime = t;
            o1.CollideTaken = false;
            o2.CollideTaken = false;
            
            return t;
        }
        private static floatv FindWallCol(
            Wrap<Ball> o1, Span<Wrap<Ball>> span, floatv time,
            Wall w, RefObj rw)
        {
            if (o1.LastCollide == rw) { return -1; }
            
            floatv t = Collisions.BallWallLinearOffset(o1.Obj, w, o1.ElapsedTime);
            if (t < 0 || o1.ColTime < t ||
                time < t || t < o1.ElapsedTime) { return -1; }
            
            if (!o1.Collide.IsNone() && !o1.Collide.IsWall())
            {
                int i = o1.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            
            o1.ColTime = t;
            o1.Collide = rw;
            return t;
        }
        private static (RefObj, RefObj, floatv) FindAllCol(
            Wrap<Ball> o1, int i, Span<Wrap<Ball>> span,
            Span<Wall> walls, floatv time, RefObj ro)
        {
            floatv minT = floatv.MaxValue;
            RefObj e1 = new RefObj();
            RefObj e2 = new RefObj();
            
            for (int j = 0; j < walls.Length; j++)
            {
                RefObj rw = new RefObj(j, ObjType.Wall);
                floatv bt = FindWallCol(o1, span, time, walls[j], rw);
                if (bt >= 0 && bt < minT)
                {
                    minT = bt;
                    e2 = rw;
                    e1 = ro;
                }
            }
            
            for (int j = i + 1; j < span.Length; j++)
            {
                Wrap<Ball> o2 = span[j];
                
                RefObj r2 = new RefObj(j, ObjType.Ball);
                floatv t = FindCol(o1, o2, time, span, ro, r2);
                    
                if (t >= 0 && (t < minT))
                {
                    minT = t;
                    e1 = ro;
                    e2 = r2;
                }
                
                // span[j] = o2;
            }
            
            return (e1, e2, minT);
        }
    }
    
    public class Wrap<T> where T : IObject
    {
        public Wrap(T t) { Obj = t; ColTime = floatv.MaxValue; }
        
        public T Obj;
        public RefObj Collide;
        public RefObj LastCollide;
        public floatv ColTime;
        public floatv ElapsedTime;
        public bool CollideTaken;
    }
    public struct RefObj
    {
        public RefObj(int i, ObjType t)
        {
            Index = i;
            Type = t;
        }
        
        public int Index;
        public ObjType Type;
        
        public bool IsNone() => Type == ObjType.None;
        public bool IsWall() => Type == ObjType.Wall;
        public static RefObj None { get; } = new RefObj(0, ObjType.None);
        
        public static bool operator ==(RefObj a, RefObj b)
        {
            return a.Type == b.Type && a.Index == b.Index;
        }
        public static bool operator !=(RefObj a, RefObj b)
        {
            return a.Type != b.Type || a.Index != b.Index;
        }
    }
    
    public struct Path
    {
        public Path(Vector2 l, Vector2 v)
        {
            References.Add(new Point(l, v, 0));
        }
        public List<Point> References = new List<Point>();
        public void Add<T>(Wrap<T> w) where T : IObject
        {
            References.Add(new Point(w.Obj.Location, w.Obj.Velocity, w.ElapsedTime));
        }
    }
    public struct Point
    {
        public Point(Vector2 l, Vector2 v, floatv t)
        {
            Location = l;
            Velocity = v;
            Time = t;
        }
        public Vector2 Location;
        public Vector2 Velocity;
        public floatv Time;
    }
}