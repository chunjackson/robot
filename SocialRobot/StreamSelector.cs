/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013-2016 Intel Corporation. All Rights Reserved.

*********************************************************************************/

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Imaging;
using Windows.Devices.Perception;

using Intel.RealSense;

namespace RSSDK
{
    public class StreamSelector
    {
        public MyComboBox deviceBox = null;
        public MyComboBox fpsBox = null;
        public MyComboBox colorBox = null;
        public MyComboBox depthBox = null;
        public MyComboBox infraredBox = null;
        public MyComboBox formatBox = null;
        Thickness margin;
        SampleFrameworkRealSense m_rsDataModel = null;

        private DependencyProperty dp_device;
        private DependencyProperty dp_profile;

        bool isDevice = false;
        bool isFPS = false;
        bool isDepth = false;
        bool isColor = false;
        bool isIR = false;
        bool isFormat = false;

        public Thickness Margin
        {
            get { return margin; }
        }

        public bool IsEnabled
        {
            get
            {
                if (deviceBox != null) return deviceBox.IsEnabled;
                if (fpsBox != null) return fpsBox.IsEnabled;
                if (colorBox != null) return colorBox.IsEnabled;
                if (depthBox != null) return depthBox.IsEnabled;
                if (infraredBox != null) return infraredBox.IsEnabled;
                if (formatBox != null) return formatBox.IsEnabled;
                return false;
            }
            set
            {
                if (deviceBox != null) deviceBox.IsEnabled = value;
                if (fpsBox != null) fpsBox.IsEnabled = value;
                if (colorBox != null) colorBox.IsEnabled = value;
                if (depthBox != null) depthBox.IsEnabled = value;
                if (infraredBox != null) infraredBox.IsEnabled = value;
                if (formatBox != null) formatBox.IsEnabled = value;
            }
        }

        public void Enable(bool dev, bool fps, bool color, bool depth, bool ir, bool format)
        {
            isDevice = dev;
            isFPS = fps;
            isColor = color;
            isDepth = depth;
            isIR = ir;
            isFormat = format;
            deviceBox.Visibility = isDevice ? Visibility.Visible : Visibility.Collapsed;
            deviceBox.IsEnabled = isDevice;
            fpsBox.Visibility = isFPS ? Visibility.Visible : Visibility.Collapsed;
            fpsBox.IsEnabled = isFPS;
            colorBox.Visibility = isColor ? Visibility.Visible : Visibility.Collapsed;
            colorBox.IsEnabled = isColor;
            depthBox.Visibility = isDepth ? Visibility.Visible : Visibility.Collapsed;
            depthBox.IsEnabled = isDepth;
            infraredBox.Visibility = isIR ? Visibility.Visible : Visibility.Collapsed;
            infraredBox.IsEnabled = isIR;
            formatBox.Visibility = isFormat ? Visibility.Visible : Visibility.Collapsed;
            formatBox.IsEnabled = isFormat;
        }

