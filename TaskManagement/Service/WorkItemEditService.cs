﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TaskManagement.Model;
using TaskManagement.ViewModel;

namespace TaskManagement.Service
{
    public class WorkItemEditService
    {
        private readonly ViewData _viewData;
        private readonly UndoService _undoService;

        public WorkItemEditService(ViewData viewData, UndoService undoService)
        {
            this._viewData = viewData;
            this._undoService = undoService;
        }

        public void Add(WorkItems wis)
        {
            if (wis == null) return;
            var items = _viewData.Original.WorkItems;
            foreach (var w in wis)
            {
                if (items.Contains(w)) continue;
                items.Add(w);
                _undoService.Add(w);
            }
            _undoService.Push();
        }

        public void Add(WorkItem wi)
        {
            if (wi == null) return;
            var items = _viewData.Original.WorkItems;
            if (items.Contains(wi)) return;
            items.Add(wi);
            _undoService.Add(wi);
            _undoService.Push();
        }

        internal void Delete()
        {
            _viewData.Original.WorkItems.Remove(_viewData.Selected);
            _undoService.Delete(_viewData.Selected);
            _undoService.Push();
        }

        internal void Devide(WorkItem selected, int devided, int remain)
        {
            var d1 = selected.Clone();
            var d2 = selected.Clone();

            d1.Period.To = _viewData.Original.Callender.ApplyOffset(d1.Period.To, -remain);
            d2.Period.From = _viewData.Original.Callender.ApplyOffset(d2.Period.From, devided);

            _undoService.Delete(selected);
            _undoService.Add(d1);
            _undoService.Add(d2);
            _undoService.Push();

            var workItems = _viewData.Original.WorkItems;
            _viewData.Selected = null;
            workItems.Remove(selected);
            workItems.Add(d1);
            workItems.Add(d2);
        }

        internal void Replace(WorkItems before, WorkItems after)
        {
            _viewData.Original.WorkItems.Remove(before);
            _viewData.Original.WorkItems.Add(after);
            _undoService.Delete(before);
            _undoService.Add(after);
            _undoService.Push();
        }

        internal void Replace(WorkItem before, WorkItem after)
        {
            if (before.Equals(after)) return;
            _viewData.Original.WorkItems.Remove(before);
            _viewData.Original.WorkItems.Add(after);
            _undoService.Delete(before);
            _undoService.Add(after);
            _undoService.Push();
        }

        internal void Done(WorkItems selected)
        {
            var done = selected.Clone();

            foreach (var w in done) w.State = TaskState.Done;

            _undoService.Delete(selected);
            _undoService.Add(done);
            _undoService.Push();

            var workItems = _viewData.Original.WorkItems;
            _viewData.Selected = null;
            workItems.Remove(selected);
            workItems.Add(done);
        }

        internal void SelectAfterward(WorkItems starts)
        {
            var newSetected = new WorkItems();
            foreach (var s in starts)
            {
                newSetected.Add(GetSameMemberAfterItems(s));
            }
            _viewData.Selected = newSetected;
        }

        private WorkItems GetSameMemberAfterItems(WorkItem s)
        {
            var result = new WorkItems();
            foreach (var w in _viewData.GetFilteredWorkItemsOfMember(s.AssignedMember))
            {
                if (_viewData.Original.Callender.GetOffset(s.Period.From, w.Period.From) >= 0)
                {
                    if (result.Contains(w)) continue;
                    result.Add(w);
                }
            }
            return result;
        }

        internal void AlignAfterward(WorkItems starts)
        {
            if (HasSameMember(starts)) return;
            var before = new WorkItems();
            var after = new WorkItems();
            var cal = _viewData.Original.Callender;
            foreach (var s in starts)
            {
                var lastTo = s.Period.To;
                foreach (var w in GetSameMemberAfterItems(s).OrderBy(o => o.Period.From))
                {
                    if (w.Equals(s)) continue;
                    before.Add(w.Clone());
                    var nextFrom = cal.ApplyOffset(lastTo, 1);
                    var offset = cal.GetOffset(w.Period.From, nextFrom);
                    var a = w.Clone();
                    a.Period = w.Period.ApplyOffset(offset, cal);
                    lastTo = a.Period.To;
                    after.Add(a);
                }
            }
            Replace(before, after);
        }

        private static bool HasSameMember(WorkItems starts)
        {
            var members = new List<Member>();
            foreach (var s in starts)
            {
                if (members.Contains(s.AssignedMember)) return true;
                members.Add(s.AssignedMember);
            }
            return false;
        }
    }
}
