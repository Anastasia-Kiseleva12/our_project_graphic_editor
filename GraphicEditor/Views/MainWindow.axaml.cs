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
        private void Draw()
        {
            DrawingCanvas.Children.Clear();
            var figures = _viewModel._figureService.Figures;

            foreach (var figure in figures)
            {
                DrawFigure(figure);
            }
        }

        private void DrawFigure(IFigure figure)
        {
            switch (figure)
            {
                case Line line:
                    DrawLine(line);
                    break;
                case Circle circle:
                    DrawCircle(circle);
                    break;
            }
        }

        private void DrawLine(Line line)
        {
            if (line.IsSelected)
            {
                var highlightGeometry = new LineGeometry
                {
                    StartPoint = new Avalonia.Point(line.Start.X, line.Start.Y),
                    EndPoint = new Avalonia.Point(line.End.X, line.End.Y)
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
                StartPoint = new Avalonia.Point(line.Start.X, line.Start.Y),
                EndPoint = new Avalonia.Point(line.End.X, line.End.Y)
            };

            var lineShape = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Data = lineGeometry
            };

            DrawingCanvas.Children.Add(lineShape);

            if (line.IsSelected)
            {
                var startPoint = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(line.Start.X - 5, line.Start.Y - 5, 0, 0)
                };

                var endPoint = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(line.End.X - 5, line.End.Y - 5, 0, 0)
                };

                DrawingCanvas.Children.Add(startPoint);
                DrawingCanvas.Children.Add(endPoint);
            }
        }

        private void DrawCircle(Circle circle)
        {
            double radius = Math.Sqrt(Math.Pow(circle.PointOnCircle.X - circle.Center.X, 2) +
                            Math.Pow(circle.PointOnCircle.Y - circle.Center.Y, 2));

            if (circle.IsSelected)
            {
                var highlightGeometry = new EllipseGeometry
                {
                    Center = new Avalonia.Point(circle.Center.X, circle.Center.Y),
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
                Center = new Avalonia.Point(circle.Center.X, circle.Center.Y),
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

            if (circle.IsSelected)
            {
                var pointOnCircle = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(circle.PointOnCircle.X - 5, circle.PointOnCircle.Y - 5, 0, 0)
                };

                DrawingCanvas.Children.Add(pointOnCircle);
            }
        }
    }
}
