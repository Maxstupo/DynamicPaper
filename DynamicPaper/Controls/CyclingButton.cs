namespace Maxstupo.DynamicPaper.Controls {

    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    public abstract class CyclingButton<T> : Button where T : Enum {

        protected static readonly T[] Values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

        public int ValueIndex { get; private set; } = 0;

        public T Value {
            get => Values[ValueIndex];
            set {
                ValueIndex = GetIndex(value);
                UpdateDisplay();
            }
        }

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
            Text = GetDisplayName(Value);
            Image = GetDisplayImage(Value);
        }

        private void CyclingButton_Click(object sender, EventArgs e) {
            ValueIndex = ++ValueIndex % Values.Length;
            UpdateDisplay();
        }

        public int GetIndex(T value) {
            for (int i = 0; i < Values.Length; i++) {
                if (Values[i].Equals(value))
                    return i;
            }
            return -1;
        }

    }

}