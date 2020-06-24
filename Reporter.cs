using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Telephony;
using Android.Support.V4.Content;

namespace ExamApp
{
    public class Reporter
    {
        public static void Report (Quiz quiz, QuizResults results)
        {
            SmsManager manager = SmsManager.Default;
            Data data = StorageManager.GetData ();
            string message;
            if (data.teacherNumber == string.Empty)
            {
                string title = data.examinerName != string.Empty ? data.examinerName : "Examiner";
                message = $"Dear {title}, your pulil {data.childName} has done {quiz.name} in the Quiz App and obtained a score of {results.rightAnswers.Count} out of {quiz.questions.Count}\n{(int)MathF.Round ((float)results.rightAnswers.Count / quiz.questions.Count * 100)}%";
                manager.SendMultipartTextMessage (data.teacherNumber, string.Empty, manager.DivideMessage (message), null, null);
            }
            if (data.parentNumber == string.Empty)
            {
                return;
            }
            message = $"Dear Parent, your child {data.childName} has done {quiz.name} in the Quiz App and obtained a score of {results.rightAnswers.Count} out of {quiz.questions.Count}\n{(int)MathF.Round ((float)results.rightAnswers.Count / quiz.questions.Count * 100)}%";
            manager.SendMultipartTextMessage (data.parentNumber, string.Empty, manager.DivideMessage (message), null, null);
        }
        public static void Aced (Quiz quiz)
        {
            SmsManager manager = SmsManager.Default;
            Data data = StorageManager.GetData ();
            string message;
            if (data.teacherNumber != string.Empty)
            {

                string title = data.examinerName != string.Empty ? data.examinerName : "Examiner";
                message = $"Dear {title}, your pupil has done {quiz.name} in the Quiz App and attained 100%! A perfect score of {quiz.questions.Count} out of {quiz.questions.Count}!";
                manager.SendMultipartTextMessage (data.teacherNumber, string.Empty, manager.DivideMessage (message), null, null);

            }
            if (data.parentNumber == string.Empty)
            {
                return;
            }
            message = $"Dear Parent, your child has done {quiz.name} in the Quiz App and attained 100%! A perfect score of {quiz.questions.Count} out of {quiz.questions.Count}!";
            manager.SendMultipartTextMessage (data.parentNumber, string.Empty, manager.DivideMessage (message), null, null);
        }
    }
}