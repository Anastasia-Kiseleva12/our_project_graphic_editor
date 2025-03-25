using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using GraphicEditor.ViewModels;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.IO;

namespace GraphicEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            IO.CanvasToSave = DrawingCanvas;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _viewModel = viewModel;
            DataContext = _viewModel;
            this.KeyDown += OnKeyDown;
            thicknessSlider.ValueChanged += ThicknessSlider_ValueChanged;

            _viewModel.FiguresChanged += () =>
            {
                Dispatcher.UIThread.Post(() => Draw(false));
            };

             DrawingCanvas.PointerPressed += OnCanvasPointerPressed;
             DrawingCanvas.PointerMoved += OnCanvasPointerMoved;
             DrawingCanvas.PointerReleased += OnCanvasPointerReleased;

            this.Closing += async (s, e) =>
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    e.Cancel = true;
                    await viewModel.Exit();
                }
            };
        }

        private void HidePopup(object sender, PointerEventArgs e)
        {
            if (sender is Border border && border.Parent is Popup popup)
            {
                popup.IsOpen = false; // Скрываем Popup при наведении
            }
        }

        private void ThicknessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Получаем новое значение толщины из слайдера
            double newThickness = e.NewValue;

            // Обновляем текст, отображающий текущее значение толщины
            thicknessValueText.Text = newThickness.ToString("F0");

            _viewModel.CurrentThickness = newThickness;

            // Вызываем метод Draw с новой толщиной
            Draw(_viewModel.SelectedFigure?.IsSelected ?? false);
        }

        private void ClickOnFillingButton(object sender, RoutedEventArgs args)
        {
            if (_viewModel.SelectedFigure != null)
            {
                var color = colorPicker.Color;
                _viewModel.SelectedFigure.SetColor(color.A, color.R, color.G, color.B);
                Draw(false);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                _viewModel.RemoveSelectedFiguresCommand.Execute().Subscribe();
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.C)
            {
                _viewModel.CopySelectedFiguresCommand.Execute().Subscribe();
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V)
            {
                _viewModel.PasteFiguresCommand.Execute().Subscribe();
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            // Отписываемся от событий при закрытии окна
            this.KeyDown -= OnKeyDown;
            base.OnClosed(e);
        }
        private void OnCanvasPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var avaloniaPoint = e.GetPosition(DrawingCanvas);
            var point = new GraphicEditor.Point (avaloniaPoint.X, avaloniaPoint.Y);
            _viewModel.HandleCanvasClick(point);

        }
        private void OnCanvasPointerMoved(object sender, PointerEventArgs e)
        {
            var avaloniaPoint = e.GetPosition(DrawingCanvas);
            var point = new GraphicEditor.Point (avaloniaPoint.X, avaloniaPoint.Y);
            _viewModel.HandleCanvasMove(point);
            Draw(false); // Перерисовываем canvas
        }
        private void OnCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _viewModel.HandleCanvasRelease();
            Draw(false); 
        }

        class Drawer(Canvas DrawingCanvas) : IDrawing
        {
            //отрисовка временных точек
            public void DrawTemporaryPoint(Point point, IImmutableSolidColorBrush fillBrush) 
            {
                var ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = fillBrush,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Margin = new Thickness(point.X - 5, point.Y - 5, 0, 0)
                };
                DrawingCanvas.Children.Add(ellipse);
            }
            //отрисовка временных линий
            public void DrawTemporaryLine(Point start, Point end, IImmutableSolidColorBrush strokeBrush)
            {
                var lineGeometry = new LineGeometry
                {
                    StartPoint = new Avalonia.Point(start.X, start.Y),
                    EndPoint = new Avalonia.Point(end.X, end.Y)
                };

                var lineShape = new Avalonia.Controls.Shapes.Path
                {
                    Stroke = strokeBrush,
                    StrokeThickness = 2,
                    Data = lineGeometry
                };

                DrawingCanvas.Children.Add(lineShape);
            }
            public void DrawLine(bool IsSelected, Point Start,Point End, double strokeThickness, uint Color, double Angle)
            {
                if (IsSelected)
                {
                    var highlightGeometry = new LineGeometry
                    {
                        StartPoint = new Avalonia.Point(Start.X, Start.Y),
                        EndPoint = new Avalonia.Point(End.X, End.Y)
                    };

                    var highlightShape = new Avalonia.Controls.Shapes.Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = strokeThickness + 4,
                        Data = highlightGeometry
                    };

                    DrawingCanvas.Children.Add(highlightShape);
                }

                var lineGeometry = new LineGeometry
                {
                    StartPoint = new Avalonia.Point(Start.X, Start.Y),
                    EndPoint = new Avalonia.Point(End.X, End.Y)
                };

                var lineShape = new Avalonia.Controls.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
                    Data = lineGeometry
                };
                if (Angle != 0)
                {
                    lineShape.RenderTransform = new RotateTransform(Angle);
                }
                DrawingCanvas.Children.Add(lineShape);


                    if (IsSelected)
                {
                    var startPoint = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Margin = new Thickness(Start.X - 5, Start.Y - 5, 0, 0)
                    };

                    var endPoint = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Margin = new Thickness(End.X - 5, End.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(startPoint);
                    DrawingCanvas.Children.Add(endPoint);
                }
            }

            public void DrawCircle( bool IsSelected, Point Center, double radius, Point PointOnCircle, double strokeThickness, uint Color, double Angle)
            {
                if (IsSelected)
                {
                    var highlightGeometry = new EllipseGeometry
                    {
                        Center = new Avalonia.Point(Center.X, Center.Y),
                        RadiusX = radius,
                        RadiusY = radius
                    };

                    var highlightShape = new Avalonia.Controls.Shapes.Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = strokeThickness + 4,
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

                var circleShape = new Avalonia.Controls.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
                    Fill = new SolidColorBrush(Color),
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
                        StrokeThickness = 2,
                        Margin = new Thickness(PointOnCircle.X - 5, PointOnCircle.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(pointOnCircle);
                }
            }

            public void DrawTriangle(bool IsSelected, Point Point1, Point Point2, Point Point3, double strokeThickness, uint Color, double Angle)
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

                    var highlightShape = new Avalonia.Controls.Shapes.Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = strokeThickness + 4,
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

                var triangleShape = new Avalonia.Controls.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
                    Fill = new SolidColorBrush(Color),
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
                        StrokeThickness = 2,
                        Margin = new Thickness(Point1.X - 5, Point1.Y - 5, 0, 0)
                    };

                    var point2Ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Margin = new Thickness(Point2.X - 5, Point2.Y - 5, 0, 0)
                    };

                    var point3Ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Margin = new Thickness(Point3.X - 5, Point3.Y - 5, 0, 0)
                    };

                    DrawingCanvas.Children.Add(point1Ellipse);
                    DrawingCanvas.Children.Add(point2Ellipse);
                    DrawingCanvas.Children.Add(point3Ellipse);
                }
            }

            public void DrawRectangle(bool IsSelected, Point P1, Point P2, Point P3, Point P4, double strokeThickness, uint Color, double Angle)
            {
                var figure = new PathFigure
                {
                    StartPoint = new Avalonia.Point(P1.X, P1.Y),
                    Segments = new PathSegments
                    {
                        new LineSegment { Point = new Avalonia.Point(P2.X, P2.Y) },
                        new LineSegment { Point = new Avalonia.Point(P3.X, P3.Y) },
                        new LineSegment { Point = new Avalonia.Point(P4.X, P4.Y) },
                        new LineSegment { Point = new Avalonia.Point(P1.X, P1.Y) }
                    },
                    IsClosed = true
                };

                var geometry = new PathGeometry { Figures = new PathFigures { figure } };

                if (IsSelected)
                {
                    var highlightShape = new Avalonia.Controls.Shapes.Path
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = strokeThickness + 4,
                        Data = geometry
                    };

                    DrawingCanvas.Children.Add(highlightShape);
                }
                var rectangleShape = new Avalonia.Controls.Shapes.Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
                    Fill = new SolidColorBrush(Color),
                    Data = geometry
                };

                DrawingCanvas.Children.Add(rectangleShape);

                if (IsSelected)
                {
                    var points = new[] { P1, P2, P3, P4 };
                    foreach (var point in points)
                    {
                        var cornerEllipse = new Ellipse
                        {
                            Width = 10,
                            Height = 10,
                            Fill = Brushes.Blue,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            Margin = new Thickness(point.X - 5, point.Y - 5, 0, 0)
                        };
                        DrawingCanvas.Children.Add(cornerEllipse);
                    }
                }
            }

        }
        private void Draw(bool IsSelected)
        {
            DrawingCanvas.Children.Clear();
            var figures = _viewModel._figureService.Figures;
            var drawer = new Drawer(DrawingCanvas);
            foreach (var figure in figures)
            {
                if (figure.IsSelected)
                {
                    figure.StrokeThickness = _viewModel.CurrentThickness;
                }
                figure.Draw(drawer, _viewModel.Angle);
            }

            // Отрисовка временных элементов (если идет создание фигуры)
            if (_viewModel.IsDrawingLine || _viewModel.IsDrawingCircle || _viewModel.IsDrawingTriangle || _viewModel.IsDrawingRectangle)
            {
                if (_viewModel.StartPoint != null)
                {
                    // Отрисовка первой точки
                    drawer.DrawTemporaryPoint(_viewModel.StartPoint, Brushes.Red);

                    // Отрисовка второй точки (если есть)
                    if (_viewModel.SecondPoint != null)
                    {
                        drawer.DrawTemporaryPoint(_viewModel.SecondPoint, Brushes.Blue);
                    }

                    // Отрисовка текущей точки (если есть)
                    if (_viewModel.CurrentPoint != null)
                    {
                        if (_viewModel.IsDrawingTriangle || _viewModel.IsDrawingRectangle)
                        {
                            drawer.DrawTemporaryPoint(_viewModel.CurrentPoint, Brushes.Green);
                        }
                        else
                        {
                            drawer.DrawTemporaryPoint(_viewModel.CurrentPoint, Brushes.Blue);
                        }

                        // Отрисовка временной линии для линии и круга
                        if (_viewModel.IsDrawingLine || _viewModel.IsDrawingCircle)
                        {
                            drawer.DrawTemporaryLine(_viewModel.StartPoint, _viewModel.CurrentPoint, Brushes.Gray);
                        }
                    }
                }
            }

            if (_viewModel.IsDrawingReflectionLine && _viewModel.StartPoint != null && _viewModel.CurrentPoint != null)
            {

                drawer.DrawTemporaryPoint(_viewModel.StartPoint, Brushes.Red);
                // Отрисовка временной линии отражения
                drawer.DrawTemporaryLine(_viewModel.StartPoint, _viewModel.CurrentPoint, Brushes.Gray);

                drawer.DrawTemporaryPoint(_viewModel.CurrentPoint, Brushes.Blue);
            }
        }

        private void OpenHelpWindow(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        

        private void OpenDocumentation(object sender, RoutedEventArgs e)
        {
            string projectRoot = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Documentation\");
            string filePath = System.IO.Path.Combine(projectRoot, "Documentation.docx");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии файла: {ex.Message}");
            }
        }
    }
  
}
