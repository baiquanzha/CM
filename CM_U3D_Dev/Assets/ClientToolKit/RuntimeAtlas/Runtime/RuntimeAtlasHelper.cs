using UnityEngine;

namespace MTool.RuntimeAtlas.Runtime
{
    public class RuntimeAtlasHelper
    {
        public static GetAtlasImageTask AllocateImageTask()
        {
            GetAtlasImageTask task = new GetAtlasImageTask();
            return task;
        }

        public static void ReleaseImageTask(GetAtlasImageTask task)
        {
        }

        public static IntegerRectangle AllocateRectangle(int x, int y, int width, int height)
        {
            return new IntegerRectangle(x, y, width, height);
        }

        public static void ReleaseRectangle(IntegerRectangle rectangle)
        {
        }

        public static AtlasRect AllocateAtlasRect(Rect rect)
        {
            AtlasRect atlasRect = new AtlasRect();
            atlasRect.Rect = rect;
            return atlasRect;
        }

        public static void ReleaseAtlasRect(AtlasRect atlasRect)
        {
        }

        public static RenderTexture CreateRuntimeAtlasRT(int width, int height, int depth, RenderTextureFormat format)
        {
            RenderTexture rt = new RenderTexture(width, height, depth, format)
            {
                autoGenerateMips = false,
                useMipMap = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            rt.Create();
            return rt;
        }
    }
}