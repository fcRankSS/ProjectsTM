﻿using FreeGridControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using TaskManagement.Model;
using TaskManagement.Service;
using TaskManagement.ViewModel;
using System.Globalization;

namespace TaskManagement.UI
{
    public class WorkItemGrid : FreeGridControl.GridControl
    {
        private ViewData _viewData;

        private WorkItemDragService _workItemDragService = new WorkItemDragService();
        private UndoService _undoService = new UndoService();
        private WorkItemEditService _editService;
        private Cursor _originalCursor;

        public WorkItemEditService EditService => _editService;

        public event EventHandler<EditedEventArgs> UndoChanged;

        public event EventHandler<string> HoveringTextChanged;
        public event EventHandler<float> RatioChanged;

        private Dictionary<CallenderDay, RowIndex> _day2RowCache = new Dictionary<CallenderDay, RowIndex>();
        private Dictionary<RowIndex, CallenderDay> _row2DayChache = new Dictionary<RowIndex, CallenderDay>();
        private Dictionary<Member, ColIndex> _member2ColChache = new Dictionary<Member, ColIndex>();
        private Dictionary<ColIndex, Member> _col2MemberChache = new Dictionary<ColIndex, Member>();
        public WorkItemGrid() { }

        internal void Initialize(ViewData viewData)
        {
            LockUpdate = true;
            if (_viewData != null) DetatchEvents();
            this._viewData = viewData;
            AttachEvents();
            var fixedRows = 3;
            var fixedCols = 3;
            this.RowCount = _viewData.GetFilteredDays().Count + fixedRows;
            this.ColCount = _viewData.GetFilteredMembers().Count + fixedCols;
            this.FixedRowCount = fixedRows;
            this.FixedColCount = fixedCols;

            ApplyDetailSetting(_viewData.Detail);

            _editService = new WorkItemEditService(_viewData, _undoService);

            LockUpdate = false;
            UpdateCache();
        }

        DrawService _drawService;
        private void UpdateCache()
        {
            _day2RowCache.Clear();
            _row2DayChache.Clear();
            _member2ColChache.Clear();
            _col2MemberChache.Clear();

            if (_drawService != null)
            {
                _drawService.Dispose();
                _drawService = null;
            }
            _drawService = new DrawService(
                _viewData,
                new Size((int)GridWidth, (int)GridHeight),
                () => new Size(Width, Height),
                new SizeF(FixedWidth, FixedHeight),
                () => new Point(HOffset, VOffset),
                () => _workItemDragService.IsActive(),
                () => new RowColRange(VisibleNormalLeftCol, VisibleNormalTopRow, VisibleNormalColCount, VisibleNormalRowCount),
                GetMemberDrawRect,
                Col2Member,
                GetNeighbers,
                Row2Day,
                GetRect,
                GetDrawRect,
                this.Font);
        }

        IEnumerable<Member> GetNeighbers(IEnumerable<Member> members)
        {
            var neighbers = new HashSet<Member>();
            foreach (var m in members)
            {
                var c = Member2Col(m);
                var l = Col2Member(new ColIndex(c.Value - 1));
                var r = Col2Member(new ColIndex(c.Value + 1));

                neighbers.Add(m);
                if (l != null) neighbers.Add(l);
                if (r != null) neighbers.Add(r);
            }
            return neighbers;
        }

        private void _viewData_FilterChanged(object sender, EventArgs e)
        {
            UpdateCache();
        }

        internal void AdjustForPrint(Rectangle printRect)
        {
            var vRatio = printRect.Height / (float)GridHeight;
            var hRatio = printRect.Width / (float)GridWidth;
            LockUpdate = true;
            for (var c = 0; c < ColCount; c++)
            {
                ColWidths[c] = (ColWidths[c] * hRatio);
            }
            for (var r = 0; r < RowCount; r++)
            {
                RowHeights[r] = (RowHeights[r] * vRatio);
            }
            LockUpdate = false;
        }

        private void ApplyDetailSetting(Detail detail)
        {
            this.ColWidths[0] = detail.DateWidth / 2;
            this.ColWidths[1] = detail.DateWidth / 4;
            this.ColWidths[2] = detail.DateWidth / 4;
            for (var c = FixedColCount; c < ColCount; c++)
            {
                this.ColWidths[c] = detail.ColWidth;
            }
            this.RowHeights[0] = detail.CompanyHeight;
            this.RowHeights[1] = detail.NameHeight;
            this.RowHeights[2] = detail.NameHeight;
            for (var r = FixedRowCount; r < RowCount; r++)
            {
                this.RowHeights[r] = detail.RowHeight;
            }
        }

