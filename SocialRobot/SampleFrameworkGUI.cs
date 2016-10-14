/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013-2016 Intel Corporation. All Rights Reserved.

*********************************************************************************/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Imaging;
using Windows.Media;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using Intel.RealSense;

namespace RSSDK
{
    public sealed partial class MainPage : Page
    {
        public CounterFPS[] fpsCounterList = null;
        public StatusLine statusLine = null;
        public StreamSelector selector = null;
        public Viewer[] viewerList = null;

        int activeSlot = 0;
        int maxSlots;

        public Brush[] brush = new Brush[4]
        {
            new SolidColorBrush(Colors.Blue),
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Yellow),
        };

        public int MaxSlots
        {
            get { return maxSlots; }
            set { maxSlots = value; if (viewerList != null && maxSlots > viewerList.Length) maxSlots = viewerList.Length; }
        }

        public void EnableCounterFPS(int numSlots)
        {
            fpsCounterList = new CounterFPS[numSlots];
            for (int i = 0; i < numSlots; i++)
            {
                fpsCounterList[i] = new CounterFPS(true);
            }
        }
        public void EnableStatusLine()
        {
            statusLine = new StatusLine();
            RelPanel.Children.Add(statusLine.statusBox);
        }

        public void EnableViewer(int slots, double left)
        {
            if (slots < 1 || slots > 4)
                slots = 1;
            viewerList = new Viewer[slots];
            for (int i = 0; i < slots; i++)
            {
                viewerList[i] = new Viewer(RelPanel, left);
            }
            MaxSlots = slots;
        }

        public void EnableSelector(SampleFrameworkRealSense rsData)
        {
            selector = new StreamSelector(rsData, RelPanel);
        }

        public UIElement ctrlFPS(int slot)
        {
            return fpsCounterList != null ? fpsCounterList[slot].fpsMessage : null;
        }

        public void ResetFps()
        {
            if (fpsCounterList != null)
            {
                for (int i = 0; i < fpsCounterList.Length; i++)
                {
                    fpsCounterList[i].Reset();
                }
            }
        }

        public void CountFrame(int slot)
        {
            if (fpsCounterList != null)
                fpsCounterList[slot].AddFrame();
        }

        public UIElement ctrlStatus
        {
            get { return statusLine != null ? statusLine.statusBox : null; }
        }

        public UIElement ctrlViewer(int slot)
        {
            if (slot < 0 || slot >= viewerList.Length) slot = 0;
            return viewerList[slot] != null ? viewerList[slot].viewerBox : null;
        }

        public int FPS(int slot)
        {
            return fpsCounterList != null ? fpsCounterList[slot].fps : 0;
        }

        public string StatusText
        {
            set
            {
                if (statusLine != null)
                {
                    statusLine.messageType = 0;
                    statusLine.SetStatus(value);
                }
            }
        }

        public string WarningText
        {
            set
            {
                if (statusLine != null)
                {
                    statusLine.messageType = 1;
                    statusLine.SetStatus("WARNING: " + value);
                }
            }
        }
        public string ErrorText
        {
            set
            {
                if (statusLine != null)
                {
                    statusLine.messageType = 2;
                    statusLine.SetStatus("ERROR: " + value);
                }
            }
        }

        public class CounterFPS
        {
            public Windows.UI.Xaml.Controls.TextBlock fpsMessage = null;
            public int fps;
            private Stopwatch timer;
            private int nominalFps;
            private int numFrames;

            public CounterFPS(bool messaging)
            {
                int frameDuration = 30;
                timer = new Stopwatch();
                nominalFps = (int)Math.Round(1000.0 / frameDuration);
                if (messaging)
                {
                    fpsMessage = new TextBlock();
                    fpsMessage.FontSize = 20;
                    fpsMessage.Foreground = new SolidColorBrush(Colors.Yellow);
                    fpsMessage.FontWeight =  FontWeights.Bold;
                    fpsMessage.TextWrapping = TextWrapping.NoWrap;
                    fpsMessage.Margin = new Thickness(10, 0, 0, 0);
                }
            }

