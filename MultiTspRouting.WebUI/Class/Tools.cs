using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ExcelDataReader;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MultiTspRouting.WebUI.Entities;
using MultiTspRouting.WebUI.Models;

namespace MultiTspRouting.WebUI.Class
{
    public class Tools
    {
        public static Random Random { get; set; }

        public static double[,] DistanceMatrix { get; set; }

        public static Node TargetNode { get; set; }

        public static int NodeCount { get; set; }

        public static int NodeStartPointCount { get; set; }

        public static int NodeWholeCount { get; set; }

        public static int[] StartBreaks { get; set; }

        public static Vehicle Vehicle { get; set; }

        #region GetNodeListFromTextFile
        public static List<Node> GetNodeListFromTextFile(string filePath = "")
        {
            if (filePath == "")
            {
                filePath = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "Files")).FirstOrDefault();
            }

            List<Node> nodes = new List<Node>();

            string[] lines = File.ReadAllLines(filePath);

            if (lines != null)
            {
                int number = 0;

                NumberFormatInfo numberFormat = new NumberFormatInfo { NegativeSign = "-", NumberDecimalSeparator = "." };
                NumberStyles numberStyles = NumberStyles.AllowTrailingSign | NumberStyles.Float | NumberStyles.AllowDecimalPoint;

                foreach (string line in lines)
                {
                    try
                    {
                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Any())
                        {
                            if (parts[0] == "EOF")
                            {
                                break;
                            }

                            if (Int32.TryParse(parts[0], out int _))
                            {
                                if (number + 1 == Convert.ToInt32(parts[0]))
                                {
                                    nodes.Add(new Node(number,
                                        Double.Parse(parts[1], numberStyles, numberFormat),
                                        Double.Parse(parts[2], numberStyles, numberFormat)));

                                    number++;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }

                //If nodes count is more than 1, than accept it and add first node value as new last node value.
                //Else do not accept and clear nodes
                if (number > 1)
                {
                    //Add first node to end cause of back to starting point
                    //nodes.Add(new Node(nodes.Count, nodes.First().X, nodes.First().Y));
                }
                else
                {
                    nodes.Clear();
                }
            }

            return nodes;
        }

        internal static void SetVehicleEnvironments(int vehicleMaxCapacity)
        {
            Vehicle = new Vehicle(NodeCount, vehicleMaxCapacity);
            StartBreaks = CreateRandomStartBreaks(Vehicle);
            NodeStartPointCount = NodeCount + Vehicle.BreakCount + 1;
            NodeWholeCount = NodeStartPointCount + Vehicle.BreakCount;
        }
        #endregion

        #region GetNodeListFromExcelFile
        public static List<Node> GetNodeListFromExcelFile(FileStream fileStream)
        {
            List<Node> nodes = new List<Node>();

            if (fileStream == null)
            {
                return nodes;
            }

            string[] lines = new string[] { };

            if (fileStream.Name.EndsWith(".txt"))
            {
                lines = File.ReadAllLines(fileStream.Name);
            }
            else if (fileStream.Name.EndsWith(".xls") || fileStream.Name.EndsWith(".xlsx"))
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    List<string> fileContent = new List<string>();

                    while (reader.Read()) //Each row of the file
                    {
                        fileContent.Add(String.Concat(reader.GetValue(0), " ", reader.GetValue(1), " ", reader.GetValue(2)));
                    }

                    lines = fileContent.ToArray();
                }
            }

            if (lines != null)
            {
                int number = 0;

                NumberFormatInfo numberFormat = new NumberFormatInfo { NegativeSign = "-", NumberDecimalSeparator = "." };
                NumberStyles numberStyles = NumberStyles.AllowTrailingSign | NumberStyles.Float | NumberStyles.AllowDecimalPoint;

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Any())
                    {
                        if (parts[0] == "EOF")
                        {
                            break;
                        }

                        if (Int32.TryParse(parts[0], out int _))
                        {
                            if (number + 1 == Convert.ToInt32(parts[0]))
                            {
                                nodes.Add(new Node(number,
                                    Double.Parse(parts[1], numberStyles, numberFormat),
                                    Double.Parse(parts[2], numberStyles, numberFormat)));

                                number++;
                            }
                        }
                    }
                }

