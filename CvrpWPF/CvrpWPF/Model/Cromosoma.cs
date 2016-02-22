using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvrpWPF.Model
{
    class Cromosoma
    {
        List<List<Node>> rutas;
        Node depot;
        int fitness = 0;
        int capacidad = 0;
        bool feasible = false;

        public Cromosoma(int numRutas, List<Node> lstNodes, Node depot,int capacidad)
        {
            rutas = new List<List<Node>>();
            for (int i = 0; i < numRutas; i++)
                rutas.Add(new List<Node>());

            Random r = new Random();
            foreach (Node n in lstNodes)
            {
                if (n.Id != depot.Id)
                {
                    int ruta = r.Next(0, numRutas);
                    rutas[ruta].Add(n);
                }
            }

            this.depot = depot;
            this.capacidad = capacidad;
        }

        public Cromosoma(Cromosoma padre, Cromosoma madre, List<Node> lstNodes)
        {
            this.capacidad = padre.capacidad;
            this.depot = padre.depot;
            this.rutas = new List<List<Node>>();
            for (int i = 0; i < padre.rutas.Count; i++)
                rutas.Add(new List<Node>());

            Random r = new Random();
            foreach (Node n in lstNodes)
            {
                if (n.Id != depot.Id)
                {
                    int progenitor = r.Next(0, 2);
                    int ruta = 0;
                    if (progenitor == 0)
                    {
                        ruta = GetIdRuta(padre, n);
                    }
                    else
                    {
                        ruta = GetIdRuta(madre, n);
                    }
                    rutas[ruta].Add(n);
                }
            }

        }

        private int GetIdRuta(Cromosoma c, Node n)
        {
            for (int i = 0; i < c.rutas.Count; i++)
                for (int j = 0; j < c.rutas[i].Count; j++)
                    if (c.rutas[i][j].Id == n.Id)
                        return i;
            return 0;
        }

        public void CalculateFitness()
        {
            int f = 0;
            foreach (List<Node> ruta in rutas)
            {
                f += CalcularDistanciaRuta(ruta);
            }
            Fitness= f;

            feasible = CheckTodasLasRutasSonFactibles();
        }

        private bool CheckTodasLasRutasSonFactibles()
        {
            foreach (List<Node> ruta in rutas)
            {
                int sumDemand = 0;
                foreach (Node n in ruta)
                    sumDemand += n.Demand;
                if (sumDemand > capacidad)
                    return false;
            }
            return true;
        }

        private int CalcularDistanciaRuta(List<Node> ruta)
        {
            int sum = 0;
            if (ruta.Count > 0)
            {
                sum += CalcularDistanciaEntreDosPuntos(depot,ruta[0]);
                for (int i = 0; i < ruta.Count - 1; i++)
                    sum += CalcularDistanciaEntreDosPuntos(ruta[i], ruta[i+1]);
                sum += CalcularDistanciaEntreDosPuntos(ruta[ruta.Count-1],depot);
            }
            return sum;
        }

        private int CalcularDistanciaEntreDosPuntos(Node a, Node b)
        {
            int distancia = (int)Math.Sqrt(Math.Pow(a.X-b.X,2)+Math.Pow(a.Y-b.Y,2));
            return distancia;
        }

        internal List<List<Node>> Rutas
        {
            get
            {
                return rutas;
            }

            set
            {
                rutas = value;
            }
        }

        public int Fitness
        {
            get
            {
                return fitness;
            }

            set
            {
                fitness = value;
            }
        }

        public bool Feasible
        {
            get
            {
                return feasible;
            }

            set
            {
                feasible = value;
            }
        }

        internal void Mutar()
        {
            Random r = new Random();
            for (int i = 0; i < 5; i++)
            {
                int route = r.Next(0, rutas.Count);
                if (rutas[route].Count >= 2)
                {
                    int a = r.Next(0, rutas[route].Count);
                    int b = r.Next(0, rutas[route].Count);
                    Node aux = rutas[route][a];
                    rutas[route][a] = rutas[route][b];
                    rutas[route][b] = aux;
                }
            }
        }
    }
}
