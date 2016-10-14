/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013-2016 Intel Corporation. All Rights Reserved.

*********************************************************************************/

using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Intel.RealSense.Face;
using Intel.RealSense;
using System;

namespace RSSDK
{
    public sealed class RealSenseFace : SampleFrameworkRealSense
    {
        public delegate void FaceDisplayContainer(FaceData data, Sample sm);
        public event FaceDisplayContainer FaceFrameProcessed;

        private FaceModule m_faceModule;

        public RealSenseFace(MainPage page) : base(page)
        {
        }

        // Initialize and start Face processing pipeline
        internal async void Start()
        {
            if (m_senseManager == null || CurrentDevice == null)
                return;
            try
            {
                // Enable Face processing
                m_faceModule = FaceModule.Activate(m_senseManager);
                if (m_faceModule == null)
                {
                    m_page.ErrorText = "Cannot create FaceModule";
                    return;
                }

                // Attach Face data handler
                m_faceModule.FrameProcessed += OnFrameProcessed;
                m_senseManager.StatusChanged += OnStatus;

                // Set Face module configuration
                FaceConfiguration faceConfiguration = m_faceModule.CreateActiveConfiguration();
                if (faceConfiguration == null)
                {
                    m_page.ErrorText = "Cannot create FaceConfiguration";
                    return;
                }
                faceConfiguration.Detection.IsEnabled = true;
                faceConfiguration.Detection.MaxTrackedFaces = 1;

                faceConfiguration.Landmarks.IsEnabled = true;
                faceConfiguration.Landmarks.MaxTrackedFaces = 1;
                faceConfiguration.Landmarks.NumLandmarks = 78;

                faceConfiguration.Pose.IsEnabled = false;

                faceConfiguration.ApplyChanges();
                //TODO: Missing API - FaceConfiguration.Dispose

                m_page.WarningText = "Waiting...";
                m_senseManager.CaptureManager.FilterByDeviceInfo(CurrentDevice.DeviceInfo);
                var sts = await m_senseManager.InitAsync();
                if (sts < Status.STATUS_NO_ERROR)
                {
                    m_page.ErrorText = "Init failed";
                    return;
                }
                m_page.ResetFps();
                // Start streaming
                sts = m_senseManager.StreamFrames();
                m_page.StatusText = sts >= Status.STATUS_NO_ERROR ? "Streaming" : "StreamFrames failed";
                m_page.Started();
            }
            catch (Exception exc)
            {
                m_page.ErrorText = "Exception: " + exc.ToString();
            }
        }

        // Stop processing
        internal void Stop()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                m_senseManager.CloseDown();
                m_page.WarningText = "Waiting...";
                m_page.Stopped();
                m_page.StatusText = "Stopped";
                m_page.CloseViewer();
            });
        }

        public void OnStatus(object sender, StatusChangedEventArgs args)
        {
            if (args.Status < Status.STATUS_NO_ERROR)
            {
                m_page.ErrorText = args.Status.ToString();
                m_page.Stopped();
            }
        }

        private async void OnFrameProcessed(object obj, FrameProcessedEventArgs data)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                m_page.CountFrame(0);
                lock (data)
                {
                    FaceFrameProcessed(data.Data, m_faceModule.Sample);
                }
                
            });
        }
    }
}
