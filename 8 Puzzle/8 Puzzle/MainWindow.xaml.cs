using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _8_Puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        const int startX = 30;
        const int startY = 50;
        const int width = 100;
        const int height = 100;
        Tuple<int, int>[,] _a = new Tuple<int, int> [3,3];
        Tuple<int, int> space = new Tuple<int, int>(-1, -1);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                var source = new BitmapImage(
                    new Uri(screen.FileName, UriKind.Absolute));
                Debug.WriteLine($"{source.Width} - {source.Height}");
                previewImage.Width = 600;
                previewImage.Height = 400;
                previewImage.Source = source;

                Canvas.SetLeft(previewImage, 400);
                Canvas.SetTop(previewImage, 50);

                // Bat dau cat thanh 9 manh

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (!((i == 2) && (j == 2)))
                        {
                            var h = (int)source.Height;
                            var w = (int)source.Width;
                            //Debug.WriteLine($"Len = {len}");
                            var rect = new Int32Rect(j * w / 3, i * h / 3, w / 3, h / 3);

                            CroppedBitmap cropBitmap = new CroppedBitmap(source as BitmapSource, rect);

                            var cropImage = new Image();
                            cropImage.Stretch = Stretch.Fill;
                            cropImage.Width = width;
                            cropImage.Height = height;
                            cropImage.Source = cropBitmap;
                            canvas.Children.Add(cropImage);                           
                            Canvas.SetLeft(cropImage, startX + j * (width + 2));
                            Canvas.SetTop(cropImage, startY + i * (height + 2));

                            cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                            cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                            cropImage.Tag = new Tuple<int, int>(i, j);

                            _a[i, j] = cropImage.Tag as Tuple<int, int>;
                        }                      
                        _a[2, 2] = space;
                    }
                }



            }
        }
        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        Point _selectedPosition;
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            var position = e.GetPosition(this);

            int x = (int)(position.X - startX) / (width + 2) * (width + 2) + startX;
            int y = (int)(position.Y - startY) / (height + 2) * (height + 2) + startY;
            
            //var (i, j) = _selectedBitmap.Tag as Tuple<int, int>;
            var spacePos = GetPos(space);
            var selectedPos = GetPos(_selectedBitmap.Tag as Tuple<int, int>) as Tuple<int,int>;
            var a = (int)(position.X - startX) / (width + 2);
            var b = (int)(position.Y - startY) / (height + 2);
            if (!CheckCanMove(selectedPos))
            {
                Canvas.SetLeft(_selectedBitmap,_selectedPosition.X);
                Canvas.SetTop(_selectedBitmap,_selectedPosition.Y);
            }
            else if (a >= 3 || b >= 3)
            {
                Canvas.SetLeft(_selectedBitmap, _selectedPosition.X);
                Canvas.SetTop(_selectedBitmap, _selectedPosition.Y);
            }
            else if (!(a==spacePos.Item2&&b==spacePos.Item1))
            {
                Canvas.SetLeft(_selectedBitmap, _selectedPosition.X);
                Canvas.SetTop(_selectedBitmap, _selectedPosition.Y);
            }
            else{
                swap(selectedPos,spacePos);
                Canvas.SetLeft(_selectedBitmap, x);
                Canvas.SetTop(_selectedBitmap, y);
            }
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _selectedBitmap = sender as Image;
            _selectedPosition = _selectedBitmap.TransformToAncestor(this)
                              .Transform(new Point(0, 0));
            _lastPosition = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = ((int)position.Y - startY) / height;
            int j = ((int)position.X - startX) / width;

            

            if (_isDragging)
            {
                var dx = position.X - _lastPosition.X;
                var dy = position.Y - _lastPosition.Y;

                var lastLeft = Canvas.GetLeft(_selectedBitmap);
                var lastTop = Canvas.GetTop(_selectedBitmap);
                Canvas.SetLeft(_selectedBitmap, lastLeft + dx);
                Canvas.SetTop(_selectedBitmap, lastTop + dy);

                _lastPosition = position;
            }
        }



        private void previewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation();
            animation.From = 200;
            animation.To = 300;
            animation.Duration = new Duration(TimeSpan.FromSeconds(1));
            animation.AutoReverse = true;
            animation.RepeatBehavior = RepeatBehavior.Forever;


            var story = new Storyboard();
            story.Children.Add(animation);
            Storyboard.SetTargetName(animation, previewImage.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
            story.Begin(this);
        }
        /// <summary>
        /// kiểm tra xem mảnh mình chọn có thể di chuyển hay không
        /// </summary>
        /// <param name="pos">vị trí của mảnh mình chọn</param>
        /// <returns></returns>
        public bool CheckCanMove(Tuple<int,int> pos)
        {
            var (i, j) = pos;
            var spacePos = GetSpace();
            if ((Math.Abs(spacePos.Item1 - i) == 1 && Math.Abs(spacePos.Item2 - j) == 0)
                || (Math.Abs(spacePos.Item1 - i) == 0 && Math.Abs(spacePos.Item2 - j) == 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// tìm vị trí của 1 mảnh 
        /// </summary>
        /// <param name="tag">tag của mảnh</param>
        /// <returns></returns>
        public Tuple<int, int> GetPos(Tuple<int,int> tag)
        {
            Tuple<int, int> notFound = new Tuple<int, int>(-5, -5);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_a[i, j].Item1 == tag.Item1 && _a[i, j].Item2 == tag.Item2)
                    {
                        var position = new Tuple<int, int>(i, j);
                        return position;
                    }
                }
            }
            return notFound;
        }


        /// <summary>
        /// tìm vị trí của ô trống trong ma trận
        /// </summary>
        /// <returns>vị trí của ô trống</returns>
        public Tuple<int,int> GetSpace()
        {
           Tuple<int, int> notFound = new Tuple<int, int>(-5,-5);
           for(int i = 0; i < 3; i++)
           {
                for(int j =0; j < 3; j++)
                {
                    if (_a[i, j].Item1 == -1)
                    {
                        var location = new Tuple<int, int>(i, j);
                        return location;
                    }
                }
           }
           return notFound;
        }
        /// <summary>
        /// đổi vị trí 2 mảnh trong ma trận
        /// </summary>
        /// <param name="a">mảnh cần di chuyển</param>
        /// <param name="b">ô trống</param>
        public void swap(Tuple<int,int> a, Tuple<int, int> b)
        {
            var spacePos = GetPos(space);
            if (b.Item1== spacePos.Item1 && b.Item2==spacePos.Item2)
            {
                _a[b.Item1, b.Item2] = _a[a.Item1, a.Item2];
                _a[a.Item1, a.Item2] = space;
            }
        }
    }
}