            internal async void Update(String line)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    fpsMessage.Text = line;
                });
            }

            public void Reset()
            {
                lock (timer)
                {
                    timer.Restart();
                    numFrames = 0;
                    fps = 0;
                    Update("");
                }
            }

            public void AddFrame()
            {
                lock (timer)
                {
                    if (!timer.IsRunning)
                        timer.Start();

                    numFrames++;

                    if (numFrames > nominalFps)
                    {
                        fps = (int)(numFrames / (timer.ElapsedMilliseconds / 1000.0));
                        timer.Restart();
                        numFrames = 0;

                        // print fps message
                        if (fpsMessage != null)
                            Update(fps + " FPS");
                    }
                }
            }
        } // CounterFPS

        public class StatusLine
        {
            public Border statusBox = null;
            public TextBlock statusText = null;
            public int messageType = 0;

            private SolidColorBrush[] brush = new SolidColorBrush[3]
            {
                new SolidColorBrush(Colors.DarkBlue),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Red)
            };

            public StatusLine()
            {
                var grad = new LinearGradientBrush();
                grad.StartPoint = new Point(0, 0.5);
                grad.EndPoint = new Point(1, 0.5);
                grad.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                var c0 = new GradientStop();
                c0.Color = Colors.LightGray;
                c0.Offset = 0;
                var c1 = new GradientStop();
                c1.Color = Colors.LightGray;
                c1.Offset = 0.5;
                var c2 = new GradientStop();
                c2.Color = Colors.Transparent;
                c2.Offset = 1;
                grad.GradientStops.Add(c0);
                grad.GradientStops.Add(c1);
                grad.GradientStops.Add(c2);

                statusBox = new Border();
                statusBox.Opacity = 0.7;
                statusBox.Background = grad;
                //statusBox.BorderThickness = new Thickness(0, 0, 0, 5);
                //statusBox.BorderBrush = grad;
                statusText = new TextBlock();
                statusText.FontSize = 20;
                statusText.Foreground = brush[0];
                statusText.TextWrapping = TextWrapping.Wrap;
                statusText.Text = "Ready";
                statusText.Margin = new Thickness(10, 0, 0, 0);
                statusText.HorizontalAlignment = HorizontalAlignment.Left;
                statusText.VerticalAlignment = VerticalAlignment.Top;
                statusBox.Child = statusText;
            }

            public async void Update(String line)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    statusText.Foreground = brush[messageType];
                    statusText.Text = line;
                });
            }

            public void SetStatus(string line)
            {
                if (statusText != null)
                    Update(line);
            }

            public void Resize(double left, double top, double width)
            {
                statusBox.Margin = new Thickness(left, top, 0, 0);
                statusBox.Width = width;
                statusText.Width = width;
            }
        } // StatusLine

        public class Viewer
        {
            public Canvas viewerBox;
            public double left = 10;
            public CompositeTransform image_scale;
            float width;
            float height;
            Windows.UI.Xaml.Controls.Image image = null;

            public Viewer(Panel panel, double left)
            {
                viewerBox = new Canvas();
                this.left = left;
                image_scale = new CompositeTransform();
                viewerBox.RenderTransform = image_scale;
                panel.Children.Add(viewerBox);
            }

            public void Flush()
            {
                image = null;
            }

            public void BeginPaint()
            {
                viewerBox.Children.Clear();
                if (image == null)
                    image = new Windows.UI.Xaml.Controls.Image();
                viewerBox.Children.Add(image);
            }

            public void EndPaint(UIElement fps)
            {
                // scale and center image
                if (width == 0 || height == 0) return;
                float w = (float)viewerBox.MaxWidth;
                float h = (float)viewerBox.MaxHeight;
                float sX = w / width;
                float sY = h / height;
                if (sX > sY) sX = sY;
                image_scale.ScaleX = image_scale.ScaleY = sX;
                image_scale.TranslateX = (w - width * image_scale.ScaleX) / 2;
                image_scale.TranslateY = (h - height * image_scale.ScaleY) / 2;
                viewerBox.Width = width;
                viewerBox.Height = height;

                if (fps != null)
                {
                    CompositeTransform invtr = fps.RenderTransform as CompositeTransform;
                    if (invtr == null)
                    {
                        fps.RenderTransform = invtr = new CompositeTransform();
                    }
                    invtr.ScaleX = invtr.ScaleY = 1 / sX;
                    viewerBox.Children.Add(fps);
                }
            }

            public async void Close()
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    viewerBox.Children.Clear();
                    viewerBox.Visibility = Visibility.Collapsed;
                    image = null;
                });
            }

            public void DrawFrame(VideoFrame frame, StreamType type, double opacity)
            {
                if (frame != null)
                {
                    if (image == null)
                    {
                        image = new Windows.UI.Xaml.Controls.Image();
                        viewerBox.Children.Add(image);
                    }
                    image.Opacity = opacity;

                    width = frame.SoftwareBitmap.PixelWidth;
                    height = frame.SoftwareBitmap.PixelHeight;
                    RenderSoftwareBitmap(frame.SoftwareBitmap, image, type, width, height);
                }
                viewerBox.Visibility = Visibility.Visible;
            }

            [ComImport]
            [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IMemoryBufferByteAccess
            {
                void GetBuffer(out IntPtr buffer, out Int32 capacity);
            }

            private static byte[] buffer = null;
            private static object cs = new object();

            internal async void RenderSoftwareBitmap(SoftwareBitmap softwareBitmap, Windows.UI.Xaml.Controls.Image image, StreamType type, float width, float height)
            {
                image.Width = width;
                image.Height = height;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var target = image.Source as WriteableBitmap;
                    if (target == null || target.PixelWidth != (int)width || target.PixelHeight != (int)height)
                    {
                        target = new WriteableBitmap((int)width, (int)height);
                        image.Source = target;
                    }
                    switch (softwareBitmap.BitmapPixelFormat)
                    {
                        default:
                            using (var converted = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8))
                            {
                                converted.CopyToBuffer(target.PixelBuffer);
                            }
                            break;
                        case BitmapPixelFormat.Bgra8:
                            softwareBitmap.CopyToBuffer(target.PixelBuffer);
                            break;
                        case BitmapPixelFormat.Gray16:
                            {
                                IntPtr data;
                                Int32 capacity;

                                int size;
                                using (var buffer2 = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
                                {
                                    var desc = buffer2.GetPlaneDescription(0);
                                    size = desc.Height * desc.Stride;
                                    if (buffer == null || buffer.Length != size * 2)
                                        buffer = new byte[size * 2];

                                    using (var reference = buffer2.CreateReference())
                                    {
                                        ((IMemoryBufferByteAccess)reference).GetBuffer(out data, out capacity);
                                        Marshal.Copy(data, buffer, 0, size);
                                    }
                                }

                                for (int i = buffer.Length - 4, j = i / 2; j >= 0; i -= 4, j -= 2)
                                {
                                    byte value;
                                    if (type == StreamType.STREAM_TYPE_IR)
                                    {
                                        value = (byte)((((int)buffer[j]) >> 1) + (((int)buffer[j + 1]) << 7));
                                    }
                                    else
                                    {
                                        value = (byte)((((int)buffer[j]) >> 3) + (((int)buffer[j + 1]) << 5));
                                   }
                                    buffer[i] = buffer[i + 1] = buffer[i + 2] = value;
                                    buffer[i + 3] = 255;
                                }

                                Stream stream = target.PixelBuffer.AsStream();
                                stream.Seek(0, SeekOrigin.Begin);
                                stream.Write(buffer, 0, buffer.Length);
                                target.Invalidate();
                            }
                            break;
                    }
                });
            }
        } // Viewer

        public void Resize(Size newSize)
        {

            var W = newSize.Width - 10;
            var H = newSize.Height - 10;

            if (statusLine != null)
            {
                statusLine.Resize(10, H - 30, W);
                H -= 40;
            }

            if (selector != null)
            {
                H -= selector.Margin.Top;
            }

            if (viewerList == null || viewerList.Length <= 0)
                return;

            Canvas imageBox = null;
            double left = viewerList[0].left;
            var dW = (W - 10 - left) / (MaxSlots > 1 ? 2 : 1);
            var dH = (H - 10) / (MaxSlots > 2 ? 2 : 1);
            for (int i = 0; i < MaxSlots; i++)
            {
                if (ctrlViewer(i) != null)
                {
                    imageBox = ctrlViewer(i) as Canvas;

                    double X0 = left;
                    if (i > 0) X0 += dW * (i % 2);
                    double Y0 = selector != null ? selector.Margin.Top : 0;
                    if (i > 1) Y0 += dH;

                    imageBox.MaxWidth = dW - 2;
                    imageBox.MaxHeight = dH - 2;

                    Thickness margin = imageBox.Margin;
                    margin.Left = X0;
                    margin.Top = Y0;
                    margin.Bottom = dH - Y0;
                    imageBox.Margin = margin;
                }
            }
        }

        public void BeginDraw(int slot)
        {
            if (viewerList != null)
            {
                if (slot < 0 || slot >= viewerList.Length) slot = 0;
                viewerList[slot].BeginPaint();
                activeSlot = slot;
            }
        }

        public void DrawFrame(VideoFrame frame, StreamType type, double opacity)
        {
            if (viewerList != null)
            {
                if (activeSlot < viewerList.Length)
                    viewerList[activeSlot].DrawFrame(frame, type, opacity);
            }
        }
        public void EndDraw(UIElement fps)
        {
            if (viewerList != null)
            {
                if (activeSlot < viewerList.Length)
                    viewerList[activeSlot].EndPaint(fps);
            }
        }
        public void CloseViewer()
        {
            if (viewerList != null)
            {
                for (int i = 0; i < viewerList.Length; i++)
                {
                    viewerList[i].Close();
                }
            }
        }

        public void FlushDraw()
        {
            if (viewerList != null)
            {
                viewerList[activeSlot].Flush();
            }
        }

        public void DrawPolyline(int color, IReadOnlyList<Point> points)
        {
            if (viewerList == null)
                return;

            if (points == null)
            return;

            Canvas imageBox = ctrlViewer(activeSlot) as Canvas;

            Polyline pl = new Polyline();
            pl.Stroke = brush[color % 4];
            pl.StrokeThickness = 3;
            for (int p = 0; p < points.Count; p++)
            {
                pl.Points.Add(points[p]);
            }
            pl.Points.Add(points[0]);
            imageBox.Children.Add(pl);
        }
    }

} // SampleFramework