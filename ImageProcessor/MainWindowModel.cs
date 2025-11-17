using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace ImageProcessor
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = "Image Processor";

        public bool AnimationActive { get; private set; } = false;
        private string _SelectedEffectOption = "1. Invert Colors";

        public string SelectedEffectOption
        {
            get => _SelectedEffectOption;
            set
            {
                _SelectedEffectOption = value;
                NotifyPropertyChanged();
            }
        }

        private BitmapSource? _imageSource;

        public BitmapSource ImageSource
        {
            get => _imageSource!;
            private set
            {
                _imageSource = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ChooseImageCommand => new Command(ChooseImage);

        //public ICommand ChangeImageFilter => new Command(ProcessImage2);

        public ICommand ProcessImageCommand => new Command(ProcessImage);

        public ICommand PlayAnimationCommand => new Command(PlayAnimation);

        public ICommand StopAnimationCommand => new Command(StopAnimation);


        private void ChooseImage()
        {
            OpenFileDialog openImg = new OpenFileDialog();
            openImg.RestoreDirectory = true;

            if (openImg.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openImg.FileName;
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.UriSource = new Uri(selectedFileName);
                source.EndInit();
                
                ImageSource = source; 
            }
        }

        private void ProcessImage()
        {
            byte[] bytes = Convert(ImageSource);
            switch (SelectedEffectOption) // should this be the private or public property?
            {
                case "1. Invert Colors":
                    ImageSource = Effect1(bytes, ImageSource);
                    break;
                case "2. Grayscale":
                    ImageSource = Effect2(bytes, ImageSource);
                    break;
                case "3. Sepia Tone":
                    ImageSource = Effect3(bytes, ImageSource);
                    break;
                case "4. Brightness Adjustment":
                    ImageSource = Effect4(bytes, ImageSource);
                    break;
                case "5. Custom Effect":
                    ImageSource = Effect5(bytes, ImageSource);
                    break;
                case "6. Slideshow Effect":
                    ImageSource = Effect6(bytes, ImageSource);
                    break;
                case "7. Black and White Effect":
                    ImageSource = Effect7(bytes, ImageSource);
                    break;
            }

            //ImageSource = Convert(bytes, ImageSource);
        }

//===============================   EFFECTS  ==================================
        public ObservableCollection<string> EffectOptions { get; } = new ObservableCollection<string>
        {
            "1. Invert Colors",
            "2. Grayscale",
            "3. Sepia Tone",
            "4. Brightness Adjustment",
            "5. Custom Effect",
            "6. Slideshow Effect",
            "7. Black and White Effect"
        };

        private BitmapSource Effect1(byte[] bytes, BitmapSource src)
        {
            Array.Reverse<byte>(bytes);
            return Convert(bytes, src);
        }

        private BitmapSource Effect2(byte[] bytes, BitmapSource src)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(255 - bytes[i]);
            }
            return Convert(bytes, src);
        }

        private BitmapSource Effect3(byte[] bytes, BitmapSource src)
        {
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                byte temp = bytes[i];
                bytes[i] = bytes[i + 1];
                bytes[i + 1] = temp;
            }

            return Convert(bytes, src);
        }

        private BitmapSource Effect4(byte[] bytes, BitmapSource src)
        {
            for (int i = 0; i < bytes.Length - 1; i += 3)
            {
                bytes[i] -= 10;
                bytes[i + 2] += 100;
            }
            return Convert(bytes, src);
        }

        private BitmapSource Effect5(byte[] bytes, BitmapSource src)
        {
            for (int i = 0; i < bytes.Length; i += 4)
            {
                //bytes[i - 2] = 0; // green to 0
                bytes[i] -= 10;
                bytes[i + 1] -= 10;
                bytes[i + 2] -= 10;
            }
            // could we just return Convert(bytes, src); here?

            int stride = (ImageSource.PixelWidth * ImageSource.Format.BitsPerPixel + 7) / 8;
            ImageSource = BitmapSource.Create(
                ImageSource.PixelWidth, ImageSource.PixelHeight,
                ImageSource.DpiY, ImageSource.DpiX,
                ImageSource.Format,
                ImageSource.Palette,
                bytes,
                stride
                );

            return ImageSource;
        }

        private BitmapSource Effect6(byte[] bytes, BitmapSource src)
        {
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                byte temp = bytes[i];
                bytes[i] = bytes[(i + 180) % bytes.Length];
                bytes[(i + 180) % bytes.Length] = temp;
            }
            
            return Convert(bytes, src);
        }

        private BitmapSource Effect7(byte[] bytes, BitmapSource src)
        {
            for (int i = 0; i < bytes.Length; i += 4)
            {
                byte blue = bytes[i];  
                byte green = bytes[i + 1]; 
                byte red = bytes[i + 2];  

                double greyscale = ((0.29 * red) + (0.59 * green) + (0.11 * blue)) / 3; 
                bytes[i] = (byte)greyscale; bytes[i + 1] = bytes[i]; bytes[i + 2] = bytes[i];
            }
            return Convert(bytes, src);
        }
        //=============================================================================



        // TODO: Complete
        //public async Task RunProcessInLoop()
        //{
        //    var bytes = Convert(ImageSource);

        //    while (true)
        //    {
        //        Array.Reverse(bytes);
        //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            ImageSource = Convert(bytes, ImageSource);
        //        });

        //        await Task.Delay(TimeSpan.FromSeconds(1));
        //    }
        //}
        //----------------------------------------------------

        private byte[] Convert(BitmapSource bitmapSource)
        {
            int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

            byte[] bytes = new byte[stride * bitmapSource.PixelHeight];

            bitmapSource.CopyPixels(Int32Rect.Empty, bytes, stride, 0);

            return bytes;
        }

        public void PlayAnimation()
        {
            try
            {
                int width = ImageSource.PixelWidth; // Null if no image selected

                SetAnimationActive(true);

                // TODO: Update for currently or last selected/cached effect
                Task _task = RunProcessInLoop();  
            }
            catch (NullReferenceException e) { } // TODO: Add error msg
        }

        private async Task RunProcessInLoop()
        {
            // Convert src img to byte array
            var bytes = Convert(ImageSource);

            while (AnimationActive)
            {
                // Alter byte array
                //Array.Reverse(bytes); // TODO: update for different animations  

                // Update window
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ProcessImage();
                    //ImageSource = Convert(bytes, ImageSource);
                });

                // Speed of animation 
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public void StopAnimation()
        {
            SetAnimationActive(false);
        }

        private void SetAnimationActive(bool value)
        {
            AnimationActive = value; 
        }

        private BitmapSource Convert(byte[] pixelBytes, BitmapSource original)
        {
            int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
            var bitmapSource = BitmapSource.Create(original.PixelWidth, original.PixelHeight, original.DpiX, original.DpiY, original.Format, original.Palette, pixelBytes, stride);
            return bitmapSource;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

//for (int i = 0; i < bytes.Length - 1; i++)
//{
//    byte temp = bytes[i];
//    bytes[i] = bytes[i + 1];
//    bytes[i + 1] = temp;

//}

// Cat slideshow
//for (int i = 0; i < bytes.Length - 1; i++)
//{
//    byte temp = bytes[i];
//    bytes[i] = bytes[(i + 180) % bytes.Length];
//    bytes[(i + 180) % bytes.Length] = temp;

//}

//for (int i = 0; i < bytes.Length - 1; i+=3)
//{
//    bytes[i] -= 10;
//    bytes[i + 2] += 100;
//}


//for (int i = 0; i < bytes.Length - 1; i += 4)
//{
//    bytes[i] -= 10;
//    bytes[i + 2] += 100;
//}
//var rand = new Random();
//for (int i = 0; i < bytes.Length - 1; i += 4)
//{
//var Rrand = rand.Next(0, 255);
//var Grand = rand.Next(0, 255);
//var Brand = rand.Next(0, 255);
//    bytes[i] = (byte)(bytes[i]+ Rrand);
//    bytes[i+1] = (byte)(bytes[i+1] +Grand);
//    //bytes[i+2] = (byte)(bytes[i+2] + Brand);
//    //bytes[i+3] -= RArand;
//}

//var output = new byte[bytes.Length];

//var blueBytes = bytes.Where((b, i) => i % 4 == 0).ToArray();
//var greenBytes = bytes.Where((b, i) => i % 4 == 1).ToArray();
//var redBytes = bytes.Where((b, i) => i % 4 == 2).ToArray();
//var alphaBytes = bytes.Where((b, i) => i % 4 == 3).ToArray();



//for (int i = 0; i < blueBytes.Length; i += 2)
//{
//    blueBytes[i] = 0;
//}

//Trace.WriteLine($"len out: {output.Length} len blue: {blueBytes.Length}");
//for (var i = 0; i < blueBytes.Length; i++)
//{
//    output[i] = blueBytes[i];
//    output[i + 1] = greenBytes[i];
//    output[i + 2] = redBytes[i];
//    output[i + 3] = alphaBytes[i];
//}
