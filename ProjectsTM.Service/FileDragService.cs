﻿using System.Windows.Forms;

namespace ProjectsTM.Service
{
    public static class FileDragService
    {
        public static string Drop(DragEventArgs e)
        {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (fileName.Length == 0) return null;
            if (string.IsNullOrEmpty(fileName[0])) return null;
            return fileName[0];
        }

        public static void DragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}