using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            var bmp = new BitmapImage(new Uri("./Images/ket.webp", UriKind.Relative));
            //Trace.WriteLine("text");
            //Trace.WriteLine(bmp);
            //var bmpSource = new BitmapSource();
            //ImageSource = bmpSource;

            PixelFormat pf = PixelFormats.Bgr32;
            int width = 200;
            int height = 200;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            // Initialize the image with data.
            Random value = new Random();
            value.NextBytes(rawImage);

            // Create a BitmapSource.
            BitmapSource bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null,
                rawImage, rawStride);

            ImageSource = bitmap;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}