using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.IO;


namespace RunStoredProcedure
{
    class Program
    {
        const int versionMajor = 1;
        const int versionMinor = 0;
        const int versionRevision = 0;
        static bool verboseMode = false;
        static string logFilePath = "ExtractSQL_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

        static void Usage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Extract views, user functions and stored procedures from a database to SQL files.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Usage: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tExtractSQL");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [-v] ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -s ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("server");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -d ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("database");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -u ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("user");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -p ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("password");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [-o ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("output_path");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tExtractSQL");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [--verbose] ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --server ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("server");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --database ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("database");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --user ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("user");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --password ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("password");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [--output ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("output_path");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tExtractSQL");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -h");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tExtractSQL");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --help");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Options:");
            Console.WriteLine("\t-v | --verbose\tBe verbose for debugging purposes.");
            Console.WriteLine("\t-s | --server\tHostname (or IP) of the database server.");
            Console.WriteLine("\t-d | --database\tName of the database to connect to.");
            Console.WriteLine("\t-u | --user\tUssername with permissions to run the query.");
            Console.WriteLine("\t-p | --password\tPassword of the user.");
            Console.WriteLine("\t-o | --output\tPath or filename to write the output SQL. If a file is given all is writen to one file,");
            Console.WriteLine("\t             \telsewise there wil be one file for each database object. This argument is optional.");
            Console.WriteLine("\t             \tIf not given the default action is to write everthing in one file named database-timestamp.sql");
            Console.WriteLine("\t             \tIf it is a directory it MUST end in the \\ symbol.");
            Console.WriteLine();
            Console.WriteLine("\t-h | --help\tShow this help message.");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Third ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("3");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("ye Software Inc. (\u00A9) 2024");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Version: {0}.{1}.{2}. ", versionMajor, versionMinor, versionRevision);
        }

        public static void WriteLog(string logMessage)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\'WriteLog\' ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("(Error creating or writing to Log file)");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void PrintInfo(string str1, string str2)
        {
            WriteLog("I: " + str1 + " " + str2);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(str1);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(str2);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintHeader(string status)
        {
            WriteLog("H: " + status);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(status);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintError(string name, string error, string detail)
        {
            WriteLog("E: " + error + ": " + "\'" + name + "\' " + "(" + detail + ")");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(error + ": ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\'" + name + "\' ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(" + detail + ")");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintVerbose(string msg)
        {
            if (true == verboseMode)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("VERBOSE: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static string EndWithDirectorySeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar;
            }
            return path;
        }


        static string CreateFullPath(string path, string filename, string extension)
        {
            // Some proc names have 'name...2022-10-10 12:34:22' in them
            filename = filename.Replace(".", "_")
                               .Replace(" ", "_")
                               .Replace(":", "_")
                               .Replace(">", "_")
                               .Replace("<", "_")
                               .Replace(",", "_")
                               .Replace("\"", "_").Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = ".\"";
            }
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename), "Filename cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentNullException(nameof(extension), "Extension cannot be null or empty");
            }
            path = EndWithDirectorySeparator(path);
            string fullFilename = $"{filename}.{extension}";
            string fullPath = Path.Combine(path, fullFilename);
            return fullPath;
        }


        static void ExtractDboToFile(string connectionString, string dboName, string path, string server, string database) {
            PrintVerbose("DBO Name: " + dboName);
            string filename;
            bool append = false;
            if (FileType.File == GetFileType(path))
            {
                filename = path;
                append = true;
            }
            else {
                filename = CreateFullPath(path, dboName, "sql");
            }
            PrintVerbose("File Name: " + filename);
            string storedProcedureName = $"[{server}].[{database}].[dbo].[sp_helptext]"; // works for views, stored procedures and user functions.
            PrintVerbose("Stored Procedure Name: " + storedProcedureName);

            WriteLog(" - " + dboName + " -> " + filename);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  - ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(dboName);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" => ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(filename);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" ... ");
            Console.Write("[");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@objname", "[" + dboName + "]");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            using (StreamWriter writer = new StreamWriter(filename, append))
                            {
                                string line = String.Empty;
                                while (reader.Read())
                                {
                                    line = (string)reader.GetValue(0);
                                    PrintVerbose("+> " + line);
                                    writer.Write(line);
                                }
                                
                            }
                        }
                    }
                }

                WriteLog("     > OK");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OK");
            }
            catch (Exception ex)
            {
                WriteLog("     > ERROR: " + ex.Message);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR");
            }
            finally {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("]");
            }
        }


        static List<string> RunSqlQuery(string connectionString, string query)
        {
            List<string> dboName = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            dboName.Add($"{name}");
                        }
                    }
                }
            }
            return dboName;
        }
 
  
        public static FileType GetFileType(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or whitespace.");
            }
            if (path.Replace("'", "").Replace("\"", "").EndsWith(@"\")) //, StringComparison.Ordinal
            {
                return FileType.Directory;
            }
            else
            {
                return FileType.File;
            }
        }

        public enum FileType
        {
            File,
            Directory
        }


        static void Main(string[] args)
        {
            string server = String.Empty;
            string database = String.Empty;
            string user = String.Empty;
            string password = String.Empty;
            string filePath = String.Empty;
            bool error = false;

            //if (args.Length != 8 && args.Length != 10 && args.Length != 8 && args.Length != 10)
            //{
            //    Usage();
            //    return;
            //}

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-s":
                    case "--server":
                        if (i + 1 < args.Length)
                            server = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -s | --server option.", "Please read usage.");
                        break;

                    case "-d":
                    case "--database":
                        if (i + 1 < args.Length)
                            database = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -d | --database option.", "Please read usage.");
                        break;

                    case "-u":
                    case "--user":
                        if (i + 1 < args.Length)
                            user = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -u | --user option.", "Please read usage.");
                        break;

                    case "-p":
                    case "--password":
                        if (i + 1 < args.Length)
                            password = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -p | --password option.", "Please read usage.");
                        break;

                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                            filePath = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -o | --output option.", "Please read usage.");
                        break;

                    case "-v":
                    case "--verbose":
                        verboseMode = true;
                        break;

                    default:
                        PrintError("Error", "Unmatched argument: " + (string)args[i], "Please read usage.");
                        error = true;
                        break;
                }
            }
            if (String.Empty == server || String.Empty == database || String.Empty == user || String.Empty == password || true == error) 
            {
                if (String.Empty == server)
                    PrintError("Fatal Error", "Missing value for -s | --server option.", "argument is mandatory.");
                if (String.Empty == database)
                    PrintError("Fatal Error", "Missing value for -d | --database option.", "argument is mandatory.");
                if (String.Empty == user)
                    PrintError("Fatal Error", "Missing value for -u | --user option.", "argument is mandatory.");
                if (String.Empty == password)
                    PrintError("Fatal Error", "Missing value for -p | --password option.", "argument is mandatory.");
                if (true == error)
                    PrintError("Error", "Unwanted arguments found", "I refuse to continue.");
                Usage();
            }
            else {
                if (String.Empty == filePath)
                {
                    filePath = @".\database_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".sql";
                }
                else
                {
                    filePath = filePath.Replace("'", "").Replace("\"", "");
                }

                PrintHeader("Start of process: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                PrintInfo("  Server:           ", server);
                PrintInfo("  Database:         ", database);
                PrintInfo("  User:             ", user);
                PrintInfo("  Password:         ", "********"); // password);
                if (FileType.Directory == GetFileType(filePath)) {
                    try
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    catch (Exception ex)
                    {
                        PrintInfo("  Error on output directory: ", ex.Message);
                    }
                    PrintInfo("  Output directory: ", filePath);
                } else {
                    PrintInfo("  Output file:      ", filePath);
                }
                
                string connectionString = "Server=" + server + ";Database=" + database + ";User Id=" + user + ";Password=" + password + ";";
                
                PrintHeader("Extracting Stored Procedures from the database: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                List<string> dboNamesSP = RunSqlQuery(connectionString, "SELECT [name] FROM sys.procedures");
                foreach (string dboName in dboNamesSP)
                {
                    ExtractDboToFile(connectionString, dboName.Trim(), filePath, server, database);
                }
                PrintHeader("Extracting Views from the database: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                List<string> dboNamesV = RunSqlQuery(connectionString, "SELECT [name] FROM sys.views");
                foreach (string dboName in dboNamesV)
                {
                    ExtractDboToFile(connectionString, dboName.Trim(), filePath, server, database);
                }
                PrintHeader("Extracting User Functions from the database: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                List<string> dboNamesF = RunSqlQuery(connectionString, "SELECT [name] FROM sys.objects WHERE [type] IN ('FN', 'IF', 'TF')");
                foreach (string dboName in dboNamesF)
                {
                    ExtractDboToFile(connectionString, dboName.Trim(), filePath, server, database);
                }

                PrintInfo("Se log file for details: ", logFilePath);
                PrintHeader("End of process: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            }
        }
    }
}