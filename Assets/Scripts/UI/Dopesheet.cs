﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRtist
{
    public class Dopesheet : MonoBehaviour
    {
        [SpaceHeader("Sub Widget Refs", 6, 0.8f, 0.8f, 0.8f)]
        [SerializeField] private Transform mainPanel = null;
        [SerializeField] private UITimeBar timeBar = null;
        [SerializeField] private UILabel firstFrameLabel = null;
        [SerializeField] private UILabel lastFrameLabel = null;
        [SerializeField] private UILabel currentFrameLabel = null;

        [SpaceHeader("Callbacks", 6, 0.8f, 0.8f, 0.8f)]
        public IntChangedEvent onAddKeyframeEvent = new IntChangedEvent();
        public IntChangedEvent onRemoveKeyframeEvent = new IntChangedEvent();
        public IntChangedEvent onPreviousKeyframeEvent = new IntChangedEvent();
        public IntChangedEvent onNextKeyframeEvent = new IntChangedEvent();
        public IntChangedEvent onChangeCurrentKeyframeEvent = new IntChangedEvent();

        private int firstFrame = 0;
        private int lastFrame = 250;
        private int currentFrame = 0;

        public int FirstFrame { get { return firstFrame; } set { firstFrame = value; UpdateFirstFrame(); } }
        public int LastFrame { get { return lastFrame; } set { lastFrame = value; UpdateLastFrame(); } }
        public int CurrentFrame { get { return currentFrame; } set { currentFrame = value; UpdateCurrentFrame(); } }

        private GameObject keyframePrefab;

        void Start()
        {
            mainPanel = transform.Find("MainPanel");
            if (mainPanel != null)
            {
                timeBar = mainPanel.Find("TimeBar").GetComponent<UITimeBar>();
                firstFrameLabel = mainPanel.Find("FirstFrameLabel").GetComponent<UILabel>();
                lastFrameLabel = mainPanel.Find("LastFrameLabel").GetComponent<UILabel>();
                currentFrameLabel = mainPanel.Find("CurrentFrameLabel").GetComponent<UILabel>();

                keyframePrefab = Resources.Load<GameObject>("Prefabs/UI/DOPESHEET/Keyframe");
            }
        }

        private void UpdateFirstFrame()
        {
            if (firstFrameLabel != null)
            {
                firstFrameLabel.Text = firstFrame.ToString();
            }
            if (timeBar != null)
            {
                timeBar.MinValue = firstFrame; // updates knob position
            }
        }

        private void UpdateLastFrame()
        {
            if (lastFrameLabel != null)
            {
                lastFrameLabel.Text = lastFrame.ToString();
            }
            if (timeBar != null)
            {
                timeBar.MaxValue = lastFrame; // updates knob position
            }
        }

        private void UpdateCurrentFrame()
        {
            if (currentFrameLabel != null)
            {
                currentFrameLabel.Text = currentFrame.ToString();
            }
            if (timeBar != null)
            {
                timeBar.Value = currentFrame; // changes the knob's position
            }
        }

        public void Show(bool doShow)
        {
            if (mainPanel != null)
            {
                mainPanel.gameObject.SetActive(doShow);

                // TMP
                FirstFrame = 11;
                LastFrame = 265;
                CurrentFrame = 54;
            }
        }

        public void UpdateFromController(ParametersController controller)
        {
            Dictionary<string, AnimationChannel> channels = controller.GetAnimationChannels();
            foreach(AnimationChannel channel in channels.Values)
            {
                if(channel.name == "location[0]")
                {
                    Transform keyframes = transform.Find("MainPanel/FakeTrackButton/Keyframes");
                    for (int i = keyframes.childCount - 1 ; i >= 0 ; i--)
                    {
                        Destroy(keyframes.GetChild(i).gameObject);
                    }

                    foreach(AnimationKey key in channel.keys)
                    {
                        GameObject keyframe = GameObject.Instantiate(keyframePrefab, keyframes);

                        float currentValue = key.time;
                        float pct = (float)(currentValue - firstFrame) / (float)(lastFrame - firstFrame);

                        float startX = 0.0f;
                        float endX = timeBar.width;
                        float posX = startX + pct * (endX - startX);

                        Vector3 knobPosition = new Vector3(posX, 0.0f, 0.0f);

                        keyframe.transform.localPosition = knobPosition;
                    }
                    
                }
            }
            // use cameraController keyframes arrays to update the tracks.
            //cameraController.position_kf;
            //cameraController.rotation_kf;
            //cameraController.focal_kf;
        }

        public void Clear()
        {
            // empty all tracks, no camera is selected.
        }
        
        // called by the slider when moved
        public void OnChangeCurrentFrame(int i)
        {
            CurrentFrame = i;
            onChangeCurrentKeyframeEvent.Invoke(i);
        }

        public void OnPrevKeyFrame()
        {
            onPreviousKeyframeEvent.Invoke(CurrentFrame);
        }

        public void OnNextKeyFrame()
        {
            onNextKeyframeEvent.Invoke(CurrentFrame);
        }

        public void OnAddKeyFrame()
        {
            onAddKeyframeEvent.Invoke(CurrentFrame);
        }

        public void OnRemoveKeyFrame()
        {
            onRemoveKeyframeEvent.Invoke(CurrentFrame);
        }
    }
}