        private void _undoService_Changed(object sender, EditedEventArgs e)
        {
            UndoChanged?.Invoke(this, e);
            _drawService.InvalidateMembers(e.UpdatedMembers);
            this.Invalidate();
        }

        ColIndex Member2Col(Member m)
        {
            return Member2Col(m, _viewData.GetFilteredMembers());
        }

        RectangleF GetMemberDrawRect(Member m)
        {
            var col = Member2Col(m, _viewData.GetFilteredMembers());
            return new RectangleF(GetLeft(col), FixedHeight, ColWidths[col.Value], GridHeight);
        }

        private void AttachEvents()
        {
            this._viewData.SelectedWorkItemChanged += _viewData_SelectedWorkItemChanged;
            this._viewData.FontChanged += _viewData_FontChanged;
            this._viewData.FilterChanged += _viewData_FilterChanged;
            this.OnDrawNormalArea += WorkItemGrid_OnDrawNormalArea;
            this.MouseDown += WorkItemGrid_MouseDown;
            this.MouseUp += WorkItemGrid_MouseUp;
            this.MouseDoubleClick += WorkItemGrid_MouseDoubleClick;
            this.MouseWheel += WorkItemGrid_MouseWheel;
            this._undoService.Changed += _undoService_Changed;
            this.MouseMove += WorkItemGrid_MouseMove;
            this.KeyDown += WorkItemGrid_KeyDown;
            this.KeyUp += WorkItemGrid_KeyUp;
        }

        private void DetatchEvents()
        {
            this._viewData.SelectedWorkItemChanged -= _viewData_SelectedWorkItemChanged;
            this._viewData.FontChanged -= _viewData_FontChanged;
            this._viewData.FilterChanged -= _viewData_FilterChanged;
            this.OnDrawNormalArea -= WorkItemGrid_OnDrawNormalArea;
            this.MouseDown -= WorkItemGrid_MouseDown;
            this.MouseUp -= WorkItemGrid_MouseDown;
            this.MouseDoubleClick -= WorkItemGrid_MouseDoubleClick;
            this.MouseWheel -= WorkItemGrid_MouseWheel;
            this._undoService.Changed -= _undoService_Changed;
            this.MouseMove -= WorkItemGrid_MouseMove;
            this.KeyDown -= WorkItemGrid_KeyDown;
        }

        private void _viewData_FontChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void WorkItemGrid_MouseWheel(object sender, MouseEventArgs e)
        {
            if (IsControlDown())
            {
                if (e.Delta > 0)
                {
                    IncRatio();
                }
                else
                {
                    DecRatio();
                }
            }
        }

