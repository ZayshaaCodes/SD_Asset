using System;
using System.Runtime.InteropServices;

public static class ClipboardHelper
{
    [DllImport("user32.dll", EntryPoint = "OpenClipboard", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool OpenClipboard(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "EmptyClipboard", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EmptyClipboard();

    [DllImport("user32.dll", EntryPoint = "SetClipboardData", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);

    [DllImport("user32.dll", EntryPoint = "CloseClipboard", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool CloseClipboard();
}
// typedef struct _DROPFILES {
//   DWORD pFiles;
//   POINT pt;
//   BOOL  fNC;
//   BOOL  fWide;
// } DROPFILES, *LPDROPFILES;
[StructLayout(LayoutKind.Sequential)]
public struct DROPFILES
{
    public uint pFiles;
    public POINT pt;
    public bool fNC;
    public bool fWide;
}
// typedef struct tagPOINT {
//   LONG x;
//   LONG y;
// } POINT, *PPOINT, *NPPOINT, *LPPOINT;
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int x;
    public int y;
}