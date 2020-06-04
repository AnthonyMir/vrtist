﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRtist
{
    public class SelectionChangedArgs : EventArgs
    {
        public Dictionary<int, GameObject> selectionBefore = new Dictionary<int, GameObject>();
    }

    public class ActiveCameraChangedArgs : EventArgs
    {
        public GameObject activeCamera = null;
    }
    public class Selection
    {
        //public static Color SelectedColor = new Color(57f / 255f, 124f / 255f, 212f / 255f);
        public static Color SelectedColor = new Color(0f / 255f, 167f / 255f, 255f / 255f);
        public static Color UnselectedColor = Color.white;

        public static Dictionary<int, GameObject> selection = new Dictionary<int, GameObject>();
        public static event EventHandler<SelectionChangedArgs> OnSelectionChanged;

        public static Material selectionMaterial;

        private static GameObject grippedObject = null;
        private static GameObject hoveredObject = null;
        private static GameObject outlinedObject = null;

        public static GameObject activeCamera = null;
        public static event EventHandler<ActiveCameraChangedArgs> OnActiveCameraChanged;

        public static void TriggerSelectionChanged()
        {
            SelectionChangedArgs args = new SelectionChangedArgs();
            fillSelection(ref args.selectionBefore);
            EventHandler<SelectionChangedArgs> handler = OnSelectionChanged;
            if (handler != null)
            {
                handler(null, args);
            }
        }

        public static void TriggerCurrentCameraChanged()
        {
            ActiveCameraChangedArgs args = new ActiveCameraChangedArgs();
            args.activeCamera = activeCamera;
            EventHandler<ActiveCameraChangedArgs> handler = OnActiveCameraChanged;
            if (handler != null)
            {
                handler(null, args);
            }
        }

        public static void fillSelection(ref Dictionary<int, GameObject> s)
        {
            foreach (KeyValuePair<int, GameObject> data in selection)
                s[data.Key] = data.Value;
        }        

        public static bool IsSelected(GameObject gObject)
        {
            return selection.ContainsKey(gObject.GetInstanceID());
        }

        private static void SetRecursiveLayer(GameObject gObject, string layerName)
        {
            gObject.layer = LayerMask.NameToLayer(layerName); // TODO: init in one of the singletons
            for(int i = 0; i < gObject.transform.childCount; i++)
            {
                SetRecursiveLayer(gObject.transform.GetChild(i).gameObject, layerName);
            }
        }

        static void SetCurrentCamera(GameObject obj)
        {
            Camera cam = obj.GetComponentInChildren<Camera>(true);
            if (null == cam)
                return;
            if (activeCamera == obj)
                return;

            if (null != activeCamera)
            {
                //SetCameraEnabled(currentCamera, false);
                activeCamera.GetComponentInChildren<Camera>(true).gameObject.SetActive(false);
            }
            activeCamera = obj;
            if(null != activeCamera)
            {
                //SetCameraEnabled(currentCamera, true);
                cam.gameObject.SetActive(true);
            }
            TriggerCurrentCameraChanged();
        }

        static void SetCameraEnabled(GameObject obj, bool value)
        {
            Camera cam = obj.GetComponentInChildren<Camera>(true);
            if (cam)
            {
                cam.gameObject.SetActive(value);
            }
        }

        static void UpdateCurrentObjectOutline()
        {
            if(outlinedObject)
            {
                RemoveFromHover(outlinedObject);
                outlinedObject = null;
            }

            if(grippedObject)
            {
                outlinedObject = grippedObject;
            }
            else
            {
                if (hoveredObject)
                {
                    outlinedObject = hoveredObject;
                    VRInput.SendHapticImpulse(VRInput.rightController, 0, 0.1f, 0.1f);
                }
            }

            if (outlinedObject)
                AddToHover(outlinedObject);
        }

        public static GameObject GetHoveredObject()
        {
            return hoveredObject;
        }

        public static void SetHoveredObject(GameObject obj)
        {
            hoveredObject = obj;
            UpdateCurrentObjectOutline();
        }

        public static GameObject GetGrippedObject()
        {
            return grippedObject;
        }

        public static void SetGrippedObject(GameObject obj)
        {
            grippedObject = obj;
            UpdateCurrentObjectOutline();
            TriggerSelectionChanged();
        }

        public static List<GameObject> GetObjects()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            if (grippedObject && !IsSelected(grippedObject))
            {
                gameObjects.Add(grippedObject);
            }
            else
            {
                foreach (GameObject obj in selection.Values)
                    gameObjects.Add(obj);
            }
            return gameObjects;
        }


        public static bool IsHandleSelected()
        {
            bool handleSelected = false;
            List<GameObject> objects = GetObjects();

            if (objects.Count == 1)
            {
                foreach (GameObject obj in objects)
                {
                    if (obj.GetComponent<UIHandle>())
                        handleSelected = true;
                }
            }
            return handleSelected;
        }

        public static bool AddToHover(GameObject gObject)
        {
            if (gObject)
            {
                SetRecursiveLayer(gObject, "Hover");
                SetCurrentCamera(gObject);
            }

            return true;
        }

        public static bool RemoveFromHover(GameObject gObject)
        {
            string layerName = "Default";

            if (selection.ContainsKey(gObject.GetInstanceID()))
            {
                layerName = "Selection";
            }
            else if (  gObject.GetComponent<LightController>()
                    || gObject.GetComponent<CameraController>()
                    || gObject.GetComponent<UIHandle>())
            {
                layerName = "UI";
            }

            if (gObject)
            {
                SetRecursiveLayer(gObject, layerName);
            }

            return true;
        }

        public static bool AddToSelection(GameObject gObject)
        {
            if (selection.ContainsKey(gObject.GetInstanceID()))
                return false;

            if (gObject.GetComponent<UIHandle>())
                return false;

            SelectionChangedArgs args = new SelectionChangedArgs();
            fillSelection(ref args.selectionBefore);

            selection.Add(gObject.GetInstanceID(), gObject);

            SetCurrentCamera(gObject);

            SetRecursiveLayer(gObject, "Selection");

            EventHandler<SelectionChangedArgs> handler = OnSelectionChanged;
            if (handler != null)
            {
                handler(null, args);
            }

            return true;
        }

        public static bool RemoveFromSelection(GameObject gObject)
        {
            if (!selection.ContainsKey(gObject.GetInstanceID()))
                return false;

            SelectionChangedArgs args = new SelectionChangedArgs();
            fillSelection(ref args.selectionBefore);

            selection.Remove(gObject.GetInstanceID());

            
            string layerName = "Default";
            if (gObject.GetComponent<LightController>()
             || gObject.GetComponent<CameraController>()
             || gObject.GetComponent<UIHandle>())
            {
                layerName = "UI";
            }

            SetRecursiveLayer(gObject, layerName);

            EventHandler<SelectionChangedArgs> handler = OnSelectionChanged;
            if (handler != null)
            {
                handler(null, args);
            }

            return true;
        }

        public static void ClearSelection()
        {
            foreach (KeyValuePair<int, GameObject> data in selection)
            {
                string layerName = "Default";
                if (data.Value.GetComponent<LightController>()
                 || data.Value.GetComponent<CameraController>()
                 || data.Value.GetComponent<UIHandle>())
                {
                    layerName = "UI";
                }

                SetRecursiveLayer(data.Value, layerName);
            }

            SelectionChangedArgs args = new SelectionChangedArgs();
            fillSelection(ref args.selectionBefore);

            selection.Clear();

            EventHandler<SelectionChangedArgs> handler = OnSelectionChanged;
            if (handler != null)
            {
                handler(null, args);
            }
        }
    }
}
