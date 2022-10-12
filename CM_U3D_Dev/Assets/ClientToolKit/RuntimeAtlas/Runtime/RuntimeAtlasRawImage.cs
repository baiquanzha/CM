using UnityEngine;
using UnityEngine.UI;

namespace MTool.RuntimeAtlas.Runtime
{
    public class RuntimeAtlasRawImage : RawImage
    {
        public RuntimeAtlasGroup AtlasGroup { get; set; } = RuntimeAtlasGroup.Size_1024;

        private bool isSetImage = false;
        private RuntimeAtlas Atlas;

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
            if (texture != null && isSetImage == false)
            {
                SetGroup(AtlasGroup);
                PreSetImage();
            }
        }

        public void SetGroup(RuntimeAtlasGroup group)
        {
            if (Atlas != null)
            {
                return;
            }
            AtlasGroup = group;
            Atlas = RuntimeAtlasManager.Instance.GetRuntimeAtlas(group, true);
        }

        private void PreSetImage()
        {
            if (string.IsNullOrEmpty(Path))
                return;
            if (Atlas.UsingCopyTexture)
                Atlas.SetTexture(Path, texture, OnGetImageCallback);
            else
                Atlas.SetTexture(Path, texture, OnGetMaterialCallback);
        }

        private void OnGetImageCallback(Texture tex, Rect rect, string path)
        {
            texture = tex;
            uvRect = rect;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        private void OnGetMaterialCallback(Material mat, Rect rect, string path)
        {
            texture = null;
            material = mat;
            uvRect = rect;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(Path) && Atlas != null)
            {
                Atlas.RemoveImage(Path, false);
                Path = null;
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