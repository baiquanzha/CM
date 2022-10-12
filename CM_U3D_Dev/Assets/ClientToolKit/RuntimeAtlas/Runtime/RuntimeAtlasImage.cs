using System;
using UnityEngine;
using UnityEngine.UI;

namespace MTool.RuntimeAtlas.Runtime
{
    public class RuntimeAtlasImage : Image
    {
        public RuntimeAtlasGroup AtlasGroup { get; set; } = RuntimeAtlasGroup.Size_1024;

        private bool isSetImage = false;

        private RuntimeAtlas Atlas;
        private Sprite defaultSprite;

        [SerializeField]
        private string path = string.Empty;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        protected override void Start()
        {
            base.Start();
#if UNITY_EDITOR
            if (Application.isPlaying)
                OnPreDoImage();
#else
            OnPreDoImage();
#endif
        }

        private void OnPreDoImage()
        {
            if (mainTexture != null && !isSetImage)
            {
                SetGroup(AtlasGroup);
                SetImage();
            }
        }

        private void SetGroup(RuntimeAtlasGroup group)
        {
            if (Atlas != null)
                return;
            AtlasGroup = group;
            Atlas = RuntimeAtlasManager.Instance.GetRuntimeAtlas(group, true);
        }

        private void SetImage()
        {
            defaultSprite = sprite;
            if (string.IsNullOrEmpty(Path))
                return;
            if (Atlas.UsingCopyTexture)
                Atlas.SetTexture(Path, mainTexture, OnGetImageCallback);
            else
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            }
        }

        public void OnGetImageCallback(Texture texture, Rect rect, string path)
        {
            int length = (int)AtlasGroup;
            Rect spriteRect = rect;
            spriteRect.x *= length;
            spriteRect.y *= length;
            spriteRect.width *= length;
            spriteRect.height *= length;
            sprite = Sprite.Create((Texture2D)texture, spriteRect, defaultSprite.pivot, defaultSprite.pixelsPerUnit, 1, SpriteMeshType.Tight, defaultSprite.border);
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(Path) && Atlas != null)
            {
                Atlas.RemoveImage(Path, false);
                Path = string.Empty;
            }
            isSetImage = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
        }
    }
}