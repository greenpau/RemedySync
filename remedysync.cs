using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using BMC.ARSystem;
using NDesk.Options;

namespace RemedySync {

    public class Logging {

        public static string GenerateDefaultLogFileName(string BaseFileName) {
            return AppDomain.CurrentDomain.BaseDirectory + "\\" + BaseFileName + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + ".log";
        }
	
        public static void WriteToLog(string LogPath, string Src, string Message) {
            try {
                using (StreamWriter s = File.AppendText(LogPath))
                {
                    s.WriteLine(DateTime.Now + ";" + Src + ";" + Message);
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
	
        public static void WriteToEventLog(string Source, string Message, System.Diagnostics.EventLogEntryType EntryType) {
            try {
                if (!System.Diagnostics.EventLog.SourceExists(Source)) {
                    System.Diagnostics.EventLog.CreateEventSource(Source, "Application");
                }
                System.Diagnostics.EventLog.WriteEntry(Source, Message, EntryType);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
	
    public class AppConfig {

        public static int Load(ref Hashtable ht) {
            try {
                // Load application settings from file
                System.Configuration.Configuration xmlConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                System.Configuration.AppSettingsSection appSettings = xmlConfig.AppSettings;
                if (appSettings.Settings.Count != 0) {
                    foreach (string key in appSettings.Settings.AllKeys) {
                        ht.Add(key, appSettings.Settings[key].Value);
		    }
                } else {
                    return 1;
                }
                return 0;
            }
            catch (Exception e) {
                Console.WriteLine("AppConfig.Exception caught\n" + e.Source.ToString() + e.Message.ToString());
                return 1;
            }
        }
		
        public static int LoadConfig(ref string err, ref Hashtable settings, string file) {
            try {
                System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader( file );
                string contents = "";
                while (reader.Read()) {
                    reader.MoveToContent();
                    if (reader.NodeType == System.Xml.XmlNodeType.Element)
                        contents += "<"+reader.Name + ">\n";
                    if (reader.NodeType == System.Xml.XmlNodeType.Text)
                        contents += reader.Value + "\n";
                }
                Console.Write(contents);
                return 0;
            } 
            catch (Exception e) {
                err = e.Source.ToString() + ": " + e.Message.ToString();
                return 1;
            }
        }
		
        public static int LoadTemplate(ref string err, ref DataSet rmdb, string file) {
            DataTable rmdb_set = new DataTable("fields");
            rmdb_set.Columns.Add("id");
            rmdb_set.Columns.Add("type");
            rmdb_set.Columns.Add("value");
            try {
                System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader( file );
                string contents = "";
                while (xmlReader.Read()) {
                    xmlReader.MoveToContent();
                    string xmlId = null;
                    string xmlType = null;
                    string xmlValue = null;
                    if (xmlReader.NodeType == System.Xml.XmlNodeType.Element) {
                        // contents += "ELEMENT <"+xmlReader.Name + ">\n";
                        if (xmlReader.HasAttributes) {
                            // contents += "Attributes of <" + xmlReader.Name + ">\n";
                            for (int i = 0; i < xmlReader.AttributeCount; i++) {
                                // contents += xmlReader[i];
                                if (i == 0)
                                    xmlId = xmlReader[i];
                                if (i == 1) 
                                    xmlType = xmlReader[i];
                            }
                        }
                    }
                    if (xmlReader.NodeType == System.Xml.XmlNodeType.Text) {
                        //contents += "TEXT " + xmlReader.Value + "\n";
                        xmlValue = xmlReader.Value;
                    }
                    // populate internal database
                    if (xmlId != null && xmlType != null) {
                        rmdb_set.Rows.Add(xmlId, xmlType, xmlValue);
                        Console.Write(xmlId, xmlType, xmlValue);
                    }
                }
                Console.Write(contents);
                rmdb.Tables.Add(rmdb_set);
                return 0;
            } 
            catch (Exception e) {
                err = e.Source.ToString() + ": " + e.Message.ToString();
                return 1;
            }
        }
    }

    class Program {
        private static string _app_name    = "remedysync";
        private static string _app_ver     = "1.0.1";
        private static string _app_author  = "Copyright (C) 2013 Paul Greenberg";
        private static string _app_lic     = "This software is distributed on an \"AS IS\" BASIS, WITHOUT WARRANTIES\n             OR CONDITIONS OF ANY KIND, either express or implied.";
        private static string _app_descr   = "Remedy API Interface";
        private static bool _show_help  = false;
        private static string _config   = null;
        private static string _template = null;
        private static string _summary  = null;
        private static string _notes    = null;
        private static string _error    = null;

        [STAThread]
        static void Main(string[] args) {
            try {
                 Hashtable rmdHeader = new Hashtable();
                 Hashtable rmdFields = new Hashtable();
                 Hashtable rmdConfig = new Hashtable();
                 DataSet rmdb = new DataSet("app");
                 OptionSet opts = new OptionSet();
                 opts.Add("c=|config=",   "specify Remedy configuration file",             delegate(string v) { _config = v.ToString(); });
                 opts.Add("t=|template=", "specify Remedy template file",                  delegate(string v) { _template = v.ToString(); });
                 opts.Add("s=|summary=",  "populate Remedy ticket 'Summary' field",        delegate(string v) { _summary = v.ToString(); });
                 opts.Add("n=|notes=",    "populate Remedy ticket 'Notes' field",          delegate(string v) { _notes = v.ToString(); });
                 opts.Add("h|?|help",     "show this message and exit",                    delegate(string v) { _show_help = v != null; });
                 opts.Parse(args);

                 if (_show_help == true) {
                     AppDisplayHelp( null );
                 }

                 if (_config == null) {
                     _error += "    Remedy configuration file is missing.\n";
                 } else {
                     string _status = null;
                     if (AppConfig.LoadConfig(ref _status, ref rmdConfig, _config) > 0) {
                         _error += "    " + _status + "\n";	
                     }
                 }
                 if (_template == null) {
                     _error += "    Remedy template file is missing.\n";
                 } else {
                     string _status = null;
                     if (AppConfig.LoadTemplate(ref _status, ref rmdb, _template) > 0) {
                         _error += "    " + _status + "\n";	
                     }
                 }

                 if (_summary == null) {
                     _error += "    Failed to provide a title or heading for the ticket/incident.\n";
                 }

                 if (_notes == null) {
                     _error += "    Failed to provide notes or comments for the ticket/incident.\n";
                 }

                 if (_error != null) {
                     AppDisplayHelp( _error );
                 }

                 Console.WriteLine(rmdb.GetXml());
            } catch(Exception e) {
                 Console.WriteLine("NDesk.Exception caught\n" + e.ToString()); 
            }

            Hashtable cnxSettings = new Hashtable();
            if (AppConfig.Load(ref cnxSettings) > 0) {
                Console.WriteLine( "Error occurred while parsing your configuration file. Please check whether the file exists or any variable missing.");
            }

            Console.WriteLine("Remedy Connection Settings: {0} over tcp/{1} with {2} ({4}, {3})",
                              cnxSettings["REMEDY_SERVER"], cnxSettings["REMEDY_PORT"],cnxSettings["REMEDY_USER"],
                              cnxSettings["REMEDY_FIRST_NAME"], cnxSettings["REMEDY_LAST_NAME"] );

            try {
                BMC.ARSystem.Server remedyCnx = new BMC.ARSystem.Server();
                BMC.ARSystem.FieldValueList qFieldList = new BMC.ARSystem.FieldValueList();
                qFieldList.Add(1000000076, "CREATE");                                     // Keyword Triggers the Create
                qFieldList.Add(1000000000, "network: upgrade connectivity diagram ");     // Summary
                qFieldList.Add(1000000151, "Prepare the statement.");                     // Notes
                qFieldList.Add(7, 1);                                                     // Status
                ///qFieldList.Add(1000000215, "Direct Input");                            // Reported Source (Mandatory)
                qFieldList.Add(1000000215, 1000);                                         // Reported Source (Mandatory)
                qFieldList.Add(1000000163, 4000);                                         // Impact
                qFieldList.Add(1000000162, 4000);                                         // Urgency
                //qFieldList.Add(1000000164, 3);                                          // Priority
                qFieldList.Add(1000000018, cnxSettings["REMEDY_FIRST_NAME"]);             // Last_Name
                qFieldList.Add(1000000019, cnxSettings["REMEDY_LAST_NAME"]);              // First_Name
                qFieldList.Add(301921200,  cnxSettings["REMEDY_LOGIN_ID"]);               // Login ID
                qFieldList.Add(1000000082, "MYCORP");                                     // Customer: Company
                qFieldList.Add(1000000001, "MYCORP");                                     // Classification: Company
                qFieldList.Add(1000000099, "User Service Request");                       // Classification: Incident Type
                qFieldList.Add(1000000251, "MYCORP");                                     // Assignment: Support Company
                qFieldList.Add(1000000014, "Technology");                                 // Assignment: Support Organization
                qFieldList.Add(1000000217, "Infrastructure Engineering");                 // Assignment: Assigned Group
                //remedyCnx.Login(cnxSettings["REMEDY_SERVER"].ToString(), cnxSettings["REMEDY_USER"].ToString(), cnxSettings["REMEDY_PASS"].ToString());
                //string remedyTkt = remedyCnx.CreateEntry("HPD:IncidentInterface_Create", qFieldList);
                //remedyCnx.Logout();
                //Console.WriteLine("Remedy Ticket {0} was successfully created.", remedyTkt);
            } catch(Exception e) {
                Console.WriteLine("ARSystem.Exception caught\n" + e.ToString()); 
            }
        }

        static void AppDisplayHelp( string AppMsg ) {
            if (AppMsg != null) {
                Console.WriteLine("\nError:\n" + AppMsg); 
            }
            if (AppMsg == null) {
                Console.WriteLine("Name:        " + _app_name);
                Console.WriteLine("Version:     " + _app_ver); 
                Console.WriteLine("Author:      " + _app_author); 
                Console.WriteLine("License:     " + _app_lic); 
                Console.WriteLine("Description: " + _app_descr + "\n");
            }
            Console.WriteLine("Usage: " + _app_name + " [options] -c|--config <FILE> -t|--template <FILE>");
            Console.WriteLine("       " + _app_name + " -c config.xml -t template.xml -s \"Test Incident\" -n \"Test Notes\"\n");
            Console.WriteLine("    options:");
            Console.WriteLine("        -c, --config     specify Remedy configuration file");
            Console.WriteLine("        -t, --template   specify Remedy template file");
            Console.WriteLine("        -s, --summary    populate Remedy ticket 'Summary' field");
            Console.WriteLine("        -n, --notes      populate Remedy ticket 'Notes' field");
            Console.WriteLine("        -h, --help       show help message");
            Environment.Exit(0);
        }
    }
}
