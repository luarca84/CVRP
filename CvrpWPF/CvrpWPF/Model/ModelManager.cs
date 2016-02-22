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

        public void LoadFile(string path)
        {
            filecvrp = new FileCvrp();
            filecvrp.LstNodes = new List<Node>();
            bool flagNODE_COORD_SECTION = false;
            bool flagDEMAND_SECTION = false;
            bool flagDEPOT_SECTION = false;

            string filename=Path.GetFileNameWithoutExtension(path);
            string[] aux = filename.Split('k');
            int numRutas = int.Parse(aux[1]);
            filecvrp.NumRutas = numRutas;

            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("NAME : "))
                    filecvrp.Name = line.Replace("NAME : ", "");
                if (line.StartsWith("COMMENT : "))
                    filecvrp.Comment = line.Replace("COMMENT : ", "");
                if (line.StartsWith("TYPE : "))
                    filecvrp.Type = line.Replace("TYPE : ", "");
                if (line.StartsWith("DIMENSION : "))
                    filecvrp.Dimension = int.Parse(line.Replace("DIMENSION : ", ""));
                if (line.StartsWith("EDGE_WEIGHT_TYPE : "))
                    filecvrp.EdgeWeightType = line.Replace("EDGE_WEIGHT_TYPE : ", "");
                if (line.StartsWith("CAPACITY : "))
                    filecvrp.Capacity = int.Parse(line.Replace("CAPACITY : ", ""));
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
                    filecvrp.LstNodes.Add(n);
                }
                if (flagDEMAND_SECTION)
                {
                    string[] array = line.Split(' ');
                    int id = int.Parse(array[0]);
                    int demand = int.Parse(array[1]);
                    Node n = filecvrp.LstNodes.Where(e => e.Id == id).First();
                    n.Demand = demand;
                }
                if (flagDEPOT_SECTION)
                {
                    string[] array = line.Split(' ');
                    int id = int.Parse(array[1]);
                    if (id != -1)
                    {
                        filecvrp.IdNodeDepot = id;
                    }
                }
            }
        }


        public List<int> RunGA(int tamañoPoblacionInicial, int numerogeneraciones, int probabilidadCruce, int probabilidadMutacion)
        {
            List<int> lstMinFitness = new List<int>();
            int numRutas = filecvrp.NumRutas;
            List<Node> lstNodes = filecvrp.LstNodes;
            Node depot = filecvrp.LstNodes.Where(e => e.Id == filecvrp.IdNodeDepot).First();

            //int tamañoPoblacionInicial = 1000;
            //int probabilidadCruce = 90;
            //int probabilidadMutacion = 80;
            //int numerogeneraciones = 1000;
            int numeroCrucesPorGeneracion = tamañoPoblacionInicial/2;

            Random r = new Random();
            List<Cromosoma> poblacion = new List<Cromosoma>();
            for (int i = 0; i < tamañoPoblacionInicial; i++)
            {
                Cromosoma c = new Cromosoma(numRutas, lstNodes, depot,filecvrp.Capacity);
                c.CalculateFitness();
                while (!c.Feasible)
                {
                    c = new Cromosoma(numRutas, lstNodes, depot, filecvrp.Capacity);
                    c.CalculateFitness();
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
                int minFitness = poblacion.Select(e => e.Fitness).Min();
                Console.WriteLine("Generacion: " + i + " Min Fitness: " + minFitness);
                lstMinFitness.Add(minFitness);
            }
            return lstMinFitness;
        }
    }
}
