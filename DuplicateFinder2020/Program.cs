using DuplicateFinderLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuplicateFinder2020
{
    class Program
    {
        public static string[] argumentList = { " -pattern ", " -target ", " -parallel " };
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Command list: ");
                Console.WriteLine("df -pattern [string] -target [string],[string]... -parallel [number] (optional)");
                Console.WriteLine("delete -parallel [number] (optional)");
                Console.WriteLine("checkconnection");
                Console.WriteLine("cleardatabase");
                Console.WriteLine("exit");

                var line = Console.ReadLine();

                string command;
                if (line.Contains(" "))
                {
                    command = line.Substring(0, line.IndexOf(" "));
                }
                else
                    command = line;

                if (command.ToLower() == "df")
                {                   
                    Scan(line);
                }
                else if (command.ToLower() == "delete")
                {
                    Delete(line);
                }
                else if (command.ToLower() == "checkconnection")
                {
                    CheckConnection();
                }
                else if (command.ToLower() == "cleardatabase")
                {
                    ClearDatabase();

                }
                else if (command.ToLower() == "exit")
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
                Console.WriteLine("--------------------------");
            }
        }

        public static void Scan(string line)
        {
            try
            {
                var searchPattern = ParseArgument(line, "-pattern");
                var paths = ParseArgument(line, "-target").Split(',');
                var maxDegreeOfParallelism = ParseArgument(line, "-parallel") ?? "1";

                DFLib df = new DFLib();
                foreach (var path in paths)
                {
                    df.StartScan(path, searchPattern, int.Parse(maxDegreeOfParallelism));
                }
                Console.WriteLine("---Scan Complete---");
            }
            catch (Exception exp)
            {
                DFLib.LogError(exp.Message);
            }
            
        }

        public static string ParseArgument(string line, string argument)
        {
            if (!line.Contains(argument))
                return null;

            var lineAfterArgument = line.Substring(line.IndexOf(argument));
            var nextArgumentAvailable = argumentList.Any(x => lineAfterArgument.Substring(argument.Count()).Contains(x));
            if(nextArgumentAvailable)
            {
                var argumentStartIndexList = new List<int>();
                foreach(var arg in argumentList)
                {
                    var index = lineAfterArgument.IndexOf(arg);
                    if (index != -1)
                        argumentStartIndexList.Add(index);
                }
                return lineAfterArgument.Substring(argument.Count() + 1, argumentStartIndexList.Min() - argument.Count() -1);
            }
            else
            {
                return lineAfterArgument.Substring(argument.Count() + 1);
            }
        }

        public static void Delete(string line)
        {
            var maxDegreeOfParallelism = int.Parse(ParseArgument(line, "-parallel") ?? "1");

            try
            {
                DFLib df = new DFLib();
                df.DeleteFiles(maxDegreeOfParallelism);
            }
            catch (Exception exp)
            {
                DFLib.LogError(exp.Message);
            }
        }

        public static void CheckConnection()
        {            
            try
            {
                DFLib df = new DFLib();
                Console.WriteLine(df.CheckConnection());
            }
            catch (Exception exp)
            {
                DFLib.LogError(exp.Message);
            }           
        }

        public static void ClearDatabase()
        {          
            try
            {
                DFLib df = new DFLib();
                df.RemoveAllData();
                Console.WriteLine("---Records successfully removed from database---");
            }
            catch (Exception exp)
            {
                DFLib.LogError(exp.Message);
            }
        }
    }
}
