using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls
{
    public class EditTextBox : TextBox
    {
        #region Fields

        public bool FireTextChanged = true;
        private bool showDropDown = true;
        public bool AllowTextChanged = true;

        #endregion

        #region Properties

        private Record selectedRecord;

        public Record SelectedRecord
        {
            get
            {
                return selectedRecord;
            }
            set
            {
                selectedRecord = value;
            }
        }

        public bool ShowDropDown
        {
            get { return showDropDown; }
            set { showDropDown = value; }
        }

        #endregion       

        #region Constructors

        public EditTextBox()
        {
            this.CharacterCasing = CharacterCasing.Upper;
            this.Name = "MCWLTextBox";
            this.Size = new System.Drawing.Size(130, 32);
        }

        #endregion
        
        #region Base Overrides

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                FireTextChanged = false;

                if(AllowTextChanged)
                    base.Text = value;

                AllowTextChanged = true;

                FireTextChanged = true;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            FireTextChanged = !this.Visible;
        }

        #endregion
    }
}
