using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConversionOptimizer
{
    public class Test : IComparable<Test>
    {
        public bool Convertable;
        public string FitnessePath;
        public string FullPath;
        public int NumLines;
        public string Status;
        public string Author, Notes;

        public Test(string path, List<string> exceptions, string inputStatus)
        {
            //Get Full Path and add context.txt
            FullPath = @"C:\Projects\FitNesseRoot\" + path.Replace('.', '\\') + @"\content.txt";

            Convertable = true;
            FitnessePath = path;

            if (inputStatus != null)
                Status = inputStatus;

            //Count number of lines
            NumLines = 0;

            TextReader testFile;

            try
            {
                testFile = new StreamReader(FullPath);

                while (testFile.Peek() != -1)
                {
                    string currLine = testFile.ReadLine();

                    if (currLine.StartsWith("|"))
                        NumLines++;

                    if (currLine.Contains("!include"))
                        MacroDetector(currLine);

                    if(!Status.Equals("Finished") && !Status.Equals("Waiting for Review") && !Status.Equals("Not to be Converted"))
                        ExceptionDetector(currLine, exceptions);
                }
            }
            catch (FileNotFoundException e)
            {
                NumLines = -1;
                Status = "Test Not Found";
                Convertable = false;
            }
            catch (DirectoryNotFoundException e)
            {
                NumLines = -1;
                Status = "Test Directory Not Found";
                Convertable = false;
            }


            if (NumLines == -1)
                return;

            //Search for unconvertable syntax

        }

        #region Object Creation Helper Methods

        private void ExceptionDetector(string currLine, List<string> exceptions)
        {
            if (exceptions != null)
            {
                currLine = currLine.ToUpper();

                foreach (string ex in exceptions)
                {
                    if (currLine.Contains(ex))
                    {
                        Convertable = false;

                        if (Status != null && Status.Contains("On Hold:") && !Status.Contains(ex))
                        {
                            Status = Status + " " + ex;
                        }
                        else
                        {
                            Status = "On Hold: " + ex;
                        }
                    }
                }
            }
        }

        private void MacroDetector(string currLine)
        {
                const string root = @"C:\Projects\FitNesseRoot\";
                /*
                 char[] splitter = { ' ' };
                 string[] macroDetected = currLine.Split(splitter);
                 */



            if (currLine.Contains("<"))
            {
                string[] buildnewpath = FitnessePath.Split('.');
                

                currLine = Regex.Match(currLine, @"([\w]+[\.]+)+(\w)+").ToString();

                string[] goUp = currLine.Split('.');

               // currLine = buildnewpath[0] + "." + currLine;

                DirectoryInfo hunt = new DirectoryInfo(FullPath.Replace("\\content.txt", ""));

                bool found = false;

                do
                {
                    DirectoryInfo[] directorylist = hunt.GetDirectories();

                    

                    foreach (DirectoryInfo directoryInfo in directorylist)
                    {
                        if (directoryInfo.Name.Equals(goUp[0]))
                        {
                            currLine = hunt.FullName + "." + currLine;

                            currLine = currLine.Replace("C:\\Projects\\FitNesseRoot\\", "").Replace("\\", ".");

                            currLine.Trim();

                            found = true;
                        }
                    }


                    hunt = hunt.Parent;

                } while (!found);
            }

            else if(!currLine.Contains("."))
            {
                string pathIt;

                if (currLine.Contains(">"))
                {
                    pathIt = Regex.Match(currLine, @"[\>][\w]+").ToString().Replace(">", "");

                    currLine = FitnessePath + "." + pathIt;
                }
                else if (currLine.Contains("^"))
                {
                    pathIt = Regex.Match(currLine, @"[\^][\w]+").ToString().Replace("^", "");

                    string[] buildnewpath = FitnessePath.Split('.');

                    buildnewpath[buildnewpath.Length - 1] = pathIt;

                    currLine = "";

                    foreach (string s in buildnewpath)
                    {
                        if (currLine.Equals(""))
                            currLine = s;
                        else
                            currLine = currLine + "." + s;
                    }
                }
                else
                {
                    pathIt = currLine.Replace("!include", "").Replace("-c", "").Replace("-C", "").Replace("-seamless", "").Trim();

                    string[] buildnewpath = FitnessePath.Split('.');

                    buildnewpath[buildnewpath.Length - 1] = pathIt;

                    currLine = "";

                    foreach (string s in buildnewpath)
                    {
                        if (currLine.Equals(""))
                            currLine = s;
                        else
                            currLine = currLine + "." + s;
                    }
                    currLine.Trim(); //comment out when certain it works.
                }

                //currLine = FitnessePath + "\\" + pathIt;
            }
            else
            {
                currLine = Regex.Match(currLine, @"([\w]+[\.]+)+(\w)+").ToString();
                currLine.Trim();
            }
          
            string macro = root + currLine.Replace('.', '\\');

            Test output;

                if (Program.MacroList.ContainsKey(currLine))
                {
                    Program.MacroList.TryGetValue(currLine, out output);
                }
                else if (Program.TestList.ContainsKey(currLine))
                {
                    Program.TestList.TryGetValue(currLine, out output);

                    Program.TestList.Remove(currLine);

                    output.Status = "Macro";

                    Program.MacroList.Add(currLine, output);
                }
                else
                { 
                    output = new Test(currLine, null, "Macro");
                    Program.MacroList.Add(currLine, output);
                }

                NumLines += output.NumLines;
        }

        #endregion


        #region IComparable<Test> Members

        public int CompareTo(Test other)
        {
            if (statusCompare().CompareTo(other.statusCompare()) == 0)
                return NumLines.CompareTo(other.NumLines);
            else
            {
                return statusCompare().CompareTo(other.statusCompare());
            }
        }



        private int statusCompare()
        {
            switch (Status)
            {
                case "Not Started":
                    return 1;
                case "In Progress":
                    return 0;
                case "On Hold":
                    return 2;
                case "Waiting for Review":
                    return 4;
                case "Finished":
                    return 5;
                case "Macro":
                    return 6;
                default:
                    return 3;
            }
        }

        #endregion
    }
}