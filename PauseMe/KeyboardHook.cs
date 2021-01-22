using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PauseMe
{
    class KeyboardHook
    {
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHook.KBDLLHookProc HookProc, IntPtr hInstance, int wParam);

		[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		private static extern bool UnhookWindowsHookEx(int idHook);

		public event KeyDownEventHandler KeyDown;

		public event KeyUpEventHandler KeyUp;

		private int KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			bool flag = nCode == 0;
			checked
			{
				if (flag)
				{
					bool flag2 = wParam == (IntPtr)256 || wParam == (IntPtr)260;
					if (flag2)
					{
						KeyboardHook.KeyDownEventHandler keyDownEvent = this.KeyDown;
						if (keyDownEvent != null)
						{
							KeyboardHook.KeyDownEventHandler keyDownEventHandler = keyDownEvent;
                            object obj = Marshal.PtrToStructure(lParam, typeof(KeyboardHook.KBDLLHOOKSTRUCT));
							keyDownEventHandler((Keys)((obj != null) ? ((KeyboardHook.KBDLLHOOKSTRUCT)obj) : default(KeyboardHook.KBDLLHOOKSTRUCT)).vkCode);
						}
					}
					else
					{
						flag2 = (wParam == (IntPtr)257 || wParam == (IntPtr)261);
						if (flag2)
						{
							KeyboardHook.KeyUpEventHandler keyUpEvent = this.KeyUp;
							if (keyUpEvent != null)
							{
								KeyboardHook.KeyUpEventHandler keyUpEventHandler = keyUpEvent;
                                object obj2 = Marshal.PtrToStructure(lParam, typeof(KeyboardHook.KBDLLHOOKSTRUCT));
								keyUpEventHandler((Keys)((obj2 != null) ? ((KeyboardHook.KBDLLHOOKSTRUCT)obj2) : default(KeyboardHook.KBDLLHOOKSTRUCT)).vkCode);
							}
						}
					}
				}
				return KeyboardHook.CallNextHookEx((int)IntPtr.Zero, nCode, wParam, lParam);
			}
		}

		public KeyboardHook()
		{
			this.KBDLLHookProcDelegate = new KeyboardHook.KBDLLHookProc(this.KeyboardProc);
			this.HHookID = IntPtr.Zero;
			this.HHookID = SetWindowsHookEx(WH_KEYBOARD_LL, KBDLLHookProcDelegate, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
			bool flag = this.HHookID == IntPtr.Zero;
			if (flag)
			{
				throw new Exception("Could not set keyboard hook");
			}
		}

		protected void Finalize()
		{
			bool flag = !(this.HHookID == IntPtr.Zero);
			if (flag)
			{
				KeyboardHook.UnhookWindowsHookEx((int)this.HHookID);
			}
			//base.Finalize();
		}

		private const int WH_KEYBOARD_LL = 13;

		private const int HC_ACTION = 0;

		private const int WM_KEYDOWN = 256;

		private const int WM_KEYUP = 257;

		private const int WM_SYSKEYDOWN = 260;

		private const int WM_SYSKEYUP = 261;

		private KeyboardHook.KBDLLHookProc KBDLLHookProcDelegate;

		private IntPtr HHookID;

		private struct KBDLLHOOKSTRUCT
		{
            public uint vkCode;

			public uint scanCode;

			public KeyboardHook.KBDLLHOOKSTRUCTFlags flags;

            public uint time;

			public UIntPtr dwExtraInfo;
		}

		[Flags]
		private enum KBDLLHOOKSTRUCTFlags : uint
		{
            LLKHF_EXTENDED = 1U,
            LLKHF_INJECTED = 16U,
            LLKHF_ALTDOWN = 32U,
            LLKHF_UP = 128U
		}

		public delegate void KeyDownEventHandler(Keys Key);

		public delegate void KeyUpEventHandler(Keys Key);

		private delegate int KBDLLHookProc(int nCode, IntPtr wParam, IntPtr lParam);
	}
}