                //If nodes count is more than 1, than accept it and add first node value as new last node value.
                //Else do not accept and clear nodes
                if (number > 1)
                {
                    //Add first node to end cause of back to starting point
                    //nodes.Add(new Node(nodes.Count, nodes.First().X, nodes.First().Y));
                }
                else
                {
                    nodes.Clear();
                }
            }

            return nodes;
        }
        #endregion

        #region GetFileName
        public static string GetFileName(IFormFile coordinateFile, IWebHostEnvironment webHostEnvironment)
        {
            string fileName;

            string filePath = Path.Combine(webHostEnvironment.WebRootPath, "CoordinateFiles\\");

            if (coordinateFile != null)
            {
                fileName = Path.Combine(filePath, coordinateFile.FileName);

                //Delete if same named file exist
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                //Save file
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    coordinateFile.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
            else
            {
                fileName = Path.Combine(filePath, "default.xlsx");
            }

            return fileName;
        }
        #endregion

        #region Create random integer
        public static int[] CreateTwoDifferentRandomIntegers(int startIndex, int endIndex)
        {
            int[] r = new int[2];

            r[0] = Random.Next(startIndex, endIndex);

            do
            {
                r[1] = Random.Next(startIndex, endIndex);
            } while (r[0] == r[1]);

            return r;
        }
        public static int[] CreateTwoRandomIntegers(int startIndex, int endIndex)
        {
            int[] r = new int[2];

            r[0] = Random.Next(startIndex, endIndex);
            r[1] = Random.Next(startIndex, endIndex);

            return r;
        }

        public static int[] CreateDifferentRandomIntegers(int startIndex, int endIndex, int arrayLength)
        {
            #region HasDuplicate Function
            bool HasDuplicate(int[] numbers)
            {
                for (int i = 0; i < numbers.Length; i++)
                {
                    for (int j = i + 1; j < numbers.Length; j++)
                    {
                        if (numbers[i] == numbers[j])
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            #endregion

            int[] r = new int[arrayLength];

            r[0] = Random.Next(startIndex, endIndex);

            do
            {
                for (int i = 1; i < arrayLength; i++)
                {
                    r[i] = Random.Next(startIndex, endIndex);
                }
            } while (HasDuplicate(r));

            return r;
        }
        #endregion

        #region GetGoogleDistanceMatrix
        public static double[,] GetGoogleDistanceMatrix(List<Node> nodes)
        {
            List<Node> nodesWithTarget = new List<Node>();
            nodesWithTarget.AddRange(nodes);
            nodesWithTarget.Add(TargetNode);

            Location[] locations = (from n in nodesWithTarget select new Location(n.Lat, n.Lng)).ToArray();

            double[,] distanceMatrix = new double[NodeCount + 1, NodeCount + 1];

            for (int i = 0; i < NodeCount + 1; i++)
            {
                int j = 0;

                do
                {
                    var request = new DistanceMatrixRequest
                    {
                        Key = "AIzaSyAo-B0aWdLTIkk49iS8W7WjTL5nTkfIa_c",
                        OriginsRaw = locations[i].Latitude.ToString().Replace(',', '.') + "," + locations[i].Longitude.ToString().Replace(',', '.'),
                        Destinations = locations.Skip(j).Take(25),
                        TrafficModel = TrafficModel.Best_Guess,
                        DepartureTime = DateTime.Now
                    };

                    var response = GoogleMaps.DistanceMatrix.Query(request);

                    foreach (var row in response.Rows)
                    {
                        foreach (var element in row.Elements)
                        {
                            distanceMatrix[i, j] = element.Distance.Value;

                            j++;
                        }
                    }
                } while (j < NodeCount);
            }

            return distanceMatrix;
        }
        #endregion

        #region GetEuclideanDistanceMatrix
        public static double[,] GetEuclideanDistanceMatrix(List<Node> nodes)
        {
            List<Node> nodesWithTarget = new List<Node>();
            nodesWithTarget.AddRange(nodes);
            nodesWithTarget.Add(TargetNode);

            double[,] distanceMatrix = new double[NodeCount + 1, NodeCount + 1];

            for (var i = 0; i < NodeCount + 1; i++)
            {
                for (int j = 0; j < NodeCount + 1; j++)
                {
                    distanceMatrix[i, j] = CalculateEuclideanDistance(nodesWithTarget[i].Lat, nodesWithTarget[j].Lat, nodesWithTarget[i].Lng, nodesWithTarget[j].Lng);
                }
            }

            return distanceMatrix;
        }

        public static double CalculateEuclideanDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }
        #endregion

        #region CreateRandomArrayByVehicleEnvironments
        public static int[] CreateRandomStartBreaks(Vehicle vehicle)
        {
            int[] breakArray;

            #region do not increase vehicle count even if full capacity
            breakArray = new int[vehicle.BreakCount];

            Vehicle newVehicle = (Vehicle)vehicle.Clone();
            int newNodeCount = Tools.NodeCount;

            int tempBreak = 0;
            for (int i = 0; i < vehicle.BreakCount; i++)
            {
                breakArray[i] = tempBreak + Tools.Random.Next(newVehicle.MinCapacity, newVehicle.MaxCapacity);

                tempBreak = breakArray[i];

                newNodeCount = newNodeCount - breakArray[i];

                newVehicle = new Vehicle(newNodeCount, vehicle.MaxCapacity);
            }

            Array.Sort(breakArray);
            #endregion

            #region Increase vehicle count if full capacity
            //if (Tools.NodeCount > vehicle.MaxCapacity && (Tools.NodeCount % vehicle.MaxCapacity == 0 || vehicle.MaxCapacity - Tools.NodeCount % vehicle.MaxCapacity < 3))
            //{
            //    breakArray = new int[vehicle.BreakCount + 1];
            //}
            //else
            //{
            //    breakArray = new int[vehicle.BreakCount];

            //}

            //if (vehicle.MinCapacity == vehicle.MaxCapacity)
            //{
            //    int j = vehicle.MinCapacity;

            //    for (int i = 0; i < vehicle.BreakCount; i++)
            //    {
            //        breakArray[i] = j;

            //        j += vehicle.MinCapacity;
            //    }
            //}
            //else
            //{
            //    Vehicle newVehicle = (Vehicle)vehicle.Clone();
            //    int newNodeCount = Tools.NodeCount;

            //    int tempBreak = 0;
            //    for (int i = 0; i < vehicle.BreakCount; i++)
            //    {
            //        breakArray[i] = tempBreak + Tools.Random.Next(newVehicle.MinCapacity, newVehicle.MaxCapacity);

            //        tempBreak = breakArray[i];

            //        newNodeCount = newNodeCount - breakArray[i];

            //        newVehicle = new Vehicle(newNodeCount, vehicle.MaxCapacity);
            //    }
            //}

            //if (Tools.NodeCount > vehicle.MaxCapacity && (Tools.NodeCount % vehicle.MaxCapacity == 0 || vehicle.MaxCapacity - Tools.NodeCount % vehicle.MaxCapacity < 3))
            //{
            //    LengthIndexModel lengthIndex = Tools.GetIndexOfMaxIntervalOfArray(breakArray);

            //    int startPoint = 0;

            //    if (lengthIndex.SourceIndex != 0)
            //    {
            //        startPoint += breakArray[lengthIndex.SourceIndex - 1];
            //    }

            //    breakArray[vehicle.BreakCount] = Tools.Random.Next(startPoint + 2, startPoint + lengthIndex.Length - 2);

            //    vehicle.BreakCount += 1;
            //    vehicle.MinCapacity = 2;

            //    Array.Sort(breakArray);
            //}
            #endregion

            return breakArray;
        }
        #endregion

        #region CalculatePartialArrayCost
        public static double CalculatePartialArrayCost(int[] nodeArray)
        {
            return SumDistanceOfNodeArrayInGivenInterval(nodeArray, 0, nodeArray.Length - 1);
        }
        #endregion

        #region SumDistanceOfNodeArrayInGivenInterval
        public static double SumDistanceOfNodeArrayInGivenInterval(int[] nodeArray, int startIndex, int endIndex)
        {
            double totalDistance = 0;

            //Add distance between last node and target
            totalDistance += DistanceMatrix[nodeArray[startIndex], nodeArray[endIndex]];

            for (; startIndex < endIndex; startIndex++)
            {
                totalDistance += DistanceMatrix[nodeArray[startIndex], nodeArray[startIndex + 1]];
            }

            return totalDistance;
        }
        #endregion

        #region GetIndexOfMaxIntervalOfArray
        public static LengthIndexModel GetIndexOfMaxIntervalOfArray(int[] array)
        {
            LengthIndexModel model = new LengthIndexModel();
            model.SourceIndex = 0;
            model.Length = array[0];

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] - array[i - 1] > model.Length)
                {
                    model.SourceIndex = i;
                    model.Length = array[i] - array[i - 1];
                }
            }

            return model;
        }
        #endregion

        #region GetNodePartLengthByPartNumber
        public static int GetNodePartLengthByPartNumber(int[] nodeArray, int partNumber, int breakCount)
        {
            int nodeLength;

            if (partNumber == 1)
            {
                nodeLength = nodeArray.Length == NodeStartPointCount ? NodeStartPointCount : nodeArray[NodeStartPointCount];
            }
            else if (partNumber - breakCount == 1)
            {
                nodeLength = NodeStartPointCount - nodeArray[NodeWholeCount - 1];
            }
            else
            {
                nodeLength = nodeArray[NodeStartPointCount - 1 + partNumber] - nodeArray[NodeStartPointCount - 2 + partNumber];
            }

            return nodeLength;
        }
        #endregion

        #region GetNodePartLengthAndIndexByPartNumber
        public static LengthIndexModel GetNodePartLengthAndIndexByPartNumber(int[] nodeArray, int partNumber, int breakCount)
        {
            LengthIndexModel lengthIndexModel = new LengthIndexModel();

            if (partNumber == 1)
            {
                lengthIndexModel.Length = nodeArray.Length == NodeStartPointCount ? NodeStartPointCount : nodeArray[NodeStartPointCount];
                lengthIndexModel.SourceIndex = 0;
            }
            else if (partNumber - breakCount == 1)
            {
                lengthIndexModel.Length = NodeStartPointCount - nodeArray[NodeWholeCount - 1];
                lengthIndexModel.SourceIndex = nodeArray[NodeWholeCount - 1];
            }
            else
            {
                lengthIndexModel.Length = nodeArray[NodeStartPointCount - 1 + partNumber] - nodeArray[NodeStartPointCount - 2 + partNumber];
                lengthIndexModel.SourceIndex = nodeArray[NodeStartPointCount + partNumber - 2];
            }

            return lengthIndexModel;
        }
        #endregion

        #region GetNodePartByPartNumber
        public static int[] GetNodePartByPartNumber(int[] nodeArray, int partNumber, int breakCount)
        {
            LengthIndexModel lengthIndexModel = GetNodePartLengthAndIndexByPartNumber(nodeArray, partNumber, breakCount);
            int[] nodePart = new int[lengthIndexModel.Length];

            Array.Copy(nodeArray, lengthIndexModel.SourceIndex, nodePart, 0, lengthIndexModel.Length);

            return nodePart;
        }
        #endregion

        #region GetNodePartAndLengthByPartNumber
        public static LengthArrayModel GetNodePartAndLengthByPartNumber(int[] nodeArray, int partNumber, int breakCount)
        {
            LengthIndexModel lengthIndexModel = GetNodePartLengthAndIndexByPartNumber(nodeArray, partNumber, breakCount);

            LengthArrayModel lengthArrayModel = new LengthArrayModel
            {
                PartArray = new int[lengthIndexModel.Length],
                Length = lengthIndexModel.Length
            };

            Array.Copy(nodeArray, lengthIndexModel.SourceIndex, lengthArrayModel.PartArray, 0, lengthIndexModel.Length);

            return lengthArrayModel;
        }
        #endregion

        #region GetNodeArrayByAddingStartPointAndBreakPoints
        public static int[] GetNodeArrayByAddingStartPointAndBreakPoints(int[] nodeArray, Vehicle vehicle)
        {
            int[] newNodeArray = new int[NodeWholeCount];
            int[] newBreaks = new int[vehicle.BreakCount];

            newNodeArray[0] = NodeCount;

            for (int i = 0; i <= vehicle.BreakCount; i++)
            {
                if (vehicle.BreakCount == 0)
                {
                    Array.Copy(nodeArray, 0, newNodeArray, 1, NodeCount);
                }
                else
                {
                    if (i == 0)
                    {
                        Array.Copy(nodeArray, 0, newNodeArray, 1, StartBreaks[i]);
                        newBreaks[0] = StartBreaks[0] + 1;
                        newNodeArray[NodeStartPointCount] = newBreaks[0];
                    }
                    else if (i == vehicle.BreakCount)
                    {
                        newNodeArray[newBreaks[i - 1]] = NodeCount;

                        Array.Copy(nodeArray, StartBreaks[i - 1], newNodeArray, newBreaks[i - 1] + 1, NodeCount - StartBreaks[i - 1]);
                    }
                    else
                    {
                        newNodeArray[newBreaks[i - 1]] = NodeCount;

                        Array.Copy(nodeArray, StartBreaks[i - 1], newNodeArray, newBreaks[i - 1] + 1, StartBreaks[i] - StartBreaks[i - 1]);
                        newBreaks[i] = StartBreaks[i] + i + 1;
                        newNodeArray[NodeStartPointCount + i] = newBreaks[i];
                    }
                }
            }

            return newNodeArray;
        }
        #endregion

        #region GetNodePartsAsLengthArrayModel
        public static List<LengthArrayModel> GetNodePartsAsLengthArrayModel(int[] nodeArray, int breakCount)
        {
            var nodeParts = new List<LengthArrayModel>();

            for (int i = 0; i <= breakCount; i++)
            {
                nodeParts.Add(GetNodePartAndLengthByPartNumber(nodeArray, i + 1, breakCount));
            }

            return nodeParts;
        }
        #endregion

        #region ConvertNodePartsToNodeArray
        public static int[] ConvertLengthArrayModelToNodeArray(List<LengthArrayModel> partModels, int breakCount)
        {
            int[] nodeArray = new int[NodeWholeCount];

            int index = 0;
            for (var j = 0; j < partModels.Count; j++)
            {
                for (int i = 0; i < partModels[j].Length; i++)
                {
                    nodeArray[index] = partModels[j].PartArray[i];
                    index += 1;
                }

                if (breakCount != 0 && j + 1 != partModels.Count)
                {
                    nodeArray[NodeStartPointCount + j] = index;
                }
            }

            return nodeArray;
        }
        #endregion

        #region GetNodePartsAsList
        public static List<List<int>> GetNodePartsAsList(int[] nodeArray, int breakCount)
        {
            var nodeParts = new List<List<int>>();

            for (int i = 0; i <= breakCount; i++)
            {
                nodeParts.Add(GetNodePartByPartNumber(nodeArray, i + 1, breakCount).ToList());
            }

            return nodeParts;
        }
        #endregion

        #region GetNodePartsAsListByAddingLastTarget
        public static List<List<Node>> GetNodePartsAsListByAddingLastTarget(int[] nodeArray, List<Node> nodes, int breakCount)
        {
            List<List<int>> nodeArrayParts = GetNodePartsAsList(nodeArray, breakCount);
            List<List<Node>> nodeParts = new List<List<Node>>();

            for (var i = 0; i < nodeArrayParts.Count; i++)
            {
                List<Node> orderedPart = nodes.Where(x => nodeArrayParts[i].Contains(x.No)).OrderBy(x => nodeArrayParts[i].FindIndex(k => k == x.No)).ToList();

                nodeParts.Add(new List<Node>());
                nodeParts[i].Add(new Node(0, TargetNode.Lat, TargetNode.Lng));
                nodeParts[i].AddRange(orderedPart);
                nodeParts[i].Add(new Node(0, TargetNode.Lat, TargetNode.Lng));
            }

            for (var i = 0; i < nodeParts.Count; i++)
            {
                for (int j = 1; j < nodeParts[i].Count - 1; j++)
                {
                    nodeParts[i][j].No += 1;
                }
            }

            return nodeParts;
        }
        #endregion

        #region ConvertListArrayToNodeArray
        public static int[] ConvertListArrayToNodeArray(List<List<int>> nodeList, int breakCount)
        {
            int[] newNodeArray = new int[NodeWholeCount];

            int counter = 0;
            for (var j = 0; j < nodeList.Count; j++)
            {
                var part = nodeList[j];

                int i = 0;
                for (; i < part.Count; i++)
                {
                    newNodeArray[counter] = part[i];

                    counter += 1;
                }

                if (j != breakCount)
                {
                    newNodeArray[NodeStartPointCount + j] = counter;
                }
            }

            return newNodeArray;
        }
        #endregion
    }
}
