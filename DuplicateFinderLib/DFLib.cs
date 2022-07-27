using DuplicateFinderDAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderLib
{
    public class DFLib
    {

        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void StartScan(string path, string searchPattern, int maxDegreeOfParallelism)
        {
            try
            {
                if (!DirectoryScanned(path))
                {
                    Logger.Info($"{path} will be scanned");
                    var dirId = CreateDirectoryInDb(path);
                    ScanFiles4Directory(path, searchPattern, dirId, maxDegreeOfParallelism);
                }
                else
                {
                    Logger.Warn($"{path} is already scanned!");
                }

                var dirs = Directory.GetDirectories(path);
                foreach (var d in dirs)
                {
                    StartScan(d, searchPattern, maxDegreeOfParallelism);
                }
            }
            catch (Exception ex) { }
        }

        public void ScanFiles4Directory(string dir, string searchPattern, decimal dirId, int maxDegreeOfParallelism)
        {
            var files = Directory.GetFiles(dir, searchPattern);
            var dfHashList = new ConcurrentBag<DFHash>();
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, item =>
            {
                using (var fs = new FileStream(item, FileMode.Open, FileAccess.Read))
                {
                    var entity = new DFHash()
                    {
                        SHA1 = BitConverter.ToString(SHA1.Create().ComputeHash(fs)),
                        PathId = dirId,
                        FileSize = fs.Length,
                        FileName = item
                    };
                    dfHashList.Add(entity);
                }
            });
            using (var context = new DFDataContext())
            {
                context.DFHashes.AddRange(dfHashList);
                context.SaveChanges();
            }

            UpdateDbPath(dirId, files.Length);

            Logger.Info($"{dir} is scanned.");
        }

        private decimal CreateDirectoryInDb(string path)
        {
            using (var context = new DFDataContext())
            {
                var entity = new DFPath
                {
                    Path = path,
                    FileCount = 0,
                    ScanStarted = DateTime.Now
                };
                context.DFPaths.Add(entity);
                context.SaveChanges();
                return entity.Id;
            }
        }

        private void UpdateDbPath(decimal id, int fileCount)
        {
            using (var context = new DFDataContext())
            {
                var entity = context.DFPaths.Where(x => x.Id == id).FirstOrDefault();
                entity.FileCount = fileCount;
                entity.ScanFinished = DateTime.Now;
                context.SaveChanges();
            }
        }

        private bool DirectoryScanned(string path)
        {
            using (var context = new DFDataContext())
            {
                var entity = context.DFPaths.Where(x => x.Path == path).FirstOrDefault();
                if (entity == null)
                    return false;
                else
                {
                    if (entity.ScanFinished != null)
                    {
                        return true;
                    }
                    else
                    {
                        var fileEntities = context.DFHashes.Where(x => x.PathId == entity.Id).ToList();
                        context.DFHashes.RemoveRange(fileEntities);
                        context.DFPaths.Remove(entity);
                        context.SaveChanges();
                        return false;
                    }
                }
            }
        }

        private List<string> GetDuplicatedFiles()
        {
            using (var context = new DFDataContext())
            {
                var duplicateFiles = context.DFHashes.ToList().GroupBy(x => x.SHA1).Select(x => new { Hash = x.Key, Files = x.Select(z => z.FileName).ToList() });
                List<string> filesToDelete = new List<string>();
                filesToDelete.AddRange(duplicateFiles.SelectMany(x => x.Files.Skip(1)).ToList());
                return filesToDelete;
            }
        }

        public void RemoveAllData()
        {
            using (var context = new DFDataContext())
            {
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE DFHashes");
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE DFPaths");
                context.SaveChanges();
            }
        }

        public bool CheckConnection()
        {
            using (var context = new DFDataContext())
            {
                var randomEntity = context.DFPaths.Select(x => x.Id).Take(1).FirstOrDefault();
                return true;
            }
        }

        public void DeleteFiles(int maxDegreeOfParallelism)
        {
            var toDeleteConcurrent = new ConcurrentBag<string>(GetDuplicatedFiles());
            Parallel.ForEach(toDeleteConcurrent, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, file =>
            {
                File.Delete(file);
                Logger.Info($"{file} is deleted.");
            });
        }

        public static void LogError(string message)
        {
            Logger.Error(message);
        }
    }
}
