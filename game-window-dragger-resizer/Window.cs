using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public enum Corner
{
    NONE,
    BOTTOM_LEFT,
    BOTTOM_RIGHT,
    TOP_LEFT,
    TOP_RIGHT
}

public class Window : MonoBehaviour {
    static IntPtr window;
    public RECT last_window_rect;

    Corner mouse_pressed_in_corner = Corner.NONE;
    public int scale_corner_size = 40;
    Vector2 last_mouse_pos = Vector2.zero;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
        }

        public static bool operator ==(RECT x, RECT y)
        {
            if (x.Left == y.Left && x.Top == y.Top && x.Right == y.Right && x.Bottom == y.Bottom)
                return true;
            return false;
        }

        public static bool operator !=(RECT x, RECT y)
        {
            if (x.Left == y.Left && x.Top == y.Top && x.Right == y.Right && x.Bottom == y.Bottom)
                return false;
            return true;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
        public static implicit operator Vector2(POINT p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);
#endif
    public static System.IntPtr GetWindowHandle()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return GetActiveWindow();
#else
        return (IntPtr)(0);
#endif
    }

    public static void SetPosition(int x, int y, int resX = 0, int resY = 0)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        print(window);
        SetWindowPos(window, 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);

        print("Setting window to: " + x + " " + y + " " + resX + " " + resY);
#endif
    }

    // Use this for initialization
    void Start () {
	
	}

    float[] mouse_pressed_at = new float[2] {0f,0f};
    float fullscreen_double_tap_time = 0.5f;

	// Update is called once per frame
	void Update () {
	    if (window == IntPtr.Zero)
        {
            if (GetWindowHandle() != IntPtr.Zero)
            {
                window = GetWindowHandle();
            }
        }

        POINT cursor_pos;
        GetCursorPos(out cursor_pos);

        Vector2 mouse_delta = new Vector2(cursor_pos.X, cursor_pos.Y) - last_mouse_pos;

        // Set corner press state
        if (Input.GetMouseButton(0) && mouse_pressed_in_corner == Corner.NONE)
        {
            // Left bottom
            if ((Input.mousePosition.x <= scale_corner_size)
            && (Input.mousePosition.y <= scale_corner_size))
            {
                print("Mouse pressed in bottom left corner");
                mouse_pressed_in_corner = Corner.BOTTOM_LEFT;
            }
            // Right bottom
            if ((Input.mousePosition.x >= Screen.width - scale_corner_size)
            && (Input.mousePosition.y <= scale_corner_size))
            {
                print("Mouse pressed in bottom right corner");
                mouse_pressed_in_corner = Corner.BOTTOM_RIGHT;
            }
            // Right top
            if ((Input.mousePosition.x >= Screen.width - scale_corner_size)
            && (Input.mousePosition.y >= Screen.height - scale_corner_size))
            {
                print("Mouse pressed in top right corner");
                mouse_pressed_in_corner = Corner.TOP_RIGHT;
            }
            // Left top
            if ((Input.mousePosition.x <= scale_corner_size)
            && (Input.mousePosition.y >= Screen.height - scale_corner_size))
            {
                print("Mouse pressed in top left corner");
                mouse_pressed_in_corner = Corner.TOP_LEFT;
            }
        }
        //

        //Make full screen on double-click
        if (Input.GetMouseButtonDown(0))
        {
            mouse_pressed_at[0] = mouse_pressed_at[1];
            mouse_pressed_at[1] = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - mouse_pressed_at[0] < fullscreen_double_tap_time)
            {
                print("INFO: Making full screen..");
                Screen.fullScreen = !Screen.fullScreen;
            }
        }
        //

        // Clear mouse pressed in corner if mouse has been released
        if (!Input.GetMouseButton(0))
        {
            mouse_pressed_in_corner = Corner.NONE;
        }
        //

        // Move window
        Rect screen_rect = new Rect(0, 0, Screen.width, Screen.height);

        RECT WinR;
        GetWindowRect(GetActiveWindow(), out WinR);

        if (WinR != last_window_rect)
        {
            print("Window moved.");
            MsgWindowSpecUpdate window_update = new MsgWindowSpecUpdate();
            window_update.height = WinR.Bottom - WinR.Top;
            window_update.width = WinR.Right - WinR.Left;
            window_update.x = WinR.Left;
            window_update.y = WinR.Top;
            print("Window height: " + window_update.height);
        }
        last_window_rect = WinR;
        //

        // Resize window if mouse has been pressed down in a corner.
        if (mouse_pressed_in_corner != Corner.NONE)
        {
            print("Scaling window, last window params: " + WinR.ToString());

            if (mouse_pressed_in_corner == Corner.TOP_LEFT)
            {
                SetPosition(WinR.Left + (int)mouse_delta.x, WinR.Top + (int)mouse_delta.y,
                    (WinR.Right - WinR.Left) - (int)mouse_delta.x, (WinR.Bottom - WinR.Top) - (int)mouse_delta.y);
            }
            else if (mouse_pressed_in_corner == Corner.BOTTOM_LEFT)
            {
                SetPosition(WinR.Left + (int)mouse_delta.x, WinR.Top,
                    (WinR.Right - WinR.Left) - (int)mouse_delta.x, (WinR.Bottom - WinR.Top) + (int)mouse_delta.y/2);
            }
            else if (mouse_pressed_in_corner == Corner.BOTTOM_RIGHT)
            {
                SetPosition(WinR.Left, WinR.Top,
                    (WinR.Right - WinR.Left) + (int)mouse_delta.x, (WinR.Bottom - WinR.Top) + (int)mouse_delta.y);
            }
            else
            {
                SetPosition(WinR.Left, WinR.Top + (int)mouse_delta.y,
                    (WinR.Right - WinR.Left) + (int)mouse_delta.x, (WinR.Bottom - WinR.Top) - (int)mouse_delta.y);
            }
        }
        else if (Input.GetMouseButton(0) && screen_rect.Contains(Input.mousePosition))
        {
            GetWindowRect(GetActiveWindow(), out WinR);

            print("Moving window, last window params: " + WinR.ToString());
            SetPosition(WinR.Left + (int)mouse_delta.x, WinR.Top + (int)mouse_delta.y);
        }
        //Input.mousePosition;

        last_mouse_pos = new Vector2(cursor_pos.X, cursor_pos.Y);
    }
}
