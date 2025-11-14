using System.ComponentModel;
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

        public ICommand ProcessImageCommand => new Command(ProcessImage);

        private void ProcessImage()
        {
            byte[] bytes = Convert(ImageSource);

            Array.Reverse<byte>(bytes);

            ImageSource = Convert(bytes, ImageSource);


        }

        private BitmapSource Convert(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Important for releasing file handle after loading
            bitmapImage.EndInit();

            return bitmapImage;
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