        public StreamSelector(SampleFrameworkRealSense rsData, Panel panel)
        {
            m_rsDataModel = rsData;
            m_rsDataModel.DevicesListUpdated += OnDeviceListArrived;
            dp_device = DependencyProperty.Register("Device", typeof(Object), typeof(ToggleMenuFlyoutItem), null);
            dp_profile = DependencyProperty.Register("Profile", typeof(Object), typeof(ToggleMenuFlyoutItem), null);

            int[] height = { 45, 45, 45, 45, 45, 45 };
            int[] width = { 320, 140, 170, 170, 170, 175 }; // device - fps - color - depth - ir - format
            margin.Left = 10;
            margin.Top = height[0] + height[1] + 5;
            margin.Bottom = 0;
            margin.Right = 10;

            // device
            int posX = 10;
            deviceBox = new MyComboBox();
            deviceBox.Height = height[0];
            deviceBox.Width = width[0];
            deviceBox.Margin = new Thickness(posX, 0, 0, 0);
            ComboBoxItem item = new ComboBoxItem();
            item.Content = "Loading Devices...";
            deviceBox.Items.Add(item);
            deviceBox.SelectedIndex = 0;
            if (panel != null)
            {
                if (panel.Children.Count > 0)
                {
                    var child = panel.Children[0] as Panel;
                    if (child != null)
                    {
                        Thickness childMargin = child.Margin;
                        childMargin.Top += margin.Top;
                        child.Margin = childMargin;
                    }
                }
                panel.Children.Add(deviceBox);
                isDevice = true;
            }
            deviceBox.SelectionChanged += DeviceBox_SelectionChanged;

            // color
            posX += width[0] + 15;
            colorBox = new MyComboBox();
            colorBox.Height = height[2];
            colorBox.Width = width[2];
            colorBox.Margin = new Thickness(posX, 0, 0, 0);
            if (panel != null) panel.Children.Insert(0, colorBox);
            isColor = true;
            colorBox.SelectionChanged += ColorBox_SelectionChanged;
            colorBox.Opened += ColorBox_DropDownOpened;
            colorBox.Closed += Validate;

            // depth
            posX += width[2] + 5;
            depthBox = new MyComboBox();
            depthBox.Height = height[3];
            depthBox.Width = width[3];
            depthBox.Margin = new Thickness(posX, 0, 0, 0);
            if (panel != null) panel.Children.Insert(0, depthBox);
            isDepth = true;
            depthBox.SelectionChanged += DepthBox_SelectionChanged;
            depthBox.Opened += DepthBox_DropDownOpened;
            depthBox.Closed += Validate;

            // infrared
            posX += width[3] + 5;
            infraredBox = new MyComboBox();
            infraredBox.Height = height[4];
            infraredBox.Width = width[4];
            infraredBox.Margin = new Thickness(posX, 0, 0, 0);
            infraredBox.SelectionChanged += InfraredBox_SelectionChanged;
            if (panel != null) panel.Children.Insert(0, infraredBox);
            isIR = true;
            infraredBox.Opened += InfraredBox_DropDownOpened;
            infraredBox.Closed += Validate;

            posX += width[4] + 5;
            margin.Right = posX;

            // fps
            posX = 10;
            fpsBox = new MyComboBox();
            fpsBox.Height = height[1];
            fpsBox.Width = width[1];
            fpsBox.Margin = new Thickness(posX, height[1] + 5, 0, 0);
            if (panel != null) panel.Children.Insert(0, fpsBox);
            isFPS = true;
            fpsBox.SelectionChanged += FpsBox_SelectionChanged;
            fpsBox.Closed += Validate;

            // format
            posX += width[1] + 5;
            formatBox = new MyComboBox();
            formatBox.Height = height[5];
            formatBox.Width = width[5];
            formatBox.Margin = new Thickness(posX, height[5] + 5, 0, 0);
            if (panel != null) panel.Children.Insert(0, formatBox);
            isFormat = true;
            formatBox.SelectionChanged += FormatBox_SelectionChanged;
            formatBox.Closed += Validate;

            posX += width[5] + 5;
            if (posX> margin.Right)
                margin.Right = posX;

        }

