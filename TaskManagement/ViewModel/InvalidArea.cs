﻿using FreeGridControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TaskManagement.Model;
using TaskManagement.UI;

namespace TaskManagement.ViewModel
{
    class InvalidArea : IDisposable
    {
        private Bitmap _bitmap;
        private Graphics _bitmapGraphics;
        private Dictionary<Member, HashSet<WorkItem>> _validList = new Dictionary<Member, HashSet<WorkItem>>();

        public Graphics Graphics => _bitmapGraphics;

        public Image Image => _bitmap;

        internal InvalidArea(int width, int height)
        {
            if (_bitmap == null)
            {
                _bitmap = new Bitmap(width, height);
                _bitmapGraphics = System.Drawing.Graphics.FromImage(_bitmap);
                _bitmapGraphics.Clear(Control.DefaultBackColor);
            }

        }
        internal void Validate(Member m)
        {
            if (IsValid(m)) return;

            _validList.Add(m, new HashSet<WorkItem>());
        }

        internal bool IsValid(Member m)
        {
            if (!_validList.TryGetValue(m, out var workItems)) return false;
            return true;
        }

        internal void Invalidate(List<Member> members, Func<Member, RectangleF> GetMemberDrawRect, Func<ColIndex, Member> col2member, Func<Member, ColIndex> member2col)
        {
            //該当メンバの列を少し広めにクリアFill＆該当メンバの両隣含めて再描画
            foreach (var m in members)
            {
                var rect = GetMemberDrawRect(m);
                rect.Inflate(1, 1);
                _bitmapGraphics.FillRectangle(BrushCache.GetBrush(Control.DefaultBackColor), rect);
            }

            var redrawMembers = new HashSet<Member>();
            foreach (var m in members)
            {
                var c = member2col(m);
                var l = col2member(new ColIndex(c.Value - 1));
                var r = col2member(new ColIndex(c.Value + 1));

                redrawMembers.Add(m);
                if (l != null) redrawMembers.Add(l);
                if (r != null) redrawMembers.Add(r);
            }
            foreach (var m in redrawMembers)
            {
                _validList.Remove(m);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _bitmapGraphics.Dispose();
                    _bitmap.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~InvalidArea()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
