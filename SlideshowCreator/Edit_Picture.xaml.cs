using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Interop;
//using System.Windows.Forms;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for the Edit picture window.
    /// </summary>
    public partial class Edit_Picture : Window
    {
        public PictureExplorerControl PictureControl;

        private string _source;
        private Bitmap _bitmap;
        private bool _left = false;
        private float _cont = 0;
        private System.Drawing.Image _dImg;
        private bool _changed = false;
        private string[] _editedImg = new string[1];

        /// <summary>
        /// Initializes everything as window opens
        /// </summary>
        public Edit_Picture()
        {
            InitializeComponent();
            RoutedCommand Newcmd1 = new RoutedCommand();
            Newcmd1.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd1, Save_Click));
            RoutedCommand Newcmd2 = new RoutedCommand();
            Newcmd2.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd2, Exit_Click));
        }

        /// <summary>
        /// Sets the source of the image to edit
        /// </summary>
        /// <param name="source">Source of the image</param>
        /// <param name="Picturecontrol">instance of PictureExplorerControl</param>
        public void set_source(string source, PictureExplorerControl Picturecontrol)
        {
            _source = source;
            img.Source = new BitmapImage(new Uri(source, UriKind.Absolute));
            _bitmap = ImageConverter.ScaleImage(new Bitmap(_source),242,430,false);
            ReloadPicture();
            this.PictureControl = Picturecontrol;
        }

        /// <summary>
        /// Converts a Bitmap to ImageSource
        /// </summary>
        /// <param name="bitmap">Input Bitmap</param>
        /// <returns></returns>
        private ImageSource BitmaptoImagesource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            ImageSource s = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return s;

        }

        /// <summary>
        /// Resets the image
        /// </summary>
        private void ReloadPicture()
        {
            _bitmap = new Bitmap(_source);
            img.Source = BitmaptoImagesource(_bitmap);
            _changed = false;
        }

        /// <summary>
        /// Apply contrast by the slider value
        /// </summary>
        private void contrast()
        {
            _cont = 0.2f * (float)Contrast.Value;
            _dImg = _bitmap;
            Bitmap bmpinverted = new Bitmap(_dImg.Width, _dImg.Height);
            ImageAttributes ia = new ImageAttributes();
            ColorMatrix cmpicture = new ColorMatrix(new float[][] {
                     new float[]{_cont ,0f,0f,0f,0f },
                     new float[]{0f,_cont,0f,0f,0f },
                     new float[]{0f,0f,_cont,0f,0f },
                     new float[]{0f,0f,0f,1f,0f },
                     new float[]{0.001f,0.001f,0.001f,0f,1f} });
            ia.SetColorMatrix(cmpicture);
            Graphics grps = Graphics.FromImage(bmpinverted);
            grps.DrawImage(_dImg, new System.Drawing.Rectangle(0, 0, _dImg.Width,_dImg.Height), 0, 0,
            _dImg.Width, _dImg.Height, GraphicsUnit.Pixel, ia);
            grps.Dispose();
            img.Source = BitmaptoImagesource(bmpinverted);
        }

        /// <summary>
        /// Apply brightness by the slider value
        /// </summary>
        private void ApplyBrightness()
        {
            _dImg = _bitmap;
            float fvalue = (float)Bright.Value / 50f;
            Bitmap bmpinverted = new Bitmap(_dImg.Width, _dImg.Height);
            ImageAttributes ia = new ImageAttributes();
            ColorMatrix cmpicture = new ColorMatrix(new float[][] {
                new float[] {1,0,0,0,0},
                new float[] {0,1,0,0,0},
                new float[] {0,0,1,0,0 },
                new float[] {0,0,0,0,0},
                new float[]{fvalue,fvalue,fvalue,1,1} });
            ia.SetColorMatrix(cmpicture);
            Graphics grps = Graphics.FromImage(bmpinverted);
            grps.DrawImage(_dImg, new System.Drawing.Rectangle(0, 0, _dImg.Width, _dImg.Height), 0, 0,
            _dImg.Width, _dImg.Height, GraphicsUnit.Pixel, ia);
            grps.Dispose();
            img.Source = BitmaptoImagesource(bmpinverted);
        }

        /// <summary>
        /// Apply Effect 1 (Green) by the slider value
        /// </summary>
        private void f1()
        {
            _dImg = _bitmap;
            Bitmap bmpinverted = new Bitmap(_dImg.Width, _dImg.Height);
            ImageAttributes ia = new ImageAttributes();
            ColorMatrix cmpicture = new ColorMatrix(new float[][] {
                new float[] {0,0,1,0,0},
                new float[] {0,1,0,0,0},
                new float[] {0,0,0,0,0},
                new float[] {0,0,0,1,0},
                new float[]{0,0,0,0,1} });
            ia.SetColorMatrix(cmpicture);
            Graphics grps = Graphics.FromImage(bmpinverted);
            grps.DrawImage(_dImg, new System.Drawing.Rectangle(0, 0, _dImg.Width, _dImg.Height), 0, 0, _dImg.Width,
            _dImg.Height, GraphicsUnit.Pixel, ia);
            grps.Dispose();
            img.Source = BitmaptoImagesource(bmpinverted);
            _bitmap = bmpinverted;
        }

        /// <summary>
        /// Apply Effect 2 (Blue) by the slider value
        /// </summary>
        private void f2()
        {
            _dImg = _bitmap;
            Bitmap bmpinverted = new Bitmap(_dImg.Width, _dImg.Height);
            ImageAttributes ia = new ImageAttributes();
            ColorMatrix cmpicture = new ColorMatrix(new float[][] {
                new float[] {1,1,1,0,0},
                new float[] {0,0,1,1,0},
                new float[] {0,0,0,0,0},
                new float[] {0,0,0,1,0},

                new float[]{0,0,0,0,1} });
            ia.SetColorMatrix(cmpicture);
            Graphics grps = Graphics.FromImage(bmpinverted);
            grps.DrawImage(_dImg, new System.Drawing.Rectangle(0, 0, _dImg.Width, _dImg.Height), 0, 0, _dImg.Width,
            _dImg.Height, GraphicsUnit.Pixel, ia);
            grps.Dispose();
            img.Source = BitmaptoImagesource(bmpinverted);
            _bitmap = bmpinverted;
        }

        /// <summary>
        /// Apply Effect 3 (Black and White) by the slider value
        /// </summary>
        private void f3()
        {
            _dImg = _bitmap;
            Bitmap bmpinverted = new Bitmap(_dImg.Width, _dImg.Height);
            ImageAttributes ia = new ImageAttributes();
            ColorMatrix cmpicture = new ColorMatrix(new float[][] {
                new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new float[] { 0,      0,      0,      1, 0 },
                new float[] { 0,      0,      0,      0, 1 }
            });
            ia.SetColorMatrix(cmpicture);
            Graphics grps = Graphics.FromImage(bmpinverted);
            grps.DrawImage(_dImg, new System.Drawing.Rectangle(0, 0, _dImg.Width, _dImg.Height), 0, 0, _dImg.Width,
            _dImg.Height, GraphicsUnit.Pixel, ia);
            grps.Dispose();
            img.Source = BitmaptoImagesource(bmpinverted);
            _bitmap = bmpinverted;
        }

        /// <summary>
        /// Rotate the picture
        /// </summary>
        private void Drehen_Click(object sender, RoutedEventArgs e)
        {
            _bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            img.Source = BitmaptoImagesource(_bitmap);
            _changed = true;

        }

        /// <summary>
        /// Saves the picture and adds it to the picture explorer
        /// </summary>
        private void save() {
            string _pathname = DateTime.Now.ToString("yyyyMMdd_hhmmss") ;
            _bitmap.Save("./" +_pathname+ ".png", ImageFormat.Png);
            _changed = false;
            _editedImg[0] = System.IO.Path.GetFullPath("./" + _pathname + ".png");
            PictureControl.AddImages(_editedImg);
        }

        /// <summary>
        /// Flips the image
        /// </summary>
        private void left_Click(object sender, RoutedEventArgs e)
        {
            if (!_left)
            {
                _bitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
                img.Source = BitmaptoImagesource(_bitmap);
                _left = true;
                _changed = true;

            }
            else
            {
                return;
            }
            
        }

        /// <summary>
        /// Flips the image
        /// </summary>
        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (_left)
            {
                _bitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
                img.Source = BitmaptoImagesource(_bitmap);
                _left = false;
                _changed = true;

            }
            else
            {
                return;
            }
        }

        private void Contrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_bitmap == null)
                return;
            contrast();
            _changed = true;

        }

        private void reload_Click(object sender, RoutedEventArgs e)
        {
            ReloadPicture();
        }

        private void effect1_Click(object sender, RoutedEventArgs e)
        {
            f1();
            _changed = true;

        }

        private void effect2_Click(object sender, RoutedEventArgs e)
        {
            f2();
            _changed = true;

        }

        private void effect3_Click(object sender, RoutedEventArgs e)
        {
            f3();
            _changed = true;

        }

        private void bright_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_bitmap == null)
                return;
            ApplyBrightness();
            _changed = true;
        }

        /// <summary>
        /// Exits the Edit picture window with warning
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (!_changed)
                this.Close();
            else
            {
                if (MessageBox.Show("Do you want to Close without Save?", "Confirm",
    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.Close();                   
                }
                else
                {
                    save();
                    this.Close();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            save();
            this.Close();
        }
    }
}
