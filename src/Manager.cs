using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zene.Structs;

namespace PHYSICS
{
    public enum ObjType
    {
        None = 0,
        Ball
    }
    
    public class Manager
    {
        private List<Wrap<Ball>> _balls = new List<Wrap<Ball>>(20);
        private List<Path> _paths = new List<Path>(20);
        
        public void AddBall(Ball b)
        {
            _balls.Add(new Wrap<Ball>(b));
            _paths.Add(new Path());
        }
        
        public void Elapse(floatv time)
        {
            // Find all init collisions
            
            // Remove false collisions (early catch)
            
            // Manage earliest
            // Update collisions with the changes to the 2 objects
            
            // repeat
        }
    }
    
    public struct Wrap<T> where T : IObject
    {
        public Wrap(T t) { Obj = t; Time = floatv.MaxValue; }
        
        public T Obj;
        public RefObj Collide;
        public floatv Time;
    }
    public struct RefObj
    {
        public RefObj(uint i, ObjType t)
        {
            Index = i;
            Type = t;
        }
        
        public uint Index;
        public ObjType Type;
        
        public bool IsNone() => Type == ObjType.None;
        public static RefObj None { get; } = new RefObj(0, ObjType.None);
    }
    
    public struct Path
    {
        public Path() { }
        public List<Point> References = new List<Point>();
    }
    public struct Point
    {
        public Vector2 Location;
        public Vector2 Veclocity;
        public floatv Time;
    }
}