        private void WorkItemGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                _workItemDragService.ToCopyMode(_viewData.Original.WorkItems);
            }
            if (e.KeyCode == Keys.Escape)
            {
                _workItemDragService.End(_editService, _viewData, true);
                _viewData.Selected = null;
            }
            this.Invalidate();
        }

        private void WorkItemGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (_viewData.Selected == null) return;
                _editService.Delete(_viewData.Selected);
                _viewData.Selected = null;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                _workItemDragService.ToMoveMode(_viewData.Original.WorkItems, _drawService.InvalidateMembers);
            }
            this.Invalidate();
        }

        private void WorkItemGrid_MouseUp(object sender, MouseEventArgs e)
        {
            using (new RedrawLock(_drawService, () => this.Invalidate()))
            {
                _workItemDragService.End(_editService, _viewData, false);
            }
        }

        private bool ScrollOneStep(ScrollDirection direction)
        {
            const int scrollCellCount = 5;
            switch (direction)
            {
                case ScrollDirection.RIGHT: ScrollHorizontal(_viewData.Detail.ColWidth * scrollCellCount); break;
                case ScrollDirection.LEFT: ScrollHorizontal(-_viewData.Detail.ColWidth * scrollCellCount); break;
                case ScrollDirection.LOWER: ScrollVertical(_viewData.Detail.RowHeight * scrollCellCount); break;
                case ScrollDirection.UPPER: ScrollVertical(-_viewData.Detail.RowHeight * scrollCellCount); break;
                default: return false;
            }
            return true;
        }

        public bool ScrollAndUpdate(ScrollDirection direction)
        {
            if (!ScrollOneStep(direction)) return false;
            return true;
        }

        private void ScrollByDragToOutsideOfPanel(Point mouseLocationOnTaskGrid)
        {
            if (!_workItemDragService.Scroll(mouseLocationOnTaskGrid, this)) return;
            Thread.Sleep(500);
        }

        private void WorkItemGrid_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateHoveringText(e);
            _workItemDragService.UpdateDraggingItem(X2Member, Y2Day, e.Location, _viewData);
            if (_workItemDragService.IsMoving()) ScrollByDragToOutsideOfPanel(e.Location);
            if (IsWorkItemExpandArea(_viewData, e.Location))
            {
                if (this.Cursor != Cursors.SizeNS)
                {
                    _originalCursor = this.Cursor;
                    this.Cursor = Cursors.SizeNS;
                }
            }
            else
            {
                if (this.Cursor == Cursors.SizeNS)
                {
                    this.Cursor = _originalCursor;
                }
            }
            this.Invalidate();
        }

        private void UpdateHoveringText(MouseEventArgs e)
        {
            if (_workItemDragService.IsMoving()) return;
            var wi = _viewData.PickFilterdWorkItem(X2Member(e.X), Y2Day(e.Y));
            HoveringTextChanged?.Invoke(this, wi == null ? string.Empty : wi.ToString());
        }

        private void WorkItemGrid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_viewData.Selected != null)
            {
                EditSelectedWorkItem();
                return;
            }
            var day = Y2Day(e.Location.Y);
            var member = X2Member(e.Location.X);
            if (day == null || member == null) return;
            var proto = new WorkItem(new Project(""), "", new Tags(new List<string>()), new Period(day, day), member);
            AddNewWorkItem(proto);
        }

        public void AddNewWorkItem(WorkItem proto)
        {
            using (var dlg = new EditWorkItemForm(proto, _viewData.Original.Callender))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var wi = dlg.GetWorkItem(_viewData.Original.Callender);
                _viewData.UpdateCallenderAndMembers(wi);
                _editService.Add(wi);
                _undoService.Push();
            }
        }

        public void EditSelectedWorkItem()
        {
            var wi = _viewData.Selected;
            if (wi == null) return;
            using (var dlg = new EditWorkItemForm(wi.Clone(), _viewData.Original.Callender))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var newWi = dlg.GetWorkItem(_viewData.Original.Callender);
                _viewData.UpdateCallenderAndMembers(newWi);
                _editService.Replace(wi, newWi);
                _viewData.Selected = newWi;
            }
        }

        private void _viewData_SelectedWorkItemChanged(object sender, SelectedWorkItemChangedArg e)
        {
            _drawService.InvalidateMembers(e.UpdatedMembers);
            if (_viewData.Selected != null) MoveVisibleRowCol(GetRowRange(_viewData.Selected).row, Member2Col(_viewData.Selected.AssignedMember, _viewData.GetFilteredMembers()));
            this.Invalidate();
        }

        private void MoveVisibleDayAndMember(CallenderDay day, Member m)
        {
            if (day == null || m == null) return;
            var row = Day2Row(day);
            if (row == null) return;
            MoveVisibleRowCol(row, Member2Col(m, _viewData.GetFilteredMembers()));
        }

        private void WorkItemGrid_MouseDown(object sender, MouseEventArgs e)
        {
            this.ActiveControl = null;

            if (IsWorkItemExpandArea(_viewData, e.Location))
            {
                if (e.Button != MouseButtons.Left) return;
                _workItemDragService.StartExpand(GetExpandDirection(_viewData, e.Location), _viewData.Selected);
                return;
            }

            var wi = PickWorkItemFromPoint(e.Location);
            _viewData.Selected = wi;
            _workItemDragService.StartMove(_viewData.Selected, e.Location, Y2Day(e.Location.Y));
        }

        private int GetExpandDirection(ViewData viewData, Point location)
        {
            if (viewData.Selected == null) return 0;
            var bounds = GetDrawRect(viewData.Selected, viewData.GetFilteredMembers(), true);
            if (!bounds.HasValue) return 0;
            if (IsTopBar(bounds.Value, location)) return +1;
            if (IsBottomBar(bounds.Value, location)) return -1;
            return 0;
        }

        private bool IsWorkItemExpandArea(ViewData viewData, Point location)
        {
            if (viewData.Selected == null) return false;
            var bounds = GetDrawRect(viewData.Selected, viewData.GetFilteredMembers(), true);
            if (!bounds.HasValue) return false;
            return IsTopBar(bounds.Value, location) || IsBottomBar(bounds.Value, location);
        }

        internal bool IsTopBar(RectangleF workItemBounds, PointF point)
        {
            var topBar = WorkItemDragService.GetTopBarRect(workItemBounds);
            return topBar.Contains(point);
        }

        internal bool IsBottomBar(RectangleF workItemBounds, PointF point)
        {
            var bottomBar = WorkItemDragService.GetBottomBarRect(workItemBounds);
            return bottomBar.Contains(point);
        }

        private CallenderDay Y2Day(int y)
        {
            if (GridHeight < y) return null;
            var r = Y2Row(y);
            return Row2Day(r);
        }

        private CallenderDay Row2Day(RowIndex r)
        {
            if (_row2DayChache.TryGetValue(r, out var day)) return day;
            if (r == null) return null;
            var days = _viewData.GetFilteredDays();
            if (r.Value - FixedRowCount < 0 || days.Count <= r.Value - FixedRowCount) return null;
            day = days.ElementAt(r.Value - FixedRowCount);
            _row2DayChache.Add(r, day);
            return day;
        }

        private Member X2Member(int x)
        {
            if (GridWidth < x) return null;
            var c = X2Col(x);
            return Col2Member(c);
        }

        private Member Col2Member(ColIndex c)
        {
            if (_col2MemberChache.TryGetValue(c, out var member)) return member;
            if (c == null) return null;
            var members = _viewData.GetFilteredMembers();
            if (c.Value - FixedColCount < 0 || members.Count <= c.Value - FixedColCount) return null;
            var result = _viewData.GetFilteredMembers().ElementAt(c.Value - FixedColCount);
            _col2MemberChache.Add(c, result);
            return result;
        }

        private WorkItem PickWorkItemFromPoint(Point location)
        {
            var m = X2Member(location.X);
            var d = Y2Day(location.Y);
            if (m == null || d == null) return null;
            return _viewData.PickFilterdWorkItem(m, d);
        }

        internal void MoveToToday()
        {
            var m = _viewData.Selected == null ? X2Member(0) : _viewData.Selected.AssignedMember;
            var now = DateTime.Now;
            var today = new CallenderDay(now.Year, now.Month, now.Day);
            MoveVisibleDayAndMember(today, m);
        }

        private void WorkItemGrid_OnDrawNormalArea(object sender, DrawNormalAreaEventArgs e)
        {
            _drawService.Draw(e.Graphics);
        }

        internal void DecRatio()
        {
            _viewData.DecRatio();
            RatioChanged?.Invoke(this, _viewData.Detail.ViewRatio);
        }

        internal void IncRatio()
        {
            _viewData.IncRatio();
            RatioChanged?.Invoke(this, _viewData.Detail.ViewRatio);
        }

        private RectangleF? GetDrawRect(WorkItem wi, Members members, bool isFrontView)
        {
            var rowRange = GetRowRange(wi);
            if (rowRange.row == null) return null;
            return GetRect(Member2Col(wi.AssignedMember, members), rowRange.row, rowRange.count, false, false, isFrontView);
        }

        private ColIndex Member2Col(Member m, Members members)
        {
            if (_member2ColChache.TryGetValue(m, out var col)) return col;
            foreach (var c in ColIndex.Range(0, ColCount))
            {
                if (members.ElementAt(c.Value).Equals(m))
                {
                    var result = c.Offset(FixedColCount);
                    _member2ColChache.Add(m, result);
                    return result;
                }
            }
            Debug.Assert(false);
            return null;
        }

        private (RowIndex row, int count) GetRowRange(WorkItem wi)
        {
            RowIndex row = null;
            int count = 0;
            foreach (var d in _viewData.Original.Callender.GetPediodDays(wi.Period))
            {
                if (!_viewData.GetFilteredDays().Contains(d)) continue;
                if (row == null)
                {
                    row = Day2Row(d);
                }
                count++;
            }
            return (row, count);
        }

        internal void Redo()
        {
            _undoService.Redo(_viewData);
        }

        internal void Undo()
        {
            _undoService.Undo(_viewData);
        }

        private RowIndex Day2Row(CallenderDay day)
        {
            if (_day2RowCache.TryGetValue(day, out var row)) return row;
            foreach (var r in RowIndex.Range(FixedRowCount, RowCount - FixedRowCount))
            {
                if (_viewData.GetFilteredDays().ElementAt(r.Value - FixedRowCount).Equals(day))
                {
                    _day2RowCache.Add(day, r);
                    return r;
                }
            }
            return null;
        }
    }
}