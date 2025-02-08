using System;
using System.Collections.Generic;
using Zene.Structs;

namespace PHYSICS
{
    public enum ObjType : byte
    {
        None = 0,
        Ball,
        Wall
    }
    public class Wrap<T> where T : IObject
    {
        public Wrap(T t) { Obj = t; ColTime = floatv.MaxValue; }
        
        public T Obj;
        public RefObj Collide;
        // Issue with acceleration
        // Issue with touching objects
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
        // WARNING - change later when adding more objects
        public bool IsObject() => Type == ObjType.Ball;

        public override bool Equals(object obj)
        {
            return obj is RefObj ro &&
                   Index == ro.Index &&
                   Type == ro.Type;
        }

        public override int GetHashCode() => HashCode.Combine(Index, Type);

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