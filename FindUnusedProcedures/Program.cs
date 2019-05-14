using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace FindUnusedProcedures
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<string>();
            var ignore = (ConfigurationManager.GetSection("ignoreList") as StringConfiguration);
            if (ignore != null && ignore.IgnoreList.Count >0 )
            {
                foreach (var o in ignore.IgnoreList)
                {
                    list.Add(((IgnoredElement) o).Value);
                }
            }
            
            var searchingPackage = LoadProcedures(list);
            TestReferences(searchingPackage);

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }

        private static HashSet<References> LoadProcedures(List<string> ignorePackages)
        {
            var list = new HashSet<References>();
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());
            var database = DatabaseFactory.CreateDatabase();
            using (var reader = database.ExecuteReader(CommandType.Text, "select object_name|| \'.\'|| PROCEDURE_NAME from USER_PROCEDURES where object_type =\'PACKAGE\'"))
            {
                while (reader.Read())
                {
                    var data = reader.GetValue(0).ToString();
                    var procs = data.Split('.').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                    if (procs.Length != 2)
                    {
                        continue;
                    }
                    if (ignorePackages.Any(x => string.Compare(x,procs[0], StringComparison.InvariantCultureIgnoreCase) ==0))
                    {
                        continue;
                    }
                    
                    list.Add( new References(){ SpName = $"{data}"});
                }
            }
            return list;
        }

        private static void TestReferences(HashSet<References> packages)
        {
            
            using (var build = MSBuildWorkspace.Create())
            {
                var sol = build.OpenSolutionAsync(@"test.sln");
                
                foreach (var project in sol.Result.Projects)
                {
                    //var comp = project.GetCompilationAsync().Result;
                    foreach (var docs in project.Documents)
                    {
                        var source = docs.GetTextAsync().Result;
                        var pp = packages.Where(x => x.Found == false && source != null && source.ToString().ToLower().Contains(x.SpName.ToLower()));
                        foreach (var p in pp)
                        {
                            packages.FirstOrDefault(x => x.SpName == p.SpName).Found = true;
                        }
                    }
                }
            }
           
            foreach (var package in packages)
            {
                if (!package.Found)
                {
                    File.AppendAllText("test.txt", package.SpName);
                    File.AppendAllText("test.txt", "\n");
                }
            }
        }
    }

    public class References
    {
        /// <summary>
        /// Stored proc name
        /// </summary>
        public string SpName { get; set; }

        /// <summary>
        /// Found
        /// </summary>
        public bool Found { get; set; } = false;
    }
}
