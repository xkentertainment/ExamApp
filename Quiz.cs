using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace ExamApp
{
    [Serializable]
    public class Quiz
    {
        public readonly DateTime creationTime;
        public string name;
        public readonly QuizQuestions questions;
        public List<QuizResults> results;
        public Quiz ()
        {
            creationTime = DateTime.Now;
            questions = new QuizQuestions (new List<Question> ());
            results = new List<QuizResults> ();
        }
        public Quiz (DateTime creationTime)
        {
            this.creationTime = creationTime;
            questions = new QuizQuestions (new List<Question> ());
            results = new List<QuizResults> ();
        }
        public Quiz (List<Question> questions)
        {
            creationTime = DateTime.Now;
            this.questions = new QuizQuestions (questions) ?? throw new ArgumentNullException (nameof (questions));
            results = new List<QuizResults> ();
        }
        public QuizResults RecentResults()
        {
            if (results.Count == 0)
                return null;
            QuizResults recent = results[0];
            foreach (QuizResults results in results)
            {
                if (results.creationTime > recent.creationTime)
                {
                    recent = results;
                }
            }
            return recent;
        }
    }
    [Serializable]
    public class QuizResults
    {
        public string attemptNumber;
        public readonly DateTime creationTime;
        public List<(Question question, string selectedAnswer)> wrongAnswers;
        public List<Question> rightAnswers;
        public QuizResults()
        {
            creationTime = DateTime.Now;
        }
        public (bool,string answer) GotCorrect(Question question)
        {
            foreach (var wrongAnswer in wrongAnswers)
            {
                if (wrongAnswer.question.id == question.id)
                {
                    return (false, wrongAnswer.selectedAnswer);
                }
            }
            return (true,string.Empty);
        }
    }
    [Serializable]
    public class Question
    {
        public string id;
        public string question;
        public SerialBitmap bitmap;
        public string rightAnswer;
        public List<string> wrongAnswers;
    }
    [Serializable]
    public class SerialBitmap
    {
        public byte[] data;
        public SerialBitmap(Bitmap from)
        {
            MemoryStream memoryStream = new MemoryStream ();
            from.Compress (Bitmap.CompressFormat.Png, 25, memoryStream);

            data = memoryStream.ToArray ();
        }
    }
    [Serializable]
    public class QuizQuestions : IEnumerable
    {
        public int Count => list.Count;
        public readonly List<Question> list;
        public QuizQuestions ()
        {
            list = new List<Question> ();
        }
        public QuizQuestions (List<Question> questions)
        {
            this.list = questions;
        }
        public Question this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }
        public Question this[string id]
        {
            get
            {
                foreach (Question _question in list)
                {
                    if (_question.id == id)
                    {
                        return _question;
                    }
                }
                throw new ArgumentOutOfRangeException ();
            }
        }
        public bool Correct (string id, string answer)
        {
            return this[id].rightAnswer == answer;
        }
        public void Shuffle()
        {
            list.Shuffle ();
        }

        public IEnumerator GetEnumerator ()
        {
            return ((IEnumerable)list).GetEnumerator ();
        }
    }
}