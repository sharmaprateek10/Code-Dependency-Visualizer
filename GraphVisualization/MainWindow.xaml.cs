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
using System.ComponentModel;

namespace GraphVisualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GraphVisualizationManager graphDrawingManager = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

        }

        private async void OnOpen(object sender, RoutedEventArgs e)
        {
            front_canvas.Children.Clear();
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "CodeDependencyAnalyzer";
            dlg.DefaultExt = "XML Files|*.xml";


            // Show open file dialog box
            //Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            //if (result == true)
            {
                string filename = dlg.FileName;
                //var xmlReadingTask =  Task.Factory.StartNew(()=>readXMLFile(filename));
                var xmlReadingTask = Task.Factory.StartNew(() => readXMLFile(@"C:\Users\pshar_000\Downloads\Graph\Test\AnalysisResult.xml"));
                await xmlReadingTask;
            }
            graphDrawingManager.drawComponents(front_canvas);
        }

        private void readXMLFile(string filePath)
        {
            CodeDepedencyXMLParser parser = new CodeDepedencyXMLParser(filePath);
            parser.parseXML();
            graphDrawingManager = new GraphVisualizationManager(parser.VisualGraph, front_canvas.ActualWidth, front_canvas.ActualHeight,this.Dispatcher);
            graphDrawingManager.Parent = this;
            graphDrawingManager.computeBase();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void front_canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as UIElement;
            var position = e.GetPosition(element);
            var transform = element.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;
            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            transform.Matrix = matrix;
        }

    }
}
