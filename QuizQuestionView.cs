using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace ExamApp
{
    public class QuizQuestionView : View
    {
        public QuizQuestionView (Context context, IAttributeSet attrs) :
            base (context, attrs)
        {
            Initialize ();
        }

        public QuizQuestionView (Context context, IAttributeSet attrs, int defStyle) :
            base (context, attrs, defStyle)
        {
            Initialize ();
        }

        private void Initialize ()
        {
            questionText = new Paint (PaintFlags.AntiAlias);
            questionText.TextSize = DensityScaler.ToScale (this, 30f);

            answerText = new Paint (PaintFlags.AntiAlias);
            answerText.TextSize = DensityScaler.ToScale (this, 25f);

            backgroundPaint = new Paint ();
            backgroundPaint.Color = Color.WhiteSmoke;

            boxPaint = new Paint ();
            boxPaint.Color = Color.White;

            questionBoxPaint = new Paint ();
            questionBoxPaint.Color = Color.ParseColor ("#D5D8DC");

            selectedAnswerBoxPaint = new Paint (PaintFlags.AntiAlias);
            selectedAnswerBoxPaint.Color = Color.ParseColor ("#AADFFF");

            rightAnswerBoxPaint = new Paint (PaintFlags.AntiAlias);
            rightAnswerBoxPaint.Color = Color.ParseColor ("#C1FFAA");

            rightAnswerTextPaint = new Paint (PaintFlags.AntiAlias);
            rightAnswerTextPaint.Color = Color.ParseColor ("#F1FFF4");
            rightAnswerTextPaint.TextSize = answerText.TextSize;

            wrongAnswerBoxPaint = new Paint (PaintFlags.AntiAlias);
            wrongAnswerBoxPaint.Color = Color.ParseColor ("#FF9494");

            wrongAnswerTextPaint = new Paint (PaintFlags.AntiAlias);
            wrongAnswerTextPaint.Color = Color.ParseColor ("#FFF1F1");
            wrongAnswerTextPaint.TextSize = answerText.TextSize;

            imagePaint = new Paint (PaintFlags.AntiAlias);

            if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
            {
                wrongIcon = Drawable.CreateFromXml (Resources, Resources.GetXml (Resource.Drawable.cancel));
            }
            else
            {
                wrongIcon = Resources.GetDrawable (Resource.Drawable.cancel);
            }
            wrongIcon.SetColorFilter (Color.DarkRed, PorterDuff.Mode.SrcAtop);

            if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
            {
                correctIcon = Drawable.CreateFromXml (Resources, Resources.GetXml (Resource.Drawable.check));
            }
            else
            {
                correctIcon = Resources.GetDrawable (Resource.Drawable.check);
            }
        correctIcon.SetColorFilter (Color.DarkOliveGreen, PorterDuff.Mode.SrcAtop);

            Touch += (sender, args) =>
            {
                if (args.Event.Action == MotionEventActions.Down)
                {
                    if (zoomed)
                    {
                        zoomed = false;
                        Invalidate ();
                        return;
                    }
                    else
                    {
                        if (imageBox.Contains ((int)args.Event.GetX (), (int)args.Event.GetY ()))
                        {
                            zoomed = true;
                            Invalidate ();
                            return;
                        }
                    }
                    if (showAnswers || hasSelectedAnswer)
                    {
                        return;
                    }

                    for (int i = 0; i < answerOptions.Count; i++)
                    {
                        Rect rect = answerOptions[i];
                        if (rect.Contains ((int)args.Event.GetX (), (int)args.Event.GetY ()))
                        {
                            hasSelectedAnswer = true;
                            selectedAnswer = potentialAnswers[i];
                            answer = selectedAnswer;
                            selectCallback?.Invoke ();
                            Invalidate ();
                            return;
                        }
                    }
                }
            };
        }
        bool zoomed;
        Drawable wrongIcon;
        Drawable correctIcon;
        public Action selectCallback;

        int sizeWidth;
        int sizeHeight;
        int padding;
        int textPad;

        Rect background;
        Rect questionBox;
        Rect imageBox;
        List<Rect> answerOptions;

        Paint questionText;
        Paint boxPaint;
        Paint answerText;
        Paint backgroundPaint;
        Paint questionBoxPaint;
        Paint selectedAnswerBoxPaint;
        Paint rightAnswerBoxPaint;
        Paint wrongAnswerBoxPaint;
        Paint rightAnswerTextPaint;
        Paint wrongAnswerTextPaint;
        Paint imagePaint;
        int betweenPad;

        protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
            sizeWidth = ResolveSizeAndState (PaddingLeft + PaddingRight + SuggestedMinimumWidth, widthMeasureSpec, 0);
            sizeHeight = ResolveSizeAndState (PaddingTop + PaddingBottom + SuggestedMinimumWidth, heightMeasureSpec, 0);

            SetMeasuredDimension (sizeWidth, sizeHeight);
            padding = DensityScaler.ToScale (this, 15);
            textPad = DensityScaler.ToScale (this, 3);
            betweenPad = DensityScaler.ToScale (this, 10);

            background = new Rect (padding, padding, sizeWidth - padding, sizeHeight - padding);
            questionBox = new Rect (background.Left + padding, background.Top + padding, background.Right - padding, (background.Top + padding) + (int)Math.Round (sizeHeight / 1.65f));

            int dim = (int)Math.Round (questionBox.Height () / 1.25f) + (int)Math.Round (questionText.Ascent (), MidpointRounding.AwayFromZero);
            imageBox = new Rect (0, 0, dim, dim);
            imageBox.OffsetTo (questionBox.CenterX () - (imageBox.Width () / 2), questionBox.CenterY () - (imageBox.Height () / 2) - (int)Math.Round (questionText.Ascent (), MidpointRounding.AwayFromZero));
            //answerOptions = new Rect (background.Left+padding, 0, background.Width () - questionBox.Width (), background.Height () - questionBox.Height ());
            answerOptions = new List<Rect> ();
            int answerHeight = (background.Height () - questionBox.Height () - padding - (betweenPad * 4)) / 4;
            for (int i = 0; i < 4; i++)
            {
                answerOptions.Add (new Rect (background.Left + padding, questionBox.Bottom + answerHeight * i + betweenPad, background.Right - padding, questionBox.Bottom + answerHeight + answerHeight * i));
            }
            if (image != null)
            {
                image = Resized (image, imageBox.Width ());
            }
            zoomedRect = new Rect (0, 0, sizeWidth, sizeWidth);
            zoomedRect.OffsetTo (0, (sizeHeight - sizeWidth) / 2);
        }
        Rect zoomedRect;
        public bool hasSelectedAnswer;
        public string selectedAnswer;
        public void SetQuizQuestion (Question value,bool shuffle = true)
        {
            showAnswers = false;
            question = value;
            hasSelectedAnswer = false;
            selectedAnswer = string.Empty;
            potentialAnswers = new List<string> ()
            {
                question.rightAnswer,
                question.wrongAnswers[0],
                question.wrongAnswers[1],
                question.wrongAnswers[2],
            };
            if (shuffle)
            {
                potentialAnswers.Shuffle ();
            }

            image = value.bitmap != null ? BitmapFactory.DecodeByteArray (value.bitmap.data, 0, value.bitmap.data.Length) : null;
            if (imageBox == null)
            {
                RequestLayout ();
            }
            else
            {
                if (image != null)
                {
                    image = Resized (image, imageBox.Width ());
                }
                Invalidate ();
            }
        }
        public bool showAnswers = false;
        public string answer = string.Empty;
        public void SetQuizQuestionSummary (Question value, string answer)
        {
            SetQuizQuestion (value, false);
            showAnswers = true;
            this.answer = answer;
        }
        Question question;
        List<string> potentialAnswers;
        Bitmap image;
        Matrix imageMatrix;

        protected override void OnDraw (Canvas canvas)
        {
            base.OnDraw (canvas);
            try
            {
                float pad = DensityScaler.ToScale (this, 5f);
                PaintBrush.DrawShadow (background, canvas);
                canvas.DrawRect (background, backgroundPaint);


                if (question != null)
                {
                    canvas.DrawRect (questionBox, questionBoxPaint);
                    PaintBrush.DrawBounds (questionBox, canvas, questionText);
                    if (image != null)
                    {
                        imageMatrix = new Matrix ();
                        imageMatrix.SetRotate (0);
                        int left = Math.Abs (image.Width - imageBox.Width ()) / 2;
                        int top = Math.Abs (image.Height - imageBox.Height ()) / 2;
                        imageMatrix.SetTranslate (imageBox.Left + left, imageBox.Top + top);

                        PaintBrush.DrawShadow (imageBox, canvas);
                        canvas.DrawRect (imageBox, boxPaint);
                        canvas.DrawBitmap (image, imageMatrix, imagePaint);
                        PaintBrush.DrawBounds (imageBox, canvas, questionText);
                        canvas.DrawText (question.question, questionBox.CenterX () - (questionText.MeasureText (question.question) / 2), imageBox.Top + questionText.Ascent (), questionText);

                    }
                    else
                    {
                        canvas.DrawText (question.question, questionBox.CenterX () - (questionText.MeasureText (question.question) / 2), questionBox.CenterY () - (questionText.Ascent () / 2), questionText);
                    }

                    int icSize = DensityScaler.ToScale (this, 60);
                    for (int i = 0; i < potentialAnswers.Count; i++)
                    {
                        bool renderWrong = false;
                        bool renderCorrect = false;
                        Paint _boxPaint = (potentialAnswers[i] == selectedAnswer ? selectedAnswerBoxPaint : boxPaint);
                        Paint _textPaint = answerText;
                        if (showAnswers)
                        {
                            if (potentialAnswers[i] == question.rightAnswer)
                            {
                                _boxPaint = rightAnswerBoxPaint;
                                _textPaint = rightAnswerTextPaint;
                                correctIcon.SetBounds (answerOptions[i].Right - icSize, answerOptions[i].CenterY () - (icSize / 2), answerOptions[i].Right, answerOptions[i].CenterY () + (icSize / 2));
                                renderCorrect = true;
                            }
                            else if (answer != null)
                            {
                                if (potentialAnswers[i] == answer && potentialAnswers[i] != question.rightAnswer)
                                {
                                    _boxPaint = wrongAnswerBoxPaint;
                                    _textPaint = wrongAnswerTextPaint;
                                    wrongIcon.SetBounds (answerOptions[i].Right - icSize, answerOptions[i].CenterY () - (icSize / 2), answerOptions[i].Right, answerOptions[i].CenterY () + (icSize / 2));
                                    renderWrong = true;
                                }
                            }
                            else
                            {
                                canvas.DrawText ("Timed out", questionBox.CenterX () - wrongAnswerTextPaint.MeasureText ("Timed out"), questionBox.Bottom - (textPad / 2), wrongAnswerTextPaint);
                            }
                        }
                        PaintBrush.DrawShadow (answerOptions[i], canvas);
                        canvas.DrawRect (answerOptions[i], _boxPaint);
                        PaintBrush.DrawBounds (answerOptions[i], canvas, questionText);

                        if (renderCorrect)
                        {
                            correctIcon.Draw (canvas);
                        }
                        else
                        if (renderWrong)
                        {
                            wrongIcon.Draw (canvas);
                        }
                        List<string> lines = Broken (answerOptions[i].Width (), potentialAnswers[i], _textPaint);
                        if (lines.Count > 1)
                        {
                            float size = _textPaint.TextSize;
                            _textPaint.TextSize /= lines.Count;
                            float ascent = -_textPaint.Ascent ();
                            ascent += pad;
                            float _top = Math.Clamp (answerOptions[i].Height () - (ascent * lines.Count), 0, float.MaxValue);
                            for (int n = 0; n < lines.Count; n++)
                            {
                                string str = lines[n];
                                canvas.DrawText (str, answerOptions[i].CenterX () - (_textPaint.MeasureText (str) / 2), answerOptions[i].Top + _top + ascent * n, _textPaint);
                            }
                            _textPaint.TextSize = size;
                        }
                        else
                        {
                            canvas.DrawText (potentialAnswers[i], answerOptions[i].CenterX () - (_textPaint.MeasureText (potentialAnswers[i]) / 2), answerOptions[i].CenterY () - (_textPaint.Ascent () / 2), _textPaint);
                        }
                    }
                    if (zoomed)
                    {
                        Bitmap bitmap = question.bitmap != null ? BitmapFactory.DecodeByteArray (question.bitmap.data, 0, question.bitmap.data.Length) : null;
                        if (bitmap != null)
                        {
                            bitmap = Resized (bitmap, zoomedRect.Width ());
                            Matrix imageMatrix = new Matrix ();
                            imageMatrix.SetRotate (0);
                            int left = Math.Abs (bitmap.Width - zoomedRect.Width ()) / 2;
                            int top = Math.Abs (bitmap.Height - zoomedRect.Height ()) / 2;
                            imageMatrix.SetTranslate (zoomedRect.Left + left, zoomedRect.Top + top);
                            canvas.DrawBitmap (bitmap, imageMatrix, imagePaint);
                        }
                    }
                }
            }
            catch { }
        }
        public Bitmap Resized (Bitmap image, int maxSize)
        {
            int width = image.Width;
            int height = image.Height;

            float bitmapRatio = (float)width / height;
            if (bitmapRatio > 1)
            {
                width = maxSize;
                height = (int)(width / bitmapRatio);
            }
            else
            {
                height = maxSize;
                width = (int)(height * bitmapRatio);
            }
            return Bitmap.CreateScaledBitmap (image, width, height, true);
        }
        public static List<string> Broken (float maxWidth, string text, Paint textPaint)
        {
            List<string> result = new List<string> ();
            float measurement = textPaint.MeasureText (text, 0, text.Length);
            float[] measurements = new float[] { measurement };
            if (maxWidth < 1)
                return new List<string> () { text };
            if (measurement > maxWidth)
            {
                string current = text;
                string lastCurrent = string.Empty;

                while (textPaint.MeasureText (current) > maxWidth && current != lastCurrent)
                {
                    int breakPoint = textPaint.BreakText (current, true, maxWidth, measurements);
                    //Removing by space
                    bool foundSpace = false;
                    if (breakPoint < current.Length)
                    {
                        for (int i = breakPoint; i >= 0; i--)
                        {
                            if (current[i] == ' ' || current[i] == '\n'||current[i]=='_')
                            {
                                foundSpace = true;
                                result.Add (current.Remove (i).Trim ());
                                string _new = string.Empty;
                                for (int x = i; x < current.Length; x++)
                                {
                                    _new += current[x];
                                }
                                lastCurrent = current;
                                current = _new;
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (!foundSpace && breakPoint < current.Length)
                    {
                        result.Add (current.Remove (breakPoint));
                        string _new = string.Empty;
                        for (int x = breakPoint; x < current.Length; x++)
                        {
                            _new += current[x];
                        }
                        current = _new;
                    }
                }
                result.Add (current.Trim ());
            }
            else
            {
                result.Clear ();
                result.Add (text);
            }
            return result;
        }
    }
    public static class ListCompat
    {
        private static Random rng = new Random ();

        public static void Shuffle<T> (this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next (n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    public static class DensityScaler
    {
        const float divisor = 320f;
        public static float ScaleValue (View view)
        {
            return ((int)view.Resources.DisplayMetrics.DensityDpi) / divisor;
        }
        public static float ToScale (View view, float value)
        {
            return ((int)view.Resources.DisplayMetrics.DensityDpi) / divisor * value;
        }
        public static int ToScale (View view, int value)
        {
            return (int)MathF.Round ((int)view.Resources.DisplayMetrics.DensityDpi / divisor * value);
        }
    }
    public static class PaintBrush
    {
        static Paint shadow;
        static Paint shadowLine;
        static Paint shadowLineDark;
        static PaintBrush ()
        {
            Init ();
        }
        static void Init ()
        {
            shadow = new Paint (PaintFlags.AntiAlias);

            shadowLine = new Paint (PaintFlags.AntiAlias);
            shadowLine.StrokeWidth = 1.5f;
            shadowLineDark = new Paint (PaintFlags.AntiAlias);
            shadowLineDark.StrokeWidth = 1.5f;

            shadow.SetMaskFilter (new BlurMaskFilter (12, BlurMaskFilter.Blur.Normal));
            shadowLine.SetMaskFilter (new BlurMaskFilter (8, BlurMaskFilter.Blur.Normal));
            shadowLineDark.SetMaskFilter (new BlurMaskFilter (8, BlurMaskFilter.Blur.Normal));

            shadow.SetARGB (0x80, 0x80, 0x80, 0x80);
            shadowLine.SetARGB (0xdd, 0x15, 0x15, 0x15);
            shadowLineDark.SetARGB (0xdd, 0x08, 0x08, 0x08);
        }
        public static void DrawBounds (Rect of, Canvas to, Paint with)
        {
            to.DrawLine (of.Left, of.Top, of.Right, of.Top, with);
            to.DrawLine (of.Left, of.Bottom, of.Right, of.Bottom, with);
            to.DrawLine (of.Left, of.Top, of.Left, of.Bottom, with);
            to.DrawLine (of.Right, of.Top, of.Right, of.Bottom, with);
        }
        public static void DrawBounds (RectF of, Canvas to, Paint with)
        {
            to.DrawLine (of.Left, of.Top, of.Right, of.Top, with);
            to.DrawLine (of.Left, of.Bottom, of.Right, of.Bottom, with);
            to.DrawLine (of.Left, of.Top, of.Left, of.Bottom, with);
            to.DrawLine (of.Right, of.Top, of.Right, of.Bottom, with);
        }
        public static void DrawBounds (Rect of, Canvas to, Paint with, bool top, bool bottom, bool left, bool right)
        {
            if (top)
            {
                to.DrawLine (of.Left, of.Top, of.Right, of.Top, with);
            }

            if (bottom)
            {
                to.DrawLine (of.Left, of.Bottom, of.Right, of.Bottom, with);
            }

            if (left)
            {
                to.DrawLine (of.Left, of.Top, of.Left, of.Bottom, with);
            }

            if (right)
            {
                to.DrawLine (of.Right, of.Top, of.Right, of.Bottom, with);
            }
        }
        public static void DrawBounds (RectF of, Canvas to, Paint with, bool top, bool bottom, bool left, bool right)
        {

            if (top)
            {
                to.DrawLine (of.Left, of.Top, of.Right, of.Top, with);
            }

            if (bottom)
            {
                to.DrawLine (of.Left, of.Bottom, of.Right, of.Bottom, with);
            }

            if (left)
            {
                to.DrawLine (of.Left, of.Top, of.Left, of.Bottom, with);
            }

            if (right)
            {
                to.DrawLine (of.Right, of.Top, of.Right, of.Bottom, with);
            }
        }
        public static void OutLine (RectF target, Canvas on, Paint with, float factor, float rounding, bool shadow = false)
        {
            try
            {
                factor += 1;
                RectF bounds = new RectF (0, 0, target.Width () * factor, target.Height () * factor);
                bounds.OffsetTo (target.Left - ((bounds.Width () - target.Width ()) / 2), target.Top - ((bounds.Height () - target.Height ()) / 2));
                if (shadow) DrawShadow (bounds, on);
                on.DrawRoundRect (bounds, rounding, rounding, with);
            }
            catch
            {
                Init ();
            }
        }
        public static void DrawShadow (Rect of, Canvas to)
        {
            try
            {
                to.DrawRect (of, shadow);
            }
            catch
            {
                Init ();
            }
        }
        public static void DrawShadow (RectF of, Canvas to, float rounding = 5)
        {
            try
            {
                to.DrawRoundRect (of, rounding, rounding, shadow);
            }
            catch
            {
                Init ();
            }
        }
        public static void DrawShadowAround (Rect target, Canvas to)
        {
            try
            {
                DrawBounds (target, to, shadow);
            }
            catch
            {
                Init ();
            }
        }
        public static void DrawShadowAround (RectF target, Canvas to)
        {
            try
            {
                DrawBounds (target, to, shadowLine);
            }
            catch
            {
                Init ();
            }
        }
        public static void DrawShadowUnder (Rect target, Canvas to)
        {
            try
            {
                DrawBounds (target, to, shadowLineDark, false, true, false, true);
            }
            catch
            {
                Init ();
            }
        }
    }
}
