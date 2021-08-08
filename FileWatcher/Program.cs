﻿using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using TE.FileWatcher.Configuration;
using TE.FileWatcher.Logging;

namespace TE.FileWatcher
{
    /// <summary>
    /// The main program class.
    /// </summary>
    class Program
    {
        // Success return code
        private const int SUCCESS = 0;

        // Error return code
        private const int ERROR = -1;

        /// <summary>
        /// The main function.
        /// </summary>
        /// <param name="args">
        /// Arguments passed into the application.
        /// </param>
        /// <returns>
        /// Returns 0 on success, otherwise non-zero.
        /// </returns>
        static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand
            {
                new Option<string>(
                    aliases: new string[] { "--folder", "-f" },
                    description: "The folder containing the configuration XML file."),

                new Option<string>(
                    aliases: new string[] { "--configFile", "-cf" },
                    description: "The name of the configuration XML file."),
            };
            rootCommand.Description = "Monitors files and folders for changes.";
            rootCommand.Handler = CommandHandler.Create<string, string>(Run);

            return rootCommand.Invoke(args);
        }

        /// <summary>
        /// Runs the file/folder watcher.
        /// </summary>
        /// <param name="folder">
        /// The folder where the config and notifications files are stored.
        /// </param>
        /// <param name="configFileName">
        /// The name of the configuration file.
        /// </param>
        /// <returns>
        /// Returns 0 if no error occurred, otherwise non-zero.
        /// </returns>
        private static int Run(string folder, string configFile)
        {            
            IConfigurationFile config = new XmlFile(folder, configFile);

            // Load the watches information from the config XML file
            Watches watches = config.Read();
            if (watches == null)
            {
                return ERROR;
            }

            // Set the logger
            if (!SetLogger(watches))
            {
                return ERROR;
            }

            // Run the watcher tasks
            if (StartWatchers(watches))
            {
                return SUCCESS;
            }
            else
            {
                return ERROR;
            }
        }

        /// <summary>
        /// Sets the logger.
        /// </summary>
        /// <param name="watches">
        /// The watches object that contains the log path.
        /// </param>
        /// <returns>
        /// True if the Logger was set, otherwise false.
        /// </returns>
        private static bool SetLogger(Watches watches)
        {
            if (watches == null)
            {
                Console.WriteLine("The watches object was not initialized.");
                return false;
            }

            try
            {
                Logger.SetFullPath(watches.Logging.LogPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The log file could not be set. Reason: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Runs the watcher tasks as defined in the configuration XML file.
        /// </summary>
        /// <param name="watches">
        /// The watches.
        /// </param>
        /// <returns>
        /// True if the tasks were started and run successfully, otherwise false.
        /// </returns>
        private static bool StartWatchers(Watches watches)
        {
            if (watches == null)
            {
                Console.WriteLine("The watches object was not initialized.");
                return false;
            }

            watches.Start();
            new System.Threading.AutoResetEvent(false).WaitOne();

            Logger.WriteLine("All watchers have closed.");
            return true;
        }
    }
}
