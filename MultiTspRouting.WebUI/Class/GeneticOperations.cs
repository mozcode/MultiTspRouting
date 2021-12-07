using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MultiTspRouting.WebUI.Entities;
using MultiTspRouting.WebUI.Models;

namespace MultiTspRouting.WebUI.Class
{
    public class GeneticOperations
    {
        #region RandomizedPopulation
        public Population RandomizedPopulation(int[] nodes, OperationModel opModel)
        {
            List<Route> routes = new List<Route>();

            for (int i = 0; i < opModel.GaEnvironments.PopulationSize;)
            {
                Route newRoute = new Route(Tools.GetNodeArrayByAddingStartPointAndBreakPoints(Shuffle(nodes), Tools.Vehicle), Tools.Vehicle.BreakCount);

                if (!routes.Exists(x => x.BiggestCost == newRoute.BiggestCost))
                {
                    routes.Add(newRoute);

                    i++;
                }
            }
            return new Population(routes);
        }

        #region Shuffle
        public int[] Shuffle(int[] nodes)
        {
            int[] newNodes = (int[])nodes.Clone();

            for (int i = 1; i < Tools.NodeCount; i++)
            {
                int rnd = Tools.Random.Next(0, i);
                int tmpNode = newNodes[rnd];
                newNodes[rnd] = newNodes[i];
                newNodes[i] = tmpNode;
            }

            return newNodes;
        }
        #endregion
        #endregion

        #region Evolve
        public Population Evolve(Population population, OperationModel opModel)
        {
            var routes2 = population.Routes.Select(x => x.NodeArray).ToList();

            List<Route> routes = new List<Route>(Elite(population, opModel.GaEnvironments.Elitism).Routes);

            for (int i = 0; i < opModel.GaEnvironments.PopulationSize - opModel.GaEnvironments.Elitism;)
            {
                Route childRoute;
                int childRouteMax = 0;
                int childRouteMin = 0;

                do
                {
                    int parent1 = TournamentSelection(population, opModel);
                    int parent2 = TournamentSelection(population, opModel);

                    while (parent1 == parent2 && opModel.GaEnvironments.PopulationSize > 3)
                    {
                        parent2 = TournamentSelection(population, opModel);
                    }
                    childRoute = OrderCrossover(population.Routes[parent1].NodeArray, population.Routes[parent2].NodeArray, opModel);

                    List<int> childRouteLengths = Tools.GetNodePartsAsLengthArrayModel(childRoute.NodeArray, Tools.Vehicle.BreakCount).Select(x => x.Length).ToList();
                    childRouteMax = childRouteLengths.Max() - 1;
                    childRouteMin = childRouteLengths.Min() - 1;

                } while (childRouteMin < Tools.Vehicle.MinCapacity || childRouteMax > Tools.Vehicle.MaxCapacity);

                if (Tools.Random.NextDouble() < opModel.GaEnvironments.MutationRate)
                {
                    childRoute = Mutate(childRoute.NodeArray, opModel);
                }

                if (Tools.Random.NextDouble() < opModel.GaEnvironments.TwoOptRate)
                {
                    childRoute = TwoOpt(childRoute, opModel);
                }

                if (!routes.Exists(x => x.BiggestCost == childRoute.BiggestCost))
                {
                    routes.Add(childRoute);

                    i++;
                }
            }
            return new Population(routes);
        }
        #endregion

        #region Elite
        Population Elite(Population population, int elitism)
        {
            return new Population(population.Routes.Take(elitism).ToList());
        }
        #endregion

        #region Selection
        int TournamentSelection(Population population, OperationModel opModel)
        {
            int[] r = Tools.CreateDifferentRandomIntegers(0, opModel.GaEnvironments.PopulationSize - 1, opModel.GaEnvironments.RandomIntegerCount);

            int selectedIndex = r[0];

            for (var i = 1; i < r.Length; i++)
            {
                if (population.Routes[r[i]].BiggestCost < population.Routes[selectedIndex].BiggestCost)
                {
                    selectedIndex = r[i];
                }
            }

            return selectedIndex;
        }
        #endregion

