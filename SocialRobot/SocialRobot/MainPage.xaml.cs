using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
/*using Intel.RealSense;*/
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Foundation;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using winsdkfb;
using winsdkfb.Graph;
using System.Diagnostics;
using Windows.Foundation.Collections;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SocialRobot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        CameraCaptureUI dialog = new CameraCaptureUI();
        //  StreamViewer rsStreamViewer;
        //   bool rendering = true;

        public MainPage()
        {
            this.InitializeComponent();
            /* rsStreamViewer = new StreamViewer(this);
             EnableSelector(rsStreamViewer);
             EnableViewer(4, 160);
             EnableStatusLine();
             EnableCounterFPS(3);

             rsStreamViewer.SampleArrived += RenderSample;*/
        }
        /* private void Start_Click(object sender, RoutedEventArgs e)
       {
           System.Threading.Tasks.Task.Run(() =>
           {
               rsStreamViewer.Start();
           });

       }
       private void Stop_Click(object sender, RoutedEventArgs e)
       {
           System.Threading.Tasks.Task.Run(() =>
           {
               rsStreamViewer.Stop();
           });
       }

       public bool IsView
       {
           get { return rendering; }
           set { rendering = value; }
       }

       private void RenderSample(Sample sample)
       {

           if (sample.Color != null)
           {
               CountFrame(0);
               if (IsView)
               {
                   BeginDraw(1);
                   DrawFrame(sample.Color, StreamType.STREAM_TYPE_COLOR, 1);
                   EndDraw(ctrlFPS(0));

               }
           }

           if (sample.Depth != null)
           {


               CountFrame(1);
               if (IsView)

               {
                   BeginDraw(2);
                   DrawFrame(sample.Depth, StreamType.STREAM_TYPE_DEPTH, 1);
                   EndDraw(ctrlFPS(1));

               }
           }

           if (sample.IR != null)
           {
               CountFrame(2);
               if (IsView)
               {
                   BeginDraw(3);
                   DrawFrame(sample.IR, StreamType.STREAM_TYPE_IR, 1);
                   EndDraw(ctrlFPS(2));
               }
           }

           if (!IsView)
           {
               CloseViewer();
               StatusText = "FPS meter: Color=" + FPS(0) + "  Depth= " + FPS(1) + "  IR= " + FPS(2);
           }
       }


       public async void Started()
       {
           await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
           {
               ctrlStart.IsEnabled = false;
               ctrlStop.IsEnabled = true;
               ctrlSwitch.IsEnabled = false;
               if (selector != null)
                   selector.IsEnabled = false;
           });
       }
       public async void Stopped()
       {
           await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
           {
               ctrlStart.IsEnabled = true;
               ctrlStop.IsEnabled = false;
               ctrlSwitch.IsEnabled = true;
               if (selector != null)
                   selector.IsEnabled = true;
           });
       }

       private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
       {
           Resize(e.NewSize);
       }

       private void AsyncSwitch_Toggled(object sender, RoutedEventArgs e)
       {
           ToggleSwitch sw = e.OriginalSource as ToggleSwitch;
           if (sw != null)
               rsStreamViewer.IsSync = sw.IsOn;
       }

       private void ctrlView_Toggled(object sender, RoutedEventArgs e)
       {
           ToggleSwitch sw = e.OriginalSource as ToggleSwitch;
           if (sw != null && rsStreamViewer != null)
               IsView = sw.IsOn;
       }*/
        private async void capture_Click(object sender, RoutedEventArgs e)
        {
            //   try
            //  {

            StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (file != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage.SetSource(fileStream);
                    await ReencodeAndSavePhotoAsync(fileStream, PhotoOrientation.Normal);
                }
                CapturePhoto.Source = bitmapImage;
            }


            //    }
            //  catch (Exception)
            //   {
            //       var dialog1 = new MessageDialog("Error");
            //       await dialog1.ShowAsync();
            //  }

        }
        private static async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, PhotoOrientation photoOrientation)
        {

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Picture", new List<string>() { ".jpeg" });
            savePicker.SuggestedFileName = "MyPhoto";


            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                var file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                        var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                        await encoder.BitmapProperties.SetPropertiesAsync(properties);
                        await encoder.FlushAsync();
                    }
                }
            }
        }


        private void reset_Click(object sender, RoutedEventArgs e)
        {
            CapturePhoto.Source = new BitmapImage(new Uri(this.BaseUri, "Asset/placeholder-sdk.png"));
        }



        public class FBPhoto
        {
            public string Id { get; set; }
            public string Post_Id { get; set; }
        }

        public class FBReturnObject
        {
            public string Id { get; set; }
            public string Post_Id { get; set; }
        }

        private async void photoupload_Click(object sender, RoutedEventArgs e)
        {

            var fop = new FileOpenPicker();
            fop.ViewMode = PickerViewMode.Thumbnail;
            fop.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fop.FileTypeFilter.Add(".jpg");
            fop.FileTypeFilter.Add(".jpeg");
            fop.FileTypeFilter.Add(".png");

            //StorageFolder storageFile = storageFile.GetFolderFromPathAsync("C:/Users/hp/Pictures/MyPhoto.jpg");
            //var stream = await storageFile.OpenReadAsync();

            StorageFile storageFile = await fop.PickSingleFileAsync();
            if (storageFile != null)
            {
                IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync();
                FBMediaStream mediaStream = new FBMediaStream(storageFile.Name, stream);

                FBSession sess = FBSession.ActiveSession;
                sess.FBAppId = "885648884904532";

                List<String> permissionList = new List<String>();
                permissionList.Add("public_profile");
                permissionList.Add("user_friends");
                permissionList.Add("user_likes");
                permissionList.Add("user_location");
                permissionList.Add("user_photos");
                permissionList.Add("publish_actions");
                FBPermissions permissions = new FBPermissions(permissionList);


                FBResult result = await sess.LoginAsync(permissions, SessionLoginBehavior.WebView);
                // await sess.LoginAsync();

                if (result.Succeeded)
                {
                    FBUser user = sess.User;
                    Debug.WriteLine(sess.User.Id);
                    Debug.WriteLine(sess.User.Name);

                    if (sess.LoggedIn)
                    {


                        // Get current user
                        // FBUser user = sess.User;

                        PropertySet parameters = new PropertySet();
                        // Set media stream
                        parameters.Add("source", mediaStream);
                        parameters.Add("caption", "Best app ever");

                        // Set Graph api path
                        string path = "/" + user.Id + "/photos";

                        var factory = new FBJsonClassFactory(s =>
                        {
                            return JsonConvert.DeserializeObject<FBReturnObject>(s);
                        });

                        var singleValue = new FBSingleValue(path, parameters, factory);
                        var result1 = await singleValue.PostAsync();
                        if (result1.Succeeded)
                        {
                            var response = result1.Object as FBReturnObject;
                            var photoResponse = result1.Object as FBPhoto;
                        }
                        else
                        {
                            Debug.WriteLine("Posting Failed.");// Posting failed
                        }
                    }
                }
                else
                {
                    //  var dialog1 = new MessageDialog("Error");
                    //  await dialog1.ShowAsync();
                }
            }


        }

        private async void LogOut_Click(object sender, RoutedEventArgs e)
        {
            FBSession sess = FBSession.ActiveSession;
            await sess.LogoutAsync();
        }
    }
}
