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
using System.Timers;
using System.ComponentModel;

namespace WpfCanvasPractise
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private Rectangle rect;
        int count = 1;
        Timer timer;


        private void registerChangedEventListener(Rectangle rectangle)
        {
            var topDescriptor = DependencyPropertyDescriptor.FromProperty(Rectangle.RenderTransformProperty, typeof(Rectangle));
            topDescriptor.AddValueChanged(rectangle, rectangle_PositionChanged_top);
            //leftDescriptor.AddValueChanged(rectangle, rectangle_PositionChanged_left);
        }

        private void rectangle_PositionChanged_top(object sender, EventArgs e)
        {
            Rectangle rectangle = (Rectangle)sender;
            var v = rectangle.RenderTransform.Value;
            string name = rectangle.Name;
        }



        public MainWindow()
        {
            double X1 = 10, X2 = 50, Y1 = 50, Y2 = 20;
            int width = 50, height = 20;
            InitializeComponent();
            Rectangle staticRectangle = new Rectangle();
            staticRectangle.Width = width;
            staticRectangle.Height = height;
            staticRectangle.Fill = Brushes.Blue;
            staticRectangle.Opacity = 0.5;
            TranslateTransform translateTransform = new TranslateTransform(X1, Y1);
            staticRectangle.RenderTransform = translateTransform;

            this.can.Children.Add(staticRectangle);

            Rectangle movedRectangle = new Rectangle();
            movedRectangle.Width = width;
            movedRectangle.Height = height;
            movedRectangle.Fill = Brushes.Blue;
            movedRectangle.Opacity = 0.5;
            TranslateTransform translateTransform1 = new TranslateTransform(X2, Y2);
            movedRectangle.RenderTransform = translateTransform1;

            this.can.Children.Add(movedRectangle);

            registerChangedEventListener(movedRectangle);


            Line line = new Line();
            line.Stroke = System.Windows.Media.Brushes.Black;
            line.X1 = X1 + width / 2;
            line.X2 = X2 + (width / 2);
            line.Y1 = Y1;
            line.Y2 = Y2 + height;
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Center;
            line.StrokeThickness = 2;
            this.can.Children.Add(line);


            this.rect = movedRectangle;

            timer = new Timer(50);
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;

            //staticRectangle.MouseMove += new MouseEventHandler(ellipse_MouseMove);
            //staticRectangle.AllowDrop = true;
            //staticRectangle.DragEnter += new DragEventHandler(ellipse_DragEnter);

            X1 = 20;
            X2 = 70;
            Y1 = 100;
            Y2 = 30;
            Line line1 = new Line();
            line1.Stroke = System.Windows.Media.Brushes.Black;
            line1.X1 = X1 + width / 2;
            line1.X2 = X2 + (width / 2);
            line1.Y1 = Y1;
            line1.Y2 = Y2 + height;
            line1.HorizontalAlignment = HorizontalAlignment.Left;
            line1.VerticalAlignment = VerticalAlignment.Center;
            line1.StrokeThickness = 2;
            this.can.Children.Add(line1);

            this.can.Children.Add(DrawLinkArrow(new Point(120,120), new Point(180,10)));

        }

        private static Shape DrawLinkArrow(Point p1, Point p2)
        {
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.35), p1.Y + ((p2.Y - p1.Y) / 1.35));
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = lineGroup;
            path.StrokeThickness = 2;
            path.Stroke = path.Fill = Brushes.Black;

            return path;
        }


        private void ellipse_DragEnter(object sender, DragEventArgs e)
        {
            Rectangle staticRectangle = sender as Rectangle;
            if (staticRectangle != null)
            {
                //DragDrop.DoDragDrop(staticRectangle, "", DragDropEffects.None);
            }

            Console.WriteLine("Drag Enter");
        }

        private void ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle staticRectangle = sender as Rectangle;
            if (staticRectangle != null)
            {
                DragDrop.DoDragDrop(staticRectangle, "", DragDropEffects.None);
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            count++;

            this.Dispatcher.Invoke(new Action(() =>
            {
                //MessageBox.Show("This should not freeze the window");
                if (count < 20)
                {
                    TranslateTransform translateTransform1 = new TranslateTransform(50 + count * 2, 20);
                    rect.RenderTransform = translateTransform1;
                    this.can.UpdateLayout();
                }

            }));

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 2; i++)
            {
                TranslateTransform translateTransform1 = new TranslateTransform(50 + count * i, 20);
                rect.RenderTransform = translateTransform1;
                this.can.UpdateLayout();
                ++count;
            }
        }
    }
}
