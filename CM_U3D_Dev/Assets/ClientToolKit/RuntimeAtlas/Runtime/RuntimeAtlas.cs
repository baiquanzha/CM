using System.Collections.Generic;
using UnityEngine;
using MTool.AssetBundleManager.Runtime;

namespace MTool.RuntimeAtlas.Runtime
{
    public class RuntimeAtlas
    {
        public bool TopFirst { get; private set; } = true;
        private float UVXDiv, UVYDiv;
        public int AtlasWidth { get; private set; }
        public int AtlasHeight { get; private set; }
        public int Padding { get; } = 3;
        public float Offset { get; } = 0;

        public List<Texture2D> Texture2Ds { get; private set; } = new List<Texture2D>();

        public List<RenderTexture> RenderTextures { get; private set; } = new List<RenderTexture>();

        public Dictionary<string, AtlasRect> UsingRects = new Dictionary<string, AtlasRect>();

        private List<GetAtlasImageTask> GetImageTasks = new List<GetAtlasImageTask>();

        public List<List<IntegerRectangle>> FreeAreas { get; private set; } = new List<List<IntegerRectangle>>();

        private List<Material> Materials = new List<Material>();

        public RuntimeAtlasGroup AtlasGroup { get; set; }

        public bool UsingCopyTexture { get; private set; } = false;

        public TextureFormat AtlasTextureFormat = TextureFormat.RGBA32;

        public RenderTextureFormat RenderTextureFormat = RenderTextureFormat.ARGB32;

        private Color32[] tmpColor;
        private Material blitMaterial;
        private int blitParamId;

        public RuntimeAtlas(RuntimeAtlasGroup group, bool topFirst = true, bool _usingCopyTexture = true)
        {
            AtlasGroup = group;
            TopFirst = topFirst;
            int length = (int)group;
            tmpColor = new Color32[length * length];
            for (int i = 0; i < tmpColor.Length; i++)
            {
                tmpColor[i] = Color.clear;
            }
            if (!UsingCopyTexture)
            {
                blitMaterial = new Material(Shader.Find("RuntimeAtlas/GraphicBlit"));
                blitParamId = Shader.PropertyToID("_DrawRect");
            }
            AtlasWidth = length;
            AtlasHeight = length;
            UVXDiv = 1f / length;
            UVYDiv = 1f / length;
            CreateNewAtlas();
        }

        public void CreateNewAtlas()
        {
            if (UsingCopyTexture)
            {
                Texture2D texture2D = new Texture2D(AtlasWidth, AtlasHeight, AtlasTextureFormat, false, true);
                texture2D.filterMode = FilterMode.Bilinear;
                texture2D.SetPixels32(0, 0, AtlasWidth, AtlasHeight, tmpColor);
                texture2D.Apply(false);
                Texture2Ds.Add(texture2D);
            }
            else
            {
                RenderTexture renderTexture = RuntimeAtlasHelper.CreateRuntimeAtlasRT(AtlasWidth, AtlasHeight, 0, RenderTextureFormat);
                renderTexture.name = string.Format("DynamicAtlas {0} -- {1}", AtlasWidth, AtlasHeight);
                renderTexture.DiscardContents(true, true);
                Material material = new Material(Shader.Find("UI/Default"));
                material.mainTexture = renderTexture;
                Materials.Add(material);

                blitMaterial.SetVector(blitParamId, new Vector4(0, 0, 1, 1));
                Texture2D cleared_texture = new Texture2D(2, 2, AtlasTextureFormat, false);
                Graphics.Blit(cleared_texture, renderTexture, blitMaterial);
                RenderTextures.Add(renderTexture);
            }

            IntegerRectangle area = RuntimeAtlasHelper.AllocateRectangle(0, 0, AtlasWidth, AtlasHeight);
            List<IntegerRectangle> list = new List<IntegerRectangle>();
            list.Add(area);
            FreeAreas.Add(list);
        }

        public void CopyTexture(int posX, int posY, int index, Texture2D srcTex)
        {
            Texture2D dstTex = Texture2Ds[index];
            Graphics.CopyTexture(srcTex, 0, 0, 0, 0, srcTex.width, srcTex.height, dstTex, 0, 0, posX, posY);
        }

        public void BlitTexture(int posX, int posY, int index, Texture2D srcTex)
        {
            Rect uv = new Rect(posX * UVXDiv, posY * UVYDiv, srcTex.width * UVXDiv, srcTex.height * UVYDiv);
            RenderTexture dest = RenderTextures[index];
            GraphicsBlit(uv, srcTex, dest, blitMaterial);
        }

