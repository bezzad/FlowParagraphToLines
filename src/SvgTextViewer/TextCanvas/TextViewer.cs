using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SvgTextViewer.TextCanvas
{
    public class TextViewer : BaseTextViewer
    {
        public TextViewer()
        {

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
                    isRtl ? RtlCulture : LtrCulture,
                    isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
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

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            CreateFormattedWords();
            foreach (var word in DrawnWords)
            {
                dc.DrawText(word.Format, word.DrawPoint);
                if (ShowWireFrame)
                    dc.DrawRectangle(null, WireFramePen, word.Area);
            }
        }
    }
}