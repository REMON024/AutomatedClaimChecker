using AzureCognitiveService.Subscription;
using ClosedXML.Excel;
using KhatiExcel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveService.DocumentSimilarity
{
    public class CognitiveSimilarity
    {
        public (bool success, string remakrs , double ratio) SubmitNid(string imgPath1 , string imgPath2, string model, string outputImage=null )
        {
            string guid = Guid.NewGuid().ToString();
            string outputPath = string.Format("C:/Sample/Document/Runtime/{0}.jpeg", guid);
            if(outputImage is null)
                ImageMarger(imgPath1,imgPath2 ,outputPath);

            var feature = GetImageNamedEntity(outputImage==null?outputPath:outputImage);

            if (!feature.img.Faces.Any())
            {
                return (false, "Proof of Address Does not contains any image of a person", 0.0);
            }

            var named_entity_category = feature.img.Categories.Select(x => x.Name + ";Cat").ToList();
            var named_entity_tags = feature.img.Tags.Select(x => x.Name + ";Tag").ToList();
            var named_entity_object = feature.img.Objects.Select(x => x.ObjectProperty + ";Object").ToList();

            var load = new LoadExcel();
            //var result = load.Fetch(@"C:/Hackathon/ImageToTextPOC/Project/AutomatedClaimChecker/AutomatedClaimChecker/wwwroot/NID Vector.xlsx");
            var result = load.Fetch(model);

            List<double> testImg = new List<double>();
            List<double> vector = new List<double>();
            List<CosineSimilarity> cosineSims = new List<CosineSimilarity>();

            for(int i = 0; i < result.data.Count()-1; i++)
            {
                List<string> trainSet = new List<string>();
                List<string> testSet = new List<string>();

                var data = result.data[i];

                double scroe = 0;

                foreach(var d in data)
                {
                    if (named_entity_category.Contains(d.ColumnName))
                    {
                        scroe = feature.img.Categories.Where(x => x.Name == d.ColumnName.Split(';')[0])
                            .Select(x => x.Score).FirstOrDefault();
                    }
                    else if (named_entity_tags.Contains(d.ColumnName))
                    {
                        scroe = feature.img.Tags.Where(x => x.Name == d.ColumnName.Split(';')[0])
                            .Select(x => x.Confidence).FirstOrDefault();
                    }
                    else if (named_entity_object.Contains(d.ColumnName))
                    {
                        scroe = feature.img.Objects.Where(x => x.ObjectProperty == d.ColumnName.Split(';')[0])
                            .Select(x => x.Confidence).FirstOrDefault();
                    }

                    testImg.Add(scroe);
                    vector.Add(Convert.ToDouble(d.ColumnValue));

                    if (Convert.ToDouble(d.ColumnValue) != 0)
                    {
                        trainSet.Add(d.ColumnName);
                    }
                   
                   
                }

                testSet.AddRange(named_entity_category);
                testSet.AddRange(named_entity_tags);
                testSet.AddRange(named_entity_object);
                trainSet.Remove("IsNID");
                var cosineSim = ComputeJaccardSimilarity(trainSet,testSet);
                cosineSims.Add(new CosineSimilarity()
                {
                    index = i,
                    score = cosineSim
                });

               
            }

           
            var mostSimilarityFound = cosineSims.OrderByDescending(x => x.score).Select(x => new { x.index ,x.score}).FirstOrDefault();

            if(mostSimilarityFound == null)
            {
                return (false, "Proof of Id Mismatch", 0.0);
            }

            var score = Convert.ToDouble(result.data[Convert.ToInt32(mostSimilarityFound.index)].Where(x=> x.ColumnName == "IsNID")
                .Select(x=> x.ColumnValue).First());

            if (score == 1)
            {
                if (mostSimilarityFound.score > 0.34)
                {
                    return (true, "Nid Found", mostSimilarityFound.score);
                }

                return (false, "The Particuler Document is not Considerd as NID", mostSimilarityFound.score);
            }
            return (false, "The Particuler Document is not Considerd as NID", mostSimilarityFound.score);
        }

        public (bool success, string remakrs, double ratio) SubmitDC(string imgPath1, string imgPath2, string model, string outputImage = null)
        {
            string guid = Guid.NewGuid().ToString();
            string outputPath = string.Format("C:/Sample/Document/Runtime/{0}.jpeg", guid);
            if (outputImage is null)
                ImageMarger(imgPath1, imgPath2, outputPath);

            var feature = GetImageNamedEntity(outputImage == null ? outputPath : outputImage);

            var named_entity_category = feature.img.Categories.Select(x => x.Name + ";Cat").ToList();
            var named_entity_tags = feature.img.Tags.Select(x => x.Name + ";Tag").ToList();
            var named_entity_object = feature.img.Objects.Select(x => x.ObjectProperty + ";Object").ToList();

            var load = new LoadExcel();
            //var result = load.Fetch(@"C:/Hackathon/ImageToTextPOC/Project/AutomatedClaimChecker/AutomatedClaimChecker/wwwroot/NID Vector.xlsx");
            var result = load.Fetch(model);

            List<double> testImg = new List<double>();
            List<double> vector = new List<double>();
            List<CosineSimilarity> cosineSims = new List<CosineSimilarity>();

            for (int i = 0; i < result.data.Count() - 1; i++)
            {
                List<string> trainSet = new List<string>();
                List<string> testSet = new List<string>();

                var data = result.data[i];

                double scroe = 0;

                foreach (var d in data)
                {
                    if (named_entity_category.Contains(d.ColumnName))
                    {
                        scroe = feature.img.Categories.Where(x => x.Name == d.ColumnName.Split(';')[0])
                            .Select(x => x.Score).FirstOrDefault();
                    }
                    else if (named_entity_tags.Contains(d.ColumnName))
                    {
                        scroe = feature.img.Tags.Where(x => x.Name == d.ColumnName.Split(';')[0])
                            .Select(x => x.Confidence).FirstOrDefault();
                    }
                    else if (named_entity_object.Contains(d.ColumnName))
                    {
                        scroe = feature.img.Objects.Where(x => x.ObjectProperty == d.ColumnName.Split(';')[0])
                            .Select(x => x.Confidence).FirstOrDefault();
                    }

                    testImg.Add(scroe);
                    vector.Add(Convert.ToDouble(d.ColumnValue));

                    if (Convert.ToDouble(d.ColumnValue) != 0)
                    {
                        trainSet.Add(d.ColumnName);
                    }

                }

                testSet.AddRange(named_entity_category);
                testSet.AddRange(named_entity_tags);
                testSet.AddRange(named_entity_object);
                trainSet.Remove("IsDC");
                var cosineSim = ComputeJaccardSimilarity(trainSet, testSet);
                cosineSims.Add(new CosineSimilarity()
                {
                    index = i,
                    score = cosineSim
                });


            }

            var mostSimilarityFound = cosineSims.OrderByDescending(x => x.score).Select(x => new { x.index, x.score }).FirstOrDefault();

            if (mostSimilarityFound == null)
            {
                return (false, "Proof of Id Mismatch", 0.0);
            }

            var score = Convert.ToDouble(result.data[Convert.ToInt32(mostSimilarityFound.index)].Where(x => x.ColumnName == "IsDC")
                .Select(x => x.ColumnValue).First());

            if (score == 1)
            {
                if (mostSimilarityFound.score > 0.34)
                {
                    return (true, "Death Certificate", mostSimilarityFound.score);
                }

                return (false, "The Particuler Document is not Considerd as Death Certificate", mostSimilarityFound.score);
            }
            return (false, "The Particuler Document is not Considerd as Death Certificate", mostSimilarityFound.score);
        }
        private (HashSet<string> set, ImageAnalysis img) GetImageNamedEntity(string imgPath)
        {
            HashSet<string> entity = new HashSet<string>();

            string apiKey = Keys.Key;
            string endpoint = Keys.Endpoint;

            try
            {
                var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
                {
                    Endpoint = endpoint
                };

                var imageBytes = File.ReadAllBytes(imgPath);

                var analyzeFeature = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Faces
            };

                var features = client.AnalyzeImageInStreamAsync(new MemoryStream(imageBytes), visualFeatures: analyzeFeature).Result;

                var named_entity_category = features.Categories.Select(x => x.Name + ";Cat").ToList();
                var named_entity_tags = features.Tags.Select(x => x.Name + ";Tag").ToList();
                var named_entity_object = features.Objects.Select(x => x.ObjectProperty + ";Object").ToList();

                foreach (var x in named_entity_category)
                {
                    entity.Add(x);
                }
                foreach (var x in named_entity_tags)
                {
                    entity.Add(x);
                }
                foreach (var x in named_entity_object)
                {
                    entity.Add(x);
                }

                return (entity, features);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Training Failed /n Error: {ex.Message}");
                return (null, null);
            }
        }
        public (List<TrainDataSet>[] dataSet, List<string> headers, string matricExcelBase64) TrainingNID(List<string> imgPaths)
        {
            HashSet<string> entity = new HashSet<string>();

            foreach (string imgPath in imgPaths)
            {
                var named_entity = GetImageNamedEntity(imgPath);

                foreach (var feature in named_entity.set)
                {
                    entity.Add(feature);
                }
            }

            var entity_list = entity.ToList();

            List<TrainDataSet>[] trainDataSet = new List<TrainDataSet>[imgPaths.Count()];

            int count = 0;

            foreach (string imgPath in imgPaths)
            {
                var named_entity = GetImageNamedEntity(imgPath);

                trainDataSet[count] = new List<TrainDataSet>();

                foreach (var n in entity_list)
                {
                    double scroe = 0;
                    if (named_entity.set.Contains(n))
                    {
                        if (n.Contains("Cat"))
                        {
                            scroe = named_entity.img.Categories.Where(x => x.Name == n.Split(';')[0]).Select(x => x.Score).FirstOrDefault();
                        }
                        else if (n.Contains("Tag"))
                        {
                            scroe = named_entity.img.Tags.Where(x => x.Name == n.Split(';')[0]).Select(x => x.Confidence).FirstOrDefault();
                        }
                        else if (n.Contains("Object"))
                        {
                            scroe = named_entity.img.Objects.Where(x => x.ObjectProperty == n.Split(';')[0]).Select(x => x.Confidence).FirstOrDefault();
                        }

                    }
                    trainDataSet[count].Add(new TrainDataSet()
                    {
                        Word = n,
                        Vector = scroe
                    });
                }

                trainDataSet[count].Add(new TrainDataSet()
                {
                    Word = "HasFace",
                    Vector = named_entity.img.Faces.Any() ? 1 : 0
                });

                if (imgPath.ToLower().Contains("notnid"))
                {
                    trainDataSet[count].Add(new TrainDataSet()
                    {
                        Word = "IsNID",
                        Vector = 0
                    });
                }
                else
                {
                    trainDataSet[count].Add(new TrainDataSet()
                    {
                        Word = "IsNID",
                        Vector = 1
                    });
                }
                count++;
            }

            entity_list.Add("HasFace");
            entity_list.Add("IsNID");

            var matric = MatricGeneration("sheet1", entity_list, trainDataSet);

            return (trainDataSet, entity_list, matric);
        }
        public (List<TrainDataSet>[] dataSet, List<string> headers,string matricExcelBase64) TrainingDOC(List<string> imgPaths)
        {
            HashSet<string> entity = new HashSet<string>();

            foreach (string imgPath in imgPaths)
            {
                var named_entity = GetImageNamedEntity(imgPath);

                foreach (var feature in named_entity.set)
                {
                    entity.Add(feature);
                }
            }

            var entity_list = entity.ToList();

            List<TrainDataSet>[] trainDataSet = new List<TrainDataSet>[imgPaths.Count()];

            int count = 0;

            foreach (string imgPath in imgPaths)
            {
                var named_entity = GetImageNamedEntity(imgPath);

                trainDataSet[count] = new List<TrainDataSet>();

                foreach (var n in entity_list)
                {
                    double scroe = 0;
                    if (named_entity.set.Contains(n))
                    {
                        if (n.Contains("Cat"))
                        {
                            scroe = named_entity.img.Categories.Where(x => x.Name == n.Split(';')[0]).Select(x => x.Score).FirstOrDefault();
                        }
                        else if (n.Contains("Tag"))
                        {
                            scroe = named_entity.img.Tags.Where(x => x.Name == n.Split(';')[0]).Select(x => x.Confidence).FirstOrDefault();
                        }
                        else if (n.Contains("Object"))
                        {
                            scroe = named_entity.img.Objects.Where(x => x.ObjectProperty == n.Split(';')[0]).Select(x => x.Confidence).FirstOrDefault();
                        }

                    }
                    trainDataSet[count].Add(new TrainDataSet()
                    {
                        Word = n,
                        Vector = scroe
                    });
                }

                if (imgPath.ToLower().Contains("notdoc"))
                {
                    trainDataSet[count].Add(new TrainDataSet()
                    {
                        Word = "IsDC",
                        Vector = 0
                    });
                }
                else
                {
                    trainDataSet[count].Add(new TrainDataSet()
                    {
                        Word = "IsDC",
                        Vector = 1
                    });
                }
                count++;
            }

            entity_list.Add("IsDC");

            var matric = MatricGeneration("sheet1", entity_list, trainDataSet);

            return (trainDataSet, entity_list, matric);
        }
        private double JaccardSimilarity(ImageAnalysis? feature1, ImageAnalysis? feature2)
        {
            if (feature1 is null || feature2 is null)
                return 0.0;

            var tags = ComputeJaccardSimilarity(feature1.Tags.Select(x => x.Name).ToList(), feature2.Tags.Select(x => x.Name).ToList());
            var objects = ComputeJaccardSimilarity(feature1.Objects.Select(x => x.ObjectProperty).ToList(), feature2.Objects.Select(x => x.ObjectProperty).ToList());
            var category = ComputeJaccardSimilarity(feature1.Categories.Select(x => x.Name).ToList(), feature2.Categories.Select(x => x.Name).ToList());
            var sum = tags + objects + category;
            if (sum == 0) return 0.0;
            return sum / 3;
        }
        private double ComputeJaccardSimilarity<T>(List<T> feature1, List<T> feature2)
        {
            var intersect = feature1.Intersect(feature2);
            var union = feature1.Union(feature2);
            var similarity = (double)intersect.Count() / union.Count();
            if (intersect.Count() == 0) return 0.0;
            return similarity;
        }
        public (string message, bool result) ImageMarger(string nidFront, string nidBack, string nid)
        {
            string imagePath1 = nidFront;
            string imagePath2 = nidBack;
            string outputPath = nid;

            try
            {
                // Load the images
                Bitmap image1 = new Bitmap(imagePath1);
                Bitmap image2 = new Bitmap(imagePath2);

                // Create a new bitmap with dimensions large enough to hold both images
                int maxWidth = Math.Max(image1.Width, image2.Width);
                int totalHeight = image1.Height + image2.Height;

                using (Bitmap mergedImage = new Bitmap(maxWidth, totalHeight))
                {
                    using (Graphics g = Graphics.FromImage(mergedImage))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        g.DrawImage(image1, 0, 0, image1.Width, image1.Height);

                        g.DrawImage(image2, 0, image1.Height, image2.Width, image2.Height);
                    }

                    // Save the merged image to the output path
                    mergedImage.Save(outputPath, ImageFormat.Jpeg);

                    return ($"Images merged successfully. Merged image saved to: {outputPath}", true);
                }
            }
            catch (Exception ex)
            {
                return ($"Error: {ex.Message}", false);
            }
        }
        private  string MatricGeneration(string SheetName, List<string> HeaderName, List<TrainDataSet>[] Rows)
        {
            try
            {
                using (XLWorkbook xLWorkbook = new XLWorkbook())
                {
                    IXLWorksheet iXLWorksheet = xLWorkbook.Worksheets.Add(SheetName);
                    for (int i = 0; i < HeaderName.Count; i++)
                    {
                        iXLWorksheet.Cell(1, i + 1).Value = HeaderName[i];
                        iXLWorksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    for (int j = 0; j < Rows.Length; j++)
                    {
                        for (int k = 0; k < Rows[j].Count; k++)
                        {
                            iXLWorksheet.Cell(j + 2, k + 1).Value = Rows[j][k].Vector;
                        }
                    }

                    MemoryStream memoryStream = new MemoryStream();
                    xLWorkbook.SaveAs(memoryStream);
                    string item = Convert.ToBase64String(memoryStream.ToArray());
                    return item;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private double CalculateCosineSimilarity(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
            {
                throw new ArgumentException("Vectors must have the same length");
            }

            // Calculate dot product
            double dotProduct = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
            }

            // Calculate magnitudes
            double magnitudeA = CalculateMagnitude(vectorA);
            double magnitudeB = CalculateMagnitude(vectorB);

            // Calculate cosine similarity
            if (magnitudeA > 0 && magnitudeB > 0)
            {
                return dotProduct / (magnitudeA * magnitudeB);
            }
            else
            {
                // Handle the case where one or both vectors have zero magnitude
                return 0.0;
            }
        }
        private double CalculateMagnitude(double[] vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += Math.Pow(vector[i], 2);
            }
            return Math.Sqrt(sum);
        }
    }
}

