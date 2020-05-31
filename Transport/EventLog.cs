using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Transport
{
public static class EventLog {
     public static string FilePath { get; set; }
     static object lockMe = new object();

     public static void Write(string format, params object[] arg) {
         Write(string.Format(format, arg));
     }
  
     public static void Write(string message) {
         if (string.IsNullOrEmpty(FilePath)) {
             //FilePath = Directory.GetCurrentDirectory();
             FilePath = System.Windows.Forms.Application.StartupPath;
         }
         string filename = FilePath + 
             string.Format("\\{0:yyyy}\\{0:MM}\\{1}-{0:yyyy-MM-dd}.txt", DateTime.Now,StaticConstant.MM_AppName);
         FileInfo finfo = new FileInfo(filename);
         if (finfo.Directory.Exists == false) {
             finfo.Directory.Create();
         }
         string writeString = string.Format("{0:yyyy/MM/dd HH:mm:ss} {1}", 
             DateTime.Now, message) + Environment.NewLine;

         lock (lockMe) {
             File.AppendAllText(filename, writeString, Encoding.Unicode);
         }
     }
     public static void WriteError(string format, params object[] arg)
     {
         WriteError(string.Format(format, arg));
     }

     public static void WriteError(string message)
     {
         if (string.IsNullOrEmpty(FilePath))
         {
             FilePath = Directory.GetCurrentDirectory();
         }
         string filename = FilePath +
             string.Format("\\{0:yyyy}\\{0:MM}\\{1}-Error-{0:yyyy-MM-dd}.txt", DateTime.Now, StaticConstant.MM_AppName);
         FileInfo finfo = new FileInfo(filename);
         if (finfo.Directory.Exists == false)
         {
             finfo.Directory.Create();
         }
         string writeString = string.Format("{0:yyyy/MM/dd HH:mm:ss} {1}",
             DateTime.Now, message) + Environment.NewLine;

         lock (lockMe) {
             File.AppendAllText(filename, writeString, Encoding.Unicode);
         }
     }

    #region CheckUser
     public static void WriteCU(string format, params object[] arg)
     {
         WriteCU(string.Format(format, arg));
     }

     public static void WriteCU(string message)
     {
         if (string.IsNullOrEmpty(FilePath))
         {
             //FilePath = Directory.GetCurrentDirectory();
             FilePath = System.Windows.Forms.Application.StartupPath;
         }
         string filename = FilePath +
             string.Format("\\{0:yyyy}\\{0:MM}\\{1}-CheckUser-{0:yyyy-MM-dd}.txt", DateTime.Now, StaticConstant.MM_AppName);
         FileInfo finfo = new FileInfo(filename);
         if (finfo.Directory.Exists == false)
         {
             finfo.Directory.Create();
         }
         string writeString = string.Format("{0:yyyy/MM/dd HH:mm:ss} {1}",
             DateTime.Now, message) + Environment.NewLine;

         lock (lockMe)
         {
             File.AppendAllText(filename, writeString, Encoding.Unicode);
         }
     }

    #endregion

}
}
