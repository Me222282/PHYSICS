using Zene.Structs;

namespace PHYSICS
{
    public interface IObject
    {
        public Vector2 Location { get; }
        public Vector2 Velocity { get; }
        public floatv Mass { get; }
        public floatv Elasticity { get; }
    }

    public struct Ball : IObject
    {
        public Ball(Vector2 l, Vector2 v, floatv e, floatv r, floatv m)
        {
            Location = l;
            Velocity = v;
            Elasticity = e;
            Radius = r;
            Mass = m;
        }
        
        public Vector2 Location;
        public Vector2 Velocity;
        public floatv Elasticity;
        public floatv Radius;
        public floatv Mass;
        
        Vector2 IObject.Location => Location;
        Vector2 IObject.Velocity => Velocity;
        floatv IObject.Elasticity => Elasticity;
        floatv IObject.Mass => Mass;
    }
    public struct Wall
    {
        public Wall(Vector2 a, Vector2 b, floatv e)
        {
            A = a;
            B = b;
            Elasticity = e;
        }
        
        public Vector2 A;
        public Vector2 B;
        public floatv Elasticity;
    }
}