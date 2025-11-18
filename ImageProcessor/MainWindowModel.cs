using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace ImageProcessor
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            // Attempt loading from cache
            string cached = ReadFileFromCache(AppDataPath);
            if (!string.IsNullOrWhiteSpace(cached) && File.Exists(cached))
            {
                UriFileSrc = new Uri(cached);
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.UriSource = UriFileSrc;
                source.EndInit();
                ImageSource = source;
            }
        }
        public string Title { get; set; } = "Image Processor";
        private Uri? UriFileSrc;

        public bool AnimationActive { get; private set; } = false;
        private int _AnimationSpeed = 1;

        private string _SelectedEffectOption = "1. Flip Byte Array";
        private bool _PlayButtonEnabled = true;
        private bool _StopButtonEnabled = false;

        public string SelectedEffectOption
        {
            get => _SelectedEffectOption;
            set
            {
                _SelectedEffectOption = value;
                NotifyPropertyChanged();
            }
        }

        public bool PlayButtonEnabled
        {
            get => _PlayButtonEnabled;
            private set
            {
                _PlayButtonEnabled = value;
                NotifyPropertyChanged();
            }
        }
        public bool StopButtonEnabled
        {
            get => _StopButtonEnabled;
            private set
            {
                _StopButtonEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public int AnimationSpeed
        {
            get => _AnimationSpeed;
            set
            {
                _AnimationSpeed = value;
                NotifyPropertyChanged();
            }
        }

        private BitmapSource? _imageSource;

        public BitmapSource? ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ChooseImageCommand => new Command(ChooseImage);

        public ICommand ProcessImageCommand => new Command(ProcessImage);
        public ICommand SaveImageCommand => new Command(SaveProcessedImageToFile);

        public ICommand PlayAnimationCommand => new Command(PlayAnimation);

        public ICommand StopAnimationCommand => new Command(StopAnimation);

        public ICommand ResetAnimationCommand => new Command(ResetAnimation);

        private void SaveProcessedImageToFile()
        {
            // Save the image
            if (_imageSource != null)
            {   // CHECK WHETHER ALREADY SAVING
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|Webp Image|*.webp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(_imageSource));
                    using (var stream = System.IO.File.Create(saveFileDialog.FileName))
                    {
                        encoder.Save(stream);
                    }
                }
            }
        }
        private void ChooseImage()
        {
            OpenFileDialog openImg = new OpenFileDialog();
            openImg.RestoreDirectory = true;

            if (openImg.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openImg.FileName;
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                UriFileSrc = new Uri(selectedFileName);
                source.UriSource = UriFileSrc;
                source.EndInit();

                ImageSource = source;

                SaveFileToCache(AppDataPath, openImg.FileName);
            }
            // Cache file
        }

        private void ReloadImage()
        {
            if (UriFileSrc is null)
            {
                return;
            }

            BitmapImage source = new BitmapImage();
            source.BeginInit();
            source.UriSource = UriFileSrc;
            source.EndInit();

            ImageSource = source;
        }

        private void ProcessImage()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }

            byte[] bytes = Convert(ImageSource);
            switch (SelectedEffectOption) // should this be the private or public property?
            {
                case "1. Flip Byte Array":
                    ImageSource = Effect1(bytes, ImageSource);
                    break;
                case "2. Invert Colours":
                    ImageSource = Effect2(bytes, ImageSource);
                    break;
                case "3. Shift Colour Channels":
                    ImageSource = Effect3(bytes, ImageSource);
                    break;
                case "4. Imbalanced Byte Operation":
                    ImageSource = Effect4(bytes, ImageSource);
                    break;
                case "5. Colour Intensity Shift":
                    ImageSource = Effect5(bytes, ImageSource);
                    break;
                case "6. Slideshow Effect":
                    ImageSource = Effect6(bytes, ImageSource);
                    break;
                case "7. Black and White Effect":
                    ImageSource = Effect7(bytes, ImageSource);
                    break;
                case "8.":
                    ImageSource = Effect8(bytes, ImageSource);
                    break;



                case "9. Add Checkered Overlay":
                    ImageSource = Effect9(bytes, ImageSource);
                    break;
            }
        }

        //===============================   EFFECTS  ==================================
        public ObservableCollection<string> EffectOptions { get; } = new ObservableCollection<string>
        {
            "1. Flip Byte Array",
            "2. Invert Colours",
            "3. Shift Colour Channels",
            "4. Imbalanced Byte Operation",
            "5. Colour Intensity Shift",
            "6. Slideshow Effect",
            "7. Black and White Effect",
            "8.",

            "9. Add Checkered Overlay"
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
                bytes[i] -= 10;
                bytes[i + 1] -= 10;
                bytes[i + 2] -= 10;
            }
            return Convert(bytes, src);
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

                double greyscale = ((0.29 * red) + (0.59 * green) + (0.11 * blue));
                bytes[i] = (byte)greyscale; bytes[i + 1] = bytes[i]; bytes[i + 2] = bytes[i];
            }
            return Convert(bytes, src);
        }
        private BitmapSource Effect8(byte[] bytes, BitmapSource src)
        {
            Trace.WriteLine(bytes.Length);
            int row = 0;
            for (int i = 0; i < bytes.Length; i += 4)
            {
                if (i % src.PixelWidth == 0) row++;
                if(row % 15 == 0)
                {
                    bytes[i] = (byte)row;
                    bytes[i + 1] = (byte)row;
                    bytes[i + 2] = (byte)row;
                }
            }
            return Convert(bytes, src);
        }

        private BitmapSource Effect9(byte[] bytes, BitmapSource src)
        {
            int stride = (ImageSource.PixelWidth * ImageSource.Format.BitsPerPixel + 7) / 8;
            byte[] arr1 = new byte[stride]; // one row of pixels

            int countHor = 0;
            int currRow = 1;
            int gridSize = 64; // kept divisible by 4 but maybe not needed?

            while (currRow < ImageSource.PixelHeight)
            {
                int countVer = 0;
                for (int i = 0; i < stride; i++)
                {
                    // copy each byte in this row
                    arr1[i] = bytes[i + (stride * (currRow))];
                    if (countVer == (gridSize * 4))
                    {
                        countVer = 0;
                        arr1[i] = 255; // max blue 
                    }
                    countVer++;
                }

                if (countHor == gridSize)
                {
                    countHor = 0; // reset count
                    for (int i = 0; i < stride; i += 4) // check each pixel across one row length
                    {
                        arr1[i] = 255; // max blue 
                    }
                }

                for (int i = 0; i < stride; i++)
                {
                    // add back in to bytes array
                    bytes[i + (stride * (currRow))] = arr1[i];
                }
                countHor++;
                currRow++;
            }
            return Convert(bytes, src);
        }


        /*
         * var bytes = Convert(ImageSource);
            for(int i = 0; i < bytes.Length; i += 4)
            {
                int row = i / ImageSource.PixelWidth;
                int positionWithinRow = i % ImageSource.PixelWidth;
                // Only swap the second half of the row
                //if(positionWithinRow > ImageSource.PixelWidth)
                //{
                //    //bytes[i] = 0;
                //    //bytes[i + 1] = 0;
                    
                //    continue;
                //}

                int destinationWithinRow = ImageSource.PixelWidth - positionWithinRow;
                int totalDestination = row * ImageSource.PixelWidth + destinationWithinRow;
                for(int j = 0; j < 3; j++)
                {

                }
                byte temp = bytes[i];
                bytes[i] = bytes[totalDestination];
                bytes[totalDestination] = temp;
                   
            }

            ImageSource = Convert(bytes, ImageSource; 
         */
        //========================================  IMAGE OPERATIONS==========================================
        public ICommand MirrorHorizontallyCommand => new Command(MirrorHorizontally);
        public ICommand MirrorVerticallyCommand => new Command(MirrorVertically);
        public ICommand RotateCommand => new Command(Rotate);
        public ICommand DownsampleCommand => new Command(Downsample);

        private void MirrorHorizontally()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }
            var bytes = Convert(ImageSource);
            for(int i = 0; i < bytes.Length -1 ; i += 4)
            {
                int row = i / ImageSource.PixelWidth * 4;
                int positionWithinRow = i % (ImageSource.PixelWidth * 4);
                //Only swap the second half of the row
                if (positionWithinRow > ImageSource.PixelWidth * 4)
                {
                    //bytes[i] = 0;
                    //bytes[i + 1] = 0;

                    continue;
                }

                int destinationWithinRow = ImageSource.PixelWidth * 4 - positionWithinRow;
                int totalDestination = row * ImageSource.PixelWidth*4 + destinationWithinRow;
                for(int j = 0; j < 2; j++)
                {
                    byte temp = bytes[i+j];
                    bytes[i+j] = bytes[totalDestination];
                    bytes[totalDestination] = temp;

                }
                   
            }

            ImageSource = Convert(bytes, ImageSource);
        }

        private void MirrorVertically()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }
            //BitmapSource original = ImageSource;
            //int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
            //var bitmapSource = BitmapSource.Create(original.PixelWidth, original.PixelHeight, original.DpiX, original.DpiY, original.Format, original.Palette, pixelBytes, stride);
            //ImageSource = bitmapSource;
        }


        private void Rotate()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }
            //BitmapSource original = ImageSource;
            //int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
            //var bitmapSource = BitmapSource.Create(original.PixelWidth, original.PixelHeight, original.DpiX, original.DpiY, original.Format, original.Palette, pixelBytes, stride);
            //ImageSource = bitmapSource;
        }

        private void Downsample()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }
            //BitmapSource original = ImageSource;
            //int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
            //var bitmapSource = BitmapSource.Create(original.PixelWidth, original.PixelHeight, original.DpiX, original.DpiY, original.Format, original.Palette, pixelBytes, stride);
            //ImageSource = bitmapSource;
        }
        
        //========================================  ANIMATION  ==========================================
        public void PlayAnimation()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }

            int width = ImageSource.PixelWidth; // Null if no image selected
            SetAnimationActive(true);
            ChangeButtonState(false, true);

            Task _task = RunProcessInLoop(); 
        }

        private async Task RunProcessInLoop()
        {
            while (AnimationActive)
            {
                // Update window
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ProcessImage();
                });

                // Speed of animation 
                await Task.Delay(TimeSpan.FromMilliseconds((11-AnimationSpeed)*100)); // 1 = 1000ms, 2 = 900ms... 10 = 100ms
            }
        }

        public void StopAnimation()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }

            SetAnimationActive(false);
            ChangeButtonState(true, false);
        }

        public void ResetAnimation()
        {
            if (ImageSource is null)
            {
                return; // handle no image selected
            }

            SetAnimationActive(false);
            ChangeButtonState(true, false);
            ReloadImage();
        }
        private void SetAnimationActive(bool value)
        {
            AnimationActive = value;
        }

        private void ChangeButtonState(bool playButton, bool stopButton)
        {
            PlayButtonEnabled = playButton;
            StopButtonEnabled = stopButton;
        }

        //========================================  CONVERSION  ==========================================


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

        //========================================  CACHING  ==========================================
        private string AppDataPath => Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.Title)).ToString();

        private void SaveFileToCache(string cacheFileDirectory, string prevFilePathText)
        {
            string CacheFilePath = Path.Combine(cacheFileDirectory, "last.txt");
            File.WriteAllText(CacheFilePath, prevFilePathText);
            Trace.WriteLine($"Writing {prevFilePathText} to cache\n");

        }

        private string ReadFileFromCache(string cacheFileDirectory)
        {
            string cacheFilePath = Path.Combine(cacheFileDirectory, "last.txt");
           
            if (!File.Exists(cacheFilePath))
            {
                return string.Empty;
            }
            using var sr = new StreamReader(cacheFilePath);
            string? line = sr.ReadLine();

            Trace.WriteLine($"Reading {line} from cache\n");
            return string.IsNullOrWhiteSpace(line) ? string.Empty : line.Trim();
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