        private void GraphicsBlit(Rect rc, Texture source, RenderTexture dest, Material mat)
        {
            mat.SetVector(blitParamId, new Vector4(rc.xMin, rc.yMin, rc.xMax, rc.yMax));
#if UNITY_EDITOR
            dest.DiscardContents();
#endif
            Graphics.Blit(source, dest, mat);
        }

        private void OnGetImage()
        {
            if (GetImageTasks.Count == 0)
                return;
            GetAtlasImageTask task = GetImageTasks[0];
            AssetBundleManager.Runtime.AssetBundleManager.LoadAsync(task.path, typeof(Texture2D), (Object obj) =>
            {
                Texture2D tex = null;
                if (obj != null)
                {
                    tex = obj as Texture2D;
                }
                OnRenderTexture(task.path, tex);
                OnGetImage();
            });
        }

        private void OnRenderTexture(string path, Texture2D texture2D)
        {
            if (texture2D == null)
            {
                for (int i = GetImageTasks.Count - 1; i >= 0; i--)
                {
                    GetAtlasImageTask task = GetImageTasks[i];
                    if (task.path.Equals(path))
                    {
                        if (UsingCopyTexture)
                            task.Callback?.Invoke(null, Rect.zero, path);
                        else
                            task.BlitCallback?.Invoke(null, Rect.zero, path);

                        RuntimeAtlasHelper.ReleaseImageTask(task);
                        GetImageTasks.RemoveAt(i);
                    }
                }
                return;
            }
            IntegerRectangle useArea = InsertArea(texture2D.width, texture2D.height, out int index);
            Rect uv = new Rect((useArea.X + Offset) * UVXDiv, (useArea.Y + Offset) * UVYDiv, (useArea.Width - Padding - Offset * 2) * UVXDiv,
                (useArea.Height - Padding - Offset * 2) * UVYDiv);
            if (UsingCopyTexture)
                CopyTexture(useArea.X, useArea.Y, index, texture2D);
            else
                BlitTexture(useArea.X, useArea.Y, index, texture2D);

            AtlasRect atlasRect = RuntimeAtlasHelper.AllocateAtlasRect(uv);
            atlasRect.TextureIndex = index;
            atlasRect.IntegerRectangle = useArea;
            UsingRects[path] = atlasRect;

            for (int i = GetImageTasks.Count - 1; i >= 0; i--)
            {
                GetAtlasImageTask task = GetImageTasks[i];
                if (task.path.Equals(path))
                {
                    UsingRects[path].ReferenceCount = UsingRects[path].ReferenceCount + 1;
                    if (UsingCopyTexture)
                    {
                        if (task.Callback != null)
                        {
                            Texture2D dstTex = Texture2Ds[index];
                            task.Callback(dstTex, uv, path);
                        }
                    }
                    else
                    {
                        if (task.BlitCallback != null)
                        {
                            Material material = Materials[index];
                            task.BlitCallback(material, uv, path);
                        }
                    }
                    RuntimeAtlasHelper.ReleaseImageTask(task);
                    GetImageTasks.RemoveAt(i);
                }
            }
        }

        public void GetImage(string path, OnCallBackTexRect callback)
        {
            if (UsingRects.ContainsKey(path))
            {
                if (callback != null)
                {
                    AtlasRect atlasRect = UsingRects[path];
                    atlasRect.ReferenceCount++;
                    Texture2D texture2D = Texture2Ds[atlasRect.TextureIndex];
                    callback(texture2D, UsingRects[path].Rect, path);
                }
                return;
            }
            GetAtlasImageTask task = RuntimeAtlasHelper.AllocateImageTask();
            task.path = path;
            task.Callback = callback;
            GetImageTasks.Add(task);
            if (GetImageTasks.Count > 1)
                return;
            OnGetImage();
        }

        public void SetTexture(string path, Texture texture, OnCallBackTexRect callback)
        {
            if (texture == null || texture.width > AtlasWidth || texture.height > AtlasHeight)
            {
                callback?.Invoke(null, Rect.zero, path);
                Debug.Log("Texture Does not meet the standard:" + path);
                return;
            }
            if (UsingRects.ContainsKey(path))
            {
                if (callback != null)
                {
                    AtlasRect atlasRect = UsingRects[path];
                    atlasRect.ReferenceCount++;
                    Texture2D texture2D = Texture2Ds[atlasRect.TextureIndex];
                    callback(texture2D, atlasRect.Rect, path);
                }
                return;
            }
            GetAtlasImageTask task = RuntimeAtlasHelper.AllocateImageTask();
            task.path = path;
            task.Callback = callback;
            GetImageTasks.Add(task);
            Texture2D tex2D = texture as Texture2D;
            OnRenderTexture(path, tex2D);
        }

