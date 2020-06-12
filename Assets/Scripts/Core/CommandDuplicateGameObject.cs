﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRtist
{
    public class DuplicateInfos
    {
        public GameObject srcObject;
        public GameObject dstObject;
    }
    public class CommandDuplicateGameObject : ICommand
    {
        GameObject srcObject;
        protected GameObject gObject = null;
        protected Transform parent = null;
        protected Vector3 position;
        protected Quaternion rotation;
        protected Vector3 scale;
        public CommandDuplicateGameObject(GameObject copy, GameObject src)
        {
            srcObject = src;
            gObject = copy;
            parent = copy.transform.parent.parent;
        }

        private void SendDuplicate()
        {
            DuplicateInfos duplicateInfos = new DuplicateInfos();
            duplicateInfos.srcObject = srcObject;
            duplicateInfos.dstObject = gObject;
            CommandManager.SendEvent(MessageType.Duplicate, duplicateInfos);
        }

        public override void Undo()
        {
            if (null == gObject) { return; }

            SendToTrash(gObject);
            gObject.transform.parent = Utils.GetTrash().transform;
            Node node = SyncData.nodes[gObject.name];
            node.RemoveInstance(gObject);
        }
        public override void Redo()
        {
            if (null == gObject) { return; }

            gObject.transform.parent = parent;
            gObject.transform.localPosition = position;
            gObject.transform.localRotation = rotation;
            gObject.transform.localScale = scale;
            Node node = SyncData.nodes[gObject.name];
            node.AddInstance(gObject);

            RestoreFromTrash(gObject, parent);
        }
        public override void Submit()
        {
            position = gObject.transform.localPosition;
            rotation = gObject.transform.localRotation;
            scale = gObject.transform.localScale;
            CommandManager.AddCommand(this);
            SendDuplicate();
        }

        public override void Serialize(SceneSerializer serializer)
        {
            Parameters parameters = srcObject.GetComponentInParent<ParametersController>().GetParameters();
            if(parameters.GetType() == typeof(GeometryParameters))
            {
                AssetSerializer assetSerializer = serializer.GetAssetSerializer(parameters.id);
                string transformPath = Utils.BuildTransformPath(srcObject);
                assetSerializer.CreateDuplicateSerializer(transformPath, gObject.name);
            }
            else
            {
                ParametersController parametersController = gObject.GetComponent<ParametersController>();
                if (parametersController)
                {
                    serializer.AddAsset(parametersController);
                }
            }
        }
    }
}