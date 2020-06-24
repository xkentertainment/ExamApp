using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Syncfusion.XlsIO;

namespace ExamApp
{
    static class ExcelImport
    {
        public static Quiz Process (Stream stream)
        {
            //ExcelPackage package = new ExcelPackage (stream);
            ExcelEngine engine = new ExcelEngine ();
            IApplication app = engine.Excel;
            IWorkbook workbook = app.Workbooks.Open (stream);
            IWorksheet sheet = workbook.ActiveSheet;
            List<Question> questions = FromExcel (sheet);

            Quiz quiz = new Quiz (questions);

            return quiz;
        }
        const int maxQuestions = 200;
        const int rowsPerThread = 5;
        static List<Question> FromExcel (IWorksheet sheet)
        {

            //sheet.Pictures[0].Picture
            GetTopPositions (sheet);
            (bool hasImage, Syncfusion.Drawing.Image) ImageContained ((int r, int c) position)
            {
                for (int i = 0; i < sheet.Pictures.Count; i++)
                {
                    var drawing = sheet.Pictures[i];
                    if (HasImage (position, sheet, drawing))
                    {
                        return (true, drawing.Picture);
                    }
                }
                return (false, null);
            }
            List<Question> questions = new List<Question> ();
            List<Task> threads = new List<Task> ();
            int totalThreads = maxQuestions / rowsPerThread;
            idPool = new Dictionary<int, int> ();
            for (int i = 0; i <= totalThreads; i++)
            {
                int start = 2 + (i * rowsPerThread);
                Task task = Task.Run (() =>
                {
                    for (int r = start; r <= maxQuestions && r <= start + rowsPerThread; r++)
                    {
                        Question question = new Question
                        {
                            id = (r - 1).ToString ()
                        };
                        if (!idPool.ContainsKey (r))
                        {
                            if (sheet[r, 2].Text == string.Empty)
                            {
                                break;
                            }
                            else
                            {
                                question.question = sheet[r, 2].Text;
                                question.rightAnswer = sheet[r, 4].Text;

                                question.wrongAnswers = new List<string> ()
                            {
                                sheet[r,5].Text,
                                sheet[r,6].Text,
                                sheet[r,7].Text
                            };

                                var (hasImage, picture) = ImageContained ((r, 3));
                                if (hasImage)
                                {
                                    Bitmap bitmap = BitmapFactory.DecodeByteArray (picture.ImageData, 0, picture.ImageData.Length);
                                    question.bitmap = new SerialBitmap (bitmap);
                                }
                                if (!string.IsNullOrEmpty (question.question) && !string.IsNullOrEmpty (question.rightAnswer) && question.wrongAnswers.Count == 3 && !idPool.ContainsKey (r))
                                {
                                    if (idPool.TryAdd (r, r))
                                    {
                                        questions.Add (question);
                                    }
                                }
                            }
                        }
                    }
                });
                threads.Add (task);
            }
            Task.WaitAll (threads.ToArray ());
            return questions;
        }
        static Dictionary<int,int> idPool = new Dictionary<int, int> ();
        const int marginOfError = 3;
        static bool HasImage ((int r, int c) position, IWorksheet worksheet, IPictureShape picture)
        {
            int left = 0;
            IRange cell = worksheet[position.r, position.c];
            for (int i = 1; i < cell.Column && i <= worksheet.Columns.Length; i++)
            {
                left += worksheet.GetColumnWidthInPixels (i);
            }
            Rect bounds = GetCellTopLeftCoordinates (worksheet, cell, left, position.r);
            bool contained = bounds.Contains (picture.Left, picture.Top) || (picture.Left == bounds.Left && picture.Top == bounds.Top);
            return contained;
        }
        static void GetTopPositions(IWorksheet worksheet)
        {
            int top = 0;
            topPositions = new Dictionary<int, int> ();
            topPositions.Add (1, 0);

            //Get top and left position:

            for (int i = 1; i <= maxQuestions; i++)
            {
                try
                {
                    top += worksheet.GetRowHeightInPixels (i);
                    topPositions.Add (i + 1, top);
                }
                catch
                {
                    break;
                }
            }
        }
        public static Dictionary<int,int> topPositions;
        static Rect GetCellTopLeftCoordinates (IWorksheet worksheet, IRange cell, int left, int row)
        {
            int top = topPositions[row];
            int width = worksheet.GetColumnWidthInPixels (cell.Column);
            int height = worksheet.GetRowHeightInPixels (cell.Row);

            return new Rect (left - marginOfError, top - marginOfError, left + width - marginOfError, top + height - marginOfError);
        }
    }
}