using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TestMousePosToTextPointer
{
    public class TextCanvas : Canvas
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(TextCanvas), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
            "LineHeight", typeof(int), typeof(TextCanvas), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty IsJustifyProperty = DependencyProperty.Register(
            "IsJustify", typeof(bool), typeof(TextCanvas), new PropertyMetadata(default(bool)));

        
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public int LineHeight
        {
            get => (int)GetValue(LineHeightProperty);
            set => SetValue(LineHeightProperty, value);
        }
        public bool IsJustify
        {
            get => (bool)GetValue(IsJustifyProperty);
            set => SetValue(IsJustifyProperty, value);
        }

        public Thickness Padding { get; set; }

        protected List<WordInfo> DrawnWords { get; set; }
        protected Point EmptyPoint { get; set; } = new Point(0, 0);
        protected Point StartSelectionPoint { get; set; }
        protected Point EndSelectionPoint { get; set; }
        protected bool IsMouseDown { get; set; }
        protected Brush SelectedBrush { get; set; }
        protected Range HighlightRange { get; set; }



        public TextCanvas()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            Cursor = Cursors.IBeam;
            DrawnWords = new List<WordInfo>();
            SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };
            FontSize = 24;
            LineHeight = 25;

            // Add the event handler for MouseLeftButtonUp.
            MouseLeftButtonUp += TextCanvasMouseLeftButtonUp;
            MouseLeftButtonDown += TextCanvasMouseLeftButtonDown;
            MouseMove += TextCanvasMouseMove;
        }

        private void TextCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                EndSelectionPoint = e.GetPosition(this);
                InvalidateVisual();
            }
        }
        private void TextCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            IsMouseDown = true;
            ClearSelection();
            StartSelectionPoint = e.GetPosition((UIElement)sender);
            EndSelectionPoint = StartSelectionPoint;
            Debug.WriteLine("Mouse Left Button Down");
            InvalidateVisual();
        }
        private void TextCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            if (IsMouseDown)
            {
                IsMouseDown = false;
                EndSelectionPoint = e.GetPosition((UIElement)sender);
                Debug.WriteLine("Mouse Left Button Up, " + HighlightRange);
                InvalidateVisual();
            }
        }

        protected void CreateFormattedWords()
        {
            DrawnWords.Clear();
            var ltrText = @"Lorem ipsum dolor i sit a amet, an consectetur adipisicing elit, sed do eiusmod tempor";
            var rtlText = @"یکی از اساسی‌ترین کتاب‌های تاریخ معاصر ایران یادداشت‌های امیراسدالله اعلم است که بعضی از وقایع رژیم گذشته را بازگو می‌کند. هم این مسئله و هم مسئله دیگری سبب شد تا انگیزه اجرای جدیدی از این اثر شکل بگیرد. از اینرو درصدد بر آمدیم ۷جلد منتشر شده یادداشت‌ها را طوری تنظیم کنیم که دارای حجم کمتر با قابلیت بیشتر باشد. برای اینکار بدون توجهّ به انتشار سال ۱۳۴۶ جلد ۷، یادداشت‌ها را به ترتیب زمانی تنظیم کردیم. یعنی جلد ۷ که سال ۱۳۴۶ میباشد در ابتدای مجموعه قرار داده‌ایم و به ترتیب جلدهای ۱ تا ۶ به دنبال آن. کارهای انجام شده عبارتند از: ۱ جلد ۷ که به ابتدای مجموعه منتقل شد دارای اشتباهاتی در تاریخ و در متن و اشعار داشت که اصلاح شد و تاریخ‌ها تماماً به صورت روز/ ماه/ سال- عددی- مرتب شده. ۲ یادداشت ناشر و دیباچه و یادداشت‌های توضیحی ویراستار در اول مجموعه، قبل از یادداشت‌های سال ۴۶ آورده شده است. ۳ ارجاعات هم با تاریخ یادداشت‌ها و هم با صفحه‌ی مربوطه آورده شده. برای مثال ۲/۲۲ /۴۶ صفحه ۱۲۸ ۴ کل مجموعه در دو دفتر در حجم تقریباً مساوی تنظیم شده. نمایندگی نامزد‌های متعددّی می‌توانند با یکدیگر مبارزه کنند و در این چارچوب انتخابات آزادانه برگزار می‌شود. در این زمینه علم داستان پرمعنایی را نقل می‌کند که سرزده با همسرش به حوزه‌ای برای نام‌نویسی و دریافت کارت الکترال می‌روند و با جمعیّت انبوهی رو‌به‌رو می‌شوندکه به او می‌گویند چون امر شاه است، می‌خواهند کارت الکترال بگیرند و در انتخابات شرکت کنند. علم روز بعد این داستان را برای شاه می‌گوید '...شاهنشاه تعجّب فرمودند.بعد فرمودند حالا معلوم نیست این‌ها به کی رأی بدهند... برای یک لحظه شاه را نگران یافتم...' ( یادداشت ۲۸ خرداد ۱۳۵۴) . ولی جایی برای نگرانی نبود و دستگاه حزبی ـ دولتی، ترتیبی داده بود که نتیجه انتخابات تفاوتی با گذشته نداشته باشد. در این گیرودار شاه در هر فرصتی یادآور می‌شد که به هر حال این تحوّلات را نباید به معناًی آغاز دموکراسی و کاهش اختیارات او پنداشت و رسانه‌های گروهی نیز موظّف بودند این نکته را خوب به همه بفهمانند. به عنوان نمونه ظاهراًً خواننده‌ای در نامه‌ای به روزنامه اطّلاعات می‌پرسد";
            var startPoint = new Point(Padding.Left, Padding.Top);
            var offset = 0;

            ProcessContent(ltrText, false, ref offset, ref startPoint);

            startPoint.Y += LineHeight; // new line
            startPoint.X = ActualWidth - Padding.Right;

            ProcessContent(rtlText, true, ref offset, ref startPoint);
        }

        protected void ProcessContent(string content, bool isRtl, ref int offset, ref Point startPoint)
        {
            var spaceWidth = 5;
            var random = false;

            var words = content.Split(' ');
            foreach (var word in words)
            {
                // Create the initial formatted text string.
                var wordFormatter = new FormattedText(
                    word,
                    isRtl ? CultureInfo.GetCultureInfo("fa-IR") : CultureInfo.GetCultureInfo("en-us"),
                    isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    FontSize,
                    Brushes.Black, 1);

                // Use an Bold font style beginning at the 28th character and continuing for 28 characters.
                random = !random;
                if (random)
                    wordFormatter.SetFontWeight(FontWeights.Bold, 0, word.Length);

                var wordW = wordFormatter.Width;
                var newLineNeeded = isRtl
                    ? (startPoint.X - wordW < Padding.Left)
                    : (startPoint.X + wordW > ActualWidth - Padding.Right);

                if (newLineNeeded)
                {
                    startPoint.Y += LineHeight; // new line
                    startPoint.X = isRtl
                        ? ActualWidth - Padding.Right
                        : Padding.Left;
                }

                var wordArea = new Rect(isRtl ? new Point(startPoint.X - wordW, startPoint.Y) : startPoint, new Size(wordW, LineHeight));
                var wordInfo = new WordInfo(wordFormatter, startPoint, wordArea, offset);
                DrawnWords.Add(wordInfo);
                offset += word.Length;
                startPoint.X += isRtl ? -wordW : wordW;
                startPoint.X += isRtl ? -spaceWidth : spaceWidth;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            InvalidateVisual();
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            CreateFormattedWords();
            foreach (var word in DrawnWords)
            {
                dc.DrawText(word.Format, word.DrawPoint);
                dc.DrawRectangle(null, new Pen(Brushes.Red, 1), word.Area);
            }

            if (IsMouseDown || HighlightRange != null)
            {
                //HighlightSelectedText(dc);
            }
        }

        public void ClearSelection()
        {
            HighlightRange = null;
            EndSelectionPoint = StartSelectionPoint = EmptyPoint;
        }
        public void Render() { InvalidateVisual(); }


        protected void HighlightSelectedText(DrawingContext dc)
        {
            int GetCorrectWordIndex(Point selectedPoint)
            {
                var result = -1;

                if (DrawnWords.LastOrDefault()?.CompareTo(selectedPoint) < 0)
                    result = DrawnWords.Count - 1;
                else if (DrawnWords.FirstOrDefault()?.CompareTo(selectedPoint) > 0)
                    result = 0;
                else
                    result = DrawnWords.BinarySearch(selectedPoint); // DrawnWords.FindIndex(w => w.CompareTo(selectedPoint) == 0); 
#if DEBUG
                if (result < 0)
                    dc.DrawEllipse(Brushes.Red, null, selectedPoint, 5, 5);
#endif

                return result;
            }

            if (StartSelectionPoint.CompareTo(EndSelectionPoint) != 0)
            {
                //var forceToFind = StartSelectionPoint.Value.CompareTo(EndSelectionPoint.Value) > 0;
                var startWord = GetCorrectWordIndex(StartSelectionPoint);
                var endWord = GetCorrectWordIndex(EndSelectionPoint);

                if (startWord < 0 && endWord >= 0) // startWord is out of word bound but endWord is in correct range
                    startWord = GetCorrectWordIndex(StartSelectionPoint);

                if (endWord < 0 && startWord >= 0) // endWord is out of word bound but startWord is in correct range
                    endWord = GetCorrectWordIndex(EndSelectionPoint);

                if ((startWord < 0 || endWord < 0) == false)
                    HighlightRange = new Range(startWord, endWord);

                if (IsMouseDown == false)
                    StartSelectionPoint = EndSelectionPoint = EmptyPoint;
            }

            if (HighlightRange == null)
                return;

            var from = Math.Min(HighlightRange.Start, HighlightRange.End);
            var to = Math.Max(HighlightRange.Start, HighlightRange.End);

            for (var w = from; w <= to; w++)
            {
                var currentWord = DrawnWords[w].Area;
                var isFirstOfLineWord = w == from || !DrawnWords[w - 1].DrawPoint.Y.Equals(currentWord.Y);

                if (isFirstOfLineWord == false)
                {
                    var previousWord = DrawnWords[w - 1];
                    var startX = previousWord.IsRtl ? previousWord.DrawPoint.X : previousWord.DrawPoint.X + previousWord.Width;
                    var width = currentWord.X - startX + currentWord.Width;

                    currentWord = new Rect(new Point(startX, currentWord.Y), new Size(width, currentWord.Height));
                }
                dc.DrawRectangle(SelectedBrush, null, currentWord);
            }
        }
    }
}
