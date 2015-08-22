using System;
using UnityEngine;

namespace CrowdPleaser.Utilities
{
    [Serializable]
    public struct Vector2i
    {
        private static readonly Vector2i _zero = new Vector2i(0);
        private static readonly Vector2i _one = new Vector2i(1);

        public Vector2i(int x, int y)
            : this()
        {
            this.x = x;
            this.y = y;
        }

        public Vector2i(int v)
            : this(v, v)
        {
        }

        public static Vector2i Zero { get { return _zero; } }
        public static Vector2i One { get { return _one; } }

        public int x;
        public int y;

        public Vector2i East { get { return MoveX(1); } }
        public Vector2i West { get { return MoveX(-1); } }
        public Vector2i Left { get { return MoveX(1); } }
        public Vector2i Right { get { return MoveX(-1); } }

        public Vector2i North { get { return MoveY(1); } }
        public Vector2i South { get { return MoveY(-1); } }
        public Vector2i Up { get { return MoveY(1); } }
        public Vector2i Down { get { return MoveY(-1); } }

        public Vector2i Inverse { get { return new Vector2i(-x, -y); } }

        public int Length2 { get { return x * x + y * y; } }

        //public static Vector2i Round(Vector2 vector2)
        //{
        //    return new Vector2i(Convert.ToInt32(vector2.x), Convert.ToInt32(vector2.y));
        //}

        //public static Vector2i Floor(Vector2 vector3)
        //{
        //    return new Vector2i((int)System.Math.Floor(vector3.x), (int)System.Math.Floor(vector3.y));
        //}        

        public static Vector2i Add(ref Vector2i i1, ref Vector2i i2)
        {
            return new Vector2i(i1.x + i2.x, i1.y + i2.y);
        }

        public static Vector2i Sub(ref Vector2i i1, ref Vector2i i2)
        {
            return new Vector2i(i1.x - i2.x, i1.y - i2.y);
        }

        //public static Vector2i operator *(Vector2i i, int value)
        //{
        //    return new Vector2i(i.x * value, i.y * value);
        //}

        //public static Vector2i operator *(int value, Vector2i i)
        //{
        //    return new Vector2i(i.x * value, i.y * value);
        //}

        public static Vector2i operator +(Vector2i i1, Vector2i i2)
        {
            return new Vector2i(i1.x + i2.x, i1.y + i2.y);
        }

        //public static Vector2i operator -(Vector2i i1, Vector2i i2)
        //{
        //    return new Vector2i(i1.x - i2.x, i1.y - i2.y);
        //}

        public static bool operator ==(Vector2i i1, Vector2i i2)
        {
            return Equals(i1, i2);
        }

        public static bool operator !=(Vector2i i1, Vector2i i2)
        {
            return !Equals(i1, i2);
        }

        public Vector2 ToVector2() { return new Vector2(x, y); }

        public override bool Equals(object obj)
        {
            Vector2i? other = obj as Vector2i?;
            return
                other.HasValue &&
                (other.Value.x == this.x) &&
                (other.Value.y == this.y);
        }

        public override int GetHashCode()
        {
            return x + (y * 104729);
        }      

        public Vector2i Move(int dx, int dy)
        {
            return new Vector2i(x + dx, y + dy);
        }

        public Vector2i MoveX(int dx)
        {
            return new Vector2i(x + dx, y);
        }

        public Vector2i MoveY(int dy)
        {
            return new Vector2i(x, y + dy);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }

        public static Vector2i Max(Vector2i a, Vector2i b)
        {
            return new Vector2i(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        public static Vector2i Min(Vector2i a, Vector2i b)
        {
            return new Vector2i(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }
        
        public static Vector2i FromVector2Round(Vector2 position)
        {
            return
                new Vector2i(
                    (int)(position.x + 0.5f),
                    (int)(position.y + 0.5f));
        }
        
        public static Vector2i FromVector2Trunc(Vector2 position)
        {
            return
                new Vector2i(
                    (int)(position.x ),
                    (int)(position.y ));
        }
        
        public bool ContainsAsSize(Vector2i point)
        {
            return point.x >= 0 && point.y >= 0 && point.x < x && point.y < y;
        }
    }
}