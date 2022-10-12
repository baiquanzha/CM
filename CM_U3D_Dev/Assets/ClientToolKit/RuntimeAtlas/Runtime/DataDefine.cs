using UnityEngine;

namespace MTool.RuntimeAtlas.Runtime
{
    public delegate void OnCallBackTexRect(Texture texture, Rect rect, string path);
    public delegate void OnCallBackMatRect(Material mat, Rect rect, string path);

    public enum RuntimeAtlasGroup
    {
        Size_256 = 256,
        Size_512 = 512,
        Size_1024 = 1024,
        Size_2048 = 2048
    }

    public class AtlasRect
    {
        public int TextureIndex = -1;
        public int ReferenceCount = 0;
        public Rect Rect;
        public IntegerRectangle IntegerRectangle;
    }

    public class GetAtlasImageTask
    {
        public string path;
        public OnCallBackTexRect Callback;
        public OnCallBackMatRect BlitCallback;
    }

    public class IntegerRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Id;

        public int Right
        {
            get
            {
                return X + Width;
            }
        }

        public int Top
        {
            get
            {
                return Y + Height;
            }
        }

        public int Size
        {
            get
            {
                return Width * Height;
            }
        }

        public Rect Rect
        {
            get
            {
                return new Rect(X, Y, Width, Height);
            }
        }

        public IntegerRectangle(int _x = 0, int _y = 0, int _width = 0, int _height = 0)
        {
            X = _x;
            Y = _y;
            Width = _width;
            Height = _height;
        }
    }
}