using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MasonryLibrary
{
    /// <summary>
    /// Interaction logic for Masonry.xaml
    /// </summary>
    public partial class Masonry
    {
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("Animation",
                                                                                                  typeof (bool),
                                                                                                  typeof (Masonry));

        public static readonly DependencyProperty AnimationSpeedProperty = DependencyProperty.Register(
            "AnimationSpeed", typeof (int), typeof (Masonry));

        private readonly Queue<UserControl> _addQueue = new Queue<UserControl>();
        private readonly Queue<MasonryTile> _animationQueue = new Queue<MasonryTile>();

        private readonly DispatcherTimer _resizeTimer = new DispatcherTimer
                                                            {
                                                                Interval = new TimeSpan(0, 0, 0, 0, 500),
                                                                IsEnabled = false
                                                            };

        private List<MasonryTile> _childs = new List<MasonryTile>();

        public Masonry()
        {
            InitializeComponent();
            DataContext = this;
            _resizeTimer.Tick += ResizeTimerTick;
        }

        public bool Animation
        {
            get { return (bool) GetValue(AnimationProperty); }
            set { SetValue(AnimationProperty, value); }
        }

        public int AnimationSpeed
        {
            get { return (int) GetValue(AnimationSpeedProperty); }
            set { SetValue(AnimationSpeedProperty, value); }
        }

        public void Add(UserControl uc)
        {
            var e = new MasonryTile(uc);
            e.ResetLayout();
            e.OnSizeChanged += TileOnOnSizeChanged;
            MasonryGrid.Children.Add(e.Element);
            _childs.Add(e);
            Refresh();
        }

        public void Add(List<UserControl> ucs)
        {
            if (_childs.Count(c => c.RunningAnimation) == 0)
            {
                foreach (MasonryTile e in ucs.Select(uc => new MasonryTile(uc)))
                {
                    e.ResetLayout();
                    e.OnSizeChanged += TileOnOnSizeChanged;
                    MasonryGrid.Children.Add(e.Element);
                    _childs.Add(e);
                }
                Refresh();
            }
            else
            {
                foreach (UserControl uc in ucs)
                {
                    _addQueue.Enqueue(uc);
                }
            }
        }

        public void Remove(UserControl uc)
        {
            _childs.Remove(_childs.FirstOrDefault(child => Equals(child.Element, uc)));
            MasonryGrid.Children.Remove(
                MasonryGrid.Children.Cast<UserControl>().FirstOrDefault(control => Equals(control, uc)));
            Refresh();
        }

        public void Remove(List<UserControl> ucs)
        {
            foreach (UserControl uc in ucs)
            {
                _childs.Remove(_childs.FirstOrDefault(child => Equals(child.Element, uc)));
                MasonryGrid.Children.Remove(
                    MasonryGrid.Children.Cast<UserControl>().FirstOrDefault(control => Equals(control, uc)));
            }
            Refresh();
        }

        public void RemoveByProperty(string property)
        {
            var remove = new List<MasonryTile>(_childs.Where(c => c.HasProperty(property)));
            foreach (MasonryTile tile in remove)
            {
                _childs.Remove(tile);
                MasonryGrid.Children.Remove(
                    MasonryGrid.Children.Cast<UserControl>().FirstOrDefault(control => Equals(control, tile.Element)));
            }
            Refresh();
        }

        public void RemoveByPropertyValue(string property, object value)
        {
            var remove = new List<MasonryTile>(_childs.Where(c => c.HasProperty(property)));
            foreach (MasonryTile tile in remove)
            {
                if (Equals(tile.GetProperty(property), value))
                {
                    _childs.Remove(tile);
                    MasonryGrid.Children.Remove(
                        MasonryGrid.Children.Cast<UserControl>().FirstOrDefault(control => Equals(control, tile.Element)));
                }
            }
            Refresh();
        }

        public void RemoveAll()
        {
            _childs.Clear();
            MasonryGrid.Children.Clear();
            Refresh();
        }

        public void Refresh()
        {
            MakePos();
            foreach (MasonryTile child in _childs)
            {
                MoveTo(child);
            }
        }

        public void SortByProperty(string property, bool reverse = false)
        {
            var order =
                new List<MasonryTile>(
                    _childs.OrderBy(c => c.HasProperty(property) ? c.GetProperty(property) : null));
            if (reverse) order.Reverse();
            _childs = order;
            Refresh();
        }

        public void SortByMethod(string method, bool reverse = false)
        {
            var order =
                new List<MasonryTile>(_childs.OrderBy(kvp => kvp.HasMethod(method) ? kvp.CallMethod(method) : null));
            if (reverse) order.Reverse();
            _childs = order;
            Refresh();
        }

        private void AnimationComplete()
        {
            if (_childs.Count(c => c.RunningAnimation) == 0)
            {
                var ucs = new List<UserControl>();
                foreach (UserControl uc in _addQueue.ToList())
                {
                    ucs.Add(uc);
                    _addQueue.Dequeue();
                }
                Add(ucs);
                foreach (MasonryTile child in _animationQueue.ToList())
                {
                    MoveTo(child);
                    _animationQueue.Dequeue();
                }
            }
        }

        private void TileOnOnSizeChanged(object sender, EventArgs.SizeChanged sizeChangedEventArgs)
        {
            Refresh();
        }

        private void MoveTo(MasonryTile child)
        {
            if (_childs.Count(c => c.RunningAnimation) == 0)
            {
                if (Animation)
                {
                    child.AnimatePosition(new Thickness(child.NewPosition.Left, child.NewPosition.Top, 0, 0),
                                          new TimeSpan(0, 0, 0, 0, AnimationSpeed), AnimationComplete);
                }
                else
                {
                    child.Position = child.NewPosition;
                }
            }
            else
            {
                _animationQueue.Enqueue(child);
            }
        }

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

        private List<int[]> MatrixJoin(List<int[]> mtx, int[] cell)
        {
            List<int[]> tMtx = mtx;
            tMtx.Add(cell);
            tMtx.Sort(MatrixSortX);
            var mtxJoin = new List<int[]>();


            for (int i = 0, imax = tMtx.Count; i < imax; i++)
            {
                if (mtxJoin.Count > 0
                    && mtxJoin[mtxJoin.Count - 1][1] == tMtx[i][0]
                    && mtxJoin[mtxJoin.Count - 1][2] == tMtx[i][2])
                {
                    mtxJoin[mtxJoin.Count - 1][1] = tMtx[i][1];
                }
                else
                {
                    mtxJoin.Add(tMtx[i]);
                }
            }
            return mtxJoin;
        }

        private List<int[]> UpdateAttachArea(List<int[]> mtx, int[] point, int[] size)
        {
            List<int[]> tMtx = mtx;
            tMtx.Sort(MatrixSortDepth);
            int[] cell = {point[0], point[0] + size[0], point[1] + size[1]};
            for (int i = 0, imax = tMtx.Count; i < imax; i++)
            {
                if (tMtx.Count - 1 >= i)
                {
                    if (cell[0] <= tMtx[i][0] && tMtx[i][1] <= cell[1])
                    {
                        tMtx.RemoveAt(i);
                    }
                    else
                    {
                        tMtx[i] = MatrixTrimWidth(tMtx[i], cell);
                    }
                }
            }
            return MatrixJoin(tMtx, cell);
        }

        private int MatrixSortDepth(int[] a, int[] b)
        {
            return ((a[2] == b[2] && a[0] > b[0]) || a[2] > b[2]) ? 1 : -1;
        }

        private int MatrixSortX(int[] a, int[] b)
        {
            return (a[0] > b[0]) ? 1 : -1;
        }

        private int[] GetAttachPoint(List<int[]> mtx, int width)
        {
            List<int[]> tMtx = mtx;
            tMtx.Sort(MatrixSortDepth);
            int max = tMtx[tMtx.Count - 1][2];
            for (int i = 0, imax = tMtx.Count; i < imax; i++)
            {
                if (tMtx[i][2] >= max) break;
                if (tMtx[i][1] - tMtx[i][0] >= width)
                {
                    return new[] {tMtx[i][0], tMtx[i][2]};
                }
            }
            return new[] {0, max};
        }

        private void MakePos()
        {
            int width = Convert.ToInt32(ActualWidth);
            var matrix = new List<int[]> {new[] {0, width, 0}};
            int hmax = 0;
            foreach (MasonryTile child in _childs)
            {
                var size = new[] {child.Width, child.Height};
                int[] point = GetAttachPoint(matrix, size[0]);
                matrix = UpdateAttachArea(matrix, point, size);
                hmax = Math.Max(hmax, point[1] + size[1]);
                child.NewPosition = new Tile.Position {Left = point[0], Top = point[1]};
            }
            Height = hmax;
        }

        private void MasonryControlSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            _resizeTimer.IsEnabled = true;
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void ResizeTimerTick(object sender, System.EventArgs eventArgs)
        {
            _resizeTimer.IsEnabled = false;
            Refresh();
        }
    }
}