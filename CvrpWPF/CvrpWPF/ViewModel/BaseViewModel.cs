﻿using CvrpWPF.Model;
using CvrpWPF.View;
using GraphX.Controls;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.PCL.Logic.Models;
using LiveCharts;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CvrpWPF.ViewModel
{
    class BaseViewModel : BindableBase
    {
        int tamañoPoblacionInicial = 1000;
        int probabilidadCruce = 90;
        int probabilidadMutacion = 80;
        int numeroGeneraciones = 1000;
        SeriesCollection series = new SeriesCollection();
        string textSolution = string.Empty;
        GXLogicCoreExample logicCoreSolution = null;

        string information_Title = string.Empty;
        string information_Comment = string.Empty;
        string information_Type = string.Empty;
        string information_Dimension = string.Empty;
        string information_EDGE_WEIGHT_TYPE = string.Empty;
        string information_Capacity = string.Empty;

        int maxNumIterationsTabu = 1000;

        double alpha = 0.9999;
        double temperature = 400.0;
        double epsilon = 0.0000000001;


        public double Alpha
        {
            get
            {
                return alpha;
            }

            set
            {
                SetProperty(ref alpha, value);
            }
        }

        public double Temperature
        {
            get
            {
                return temperature;
            }

            set
            {
                SetProperty(ref temperature, value);
            }
        }

        public double Epsilon
        {
            get
            {
                return epsilon;
            }

            set
            {
                SetProperty(ref epsilon, value);
            }
        }

        public int TamañoPoblacionInicial
        {
            get
            {
                return tamañoPoblacionInicial;
            }

            set
            {
                SetProperty(ref tamañoPoblacionInicial, value);
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
                SetProperty(ref probabilidadCruce, value);
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
                SetProperty(ref probabilidadMutacion, value);
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
                SetProperty(ref numeroGeneraciones, value);
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
                SetProperty(ref series, value);
            }
        }

        public string TextSolution
        {
            get
            {
                return textSolution;
            }

            set
            {
                SetProperty(ref textSolution, value);
            }
        }

        public GXLogicCoreExample LogicCoreSolution
        {
            get
            {
                return logicCoreSolution;
            }

            set
            {
                SetProperty(ref logicCoreSolution, value);
            }
        }

        public string Information_Title
        {
            get
            {
                return information_Title;
            }

            set
            {
                SetProperty(ref information_Title, value);
            }
        }

        public string Information_Comment
        {
            get
            {
                return information_Comment;
            }

            set
            {
                SetProperty(ref information_Comment, value);
            }
        }

        public string Information_Type
        {
            get
            {
                return information_Type;
            }

            set
            {
                SetProperty(ref information_Type, value);
            }
        }

        public string Information_Dimension
        {
            get
            {
                return information_Dimension;
            }

            set
            {
                SetProperty(ref information_Dimension, value);
            }
        }

        public string Information_EDGE_WEIGHT_TYPE
        {
            get
            {
                return information_EDGE_WEIGHT_TYPE;
            }

            set
            {
                SetProperty(ref information_EDGE_WEIGHT_TYPE, value);
            }
        }

        public string Information_Capacity
        {
            get
            {
                return information_Capacity;
            }

            set
            {
                SetProperty(ref information_Capacity, value);
            }
        }

        public int MaxNumIterationsTabu
        {
            get
            {
                return maxNumIterationsTabu;
            }

            set
            {
                SetProperty(ref maxNumIterationsTabu, value);
            }
        }




        #region Button Load

        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new DelegateCommand( MyAction, CanExecuteAction));
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
                ((DelegateCommand)ClickRunCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)ClickRunTabuCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)ClickRunSACommand).RaiseCanExecuteChanged();

                Information_Title = ModelManager.Instance.Filecvrp.Name;
                Information_Capacity = ModelManager.Instance.Filecvrp.Capacity.ToString();
                Information_Comment = ModelManager.Instance.Filecvrp.Comment;
                Information_Dimension = ModelManager.Instance.Filecvrp.Dimension.ToString();
                Information_EDGE_WEIGHT_TYPE = ModelManager.Instance.Filecvrp.EdgeWeightType;
                Information_Type = ModelManager.Instance.Filecvrp.Type;
            }
        }


        #endregion

        #region Button Run

        private ICommand _clickRunCommand;
        public ICommand ClickRunCommand
        {
            get
            {
                return _clickRunCommand ?? (_clickRunCommand = new DelegateCommand(MyActionRun, CanExecuteActionRun));
            }
        }
              

        private bool CanExecuteActionRun()
        {
            return ModelManager.Instance.Filecvrp != null;
        }

        public void MyActionRun()
        {
            Solution s = ModelManager.Instance.RunGA(TamañoPoblacionInicial, NumeroGeneraciones, ProbabilidadCruce, ProbabilidadMutacion);
            UpdateTabChart(s);
            UpdateTabSolution(s);
            UpdateTabGraphX(s);   
        }

        private void UpdateTabGraphX(Solution s)
        {

            //Create data graph object
            var graph = new GraphExample();

            //Create and add vertices using some DataSource for ID's
            foreach (Node n in ModelManager.Instance.Filecvrp.LstNodes)
            {
                graph.AddVertex(new DataVertex() { ID = n.Id, Text = n.Id.ToString() });
                
            }
            foreach (List<Node> ruta in s.BestRutas)
            {
                if (ruta.Count > 0)
                {
                    DataVertex origen = graph.Vertices.ToList()[ModelManager.Instance.Filecvrp.IdNodeDepot - 1];
                    DataVertex destino = graph.Vertices.ToList()[ruta[0].Id - 1];
                    graph.AddEdge(new DataEdge(origen, destino));
                    for (int i = 0; i < ruta.Count - 1; i++)
                    {
                        origen = graph.Vertices.ToList()[ruta[i].Id - 1];
                        destino = graph.Vertices.ToList()[ruta[i + 1].Id - 1];
                        graph.AddEdge(new DataEdge(origen, destino));
                    }

                    origen = graph.Vertices.ToList()[ruta[ruta.Count - 1].Id - 1];
                    destino = graph.Vertices.ToList()[ModelManager.Instance.Filecvrp.IdNodeDepot - 1];
                    graph.AddEdge(new DataEdge(origen, destino));
                }
            }

            var LogicCore = new GXLogicCoreExample();
            LogicCore.Graph = graph;
            //This property sets layout algorithm that will be used to calculate vertices positions
            //Different algorithms uses different values and some of them uses edge Weight property.
            LogicCore.DefaultLayoutAlgorithm = GraphX.PCL.Common.Enums.LayoutAlgorithmTypeEnum.KK;
            //Now we can set optional parameters using AlgorithmFactory
            //NOTE: default parameters can be automatically created each time you change Default algorithms
            LogicCore.DefaultLayoutAlgorithmParams =
                               LogicCore.AlgorithmFactory.CreateLayoutParameters(GraphX.PCL.Common.Enums.LayoutAlgorithmTypeEnum.KK);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            ((KKLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            LogicCore.DefaultOverlapRemovalAlgorithm = GraphX.PCL.Common.Enums.OverlapRemovalAlgorithmTypeEnum.FSA;
            //Setup optional params
            LogicCore.DefaultOverlapRemovalAlgorithmParams =
                              LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.PCL.Common.Enums.OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            LogicCore.DefaultEdgeRoutingAlgorithm = GraphX.PCL.Common.Enums.EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            LogicCore.AsyncAlgorithmCompute = false;

            LogicCoreSolution = LogicCore;
        }

        private void UpdateTabSolution(Solution s)
        {
            TextSolution = string.Empty;
            int x = 0;
            foreach (List<Node> ruta in s.BestRutas)
            {
                string line = "ruta " + x + ":";
                x++;
                foreach (Node n in ruta)
                    line += " " + n.Id;
                line += "\n";
                TextSolution += line;
            }
            TextSolution += "Cost " + s.FitnessBestRutas;
        }

        private void UpdateTabChart(Solution s)
        {
            ChartValues<double> cv = new ChartValues<double>();
            foreach (int i in s.LstMinFitness)
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
        }

        #endregion


        #region Button Run Tabu

        private ICommand _clickRunTabuCommand;
        public ICommand ClickRunTabuCommand
        {
            get
            {
                return _clickRunTabuCommand ?? (_clickRunTabuCommand = new DelegateCommand(MyActionRunTabu, CanExecuteActionRunTabu));
            }
        }
        

        private bool CanExecuteActionRunTabu()
        {
            return ModelManager.Instance.Filecvrp != null;
        }

        public void MyActionRunTabu()
        {
            Solution s = ModelManager.Instance.RunTS(MaxNumIterationsTabu);
            UpdateTabChart(s);
            UpdateTabSolution(s);
            UpdateTabGraphX(s);
        }

        #endregion


        #region Button Run SA

        private ICommand _clickRunSACommand;
        public ICommand ClickRunSACommand
        {
            get
            {
                return _clickRunSACommand ?? (_clickRunSACommand = new DelegateCommand(MyActionRunSA, CanExecuteActionRunSA));
            }
        }        

        private bool CanExecuteActionRunSA()
        {
            return ModelManager.Instance.Filecvrp != null;
        }

        public void MyActionRunSA()
        {
            Solution s = ModelManager.Instance.RunSA(Alpha,Temperature,Epsilon);
            UpdateTabChart(s);
            UpdateTabSolution(s);
            UpdateTabGraphX(s);
        }

        #endregion

    }




    //Layout visual class
    public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> { }

    //Graph data class
    public class GraphExample : BidirectionalGraph<DataVertex, DataEdge> { }

    //Logic core class
    public class GXLogicCoreExample : GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> { }

    //Vertex data object
    public class DataVertex : VertexBase
    {
        /// <summary>
        /// Some string property for example purposes
        /// </summary>
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    //Edge data object
    public class DataEdge : EdgeBase<DataVertex>
    {
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public DataEdge()
            : base(null, null, 1)
        {
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

}
