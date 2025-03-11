using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
        private bool _isDragging = false;
        private GraphicEditor.Point _dragStartPoint;
        private IFigure _draggedFigure;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _viewModel = viewModel;
            DataContext = _viewModel;
            this.KeyDown += OnKeyDown;

            thicknessSlider.ValueChanged += ThicknessSlider_ValueChanged;

            _viewModel.FiguresChanged += () =>
            {
                Dispatcher.UIThread.Post(() => Draw(false, thicknessSlider.Value));
            };

             DrawingCanvas.PointerPressed += OnCanvasPointerPressed;
            DrawingCanvas.PointerMoved += OnCanvasPointerMoved;
            DrawingCanvas.PointerReleased += OnCanvasPointerReleased;
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

            // Вызываем метод Draw с новой толщиной
            Draw(_viewModel.SelectedFigure?.IsSelected ?? false, newThickness);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                _viewModel.RemoveSelectedFiguresCommand.Execute().Subscribe();
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

            // Проверяем, была ли нажата фигура
            _draggedFigure = _viewModel.GetFigureAtPoint(point);
            if (_draggedFigure != null)
            {
                _isDragging = true;
                _dragStartPoint = point;
                _viewModel.SelectFigure(_draggedFigure);
            }
            else
            {
                _viewModel.HandleCanvasClick(point);
            }
        }
        private void OnCanvasPointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging && _draggedFigure != null)
            {
                var avaloniaPoint = e.GetPosition(DrawingCanvas);
                var point = new GraphicEditor.Point (avaloniaPoint.X, avaloniaPoint.Y);

                var vector = new GraphicEditor.Point(point.X - _dragStartPoint.X, point.Y - _dragStartPoint.Y);
                _viewModel.SelectedFigure.Move(vector);

                // Обновляем начальную точку для следующего перемещения
                _dragStartPoint = point;

                // Перерисовываем холст
                Draw(_viewModel.SelectedFigure?.IsSelected ?? false, thicknessSlider.Value);
            }
            else
            {
                var avaloniaPoint = e.GetPosition(DrawingCanvas);
                var point = new GraphicEditor.Point (avaloniaPoint.X, avaloniaPoint.Y);
                _viewModel.HandleCanvasMove(point);
                Draw(false, thicknessSlider.Value); // Перерисовываем canvas
            }
        }
        private void OnCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                
                _draggedFigure = null;
            }
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
                    StrokeThickness = 1,
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

                var lineShape = new Path
                {
                    Stroke = strokeBrush,
                    StrokeThickness = 2,
                    Data = lineGeometry
                };

                DrawingCanvas.Children.Add(lineShape);
            }
            public void DrawLine(bool IsSelected, Point Start,Point End, double strokeThickness, int Color)
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

                var lineShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
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

            public void DrawCircle( bool IsSelected, Point Center, double radius, Point PointOnCircle, double strokeThickness, int Color)
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

                var circleShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
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

            public void DrawTriangle(bool IsSelected, Point Point1, Point Point2, Point Point3, double strokeThickness, int Color)
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

                var triangleShape = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness,
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

            public void DrawRectangle(bool IsSelected, Point TopLeft, Point BottomRight, double strokeThickness, int Color)
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
                        StrokeThickness = strokeThickness + 4,
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
                    StrokeThickness = strokeThickness,
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
        private void Draw(bool IsSelected, double strokeThickness)
        {
            DrawingCanvas.Children.Clear();
            var figures = _viewModel._figureService.Figures;
            var drawer = new Drawer(DrawingCanvas);
            foreach (var figure in figures)
            {
                if (figure.IsSelected)
                {
                    figure.StrokeThickness = strokeThickness;
                }
                figure.Draw(drawer);
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
        }

    }
  
}
