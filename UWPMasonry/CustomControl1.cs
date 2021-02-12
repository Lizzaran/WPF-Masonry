using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace UWPMasonry
{
    public class CustomControl1 : ItemsControl
    {
        #region Static Fields

        /// <summary>
        ///     The spacing property
        /// </summary>
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing),typeof(int), typeof(CustomControl1), null);


        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the spacing.
        /// </summary>
        /// <value>
        ///     The spacing.
        /// </value>
        public int Spacing
        {
            get
            {
                return (int)this.GetValue(SpacingProperty);
            }
            set
            {
                this.SetValue(SpacingProperty, value);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Updates this instance.
        /// </summary>
        public virtual void Update()
        {
            var matrix = new List<int[]> { new[] { 0, (int)this.ActualWidth, 0 } };
            var hMax = 0;
            foreach (var child in this.Items)
            {
                var element = child as FrameworkElement;
                if (element != null)
                {
                    var size = new[]
                                   { (int)element.ActualWidth + this.Spacing, (int)element.ActualHeight + this.Spacing };
                    var point = this.GetAttachPoint(matrix, size[0]);
                    matrix = this.UpdateAttachArea(matrix, point, size);
                    hMax = Math.Max(hMax, point[1] + size[1]);
                    this.UpdateAlignment(element);
                    var oldThickness = element.Margin;
                    if (Math.Abs(oldThickness.Left - point[0]) > 1 || Math.Abs(oldThickness.Top - point[1]) > 1)
                    {
                        this.SetPosition(element, point[1], point[0]);
                    }
                }
            }
        }

        #endregion

        public CustomControl1()
        {
            this.DefaultStyleKey = typeof(CustomControl1);

           
        }

        #region Methods

        /// <summary>
        ///     Adds the specified object as the child of the <see cref="T:System.Windows.Controls.ItemsControl" /> object.
        /// </summary>
        /// <param name="value">The object to add as a child.</param>
        /// <exception cref="InvalidDataException">Child has to derive from FrameworkElement.</exception>
        protected void AddChild(object value)
        {
            if (!(value is FrameworkElement))
            {
                throw new InvalidDataException("Child has to derive from FrameworkElement.");
            }

            this.Items.Add(value);
        }

        /// <summary>
        ///     Handles the child desired size changed.
        /// </summary>
        /// <param name="child">The child.</param>
        protected virtual void HandleChildDesiredSizeChanged(UIElement child)
        {
            this.HandleUpdate(child as FrameworkElement);
        }



        /// <summary>
        ///     Handles the update.
        /// </summary>
        /// <param name="element">The element.</param>
        protected void HandleUpdate(FrameworkElement element)
        {
            if (element != null)
            {
                if (element.IsLoaded)
                {
                    this.Update();
                }
                else
                {
                    element.Loaded += delegate { this.Update(); };
                }
            }
        }


        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);

            if (Items != null)
            {
                foreach (var child in Items)
                {
                    this.HandleUpdate(child as FrameworkElement);
                }
            }
        }




        protected override Size ArrangeOverride(Size finalSize)
        {
            this.Update();

            return base.ArrangeOverride(finalSize);
        }

        protected override void OnBringIntoViewRequested(BringIntoViewRequestedEventArgs e)
        {
            this.Update();

            base.OnBringIntoViewRequested(e);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            this.Update();

            base.PrepareContainerForItemOverride(element, item);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.Update();

            return base.MeasureOverride(availableSize);
        }

         
        ///// <summary>
        /////     Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified information as
        /////     part of the eventual event data.
        ///// </summary>
        ///// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        //protected override void OnRenderSizeChanged(Size sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //    this.HandleRenderSizeChanged(sizeInfo);
        //}

        ///// <summary>
        /////     Supports layout behavior when a child element is resized.
        ///// </summary>
        ///// <param name="child">The child element that is being resized.</param>
        //protected override void OnChildDesiredSizeChanged(UIElement child)
        //{
        //    base.OnChildDesiredSizeChanged(child);
        //    this.HandleChildDesiredSizeChanged(child);
        //}

        /// <summary>
        ///     Sets the position.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="newTop">The new top.</param>
        /// <param name="newLeft">The new left.</param>
        protected virtual void SetPosition(FrameworkElement element, int newTop, int newLeft)
        {
            if (element != null)
            {
                element.Margin = new Thickness(newLeft, newTop, 0, 0);
            }
        }

        /// <summary>
        ///     Updates the alignment.
        /// </summary>
        /// <param name="element">The element.</param>
        protected virtual void UpdateAlignment(FrameworkElement element)
        {
            if (element != null)
            {
                element.HorizontalAlignment = HorizontalAlignment.Left;
                element.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        /// <summary>
        ///     Gets the attach point.
        /// </summary>
        /// <param name="mtx">The MTX.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        private int[] GetAttachPoint(List<int[]> mtx, int width)
        {
            mtx.Sort(this.MatrixSortDepth);
            var max = mtx[mtx.Count - 1][2];
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtx[i][2] >= max)
                {
                    break;
                }
                if (mtx[i][1] - mtx[i][0] >= width)
                {
                    return new[] { mtx[i][0], mtx[i][2] };
                }
            }
            return new[] { 0, max };
        }

        /// <summary>
        ///     Matrixes the join.
        /// </summary>
        /// <param name="mtx">The MTX.</param>
        /// <param name="cell">The cell.</param>
        /// <returns></returns>
        private List<int[]> MatrixJoin(List<int[]> mtx, int[] cell)
        {
            mtx.Add(cell);
            mtx.Sort(this.MatrixSortX);
            var mtxJoin = new List<int[]>();
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtxJoin.Count > 0 && mtxJoin[mtxJoin.Count - 1][1] == mtx[i][0]
                    && mtxJoin[mtxJoin.Count - 1][2] == mtx[i][2])
                {
                    mtxJoin[mtxJoin.Count - 1][1] = mtx[i][1];
                }
                else
                {
                    mtxJoin.Add(mtx[i]);
                }
            }
            return mtxJoin;
        }

        /// <summary>
        ///     Matrixes the sort depth.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private int MatrixSortDepth(int[] a, int[] b)
        {
            return (a[2] == b[2] && a[0] > b[0]) || a[2] > b[2] ? 1 : -1;
        }

        /// <summary>
        ///     Matrixes the sort x.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private int MatrixSortX(int[] a, int[] b)
        {
            return a[0] > b[0] ? 1 : -1;
        }

        /// <summary>
        ///     Matrixes the width of the trim.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private int[] MatrixTrimWidth(int[] a, int[] b)
        {
            if (a[0] >= b[0] && a[0] < b[1] || a[1] >= b[0] && a[1] < b[1])
            {
                if (a[0] >= b[0] && a[0] < b[1])
                {
                    a[0] = b[1];
                }
                else
                {
                    a[1] = b[0];
                }
            }
            return a;
        }

        /// <summary>
        ///     Updates the attach area.
        /// </summary>
        /// <param name="mtx">The MTX.</param>
        /// <param name="point">The point.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private List<int[]> UpdateAttachArea(List<int[]> mtx, int[] point, int[] size)
        {
            mtx.Sort(this.MatrixSortDepth);
            int[] cell = { point[0], point[0] + size[0], point[1] + size[1] };
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtx.Count - 1 >= i)
                {
                    if (cell[0] <= mtx[i][0] && mtx[i][1] <= cell[1])
                    {
                        mtx.RemoveAt(i);
                    }
                    else
                    {
                        mtx[i] = this.MatrixTrimWidth(mtx[i], cell);
                    }
                }
            }
            return this.MatrixJoin(mtx, cell);
        }

        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

        }
    }
}
