using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using GraphicEditor.ViewModels;

namespace GraphicEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.FiguresChanged += () =>
            {
                Dispatcher.UIThread.Post(() => Draw());
            };

             DrawingCanvas.PointerPressed += OnCanvasPointerPressed;
        }
        private void OnCanvasPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var avaloniaPoint = e.GetPosition(DrawingCanvas); 
            var point = new GraphicEditor.Point { X = avaloniaPoint.X, Y = avaloniaPoint.Y }; 
            _viewModel.HandleCanvasClick(point); 
        }
        class Drawer(Canvas DrawingCanvas) : IDrawing
        {
            public void DrawLine(bool IsSelected, Point Start,Point End)
            {
                if (IsSelected)
                {
                    var highlightGeometry = new LineGeometry
                    {
                        StartPoint = new Avalonia.Point(Start.X, Start.Y),
                        EndPoint = new Avalonia.Point(End.X, End.Y)
                    };

                    var highlightShape = new Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = 6,
                        Data = highlightGeometry
                    };

                    DrawingCanvas.Children.Add(highlightShape);
                }

                var lineGeometry = new LineGeometry
                {
                    StartPoint = new Avalonia.Point(Start.X, Start.Y),
                    EndPoint = new Avalonia.Point(End.X, End.Y)
                };

                var lineShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Data = lineGeometry
                };

                DrawingCanvas.Children.Add(lineShape);

                if (IsSelected)
                {
                    var startPoint = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(Start.X - 5, Start.Y - 5, 0, 0)
                    };

                    var endPoint = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(End.X - 5, End.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(startPoint);
                    DrawingCanvas.Children.Add(endPoint);
                }
            }

            public void DrawCircle( bool IsSelected, Point Center, double radius, Point PointOnCircle)
            {
                if (IsSelected)
                {
                    var highlightGeometry = new EllipseGeometry
                    {
                        Center = new Avalonia.Point(Center.X, Center.Y),
                        RadiusX = radius,
                        RadiusY = radius
                    };

                    var highlightShape = new Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = 6,
                        Data = highlightGeometry
                    };

                    DrawingCanvas.Children.Add(highlightShape);
                }

                var circleGeometry = new EllipseGeometry
                {
                    Center = new Avalonia.Point(Center.X, Center.Y),
                    RadiusX = radius,
                    RadiusY = radius
                };

                var circleShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Data = circleGeometry
                };

                DrawingCanvas.Children.Add(circleShape);

                if (IsSelected)
                {
                    var pointOnCircle = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(PointOnCircle.X - 5, PointOnCircle.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(pointOnCircle);
                }
            }
        }
        private void Draw()
        {
            DrawingCanvas.Children.Clear();
            var figures = _viewModel._figureService.Figures;
            var drawer = new Drawer(DrawingCanvas);
            foreach (var figure in figures)
                figure.Draw(drawer);
        }
    }
}
