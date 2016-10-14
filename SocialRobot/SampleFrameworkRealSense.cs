/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013-2016 Intel Corporation. All Rights Reserved.

*********************************************************************************/

using System;
using System.Collections.Generic;
using Windows.Devices.Perception;
using Intel.RealSense;

namespace RSSDK
{
    // Common part for samples to interact with RealSense devices and streams
    public class SampleFrameworkRealSense
    {
        public delegate void DeviceChangedContainer(IReadOnlyList<Device> devices);
        public event DeviceChangedContainer DevicesListUpdated;

        public SenseManager m_senseManager = null;
        protected MainPage m_page;
        public SampleFrameworkRealSense(MainPage page)
        {
            m_page = page;
            m_senseManager = SenseManager.CreateInstance();
            if (m_senseManager == null)
                page.ErrorText = "SenseManager is not created";
            PopulateDevice();
        }

        public Device CurrentDevice
        {
            get { return m_currentDevice; }
            set { m_currentDevice = value; }
        }

        Device m_currentDevice = null;
        protected Dictionary<StreamType, PerceptionVideoProfile> m_currentProfiles = new Dictionary<StreamType, PerceptionVideoProfile>();

        public void onProfileChanged(StreamType streamType, PerceptionVideoProfile profile)
        {
            m_currentProfiles[streamType] = profile;
        }
        public void onProfileRemoved(StreamType streamType)
        {
            m_currentProfiles.Remove(streamType);
        }

        protected async void PopulateDevice()
        {
            var ms_devices = await Device.FindAllAsync(m_senseManager);
            DevicesListUpdated(ms_devices);
        }

        // Validate current profile combination can be enabled together
        public bool isCurrentProfileSetValid()
        {
            return m_currentDevice == null || m_currentDevice.IsStreamProfileSetValid(m_currentProfiles);
        }

        // Validate current profile combination with new proifile can be enabled together
        public bool isProfileValid(StreamType streamType, PerceptionVideoProfile profile)
        {
            Dictionary<StreamType, PerceptionVideoProfile> profiles = m_currentProfiles;
            profiles[streamType] = profile;
            return m_currentDevice == null || m_currentDevice.IsStreamProfileSetValid(m_currentProfiles);
        }

        // Combine supported profiles for all stream types
        public Dictionary<StreamType, IReadOnlyList<PerceptionVideoProfile>> GetStreamTypeToProfiles()
        {
            Dictionary<StreamType, IReadOnlyList<PerceptionVideoProfile>> streamTypeToProfiles = new Dictionary<StreamType, IReadOnlyList<PerceptionVideoProfile>>();
            PerceptionColorFrameSource colorSource = m_currentDevice.Sources[StreamType.STREAM_TYPE_COLOR] as PerceptionColorFrameSource;
            if (colorSource != null && colorSource.SupportedVideoProfiles.Count > 0)
                streamTypeToProfiles[StreamType.STREAM_TYPE_COLOR] = colorSource.SupportedVideoProfiles;

            PerceptionDepthFrameSource depthSource = m_currentDevice.Sources[StreamType.STREAM_TYPE_DEPTH] as PerceptionDepthFrameSource;
            if (depthSource != null && depthSource.SupportedVideoProfiles.Count > 0)
                streamTypeToProfiles[StreamType.STREAM_TYPE_DEPTH] = depthSource.SupportedVideoProfiles;

            PerceptionInfraredFrameSource infraredSource = m_currentDevice.Sources[StreamType.STREAM_TYPE_IR] as PerceptionInfraredFrameSource;
            if (infraredSource != null && infraredSource.SupportedVideoProfiles.Count > 0)
                streamTypeToProfiles[StreamType.STREAM_TYPE_IR] = infraredSource.SupportedVideoProfiles;
            return streamTypeToProfiles;
        }
    }
}
