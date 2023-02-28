using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HarAnalyzer.View
{
    public class LayoutPanel : Panel
    {
        #region Orientation Dependency Property

        public static readonly DependencyProperty OrientationProperty =
          DependencyProperty.Register("Orientation", typeof(Orientation), typeof(LayoutPanel),
              new FrameworkPropertyMetadata(Orientation.Vertical,
                  FrameworkPropertyMetadataOptions.AffectsArrange |
                  FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        #region Interval Dependency Property

        public static readonly DependencyProperty IntervalProperty =
          DependencyProperty.Register("Interval", typeof(double), typeof(LayoutPanel),
              new FrameworkPropertyMetadata(5d,
                  FrameworkPropertyMetadataOptions.AffectsArrange |
                  FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double Interval
        {
            get { return (double)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        #endregion

        #region Layout Mode Attached Property

        public static readonly DependencyProperty LayoutModeProperty =
            DependencyProperty.RegisterAttached("LayoutMode", typeof(LayoutMode), typeof(LayoutPanel),
                new FrameworkPropertyMetadata(LayoutMode.Auto, new PropertyChangedCallback(OnLayoutModeChanged)));

        private static void OnLayoutModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element != null)
            {
                var parent = VisualTreeHelper.GetParent(element) as LayoutPanel;
                if (parent != null)
                    parent.InvalidateMeasure();
            }
        }

        public static void SetLayoutMode(UIElement element, LayoutMode value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(LayoutModeProperty, value);
        }

        [AttachedPropertyBrowsableForChildren()]
        public static LayoutMode GetLayoutMode(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return ((LayoutMode)element.GetValue(LayoutModeProperty));
        }

        #endregion

        #region Layout Size Attached Property

        public static readonly DependencyProperty LayoutSizeProperty =
            DependencyProperty.RegisterAttached("LayoutSize", typeof(double), typeof(LayoutPanel),
                new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnLayoutSizeChanged)));

        private static void OnLayoutSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element != null)
            {
                var parent = VisualTreeHelper.GetParent(element) as LayoutPanel;
                if (parent != null)
                    parent.InvalidateMeasure();
            }
        }

        public static void SetLayoutSize(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(LayoutSizeProperty, value);
        }

        [AttachedPropertyBrowsableForChildren()]
        public static double GetLayoutSize(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return ((double)element.GetValue(LayoutSizeProperty));
        }

        #endregion

        // measure override returns desired size of an element.
        // desired size is a minimum affordable size to fit objects content.
        // this should be a well-defined value, no double.PositiveInfinity | double.NaN is allowed.
        // it is possible for input to be double.PositiveInfinity (for example for scrolled area).
        // desired size can then be corrected by minimum width and height, clipped area of a parent.

        protected override Size MeasureOverride(Size availableSize)
        {
            // Warning! the width and height here are calculated regardless of orientation.
            // for horizontal orientation they match, but for vertical orientation
            // they are flipped by TranslateWidth(), TranslateHeight(), TranslateSize(), TranslateRect() functions.

            var interval = Interval;
            var orientation = Orientation;
            var elements = GetElements();
            var occupiedWidth = 0d;
            var occupiedHeight = 0d;
            var availableWidth = TranslateWidth(availableSize, orientation);
            var availableHeight = TranslateHeight(availableSize, orientation);

            // handling non-stretched elements first, leaving the remaining space for the stretched ones.

            foreach (var element in elements)
            {
                if (element.IsStretched)
                {
                    // if the element is stretched, assuming for now that its size is 0.
                    // we will handle the stretched size later, dividing the remaining space.

                    if (!element.IsLast)
                        occupiedWidth += interval;
                }
                else
                {
                    var elementAvailableWidth = GetElementAvailableWidth(element, availableWidth, occupiedWidth, interval);
                    var elementAvailableHeight = availableHeight;
                    var elementAvailableSize = TranslateSize(elementAvailableWidth, elementAvailableHeight, orientation);
                    var uiElement = element.UIElement;
                    uiElement.Measure(elementAvailableSize);
                    var elementDesiredSize = uiElement.DesiredSize;
                    var elementDesiredWidth = TranslateWidth(elementDesiredSize, orientation);
                    var elementDesiredHeight = TranslateHeight(elementDesiredSize, orientation);

                    occupiedWidth += elementDesiredWidth + (element.IsLast ? 0 : interval);
                    occupiedHeight = Math.Max(elementDesiredHeight, occupiedHeight);
                }
            }

            // calculating the amount of space per single stretched element

            var stretchedWidth = 0d;
            var stretchedElements = elements.Where(e => e.IsStretched).ToList();
            if (stretchedElements.Count > 0)
            {
                var remainingWidth = Math.Max(0, availableWidth - occupiedWidth);
                stretchedWidth = (remainingWidth / stretchedElements.Count);
            }

            // measuring stretched elements

            foreach (var stretchedElement in stretchedElements)
            {
                var elementAvailableWidth = stretchedWidth;
                var elementAvailableHeight = availableHeight;
                var elementAvailableSize = TranslateSize(elementAvailableWidth, elementAvailableHeight, orientation);
                stretchedElement.UIElement.Measure(elementAvailableSize);
                var elementDesiredSize = stretchedElement.UIElement.DesiredSize;
                var elementDesiredWidth = TranslateWidth(elementDesiredSize, orientation);
                var elementDesiredHeight = TranslateHeight(elementDesiredSize, orientation);

                occupiedWidth += elementDesiredWidth;
                occupiedHeight = Math.Max(elementDesiredHeight, occupiedHeight);
            }

            var desiredSize = TranslateSize(occupiedWidth, occupiedHeight, orientation);
            return desiredSize;
        }

        // determines the final arrangement of elements.
        // the input is the final area within which elements will be arranged.
        // the final size doesn't coincide with desired size and can be larger,
        // but it will not be less than desired size.

        protected override Size ArrangeOverride(Size finalSize)
        {
            /// Warning! the width and height here are calculated regardless of orientation.
            // for horizontal orientation they match, but for vertical orientation
            // they are flipped by TranslateWidth(), TranslateHeight(), TranslateSize(), TranslateRect() functions.

            var elements = GetElements();
            var interval = Interval;
            var orientation = Orientation;
            var finalWidth = TranslateWidth(finalSize, orientation);

            // first run is to determine the overall width of non-stretched elements and based on that
            // derive the average size of stretched elements from remaining space

            var minOccupiedWidth = 0d;
            foreach (var element in elements)
            {
                if (!element.IsStretched)
                    minOccupiedWidth += GetElementFinalWidth(element, finalWidth, minOccupiedWidth, interval, orientation);
                if (!element.IsLast) // still adding interval for stretched elements
                    minOccupiedWidth += interval;
            }

            var remainingWidth = Math.Max(0, finalWidth - minOccupiedWidth);
            var stretchCount = elements.Where(e => e.IsStretched).Count();
            var stretchWidth = remainingWidth / stretchCount;

            // once all widths are set, we can place elements in appropriate places

            var occupiedWidth = 0d;
            foreach (var element in elements)
            {
                var elementWidth = 0d;
                if (element.IsStretched)
                    elementWidth = stretchWidth;
                else elementWidth = GetElementFinalWidth(element, finalWidth, occupiedWidth, interval, orientation);
                var elementHeight = TranslateHeight(finalSize, orientation);

                var rect = TranslateRect(occupiedWidth, 0, elementWidth, elementHeight, orientation);
                var uiElement = element.UIElement;
                uiElement.Arrange(rect);

                occupiedWidth += elementWidth;
                if (!element.IsLast)
                    occupiedWidth += interval;
            }

            return finalSize;
        }

        private double GetElementFinalWidth(Element element, double totalWidth, double occupiedWidth, double interval, Orientation orientation)
        {
            var layoutMode = element.LayoutMode;
            var layoutSize = element.LayoutSize;
            var surroundingIntervals = (element.IsFirst ? 0d : interval) + (element.IsLast ? 0d : interval);
            var availableWidth = Math.Max(0, totalWidth - occupiedWidth);
            var desiredWidth = TranslateWidth(element.UIElement.DesiredSize, orientation);
            if (layoutMode == LayoutMode.Stretch)
                return 0; // this is calculated in different place
            if (layoutMode == LayoutMode.Auto)
                return Math.Min(availableWidth, desiredWidth);
            if (layoutMode == LayoutMode.Fixed)
                return Math.Min(availableWidth, Math.Max(0, layoutSize));
            if (layoutMode == LayoutMode.Percentage)
                return Math.Min(availableWidth, Math.Max(0, layoutSize * 0.01 * totalWidth));
            if (layoutMode == LayoutMode.FixedGreedy)
                return Math.Min(availableWidth, Math.Max(0, layoutSize - surroundingIntervals));
            if (layoutMode == LayoutMode.PercentageGreedy)
                return Math.Min(availableWidth, Math.Max(0, layoutSize * 0.01 * totalWidth - surroundingIntervals));
            return availableWidth;
        }

        private double GetElementAvailableWidth(Element element, double totalWidth, double occupiedWidth, double interval)
        {
            var layoutMode = element.LayoutMode;
            var layoutSize = element.LayoutSize;
            var surroundingIntervals = (element.IsFirst ? 0d : interval) + (element.IsLast ? 0d : interval);
            var availableWidth = Math.Max(0, totalWidth - occupiedWidth);
            if (layoutMode == LayoutMode.Stretch)
                return availableWidth; // this is calculated in different place
            if (layoutMode == LayoutMode.Auto)
                return availableWidth;
            if (layoutMode == LayoutMode.Fixed)
                return Math.Min(availableWidth, Math.Max(0, layoutSize));
            if (layoutMode == LayoutMode.Percentage)
                return Math.Min(availableWidth, Math.Max(0, layoutSize * 0.01 * totalWidth));
            if (layoutMode == LayoutMode.FixedGreedy)
                return Math.Min(availableWidth, Math.Max(0, layoutSize - surroundingIntervals));
            if (layoutMode == LayoutMode.PercentageGreedy)
                return Math.Min(availableWidth, Math.Max(0, layoutSize * 0.01 * totalWidth - surroundingIntervals));
            return availableWidth;
        }

        private double TranslateWidth(Size size, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
                return size.Width;
            return size.Height;
        }

        private double TranslateHeight(Size size, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
                return size.Height;
            return size.Width;
        }

        private Size TranslateSize(double width, double height, Orientation orientation)
        {
            var size = new Size();
            if (orientation == Orientation.Horizontal)
            {
                size.Width = width;
                size.Height = height;
            }
            if (orientation == Orientation.Vertical)
            {
                size.Width = height;
                size.Height = width;
            }
            return size;
        }

        private Rect TranslateRect(double left, double top, double width, double height, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
                return new Rect(left, top, width, height);
            return new Rect(top, left, height, width);
        }

        private List<Element> GetElements()
        {
            var uiElements = InternalChildren.OfType<UIElement>().Where(e => e != null && e.Visibility != Visibility.Collapsed);
            var elements = new List<Element>();

            foreach (var uiElement in uiElements)
            {
                var element = new Element();
                element.UIElement = uiElement;
                element.LayoutMode = (LayoutMode)uiElement.GetValue(LayoutModeProperty);
                element.LayoutSize = (double)uiElement.GetValue(LayoutSizeProperty);
                element.IsFirst = (uiElement == uiElements.First());
                element.IsLast = (uiElement == uiElements.Last());
                elements.Add(element);
            }

            return elements;
        }

        private class Element
        {
            public UIElement UIElement { get; set; }
            public LayoutMode LayoutMode { get; set; }
            public double LayoutSize { get; set; }
            public bool IsFirst { get; set; }
            public bool IsLast { get; set; }
            public bool IsStretched { get { return LayoutMode == LayoutMode.Stretch; } }
            public bool IsAuto { get { return LayoutMode == LayoutMode.Auto; } }
            public bool IsFixed { get { return LayoutMode == LayoutMode.Fixed; } }
            public bool IsPercentage { get { return LayoutMode == LayoutMode.Percentage; } }
            public bool IsFixedGreedy { get { return LayoutMode == LayoutMode.FixedGreedy; } }
            public bool IsPercentageGreedy { get { return LayoutMode == LayoutMode.PercentageGreedy; } }
        }
    }

    public enum LayoutMode
    {
        Auto,
        Stretch,
        Fixed,
        FixedGreedy,
        Percentage,
        PercentageGreedy
    }
}
