using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KindleToOneNote
{
    public partial class AmazonRegistrationForm : Form
    {
        public AmazonRegistrationForm()
        {
            InitializeComponent();
        }

        private void buttonFetchKindleData_Click(object sender, EventArgs e)
        {
            User = new User();
            User.UserName = textBoxEmailAddress.Text;
            User.Password = textBoxPassword.Text;
            Close();
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            User = null;
            Close();
        }

        public User User { get; private set; }

    }
}
