﻿using TMPro;
using UnityEngine;

namespace VRtist
{
    public class CameraItem : ListItemContent
    {
        public GameObject cameraObject;
        public UIDynamicListItem item;

        public void Start()
        {
            Selection.OnSelectionChanged += OnSelectionChanged;
            Selection.OnActiveCameraChanged += OnActiveCameraChanged;
        }

        public void OnDestroy()
        {
            Selection.OnSelectionChanged -= OnSelectionChanged;
            Selection.OnActiveCameraChanged -= OnActiveCameraChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs args)
        {
            if (Selection.IsSelected(cameraObject))
            {
                SetColor(UIElement.default_pushed_color);
            }
            else
            {
                SetColor(UIElement.default_background_color);
            }
        }

        private void OnActiveCameraChanged(object sender, ActiveCameraChangedArgs args)
        {
            if (args.activeCamera == cameraObject)
            {
                SetColor(UIElement.default_hover_color);
            }
            else
            {
                if (Selection.IsSelected(cameraObject))
                {
                    SetColor(UIElement.default_pushed_color);
                }
                else
                {
                    SetColor(UIElement.default_background_color);
                }
            }
        }

        public void SetColor(Color color)
        {
            gameObject.GetComponentInChildren<MeshRenderer>(true).materials[0].SetColor("_BaseColor", color);
        }

        public void SetCameraObject(GameObject cameraObject)
        {
            this.cameraObject = cameraObject;
            Camera cam = cameraObject.GetComponentInChildren<Camera>(true);
            SetColor(UIElement.default_background_color);
            gameObject.GetComponentInChildren<MeshRenderer>(true).materials[1].SetTexture("_UnlitColorMap", cam.targetTexture);
            TextMeshProUGUI text = transform.Find("Canvas/Panel/Text").gameObject.GetComponent<TextMeshProUGUI>();
            text.text = cameraObject.name;
        }
    }
}