        private void FpsBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems == null || args.AddedItems.Count == 0)
                return;
            FpsComboBoxItem selItem = args.AddedItems[0] as FpsComboBoxItem;
            PopulateStreamsFromDevice(selItem.Fps);
        }

        private void FormatBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems == null || args.AddedItems.Count == 0)
                return;
            ComboBoxItem selItem = args.AddedItems[0] as ComboBoxItem;
            if (fpsBox != null)
            {
                FpsComboBoxItem fpsBoxItem = fpsBox.SelectedItem as FpsComboBoxItem;
                PopulateStreamsFromDevice(fpsBoxItem.Fps);
            }
        }

        private void StreamBox_SelectionChanged(object sender, SelectionChangedEventArgs args, StreamType streamType)
        {
            if (args.AddedItems == null || args.AddedItems.Count == 0)
                return;
            ComboBoxItem selItem = args.AddedItems[0] as ComboBoxItem;
            if (streamTypeToCombobox(streamType).SelectedIndex > 0)
                m_rsDataModel.onProfileChanged(streamType, selItem.GetValue(dp_profile) as PerceptionVideoProfile);
            else
                m_rsDataModel.onProfileRemoved(streamType);
        }

        private void ColorBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            StreamBox_SelectionChanged(sender, args, StreamType.STREAM_TYPE_COLOR);
        }

        private void DepthBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            StreamBox_SelectionChanged(sender, args, StreamType.STREAM_TYPE_DEPTH);
        }

        private void InfraredBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            StreamBox_SelectionChanged(sender, args, StreamType.STREAM_TYPE_IR);
        }

        private void DeviceBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems == null || args.AddedItems.Count == 0)
                return;

            ComboBoxItem selItem = args.AddedItems[0] as ComboBoxItem;
            m_rsDataModel.CurrentDevice = selItem.GetValue(dp_device) as Device;
            if (m_rsDataModel.CurrentDevice == null)
                return;
            PopulateFPSFromDevice();
            PopulateFormatFromDevice();
        }

        private void Validate(object sender, EventArgs arg)
        {
            var color = colorBox.SelectedItem as ComboBoxItem;
            var depth = depthBox.SelectedItem as ComboBoxItem;
            var ir = infraredBox.SelectedItem as ComboBoxItem;

            if (m_rsDataModel.isCurrentProfileSetValid())
            {
                colorBox.Background = new SolidColorBrush(Colors.MintCream);
                depthBox.Background = new SolidColorBrush(Colors.MintCream);
                infraredBox.Background = new SolidColorBrush(Colors.MintCream);
            }
            else
            {
                colorBox.Background = new SolidColorBrush(Colors.LightPink);
                depthBox.Background = new SolidColorBrush(Colors.LightPink);
                infraredBox.Background = new SolidColorBrush(Colors.LightPink);
            }
        }

        private void streamBox_DropDownOpened(StreamType streamType)
        {
            int numberOfInvalids = 0; //declare a variable that counts a number of invalid sets Depth-IR
            ComboBox streamBox = streamTypeToCombobox(streamType);
            foreach (ComboBoxItem item in streamBox.Items)
            {
                if (m_rsDataModel.isProfileValid(streamType, item.GetValue(dp_profile) as PerceptionVideoProfile))
                    item.Background = new SolidColorBrush(Colors.MintCream);
                else
                {
                    item.Background = new SolidColorBrush(Colors.LightPink);
                    numberOfInvalids++;
                }
            }
            if (numberOfInvalids == streamBox.Items.Count - 2) //if all sets Depth-IR are invalid (except "None" and "Auto")
            {
                //set color of "Auto" item in infraredBox to Pink.
                (streamBox.Items[1] as ComboBoxItem).Background = new SolidColorBrush(Colors.LightPink);

                //need to somehow block the set Color-Depth-IR if "Auto" is selected
            }
        }

        private void ColorBox_DropDownOpened(object sender, EventArgs arg)
        {
            streamBox_DropDownOpened(StreamType.STREAM_TYPE_COLOR);
        }

        private void DepthBox_DropDownOpened(object sender, EventArgs args)
        {
            streamBox_DropDownOpened(StreamType.STREAM_TYPE_DEPTH);
        }

        private void InfraredBox_DropDownOpened(object sender, EventArgs args)
        {
            streamBox_DropDownOpened(StreamType.STREAM_TYPE_IR);
        }

        internal void PopulateFPSFromDevice()
        {
            fpsBox.Items.Clear();
            ISet<int> fpsSet = new SortedSet<int>();
            Dictionary<StreamType, IReadOnlyList<PerceptionVideoProfile>> streamTypeToProfiles = m_rsDataModel.GetStreamTypeToProfiles();
            foreach (var typeProfilesPair in streamTypeToProfiles)
                foreach (PerceptionVideoProfile profile in typeProfilesPair.Value)
                    fpsSet.Add((int)Math.Round(1000.0 / profile.FrameDuration.TotalMilliseconds));
            foreach (int fps in fpsSet)
                fpsBox.Items.Add(new FpsComboBoxItem(fps));
            // add all fps item
            fpsBox.Items.Insert(0, new FpsComboBoxItem());

            if (fpsBox.Items.Count > 0)
                fpsBox.SelectedIndex = 0;
        }

        private String streamTypeToString(Intel.RealSense.StreamType type)
        {
            switch (type)
            {
                case StreamType.STREAM_TYPE_COLOR:
                    return "Color";
                case StreamType.STREAM_TYPE_DEPTH:
                    return "Depth";
                case StreamType.STREAM_TYPE_IR:
                    return "IR";
            }
            return "Unknown";
        }

        private MyComboBox streamTypeToCombobox(Intel.RealSense.StreamType type)
        {
            switch (type)
            {
                case StreamType.STREAM_TYPE_COLOR:
                    return colorBox;
                case StreamType.STREAM_TYPE_DEPTH:
                    return depthBox;
                case StreamType.STREAM_TYPE_IR:
                    return infraredBox;
            }
            return null;
        }

        private bool isStreamTypeEnabled(Intel.RealSense.StreamType type)
        {
            switch (type)
            {
                case StreamType.STREAM_TYPE_COLOR:
                    return isColor;
                case StreamType.STREAM_TYPE_DEPTH:
                    return isIR;
                case StreamType.STREAM_TYPE_IR:
                    return isFormat;
            }
            return false;
        }

        internal void PopulateStreamsFromDevice(int fps)
        {
            BitmapPixelFormat colorFormat = BitmapPixelFormat.Unknown;
            if (formatBox != null && formatBox.Items.Count > 0)
            {
                FormatComboBoxItem form = formatBox.SelectedItem as FormatComboBoxItem;
                colorFormat = form.PixelFormat;
            }

            Dictionary<StreamType, IReadOnlyList<PerceptionVideoProfile>> streamTypeToProfiles = m_rsDataModel.GetStreamTypeToProfiles();
            foreach (var typeProfilesPair in streamTypeToProfiles)
            {
                StreamType type = typeProfilesPair.Key;
                MyComboBox streamCombobox = streamTypeToCombobox(type);
                streamCombobox.Items.Clear();

                ComboBoxItem noneBox = new ComboBoxItem();
                noneBox.Content = String.Format("{0} ( None )", streamTypeToString(type));
                //noneBox.SetValue(dp_profile, null);
                streamCombobox.Items.Add(noneBox);

                ComboBoxItem autoBoxItem = new ComboBoxItem();
                autoBoxItem.Content = String.Format("{0} ( Auto )", streamTypeToString(type));
                autoBoxItem.SetValue(dp_profile, null);
                autoBoxItem.Background = new SolidColorBrush(Colors.MintCream);
                streamCombobox.Items.Add(autoBoxItem);

                foreach (PerceptionVideoProfile profile in typeProfilesPair.Value)
                {
                    double currFps = Math.Round(1000.0 / profile.FrameDuration.TotalMilliseconds);
                    if (fps != 0 && currFps != fps)
                        continue;
                    if (type == StreamType.STREAM_TYPE_COLOR && !profile.BitmapPixelFormat.Equals(colorFormat) && !colorFormat.Equals(BitmapPixelFormat.Unknown))
                        continue;

                    ComboBoxItem streamBoxItem = new ComboBoxItem();
                    streamBoxItem.Content = String.Format("{0}: {1}x{2}", streamTypeToString(type), profile.Width, profile.Height);
                    if (fps == 0)
                        streamBoxItem.Content = streamBoxItem.Content + String.Format("x{0}", currFps);
                    if (type == StreamType.STREAM_TYPE_IR || colorFormat.Equals(BitmapPixelFormat.Unknown) && type == StreamType.STREAM_TYPE_COLOR)
                        streamBoxItem.Content = streamBoxItem.Content + String.Format(" {0} ", profile.BitmapPixelFormat.ToString());

                    streamBoxItem.SetValue(dp_profile, profile);
                    streamCombobox.Items.Add(streamBoxItem);
                }
                streamCombobox.SelectedIndex = 1;
                if (isStreamTypeEnabled(type))
                {
                    if (streamCombobox.Items.Count > 1)
                    {
                        streamCombobox.Visibility = Visibility.Visible;
                        streamCombobox.IsEnabled = true;
                    }
                    else
                    {
                        streamCombobox.Visibility = Visibility.Collapsed;
                        streamCombobox.IsEnabled = false;
                    }
                }
            }
        }

        public void PopulateFormatFromDevice()
        {
            formatBox.Items.Clear();
            //collect color formats
            PerceptionColorFrameSource colorSource = m_rsDataModel.CurrentDevice.Sources[StreamType.STREAM_TYPE_COLOR] as PerceptionColorFrameSource;
            ISet<BitmapPixelFormat> formatSet = new SortedSet<BitmapPixelFormat>();
            if (colorSource == null || colorSource.SupportedVideoProfiles.Count == 0)
                return;
            foreach (PerceptionVideoProfile profile in colorSource.SupportedVideoProfiles)
                formatSet.Add(profile.BitmapPixelFormat);
            foreach (BitmapPixelFormat pixelFormat in formatSet)
                formatBox.Items.Add(new FormatComboBoxItem(pixelFormat));
            // add all format item
            formatBox.Items.Insert(0, new FormatComboBoxItem());

            formatBox.SelectedIndex = 0;
        }

        private void OnDeviceListArrived(IReadOnlyList<Device> devices)
        {
            deviceBox.Items.Clear();
            if (devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    ComboBoxItem deviceItem = new ComboBoxItem();
                    deviceItem.Content = device.DeviceInfo.Name;
                    deviceItem.DataContext = device;
                    deviceItem.SetValue(dp_device, device);
                    deviceBox.Items.Add(deviceItem);
                }
            }
            else
            {
                ComboBoxItem noCameraitem = new ComboBoxItem();
                noCameraitem.Content = "No cameras found";
                noCameraitem.Name = "NoCameras";
                noCameraitem.SetValue(dp_device, null);
                deviceBox.Items.Add(noCameraitem);
            }

            deviceBox.SelectedIndex = 0;
        }

        private void PopulateStreamsFromFile(Windows.Storage.StorageFile file)
        {
            // TODO: Need to determine which streams are in the file
        }

        private async void LoadPlaybackFile(ComboBoxItem item)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".rssdk");
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var newFile = await file.CopyAsync(Windows.Storage.ApplicationData.Current.TemporaryFolder, file.Name, Windows.Storage.NameCollisionOption.ReplaceExisting);
                ComboBoxItem fileItem = item.FindName("PlaybackFilePlaceholder") as ComboBoxItem;
                fileItem.Content = "Playback " + file.Name;
                fileItem.Height = item.Height;
                fileItem.SetValue(dp_device, newFile);

                (item.FindName("Devices") as ComboBox).SelectedItem = fileItem;
            }
        }

    } // streamSelector

    public sealed class MyComboBox : ComboBox
    {
        public event EventHandler Opened = null;
        public event EventHandler Closed = null;
        protected override void OnDropDownOpened(System.Object e)
        {
            if (Opened != null)
                Opened(e, new EventArgs());
        }
        protected override void OnDropDownClosed(System.Object e)
        {
            if (Closed != null)
                Closed(e, new EventArgs());
        }
    }

    public class FpsComboBoxItem : ComboBoxItem
    {
        public FpsComboBoxItem(int fps = 0)
        {
            m_fps = fps;
            if (fps != 0)
                Content = String.Format("FPS ({0})", fps);
            else
                Content = String.Format("FPS (ALL)");
        }
        public int Fps
        {
            get { return m_fps; }
        }
        private int m_fps;
    }

    public class FormatComboBoxItem : ComboBoxItem
    {
        public FormatComboBoxItem(BitmapPixelFormat pixelFormat = 0)
        {
            m_pixelformat = pixelFormat;
            if (pixelFormat != BitmapPixelFormat.Unknown)
                Content = String.Format("Format ({0})", pixelFormat.ToString());
            else
                Content = String.Format("Format(ALL)", pixelFormat.ToString());
        }
        public BitmapPixelFormat PixelFormat
        {
            get { return m_pixelformat; }
        }
        private BitmapPixelFormat m_pixelformat;
    }

} // SampleFramework