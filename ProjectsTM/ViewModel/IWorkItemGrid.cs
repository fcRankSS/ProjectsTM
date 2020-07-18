﻿using FreeGridControl;
using System.Collections.Generic;
using System.Drawing;
using ProjectsTM.Model;
using ProjectsTM.UI;

namespace ProjectsTM.ViewModel
{
    interface IWorkItemGrid
    {
        Member X2Member(int x);
        ColIndex X2Col(int x);
        Member Col2Member(ColIndex c);
        ColIndex Member2Col(Member m);
        CallenderDay Y2Day(int y);
        CallenderDay Row2Day(RowIndex r);
        RowIndex Y2Row(int y);

        Rectangle? GetMemberDrawRect(Member m);
        Rectangle? GetRect(ColIndex col, RowIndex r, int rowCount, bool isFixedRow, bool isFixedCol, bool isFrontView);
        Rectangle? GetWorkItemDrawRect(WorkItem wi, IEnumerable<Member> members, bool isFrontView);
        IEnumerable<Member> GetNeighbers(IEnumerable<Member> members);


        Size FullSize { get; }
        Size VisibleSize { get; }
        Size FixedSize { get; }
        Point ScrollOffset { get; }
        RowColRange VisibleRowColRange { get; }
        Rectangle? GetRangeSelectBound();
        bool IsSelected(Member m);
        bool IsSelected(CallenderDay d);
        void Invalidate();
        WorkItem PickWorkItemFromPoint(RawPoint curOnRaw);
        bool IsFixedArea(Point location);
        RawPoint Client2Raw(Point location);
        void IncRatio();
        void DecRatio();
        void EditSelectedWorkItem();
        void AddNewWorkItem(WorkItem proto);
    }
}
