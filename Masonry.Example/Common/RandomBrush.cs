#region License

/*
 Copyright 2013 - 2016 Nikita Bernthaler
 RandomBrush.cs is part of Masonry.Example.

 Masonry.Example is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 Masonry.Example is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with Masonry.Example. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

namespace Masonry.Example.Common
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Media;

    public class RandomBrush
    {
        #region Fields

        private readonly List<Brush> brushes;

        private readonly Random random;

        #endregion

        #region Constructors and Destructors

        public RandomBrush()
        {
            this.random = new Random();
            this.brushes = new List<Brush>();
            var props = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var propInfo in props)
            {
                if (!propInfo.Name.Contains("White") && !propInfo.Name.Contains("Gray"))
                {
                    this.brushes.Add((Brush)propInfo.GetValue(null, null));
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        public Brush GetRandom()
        {
            return this.brushes[this.random.Next(this.brushes.Count)];
        }

        #endregion
    }
}