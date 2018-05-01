using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTest
{
    class Program
    {
        static int Main(string[] args)
        {
            //string targetName = "D2304", startName = "D1-1", fileName = Guid.NewGuid().ToString();
            //var procInfoUnity = new ProcessStartInfo(@"D:\Unity Projects\5\Dijkstra and A Star\MapPathFinding\Dijkstra\CNHomework.exe ");
            //procInfoUnity.Arguments = "\"" + targetName + "\" \"" + startName + "\" \"" + fileName + "\"";
            ////var unityProcPath = $@"""D:\Unity Projects\5\Dijkstra and A Star\MapPathFinding\Dijkstra\CNHomework.exe"" ""{targetName}"" ""{startName}"" ""{fileName}""";
            //var unityProc = Process.Start(procInfoUnity);
            //unityProc.EnableRaisingEvents = true;
            //var selectedVideo = "";
            //unityProc.WaitForExit();

            var videoPath = @"C:\Users\OmerG\Documents\MapPathFinding\Video\";
            var videos = Directory.GetFiles(videoPath);
            //selectedVideo = videos.FirstOrDefault((x) => x.Contains(fileName));
            //Process.Start(selectedVideo);
            IMongoDatabase db = new MongoClient().GetDatabase("VideoDB");
            var videoCollection = db.GetCollection<BsonDocument>("Videos");

            //UploadFileAsync(db, videoPath, Path.GetFileName(selectedVideo));
            UploadFileAsync(videoCollection.Database, videoPath+ "2018 - 05 - 01 - 19 - 54 - 42 - 1c5313ec - 74f0 - 43a5 - b292 - 7c27b2ce3e03.mp4", "2018-05-01-19-54-42-1c5313ec-74f0-43a5-b292-7c27b2ce3e03.mp4");

            return 0;
        }
        public async Task UploadDemoAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting GridFSDemo");
            IMongoDatabase database = collection.Database;
            const string filePath = @"C:\temp\mars996.png";
            //the name of the uploaded GridFS file
            const string fileName = @"mars996";
            //add some metadata to the GridFSFileInfo object
            // to facilitate searching
            var photoMetadata = new BsonDocument
         {
             { "Category", "Astronomy" },
             { "SubGroup", "Planet" },
             { "ImageWidth", 640 },
             { "ImageHeight", 480 }
         };
            var uploadOptions = new GridFSUploadOptions { Metadata = photoMetadata };
            try
            {
                await UploadFileAsync(database, filePath, fileName, uploadOptions);
            }
            catch (Exception e)
            {
                Console.WriteLine("***GridFS Error " + e.Message);
            }
        }
        public static async Task UploadFileAsync(
                 IMongoDatabase database,
                 string filePath,
                 string fileName,
                 GridFSUploadOptions uploadOptions = null)
        {
            var gridFsBucket = new GridFSBucket(database);
            using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
            {
                using (
                    GridFSUploadStream destinationStream =
                        await gridFsBucket.OpenUploadStreamAsync(fileName, uploadOptions))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                    await destinationStream.CloseAsync();
                }
            }
            Console.WriteLine();
        }
        //public async Task DemoDownloadFilesAsync()
        //{
        //    IMongoCollection<GridFSFileInfo> filesCollection = database.GetCollection<GridFSFileInfo>("fs.files");
        //    List<GridFSFileInfo> fileInfos = await DemoFindFilesAsync(filesCollection);
        //    foreach (GridFSFileInfo gridFsFileInfo in fileInfos)
        //    {
        //        Console.WriteLine("Found file {0} Length {1}", gridFsFileInfo.Filename, gridFsFileInfo.Length);
        //        try
        //        {
        //            await DemoDownloadFileAsync(database, filePath, fileName);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("***GridFS Error " + e.Message);
        //        }
        //    }

        //}
    }
}
