using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiTspRouting.WebUI.Class;
using MultiTspRouting.WebUI.Entities;
using MultiTspRouting.WebUI.Models;

namespace MultiTspRouting.WebUI.Controllers
{
    public class HomeController : Controller
    {
        #region DI
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            HomeViewModel homeView = new HomeViewModel();
            homeView.GaEnvironments = new GaEnvironments();

            return View(homeView);
        }

        [HttpPost]
        public IActionResult Index(HomeViewModel homeView, bool withGoogleDistance = false)
        {
            #region Get Coordinates from File, Set and Check First Values
            Tools.Random = new Random();

            OperationModel opModel = new OperationModel();

            using (FileStream fileStream = System.IO.File.Open(Tools.GetFileName(homeView.CoordinateFile, _webHostEnvironment), FileMode.Open, FileAccess.Read))
            {
                homeView.Nodes = Tools.GetNodeListFromExcelFile(fileStream);
            }

            Tools.NodeCount = homeView.Nodes.Count;

            if (Tools.NodeCount < 3)
            {
                return View(homeView);
            }

            Tools.TargetNode = new Node(Tools.NodeCount, Double.Parse(homeView.TargetLat), Double.Parse(homeView.TargetLng));

            Tools.SetVehicleEnvironments(homeView.VehicleMaxCapacity);

            opModel.GaEnvironments = homeView.GaEnvironments;

            SetGoogleMap(homeView);

            if (withGoogleDistance)
            {
                Tools.DistanceMatrix = Tools.GetGoogleDistanceMatrix(homeView.Nodes);
            }
            else
            {
                Tools.DistanceMatrix = Tools.GetEuclideanDistanceMatrix(homeView.Nodes);
            }
            #endregion

            EvolveWithGeneticAlgorithm(opModel, homeView);

            return View(homeView);
        }
        #endregion

        #region EvolveWithGeneticAlgorithm
        private void EvolveWithGeneticAlgorithm(OperationModel opModel, HomeViewModel homeView)
        {
            #region Initialize and Set
            int generation = 1;
            int tempGeneration = 1;
            bool better = true;
            Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            GeneticOperations geneticOperations = new GeneticOperations();

            //geneticOperations.CheckAndSetEnvironments(opModel.GaEnvironments, Tools.Vehicle);
            homeView.GaEnvironments = opModel.GaEnvironments;
            homeView.CostViewModels = new List<CostViewModel>();
            #endregion

            Population population = geneticOperations.RandomizedPopulation(homeView.Nodes.Select(x => x.No).ToArray(), opModel);

            while (true)
            {
                if ((generation - tempGeneration) > opModel.GaEnvironments.IterationNumber *
                    opModel.GaEnvironments.IterationTerminationPercent / 100)
                {
                    break;
                }

                #region Show route values
                if (better || generation == 1 || generation == opModel.GaEnvironments.IterationNumber)
                {
                    if (generation == opModel.GaEnvironments.IterationNumber)
                    {
                        break;
                    }

                    if (better)
                    {
                        better = false;
                    }
                }

                homeView.CostViewModels.Add(new CostViewModel(generation - 1, population.BestCost));
                #endregion

                double tmpMaxFitness = population.BestCost;

                population = geneticOperations.Evolve(population, opModel);

                if (population.BestCost < tmpMaxFitness)
                {
                    tempGeneration = generation;
                    better = true;
                }

                generation++;
            }

            watch.Stop();

            homeView.BestRoute = population.Routes[0];
            homeView.PartialNodes = Tools.GetNodePartsAsListByAddingLastTarget(homeView.BestRoute.NodeArray, homeView.Nodes, Tools.Vehicle.BreakCount);

            homeView.DistanceMatrix = Tools.DistanceMatrix;

            #region Set Statistical Results
            homeView.Results = new Results
            {
                BiggestCost = population.Routes[0].BiggestCost,
                ExecutionTime = String.Format("min:sec:millis  {0:00}:{1:00}.{2}", watch.Elapsed.Minutes, watch.Elapsed.Seconds, watch.Elapsed.Milliseconds),
                LastBestIteration = tempGeneration,
                TotalIteration = generation
            };
            #endregion
        }
        #endregion

        #region SetGoogleMap
        private void SetGoogleMap(HomeViewModel homeView)
        {
            homeView.MapCenter = new Node(0, homeView.Nodes.Sum(x => x.Lat) / homeView.Nodes.Count,
                homeView.Nodes.Sum(x => x.Lng) / homeView.Nodes.Count);

            double latDifference = homeView.Nodes.Max(x => x.Lat) - homeView.Nodes.Min(x => x.Lat);
            double lngDifference = homeView.Nodes.Max(x => x.Lng) - homeView.Nodes.Min(x => x.Lng);

            if (latDifference + lngDifference > 10)
            {
                homeView.MapZoom = 4;
            }
            else if (latDifference + lngDifference > 1)
            {
                homeView.MapZoom = 8;
            }
            else if (latDifference + lngDifference > 0.1)
            {
                homeView.MapZoom = 12;
            }
            else if (latDifference + lngDifference > 0.01)
            {
                homeView.MapZoom = 14;
            }
            else
            {
                homeView.MapZoom = 16;
            }
        }
        #endregion
    }
}