        public void SetTexture(string path, Texture _texture, OnCallBackMatRect callback)
        {
            if (_texture == null || _texture.width > AtlasWidth || _texture.height > AtlasHeight)
            {
                callback?.Invoke(null, Rect.zero, path);
                return;
            }
            if (UsingRects.ContainsKey(path))
            {
                if (callback != null)
                {
                    AtlasRect atlasRect = UsingRects[path];
                    atlasRect.ReferenceCount++;
                    Material material = Materials[atlasRect.TextureIndex];
                    callback(material, UsingRects[path].Rect, path);
                }
                return;
            }
            GetAtlasImageTask task = RuntimeAtlasHelper.AllocateImageTask();
            task.path = path;
            task.BlitCallback = callback;
            GetImageTasks.Add(task);
            Texture2D tex = _texture as Texture2D;
            OnRenderTexture(path, tex);
        }

        public IntegerRectangle InsertArea(int width, int height, out int index)
        {
            IntegerRectangle result;
            IntegerRectangle freeArea = GetFreeArea(width, height, out index, out bool justRightSize);
            if (!justRightSize)
            {
                int resultWidth = (width + Padding) > freeArea.Width ? freeArea.Width : (width + Padding);
                int resultHeight = (height + Padding) > freeArea.Height ? freeArea.Height : (height + Padding);
                result = RuntimeAtlasHelper.AllocateRectangle(freeArea.X, freeArea.Y, resultWidth, resultHeight);
                GenerateDividedAreas(index, result, freeArea, TopFirst);
            }
            else
                result = RuntimeAtlasHelper.AllocateRectangle(freeArea.X, freeArea.Y, freeArea.Width, freeArea.Height);
            RemoveFreeArea(index, freeArea);
            return result;
        }

        /// <summary>
        /// 获取可用的空闲区域
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="index"></param>
        /// <param name="justRightSize"></param>
        /// <returns></returns>
        private IntegerRectangle GetFreeArea(int width, int height, out int index, out bool justRightSize)
        {
            index = -1;
            justRightSize = false;

            if (width > AtlasWidth || height > AtlasHeight)
            {
                Debug.LogError("ERROR 图片尺寸大于图集大小: 图片大小为" + width + "x" + height + "  图集大小为" + AtlasWidth + "x" + AtlasHeight);
                return null;
            }
            int paddedWidth = width + Padding;
            int paddedHeight = height + Padding;
            int count = FreeAreas.Count;
            IntegerRectangle tempArea = null;
            for (int i = 0; i < count; i++)
            {
                var list = FreeAreas[i];
                bool findResult = false;
                foreach (var area in list)
                {
                    bool isJustFullWidth = (width == area.Width || width == AtlasWidth);
                    bool isJustFullHeight = (height == area.Height || height == AtlasHeight);
                    bool isFitWidth = isJustFullWidth || paddedWidth <= area.Width;
                    bool isFitHeight = paddedHeight <= area.Height || isJustFullHeight;
                    if (isFitWidth && isFitHeight)
                    {
                        index = i;
                        justRightSize = (isJustFullWidth || paddedWidth == area.Width) && (isJustFullHeight || paddedHeight == area.Height);
                        if (isJustFullWidth && isJustFullHeight)
                            return area;
                        findResult = true;
                        if (tempArea != null)
                        {
                            if (TopFirst)
                            {
                                if (tempArea.Height > area.Height)
                                    tempArea = area;
                            }
                            else
                            {
                                if (tempArea.Width > area.Width)
                                    tempArea = area;
                            }
                        }
                        else
                            tempArea = area;
                    }
                }
                if (findResult)
                    break;
            }
            if (tempArea != null)
                return tempArea;
            CreateNewAtlas();
            index = FreeAreas.Count - 1;
            justRightSize = false;
            return FreeAreas[index][0];
        }

        /// <summary>
        /// 切割矩形
        /// </summary>
        private void GenerateDividedAreas(int index, IntegerRectangle divider, IntegerRectangle freeArea, bool topFirst)
        {
            int rightDelta = freeArea.Right - divider.Right;
            if (rightDelta > 0)
            {
                if (topFirst)
                {
                    IntegerRectangle area = RuntimeAtlasHelper.AllocateRectangle(divider.Right, divider.Y, rightDelta, divider.Height);
                    AddFreeArea(index, area);
                }
                else
                {
                    IntegerRectangle area = RuntimeAtlasHelper.AllocateRectangle(divider.Right, divider.Y, rightDelta, freeArea.Height);
                    AddFreeArea(index, area);
                }
            }

            int topDelta = freeArea.Top - divider.Top;
            if (topDelta > 0)
            {
                if (topFirst)
                {
                    IntegerRectangle area = RuntimeAtlasHelper.AllocateRectangle(freeArea.X, divider.Top, freeArea.Width, topDelta);
                    AddFreeArea(index, area);
                }
                else
                {
                    IntegerRectangle area = RuntimeAtlasHelper.AllocateRectangle(freeArea.X, divider.Top, divider.Width, topDelta);
                    AddFreeArea(index, area);
                }
            }
        }

