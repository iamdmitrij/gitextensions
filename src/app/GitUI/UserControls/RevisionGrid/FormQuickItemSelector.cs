﻿namespace GitUI.UserControls.RevisionGrid
{
    internal partial class FormQuickItemSelector : GitExtensionsForm
    {
        private const short MaxVisibleItemsWithoutScroll = 8;
        private const short MaxRefLength = 100;

        public FormQuickItemSelector()
        {
            InitializeComponent();
            InitializeComplete();

            lbxRefs.DisplayMember = nameof(ItemData.Label);
        }

        /// <summary>
        /// Gets the item selected by the user.
        /// </summary>
        public object? SelectedItem => (lbxRefs.SelectedItem as ItemData)?.Item;

        protected void Init(IReadOnlyList<ItemData> items, string buttonText, int selectedIndex = 0)
        {
            btnAction.Text = buttonText;

            lbxRefs.Items.Clear();
            if (items.Count == 0)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // constrain the listbox min width, otherwise the button may not have enough space
            float longestItemWidth = 150f;
            try
            {
                lbxRefs.BeginUpdate();

                using Graphics graphics = lbxRefs.CreateGraphics();
                foreach (ItemData item in items)
                {
                    lbxRefs.Items.Add(item);

                    // assume that the branch names or tags are never longer than MaxRefLength symbols long
                    // if they are (sanity!) - don't resize past beyond certain limit
                    string label = item.Label.Length > MaxRefLength ? item.Label[..MaxRefLength] : item.Label;
                    longestItemWidth = Math.Max(longestItemWidth, graphics.MeasureString(label, lbxRefs.Font).Width);
                }
            }
            finally
            {
                // What is this magic number "MaxVisibleItemsWithoutScroll + 0.5"?
                //
                // We are limiting number of visible items in the listbox before enabling vscroll, this is to reduce the risk of not fitting on user's screen.
                // When listbox.IntegralHeight=true, the listbox automatically calculates its size and only show full items (i.e. you can't render it
                // at 20px high if the ItemHeight=13px, it can only be 13, 26, 39 etc (NB: slightly more if you account for the furniture)).
                // listbox.PreferredHeight returns the height of the listbox showing all items.
                //
                // For some strange reason, MaxVisibleItemsWithoutScroll*listbox.ItemHeight renders the listbox showing MaxVisibleItemsWithoutScroll-1 items.
                //
                // To fix this, trick the listbox and set its height to show MaxVisibleItemsWithoutScroll+0.5
                // this internally forces it to re-calculates its bounds and then to calculate its preferred height.

                lbxRefs.Height = Math.Min(lbxRefs.PreferredHeight, (int)((MaxVisibleItemsWithoutScroll + 0.5) * lbxRefs.ItemHeight));
                lbxRefs.Width = (int)longestItemWidth;
                lbxRefs.EndUpdate();

                if (selectedIndex >= 0 && selectedIndex < lbxRefs.Items.Count)
                {
                    lbxRefs.SelectedIndex = selectedIndex;
                }

                lbxRefs.Focus();

                // resize the form to look nice
                Size = new Size(lbxRefs.Width + Padding.Left + Padding.Right,
                                lbxRefs.Height + Padding.Top + flowLayoutPanel1.Height);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lbxRefs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            AcceptButton.PerformClick();
        }

        internal sealed class ItemData
        {
            public string Label { get; }
            public object Item { get; }

            public ItemData(string label, object item)
            {
                Label = label;
                Item = item;
            }
        }
    }
}
