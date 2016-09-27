/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace Teltec.Common
{
	public static class NativeMethods
	{
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_RBUTTONDOWN = 0x0204;

		public const int TV_FIRST = 0x1100;
		public const int TVSIL_STATE = 2;
		public const int TVM_SETIMAGELIST = TV_FIRST + 9;
		public const int TVM_SETITEM = TV_FIRST + 63;

		public const int TVIF_STATE = 0x0008;
		public const int TVIF_HANDLE = 0x0010;

		public const int TVIS_STATEIMAGEMASK = 0xF000;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct TV_ITEM
		{
			internal int mask;
			internal IntPtr hItem;
			internal int state;
			internal int stateMask;
			internal IntPtr /* LPTSTR */ pszText;
			internal int cchTextMax;
			internal int iImage;
			internal int iSelectedImage;
			internal int cChildren;
			internal IntPtr lParam;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern IntPtr SendMessage(HandleRef hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern IntPtr SendMessage(HandleRef hWnd, int Msg, IntPtr wParam, ref TV_ITEM lParam);
	}
}