        private void AddFreeArea(int index, IntegerRectangle area)
        {
            List<IntegerRectangle> list = FreeAreas[index];
            list.Add(area);
        }

        private void RemoveFreeArea(int index, IntegerRectangle area)
        {
            RuntimeAtlasHelper.ReleaseRectangle(area);
            FreeAreas[index].Remove(area);
        }

        private IntegerRectangle OnMergeAreaRecursive(IntegerRectangle target, int textureIndex)
        {
            List<IntegerRectangle> rectangles = FreeAreas[textureIndex];
            IntegerRectangle mergeRect = null;
            foreach (var freeArea in rectangles)
            {
                if (target.Right == freeArea.X && target.Y == freeArea.Y && target.Height == freeArea.Height) //右
                {
                    mergeRect = RuntimeAtlasHelper.AllocateRectangle(target.X, target.Y, target.Width + freeArea.Width, target.Height);
                }
                else if (target.X == freeArea.X && target.Top == freeArea.Y && target.Width == freeArea.Width)//上
                {
                    mergeRect = RuntimeAtlasHelper.AllocateRectangle(target.X, target.Y, target.Width, target.Height + freeArea.Height);
                }
                else if (target.X == freeArea.Right && target.Y == freeArea.Y && target.Height == freeArea.Height)//左
                {
                    mergeRect = RuntimeAtlasHelper.AllocateRectangle(freeArea.X, freeArea.Y, freeArea.Width + target.Width, freeArea.Height);
                }
                else if (target.X == freeArea.X && target.Y == freeArea.Top && target.Width == freeArea.Width)//下
                {
                    mergeRect = RuntimeAtlasHelper.AllocateRectangle(freeArea.X, freeArea.Y, target.Width, target.Height + freeArea.Height);
                }
                if (mergeRect != null)
                {
                    RemoveFreeArea(textureIndex, freeArea);
                    return mergeRect;
                }
            }
            if (mergeRect == null)
                AddFreeArea(textureIndex, target);
            return mergeRect;
        }

        private bool OnMergeArea(IntegerRectangle rectangle, int textureIndex)
        {
            bool isMerge = false;
            while (rectangle != null)
            {
                rectangle = OnMergeAreaRecursive(rectangle, textureIndex);
                if (rectangle != null)
                    isMerge = true;
            }
            return isMerge;
        }

        public void RemoveImage(string path, bool clearRange = false)
        {
            if (UsingRects.ContainsKey(path))
            {
                AtlasRect atlasRect = UsingRects[path];
                atlasRect.ReferenceCount--;
                if (atlasRect.ReferenceCount == 0)
                {
                    if (clearRange)
                        ClearTexture(atlasRect.TextureIndex, atlasRect.IntegerRectangle.Rect);
                    OnMergeArea(atlasRect.IntegerRectangle, atlasRect.TextureIndex);
                    UsingRects.Remove(path);
                    RuntimeAtlasHelper.ReleaseAtlasRect(atlasRect);
                }
            }
        }

        private void ClearTexture(int index, Rect rect)
        {
            Texture2D dstTex = Texture2Ds[index];
            int width = (int)rect.width;
            int height = (int)rect.height;
            Color32[] colors = new Color32[width * height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.clear;
            dstTex.SetPixels32((int)rect.x, (int)rect.y, width, height, colors);
            dstTex.Apply();
        }

        public void ClearTextureWithBlit()
        {
            int freeAreasCount = FreeAreas.Count;
            for (int i = 0; i < freeAreasCount; i++)
            {
                var list = FreeAreas[i];
                int listCount = list.Count;
                RenderTexture dest = RenderTextures[i];
                foreach (IntegerRectangle rect in list)
                {
                    Rect uv = new Rect(rect.X * UVXDiv, rect.Y * UVYDiv, rect.Width * UVXDiv, rect.Height * UVYDiv);
                    Texture2D srcTex = new Texture2D(2, 2, AtlasTextureFormat, false);
                    GraphicsBlit(uv, srcTex, dest, blitMaterial);
                }
            }
        }
    }
}
