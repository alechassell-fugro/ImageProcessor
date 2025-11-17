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

        public ICommand ChangeImageFilter => new Command(ProcessImage2);

        public ICommand ProcessImageCommand => new Command(ProcessImage);


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

            //Array.Reverse<byte>(bytes);

            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    bytes[i] = (byte)(bytes[i] >> 1);
            //}

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

            var output = new byte[bytes.Length];

            var blueBytes = bytes.Where((b, i) => i % 4 == 0).ToArray();
            var greenBytes = bytes.Where((b, i) => i % 4 == 1).ToArray();
            var redBytes = bytes.Where((b, i) => i % 4 == 2).ToArray();
            var alphaBytes = bytes.Where((b, i) => i % 4 == 3).ToArray();

            Trace.WriteLine($"len out: {output.Length} len blue: {blueBytes.Length}");
            for (var i = 0; i < blueBytes.Length; i++)
            {
                output[i] = blueBytes[i];
                output[i+1] = greenBytes[i];
                output[i+2] = redBytes[i];
                output[i + 3] = alphaBytes[i];
            }

            ImageSource = Convert(bytes, ImageSource);
        }

        private void ProcessImage2()
        {
            byte[] bytes = Convert(ImageSource);

            for (int i = 0; i < bytes.Length; i += 4)
            {
                //bytes[i - 2] = 0; // green to 0
                bytes[i] -= 10; 
                bytes[i + 1] -= 10; 
                bytes[i + 2] -= 10; 
            }

            ImageSource = ChangeImageBackground(bytes, ImageSource);
        }

        // TODO: Complete
        public async Task RunProcessInLoop()
        {
            var bytes = Convert(ImageSource);

            while (true)
            {
                Array.Reverse(bytes);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ImageSource = Convert(bytes, ImageSource);
                });

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private BitmapSource ChangeImageBackground(byte[] pixelBytes, BitmapSource src)
        {
            int stride = (src.PixelWidth * src.Format.BitsPerPixel + 7) / 8;
            BitmapSource updatedImg = BitmapSource.Create(src.PixelWidth, src.PixelHeight, src.DpiY, src.DpiX, src.Format, src.Palette, pixelBytes, stride);
            return updatedImg;
        }

        private byte[] Convert(BitmapSource bitmapSource)
        {
            int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

            byte[] bytes = new byte[stride * bitmapSource.PixelHeight];

            bitmapSource.CopyPixels(Int32Rect.Empty, bytes, stride, 0);

            return bytes;
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