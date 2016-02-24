using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvrpWPF.Model
{
    class ModelManager
    {
        static ModelManager instance = new ModelManager();
        FileCvrp filecvrp;
        private ModelManager() { }

        internal static ModelManager Instance
        {
            get
            {
                return instance;
            }

            set
            {
                instance = value;
            }
        }

        internal FileCvrp Filecvrp
        {
            get
            {
                return filecvrp;
            }

            set
            {
                filecvrp = value;
            }
        }

        public void LoadFile(string path)
        {
            Filecvrp = new FileCvrp();
            Filecvrp.LstNodes = new List<Node>();
            bool flagNODE_COORD_SECTION = false;
            bool flagDEMAND_SECTION = false;
            bool flagDEPOT_SECTION = false;

            string filename=Path.GetFileNameWithoutExtension(path);
            string[] aux = filename.Split('k');
            int numRutas = int.Parse(aux[1]);
            Filecvrp.NumRutas = numRutas;

            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("NAME : "))
                    Filecvrp.Name = line.Replace("NAME : ", "");
                if (line.StartsWith("COMMENT : "))
                    Filecvrp.Comment = line.Replace("COMMENT : ", "");
                if (line.StartsWith("TYPE : "))
                    Filecvrp.Type = line.Replace("TYPE : ", "");
                if (line.StartsWith("DIMENSION : "))
                    Filecvrp.Dimension = int.Parse(line.Replace("DIMENSION : ", ""));
                if (line.StartsWith("EDGE_WEIGHT_TYPE : "))
                    Filecvrp.EdgeWeightType = line.Replace("EDGE_WEIGHT_TYPE : ", "");
                if (line.StartsWith("CAPACITY : "))
                    Filecvrp.Capacity = int.Parse(line.Replace("CAPACITY : ", ""));
                if (line.StartsWith("NODE_COORD_SECTION"))
                {
                    flagNODE_COORD_SECTION = true;
                    continue;
                }
                if (line.StartsWith("DEMAND_SECTION"))
                {
                    flagDEMAND_SECTION = true;
                    flagNODE_COORD_SECTION = false;
                    continue;
                }
                if (line.StartsWith("DEPOT_SECTION"))
                {
                    flagDEMAND_SECTION = false;
                    flagDEPOT_SECTION = true;
                    continue;
                }
                if (line.StartsWith("EOF"))
                {
                    flagDEPOT_SECTION = false;
                    continue;
                }
                if (flagNODE_COORD_SECTION)
                {
                    string[] array = line.Split(' ');
                    int id =int.Parse( array[1]);
                    int x = int.Parse(array[2]);
                    int y = int.Parse(array[3]);

                    Node n = new Node();
                    n.Id = id;
                    n.X = x;
                    n.Y = y;
                    Filecvrp.LstNodes.Add(n);
                }
                if (flagDEMAND_SECTION)
                {
                    string[] array = line.Split(' ');
                    int id = int.Parse(array[0]);
                    int demand = int.Parse(array[1]);
                    Node n = Filecvrp.LstNodes.Where(e => e.Id == id).First();
                    n.Demand = demand;
                }
                if (flagDEPOT_SECTION)
                {
                    string[] array = line.Split(' ');
                    int id = int.Parse(array[1]);
                    if (id != -1)
                    {
                        Filecvrp.IdNodeDepot = id;
                    }
                }
            }
        }

        public Solution RunSA(double alpha, double temperature,double epsilon)
        {
            int iteration = -1;

            //the probability
            double proba;
            //double alpha = 0.9999;
            //double temperature = 400.0;
            //double epsilon = 0.0000000001;
            double delta;

            List<double> lstMinFitness = new List<double>();
            int numRutas = Filecvrp.NumRutas;
            List<Node> lstNodes = Filecvrp.LstNodes;
            Node depot = Filecvrp.LstNodes.Where(e => e.Id == Filecvrp.IdNodeDepot).First();
            SASolution currentSolutionSA = new SASolution(numRutas, lstNodes, depot, Filecvrp.Capacity);
            currentSolutionSA.CalculateFitness();
            double distance = currentSolutionSA.Fitness;
            Random r = new Random();

            //while the temperature did not reach epsilon
            while (temperature > epsilon)
            {
                iteration++;

                //get the next random permutation of distances 
                int selectedNode = r.Next(0, lstNodes.Count);
                while(depot.Id == lstNodes[selectedNode].Id)
                    selectedNode = r.Next(0, lstNodes.Count);
                Node n = lstNodes[selectedNode];
                int routeindex = r.Next(0, numRutas);
                SASolution nextSolutionSA = new SASolution(currentSolutionSA, n, routeindex);
                nextSolutionSA.CalculateFitness();
                //compute the distance of the new permuted configuration
                delta = nextSolutionSA.Fitness - distance;
                //if the new distance is better accept it and assign it
                if (delta < 0  && nextSolutionSA.Feasible)
                {
                    currentSolutionSA = nextSolutionSA;
                    distance = delta + distance;
                }
                else
                {
                    proba = r.Next();
                    //if the new distance is worse accept 
                    //it but with a probability level
                    //if the probability is less than 
                    //E to the power -delta/temperature.
                    //otherwise the old value is kept
                    if (proba < Math.Exp(-delta / temperature))
                    {
                        currentSolutionSA = nextSolutionSA;
                        distance = delta + distance;
                    }
                }
                //cooling process on every iteration
                temperature *= alpha;

                //lstMinFitness.Add(currentSolutionSA.Fitness);
            }

            distance = currentSolutionSA.Fitness;

            Solution s = new Solution();
            s.LstMinFitness = lstMinFitness;
            s.BestRutas = currentSolutionSA.Rutas;
            s.FitnessBestRutas = currentSolutionSA.Fitness;
            return s;
        }

        public Solution RunTS(int maxNumIterations)
        {
            List<double> lstMinFitness = new List<double>();
            int maxLengthTabuList = 10;
            int numRutas = Filecvrp.NumRutas;
            List<Node> lstNodes = Filecvrp.LstNodes;
            Node depot = Filecvrp.LstNodes.Where(e => e.Id == Filecvrp.IdNodeDepot).First();
            TabuSolution ts = new TabuSolution(numRutas, lstNodes, depot, Filecvrp.Capacity);
            ts.CalculateFitness();
            TabuSolution tsBestSolution = ts;
            List<Node> tabuList = new List<Node>();
            Random r = new Random();
            for (int i =0 ; i < maxNumIterations; i++)
            {
                TabuSolution mejorsolucioncandidata = null;
                double fitnessmejorsolucioncandidata = double.MaxValue;
                Node selectedNode = null;
                foreach (Node n in lstNodes)
                {
                    if (n.Id != depot.Id && !tabuList.Contains(n))
                    {
                        int routeIndex = r.Next(0, numRutas);
                        TabuSolution solcandidata = new TabuSolution(ts, n, routeIndex);
                        solcandidata.CalculateFitness();
                        if (solcandidata.Feasible && solcandidata.Fitness < fitnessmejorsolucioncandidata)
                        {
                            fitnessmejorsolucioncandidata = solcandidata.Fitness;
                            mejorsolucioncandidata = solcandidata;
                            selectedNode = n;
                        }
                    }
                }

                ts = mejorsolucioncandidata;
                

                if (ts.Fitness < tsBestSolution.Fitness)
                    tsBestSolution = ts;
                lstMinFitness.Add(tsBestSolution.Fitness);

                tabuList.Add(selectedNode);
                if (tabuList.Count > maxLengthTabuList)
                    tabuList.RemoveAt(0);
            }

            Solution s = new Solution();
            s.LstMinFitness = lstMinFitness;
            s.BestRutas = tsBestSolution.Rutas;
            s.FitnessBestRutas = tsBestSolution.Fitness;
            return s;
        }

        public Solution RunGA(int tamañoPoblacionInicial, int numerogeneraciones, int probabilidadCruce, int probabilidadMutacion)
        {
            List<double> lstMinFitness = new List<double>();
            int numRutas = Filecvrp.NumRutas;
            List<Node> lstNodes = Filecvrp.LstNodes;
            Node depot = Filecvrp.LstNodes.Where(e => e.Id == Filecvrp.IdNodeDepot).First();
            int numeroCrucesPorGeneracion = tamañoPoblacionInicial/2;

            Random r = new Random();
            List<Cromosoma> poblacion = new List<Cromosoma>();
            Cromosoma bestCromosoma = null;
            for (int i = 0; i < tamañoPoblacionInicial; i++)
            {
                Cromosoma c = new Cromosoma(numRutas, lstNodes, depot,Filecvrp.Capacity);
                c.CalculateFitness();
                int maxIterations = 1000;
                int iteration = 0;
                while (!c.Feasible && iteration < maxIterations)
                {
                    c = new Cromosoma(numRutas, lstNodes, depot, Filecvrp.Capacity);
                    c.CalculateFitness();
                    iteration++;
                }
                poblacion.Add(c);
            }

            for (int i = 0; i < numerogeneraciones; i++)
            {
                for (int j = 0; j < numeroCrucesPorGeneracion; j++)
                {
                    int indexPadre = r.Next(0, poblacion.Count);
                    int indexMadre = r.Next(0, poblacion.Count);
                    if (r.Next(0, 100) < probabilidadCruce)
                    {
                        Cromosoma h1 = new Cromosoma(poblacion[indexPadre],poblacion[indexMadre], lstNodes);
                        if (r.Next(0, 100) < probabilidadMutacion)
                            h1.Mutar();
                        h1.CalculateFitness();
                        Cromosoma h2 = new Cromosoma(poblacion[indexPadre], poblacion[indexMadre], lstNodes);
                        if (r.Next(0, 100) < probabilidadMutacion)
                            h2.Mutar();
                        h2.CalculateFitness();

                        if (h1.Fitness < poblacion[indexPadre].Fitness  && h1.Feasible)
                            poblacion[indexPadre] = h1;
                        if (h2.Fitness < poblacion[indexMadre].Fitness  && h2.Feasible)
                            poblacion[indexMadre] = h2;

                        
                    }                    
                }
                double minFitness = poblacion.Select(e => e.Fitness).Min();
                Console.WriteLine("Generacion: " + i + " Min Fitness: " + minFitness);
                lstMinFitness.Add(minFitness);
                bestCromosoma = poblacion.Where(e => e.Fitness == minFitness).First();
            }

            Solution s = new Solution();
            s.LstMinFitness = lstMinFitness;
            s.BestRutas = bestCromosoma.Rutas;
            s.FitnessBestRutas = bestCromosoma.Fitness;
            return s;
        }
    }
}
