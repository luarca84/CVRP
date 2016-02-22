using CvrpWPF.Model;
using LiveCharts;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CvrpWPF.ViewModel
{
    class BaseViewModel : INotifyPropertyChanged
    {
        int tamañoPoblacionInicial = 1000;
        int probabilidadCruce = 90;
        int probabilidadMutacion = 80;
        int numeroGeneraciones = 1000;
        SeriesCollection series = new SeriesCollection();

        public int TamañoPoblacionInicial
        {
            get
            {
                return tamañoPoblacionInicial;
            }

            set
            {
                tamañoPoblacionInicial = value;
                RaisePropertyChanged("TamañoPoblacionInicial");
            }
        }

        public int ProbabilidadCruce
        {
            get
            {
                return probabilidadCruce;
            }

            set
            {
                probabilidadCruce = value;
                RaisePropertyChanged("ProbabilidadCruce");
            }
        }

        public int ProbabilidadMutacion
        {
            get
            {
                return probabilidadMutacion;
            }

            set
            {
                probabilidadMutacion = value;
                RaisePropertyChanged("ProbabilidadMutacion");
            }
        }

        public int NumeroGeneraciones
        {
            get
            {
                return numeroGeneraciones;
            }

            set
            {
                numeroGeneraciones = value;
                RaisePropertyChanged("NumeroGeneraciones");
            }
        }

        public SeriesCollection Series
        {
            get
            {
                return series;
            }

            set
            {
                series = value;
                RaisePropertyChanged("Series");
            }
        }




        #region Button Load

        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new CommandHandler(() => MyAction(), CanExecuteAction()));
            }
        }

        

        private bool CanExecuteAction()
        {
            return true;
        }

        public void MyAction()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? b = ofd.ShowDialog();
            if (b == true)
            {
                ModelManager.Instance.LoadFile(ofd.FileName);
            }
        }

        #endregion

        #region Button Run

        private ICommand _clickRunCommand;
        public ICommand ClickRunCommand
        {
            get
            {
                return _clickRunCommand ?? (_clickRunCommand = new CommandHandler(() => MyActionRun(), CanExecuteActionRun()));
            }
        }

        

        private bool CanExecuteActionRun()
        {
            return true;
        }

        public void MyActionRun()
        {
            List<int> lst = ModelManager.Instance.RunGA(TamañoPoblacionInicial, NumeroGeneraciones, ProbabilidadCruce, ProbabilidadMutacion);

            ChartValues<double> cv = new ChartValues<double>();
            foreach (int i in lst)
                cv.Add(i);
            var lineSeries = new LineSeries
            {
                Title = "Min Fitness by generation",
                Values = cv
            };

            //Series.Clear();
            while (series.Count > 0)
                series.RemoveAt(0);

            Series.Add(lineSeries);
            RaisePropertyChanged("Series");
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            var handlers = PropertyChanged;
            if (handlers != null)
            {
                var args = new PropertyChangedEventArgs(property);
                handlers(this, args);
            }
        }
    }

    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
