﻿using ProjectsTM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ProjectsTM.UI.Common
{
    public partial class EditWorkItemForm : BaseForm
    {
        private readonly Callender _callender;
        private readonly IEnumerable<Member> _members;

        public EditWorkItemForm(WorkItem wi, WorkItems workItems, Callender callender, IEnumerable<Member> members)
        {
            InitializeComponent();
            if (wi == null) wi = new WorkItem();
            this._callender = callender;
            this._members = members;
            comboBoxWorkItemName.Text = wi.Name ?? string.Empty;
            comboBoxProject.Text = wi.Project == null ? string.Empty : wi.Project.ToString();
            comboBoxMember.Text = wi.AssignedMember == null ? string.Empty : wi.AssignedMember.ToSerializeString();
            textBoxFrom.Text = wi.Period == null ? string.Empty : wi.Period.From.ToString();
            textBoxTo.Text = wi.Period == null ? string.Empty : _callender.GetPeriodDayCount(wi.Period).ToString();
            textBoxTags.Text = wi.Tags == null ? string.Empty : wi.Tags.ToString();
            textBoxDescription.Text = wi.Description ?? string.Empty;
            InitDropDownList(wi.State);
            InitCombbox(members, workItems);
            UpdateEndDay();
        }

        private void InitDropDownList(TaskState state)
        {
            comboBoxState.Items.Clear();
            foreach (TaskState e in Enum.GetValues(typeof(TaskState)))
            {
                if (e == TaskState.New) continue;
                comboBoxState.Items.Add(e);
            }
            comboBoxState.SelectedItem = state;
        }

        private void InitCombbox(IEnumerable<Member> members, WorkItems workItems)
        {
            foreach (var m in members)
            {
                comboBoxMember.Items.Add(m.ToSerializeString());
            }
            comboBoxWorkItemName.Items.AddRange(GetTasks(workItems));
            comboBoxProject.Items.AddRange(GetProjects(workItems).ToArray());
        }
        private static List<Project> GetProjects(WorkItems workItems)
        {
            var result = new List<Project>();
            foreach (var wi in workItems)
            {
                if (!result.Contains(wi.Project)) result.Add(wi.Project);
            }
            return result;
        }
        private static string[] GetTasks(WorkItems workItems)
        {
            var result = new List<string>();
            foreach (var wi in workItems)
            {
                if (!result.Contains(wi.Name)) result.Add(wi.Name);
            }
            return result.ToArray();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!CheckEdit()) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonRegexEscape_Click(object sender, EventArgs e)
        {
            var wi = CreateWorkItem(_callender);
            if (wi == null) return;
            using (var dlg = new EditMemberForm(Regex.Escape(wi.ToString())))
            {
                dlg.Text = "正規表現エスケープ";
                dlg.ReadOnly = true;
                dlg.ShowDialog();
            }
        }

        private bool ValidateAssignedMember()
        {
            return _members.Contains(Member.Parse(comboBoxMember.Text));
        }

        bool CheckEdit()
        {
            if (!ValidateAssignedMember())
            {
                MessageBox.Show("担当者が存在しません。", "不正な入力", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return CreateWorkItem(_callender) != null;
        }

        private WorkItem CreateWorkItem(Callender callender)
        {
            var p = GetProject();
            if (p == null) return null;
            var w = GetWorkItemName();
            if (w == null) return null;
            var period = GetPeriod(callender, textBoxFrom.Text, textBoxTo.Text);
            if (period == null) return null;
            var m = GetAssignedMember();
            if (m == null) return null;
            return new WorkItem(p, w, GetTags(), period, m, GetState(), GetDescrption());
        }

        private Member GetAssignedMember()
        {
            return Member.Parse(comboBoxMember.Text);
        }

        private static Period GetPeriod(Callender callender, string fromText, string toText)
        {
            var from = GetDayByDate(fromText);
            if (!TryGetDayByCount(toText, from, callender, out var to)) return null;
            if (from == null || to == null) return null;
            var result = new Period(from, to);
            if (callender.GetPeriodDayCount(result) == 0) return null;
            return result;
        }

        private static CallenderDay GetDayByDate(string text)
        {
            return CallenderDay.Parse(text);
        }

        private static bool TryGetDayByCount(string countText, CallenderDay from, Callender callender, out CallenderDay result)
        {
            result = CallenderDay.Invalid;
            if (!int.TryParse(countText, out int dayCount)) return false;
            return callender.TryApplyOffset(from, dayCount - 1, out result);
        }

        private Tags GetTags()
        {
            return new Tags(textBoxTags.Text.Split('|').ToList());
        }

        private string GetWorkItemName()
        {
            if (string.IsNullOrEmpty(comboBoxWorkItemName.Text)) return null;
            return comboBoxWorkItemName.Text;
        }

        private Project GetProject()
        {
            return new Project(comboBoxProject.Text);
        }

        public WorkItem GetWorkItem()
        {
            var period = GetPeriod(_callender, textBoxFrom.Text, textBoxTo.Text);
            return new WorkItem(GetProject(), GetWorkItemName(), GetTags(), period, GetAssignedMember(), GetState(), GetDescrption());
        }

        private string GetDescrption() { return textBoxDescription.Text; }

        private TaskState GetState()
        {
            return (TaskState)comboBoxState.SelectedItem;
        }

        private void UpdateEndDay()
        {
            var period = GetPeriod(_callender, textBoxFrom.Text, textBoxTo.Text);
            textBoxTo.Text = period == null ? string.Empty : _callender.GetPeriodDayCount(period).ToString();
        }
    }
}
