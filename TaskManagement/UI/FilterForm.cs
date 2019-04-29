﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using TaskManagement.Model;
using TaskManagement.ViewModel;

namespace TaskManagement.UI
{
    public partial class FilterForm : Form
    {
        private ViewData _viewData;

        public FilterForm(ViewData viewData)
        {
            InitializeComponent();

            _viewData = viewData;
            checkedListBox1.DisplayMember = "NaturalString";

            foreach (var m in _viewData.Original.Members)
            {
                var check = _viewData.Filter == null ? true : !_viewData.Filter.HideMembers.Contain(m);
                checkedListBox1.Items.Add(m, check);
            }

            if (viewData.Filter == null || viewData.Filter.Period == null)
            {
                ClearPeriodFilter();
            }
            else
            {
                textBoxFrom.Text = viewData.Filter.Period.From.ToString();
                textBoxTo.Text = viewData.Filter.Period.To.ToString();
            }

            if (viewData.Filter != null && !string.IsNullOrEmpty(viewData.Filter.WorkItem))
            {
                textBoxWorkItem.Text = viewData.Filter.WorkItem;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if(!IsValid()) return;
            _viewData.SetFilter(GetFilter());
            Close();
        }

        private bool IsValid()
        {
            var from = textBoxFrom.Text;
            var to = textBoxTo.Text;
            if (string.IsNullOrEmpty(from) && string.IsNullOrEmpty(to)) return true;
            var fromDay = CallenderDay.Parse(textBoxFrom.Text);
            var toDay = CallenderDay.Parse(textBoxTo.Text);
            return fromDay != null && toDay != null;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Filter GetFilter()
        {
            return new Filter(GetWorkItemFilter(), GetPeriodFilter(), GetMembersFilter());
        }

        private string GetWorkItemFilter()
        {
            if (string.IsNullOrEmpty(textBoxWorkItem.Text)) return null;
            return textBoxWorkItem.Text;
        }

        private Period GetPeriodFilter()
        {
            var from = CallenderDay.Parse(textBoxFrom.Text);
            var to = CallenderDay.Parse(textBoxTo.Text);
            if (from == null || to == null) return null;
            if (!_viewData.Original.Callender.Days.Contains(from)) return null;
            if (!_viewData.Original.Callender.Days.Contains(to)) return null;
            return new Period(from, to);
        }

        private Members GetMembersFilter()
        {
            var result = new Members();
            var remains = GetRemainingMemger();
            foreach (var m in _viewData.Original.Members)
            {
                if (remains.Contain(m)) continue;
                result.Add(m);
            }
            return result;
        }

        private Members GetRemainingMemger()
        {
            var result = new Members();
            foreach (var c in checkedListBox1.CheckedItems)
            {
                if (c is Member m) result.Add(m);
            }
            return result;
        }

        private void buttonClearWorkItem_Click(object sender, EventArgs e)
        {
            textBoxWorkItem.Text = string.Empty;
        }

        private void buttonClearPeriod_Click(object sender, EventArgs e)
        {
            ClearPeriodFilter();
        }

        private void ClearPeriodFilter()
        {
            var days = _viewData.Original.Callender.Days;
            if (days.Count == 0) return;
            textBoxFrom.Text = days.First().ToString();
            textBoxTo.Text = days.Last().ToString();
        }

        private void buttonClearMembers_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < checkedListBox1.Items.Count; index++)
            {
                checkedListBox1.SetItemChecked(index, true);
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            using(var dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                using (var reader = new StreamReader(dlg.FileName))
                {
                    var s = new XmlSerializer(typeof(Members));
                    var remain = (Members)s.Deserialize(reader);

                    for(var index = 0; index < checkedListBox1.Items.Count; index++)
                    {
                        var m = checkedListBox1.Items[index] as Member;
                        checkedListBox1.SetItemChecked(index, remain.Contain(m));
                    }
                }
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            using(var dlg = new SaveFileDialog())
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                using (var writer = new StreamWriter(dlg.FileName))
                {
                    var s = new XmlSerializer(typeof(Members));
                    s.Serialize(writer, GetRemainingMemger());
                }
            }
        }
    }
}