        #region Crossover
        Route OrderCrossover(int[] parent1, int[] parent2, OperationModel opModel)
        {
            int[] rndPart = Tools.CreateTwoRandomIntegers(1, Tools.Vehicle.BreakCount + 1);

            LengthArrayModel p1Part = Tools.GetNodePartAndLengthByPartNumber(parent1, rndPart[0], Tools.Vehicle.BreakCount);
            LengthArrayModel p2Part = Tools.GetNodePartAndLengthByPartNumber(parent2, rndPart[1], Tools.Vehicle.BreakCount);

            int[] rndInterval = Tools.CreateTwoDifferentRandomIntegers(1, p1Part.Length < p2Part.Length ? p1Part.Length : p2Part.Length);

            Array.Sort(rndInterval);

            List<int> midPart = p1Part.PartArray.Skip(rndInterval[0]).Take(rndInterval[1] - rndInterval[0] + 1).ToList();

            List<int> p2Rest = p2Part.PartArray.TakeLast(p2Part.Length - rndInterval[1] - 1).Concat(p2Part.PartArray.Skip(1).Take(rndInterval[1]))
                .Except(midPart).ToList();

            List<int> newPart = new List<int> { Tools.NodeCount };
            newPart.AddRange(p2Rest.Take(rndInterval[0] - 1).ToList());
            newPart.AddRange(midPart);
            newPart.AddRange(p2Rest.Except(newPart));

            Route route;

            if (Tools.Vehicle.BreakCount == 0)
            {
                route = new Route(newPart.ToArray(), Tools.Vehicle.BreakCount);
            }
            else
            {
                var parent2Cleaned = Tools.GetNodePartsAsList(parent2, Tools.Vehicle.BreakCount);
                var newParent2 = new List<List<int>>();

                for (var i = 0; i < parent2Cleaned.Count; i++)
                {
                    if (i + 1 == rndPart[1])
                    {
                        newParent2.Add(newPart);
                    }
                    else
                    {
                        newParent2.Add(parent2Cleaned[i].Except(midPart).ToList());
                    }
                }

                route = new Route(Tools.ConvertListArrayToNodeArray(newParent2, Tools.Vehicle.BreakCount), Tools.Vehicle.BreakCount);
            }

            return route;
        }
        #endregion

        #region Mutate
        Route Mutate(int[] nodeArray, OperationModel opModel)
        {
            int[] rndPart = Tools.CreateTwoRandomIntegers(1, Tools.Vehicle.BreakCount + 1);

            LengthArrayModel part1 = Tools.GetNodePartAndLengthByPartNumber(nodeArray, rndPart[0], Tools.Vehicle.BreakCount);
            LengthArrayModel part2 = Tools.GetNodePartAndLengthByPartNumber(nodeArray, rndPart[1], Tools.Vehicle.BreakCount);

            int rnd1 = Tools.Random.Next(1, part1.Length);
            int rnd2 = Tools.Random.Next(1, part2.Length);

            int index1 = Array.IndexOf(nodeArray, part1.PartArray[rnd1]);
            int index2 = Array.IndexOf(nodeArray, part2.PartArray[rnd2]);

            int tmpNode = nodeArray[index1];
            nodeArray[index1] = nodeArray[index2];
            nodeArray[index2] = tmpNode;

            Route newRoute = new Route(nodeArray, Tools.Vehicle.BreakCount);

            return newRoute;
        }
        #endregion

        #region TwoOpt
        public Route TwoOpt(Route route, OperationModel opModel)
        {
            List<LengthArrayModel> partModels = Tools.GetNodePartsAsLengthArrayModel(route.NodeArray, Tools.Vehicle.BreakCount);

            foreach (var part in partModels)
            {
                part.PartArray = TwoOptForPart(part);
            }

            Route newRoute = new Route(Tools.ConvertLengthArrayModelToNodeArray(partModels, Tools.Vehicle.BreakCount), Tools.Vehicle.BreakCount);
            
            return newRoute;
        }

        public int[] TwoOptForPart(LengthArrayModel partModel)
        {
            double cost = Tools.CalculatePartialArrayCost(partModel.PartArray);

            for (int i = 1; i < partModel.Length - 1; i++)
            {
                for (int k = i + 1; k < partModel.Length; k++)
                {
                    var newPart = TwoOptSwap(partModel.PartArray, i, k);

                    var newCost = Tools.CalculatePartialArrayCost(newPart);

                    if (newCost < cost)
                    {
                        partModel.PartArray = newPart;
                    }
                }
            }

            #region TwoOptSwap
            int[] TwoOptSwap(int[] myNodes, int i, int k)
            {
                int[] swappedNodes = new int[partModel.Length];

                Array.Copy(myNodes, swappedNodes, partModel.Length);
                Array.Reverse(swappedNodes, i, k - i + 1);

                return swappedNodes;
            }
            #endregion

            return partModel.PartArray;
        }
        #endregion

        #region CheckAndSetEnvironments
        public void CheckAndSetEnvironments(GaEnvironments environments, Vehicle vehicle)
        {
            if (environments.PopulationSize > (Tools.Vehicle.MaxCapacity + Tools.Vehicle.MinCapacity) * 4)
            {
                environments.PopulationSize = (Tools.Vehicle.MaxCapacity + Tools.Vehicle.MinCapacity) * 4;
            }

            if (environments.IterationNumber > environments.PopulationSize * 5)
            {
                environments.IterationNumber = environments.PopulationSize * 5;
            }

            while (environments.Elitism > 1 && environments.Elitism * 5 >= environments.PopulationSize)
            {
                environments.Elitism /= 2;
            }

            if (environments.RandomIntegerCount * 2 > environments.PopulationSize)
            {
                environments.RandomIntegerCount = environments.PopulationSize < 4 ? 1 : 2;
            }
        }
        #endregion
    }
}
