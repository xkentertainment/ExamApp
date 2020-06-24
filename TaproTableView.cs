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

namespace ExamApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Android.Animation;
    using Android.App;
    using Android.Content;
    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.OS;
    using Android.Runtime;
    using Android.Text;
    using Android.Util;
    using Android.Views;
    using Android.Views.InputMethods;
    using Android.Widget;
    using Java.Lang;

    namespace SMAS.Views
    {
        public class TaproTableView : View, ValueAnimator.IAnimatorUpdateListener
        {

            public string firstRow = "1";
            public string secondRow = "2";
            public string thirdRow = "3";
            public string fourthRow = "69";
            public string title = "Tapro Table View";
            public int size { get; private set; }
            public int limit { get; private set; }
            public bool showNavigation { get; set; }
            public bool showLeftNav { get; set; }
            public bool showRightNav { get; set; }
            public bool actionsShown { get; set; }
            public string pageIndicator;
            //ValueAnimator showActionAnimator;
            //ValueAnimator hideActionAnimator;
            public ITableEventListener eventListener { get; private set; }

            public void SetListener (ITableEventListener listener)
            {
                eventListener = listener;
            }
            public Dictionary<string, List<string>> Data
            {
                get => _data;
                set
                {
                    _data = value;

                    if (!_data.ContainsKey (secondRow))
                        renderSecondRow = false;
                    if (!_data.ContainsKey (thirdRow))
                        renderThirdRow = false;
                    if (!_data.ContainsKey (fourthRow))
                        renderFourthRow = false;
                    eventListener?.TableUpdated ();
                    Invalidate ();
                }
            }
            public Dictionary<int, List<int>> actionRows;
            public Dictionary<string, List<KeyValuePair<int, string>>> highlighter;
            public Dictionary<int, ValueAnimator> showAnimators;
            public Dictionary<int, ValueAnimator> hideAnimators;
            public Dictionary<TableAction, Action<int>> Actions
            {
                get => _actions;
                set
                {
                    _actions = value;
                    actionButtonPaints = new List<Paint> ();
                    showAnimators = new Dictionary<int, ValueAnimator> ();
                    hideAnimators = new Dictionary<int, ValueAnimator> ();
                    int actionCount = value.Count;
                    if (Data != null && Data.Count > 0)
                    {

                        for (int i = 0; i < _actions.Count; i++)
                        {
                            actionButtonPaints.Add (new Paint (PaintFlags.AntiAlias));
                            ResolveActionColor (_actions.Keys.ToArray ()[i], actionButtonPaints[i]);
                        }
                        for (int n = 0; n < Data[firstRow].Count; n++)
                        {
                            if (actionRows != null && actionRows.Count > 0 && n < actionRows.Count && actionRows.ContainsKey (n))
                            {
                                actionCount = actionRows[n].Count;
                            }
                            ValueAnimator showActionAnimator = ValueAnimator.OfFloat (0f, actionButtonSize * actionCount);
                            showActionAnimator.SetDuration (200);
                            ValueAnimator hideActionAnimator = ValueAnimator.OfFloat (actionButtonSize * actionCount, 0f);
                            hideActionAnimator.SetDuration (200);

                            showActionAnimator.Update += (sender, args) =>
                            {
                                actionButtonAnimatedValue = (float)args.Animation.AnimatedValue;
                                Invalidate ();
                            };
                            hideActionAnimator.Update += (sender, args) =>
                            {
                                actionButtonAnimatedValue = (float)args.Animation.AnimatedValue;
                                Invalidate ();
                            };
                            showAnimators.Add (n, showActionAnimator);
                            hideAnimators.Add (n, hideActionAnimator);
                        }
                    }
                }
            }
            public static void ResolveActionColor (TableAction action, Paint paint)
            {
                switch (action)
                {
                    case TableAction.View:
                        paint.SetARGB (0xff, 0x41, 0x61, 0xe1);
                        break;
                    case TableAction.Delete:
                        paint.SetARGB (0xff, 0xff, 0x15, 0x15);
                        break;
                    case TableAction.Edit:
                        paint.SetARGB (0xff, 0xff, 0xa5, 0x00);
                        break;
                    case TableAction.Refresh:
                        paint.SetARGB (0xff, 0x41, 0x61, 0xe1);
                        break;
                    case TableAction.Undo:
                        paint.SetARGB (0xff, 0x4C, 0xF9, 0x80);
                        break;
                    case TableAction.Check:
                        paint.SetARGB (0xff, 0x4A, 0xD5, 0x7B);
                        break;
                    default:
                        paint.SetARGB (0xff, 0x80, 0x80, 0x80);
                        break;
                }
            }
            Dictionary<TableAction, Action<int>> _actions;
            Dictionary<string, List<string>> _data;
            protected TaproTableView (IntPtr intPtr, JniHandleOwnership ownership) : base (intPtr, ownership)
            {

            }
            public TaproTableView (Context context) :
                base (context)
            {
                Create (context);
            }
            public TaproTableView (Context context, IAttributeSet attrs) : base (context, attrs)
            {
                Create (context, attrs);
            }
            bool renderSecondRow = true;
            bool renderThirdRow = true;
            bool renderFourthRow = true;

            Paint rowBackGround;
            Paint navButtonPaint;
            Paint divisionPaint;
            Paint titlePaint;
            Paint itemPaint;
            Paint navPagePaint;
            Paint badgePaint;
            Paint whiteItemPaint;
            Paint tableTitlePaint;
            Paint searchLinePaint;
            Paint searchTextPaint;
            List<Paint> actionButtonPaints;

            Rect itemRow;
            Rect divisionLine;
            RectF navButtons;
            Rect actionButtons;
            RectF badges;
            Rect titleRow;
            Rect searchIconRect;
            Rect dropDownIconRect;
            Rect dropDownRect;

            Drawable prevButton;
            Drawable nextButton;

            Drawable editAction;
            Drawable viewAction;
            Drawable deleteAction;
            Drawable infoAction;
            Drawable refreshAction;
            Drawable checkAction;
            Drawable undoAction;
            Drawable searchIcon;
            Drawable dropDownIcon;

            readonly bool debug = false;
            public void Create (Context context, IAttributeSet attrs = null)
            {
                lastTouchedIndex = -1;
                searchBuilder = new SpannableStringBuilder ();
                FocusableInTouchMode = true;
                searchString = string.Empty;
                if (attrs != null)
                {
                    showNavigation = true;
                    showLeftNav = true;
                    showRightNav = true;


                    //initialize paints
                    titlePaint = new Paint (PaintFlags.AntiAlias);
                    itemPaint = new Paint (PaintFlags.AntiAlias);
                    navButtonPaint = new Paint (PaintFlags.AntiAlias);
                    navPagePaint = new Paint (PaintFlags.AntiAlias);
                    rowBackGround = new Paint (PaintFlags.AntiAlias);
                    divisionPaint = new Paint (PaintFlags.AntiAlias);
                    badgePaint = new Paint (PaintFlags.AntiAlias);
                    whiteItemPaint = new Paint (itemPaint);
                    actionButtonPaints = new List<Paint> ();
                    tableTitlePaint = new Paint (PaintFlags.AntiAlias);
                    searchLinePaint = new Paint (PaintFlags.AntiAlias);
                    searchTextPaint = new Paint (PaintFlags.AntiAlias);
                    dropDownPaint = new Paint (PaintFlags.AntiAlias);

                    //set colors
                    divisionPaint.SetARGB (0xff, 0xb2, 0xb2, 0xb2);
                    rowBackGround.SetARGB (0xff, 0xf4, 0xf4, 0xf4);
                    navButtonPaint.SetARGB (0xff, 0x15, 0x65, 0xc0);
                    titlePaint.SetARGB (0xff, 0x3b, 0x3b, 0x3b);
                    itemPaint.SetARGB (0xff, 0x4a, 0x4a, 0x4a);
                    whiteItemPaint.SetARGB (0xff, 0xff, 0xff, 0xff);
                    searchLinePaint.Color = Color.DarkGray;
                    dropDownPaint.Color = Color.White;


                    //set text sizes for titles and items
                    TaproTableView thisView = this;
                    thisView.titlePaint.TextSize *= 1.95f * DensityScaler.ScaleValue (thisView);
                    thisView.itemPaint.TextSize *= 2.1f * DensityScaler.ScaleValue (thisView);
                    thisView.whiteItemPaint.TextSize *= 1.8f * DensityScaler.ScaleValue (thisView);
                    thisView.navPagePaint.TextSize *= 1.8f * DensityScaler.ScaleValue (thisView);
                    thisView.searchTextPaint.TextSize *= 2.5f * DensityScaler.ScaleValue (thisView);
                    thisView.tableTitlePaint.TextSize *= 3.25f * DensityScaler.ScaleValue (thisView);

                    thisView.divisionPaint.StrokeWidth = DensityScaler.ToScale (thisView, 4f);
                    searchLinePaint.StrokeWidth = DensityScaler.ToScale (thisView, 3f);

                    Touch += TouchEvent;
                    KeyPress += KeyPressed;

                    prevButton = Drawable.CreateFromXml (Resources, Resources.GetXml (Resource.Drawable.ic_prev_button));
                    nextButton = Drawable.CreateFromXml (Resources, Resources.GetXml (Resource.Drawable.ic_next_button));
                }
            }

            private void KeyPressed (object sender, KeyEventArgs e)
            {
                if (e.Event.Action == KeyEventActions.Up)
                {
                    if (searchString.Length > 0 && e.KeyCode == Keycode.Del)
                    {
                        searchString = searchString.Remove (searchString.Length - 1);
                    }
                    else if (e.KeyCode != Keycode.Del)
                    {
                        searchString += (char)e.Event.UnicodeChar;
                    }
                    else
                    {
                        return;
                    }
                    Task.Run (() =>
                    {
                        actionsShown = false;
                        eventListener?.SearchUpdated (searchString);
                    });
                    Invalidate ();
                }
            }

            public override IInputConnection OnCreateInputConnection (EditorInfo outAttrs)
            {
                KeyboardConnection baseInputConnection = new KeyboardConnection (this, false, searchBuilder);
                outAttrs.ActionLabel = null;
                outAttrs.InputType = InputTypes.Null;
                outAttrs.ImeOptions = ImeFlags.NoEnterAction;
                return baseInputConnection;
            }
            SpannableStringBuilder searchBuilder;

            protected override void OnVisibilityChanged (View changedView, [GeneratedEnum] ViewStates visibility)
            {
                if (this.Parent == changedView)
                {
                    if (visibility == ViewStates.Gone || visibility == ViewStates.Invisible)
                    {
                        actionsShown = false;
                        searchBuilder = new SpannableStringBuilder ();
                        _showSearch = false;

                    }
                    else
                    {
                        lastTouchedIndex = -1;
                        eventListener?.SetPageLimit ();
                    }
                }
                base.OnVisibilityChanged (changedView, visibility);
            }
            bool allowTouches = true;
            int lastTouchedIndex;
            int lastActionShownIndex;
            public bool canSearch;
            bool moved;
            private void TouchEvent (object sender, TouchEventArgs e)
            {
                if (Visibility != ViewStates.Visible)
                    return;
                if (e.Event.Action == MotionEventActions.Move)
                {
                    moved = true;
                }
                if (allowTouches && e.Event.Action == MotionEventActions.Up)
                {
                    if (moved)
                    {
                        moved = false;
                        return;
                    }
                    if (showDropDown)
                    {
                        if (dropDownRect.Contains ((int)e.Event.GetX (), (int)e.Event.GetY ()))
                        {
                            DropDownAction (e.Event.GetY ());
                            showDropDown = false;
                            return;
                        }
                        showDropDown = false;
                        Invalidate ();
                        return;
                    }
                    lastTouchedIndex = Touched (e.Event.GetX (), e.Event.GetY ());
                    if (lastTouchedIndex >= 0)
                    {
                        eventListener?.ItemSelected (lastTouchedIndex);
                        if (_actions != null)
                        {
                            ShowActions (e.Event.GetX (), e.Event.GetY ());
                        }
                    }
                }
            }
            void DropDownAction (float y)
            {
                string[] vals = dropDownActions.Values.ToArray ();
                for (int i = 0; i <= vals.Length; i++)
                {
                    if (y < dropDownRect.Top + (i * dropDownItemSize))
                    {
                        eventListener?.DropDownActionClicked (vals[i - 1]);
                        return;
                    }
                }
            }
            public void ShowActions (float x, float y)
            {
                if (!actionsShown)
                {
                    if (_data != null && _data.Count > 0 && lastTouchedIndex < _data[firstRow].Count)
                    {
                        actionsShown = true;
                    }
                    if (lastTouchedIndex < showAnimators.Count)
                        showAnimators[lastTouchedIndex].Start ();
                    Invalidate ();
                    lastActionShownIndex = lastTouchedIndex;
                }
                else
                {
                    if (lastActionShownIndex != lastTouchedIndex)
                    {
                        if (hideAnimators.ContainsKey (lastActionShownIndex))
                        {
                            allowTouches = false;
                            hideAnimators[lastActionShownIndex].Start ();
                            hideAnimators[lastActionShownIndex].AnimationEnd += AnimationEnd;
                        }
                    }
                    else
                    {
                        if (ClickedAction (x, y) >= 0)
                        {
                            _actions[_actions.Keys.ToArray ()[ClickedAction (x, y)]].Invoke (lastActionShownIndex);
                        }
                    }
                }
            }
            public void ShowActions ()
            {
                if (!actionsShown)
                {
                    if (_data != null && _data.Count > 0 && lastTouchedIndex < _data[firstRow].Count)
                    {
                        actionsShown = true;
                    }
                    if (lastActionShownIndex < showAnimators.Count)
                    {
                        showAnimators[lastActionShownIndex].Start ();
                    }

                    Invalidate ();
                    lastActionShownIndex = lastTouchedIndex;
                }
                else
                {
                    if (lastActionShownIndex != lastTouchedIndex)
                    {
                        allowTouches = false;
                        hideAnimators[lastActionShownIndex].Start ();
                        hideAnimators[lastActionShownIndex].AnimationEnd += AnimationEnd;
                    }
                }
            }

            private void AnimationEnd (object sender, EventArgs e)
            {
                hideAnimators[lastActionShownIndex].AnimationEnd -= AnimationEnd;
                lastActionShownIndex = lastTouchedIndex;
                actionsShown = false;
                allowTouches = true;
                ShowActions ();
            }

            public void DismissActions ()
            {
                if (actionsShown)
                {
                    allowTouches = false;
                    hideAnimators[lastActionShownIndex].AnimationEnd += DismissActionEnd;
                    hideAnimators[lastActionShownIndex].Start ();
                    Invalidate ();
                }
            }

            private void DismissActionEnd (object sender, EventArgs e)
            {
                hideAnimators[lastActionShownIndex].AnimationEnd -= DismissActionEnd;
                actionsShown = false;
                allowTouches = true;
            }

            int sizeHeight;
            int sizeWidth;
            int rowHeight;
            int navButtonSize;
            int actionButtonSize;
            int navPageWidth;
            int navPosition;
            int tableTitleBoxHeight;
            float badgePad;
            int iconPad;
            int dropDownItemSize;
            protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure (widthMeasureSpec, heightMeasureSpec);

                rowHeight = DensityScaler.ToScale (this, 70);
                tableTitleBoxHeight = (int)MathF.Round (rowHeight * 1.5f);
                actionButtonSize = rowHeight;
                navButtonSize = rowHeight;
                navPageWidth = DensityScaler.ToScale (this, 200);
                iconPad = DensityScaler.ToScale (this, 15);
                dropDownItemSize = DensityScaler.ToScale (this, 100);

                badgePad = DensityScaler.ToScale (this, 4f);

                sizeWidth = ResolveSizeAndState (PaddingRight + PaddingLeft + SuggestedMinimumWidth, widthMeasureSpec, 1);
                sizeHeight = ResolveSizeAndState (PaddingTop + PaddingBottom + SuggestedMinimumHeight + navButtonSize + rowHeight + tableTitleBoxHeight + ((_data != null && _data.Count > 0 && SuggestedMinimumHeight == 0) ? _data[_data.Keys.ToArray ()[0]].Count * rowHeight : 0), heightMeasureSpec, 1);

                SetMeasuredDimension (sizeWidth, sizeHeight);

                titleRow = new Rect (pad / 2, 0, sizeWidth - pad / 2, tableTitleBoxHeight);
                titleHeight = -titlePaint.Ascent ();

                badges = new RectF (0, 0, 0, 0);

                itemRow = new Rect (pad / 2, 0, sizeWidth - pad / 2, rowHeight);

                divisionLine = new Rect (pad / 2, itemRow.Bottom, sizeWidth - pad / 2, itemRow.Bottom + 2);
                navPageWidth = sizeWidth / 10;
                navPosition = (sizeWidth / 2) - (navButtonSize + (navPageWidth));
                navButtons = new RectF (0, 0, navButtonSize, navButtonSize);


                limit = (int)MathF.Round ((sizeHeight - (navButtonSize + 5) - tableTitleBoxHeight) / rowHeight, MidpointRounding.ToEven) - 1;
                //limit = Math.Clamp (limit, 1, int.MaxValue);

                CalculateSize ();
                eventListener?.SetPageLimit ();

            }
            public void CalculateSize ()
            {
                size = sizeHeight / rowHeight;
            }
            public bool showSearch
            {
                get => _showSearch;
                set
                {
                    _showSearch = value;
                    Invalidate ();
                }
            }

            bool _showSearch;
            public string searchString;
            float titleHeight = 9;
            int pad = 10;
            //public float nameOffset = 0.0f;
            public float firstRowRatio = 0.3f;
            public float secondRowRatio = 0.25f;
            public float thirdRowRatio = 0.20f;

            float actionButtonAnimatedValue;
            protected override void OnDraw (Canvas canvas)
            {
                try
                {
                    base.OnDraw (canvas);

                    rowBackGround.SetARGB (0xff, 0xfa, 0xfa, 0xfa);
                    canvas.DrawRect (titleRow, rowBackGround);
                    canvas.DrawLine (titleRow.Left, titleRow.Top, titleRow.Right, titleRow.Top, divisionPaint);
                    canvas.DrawLine (titleRow.Left, titleRow.Bottom, titleRow.Right, titleRow.Bottom, divisionPaint);

                    if (canSearch)
                    {
                        searchIcon.Draw (canvas);
                    }

                    if (!showSearch)
                    {
                        canvas.DrawText (title, titleRow.CenterX () - (tableTitlePaint.MeasureText (title) / 2), tableTitleBoxHeight / 2 - (tableTitlePaint.Ascent () / 2), tableTitlePaint);
                        if (dropDownActions != null)
                        {
                            dropDownIcon.Draw (canvas);
                        }
                    }
                    else
                    {
                        float yp = tableTitleBoxHeight / 2 - (searchTextPaint.Ascent () / 2);
                        canvas.DrawLine (searchIconRect.Right, yp, titleRow.Right - searchIconRect.Width (), yp, searchLinePaint);
                        canvas.DrawText (searchString, searchIconRect.Right + pad, yp - (pad / 2), searchTextPaint);
                    }

                    rowBackGround.SetARGB (0xff, 0xff, 0xff, 0xff);
                    itemRow.OffsetTo (itemRow.Left, tableTitleBoxHeight);

                    float ypos = tableTitleBoxHeight + itemRow.Bottom - (itemRow.ExactCenterY () - (titleHeight / 2));
                    canvas.DrawRect (itemRow, rowBackGround);

                    //Draw Titles
                    canvas.DrawText (_data != null && _data.Count > 0 ? _data.Keys.ToArray ()[0].ToUpper () : "", pad, ypos, titlePaint);

                    float secondPad = pad + (sizeWidth * firstRowRatio);
                    float thirdPad = pad + (sizeWidth * (secondRowRatio + firstRowRatio));
                    float fourthPad = pad + (sizeWidth * (thirdRowRatio + secondRowRatio + firstRowRatio));

                    //change offset if column missing
                    if (!renderThirdRow && !renderSecondRow && renderFourthRow)
                    {
                        fourthPad += sizeWidth * secondRowRatio / 4;
                    }

                    //Render titles
                    if (renderSecondRow)
                    {
                        canvas.DrawText (_data != null && _data.Count > 0 ? _data.Keys.ToArray ()[1].ToUpper () : "", secondPad, ypos, titlePaint);
                    }

                    if (renderThirdRow)
                    {
                        canvas.DrawText (_data != null && _data.Count > 0 ? _data.Keys.ToArray ()[2].ToUpper () : "", thirdPad, ypos, titlePaint);
                    }

                    if (renderFourthRow)
                    {
                        canvas.DrawText (_data != null && _data.Count > 0 ? _data.Keys.ToArray ()[3].ToUpper () : "", fourthPad, ypos, titlePaint);
                    }



                    if (_data != null)
                    {
                        bool even = false;
                        if (_data.ContainsKey (firstRow))
                        {
                            for (int i = 0; i < _data[firstRow].Count; i++)
                            {
                                //color changes of rowBG
                                if (even)
                                {
                                    rowBackGround.SetARGB (0xff, 0xfe, 0xfe, 0xfe);
                                }
                                else
                                {
                                    rowBackGround.SetARGB (0xff, 0xfc, 0xfc, 0xfc);
                                }
                                //Setting offsets for each item
                                int offset = tableTitleBoxHeight + itemRow.Height () * (i + 1);
                                itemRow.OffsetTo (itemRow.Left, offset);
                                divisionLine.OffsetTo (divisionLine.Left, offset);
                                ypos = (itemRow.Bottom + offset) - (itemRow.ExactCenterY () - (-itemPaint.Ascent () / 2));
                                canvas.DrawRect (itemRow, rowBackGround);
                                canvas.DrawRect (divisionLine, divisionPaint);

                                //First row
                                bool highlighted = false;

                                if (highlighter.ContainsKey (firstRow))
                                {
                                    foreach (var pair in highlighter[firstRow])
                                    {
                                        if (pair.Key == i)
                                        {
                                            highlighted = true;
                                            //DrawBadge (canvas, pair.Value, ypos, pad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), itemPaint.MeasureText (_data[firstRow][i]));
                                            break;
                                        }
                                        else
                                        {
                                            highlighted = false;
                                        }
                                    }
                                }
                                canvas.DrawText (_data[firstRow][i], pad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), ypos, highlighted ? whiteItemPaint : itemPaint);

                                //render optional columns
                                if (renderSecondRow)
                                {
                                    if (_data.ContainsKey (secondRow))
                                    {
                                        highlighted = false;
                                        if (highlighter.ContainsKey (secondRow))
                                        {
                                            foreach (var pair in highlighter[secondRow])
                                            {
                                                if (pair.Key == i)
                                                {
                                                    highlighted = true;
                                                    //DrawBadge (canvas, pair.Value, ypos, secondPad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), itemPaint.MeasureText (_data[secondRow][i]));
                                                    break;
                                                }
                                                else
                                                {
                                                    highlighted = false;
                                                }
                                            }
                                        }
                                        canvas.DrawText (_data[secondRow][i], secondPad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), ypos, highlighted ? whiteItemPaint : itemPaint);
                                    }
                                    else
                                    {
                                        renderSecondRow = false;
                                    }
                                }

                                if (renderThirdRow)
                                {
                                    if (_data.ContainsKey (thirdRow))
                                    {
                                        if (highlighter.ContainsKey (thirdRow))
                                        {
                                            foreach (var pair in highlighter[thirdRow])
                                            {
                                                if (pair.Key == i)
                                                {
                                                    highlighted = true;
                                                    //DrawBadge (canvas, pair.Value, ypos, thirdPad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), itemPaint.MeasureText (_data[thirdRow][i]));
                                                    break;
                                                }
                                                else
                                                {
                                                    highlighted = false;
                                                }
                                            }
                                        }
                                        canvas.DrawText (_data[thirdRow][i], thirdPad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), ypos, highlighted ? whiteItemPaint : itemPaint);

                                    }
                                    else
                                    {
                                        renderThirdRow = false;
                                    }
                                }

                                if (renderFourthRow)
                                {
                                    if (_data.ContainsKey (fourthRow))
                                    {
                                        if (highlighter.ContainsKey (fourthRow))
                                        {
                                            foreach (var pair in highlighter[fourthRow])
                                            {
                                                if (pair.Key == i)
                                                {
                                                    highlighted = true;
                                                    //DrawBadge (canvas, pair.Value, ypos, fourthPad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), itemPaint.MeasureText (_data[fourthRow][i]));
                                                    break;
                                                }
                                                else
                                                {
                                                    highlighted = false;
                                                }
                                            }
                                        }
                                        canvas.DrawText (_data[fourthRow][i], fourthPad - (int)(actionsShown && i == lastActionShownIndex ? actionButtonAnimatedValue : 0), ypos, highlighted ? whiteItemPaint : itemPaint);
                                    }
                                    else
                                    {
                                        renderFourthRow = false;
                                    }
                                }
                                if (actionsShown && i == lastActionShownIndex)
                                {
                                    int npos = 0;
                                    for (int n = 0; n < actionButtonPaints.Count; n++)
                                    {

                                        if (actionRows.Count == 0 ? true : ((i < actionRows.Count) ? (actionRows[i].Contains (n)) : false))
                                        {
                                            actionButtons.OffsetTo ((int)(sizeWidth - actionButtonAnimatedValue + (actionButtonSize * npos) - (pad / 2)), itemRow.Top);
                                            canvas.DrawRect (actionButtons, actionButtonPaints[n]);

                                            Drawable icon;
                                            switch (_actions.Keys.ToArray ()[n])
                                            {
                                                case TableAction.Edit:
                                                    icon = editAction;
                                                    break;
                                                case TableAction.Delete:
                                                    icon = deleteAction;
                                                    break;
                                                case TableAction.View:
                                                    icon = viewAction;
                                                    break;
                                                case TableAction.Check:
                                                    icon = checkAction;
                                                    break;
                                                case TableAction.Refresh:
                                                    icon = refreshAction;
                                                    break;
                                                case TableAction.Undo:
                                                    icon = undoAction;
                                                    break;
                                                default:
                                                    icon = infoAction;
                                                    break;
                                            }
                                            //icon.SetBounds ((int)(sizeWidth - actionButtonAnimatedValue + (actionButtonSize * n)) + iconPad - (pad / 2), rowHeight * (i + 1) + iconPad, (int)(sizeWidth - actionButtonAnimatedValue + (actionButtonSize * n)) - iconPad + actionButtonSize - (pad / 2), (rowHeight * (i + 1)) + actionButtonSize - iconPad);
                                            icon.SetBounds (actionButtons.Left + iconPad, actionButtons.Top + iconPad, actionButtons.Right - iconPad, actionButtons.Bottom - iconPad);
                                            icon.SetColorFilter (Color.White, PorterDuff.Mode.Multiply);
                                            icon.Draw (canvas);
                                        }
                                        else
                                        {
                                            npos--;
                                        }
                                        npos++;
                                    }

                                }
                                even = !even;
                            }
                            divisionLine.OffsetTo (divisionLine.Left, tableTitleBoxHeight + (itemRow.Height () * (_data[firstRow].Count + 1)));
                            canvas.DrawRect (divisionLine, divisionPaint);

                        }
                        if (showNavigation)
                        {
                            if (showLeftNav)
                            {
                                navButtons.OffsetTo (navPosition - (navButtonSize / 2), sizeHeight - navButtonSize - 5);
                                canvas.DrawRoundRect (navButtons, 5, 5, navButtonPaint);
                                prevButton.SetBounds ((int)MathF.Round (navButtons.Left), (int)MathF.Round (navButtons.Top), (int)MathF.Round (navButtons.Right), (int)MathF.Round (navButtons.Bottom));
                                prevButton.SetColorFilter (Color.White, PorterDuff.Mode.Multiply);
                                prevButton.Draw (canvas);

                            }
                            canvas.DrawText (pageIndicator, (sizeWidth / 2) - (navPagePaint.MeasureText (pageIndicator) / 2), sizeHeight - 5, navPagePaint);
                            if (showRightNav)
                            {
                                navButtons.OffsetTo (sizeWidth - navPosition - (navButtonSize / 2), sizeHeight - navButtonSize - 5);
                                canvas.DrawRoundRect (navButtons, 5, 5, navButtonPaint);
                                nextButton.SetBounds ((int)MathF.Round (navButtons.Left), (int)MathF.Round (navButtons.Top), (int)MathF.Round (navButtons.Right), (int)MathF.Round (navButtons.Bottom));
                                nextButton.SetColorFilter (Color.White, PorterDuff.Mode.Multiply);
                                nextButton.Draw (canvas);
                            }
                        }
                    }

                    if (showDropDown)
                    {
                        PaintBrush.DrawShadow (dropDownRect, canvas);
                        canvas.DrawRect (dropDownRect, dropDownPaint);
                        string[] items = dropDownActions.Keys.ToArray ();
                        for (int i = 0; i < dropDownActions.Count; i++)
                        {
                            int ya = dropDownRect.Top + ((i + 1) * dropDownItemSize);
                            canvas.DrawLine (dropDownRect.Left + pad, ya, dropDownRect.Right - pad, ya, divisionPaint);
                            canvas.DrawText (items[i], dropDownRect.Left + pad, ya - (dropDownItemSize / 2) - (searchTextPaint.Ascent () / 2), searchTextPaint);
                        }
                    }
                }
                catch { }
            }
            public Dictionary<string, string> dropDownActions
            {
                get => _dropDownActions;
                set
                {
                    _dropDownActions = value;
                    dropDownRect = new Rect (0, 0, sizeWidth / 3, dropDownItemSize * value.Count);
                    dropDownRect.OffsetTo (dropDownIconRect.Right - sizeWidth / 3, dropDownIconRect.Bottom);
                }
            }
            Dictionary<string, string> _dropDownActions;
            public bool showDropDown;
            Paint dropDownPaint;

            void DropDown ()
            {
                if (dropDownActions != null)
                {
                    showDropDown = !showDropDown;
                    Invalidate ();
                }
            }
            int Touched (float x, float y)
            {
                float ypos = y + rowHeight;
                if (_data != null)
                {
                    
                    float g = sizeHeight - navButtonSize - 5;
                    if (y > g && showNavigation && y < sizeHeight - 5)
                    {
                        if (x > navPosition - (navButtonSize / 2) && x < navPosition + navButtonSize && showLeftNav)
                        {
                            eventListener?.Previous ();
                            actionsShown = false;
                            return -1;
                        }
                        if (x < sizeWidth - navPosition + (navButtonSize / 2) && x > sizeWidth - navPosition - (navButtonSize / 2) && showRightNav)
                        {
                            eventListener?.Next ();
                            actionsShown = false;
                            return -1;
                        }
                    }
                    return ClickedItem (ypos - rowHeight);
                }
                return -1;
            }
            int ClickedItem (float pos)
            {
                if (pos > tableTitleBoxHeight + rowHeight)
                {
                    for (int i = 0; i <= limit; i++)
                    {
                        if (pos < (rowHeight * (i + 1)) + tableTitleBoxHeight && i > 0)
                        {
                            return i - 1;
                        }
                    }
                }
                return -1;
            }
            int ClickedAction (float xpos, float ypos)
            {
                if (ypos < rowHeight)
                    return -1;
                bool AnimDone ()
                {
                    if (actionRows.Count == 0)
                    {
                        return actionButtonAnimatedValue == actionButtonSize * _actions.Count;
                    }
                    else
                    {
                        if (lastTouchedIndex < actionRows.Count)
                        {
                            return actionButtonAnimatedValue == actionButtonSize * actionRows[lastTouchedIndex].Count;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                int actionTotal = (actionRows.Count > 0 ? actionRows[lastTouchedIndex].Count : _actions.Count);
                if (lastActionShownIndex == Touched (xpos, ypos))
                {
                    if (actionsShown && xpos > actionButtonSize * actionTotal && AnimDone ())
                    {
                        int n = actionTotal - 1;
                        for (int i = 0; i < actionTotal && n >= 0; i++)
                        {
                            if (xpos > sizeWidth - (actionButtonSize * (i + 1)))
                            {
                                if (actionRows.Count == 0)
                                {
                                    return n;
                                }
                                else
                                {
                                    return actionRows[lastActionShownIndex][n];
                                }
                            }
                            n--;
                        }
                    }
                }
                return -1;
            }
            void ValueAnimator.IAnimatorUpdateListener.OnAnimationUpdate (ValueAnimator animation)
            {
            }

            public interface ITableEventListener
            {
                void SetPageLimit ();
                void Next ();
                void Previous ();
                void ItemSelected (int item);
                void TableUpdated ();
                void SearchUpdated (string to);
                void DropDownActionClicked (string action);
            }
            public enum TableAction
            {
                Edit,
                Delete,
                View,
                Info,
                Refresh,
                Check,
                Undo
            }
        }
        public class KeyboardConnection : BaseInputConnection
        {
            readonly SpannableStringBuilder builder;
            public KeyboardConnection (View view, bool b, SpannableStringBuilder builder) : base (view, b)
            {
                this.builder = builder;
            }
            public override IEditable Editable => builder;
        }
    }
}