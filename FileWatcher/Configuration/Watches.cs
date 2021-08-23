﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TE.FileWatcher.Logging;

namespace TE.FileWatcher.Configuration
{
    /// <summary>
    /// The watches root node in the XML file.
    /// </summary>
    [XmlRoot("watches")]
    public class Watches
    {
        /// <summary>
        /// Gets or sets the logging information.
        /// </summary>
        [XmlElement("logging")]
        public Logging Logging { get; set; } = new Logging();

        /// <summary>
        /// Gets or sets the watches list.
        /// </summary>
        [XmlElement("watch")]
        public List<Watch> WatchList { get; set; }

        /// <summary>
        /// Starts the watches.
        /// </summary>
        public void Start()
        {
            foreach (Watch watch in WatchList)
            {
                try
                {
                    Task.Run(() => watch.Start());
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex.Message, LogLevel.ERROR);
                }
            }
        }
    }
}
