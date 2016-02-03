#region License

/*
 Copyright 2013 - 2016 Nikita Bernthaler
 AnimatedMasonryControl.cs is part of Masonry.

 Masonry is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 Masonry is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with Masonry. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

namespace Masonry
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    /// <summary>
    ///     The Animated Masonry Control
    /// </summary>
    /// <seealso cref="Masonry.MasonryControl" />
    public class AnimatedMasonryControl : MasonryControl
    {
        #region Static Fields

        /// <summary>
        ///     The animation duration property
        /// </summary>
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(AnimatedMasonryControl));

        #endregion

        #region Fields

        /// <summary>
        ///     The request update
        /// </summary>
        private bool requestUpdate;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimatedMasonryControl" /> class.
        /// </summary>
        public AnimatedMasonryControl()
        {
            this.AnimationManager = new AnimationManager(MarginProperty);
            this.AnimationManager.OnCompleted += this.OnAnimationManagerCompleted;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the duration of the animation.
        /// </summary>
        /// <value>
        ///     The duration of the animation.
        /// </value>
        public Duration AnimationDuration
        {
            get
            {
                return (Duration)this.GetValue(AnimationDurationProperty);
            }
            set
            {
                this.SetValue(AnimationDurationProperty, value);
            }
        }

        /// <summary>
        ///     Gets the animation manager.
        /// </summary>
        /// <value>
        ///     The animation manager.
        /// </value>
        public AnimationManager AnimationManager { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Updates this instance.
        /// </summary>
        public override void Update()
        {
            base.Update();
            this.AnimationManager.Start();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the animation.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="newTop">The new newTop.</param>
        /// <param name="newLeft">The new newLeft.</param>
        /// <returns></returns>
        protected virtual AnimationTimeline CreateAnimation(FrameworkElement element, int newTop, int newLeft)
        {
            return new ThicknessAnimation
                       {
                           From = element.Margin, To = new Thickness(newLeft, newTop, 0, 0),
                           Duration = this.AnimationDuration
                       };
        }

        /// <summary>
        ///     Handles the child desired size changed.
        /// </summary>
        /// <param name="child">The child.</param>
        protected override void HandleChildDesiredSizeChanged(UIElement child)
        {
            if (this.AnimationManager.IsRunning)
            {
                this.requestUpdate = true;
            }
            else
            {
                base.HandleChildDesiredSizeChanged(child);
            }
        }

        /// <summary>
        ///     Handles the render size changed.
        /// </summary>
        /// <param name="sizeInfo">The size information.</param>
        protected override void HandleRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (this.AnimationManager.IsRunning)
            {
                this.requestUpdate = true;
            }
            else
            {
                base.HandleRenderSizeChanged(sizeInfo);
            }
        }

        /// <summary>
        ///     Sets the position.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="newTop">The new newTop.</param>
        /// <param name="newLeft">The new newLeft.</param>
        protected override void SetPosition(FrameworkElement element, int newTop, int newLeft)
        {
            if (element != null)
            {
                this.AnimationManager.Enqueue(element, this.CreateAnimation(element, newTop, newLeft));
            }
        }

        /// <summary>
        ///     Called when [animation manager completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAnimationManagerCompleted(object sender, EventArgs eventArgs)
        {
            if (this.requestUpdate)
            {
                this.requestUpdate = false;
                this.Update();
            }
        }

        #endregion
    }
}