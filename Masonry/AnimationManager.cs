#region License

/*
 Copyright 2013 - 2016 Nikita Bernthaler
 AnimationManager.cs is part of Masonry.

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
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Animation;

    /// <summary>
    ///     The Animation Manager
    /// </summary>
    public class AnimationManager
    {
        #region Fields

        /// <summary>
        ///     The active animations
        /// </summary>
        private readonly List<AnimationItem> activeAnimations;

        /// <summary>
        ///     The animation property
        /// </summary>
        private readonly DependencyProperty animationProperty;

        /// <summary>
        ///     The animation queue
        /// </summary>
        private readonly Queue<AnimationItem> animationQueue;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimationManager" /> class.
        /// </summary>
        /// <param name="animationProperty">The animation property.</param>
        public AnimationManager(DependencyProperty animationProperty)
        {
            this.animationProperty = animationProperty;
            this.animationQueue = new Queue<AnimationItem>();
            this.activeAnimations = new List<AnimationItem>();
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Occurs when [on completed].
        /// </summary>
        public event EventHandler OnCompleted;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether an animation is running.
        /// </summary>
        /// <value>
        ///     <c>true</c> if an animation is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning => this.activeAnimations.Any();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Enqueues the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="animation">The animation.</param>
        public void Enqueue(FrameworkElement element, AnimationTimeline animation)
        {
            if (element != null && animation != null)
            {
                this.animationQueue.Enqueue(new AnimationItem(element, animation));
            }
        }

        /// <summary>
        ///     Starts the animation.
        /// </summary>
        public void Start()
        {
            while (this.animationQueue.Count != 0)
            {
                var animationItem = this.animationQueue.Dequeue();
                if (animationItem != null && animationItem.Element.IsLoaded)
                {
                    this.ClearActiveAnimations(animationItem);
                    this.HandleAnimationEvent(animationItem);
                    animationItem.Element.BeginAnimation(this.animationProperty, animationItem.Animation);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Clears the active animations.
        /// </summary>
        /// <param name="animationItem">The animation item.</param>
        protected void ClearActiveAnimations(AnimationItem animationItem)
        {
            foreach (var activeAnimation in this.activeAnimations.ToArray())
            {
                if (activeAnimation.Element.Equals(animationItem.Element))
                {
                    this.activeAnimations.Remove(activeAnimation);
                }
            }
        }

        /// <summary>
        ///     Handles the animation event.
        /// </summary>
        /// <param name="animationItem">The animation item.</param>
        protected void HandleAnimationEvent(AnimationItem animationItem)
        {
            foreach (var a in this.activeAnimations.ToArray())
            {
                if (a.Element.Equals(animationItem.Element))
                {
                    this.activeAnimations.Remove(a);
                }
            }
            this.activeAnimations.Add(animationItem);
            animationItem.Animation.Completed += delegate
                {
                    this.activeAnimations.Remove(animationItem);
                    if (!this.activeAnimations.Any())
                    {
                        this.OnCompleted?.Invoke(null, EventArgs.Empty);
                    }
                };
        }

        #endregion
    }

    /// <summary>
    ///     The Animation Item
    /// </summary>
    public class AnimationItem
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimationItem" /> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="animation">The animation.</param>
        public AnimationItem(FrameworkElement element, AnimationTimeline animation)
        {
            this.Element = element;
            this.Animation = animation;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the animation.
        /// </summary>
        /// <value>
        ///     The animation.
        /// </value>
        public AnimationTimeline Animation { get; set; }

        /// <summary>
        ///     Gets or sets the element.
        /// </summary>
        /// <value>
        ///     The element.
        /// </value>
        public FrameworkElement Element { get; set; }

        #endregion
    }
}