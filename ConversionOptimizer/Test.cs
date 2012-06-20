using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConversionOptimizer
{
    public class Test : IComparable<Test>
    {
        public bool Convertable;
        public string FitnessePath;
        public string FullPath;
        public int NumLines;
        public string Status;

        public Test(string path, List<string> exceptions)
        {
            //Get Full Path and add context.txt
            FullPath = @"C:\Projects\FitNesseRoot\" + path.Replace('.', '\\') + @"\content.txt";

            //Count number of lines
            NumLines = 0;

            TextReader testFile;

            try
            {
                testFile = new StreamReader(FullPath);

               while(testFile.Peek() != -1)
               {
                   NumLines++;

                   string currLine = testFile.ReadLine().ToUpper();

                   if (exceptions != null)
                       foreach (string ex in exceptions)
                       {
                           if (currLine.Contains(ex))
                           {
                               Convertable = false;

                               if(Status != null && Status.Contains("On Hold:") && !Status.Contains(ex))
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

        

        public int statusCompare()
        {
            switch (Status)
            {
                case "Not Started":
                    return 0;
                case "In Progress":
                    return 1;
                case "On Hold":
                    return 2;
                case "Waiting for Review":
                    return 4;
                case "Finished":
                    return 5;
                default:
                    return 3;
            }
        }

        #endregion
    }
}