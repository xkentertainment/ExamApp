using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ExamApp
{
    public static class StorageManager
    {
        public static string storagePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
        static string quizPath = "quiz.uwu";
        static string dataPath = "data.uwu";

        public static void SaveQuiz (Quiz quiz)
        {
            List<Quiz> history = GetQuizzes ();
            bool contained = false;
            int containedAt = -1;
            for (int i = 0; i < history.Count; i++)
            {
                if (history[i].creationTime == quiz.creationTime)
                {
                    contained = true;
                    containedAt = i;
                }
            }
            if (contained)
            {
                history.RemoveAt (containedAt);
            }
            history.Add (quiz);
            BinaryFormatter bf = new BinaryFormatter ();

            FileStream file = File.Create (storagePath + quizPath);
            bf.Serialize (file, history);
            file.Close ();
        }
        public static bool DeleteQuiz(DateTime id)
        {
            List<Quiz> history = GetQuizzes ();
            bool contained = false;
            int containedAt = -1;
            for (int i = 0; i < history.Count; i++)
            {
                if (history[i].creationTime == id)
                {
                    contained = true;
                    containedAt = i;
                }
            }
            if (contained)
            {
                history.RemoveAt (containedAt);
            }
            BinaryFormatter bf = new BinaryFormatter ();

            FileStream file = File.Create (storagePath + quizPath);
            bf.Serialize (file, history);
            file.Close ();
            return contained;
        }
        public static bool NameTaken(string name)
        {
            List<Quiz> history = GetQuizzes ();
            foreach (Quiz q in history)
            {
                if (q.name.ToLower () == name.ToLower ())
                    return true;
            }
            return false;
        }
        public static List<Quiz> GetQuizzes ()
        {
            if (!File.Exists (storagePath + quizPath))
                return new List<Quiz> ();
            using (FileStream file = File.Open (storagePath + quizPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter ();
                List<Quiz> quizzes = bf.Deserialize (file) as List<Quiz>;
                file.Close ();
                return quizzes;
            }
        }
        public static Data GetData()
        {
            if (!File.Exists (storagePath + dataPath))
                return new Data();
            using (FileStream file = File.Open (storagePath + dataPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter ();
                Data data = bf.Deserialize (file) as Data;
                file.Close ();
                return data;
            }
        }
        public static void SaveData(Data data)
        {
            FileStream file = File.Create (storagePath + dataPath);
            BinaryFormatter bf = new BinaryFormatter ();

            bf.Serialize (file, data);
            file.Close ();
        }
    }

    [Serializable]
    public class Data
    {
        public string examinerName { get; set; }
        public string parentNumber { get; set; }
        public string teacherNumber { get; set; }
        public bool requirePassword { get; set; }
        public string examinerPassword { get; set; }
        public string childName { get; set; }
        public int timerLength { get; set; }
        public Data ()
        {
            parentNumber = string.Empty;
            parentNumber = string.Empty;
            requirePassword = false;
            examinerPassword = string.Empty;
            teacherNumber = string.Empty;
            childName = string.Empty;
            timerLength = 10;
        }
    }
}