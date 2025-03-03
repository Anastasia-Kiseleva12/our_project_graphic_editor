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

            public void DrawTriangle(bool IsSelected, Point Point1, Point Point2, Point Point3)
            {
                if (IsSelected)
                {
                    var highlightGeometry = new PathGeometry();
                    var highlightFigure = new PathFigure
                    {
                        StartPoint = new Avalonia.Point(Point1.X, Point1.Y)
                    };
                    highlightFigure.Segments.Add(new LineSegment { Point = new Avalonia.Point(Point2.X, Point2.Y) });
                    highlightFigure.Segments.Add(new LineSegment { Point = new Avalonia.Point(Point3.X, Point3.Y) });
                    highlightFigure.Segments.Add(new LineSegment { Point = new Avalonia.Point(Point1.X, Point1.Y) });
                    highlightGeometry.Figures.Add(highlightFigure);

                    var highlightShape = new Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = 6,
                        Data = highlightGeometry
                    };

                    DrawingCanvas.Children.Add(highlightShape);
                }

                var triangleGeometry = new PathGeometry();
                var triangleFigure = new PathFigure
                {
                    StartPoint = new Avalonia.Point(Point1.X, Point1.Y)
                };
                triangleFigure.Segments.Add(new LineSegment { Point = new Avalonia.Point(Point2.X, Point2.Y) });
                triangleFigure.Segments.Add(new LineSegment { Point = new Avalonia.Point(Point3.X, Point3.Y) });
                triangleFigure.Segments.Add(new LineSegment { Point = new Avalonia.Point(Point1.X, Point1.Y) });
                triangleGeometry.Figures.Add(triangleFigure);

                var triangleShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Data = triangleGeometry
                };

                DrawingCanvas.Children.Add(triangleShape);

                if (IsSelected)
                {
                    var point1Ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(Point1.X - 5, Point1.Y - 5, 0, 0)
                    };

                    var point2Ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(Point2.X - 5, Point2.Y - 5, 0, 0)
                    };

                    var point3Ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(Point3.X - 5, Point3.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(point1Ellipse);
                    DrawingCanvas.Children.Add(point2Ellipse);
                    DrawingCanvas.Children.Add(point3Ellipse);
                }
            }

            public void DrawRectangle(bool IsSelected, Point TopLeft, Point BottomRight)
            {
                if (IsSelected)
                {
                    var highlightGeometry = new RectangleGeometry
                    {
                        Rect = new Rect(new Avalonia.Point(TopLeft.X, TopLeft.Y), new Avalonia.Point(BottomRight.X, BottomRight.Y))
                    };

                    var highlightShape = new Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = 6,
                        Data = highlightGeometry
                    };

                    DrawingCanvas.Children.Add(highlightShape);
                }

                var rectangleGeometry = new RectangleGeometry
                {
                    Rect = new Rect(new Avalonia.Point(TopLeft.X, TopLeft.Y), new Avalonia.Point(BottomRight.X, BottomRight.Y))
                };

                var rectangleShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Data = rectangleGeometry
                };

                DrawingCanvas.Children.Add(rectangleShape);

                if (IsSelected)
                {
                    var topLeftEllipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(TopLeft.X - 5, TopLeft.Y - 5, 0, 0)
                    };

                    var bottomRightEllipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Margin = new Thickness(BottomRight.X - 5, BottomRight.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(topLeftEllipse);
                    DrawingCanvas.Children.Add(bottomRightEllipse);
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
