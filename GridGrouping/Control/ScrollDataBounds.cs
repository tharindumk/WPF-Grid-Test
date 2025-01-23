using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls
{
    public class ScrollDataBounds : INotifyPropertyChanged
    {
        #region Properties

        private Visibility visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                NotifyPropertyChanged("Visibility");
            }
        }

        private int maximum = 100;

        public int Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;

                if (maximum == 0)
                {
                    if (Value > 0)
                    {
                        Value = 0;
                        FireValueChangedEvent();
                    }

                    Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    Visibility = System.Windows.Visibility.Visible;
                }

                NotifyPropertyChanged("Maximum");
            }
        }

        private int minimum = 0;

        public int Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;
                NotifyPropertyChanged("Minimum");
            }
        }

        private int value = 0;

        public int Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                NotifyPropertyChanged("Value");
            }
        }

        private int viewportSize = 0;

        public int ViewportSize
        {
            get { return viewportSize; }
            set
            {
                viewportSize = value;
                NotifyPropertyChanged("ViewportSize");
            }
        }

        #endregion

        #region Events

        public void FireValueChangedEvent()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, new EventArgs());
            }
        }

        public event EventHandler ValueChanged;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
