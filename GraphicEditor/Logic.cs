using System;
using System.Collections.Generic;
using System.Linq;

using DynamicData;

namespace GraphicEditor
{
    public class FigureService: ILogic
    {
        public readonly SourceCache<IFigure,string> _figures = new(fig=>fig.Id); //Все фигуры
        public readonly HashSet<IFigure> _selectedFigures = new(); //Выбранные фигуры

        public IEnumerable<IFigure> Figures => _figures.Items;

        public void AddFigure(IFigure figure)
        {
            if (figure == null) throw new ArgumentNullException(nameof(figure));
            _figures.AddOrUpdate(figure);
        }

        public void RemoveFigure(IFigure figure)// удаление
        {
            if (figure == null) return;
            _figures.Remove(figure);
            _selectedFigures.Remove(figure);
        }

        public IFigure Create(string name, IDictionary<string, Point> parameters, IDictionary<string, double> doubleparameters)
        {
            if (!FigureFabric.AvailableFigures.Contains(name))
            {
                throw new ArgumentException($"Фигура с именем {name} не найдена.", nameof(name));
            }

            var figure = FigureFabric.CreateFigure(name,doubleparameters, parameters);        

            return figure;
        }
        public IFigure CreateDefault(string name)
        {
            if (!FigureFabric.AvailableFigures.Contains(name))
            {
                throw new ArgumentException($"Фигура с именем {name} не найдена.", nameof(name));
            }

            var figure = FigureFabric.CreateFigureDefault(name);

            return figure;
        }

        public IFigure? Find(Point p, double eps)
        {
            return Figures.FirstOrDefault(f => f.IsIn(p, eps));
        }


        public IEnumerable<(string, Type)> GetParameters(string figure)
        {
            throw new NotImplementedException();
        }

        public void Select(IFigure f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            f.IsSelected = true;
            _selectedFigures.Add(f);
        }

        public void UnSelect(IFigure f)
        {
            if (f == null)
            {
                //Снимаем выделение со всех фигур
                foreach (var figure in _selectedFigures)
                {
                    figure.IsSelected = false;
                }
                _selectedFigures.Clear();
            }
            else
            {
                if (_selectedFigures.Contains(f)) //Проверяем что фигура была выделена
                {
                    f.IsSelected = false;
                    _selectedFigures.Remove(f);
                }
            }
        }
    }
}
