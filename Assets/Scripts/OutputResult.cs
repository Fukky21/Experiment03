using System.IO;
using UnityEngine;

public class OutputResult
{
    public static int initialize(string fileName)
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }

        string filePath = Application.dataPath + "/Results/" + fileName;
        FileInfo fi = new FileInfo(filePath);
        string header = "This is Header";

        if (!fi.Exists)
        {
            using (StreamWriter writer = fi.CreateText())
            {
                writer.WriteLine(header);
                writer.Close();
            }
            return 0;
        }
        else
        {
            // ファイルが既に存在するときは、-1を返す
            Debug.Log("File already exists");
            return -1;
        }
    }

    public static void writeResult(string fileName)
    {
        string filePath = Application.dataPath + "/Results/" + fileName;
        FileInfo fi = new FileInfo(filePath);
        string dataRow = "End of file";

        using (StreamWriter writer = fi.AppendText())
        {
            writer.WriteLine(dataRow);
            writer.Close();
        }
    }
}
