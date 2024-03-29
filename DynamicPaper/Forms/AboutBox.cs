﻿namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Forms;

    partial class AboutBox : Form {

        #region Assembly Attribute Accessors

        public string AssemblyTitle {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0) {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != string.Empty)
                        return titleAttribute.Title;
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AssemblyDescription {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                    return string.Empty;
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public string AssemblyProduct {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                    return string.Empty;
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                    return string.Empty;
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                    return string.Empty;
                return ((AssemblyCompanyAttribute) attributes[0]).Company;
            }
        }
        #endregion

        public AboutBox() {
            InitializeComponent();
        }

        private void AboutBox_Load(object sender, EventArgs e) {
            lblTitle.Text = ProductName;
            lblVersion.Text = ProductVersion;

        }

        private void btnOkay_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }

        private void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            string url = (string) ((Control) sender).Tag;
            Process.Start(url);
        }

    }

}