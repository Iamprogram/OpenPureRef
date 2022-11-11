using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Forms;

using Aspose.Words;
using Aspose.Words.Loading;
using System.IO;
using SelectPdf;

using System.Configuration;

using GrapeCity.Documents.Pdf;
//using System.IO;
using GrapeCity.Documents.Imaging;
using GrapeCity.Documents.Drawing;
using System.Drawing;
using GrapeCity.Documents.Text;
using GrapeCity.Documents.Common;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Color = System.Drawing.Color;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using System.Configuration;
using System.Windows.Markup;


using Spire.PdfViewer;
using Spire.PdfViewer.Forms;
using Spire.Pdf;
using System.Drawing.Imaging;

//using System.Xaml;

namespace OpenWpfPanAndZoom
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool anyChangesinCanvas = false;
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            // Track when objects are added and removed
            if (visualAdded != null)
            {
                // Do stuff with the added object
                anyChangesinCanvas = true;
            }
            if (visualRemoved != null)
            {
                // Do stuff with the removed object
                anyChangesinCanvas = true;
            }

            // Call base function
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs  e)
        {
            if (anyChangesinCanvas)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Please Be Sure That changes Are Saved.  Do You Want To Close This Program?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        static int pdfcount = 0;
        double rightX, rightY;
        private void pageCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightX = e.GetPosition(canvas).X;
            rightY = e.GetPosition(canvas).Y;
        }

        private void MaximizeWindow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }
        
        private void MinimizeWindow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        private void RestoreWindow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        private void NewWorkspace_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // if clear, will remove gridlines......
            //canvas.Children.Clear();
            // so only remove wigdet and images
            List<UIElement> itemstoremove = new List<UIElement>();
            foreach (UIElement child in canvas.Children)
            {
                string typename = child.GetType().FullName;
                if (typename.Contains("Image"))
                {
                    //canvas.Children.Remove(child);
                    itemstoremove.Add(child);
                }
                if (typename.Contains("Widget"))
                {
                    //canvas.Children.Remove(child);
                    itemstoremove.Add(child);
                }
            }
            foreach (UIElement ui in itemstoremove)
            {
                canvas.Children.Remove(ui);
            }

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "OpenPureRef Workspace files (*.pws)|*.pws|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            string selectedFileName = "";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Code to write the stream goes here.
                    selectedFileName = saveFileDialog1.FileName;
                    currentCanvasXAMLFullpath = selectedFileName;

                    myStream.Close();
                }
            }

        }

        private void LoadWorkspace_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.xaml)|*.xaml|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            string selectedFileName = "";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedFileName = dlg.FileName;
                currentCanvasXAMLFullpath = selectedFileName;
            }
            FileStream fs = File.Open(selectedFileName, FileMode.Open, FileAccess.Read);
            CustomControls.PanAndZoomCanvas savedCanvas = XamlReader.Load(fs) as CustomControls.PanAndZoomCanvas;
            fs.Close();

            canvas.Children.Clear();
            /*
            foreach (UIElement child in savedCanvas.Children)
            {
                string typename = child.GetType().FullName;
                if (typename.Contains("Image"))
                {
                    canvas.Children.Add(child);
                }
            }
            */
            foreach (UIElement child in savedCanvas.Children)
            {
                var xaml = System.Windows.Markup.XamlWriter.Save(child);
                var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as UIElement;
                canvas.Children.Add(deepCopy);
            }

            selectedRect = new System.Windows.Shapes.Rectangle();
            selectedRect.Stroke = new SolidColorBrush(Colors.Red);
            //rect.Fill = new SolidColorBrush(Colors.Black);
            selectedRect.StrokeThickness = 0;
            selectedRect.Width = 200;
            selectedRect.Height = 200;
            Canvas.SetLeft(selectedRect, 200);
            Canvas.SetTop(selectedRect, 200);
            canvas.Children.Add(selectedRect);
            //CanvasBorder.BorderThickness = new Thickness(5);

            // remove current canvas 
            //canvasgrid.Children.Remove(canvas);
            // load existing canvas 
            //canvasgrid.Children.Add(savedCanvas);
        }

        string currentCanvasXAMLFullpath = "";

        private void SaveWorkspace_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            /*
            string canvasXAMLFullpath = "";

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "OpenPureRef Workspace files (*.pws)|*.pws";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Code to write the stream goes here.

                    canvasXAMLFullpath = Path.GetFullPath(saveFileDialog1.FileName); 
                    myStream.Close();
                }
            }
            */

            //string temporary = canvasXAMLFullpath + ".tmp.xaml";
            string convertedFileName = currentCanvasXAMLFullpath.Replace(".pws", "").Replace(".PWS", "").Replace(".xaml", "").Replace(".XAML", "") + ".xaml";

            FileStream fs = File.Open(convertedFileName, FileMode.Create);
            XamlWriter.Save(canvas, fs);
            fs.Close();
        }

        private void SaveWorkspaceAs_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string canvasXAMLFullpath = "";

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "OpenPureRef Workspace files (*.pws)|*.pws";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Code to write the stream goes here.

                    canvasXAMLFullpath = Path.GetFullPath(saveFileDialog1.FileName);
                    myStream.Close();
                }
            }

            //string temporary = canvasXAMLFullpath + ".tmp.xaml";
            string convertedFileName = canvasXAMLFullpath.Replace(".pws", "").Replace(".PWS", "") + ".xaml";

            FileStream fs = File.Open(convertedFileName, FileMode.Create);
            XamlWriter.Save(canvas, fs);
            fs.Close();
        }

        private void ApplicationExit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Application.Current.Shutdown();
            Close();
        }

        private UIElement _selectedElement;

        private void Canvas_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // clear selected first
                if(_selectedElement !=null) {
                    string typename = _selectedElement.GetType().FullName;
                    if (typename.Contains("Image"))
                    {
                        Image tempImage = _selectedElement as Image;
                        //tempImage.BorderBrush = Brushes.Red;
                        //tempImage.BorderThickness = new Thickness(5);
                        selectedRect.StrokeThickness = 0;
                    }
                    if (typename.Contains(".Widget"))
                    {
                        CustomControls.Widget tempImage = _selectedElement as CustomControls.Widget;
                        tempImage.BorderBrush = Brushes.Red;
                        tempImage.BorderThickness = new Thickness(0);
                    }
                }
                _selectedElement = null;
                if (canvas.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    string typename = _selectedElement.GetType().FullName;
                    if (typename.Contains("Image")){
                        Image tempImage = _selectedElement as Image;
                        double debugTop = Canvas.GetTop(_selectedElement);
                        double debugLeft = Canvas.GetLeft(_selectedElement);
                        Canvas.SetLeft(selectedRect, Canvas.GetLeft(_selectedElement)-5);
                        Canvas.SetTop(selectedRect, Canvas.GetTop(_selectedElement)-5);
                        selectedRect.Width = tempImage.Width + 10;
                        selectedRect.Height = tempImage.Height + 10;
                        selectedRect.StrokeThickness = currentSelectedBorderThickness;
                    }
                    if (typename.Contains(".Widget")){
                        CustomControls.Widget tempImage = _selectedElement as CustomControls.Widget;
                        tempImage.BorderBrush = Brushes.Red;
                        tempImage.BorderThickness = new Thickness(currentSelectedBorderThickness);
                    }
                }
            }


            var tempcanvas = sender as Canvas;
            if (tempcanvas == null){
                return;
            }
               

            HitTestResult hitTestResult = VisualTreeHelper.HitTest(canvas, e.GetPosition(canvas));
            var element = hitTestResult.VisualHit;
            
            // do something with element
            //element.BorderBrush = Brushes.Red;
            //element.BorderThickness = new Thickness(5);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // clear selected first
                if (_selectedElement != null)
                {
                    string typename = _selectedElement.GetType().FullName;
                    if (typename.Contains("Image"))
                    {
                        Image tempImage = _selectedElement as Image;
                        //tempImage.BorderBrush = Brushes.Red;
                        //tempImage.BorderThickness = new Thickness(5);
                        selectedRect.StrokeThickness = 0;
                    }
                    if (typename.Contains(".Widget"))
                    {
                        CustomControls.Widget tempImage = _selectedElement as CustomControls.Widget;
                        tempImage.BorderBrush = Brushes.Red;
                        tempImage.BorderThickness = new Thickness(0);
                    }
                }
                _selectedElement = null;
                if (canvas.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    string typename = _selectedElement.GetType().FullName;
                    if (typename.Contains("Image"))
                    {
                        Image tempImage = _selectedElement as Image;
                        Canvas.SetLeft(selectedRect, Canvas.GetLeft(_selectedElement) - 5);
                        Canvas.SetTop(selectedRect, Canvas.GetTop(_selectedElement) - 5);
                        selectedRect.Width = tempImage.Width + 10;
                        selectedRect.Height = tempImage.Height + 10;
                        selectedRect.StrokeThickness = currentSelectedBorderThickness;
                    }
                    if (typename.Contains(".Widget"))
                    {
                        CustomControls.Widget tempImage = _selectedElement as CustomControls.Widget;
                        tempImage.BorderBrush = Brushes.Red;
                        tempImage.BorderThickness = new Thickness(currentSelectedBorderThickness);
                    }

                    canvas.Focus();
                }
            }
        }

        private void canvas_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (_selectedElement != null){
                    canvas.Children.Remove(_selectedElement);
                    selectedRect.StrokeThickness = 0;
                }
            }

            if (cropSelectionRectangle != null)
            {
                if (e.Key == Key.X)
                {
                    cropSelectionRectangle.Width += 1;
                    cropSelectionRectangle.Height += 1;
                }
                else if (e.Key == Key.S)
                {
                    cropSelectionRectangle.Width -= 1;
                    cropSelectionRectangle.Height -= 1;
                }
            }

        }

        System.Windows.Shapes.Rectangle cropSelectionRectangle;
        private void CropImageSetAreaRef_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // crop
            Canvas cropCanvas = new Canvas();
            cropCanvas.ClipToBounds = true;
            cropCanvas.Width = 200;
            cropCanvas.Height = 200;
            Canvas.SetLeft(cropCanvas, 200);
            Canvas.SetTop(cropCanvas, 200);
            canvas.Children.Add(cropCanvas);

            cropSelectionRectangle = new System.Windows.Shapes.Rectangle();
            cropSelectionRectangle.Stroke = new SolidColorBrush(Colors.Red);
            //cropSelectionRectangle.Fill = new SolidColorBrush(Colors.Black);
            cropSelectionRectangle.Visibility = Visibility.Visible;
            cropSelectionRectangle.StrokeThickness = 2;
            cropSelectionRectangle.Width = 100;
            cropSelectionRectangle.Height = 100;
            Canvas.SetLeft(cropSelectionRectangle, 20);
            Canvas.SetTop(cropSelectionRectangle, 20);
            canvas.Children.Add(cropSelectionRectangle);
            //cropCanvas.Children.Add(cropSelectionRectangle);

        }
        private void SetImageStretch(Image image)
        {
            if (image.Source.Width > image.ActualWidth || image.Source.Height > image.ActualHeight)
                image.Stretch = Stretch.Uniform;
            else image.Stretch = Stretch.None;
        }

        private void CropImageExecuteRef_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Image LoadedImage = (Image)_selectedElement;
            BitmapSource bitmapSource = (BitmapSource)LoadedImage.Source;
            var imagePosition = LoadedImage.TransformToAncestor(canvasgrid).Transform(new Point(0, 0));
            Rect rect1 = new Rect(Math.Max(Canvas.GetLeft(cropSelectionRectangle) - imagePosition.X, 0), Math.Max(Canvas.GetTop(cropSelectionRectangle) - imagePosition.Y, 0), cropSelectionRectangle.Width, cropSelectionRectangle.Height);
            //var ratioX = LoadedImage.Source.Width / LoadedImage.ActualWidth;
            //var ratioY = LoadedImage.Source.Height / LoadedImage.ActualHeight;
            var ratioX = bitmapSource.PixelWidth / LoadedImage.ActualWidth;
            var ratioY = bitmapSource.PixelHeight / LoadedImage.ActualHeight;

            Int32Rect rcFrom = new Int32Rect
            {
                X = (int)(rect1.X * ratioX),
                Y = (int)(rect1.Y * ratioY),
                Width = (int)(rect1.Width * ratioX),
                Height = (int)(rect1.Height * ratioY)
            };

            //rcFrom = new Int32Rect(bitmapSource.PixelWidth / 4, bitmapSource.PixelHeight / 4, bitmapSource.PixelWidth / 2, bitmapSource.PixelHeight / 2);

            try
            {
                Image cropedImage = new Image();
                cropedImage.Width = rect1.Width;
                cropedImage.Height = rect1.Height;
                BitmapSource bs = new CroppedBitmap((BitmapSource)LoadedImage.Source, rcFrom);
                cropedImage.Source = bs;
                //SetImageStretch(cropedImage);
                Canvas.SetLeft(cropedImage, 200);
                Canvas.SetTop(cropedImage, 200);
                canvas.Children.Add(cropedImage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            canvas.Children.Remove(cropSelectionRectangle);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Enabled_MoveWindow) {
                if (e.ChangedButton == MouseButton.Left)
                {
                    DragMove();
                }
            }
            else
            {
                return;
            }

        }

        private void RotateImage90Ref_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateImage(90);
        }
        private void RotateImage180Ref_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateImage(180);
        }
        private void RotateImage270Ref_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateImage(270);
        }
        private void RotateImage360Ref_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateImage(360);
        }

        private void RotateImage(int angle)
        {
            if (_selectedElement != null)
            {
                string typename = _selectedElement.GetType().FullName;
                if (typename.Contains("Image"))
                {
                    TransformGroup trGrp = new TransformGroup();
                    RotateTransform rotateTransform = new RotateTransform(angle);
                    trGrp.Children.Add(rotateTransform);
                    Image tempimage = (Image)_selectedElement;
                    rotateTransform.CenterX = tempimage.ActualHeight / 2;
                    rotateTransform.CenterY = tempimage.ActualWidth / 2;
                    tempimage.LayoutTransform = trGrp;
                    /*
                    string originalImagePath="";
                    string typename2 = tempimage.Source.GetType().FullName;
                    if (typename2.Contains("TransformedBitmap"))
                    {
                        TransformedBitmap transformBmp1 = (TransformedBitmap)tempimage.Source;
                        BitmapImage tempbs = (BitmapImage)transformBmp1.Source;
                        originalImagePath = tempbs.UriSource.AbsolutePath;
                    }
                    else
                    {
                        BitmapImage tempBmpImage = (BitmapImage)tempimage.Source;
                        originalImagePath = tempBmpImage.UriSource.AbsolutePath;
                    }
                    // Create an Image
                    Image imgControl = new Image();
                    imgControl.Width = 300;
                    imgControl.Height = 200;
                    // Create the TransformedBitmap
                    TransformedBitmap transformBmp = new TransformedBitmap();
                    // Create a BitmapImage
                    BitmapImage bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.UriSource = new Uri(originalImagePath, UriKind.RelativeOrAbsolute);
                    bmpImage.EndInit();
                    // Properties must be set between BeginInit and EndInit
                    transformBmp.BeginInit();
                    transformBmp.Source = bmpImage;
                    RotateTransform transform = new RotateTransform(angle);
                    transformBmp.Transform = transform;
                    transformBmp.EndInit();
                    // Set Image.Source to TransformedBitmap
                    imgControl.Source = transformBmp;

                    Canvas.SetLeft(imgControl, 200);
                    Canvas.SetTop(imgControl, 200);
                    canvas.Children.Add(imgControl);
                    */
                }
                //selectedRect.StrokeThickness = 0;
            }
        }

        bool Enabled_MoveWindow = false;
        private void Enable_Window_MouseDown(object sender, RoutedEventArgs e) {
            if (!Enabled_MoveWindow){
                //MouseDown = null;
                Enabled_MoveWindow = true;
            }
        }
        private void Disable_Window_MouseDown(object sender, RoutedEventArgs e)
        {
            if (Enabled_MoveWindow)
            {
                //MouseDown = null;
                Enabled_MoveWindow = false;
            }
        }

        int imageDPI = 75;
        System.Windows.Shapes.Rectangle selectedRect;
        int currentSelectedBorderThickness = 5;
        public MainWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["currentSelectedBorderThickness"]))
            {
                int.TryParse(ConfigurationManager.AppSettings["currentSelectedBorderThickness"], out currentSelectedBorderThickness);
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ImageDPIfactor"]))
            {
                int.TryParse(ConfigurationManager.AppSettings["ImageDPIfactor"], out imageDPI);
            }

            float tempZoomfactor = 1.1f;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ZoomScalefactor"])){
                float.TryParse( ConfigurationManager.AppSettings["ZoomScalefactor"], out tempZoomfactor);
            }
            canvas.Zoomfactor = tempZoomfactor;
            canvas.SetGridVisibility(Visibility.Hidden);

            selectedRect = new System.Windows.Shapes.Rectangle();
            selectedRect.Stroke = new SolidColorBrush(Colors.Red);
            //rect.Fill = new SolidColorBrush(Colors.Black);
            selectedRect.StrokeThickness = 0;
            selectedRect.Width = 200;
            selectedRect.Height = 200;
            Canvas.SetLeft(selectedRect, 200);
            Canvas.SetTop(selectedRect, 200);
            canvas.Children.Add(selectedRect);
            //CanvasBorder.BorderThickness = new Thickness(5);



            //Hole.HoleEntity.Tag = HolePattern as object;
            //canvas.Children.Add(Hole.HoleEntity);

            // moving window : howto ????????????????????????
            MouseDown += Window_MouseDown;

            // faster zooming in/out
            //canvas.MouseEnter += new MouseEventHandler(canvas_MouseEnter);
            /*
            canvas.MouseWheel += new MouseWheelEventHandler(canvas_MouseWheel);
            
            void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
            {
                Point p = e.MouseDevice.GetPosition(canvas);

                Matrix m = canvas.RenderTransform.Value;
                if (e.Delta > 0)
                    m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
                else
                    m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

                canvas.RenderTransform = new MatrixTransform(m);
            }
            
            
            void canvas_MouseWheel2(object sender, MouseWheelEventArgs e)
            {
                var st = new ScaleTransform();
                canvas.RenderTransform = st;
                if (e.Delta > 0)
                {
                    st.ScaleX *= 2;
                    st.ScaleY *= 2;
                }
                else
                {
                    st.ScaleX /= 2;
                    st.ScaleY /= 2;
                }
            }
            */
            /*
            CustomControls.Widget w1 = new CustomControls.Widget
            {
                Width = 200,
                Height = 50
            };
            canvas.Children.Add(w1);
            w1.Header.Text = "Image Art :";
            Canvas.SetTop(w1, 100);
            Canvas.SetLeft(w1, 100);

            CustomControls.Widget w2 = new CustomControls.Widget
            {
                Width = 200,
                Height = 50
            };
            canvas.Children.Add(w2);
            w2.Header.Text = "PDF Art :";
            w2.HeaderRectangle.Fill = System.Windows.Media.Brushes.Yellow;
            Canvas.SetTop(w2, 100);
            Canvas.SetLeft(w2, 400);
            */
            CustomControls.NoteWidget w3 = new CustomControls.NoteWidget
            {
                Width = 200,
                Height = 100
            };
            canvas.Children.Add(w3);
            //w3.Header.Text = "Note :";
            //w3.HeaderRectangle.Fill = Brushes.Red;
            //w3.NoteText.Text = ".........";
            Canvas.SetTop(w3, 100);
            Canvas.SetLeft(w3, 800);

            /*
            Microsoft.Web.WebView2.Wpf.WebView2 wpfWebViewerPDF = new Microsoft.Web.WebView2.Wpf.WebView2();
            wpfWebViewerPDF.Width = 800;
            wpfWebViewerPDF.Height = 800;
            wpfWebViewerPDF.Source = new Uri("C:\\SA\\sample.pdf");
            //wpfWebViewerPDF.Source = new Uri("http://www.google.com");
            wpfWebViewerPDF.Visibility = Visibility.Visible;
            canvas.Children.Add(wpfWebViewerPDF);
            Canvas.SetTop(wpfWebViewerPDF, 60);
            Canvas.SetLeft(wpfWebViewerPDF, 400);
            */
            /*
            ImageCodecInfo myImageCodecInfo;
            System.Drawing.Imaging.Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;
            // Get an ImageCodecInfo object that represents the JPEG codec.
            myImageCodecInfo = GetEncoderInfo("image/jpeg");

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            myEncoder = System.Drawing.Imaging.Encoder.Quality;
            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            myEncoderParameters = new EncoderParameters(1);
            // Save the bitmap as a JPEG file with quality level 75.
            myEncoderParameter = new EncoderParameter(myEncoder, 500L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            //myBitmap.Save("Shapes075.jpg", myImageCodecInfo, myEncoderParameters);
            source0.Save("C:\\SA\\sample1117.jpg", myImageCodecInfo, myEncoderParameters);
            */

            //canvas.Children.Add(pdfViewer);
            //Canvas.SetTop(pdfViewer, 60);
            //Canvas.SetLeft(pdfViewer, 400);


            System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();
            textBox.Text = "Title here";
            textBox.FontSize = 16f;
            textBox.Background = System.Windows.Media.Brushes.Red;
            textBox.Name = "TextTitle";
            canvas.Children.Add(textBox);
            Canvas.SetTop(textBox, 10);
            Canvas.SetLeft(textBox, 400);

            TextBlock TB2 = new TextBlock();
            TB2.TextWrapping = TextWrapping.WrapWithOverflow;
            TB2.Text = "Try Delete ME : click then Press DELETE key";
            TB2.Background = Brushes.Green;
            TB2.Name = "TextDelete";
            canvas.Children.Add(TB2);
            Canvas.SetLeft(TB2, 50);
            Canvas.SetTop(TB2, 400);


            TextBlock TB = new TextBlock();
            TB.TextWrapping = TextWrapping.WrapWithOverflow;
            TB.Text = "Help: ****** OpenPureRef  ******  \n 1 : can edit text notes as titles \n 2 : if need delete some item, just click item then press DELETE key \n 3: select multiple pdf before dragging and dropping them into the application \n 4 : Move window using Left-mouse drag after [ enable move window ] by Menu ; [ Disable move window ] after move \n 5 : Drag select Rectangle Over Croped image, set Crop Area by Press x key for enlarge + s for shrink, after setup Crop area ready, click submenu Crop \n ";
            TB.Background = Brushes.White;
            TB.Name = "TextHelper";
            canvas.Children.Add(TB);
            Canvas.SetLeft(TB, 10);
            Canvas.SetTop(TB, 50);

        }

        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private void AddNoteRef_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomControls.NoteWidget w3 = new CustomControls.NoteWidget
            {
                Width = 200,
                Height =100
            };
            canvas.Children.Add(w3);
            //w3.Header.Text = "Note :";
            //w3.HeaderRectangle.Fill = Brushes.Red;
            //w3.NoteText.Text = ".........";
            Canvas.SetTop(w3, 100);
            Canvas.SetLeft(w3, 800);

        }

        private void Canvas_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;

            e.Handled = true;
        }
        private void Window_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;

            e.Handled = true;
        }

        private void Canvas_Drop(object sender, System.Windows.DragEventArgs e)
        {
            pdfcount++;

            /*
            if (e.Data.GetDataPresent("myFormat"))
            {
                Contact contact = e.Data.GetData("myFormat") as Contact;
                ListView listView = sender as ListView;
                listView.Items.Add(contact);
            }
            */
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string finalfilenames = "";
                string[] filenames = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                for (int i=0;i< filenames.Count(); i++) {
                    if (filenames[i].EndsWith(".pdf"))
                    {
                        // PDF processing ......
                        //var doc = new Document(filenames[0]);
                        //PdfLoadOptions loadOptions = new PdfLoadOptions();
                        //loadOptions.SkipPdfImages=true;
                        /*
                        System.IO.FileStream pdfstream = File.OpenRead(filenames[0]);
                        var doc = new Document(pdfstream);
                        for (int page = 0; page < doc.PageCount; page++)
                        {
                            Document extractedPage = doc.ExtractPages(page, 1);
                            extractedPage.Save($"Output{pdfcount}_{page + 1}.jpg");
                        }
                        string currentFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                        finalfilenames = currentFolder + $"\\" + $"Output{pdfcount}_1.jpg";
                        */
                        /*
                        string currentFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                        // for removing watermark, using other library
                        // instantiate a pdf rasterizer (pdf to image converter) object
                        PdfRasterizer rasterizer = new PdfRasterizer();
                        // load PDF file
                        rasterizer.Load(filenames[0]);
                        // set the properties
                        rasterizer.StartPageNumber = 1;
                        rasterizer.EndPageNumber = 1;
                        System.Drawing.Image[] images = rasterizer.ConvertToImages();
                        rasterizer.ImagesPath = currentFolder;
                        rasterizer.ImagesPrefix = $"Output{pdfcount}";
                        rasterizer.ImagesFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                        rasterizer.Save();

                        finalfilenames = currentFolder + $"\\" + $"Output{pdfcount}1.jpeg";
                        */

                        string currentFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                        String currentUserDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        currentUserDataFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.UserAppDataPath);

                        // for removing watermark, using other library
                        //===========
                        // limit free versin not use for produt
                        /*
                        Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
                        pdf.LoadFromFile(filenames[i]);
                        System.Drawing.Image source0 = pdf.SaveAsImage(0, imageDPI, imageDPI);
                        source0.Save(currentUserDataFolder + $"\\" + $"Output{pdfcount}{i}_1.jpg");
                        */
                        //==============
                        
                        GcPdfDocument doc = new GcPdfDocument();
                        //using (var fs = new FileStream(Path.Combine("Resources", "MonthlyProjectExpenseTracking.pdf"), FileMode.Open, FileAccess.Read))
                        {
                            System.IO.FileStream fs = File.OpenRead(filenames[i]);
                            //var fs = pdfstream;
                            doc.Load(fs, "");
                            //doc.ImageOptions.JpegQuality = 100;
                            //doc.ImageOptions.CompressColors  = false;
                            //Render a page as an image
                            var page1 = doc.Pages.First();
                            SaveAsImageOptions optopnDPI = new SaveAsImageOptions();
                            optopnDPI.Resolution = imageDPI;
                            page1.SaveAsJpeg(currentUserDataFolder + $"\\" + $"Output{pdfcount}{i}_1.jpg",optopnDPI);
                        }
                        

                        finalfilenames = currentUserDataFolder + $"\\" + $"Output{pdfcount}{i}_1.jpg";

                    }
                    else
                    {
                        finalfilenames = filenames[i];
                    }

                    // Create Image and set its width and height  
                    Image dynamicImage = new Image();
                    dynamicImage.Width = 300;
                    dynamicImage.Height = 200;

                    // Create a BitmapSource  
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(finalfilenames);
                    //bitmap.Rotation = Rotation.Rotate270;
                    bitmap.EndInit();

                    // Set Image.Source  
                    dynamicImage.Source = bitmap;

                    // Add Image to Window  
                    canvas.Children.Add(dynamicImage);
                    Canvas.SetTop(dynamicImage, 500 + 20 * i);
                    Canvas.SetLeft(dynamicImage, 500 + 20 * i);

                }
            }


            /*
            if (e.Data.GetDataPresent(typeof(ImageSource))) {
                foreach (var format in e.Data.GetFormats())
                {
                    ImageSource imageSource = e.Data.GetData(format) as ImageSource;
                    if (imageSource != null)
                    {
                        Image img = new Image();
                        img.Height = 100;
                        img.Source = imageSource;
                        ((Canvas)sender).Children.Add(img);
                    }
                }
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("this is test");
                foreach (var format in e.Data.GetFormats())
                {
                    ImageSource imageSource = e.Data.GetData(format) as ImageSource;
                    if (imageSource != null)
                    {
                        Image img = new Image();
                        img.Height = 100;
                        img.Source = imageSource;
                        ((Canvas)sender).Children.Add(img);
                    }
                }
            }
            */
        }
        private void ImportImageRef_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Create Image and set its width and height  
            Image dynamicImage = new Image();
            dynamicImage.Width = 300;
            dynamicImage.Height = 200;

            // Create a BitmapSource  
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"C:\SA\youtubelogo2.JPG");
            bitmap.EndInit();

            // Set Image.Source  
            dynamicImage.Source = bitmap;

            // Add Image to Window  
            canvas.Children.Add(dynamicImage);
            Canvas.SetTop(dynamicImage, 500);
            Canvas.SetLeft(dynamicImage, 500);


            Image dynamicImage2 = new Image();
            dynamicImage2.Width = 300;
            dynamicImage2.Height = 200;

            // Create a BitmapSource  
            BitmapImage bitmap2 = new BitmapImage();
            bitmap2.BeginInit();
            bitmap2.UriSource = new Uri(@"C:\SA\forest.JPG");
            bitmap2.EndInit();

            // Set Image.Source  
            dynamicImage2.Source = bitmap2;

            // Add Image to Window  
            canvas.Children.Add(dynamicImage2);
            Canvas.SetTop(dynamicImage2, 200);
            Canvas.SetLeft(dynamicImage2, 200);


            //document.AddTextArea(RightX, RightY);
            //DrawPage();
        }

        private void ImportVectRefFile_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.svg)|*.svg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Image dynamicImage = new Image();
                dynamicImage.Width = 300;
                dynamicImage.Height = 200;

                string selectedFileName = dlg.FileName;
                var converter = new SharpVectors.Converters.ImageSvgConverter(null);
                string temporary = selectedFileName + ".tmp.png";
                string convertedFileName = selectedFileName.Replace(".svg", "").Replace(".SVG", "") + ".png";
                converter.Convert(selectedFileName, temporary);

                //FileNameLabel.Content = selectedFileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(temporary);
                bitmap.EndInit();
                //ImageViewer1.Source = bitmap;

                // Set Image.Source  
                dynamicImage.Source = bitmap;
                //File.Delete(temporary);

                // Add Image to Window  
                canvas.Children.Add(dynamicImage);
                Canvas.SetTop(dynamicImage, 200);
                Canvas.SetLeft(dynamicImage, 200);

            }
        }
        
        private void ImportImageRefFile_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Image dynamicImage = new Image();
                dynamicImage.Width = 300;
                dynamicImage.Height = 200;

                string selectedFileName = dlg.FileName;
                //FileNameLabel.Content = selectedFileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);
                bitmap.EndInit();
                //ImageViewer1.Source = bitmap;

                // Set Image.Source  
                dynamicImage.Source = bitmap;

                // Add Image to Window  
                canvas.Children.Add(dynamicImage);
                Canvas.SetTop(dynamicImage, 200);
                Canvas.SetLeft(dynamicImage, 200);

            }
        }
        private void ImportPDFImageRefFile_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            pdfcount++;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.pdf)|*.pdf|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            String currentUserDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            currentUserDataFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.UserAppDataPath);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Image dynamicImage = new Image();
                dynamicImage.Width = 300;
                dynamicImage.Height = 200;

                string selectedFileName = dlg.FileName;
                //FileNameLabel.Content = selectedFileName;
                // PDF processing ......
                var doc = new Document(selectedFileName);
                for (int page = 0; page < doc.PageCount; page++)
                {
                    Document extractedPage = doc.ExtractPages(page, 1);
                    extractedPage.Save(currentUserDataFolder + $"\\" + $"Output{pdfcount}_{page + 1}.jpg");
                }

                string currentFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(currentUserDataFolder + $"\\" + $"Output{pdfcount}_1.jpg");
                bitmap.EndInit();
                //ImageViewer1.Source = bitmap;

                // Set Image.Source  
                dynamicImage.Source = bitmap;

                // Add Image to Window  
                canvas.Children.Add(dynamicImage);
                Canvas.SetTop(dynamicImage, 200);
                Canvas.SetLeft(dynamicImage, 200);
            }

        }

    }
}
