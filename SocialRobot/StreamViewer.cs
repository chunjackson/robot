/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013-2016 Intel Corporation. All Rights Reserved.

*********************************************************************************/

using System;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using System.Collections.Generic;
using Windows.Devices.Perception;
using Windows.UI.Xaml.Controls;

using Intel.RealSense;

namespace RSSDK
{
    public sealed class StreamViewer : SampleFrameworkRealSense
    {
        public delegate void SampleArrivedContainer(Sample sample);
        public event SampleArrivedContainer SampleArrived;

        bool synchronized = false;
        bool streaming = false;

        private List<SampleReader> readers = new List<SampleReader>();

        public StreamViewer(MainPage page) : base(page)
        {
            m_senseManager.StatusChanged += OnStatus;
        }

        // Initialize and start streaming pipeline
        internal async void Start()
        {
            if (m_senseManager == null || CurrentDevice == null || streaming)
                return;
            try
            {
                
                m_page.WarningText = "Waiting...";

                if (m_currentProfiles.Count < 1)
                {
                    m_page.WarningText = "No stream selected";
                    return;
                }
                if (!isCurrentProfileSetValid())
                {
                    m_page.WarningText = "Unsupported profile combination";
                    return;
                }

                if (IsSync)
                {
                    // Create one reader for all selected streams in sync mode
                    var reader = SampleReader.Activate(m_senseManager);
                    // Attach sample processing handler
                    reader.SampleArrived += ProcessSample;
                    // Apply all selected stream profiles
                    reader.EnableStreams(m_currentProfiles);
                    readers.Add(reader);
                }
                else
                {
                    // Create individual reader for each stream
                    foreach (var profile in m_currentProfiles)
                    {
                        // Default properties for auto selection
                        int width = 0;
                        int height = 0;
                        float fps = 0;
                        if (profile.Value != null)
                        {
                            width = profile.Value.Width;
                            height = profile.Value.Height;
                            // Convert milliseconds to frame-per-second
                            fps = (float)Math.Round(1000.0 / profile.Value.FrameDuration.TotalMilliseconds);
                        }
                        // Activate reader for specific stream
                        var reader = SampleReader.Activate(m_senseManager);
                        // Attach sample processing handler
                        reader.SampleArrived += ProcessSample;
                        // Apply stream profile
                        reader.EnableStream(profile.Key, width, height, fps);
                        readers.Add(reader);
                    }
                }

                // Choose specific device for streaming
                m_senseManager.CaptureManager.FilterByDeviceInfo(CurrentDevice.DeviceInfo);
                // Initialize streaming pipeline
                var sts = await m_senseManager.InitAsync();
                if (sts < Status.STATUS_NO_ERROR)
                {
                    m_page.ErrorText = "Init failed";
                    return;
                }

                // Start streaming
                if ((sts = m_senseManager.StreamFrames()) == Intel.RealSense.Status.STATUS_NO_ERROR)
                    m_page.StatusText = "Streaming";
                else
                    m_page.ErrorText = "Failed to stream: " + sts.ToString();

                // Start gui elements
                m_page.Started();
                streaming = true;
            }
            catch (Exception exc)
            {
                m_page.ErrorText = "Exception: " + exc.ToString();
            }
        }

        internal void Stop()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                m_page.WarningText = "Waiting...";
                // Stop the SenseManager streaming
                m_senseManager.CloseDown();
                // Cleanup all stream readers
                foreach (SampleReader reader in readers)
                {
                    reader.Dispose();
                }
                readers.Clear();
                // Stop gui objects
                m_page.CloseViewer();
                m_page.StatusText = "Stopped";
                m_page.Stopped();
                m_page.ResetFps();
                streaming = false;
            });
        }

        public void OnStatus(object sender, StatusChangedEventArgs args)
        {
            if (args.Status < Status.STATUS_NO_ERROR)
                m_page.ErrorText = "Code = " + args.Status.ToString();
        }

        public bool IsSync
        {
            get { return synchronized; }
            set { synchronized = value; }
        }

        // Sample processing
        private async void ProcessSample(Object module, SampleArrivedEventArgs args)
        {
            // Send arrived sample to render
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SampleArrived(args.Sample);
                args.Sample.Dispose();
            });
        }
    }
}
