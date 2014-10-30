using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Timers;

namespace GraphVisualization
{
    class GraphVisualizationManager
    {
        private int size = 100;
        private double canvasWidth, canvasHeight;
        private Graph visualGraph = null;
        private static readonly int VertexCountThreshold = 20;
        private List<VertexWrapper> cordinatesWrapper = new List<VertexWrapper>();
        private Dictionary<string, int> wrapperMap = new Dictionary<string, int>();
        private Window parentWindow;
        private double resultantForce = -1;
        private static int stepSize = 15;
        private double originX, originY;
        private static readonly int EQUAL_SOLV = 50;
        private static readonly int EQUAL_SOLV_ERROR = 500;
        private int recursionLevel = 0;
        private Dispatcher dispatcher = null;
        private System.Timers.Timer timer = null;
        private Canvas drawingCanvas = null;

        public Window Parent
        {
            get { return parentWindow; }
            set { this.parentWindow = value; }
        }

        public GraphVisualizationManager(Graph visualGraph, double width, double height, Dispatcher uiDispatcher)
        {
            this.visualGraph = visualGraph;
            this.canvasHeight = height;
            this.canvasWidth = width;
            originX = width / 2;
            originY = height / 2;
            timer = new System.Timers.Timer(50);
            timer.Elapsed += OnTimedEvent;
            this.dispatcher = uiDispatcher;
            //timer.Enabled = true
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            dispatcher.BeginInvoke(new Action(() =>
            {
                drawingCanvas.UpdateLayout();

            }));
        }

        private void registerChangedEventListener(Rectangle rectangle)
        {
            var topDescriptor = DependencyPropertyDescriptor.FromProperty(Rectangle.RenderTransformProperty, typeof(Rectangle));
            topDescriptor.AddValueChanged(rectangle, rectangle_PositionChanged);

        }

        private void rectangle_PositionChanged(object sender, EventArgs e)
        {
            Rectangle rectangle = (Rectangle)sender;
            string name = rectangle.Name;

        }

        public void computeBase()
        {
            int vertexCount = visualGraph.VertexList.Count;
            if (vertexCount > VertexCountThreshold)
            {
                size = size / 2;

            }

            Random random = new Random(DateTime.Now.Millisecond);
            while (cordinatesWrapper.Count != vertexCount)
            {
                double left = random.NextDouble() * (canvasWidth - 2 * size);
                double top = random.NextDouble() * (canvasHeight - 2 * size);

                Boolean available = true;
                foreach (VertexWrapper vw in cordinatesWrapper)
                {
                    if (vw.containsCordinate(left, top, size))
                    {
                        available = false;
                        break;
                    }
                }

                if (available)
                {
                    cordinatesWrapper.Add(new VertexWrapper(left, top));
                }
            }

        }

        public void drawComponents(Canvas drawingCanvas)
        {
            List<string> vertexNames = visualGraph.VertexList;
            for (int i = 0; i < vertexNames.Count; i++)
            {
                wrapperMap.Add(vertexNames[i], i);
                VertexWrapper wrapper = cordinatesWrapper[i];
                Rectangle vertex = new Rectangle();
                //since names cannot have dot
                vertex.Name = vertexNames[i].Replace('.', '_');
                vertex.Stroke = new SolidColorBrush(Colors.Black);
                vertex.Fill = new SolidColorBrush(Colors.White);
                vertex.Width = size;
                vertex.Height = size / 3;
                TranslateTransform translateTransform1 = new TranslateTransform(wrapper.Left, wrapper.Top);
                vertex.RenderTransform = translateTransform1;

                drawingCanvas.Children.Add(vertex);
                parentWindow.RegisterName(vertex.Name, vertex);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = vertexNames[i];
                textBlock.Foreground = new SolidColorBrush(Colors.Black);
                textBlock.FontSize = 10;
                Canvas.SetLeft(textBlock, wrapper.Left + 5);
                Canvas.SetTop(textBlock, wrapper.Top + 5);

                drawingCanvas.Children.Add(textBlock);
                wrapper.Rect = vertex;
                //registerChangedEventListener(vertex);
                wrapper.NameTextBlock = textBlock;
            }
            //addArrowDependencies(drawingCanvas);
            DrawArrows(drawingCanvas);
            this.drawingCanvas = drawingCanvas;
            Thread thread = new Thread(new ThreadStart(solve));
            thread.Start();
        }

