using System;

namespace EPMConnector
{
    public class ModLoging
    {
        public static string Pfad = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

        public static string Datei = "Log.txt";
        public enum eTyp
        {
            Information,
            Warning,
            Exception
        }

        public static Exception LastException = null;

        public static void Log_Exception(Exception ex, string ZusatzInfo = "", string LogDatei = "")
        {
            Log("Error: " + ex.ToString() + ": " + (string.IsNullOrEmpty(ZusatzInfo) ? "" : "More Infos: " + ZusatzInfo + " - "), eTyp.Exception, LogDatei);
        }

        public static void Log(string Message, eTyp Typ = eTyp.Information, string LogDatei = "")
        {
            System.IO.FileStream stmFile = default(System.IO.FileStream);
            System.IO.StreamWriter binWriter = default(System.IO.StreamWriter);
            bool FileExists = false;

            try
            {
                if (string.IsNullOrEmpty(LogDatei))
                {
                    if (string.IsNullOrEmpty(Datei))
                        Datei = "Log.txt";

                    if (System.IO.Directory.Exists(Pfad) == false)
                    {
                        return;
                    }
                    LogDatei = Pfad + Datei;

                }
                else
                {
                    if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(LogDatei)) == false)
                    {
                        return;
                    }
                }

                FileExists = System.IO.File.Exists(LogDatei);

                stmFile = new System.IO.FileStream(LogDatei, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                binWriter = new System.IO.StreamWriter(stmFile);

                if (FileExists == false)
                    binWriter.WriteLine("Date" + "\t" + "Time " + "\t" + "Status" + "\t" + "Message");

                binWriter.WriteLine(Get_LogString(Message, Typ));

                binWriter.Flush();
                binWriter.Close();
                stmFile.Close();
            }
            catch (Exception)
            {
            }
        }

        public static string Get_LogString(string Message, eTyp Typ = eTyp.Information)
        {
            string functionReturnValue = null;
            switch (Typ)
            {
                case eTyp.Information:
                    functionReturnValue = String.Format("{0:dd.MM.yyyy}", DateTime.Now) + "\t" + String.Format("{0:HH:mm:ss.fff}", DateTime.Now) + "\t" + "I" + "\t" + Message;
                    break;
                case eTyp.Warning:
                    functionReturnValue = String.Format("{0:dd.MM.yyyy}", DateTime.Now) + "\t" + String.Format("{0:HH:mm:ss.fff}", DateTime.Now) + "\t" + "W" + "\t" + Message;
                    break;
                case eTyp.Exception:
                    functionReturnValue = String.Format("{0:dd.MM.yyyy}", DateTime.Now) + "\t" + String.Format("{0:HH:mm:ss.fff}", DateTime.Now) + "\t" + "E" + "\t" + Message;
                    break;
                default:
                    functionReturnValue = "";
                    break;
            }
            return functionReturnValue;
        }
    }
}
