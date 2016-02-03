#region License

/*
 Copyright 2013 - 2016 Nikita Bernthaler
 MainViewModel.cs is part of Masonry.Example.

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

namespace Masonry.Example.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using Masonry.Example.Common;

    using Prism.Commands;
    using Prism.Mvvm;

    internal class MainViewModel : BindableBase
    {
        #region Fields

        private readonly Random random;

        private readonly RandomBrush randomBrush;

        private string amount;

        private ObservableCollection<FrameworkElement> elements;

        private string header;

        #endregion

        #region Constructors and Destructors

        public MainViewModel()
        {
            this.random = new Random();
            this.randomBrush = new RandomBrush();
            this.Elements = new ObservableCollection<FrameworkElement>();
            this.AddClickCommand = new DelegateCommand(this.OnAddButtonClick);
            this.ResetClickCommand = new DelegateCommand(this.OnResetButtonClick);
            this.Header = "Masonry Example";
            this.Amount = "5";
        }

        #endregion

        #region Public Properties

        public ICommand AddClickCommand { get; private set; }

        public string Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                this.SetProperty(ref this.amount, value);
            }
        }

        public ObservableCollection<FrameworkElement> Elements
        {
            get
            {
                return this.elements;
            }
            set
            {
                this.SetProperty(ref this.elements, value);
            }
        }

        public string Header
        {
            get
            {
                return this.header;
            }
            private set
            {
                this.SetProperty(ref this.header, value);
            }
        }

        public ICommand ResetClickCommand { get; private set; }

        #endregion

        #region Methods

        private void OnAddButtonClick()
        {
            int amountValue;
            if (int.TryParse(this.Amount, out amountValue))
            {
                for (var i = 0; i < amountValue; i++)
                {
                    this.Elements.Add(
                        new Border
                            {
                                Width = 200, Height = this.random.Next(100, 300), BorderThickness = new Thickness(1),
                                BorderBrush = Brushes.Black, Background = this.randomBrush.GetRandom()
                            });
                }
            }
        }

        private void OnResetButtonClick()
        {
            this.Elements.Clear();
        }

        #endregion
    }
}