        public void solve()
        {
            try
            {
                while (true)
                {
                    if ((resultantForce > 0 && resultantForce < EQUAL_SOLV) || (recursionLevel > 30 && resultantForce < EQUAL_SOLV_ERROR))
                    {
                        timer.Enabled = false;
                        break;
                    }
                    solveForces(visualGraph.VertexList, drawingCanvas);
                    updateUI();
                    Thread.Sleep(300);
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        private void updateUI()
        {
            dispatcher.Invoke(new Action(() =>
            {
                foreach (VertexWrapper vertex in cordinatesWrapper)
                {
                    TranslateTransform transform = new TranslateTransform(vertex.Left, vertex.Top);
                    vertex.Rect.RenderTransform = transform;
                    Canvas.SetLeft(vertex.NameTextBlock, vertex.Left + 5);
                    Canvas.SetTop(vertex.NameTextBlock, vertex.Top + 5);

                    foreach (Shape s in vertex.getIN())
                    {
                        drawingCanvas.Children.Remove(s);
                    }

                    foreach (Shape s in vertex.getOUT())
                    {
                        drawingCanvas.Children.Remove(s);
                    }
                }
                DrawArrows(drawingCanvas);
                drawingCanvas.UpdateLayout();
            }));
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


        private void solveForces(List<String> vertices, Canvas drawingCanvas)
        {
            Console.WriteLine(resultantForce);
            ++recursionLevel;

            resultantForce = 0;
            double xr = 0, yr = 0;
            foreach (string vertexName in vertices)
            {
                HashSet<string> neighbors = visualGraph.getNeighbors(vertexName);
                VertexWrapper sourceWrapper = cordinatesWrapper[wrapperMap[vertexName]];
                double xForceMagnitude = 0, yForceMagnitude = 0;
                foreach (string n in neighbors)
                {
                    VertexWrapper targetWrapper = cordinatesWrapper[wrapperMap[n]];
                    double X1 = sourceWrapper.Left + (size / 2) - originX;
                    double X2 = targetWrapper.Left + (size / 2) - originX;
                    double Y1 = sourceWrapper.Top + (size / 3) - originY;
                    double Y2 = targetWrapper.Top + (size / 3) - originY;

                    double forceMagnitude = Math.Sqrt(Math.Pow(X2 - X1, 2) + Math.Pow(Y2 - Y1, 2));
                    double direction = Math.Atan(((Y2 - Y1) / (X2 - X1))) * 180 / Math.PI;

                    yForceMagnitude += forceMagnitude * Math.Sin(direction);
                    xForceMagnitude += forceMagnitude * Math.Cos(direction);
                }
                double resultantForceM = Math.Sqrt(Math.Pow(xForceMagnitude, 2) + Math.Pow(yForceMagnitude, 2));
                double resultantForceD = Math.Atan(yForceMagnitude / xForceMagnitude) * 180 / Math.PI;

                xr += resultantForceM * Math.Cos(resultantForceD);
                yr += resultantForceM * Math.Sin(resultantForceD);

                if (!double.IsNaN(resultantForceD))
                {
                    double effectiveDir = 180 + resultantForceD;
                    sourceWrapper.Left = sourceWrapper.Left + stepSize * Math.Cos(effectiveDir);
                    sourceWrapper.Top = sourceWrapper.Top + stepSize * Math.Sin(effectiveDir);
                }

            }
            resultantForce = Math.Sqrt(Math.Pow(xr, 2) + Math.Pow(yr, 2));
        }

        private void DrawArrows(Canvas drawingCanvas)
        {
            foreach (string vertexName in visualGraph.VertexList)
            {
                HashSet<string> neighbors = visualGraph.UndirectedGraph[vertexName];
                VertexWrapper sourceWrapper = cordinatesWrapper[wrapperMap[vertexName]];
                foreach (string n in neighbors)
                {
                    VertexWrapper targetWrapper = cordinatesWrapper[wrapperMap[n]];

                    double X1 = sourceWrapper.Left + (size / 2);
                    double X2 = targetWrapper.Left + (size / 2);
                    double Y1 = sourceWrapper.Top + (size / 3);
                    double Y2 = targetWrapper.Top + (size / 3);
                    //Mod
                    Shape arrowDep = DrawLinkArrow(new Point(X1, Y1), new Point(X2, Y2));
                    drawingCanvas.Children.Add(arrowDep);

                    sourceWrapper.addOUT(arrowDep);
                    targetWrapper.addIN(arrowDep);
                }
            }
        }

    }

    class VertexWrapper
    {
        private double left;
        private double top;
        private VectorForce force;
        private Rectangle canvasRect;
        private List<Line> lines = new List<Line>();
        private List<Line> targetEdges = new List<Line>();

        private List<Shape> outgoingEdges = new List<Shape>();
        private List<Shape> incomingEdges = new List<Shape>();
        private TextBlock name;


        public void addOUT(Shape line)
        {
            outgoingEdges.Add(line);
        }

        public void addIN(Shape line)
        {
            incomingEdges.Add(line);
        }

        public List<Shape> getOUT()
        {
            return outgoingEdges;
        }

        public List<Shape> getIN()
        {
            return incomingEdges;
        }

        public TextBlock NameTextBlock
        {
            get { return name; }
            set { this.name = value; }
        }

        public void addLine(Line line)
        {
            lines.Add(line);
        }

        public void addTargetLine(Line line)
        {
            targetEdges.Add(line);
        }

        public List<Line> getTargetLines()
        {
            return targetEdges;
        }

        public List<Line> getLines()
        {
            return lines;
        }
        public VectorForce Force
        {
            get { return force; }
            set { this.force = value; }
        }

        public Rectangle Rect
        {
            get { return canvasRect; }
            set { this.canvasRect = value; }
        }
        public double Left
        {
            get { return left; }
            set { this.left = value; }
        }

        public double Top
        {
            get { return top; }
            set { this.top = value; }
        }
        public VertexWrapper(double left, double top)
        {
            this.left = left;
            this.top = top;
        }
        public Boolean containsCordinate(double left, double top, int size)
        {
            if (Math.Abs(this.left - left) < size && Math.Abs(this.top - top) < size)
            {
                return true;
            }
            return false;
        }
    }


    class VectorForce
    {
        private double direction;//in degrees
        private double magnitude;

        public double Dir
        {
            get { return direction; }
            set { this.direction = value; }
        }

        public double Magnitude
        {
            get { return magnitude; }
            set { this.magnitude = value; }
        }

        public VectorForce(double direction, double magnitude)
        {
            this.direction = direction;
            this.magnitude = magnitude;
        }
    }
}
