namespace Maxstupo.DynamicPaper.Controls {

    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    public abstract class CyclingButton<T> : Button where T : Enum {

        protected readonly T[] Values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

        public int ValueIndex { get; set; } = 0;

        public T CurrentValue => Values[ValueIndex];

        public CyclingButton() {
            Click += CyclingButton_Click;
            UpdateDisplay();
        }

        public virtual string GetDisplayName(T value) {
            return value.ToString();
        }

        public virtual Image GetDisplayImage(T value) {
            return null;
        }

        protected void UpdateDisplay() {
            Text = GetDisplayName(CurrentValue);
            Image = GetDisplayImage(CurrentValue);
        }

        private void CyclingButton_Click(object sender, EventArgs e) {
            ValueIndex = ++ValueIndex % Values.Length;
            UpdateDisplay();
        }

    }

}