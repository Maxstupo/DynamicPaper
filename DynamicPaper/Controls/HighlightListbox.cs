namespace Maxstupo.DynamicPaper.Controls {
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ListBoxItemHighlighting : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ListBoxItemDisplayText : Attribute { }

    public class HighlightListbox : ListBox {
        private PropertyInfo displayTextProperty;
        private PropertyInfo highlightedProperty;

        public Color SelectedForegroundColor { get; set; } = SystemColors.HighlightText;
        public Color SelectedBackgroundColor { get; set; } = SystemColors.Highlight;

        public Color HighlightedBackColor { get; set; } = ControlPaint.LightLight(ControlPaint.LightLight(Color.Black));

        public bool PerItemSchema { get; set; } = false;

        public HighlightListbox() {
            DoubleBuffered = true;
            IntegralHeight = false;
            SelectionMode = SelectionMode.MultiExtended;

            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += DoubleBufferedListBox_DrawItem;
        }

        public void RefreshSchemaChanged() {
            displayTextProperty = null;
            highlightedProperty = null;
            Refresh();
        }

        private void DoubleBufferedListBox_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0 || Items.Count == 0)
                return;

            object item = Items[e.Index];

            if (displayTextProperty == null || PerItemSchema)
                displayTextProperty = item.GetType().GetProperties().FirstOrDefault(x => x.GetCustomAttribute<ListBoxItemDisplayText>() != null && typeof(string).IsAssignableFrom(x.PropertyType));
            if (highlightedProperty == null || PerItemSchema)
                highlightedProperty = item.GetType().GetProperties().FirstOrDefault(x => x.GetCustomAttribute<ListBoxItemHighlighting>() != null && typeof(bool).IsAssignableFrom(x.PropertyType));

            string text = displayTextProperty != null ? (displayTextProperty.GetValue(item) as string) : GetItemText(item);

            bool isHighlighted = highlightedProperty != null && (bool) highlightedProperty.GetValue(item);


            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (Brush foregroundBrush = new SolidBrush(isSelected ? SelectedForegroundColor : ForeColor)) {

                if (isSelected) {
                    e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State ^ DrawItemState.Selected, SelectedForegroundColor, SelectedBackgroundColor);

                } else if (isHighlighted) {
                    e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State, ForeColor, HighlightedBackColor);

                }

                e.DrawBackground();

                e.Graphics.DrawString(text, e.Font, foregroundBrush, e.Bounds, StringFormat.GenericDefault);

                e.DrawFocusRectangle();
            }

        }

    }

}