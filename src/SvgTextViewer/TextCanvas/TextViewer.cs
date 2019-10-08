﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using MethodTimer;

namespace SvgTextViewer.TextCanvas
{
    public class TextViewer : BaseTextViewer
    {
        [Time]
        protected void ProcessContent(List<List<WordInfo>> content, Point startPoint)
        {
            DrawWords.Clear();
            var line = new List<WordInfo>();
            foreach (var para in content)
            {
                foreach (var word in para)
                {
                    word.SpaceWidth = FontSize * 0.3;

                    // Create the initial formatted text string.
                    var wordFormatter = new FormattedText(
                        word.Text,
                        word.IsRtl ? RtlCulture : LtrCulture,
                        word.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                        new Typeface(FontFamily, FontStyles.Normal, word.Styles.ContainsKey(StyleType.FontWeight) ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal),
                        FontSize,
                        word.Styles.ContainsKey(StyleType.Color) ? (SolidColorBrush)new BrushConverter().ConvertFromString(word.Styles[StyleType.Color].Value) : Brushes.Black,
                        PixelsPerDip);

                    var wordW = wordFormatter.Width;
                    var newLineNeeded = IsContentRtl
                        ? (startPoint.X - wordW < Padding.Left)
                        : (startPoint.X + wordW > ActualWidth - Padding.Right);
                    
                    if (newLineNeeded)
                    {
                        Lines.Add(line);
                        line = new List<WordInfo>();
                        startPoint.Y += LineHeight; // new line
                        startPoint.X = IsContentRtl
                            ? ActualWidth - Padding.Right
                            : Padding.Left;
                    }

                    line.Add(word);
                    var wordArea = new Rect(word.IsRtl ? new Point(startPoint.X - wordW, startPoint.Y) : startPoint, new Size(wordW, LineHeight));
                    word.Area = wordArea;
                    word.Format = wordFormatter;
                    word.DrawPoint = startPoint;
                    DrawWords.Add(word);

                    startPoint.X += word.IsRtl ? -wordW : wordW;
                    startPoint.X += word.IsRtl ? -word.SpaceWidth : word.SpaceWidth;
                }
                
                // new line + ParagraphSpace
                Lines.Add(line);
                line = new List<WordInfo>();
                startPoint.Y += LineHeight + ParagraphSpace; 
                startPoint.X = IsContentRtl
                    ? ActualWidth - Padding.Right
                    : Padding.Left;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            var startPoint = new Point(IsContentRtl
                ? ActualWidth - Padding.Right
                : Padding.Left, Padding.Top);

            ProcessContent(PageContent, startPoint);

            foreach (var word in PageContent[0])
            {
                dc.DrawText(word.Format, word.DrawPoint);
                if (ShowWireFrame)
                    dc.DrawRectangle(null, WireFramePen, word.Area);
            }
        }
    }
}