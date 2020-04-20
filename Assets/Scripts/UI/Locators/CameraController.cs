﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRtist
{
    public class CameraController : ParametersController
    {
        private Camera cameraObject = null;
        public float focal = 35f;
        public float near = 0.07f;
        public float far = 1000f;

        private LineRenderer frustumRenderer;

        protected override void Start()
        {
            cameraObject = gameObject.GetComponentInChildren<Camera>();
            GameObject frustum = transform.Find("Frustum").gameObject;
            frustumRenderer = frustum.GetComponent<LineRenderer>();
            frustumRenderer.enabled = false;
        }

        void Update()
        {
            if (null == cameraObject)
                return;

            float scale = GlobalState.worldScale;
            cameraObject.farClipPlane = far * scale;
            cameraObject.nearClipPlane = near * scale;
            cameraObject.focalLength = focal;

            // Only draw frustum for selected camera
            if(CameraTool.showCameraFrustum && gameObject.layer == LayerMask.NameToLayer("Selection"))
            {
                DrawFrustum();
            }
            else
            {
                frustumRenderer.enabled = false;
            }
        }

        public override void CopyParameters(ParametersController otherController)
        {
            base.CopyParameters(otherController);

            CameraController other = otherController as CameraController;
            focal = other.focal;
            near = other.near;
            far = other.far;            
        }

        private void DrawFrustum()
        {
            frustumRenderer.enabled = true;
            frustumRenderer.gameObject.layer = LayerMask.NameToLayer("UI");  // we don't want the selection outline

            float halfWidthFactor = cameraObject.sensorSize.x * 0.5f / focal;
            float halfHeightFactor = cameraObject.sensorSize.y * 0.5f / focal;

            float nearHalfWidth = halfWidthFactor * near;
            float nearHalfHeight = halfHeightFactor * near;
            float farHalfWidth = halfWidthFactor * far;
            float farHalfHeight = halfHeightFactor * far;

            Vector3[] points = new Vector3[16];
            points[0] = new Vector3( nearHalfWidth, -nearHalfHeight, near);
            points[1] = new Vector3( nearHalfWidth,  nearHalfHeight, near);
            points[2] = new Vector3(-nearHalfWidth,  nearHalfHeight, near);
            points[3] = new Vector3(-nearHalfWidth, -nearHalfHeight, near);
            points[4] = new Vector3( nearHalfWidth, -nearHalfHeight, near);

            points[5] = new Vector3( farHalfWidth, -farHalfHeight, far);
            points[6] = new Vector3( farHalfWidth,  farHalfHeight, far);
            points[7] = new Vector3(-farHalfWidth,  farHalfHeight, far);
            points[8] = new Vector3(-farHalfWidth, -farHalfHeight, far);
            points[9] = new Vector3( farHalfWidth, -farHalfHeight, far);

            points[10] = new Vector3(  farHalfWidth,   farHalfHeight, far);
            points[11] = new Vector3( nearHalfWidth,  nearHalfHeight, near);
            points[12] = new Vector3(-nearHalfWidth,  nearHalfHeight, near);
            points[13] = new Vector3( -farHalfWidth,   farHalfHeight, far);
            points[14] = new Vector3( -farHalfWidth,  -farHalfHeight, far);
            points[15] = new Vector3(-nearHalfWidth, -nearHalfHeight, near);

            // Remove camera object scale
            float invScale = 1f / frustumRenderer.transform.parent.lossyScale.x;
            invScale *= GlobalState.worldScale;
            frustumRenderer.transform.localScale = new Vector3(invScale, invScale, invScale);

            frustumRenderer.positionCount = points.Length;
            frustumRenderer.SetPositions(points);
        }
    }
}
