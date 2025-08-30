using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System;

//
// Resumen:
//     Representa los flujos de entrada, salida y error estándar para las aplicaciones
//     de consola. Esta clase no puede heredarse.
public static class Console
{
    [Flags]
    internal enum ControlKeyState
    {
        RightAltPressed = 1,
        LeftAltPressed = 2,
        RightCtrlPressed = 4,
        LeftCtrlPressed = 8,
        ShiftPressed = 0x10,
        NumLockOn = 0x20,
        ScrollLockOn = 0x40,
        CapsLockOn = 0x80,
        EnhancedKey = 0x100
    }

    internal sealed class ControlCHooker : CriticalFinalizerObject
    {
        private bool _hooked;

        [SecurityCritical]
        private Win32Native.ConsoleCtrlHandlerRoutine _handler;

        [SecurityCritical]
        internal ControlCHooker()
        {
            _handler = BreakEvent;
        }

        ~ControlCHooker()
        {
            Unhook();
        }

        [SecuritySafeCritical]
        internal void Hook()
        {
            if (!_hooked)
            {
                if (!Win32Native.SetConsoleCtrlHandler(_handler, addOrRemove: true))
                {
                    __Error.WinIOError();
                }

                _hooked = true;
            }
        }

        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Unhook()
        {
            if (_hooked)
            {
                if (!Win32Native.SetConsoleCtrlHandler(_handler, addOrRemove: false))
                {
                    __Error.WinIOError();
                }

                _hooked = false;
            }
        }
    }

    private sealed class ControlCDelegateData
    {
        internal ConsoleSpecialKey ControlKey;

        internal bool Cancel;

        internal bool DelegateStarted;

        internal ManualResetEvent CompletionEvent;

        internal ConsoleCancelEventHandler CancelCallbacks;

        internal ControlCDelegateData(ConsoleSpecialKey controlKey, ConsoleCancelEventHandler cancelCallbacks)
        {
            ControlKey = controlKey;
            CancelCallbacks = cancelCallbacks;
            CompletionEvent = new ManualResetEvent(initialState: false);
        }
    }

    private const int DefaultConsoleBufferSize = 256;

    private const short AltVKCode = 18;

    private const int NumberLockVKCode = 144;

    private const int CapsLockVKCode = 20;

    private const int MinBeepFrequency = 37;

    private const int MaxBeepFrequency = 32767;

    private const int MaxConsoleTitleLength = 24500;

    private static readonly UnicodeEncoding StdConUnicodeEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);

    private static volatile TextReader _in;

    private static volatile TextWriter _out;

    private static volatile TextWriter _error;

    private static volatile ConsoleCancelEventHandler _cancelCallbacks;

    private static volatile ControlCHooker _hooker;

    [SecurityCritical]
    private static Win32Native.InputRecord _cachedInputRecord;

    private static volatile bool _haveReadDefaultColors;

    private static volatile byte _defaultColors;

    private static volatile bool _isOutTextWriterRedirected = false;

    private static volatile bool _isErrorTextWriterRedirected = false;

    private static volatile Encoding _inputEncoding = null;

    private static volatile Encoding _outputEncoding = null;

    private static volatile bool _stdInRedirectQueried = false;

    private static volatile bool _stdOutRedirectQueried = false;

    private static volatile bool _stdErrRedirectQueried = false;

    private static bool _isStdInRedirected;

    private static bool _isStdOutRedirected;

    private static bool _isStdErrRedirected;

    private static volatile object s_InternalSyncObject;

    private static volatile object s_ReadKeySyncObject;

    private static volatile IntPtr _consoleInputHandle;

    private static volatile IntPtr _consoleOutputHandle;

    private static object InternalSyncObject
    {
        get
        {
            if (s_InternalSyncObject == null)
            {
                object value = new object();
                Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
            }

            return s_InternalSyncObject;
        }
    }

    private static object ReadKeySyncObject
    {
        get
        {
            if (s_ReadKeySyncObject == null)
            {
                object value = new object();
                Interlocked.CompareExchange<object>(ref s_ReadKeySyncObject, value, (object)null);
            }

            return s_ReadKeySyncObject;
        }
    }

    private static IntPtr ConsoleInputHandle
    {
        [SecurityCritical]
        get
        {
            if (_consoleInputHandle == IntPtr.Zero)
            {
                _consoleInputHandle = Win32Native.GetStdHandle(-10);
            }

            return _consoleInputHandle;
        }
    }

    private static IntPtr ConsoleOutputHandle
    {
        [SecurityCritical]
        get
        {
            if (_consoleOutputHandle == IntPtr.Zero)
            {
                _consoleOutputHandle = Win32Native.GetStdHandle(-11);
            }

            return _consoleOutputHandle;
        }
    }

    //
    // Resumen:
    //     Obtiene un valor que indica si la entrada se ha redirigido desde el flujo de
    //     entrada estándar.
    //
    // Devuelve:
    //     true si se redirige la entrada; si no, false.
    public static bool IsInputRedirected
    {
        [SecuritySafeCritical]
        get
        {
            if (_stdInRedirectQueried)
            {
                return _isStdInRedirected;
            }

            lock (InternalSyncObject)
            {
                if (_stdInRedirectQueried)
                {
                    return _isStdInRedirected;
                }

                _isStdInRedirected = IsHandleRedirected(ConsoleInputHandle);
                _stdInRedirectQueried = true;
                return _isStdInRedirected;
            }
        }
    }

    //
    // Resumen:
    //     Obtiene un valor que indica si la salida se ha redirigido desde el flujo de salida
    //     estándar.
    //
    // Devuelve:
    //     true si se redirige la salida; si no, false.
    public static bool IsOutputRedirected
    {
        [SecuritySafeCritical]
        get
        {
            if (_stdOutRedirectQueried)
            {
                return _isStdOutRedirected;
            }

            lock (InternalSyncObject)
            {
                if (_stdOutRedirectQueried)
                {
                    return _isStdOutRedirected;
                }

                _isStdOutRedirected = IsHandleRedirected(ConsoleOutputHandle);
                _stdOutRedirectQueried = true;
                return _isStdOutRedirected;
            }
        }
    }

    //
    // Resumen:
    //     Obtiene un valor que indica si el flujo de salida de errores se ha redirigido
    //     desde el flujo de errores estándar.
    //
    // Devuelve:
    //     true si se redirige la salida de error; si no, false.
    public static bool IsErrorRedirected
    {
        [SecuritySafeCritical]
        get
        {
            if (_stdErrRedirectQueried)
            {
                return _isStdErrRedirected;
            }

            lock (InternalSyncObject)
            {
                if (_stdErrRedirectQueried)
                {
                    return _isStdErrRedirected;
                }

                IntPtr stdHandle = Win32Native.GetStdHandle(-12);
                _isStdErrRedirected = IsHandleRedirected(stdHandle);
                _stdErrRedirectQueried = true;
                return _isStdErrRedirected;
            }
        }
    }

    //
    // Resumen:
    //     Obtiene el flujo de entrada estándar.
    //
    // Devuelve:
    //     System.IO.TextReader que representa el flujo de entrada estándar.
    public static TextReader In
    {
        [SecuritySafeCritical]
        [HostProtection(SecurityAction.LinkDemand, UI = true)]
        get
        {
            if (_in == null)
            {
                lock (InternalSyncObject)
                {
                    if (_in == null)
                    {
                        Stream stream = OpenStandardInput(256);
                        TextReader @in;
                        if (stream == Stream.Null)
                        {
                            @in = StreamReader.Null;
                        }
                        else
                        {
                            Encoding inputEncoding = InputEncoding;
                            @in = TextReader.Synchronized(new StreamReader(stream, inputEncoding, detectEncodingFromByteOrderMarks: false, 256, leaveOpen: true));
                        }

                        Thread.MemoryBarrier();
                        _in = @in;
                    }
                }
            }

            return _in;
        }
    }

    //
    // Resumen:
    //     Obtiene el flujo de salida estándar.
    //
    // Devuelve:
    //     System.IO.TextWriter que representa el flujo de salida estándar.
    public static TextWriter Out
    {
        [HostProtection(SecurityAction.LinkDemand, UI = true)]
        get
        {
            if (_out == null)
            {
                InitializeStdOutError(stdout: true);
            }

            return _out;
        }
    }

    //
    // Resumen:
    //     Obtiene el flujo de salida de error estándar.
    //
    // Devuelve:
    //     Objeto System.IO.TextWriter que representa el flujo de salida de error estándar.
    public static TextWriter Error
    {
        [HostProtection(SecurityAction.LinkDemand, UI = true)]
        get
        {
            if (_error == null)
            {
                InitializeStdOutError(stdout: false);
            }

            return _error;
        }
    }

    //
    // Resumen:
    //     Obtiene o establece la codificación que usa la consola para leer la entrada.
    //
    //
    // Devuelve:
    //     Codificación usada para leer la entrada de la consola.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de la propiedad en una operación de conjunto es null.
    //
    //   T:System.IO.IOException:
    //     Error durante la ejecución de esta operación.
    //
    //   T:System.Security.SecurityException:
    //     Tu aplicación no tiene permiso para realizar esta operación.
    public static Encoding InputEncoding
    {
        [SecuritySafeCritical]
        get
        {
            if (_inputEncoding != null)
            {
                return _inputEncoding;
            }

            lock (InternalSyncObject)
            {
                if (_inputEncoding != null)
                {
                    return _inputEncoding;
                }

                uint consoleCP = Win32Native.GetConsoleCP();
                _inputEncoding = Encoding.GetEncoding((int)consoleCP);
                return _inputEncoding;
            }
        }
        [SecuritySafeCritical]
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            lock (InternalSyncObject)
            {
                if (!IsStandardConsoleUnicodeEncoding(value))
                {
                    uint codePage = (uint)value.CodePage;
                    if (!Win32Native.SetConsoleCP(codePage))
                    {
                        __Error.WinIOError();
                    }
                }

                _inputEncoding = (Encoding)value.Clone();
                _in = null;
            }
        }
    }

    //
    // Resumen:
    //     Obtiene o establece la codificación que usa la consola para escribir la salida.
    //
    //
    // Devuelve:
    //     Codificación usada para escribir la salida de la consola.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de la propiedad en una operación de conjunto es null.
    //
    //   T:System.IO.IOException:
    //     Error durante la ejecución de esta operación.
    //
    //   T:System.Security.SecurityException:
    //     Tu aplicación no tiene permiso para realizar esta operación.
    public static Encoding OutputEncoding
    {
        [SecuritySafeCritical]
        get
        {
            if (_outputEncoding != null)
            {
                return _outputEncoding;
            }

            lock (InternalSyncObject)
            {
                if (_outputEncoding != null)
                {
                    return _outputEncoding;
                }

                uint consoleOutputCP = Win32Native.GetConsoleOutputCP();
                _outputEncoding = Encoding.GetEncoding((int)consoleOutputCP);
                return _outputEncoding;
            }
        }
        [SecuritySafeCritical]
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            lock (InternalSyncObject)
            {
                if (_out != null && !_isOutTextWriterRedirected)
                {
                    _out.Flush();
                    _out = null;
                }

                if (_error != null && !_isErrorTextWriterRedirected)
                {
                    _error.Flush();
                    _error = null;
                }

                if (!IsStandardConsoleUnicodeEncoding(value))
                {
                    uint codePage = (uint)value.CodePage;
                    if (!Win32Native.SetConsoleOutputCP(codePage))
                    {
                        __Error.WinIOError();
                    }
                }

                _outputEncoding = (Encoding)value.Clone();
            }
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el color de fondo de la consola.
    //
    // Devuelve:
    //     Valor que especifica el color de fondo de la consola; es decir, el color que
    //     aparece detrás de cada carácter. El valor predeterminado es negro.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El color especificado en una operación de establecimiento no es un miembro válido
    //     de System.ConsoleColor.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static ConsoleColor BackgroundColor
    {
        [SecuritySafeCritical]
        get
        {
            bool succeeded;
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
            if (!succeeded)
            {
                return ConsoleColor.Black;
            }

            Win32Native.Color c = (Win32Native.Color)(bufferInfo.wAttributes & 0xF0);
            return ColorAttributeToConsoleColor(c);
        }
        [SecuritySafeCritical]
        set
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            Win32Native.Color color = ConsoleColorToColorAttribute(value, isBackground: true);
            bool succeeded;
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
            if (succeeded)
            {
                short wAttributes = bufferInfo.wAttributes;
                wAttributes &= -241;
                wAttributes = (short)((ushort)wAttributes | (ushort)color);
                Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, wAttributes);
            }
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el color de primer plano de la consola.
    //
    // Devuelve:
    //     Enumeración System.ConsoleColor que especifica el color de primer plano de la
    //     consola; es decir, el color de cada carácter que se muestra. El valor predeterminado
    //     es gris.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El color especificado en una operación de establecimiento no es un miembro válido
    //     de System.ConsoleColor.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static ConsoleColor ForegroundColor
    {
        [SecuritySafeCritical]
        get
        {
            bool succeeded;
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
            if (!succeeded)
            {
                return ConsoleColor.Gray;
            }

            Win32Native.Color c = (Win32Native.Color)(bufferInfo.wAttributes & 0xF);
            return ColorAttributeToConsoleColor(c);
        }
        [SecuritySafeCritical]
        set
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            Win32Native.Color color = ConsoleColorToColorAttribute(value, isBackground: false);
            bool succeeded;
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
            if (succeeded)
            {
                short wAttributes = bufferInfo.wAttributes;
                wAttributes &= -16;
                wAttributes = (short)((ushort)wAttributes | (ushort)color);
                Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, wAttributes);
            }
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el alto del área del búfer.
    //
    // Devuelve:
    //     El alto actual, en filas, del área del búfer.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor de una operación set es mayor o igual que cero. O bien El valor de una
    //     operación set es mayor o igual que System.Int16.MaxValue. O bien El valor de
    //     una operación Set es menor que System.Console.WindowTop + System.Console.WindowHeight.
    //
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static int BufferHeight
    {
        [SecuritySafeCritical]
        get
        {
            return GetBufferInfo().dwSize.Y;
        }
        set
        {
            SetBufferSize(BufferWidth, value);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el ancho del área del búfer.
    //
    // Devuelve:
    //     El ancho actual, en columnas, del área del búfer.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor de una operación set es mayor o igual que cero. O bien El valor de una
    //     operación set es mayor o igual que System.Int16.MaxValue. O bien El valor de
    //     una operación Set es menor que System.Console.WindowLeft + System.Console.WindowWidth.
    //
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static int BufferWidth
    {
        [SecuritySafeCritical]
        get
        {
            return GetBufferInfo().dwSize.X;
        }
        set
        {
            SetBufferSize(value, BufferHeight);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el alto del área de la ventana de la consola.
    //
    // Devuelve:
    //     Alto de la ventana de la consola, medido en filas.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor de la propiedad System.Console.WindowWidth o el valor de la propiedad
    //     System.Console.WindowHeight es menor o igual que 0. O bien El valor de la propiedad
    //     System.Console.WindowHeight más el valor de la propiedad System.Console.WindowTop
    //     es mayor o igual que System.Int16.MaxValue. O bien El valor de la propiedad System.Console.WindowWidth
    //     o el valor de la propiedad System.Console.WindowHeight es mayor que el mayor
    //     ancho o la mayor altura de ventana posible para la resolución de pantalla y la
    //     fuente de consola actuales.
    //
    //   T:System.IO.IOException:
    //     Error al leer o escribir información.
    public static int WindowHeight
    {
        [SecuritySafeCritical]
        get
        {
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
            return bufferInfo.srWindow.Bottom - bufferInfo.srWindow.Top + 1;
        }
        set
        {
            SetWindowSize(WindowWidth, value);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el ancho de la ventana de la consola.
    //
    // Devuelve:
    //     Ancho de la ventana de la consola, medido en columnas.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor de la propiedad System.Console.WindowWidth o el valor de la propiedad
    //     System.Console.WindowHeight es menor o igual que 0. O bien El valor de la propiedad
    //     System.Console.WindowHeight más el valor de la propiedad System.Console.WindowTop
    //     es mayor o igual que System.Int16.MaxValue. O bien El valor de la propiedad System.Console.WindowWidth
    //     o el valor de la propiedad System.Console.WindowHeight es mayor que el mayor
    //     ancho o la mayor altura de ventana posible para la resolución de pantalla y la
    //     fuente de consola actuales.
    //
    //   T:System.IO.IOException:
    //     Error al leer o escribir información.
    public static int WindowWidth
    {
        [SecuritySafeCritical]
        get
        {
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
            return bufferInfo.srWindow.Right - bufferInfo.srWindow.Left + 1;
        }
        set
        {
            SetWindowSize(value, WindowHeight);
        }
    }

    //
    // Resumen:
    //     Obtiene el máximo número posible de columnas para la ventana de la consola, basado
    //     en la fuente y la resolución de pantalla actuales.
    //
    // Devuelve:
    //     El ancho de la ventana de la consola más grande posible medido en columnas.
    public static int LargestWindowWidth
    {
        [SecuritySafeCritical]
        get
        {
            return Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle).X;
        }
    }

    //
    // Resumen:
    //     Obtiene el máximo número posible de filas para la ventana de la consola, basado
    //     en la fuente y la resolución de pantalla actuales.
    //
    // Devuelve:
    //     El alto de la ventana de la consola más grande posible medido en filas.
    public static int LargestWindowHeight
    {
        [SecuritySafeCritical]
        get
        {
            return Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle).Y;
        }
    }

    //
    // Resumen:
    //     Obtiene o establece la posición más a la izquierda del área de la ventana de
    //     la consola con respecto al búfer de pantalla.
    //
    // Devuelve:
    //     Posición más a la izquierda de la ventana de la consola, medida en columnas.
    //
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     En una operación set, el valor que se asigna es menor que cero. O bien Como resultado
    //     de la asignación, System.Console.WindowLeft más System.Console.WindowWidth superaría
    //     System.Console.BufferWidth.
    //
    //   T:System.IO.IOException:
    //     Error al leer o escribir información.
    public static int WindowLeft
    {
        [SecuritySafeCritical]
        get
        {
            return GetBufferInfo().srWindow.Left;
        }
        set
        {
            SetWindowPosition(value, WindowTop);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece la posición superior del área de la ventana de la consola
    //     con respecto al búfer de pantalla.
    //
    // Devuelve:
    //     Posición superior de la ventana de la consola, medida en filas.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     En una operación set, el valor que se asigna es menor que cero. O bien Como resultado
    //     de la asignación, System.Console.WindowTop más System.Console.WindowHeight superaría
    //     System.Console.BufferHeight.
    //
    //   T:System.IO.IOException:
    //     Error al leer o escribir información.
    public static int WindowTop
    {
        [SecuritySafeCritical]
        get
        {
            return GetBufferInfo().srWindow.Top;
        }
        set
        {
            SetWindowPosition(WindowLeft, value);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece la posición en columnas del cursor en el área del búfer.
    //
    //
    // Devuelve:
    //     La posición actual, en columnas, del cursor.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor de una operación Set es menor que cero. O bien El valor en una operación
    //     Set es mayor o igual que System.Console.BufferWidth.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static int CursorLeft
    {
        [SecuritySafeCritical]
        get
        {
            return GetBufferInfo().dwCursorPosition.X;
        }
        set
        {
            SetCursorPosition(value, CursorTop);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece la posición en filas del cursor en el área del búfer.
    //
    // Devuelve:
    //     La posición actual, en filas, del cursor.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor de una operación Set es menor que cero. O bien El valor en una operación
    //     Set es mayor o igual que System.Console.BufferHeight.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static int CursorTop
    {
        [SecuritySafeCritical]
        get
        {
            return GetBufferInfo().dwCursorPosition.Y;
        }
        set
        {
            SetCursorPosition(CursorLeft, value);
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el alto del cursor en una celda de carácter.
    //
    // Devuelve:
    //     El tamaño del cursor expresado como porcentaje del alto de una celda de carácter.
    //     El valor de propiedad varía entre 1 y 100.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El valor especificado en una operación Set es menor que 1 o mayor que 100.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static int CursorSize
    {
        [SecuritySafeCritical]
        get
        {
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
            {
                __Error.WinIOError();
            }

            return cci.dwSize;
        }
        [SecuritySafeCritical]
        set
        {
            if (value < 1 || value > 100)
            {
                throw new ArgumentOutOfRangeException("value", value, Environment.GetResourceString("ArgumentOutOfRange_CursorSize"));
            }

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
            {
                __Error.WinIOError();
            }

            cci.dwSize = value;
            if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref cci))
            {
                __Error.WinIOError();
            }
        }
    }

    //
    // Resumen:
    //     Obtiene o establece un valor que indica si el cursor es visible.
    //
    // Devuelve:
    //     true si el cursor es visible; en caso contrario, false.
    //
    // Excepciones:
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static bool CursorVisible
    {
        [SecuritySafeCritical]
        get
        {
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
            {
                __Error.WinIOError();
            }

            return cci.bVisible;
        }
        [SecuritySafeCritical]
        set
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
            {
                __Error.WinIOError();
            }

            cci.bVisible = value;
            if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref cci))
            {
                __Error.WinIOError();
            }
        }
    }

    //
    // Resumen:
    //     Obtiene o establece el título que se va a mostrar en la barra de título de la
    //     consola.
    //
    // Devuelve:
    //     Cadena que se va a mostrar en la barra de título de la consola. La cadena de
    //     título tiene una longitud máxima de 24.500 caracteres.
    //
    // Excepciones:
    //   T:System.InvalidOperationException:
    //     En una operación get, el título recuperado tiene más de 24.500 caracteres.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     En una operación set, el título especificado tiene más de 24.500 caracteres.
    //
    //
    //   T:System.ArgumentNullException:
    //     En una operación set, el título especificado es null.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static string Title
    {
        [SecuritySafeCritical]
        get
        {
            string s = null;
            int outTitleLength = -1;
            int titleNative = GetTitleNative(JitHelpers.GetStringHandleOnStack(ref s), out outTitleLength);
            if (titleNative != 0)
            {
                __Error.WinIOError(titleNative, string.Empty);
            }

            if (outTitleLength > 24500)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
            }

            return s;
        }
        [SecuritySafeCritical]
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length > 24500)
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
            }

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            if (!Win32Native.SetConsoleTitle(value))
            {
                __Error.WinIOError();
            }
        }
    }

    //
    // Resumen:
    //     Obtiene un valor que indica si hay disponible una acción de presionar una tecla
    //     en el flujo de entrada.
    //
    // Devuelve:
    //     true si hay disponible una acción de presionar una tecla; en caso contrario,
    //     false.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.InvalidOperationException:
    //     La entrada estándar se redirige a un archivo en lugar de al teclado.
    public static bool KeyAvailable
    {
        [SecuritySafeCritical]
        [HostProtection(SecurityAction.LinkDemand, UI = true)]
        get
        {
            if (_cachedInputRecord.eventType == 1)
            {
                return true;
            }

            Win32Native.InputRecord buffer = default(Win32Native.InputRecord);
            int numEventsRead = 0;
            while (true)
            {
                if (!Win32Native.PeekConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 6)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleKeyAvailableOnFile"));
                    }

                    __Error.WinIOError(lastWin32Error, "stdin");
                }

                if (numEventsRead == 0)
                {
                    return false;
                }

                if (IsKeyDownEvent(buffer) && !IsModKey(buffer))
                {
                    break;
                }

                if (!Win32Native.ReadConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead))
                {
                    __Error.WinIOError();
                }
            }

            return true;
        }
    }

    //
    // Resumen:
    //     Obtiene un valor que indica si está activada o desactivada la alternancia de
    //     teclado de BLOQ NUM.
    //
    // Devuelve:
    //     true si está activado BLOQ NUM; false si está desactivado BLOQ NUM.
    public static bool NumberLock
    {
        [SecuritySafeCritical]
        get
        {
            short keyState = Win32Native.GetKeyState(144);
            return (keyState & 1) == 1;
        }
    }

    //
    // Resumen:
    //     Obtiene un valor que indica si se activa o desactiva la alternancia de teclado
    //     de BLOQ MAYÚS.
    //
    // Devuelve:
    //     true si se activa BLOQ MAYÚS; false si se desactiva BLOQ MAYÚS.
    public static bool CapsLock
    {
        [SecuritySafeCritical]
        get
        {
            short keyState = Win32Native.GetKeyState(20);
            return (keyState & 1) == 1;
        }
    }

    //
    // Resumen:
    //     Obtiene o establece un valor que indica si la combinación de la tecla modificadora
    //     System.ConsoleModifiers.Control y de la tecla de consola System.ConsoleKey.C
    //     (Ctrl+C) se trata como una entrada ordinaria o como una interrupción controlada
    //     por el sistema operativo.
    //
    // Devuelve:
    //     true si Ctrl+C se trata como una entrada ordinaria; de lo contrario, false.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     No se puede obtener o establecer el modo de entrada del búfer de entrada de consola.
    public static bool TreatControlCAsInput
    {
        [SecuritySafeCritical]
        get
        {
            IntPtr consoleInputHandle = ConsoleInputHandle;
            if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
            }

            int mode = 0;
            if (!Win32Native.GetConsoleMode(consoleInputHandle, out mode))
            {
                __Error.WinIOError();
            }

            return (mode & 1) == 0;
        }
        [SecuritySafeCritical]
        set
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            IntPtr consoleInputHandle = ConsoleInputHandle;
            if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
            }

            int mode = 0;
            bool consoleMode = Win32Native.GetConsoleMode(consoleInputHandle, out mode);
            mode = ((!value) ? (mode | 1) : (mode & -2));
            if (!Win32Native.SetConsoleMode(consoleInputHandle, mode))
            {
                __Error.WinIOError();
            }
        }
    }

    //
    // Resumen:
    //     Se produce cuando la tecla modificadora System.ConsoleModifiers.Control (Ctrl)
    //     y la tecla de consola System.ConsoleKey.C (C) o la tecla Interrumpir se presionan
    //     simultáneamente (Ctrl+C o Ctrl+Inter).
    public static event ConsoleCancelEventHandler CancelKeyPress
    {
        [SecuritySafeCritical]
        add
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            lock (InternalSyncObject)
            {
                _cancelCallbacks = (ConsoleCancelEventHandler)Delegate.Combine(_cancelCallbacks, value);
                if (_hooker == null)
                {
                    _hooker = new ControlCHooker();
                    _hooker.Hook();
                }
            }
        }
        [SecuritySafeCritical]
        remove
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            lock (InternalSyncObject)
            {
                _cancelCallbacks = (ConsoleCancelEventHandler)Delegate.Remove(_cancelCallbacks, value);
                if (_hooker != null && _cancelCallbacks == null)
                {
                    _hooker.Unhook();
                }
            }
        }
    }

    [SecuritySafeCritical]
    private static bool IsHandleRedirected(IntPtr ioHandle)
    {
        SafeFileHandle handle = new SafeFileHandle(ioHandle, ownsHandle: false);
        int fileType = Win32Native.GetFileType(handle);
        if ((fileType & 2) != 2)
        {
            return true;
        }

        int mode;
        bool consoleMode = Win32Native.GetConsoleMode(ioHandle, out mode);
        return !consoleMode;
    }

    [SecuritySafeCritical]
    private static void InitializeStdOutError(bool stdout)
    {
        lock (InternalSyncObject)
        {
            if ((!stdout || _out == null) && (stdout || _error == null))
            {
                TextWriter textWriter = null;
                Stream stream = ((!stdout) ? OpenStandardError(256) : OpenStandardOutput(256));
                if (stream == Stream.Null)
                {
                    textWriter = TextWriter.Synchronized(StreamWriter.Null);
                }
                else
                {
                    Encoding outputEncoding = OutputEncoding;
                    StreamWriter streamWriter = new StreamWriter(stream, outputEncoding, 256, leaveOpen: true);
                    streamWriter.HaveWrittenPreamble = true;
                    streamWriter.AutoFlush = true;
                    textWriter = TextWriter.Synchronized(streamWriter);
                }

                if (stdout)
                {
                    _out = textWriter;
                }
                else
                {
                    _error = textWriter;
                }
            }
        }
    }

    private static bool IsStandardConsoleUnicodeEncoding(Encoding encoding)
    {
        if (!(encoding is UnicodeEncoding unicodeEncoding))
        {
            return false;
        }

        if (StdConUnicodeEncoding.CodePage == unicodeEncoding.CodePage)
        {
            return StdConUnicodeEncoding.bigEndian == unicodeEncoding.bigEndian;
        }

        return false;
    }

    private static bool GetUseFileAPIs(int handleType)
    {
        switch (handleType)
        {
            case -10:
                if (IsStandardConsoleUnicodeEncoding(InputEncoding))
                {
                    return IsInputRedirected;
                }

                return true;
            case -11:
                if (IsStandardConsoleUnicodeEncoding(OutputEncoding))
                {
                    return IsOutputRedirected;
                }

                return true;
            case -12:
                if (IsStandardConsoleUnicodeEncoding(OutputEncoding))
                {
                    return IsErrorRedirected;
                }

                return true;
            default:
                return true;
        }
    }

    [SecuritySafeCritical]
    private static Stream GetStandardFile(int stdHandleName, FileAccess access, int bufferSize)
    {
        IntPtr stdHandle = Win32Native.GetStdHandle(stdHandleName);
        SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, ownsHandle: false);
        if (safeFileHandle.IsInvalid)
        {
            safeFileHandle.SetHandleAsInvalid();
            return Stream.Null;
        }

        if (stdHandleName != -10 && !ConsoleHandleIsWritable(safeFileHandle))
        {
            return Stream.Null;
        }

        bool useFileAPIs = GetUseFileAPIs(stdHandleName);
        return new __ConsoleStream(safeFileHandle, access, useFileAPIs);
    }

    [SecuritySafeCritical]
    private unsafe static bool ConsoleHandleIsWritable(SafeFileHandle outErrHandle)
    {
        byte b = 65;
        int numBytesWritten;
        int num = Win32Native.WriteFile(outErrHandle, &b, 0, out numBytesWritten, IntPtr.Zero);
        return num != 0;
    }

    //
    // Resumen:
    //     Reproduce el sonido de un bip a través del altavoz de la consola.
    //
    // Excepciones:
    //   T:System.Security.HostProtectionException:
    //     Este método se ha ejecutado en un servidor, como SQL Server, que no permite el
    //     acceso a una interfaz de usuario.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Beep()
    {
        Beep(800, 200);
    }

    //
    // Resumen:
    //     Reproduce el sonido de un bip con una frecuencia y duración especificadas a través
    //     del altavoz de la consola.
    //
    // Parámetros:
    //   frequency:
    //     Frecuencia del bip, que va de 37 a 32.767 hercios.
    //
    //   duration:
    //     Duración del bip, medida en milisegundos.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     frequency es menor que 37 o mayor que 32767 hercios. O bien duration es menor
    //     o igual que cero.
    //
    //   T:System.Security.HostProtectionException:
    //     Este método se ha ejecutado en un servidor, como SQL Server, que no permite el
    //     acceso a la consola.
    [SecuritySafeCritical]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Beep(int frequency, int duration)
    {
        if (frequency < 37 || frequency > 32767)
        {
            throw new ArgumentOutOfRangeException("frequency", frequency, Environment.GetResourceString("ArgumentOutOfRange_BeepFrequency", 37, 32767));
        }

        if (duration <= 0)
        {
            throw new ArgumentOutOfRangeException("duration", duration, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
        }

        Win32Native.Beep(frequency, duration);
    }

    //
    // Resumen:
    //     Borra la información que se muestra en el búfer de pantalla y en la correspondiente
    //     ventana de la consola.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public static void Clear()
    {
        Win32Native.COORD cOORD = default(Win32Native.COORD);
        IntPtr consoleOutputHandle = ConsoleOutputHandle;
        if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
        {
            throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
        }

        Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
        int num = bufferInfo.dwSize.X * bufferInfo.dwSize.Y;
        int pNumCharsWritten = 0;
        if (!Win32Native.FillConsoleOutputCharacter(consoleOutputHandle, ' ', num, cOORD, out pNumCharsWritten))
        {
            __Error.WinIOError();
        }

        pNumCharsWritten = 0;
        if (!Win32Native.FillConsoleOutputAttribute(consoleOutputHandle, bufferInfo.wAttributes, num, cOORD, out pNumCharsWritten))
        {
            __Error.WinIOError();
        }

        if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, cOORD))
        {
            __Error.WinIOError();
        }
    }

    [SecurityCritical]
    private static Win32Native.Color ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
    {
        if (((uint)color & 0xFFFFFFF0u) != 0)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"));
        }

        Win32Native.Color color2 = (Win32Native.Color)color;
        if (isBackground)
        {
            color2 = (Win32Native.Color)((int)color2 << 4);
        }

        return color2;
    }

    [SecurityCritical]
    private static ConsoleColor ColorAttributeToConsoleColor(Win32Native.Color c)
    {
        if ((c & Win32Native.Color.BackgroundMask) != 0)
        {
            c = (Win32Native.Color)((int)c >> 4);
        }

        return (ConsoleColor)c;
    }

    //
    // Resumen:
    //     Establece los colores de primer plano y de fondo de la consola en sus valores
    //     predeterminados.
    //
    // Excepciones:
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public static void ResetColor()
    {
        new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
        bool succeeded;
        Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
        if (succeeded)
        {
            short attributes = _defaultColors;
            Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attributes);
        }
    }

    //
    // Resumen:
    //     Copia un área de origen especificada del búfer de pantalla en un área de destino
    //     determinada.
    //
    // Parámetros:
    //   sourceLeft:
    //     Columna situada más a la izquierda del área de origen.
    //
    //   sourceTop:
    //     Fila superior del área de origen.
    //
    //   sourceWidth:
    //     Número de columnas en el área de origen.
    //
    //   sourceHeight:
    //     Número de filas en el área de origen.
    //
    //   targetLeft:
    //     Columna situada más a la izquierda del área de destino.
    //
    //   targetTop:
    //     Fila superior del área de destino.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     Uno o más de los parámetros es menor que cero. O bien sourceLeft o targetLeft
    //     es mayor o igual que System.Console.BufferWidth. O bien sourceTop o targetTop
    //     es mayor o igual que System.Console.BufferHeight. O bien sourceTop + sourceHeight
    //     es mayor o igual que System.Console.BufferHeight. O bien sourceLeft + sourceWidth
    //     es mayor o igual que System.Console.BufferWidth.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
    {
        MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, ' ', ConsoleColor.Black, BackgroundColor);
    }

    //
    // Resumen:
    //     Copia un área de origen especificada del búfer de pantalla en un área de destino
    //     determinada.
    //
    // Parámetros:
    //   sourceLeft:
    //     Columna situada más a la izquierda del área de origen.
    //
    //   sourceTop:
    //     Fila superior del área de origen.
    //
    //   sourceWidth:
    //     Número de columnas en el área de origen.
    //
    //   sourceHeight:
    //     Número de filas en el área de origen.
    //
    //   targetLeft:
    //     Columna situada más a la izquierda del área de destino.
    //
    //   targetTop:
    //     Fila superior del área de destino.
    //
    //   sourceChar:
    //     Carácter que se usa para rellenar el área de origen.
    //
    //   sourceForeColor:
    //     Color de primer plano que se usa para rellenar el área de origen.
    //
    //   sourceBackColor:
    //     Color de fondo que se usa para rellenar el área de origen.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     Uno o más de los parámetros es menor que cero. O bien sourceLeft o targetLeft
    //     es mayor o igual que System.Console.BufferWidth. O bien sourceTop o targetTop
    //     es mayor o igual que System.Console.BufferHeight. O bien sourceTop + sourceHeight
    //     es mayor o igual que System.Console.BufferHeight. O bien sourceLeft + sourceWidth
    //     es mayor o igual que System.Console.BufferWidth.
    //
    //   T:System.ArgumentException:
    //     Uno o ambos parámetros de color no son miembros de la enumeración System.ConsoleColor.
    //
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public unsafe static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
    {
        if (sourceForeColor < ConsoleColor.Black || sourceForeColor > ConsoleColor.White)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceForeColor");
        }

        if (sourceBackColor < ConsoleColor.Black || sourceBackColor > ConsoleColor.White)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceBackColor");
        }

        Win32Native.COORD dwSize = GetBufferInfo().dwSize;
        if (sourceLeft < 0 || sourceLeft > dwSize.X)
        {
            throw new ArgumentOutOfRangeException("sourceLeft", sourceLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (sourceTop < 0 || sourceTop > dwSize.Y)
        {
            throw new ArgumentOutOfRangeException("sourceTop", sourceTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (sourceWidth < 0 || sourceWidth > dwSize.X - sourceLeft)
        {
            throw new ArgumentOutOfRangeException("sourceWidth", sourceWidth, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (sourceHeight < 0 || sourceTop > dwSize.Y - sourceHeight)
        {
            throw new ArgumentOutOfRangeException("sourceHeight", sourceHeight, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (targetLeft < 0 || targetLeft > dwSize.X)
        {
            throw new ArgumentOutOfRangeException("targetLeft", targetLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (targetTop < 0 || targetTop > dwSize.Y)
        {
            throw new ArgumentOutOfRangeException("targetTop", targetTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (sourceWidth == 0 || sourceHeight == 0)
        {
            return;
        }

        new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
        Win32Native.CHAR_INFO[] array = new Win32Native.CHAR_INFO[sourceWidth * sourceHeight];
        dwSize.X = (short)sourceWidth;
        dwSize.Y = (short)sourceHeight;
        Win32Native.COORD bufferCoord = default(Win32Native.COORD);
        Win32Native.SMALL_RECT readRegion = default(Win32Native.SMALL_RECT);
        readRegion.Left = (short)sourceLeft;
        readRegion.Right = (short)(sourceLeft + sourceWidth - 1);
        readRegion.Top = (short)sourceTop;
        readRegion.Bottom = (short)(sourceTop + sourceHeight - 1);
        bool flag;
        fixed (Win32Native.CHAR_INFO* pBuffer = array)
        {
            flag = Win32Native.ReadConsoleOutput(ConsoleOutputHandle, pBuffer, dwSize, bufferCoord, ref readRegion);
        }

        if (!flag)
        {
            __Error.WinIOError();
        }

        Win32Native.COORD cOORD = default(Win32Native.COORD);
        cOORD.X = (short)sourceLeft;
        Win32Native.Color color = ConsoleColorToColorAttribute(sourceBackColor, isBackground: true);
        color |= ConsoleColorToColorAttribute(sourceForeColor, isBackground: false);
        short wColorAttribute = (short)color;
        for (int i = sourceTop; i < sourceTop + sourceHeight; i++)
        {
            cOORD.Y = (short)i;
            if (!Win32Native.FillConsoleOutputCharacter(ConsoleOutputHandle, sourceChar, sourceWidth, cOORD, out var pNumCharsWritten))
            {
                __Error.WinIOError();
            }

            if (!Win32Native.FillConsoleOutputAttribute(ConsoleOutputHandle, wColorAttribute, sourceWidth, cOORD, out pNumCharsWritten))
            {
                __Error.WinIOError();
            }
        }

        Win32Native.SMALL_RECT writeRegion = default(Win32Native.SMALL_RECT);
        writeRegion.Left = (short)targetLeft;
        writeRegion.Right = (short)(targetLeft + sourceWidth);
        writeRegion.Top = (short)targetTop;
        writeRegion.Bottom = (short)(targetTop + sourceHeight);
        fixed (Win32Native.CHAR_INFO* buffer = array)
        {
            flag = Win32Native.WriteConsoleOutput(ConsoleOutputHandle, buffer, dwSize, bufferCoord, ref writeRegion);
        }
    }

    [SecurityCritical]
    private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo()
    {
        bool succeeded;
        return GetBufferInfo(throwOnNoConsole: true, out succeeded);
    }

    [SecuritySafeCritical]
    private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo(bool throwOnNoConsole, out bool succeeded)
    {
        succeeded = false;
        IntPtr consoleOutputHandle = ConsoleOutputHandle;
        if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
        {
            if (!throwOnNoConsole)
            {
                return default(Win32Native.CONSOLE_SCREEN_BUFFER_INFO);
            }

            throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
        }

        if (!Win32Native.GetConsoleScreenBufferInfo(consoleOutputHandle, out var lpConsoleScreenBufferInfo))
        {
            bool consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-12), out lpConsoleScreenBufferInfo);
            if (!consoleScreenBufferInfo)
            {
                consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-10), out lpConsoleScreenBufferInfo);
            }

            if (!consoleScreenBufferInfo)
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error == 6 && !throwOnNoConsole)
                {
                    return default(Win32Native.CONSOLE_SCREEN_BUFFER_INFO);
                }

                __Error.WinIOError(lastWin32Error, null);
            }
        }

        if (!_haveReadDefaultColors)
        {
            _defaultColors = (byte)((uint)lpConsoleScreenBufferInfo.wAttributes & 0xFFu);
            _haveReadDefaultColors = true;
        }

        succeeded = true;
        return lpConsoleScreenBufferInfo;
    }

    //
    // Resumen:
    //     Establece el alto y el ancho del área del búfer de pantalla en los valores especificados.
    //
    //
    // Parámetros:
    //   width:
    //     Ancho del área del búfer medido en columnas.
    //
    //   height:
    //     Alto del área del búfer medido en filas.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     height o width es menor o igual que cero. O bien height o width es mayor o igual
    //     que System.Int16.MaxValue. O bien width es menor que System.Console.WindowLeft
    //     + System.Console.WindowWidth. O bien height es menor que System.Console.WindowTop
    //     + System.Console.WindowHeight.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public static void SetBufferSize(int width, int height)
    {
        new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
        Win32Native.SMALL_RECT srWindow = GetBufferInfo().srWindow;
        if (width < srWindow.Right + 1 || width >= 32767)
        {
            throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
        }

        if (height < srWindow.Bottom + 1 || height >= 32767)
        {
            throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
        }

        Win32Native.COORD size = default(Win32Native.COORD);
        size.X = (short)width;
        size.Y = (short)height;
        if (!Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size))
        {
            __Error.WinIOError();
        }
    }

    //
    // Resumen:
    //     Establece el alto y el ancho de la ventana de la consola en los valores especificados.
    //
    //
    // Parámetros:
    //   width:
    //     Ancho de la ventana de la consola, medido en columnas.
    //
    //   height:
    //     Alto de la ventana de la consola, medido en filas.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     width o height es menor o igual que cero. O bien width más System.Console.WindowLeft
    //     o height más System.Console.WindowTop es mayor o igual que System.Int16.MaxValue.
    //     O bien width o height es mayor que el mayor ancho o la altura de ventana más
    //     grande posible para la resolución de pantalla y la fuente de consola actuales.
    //
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public unsafe static void SetWindowSize(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
        }

        new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
        Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
        bool flag = false;
        Win32Native.COORD size = default(Win32Native.COORD);
        size.X = bufferInfo.dwSize.X;
        size.Y = bufferInfo.dwSize.Y;
        if (bufferInfo.dwSize.X < bufferInfo.srWindow.Left + width)
        {
            if (bufferInfo.srWindow.Left >= 32767 - width)
            {
                throw new ArgumentOutOfRangeException("width", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
            }

            size.X = (short)(bufferInfo.srWindow.Left + width);
            flag = true;
        }

        if (bufferInfo.dwSize.Y < bufferInfo.srWindow.Top + height)
        {
            if (bufferInfo.srWindow.Top >= 32767 - height)
            {
                throw new ArgumentOutOfRangeException("height", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
            }

            size.Y = (short)(bufferInfo.srWindow.Top + height);
            flag = true;
        }

        if (flag && !Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size))
        {
            __Error.WinIOError();
        }

        Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
        srWindow.Bottom = (short)(srWindow.Top + height - 1);
        srWindow.Right = (short)(srWindow.Left + width - 1);
        if (!Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, absolute: true, &srWindow))
        {
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (flag)
            {
                Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, bufferInfo.dwSize);
            }

            Win32Native.COORD largestConsoleWindowSize = Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle);
            if (width > largestConsoleWindowSize.X)
            {
                throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", largestConsoleWindowSize.X));
            }

            if (height > largestConsoleWindowSize.Y)
            {
                throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", largestConsoleWindowSize.Y));
            }

            __Error.WinIOError(lastWin32Error, string.Empty);
        }
    }

    //
    // Resumen:
    //     Establece la posición de la ventana de la consola con respecto al búfer de pantalla.
    //
    //
    // Parámetros:
    //   left:
    //     Posición en columnas de la esquina superior izquierda de la ventana de la consola.
    //
    //
    //   top:
    //     Posición en filas de la esquina superior izquierda de la ventana de la consola.
    //
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     left o top es menor que cero. O bien left + System.Console.WindowWidth es mayor
    //     que System.Console.BufferWidth. O bien top + System.Console.WindowHeight es mayor
    //     que System.Console.BufferHeight.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public unsafe static void SetWindowPosition(int left, int top)
    {
        new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
        Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
        Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
        int num = left + srWindow.Right - srWindow.Left + 1;
        if (left < 0 || num > bufferInfo.dwSize.X || num < 0)
        {
            throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
        }

        int num2 = top + srWindow.Bottom - srWindow.Top + 1;
        if (top < 0 || num2 > bufferInfo.dwSize.Y || num2 < 0)
        {
            throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
        }

        srWindow.Bottom -= (short)(srWindow.Top - top);
        srWindow.Right -= (short)(srWindow.Left - left);
        srWindow.Left = (short)left;
        srWindow.Top = (short)top;
        if (!Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, absolute: true, &srWindow))
        {
            __Error.WinIOError();
        }
    }

    //
    // Resumen:
    //     Establece la posición del cursor.
    //
    // Parámetros:
    //   left:
    //     Posición en columnas del cursor. Las columnas se numeran de izquierda a derecha
    //     a partir de 0.
    //
    //   top:
    //     Posición en filas del cursor. Las filas se numeran de arriba abajo a partir de
    //     0.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     left o top es menor que cero. O bien left es mayor o igual que System.Console.BufferWidth.
    //     O bien top es mayor o igual que System.Console.BufferHeight.
    //
    //   T:System.Security.SecurityException:
    //     El usuario no tiene permiso para realizar esta acción.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [SecuritySafeCritical]
    public static void SetCursorPosition(int left, int top)
    {
        if (left < 0 || left >= 32767)
        {
            throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        if (top < 0 || top >= 32767)
        {
            throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
        }

        new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
        IntPtr consoleOutputHandle = ConsoleOutputHandle;
        Win32Native.COORD cursorPosition = default(Win32Native.COORD);
        cursorPosition.X = (short)left;
        cursorPosition.Y = (short)top;
        if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, cursorPosition))
        {
            int lastWin32Error = Marshal.GetLastWin32Error();
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
            if (left < 0 || left >= bufferInfo.dwSize.X)
            {
                throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }

            if (top < 0 || top >= bufferInfo.dwSize.Y)
            {
                throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }

            __Error.WinIOError(lastWin32Error, string.Empty);
        }
    }

    [DllImport("QCall", CharSet = CharSet.Ansi)]
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    private static extern int GetTitleNative(StringHandleOnStack outTitle, out int outTitleLength);

    //
    // Resumen:
    //     Obtiene la siguiente tecla de carácter o de función presionada por el usuario.
    //     La tecla presionada se muestra en la ventana de la consola.
    //
    // Devuelve:
    //     Objeto que describe la constante System.ConsoleKey y el carácter Unicode, si
    //     existe, que corresponden a la tecla presionada en la consola. El objeto System.ConsoleKeyInfo
    //     también describe, en una combinación bit a bit de valores de System.ConsoleModifiers,
    //     si alguna de las teclas modificadoras Mayús, Alt o Ctrl se presionaron al mismo
    //     tiempo que la tecla de la consola.
    //
    // Excepciones:
    //   T:System.InvalidOperationException:
    //     La propiedad System.Console.In se ha redirigido desde alguna otra secuencia distinta
    //     de la consola.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static ConsoleKeyInfo ReadKey()
    {
        return ReadKey(intercept: false);
    }

    [SecurityCritical]
    private static bool IsAltKeyDown(Win32Native.InputRecord ir)
    {
        return (ir.keyEvent.controlKeyState & 3) != 0;
    }

    [SecurityCritical]
    private static bool IsKeyDownEvent(Win32Native.InputRecord ir)
    {
        if (ir.eventType == 1)
        {
            return ir.keyEvent.keyDown;
        }

        return false;
    }

    [SecurityCritical]
    private static bool IsModKey(Win32Native.InputRecord ir)
    {
        short virtualKeyCode = ir.keyEvent.virtualKeyCode;
        if ((virtualKeyCode < 16 || virtualKeyCode > 18) && virtualKeyCode != 20 && virtualKeyCode != 144)
        {
            return virtualKeyCode == 145;
        }

        return true;
    }

    //
    // Resumen:
    //     Obtiene la siguiente tecla de carácter o de función presionada por el usuario.
    //     Opcionalmente, la tecla presionada se muestra en la ventana de la consola.
    //
    // Parámetros:
    //   intercept:
    //     Determina si la tecla presionada se muestra en la ventana de la consola. true
    //     para que no se muestre la tecla presionada; de lo contrario, false.
    //
    // Devuelve:
    //     Objeto que describe la constante System.ConsoleKey y el carácter Unicode, si
    //     existe, que corresponden a la tecla presionada en la consola. El objeto System.ConsoleKeyInfo
    //     también describe, en una combinación bit a bit de valores de System.ConsoleModifiers,
    //     si alguna de las teclas modificadoras Mayús, Alt o Ctrl se presionaron al mismo
    //     tiempo que la tecla de la consola.
    //
    // Excepciones:
    //   T:System.InvalidOperationException:
    //     La propiedad System.Console.In se ha redirigido desde alguna otra secuencia distinta
    //     de la consola.
    [SecuritySafeCritical]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static ConsoleKeyInfo ReadKey(bool intercept)
    {
        int numEventsRead = -1;
        Win32Native.InputRecord buffer;
        lock (ReadKeySyncObject)
        {
            if (_cachedInputRecord.eventType == 1)
            {
                buffer = _cachedInputRecord;
                if (_cachedInputRecord.keyEvent.repeatCount == 0)
                {
                    _cachedInputRecord.eventType = -1;
                }
                else
                {
                    _cachedInputRecord.keyEvent.repeatCount--;
                }
            }
            else
            {
                while (true)
                {
                    if (!Win32Native.ReadConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead) || numEventsRead == 0)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleReadKeyOnFile"));
                    }

                    short virtualKeyCode = buffer.keyEvent.virtualKeyCode;
                    if ((!IsKeyDownEvent(buffer) && virtualKeyCode != 18) || (buffer.keyEvent.uChar == '\0' && IsModKey(buffer)))
                    {
                        continue;
                    }

                    ConsoleKey consoleKey = (ConsoleKey)virtualKeyCode;
                    if (!IsAltKeyDown(buffer))
                    {
                        break;
                    }

                    if (consoleKey < ConsoleKey.NumPad0 || consoleKey > ConsoleKey.NumPad9)
                    {
                        switch (consoleKey)
                        {
                            case ConsoleKey.Clear:
                            case ConsoleKey.PageUp:
                            case ConsoleKey.PageDown:
                            case ConsoleKey.End:
                            case ConsoleKey.Home:
                            case ConsoleKey.LeftArrow:
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.RightArrow:
                            case ConsoleKey.DownArrow:
                            case ConsoleKey.Insert:
                                continue;
                        }

                        break;
                    }
                }

                if (buffer.keyEvent.repeatCount > 1)
                {
                    buffer.keyEvent.repeatCount--;
                    _cachedInputRecord = buffer;
                }
            }
        }

        ControlKeyState controlKeyState = (ControlKeyState)buffer.keyEvent.controlKeyState;
        bool shift = (controlKeyState & ControlKeyState.ShiftPressed) != 0;
        bool alt = (controlKeyState & (ControlKeyState.RightAltPressed | ControlKeyState.LeftAltPressed)) != 0;
        bool control = (controlKeyState & (ControlKeyState.RightCtrlPressed | ControlKeyState.LeftCtrlPressed)) != 0;
        ConsoleKeyInfo result = new ConsoleKeyInfo(buffer.keyEvent.uChar, (ConsoleKey)buffer.keyEvent.virtualKeyCode, shift, alt, control);
        if (!intercept)
        {
            Write(buffer.keyEvent.uChar);
        }

        return result;
    }

    private static bool BreakEvent(int controlType)
    {
        if (controlType == 0 || controlType == 1)
        {
            ConsoleCancelEventHandler cancelCallbacks = _cancelCallbacks;
            if (cancelCallbacks == null)
            {
                return false;
            }

            ConsoleSpecialKey controlKey = ((controlType != 0) ? ConsoleSpecialKey.ControlBreak : ConsoleSpecialKey.ControlC);
            ControlCDelegateData controlCDelegateData = new ControlCDelegateData(controlKey, cancelCallbacks);
            WaitCallback callBack = ControlCDelegate;
            if (!ThreadPool.QueueUserWorkItem(callBack, controlCDelegateData))
            {
                return false;
            }

            TimeSpan timeout = new TimeSpan(0, 0, 30);
            controlCDelegateData.CompletionEvent.WaitOne(timeout, exitContext: false);
            if (!controlCDelegateData.DelegateStarted)
            {
                return false;
            }

            controlCDelegateData.CompletionEvent.WaitOne();
            controlCDelegateData.CompletionEvent.Close();
            return controlCDelegateData.Cancel;
        }

        return false;
    }

    private static void ControlCDelegate(object data)
    {
        ControlCDelegateData controlCDelegateData = (ControlCDelegateData)data;
        try
        {
            controlCDelegateData.DelegateStarted = true;
            ConsoleCancelEventArgs consoleCancelEventArgs = new ConsoleCancelEventArgs(controlCDelegateData.ControlKey);
            controlCDelegateData.CancelCallbacks(null, consoleCancelEventArgs);
            controlCDelegateData.Cancel = consoleCancelEventArgs.Cancel;
        }
        finally
        {
            controlCDelegateData.CompletionEvent.Set();
        }
    }

    //
    // Resumen:
    //     Adquiere el flujo de error estándar.
    //
    // Devuelve:
    //     El flujo de error estándar.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static Stream OpenStandardError()
    {
        return OpenStandardError(256);
    }

    //
    // Resumen:
    //     Adquiere el flujo de error estándar, que se establece en un tamaño de búfer especificado.
    //
    //
    // Parámetros:
    //   bufferSize:
    //     Tamaño del búfer de flujo interno.
    //
    // Devuelve:
    //     El flujo de error estándar.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     bufferSize es menor o igual que cero.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static Stream OpenStandardError(int bufferSize)
    {
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        return GetStandardFile(-12, FileAccess.Write, bufferSize);
    }

    //
    // Resumen:
    //     Adquiere el flujo de entrada estándar.
    //
    // Devuelve:
    //     Flujo de entrada estándar.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static Stream OpenStandardInput()
    {
        return OpenStandardInput(256);
    }

    //
    // Resumen:
    //     Adquiere el flujo de entrada estándar, que se establece en un tamaño de búfer
    //     especificado.
    //
    // Parámetros:
    //   bufferSize:
    //     Tamaño del búfer de flujo interno.
    //
    // Devuelve:
    //     Flujo de entrada estándar.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     bufferSize es menor o igual que cero.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static Stream OpenStandardInput(int bufferSize)
    {
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        return GetStandardFile(-10, FileAccess.Read, bufferSize);
    }

    //
    // Resumen:
    //     Adquiere el flujo de salida estándar.
    //
    // Devuelve:
    //     Flujo de salida estándar.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static Stream OpenStandardOutput()
    {
        return OpenStandardOutput(256);
    }

    //
    // Resumen:
    //     Adquiere el flujo de salida estándar, que se establece en un tamaño de búfer
    //     especificado.
    //
    // Parámetros:
    //   bufferSize:
    //     Tamaño del búfer de flujo interno.
    //
    // Devuelve:
    //     Flujo de salida estándar.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     bufferSize es menor o igual que cero.
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static Stream OpenStandardOutput(int bufferSize)
    {
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        return GetStandardFile(-11, FileAccess.Write, bufferSize);
    }

    //
    // Resumen:
    //     Establece la propiedad System.Console.In en el objeto System.IO.TextReader especificado.
    //
    //
    // Parámetros:
    //   newIn:
    //     Flujo que constituye la nueva entrada estándar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de newIn es null.
    //
    //   T:System.Security.SecurityException:
    //     El llamador no dispone del permiso requerido.
    [SecuritySafeCritical]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void SetIn(TextReader newIn)
    {
        if (newIn == null)
        {
            throw new ArgumentNullException("newIn");
        }

        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
        newIn = TextReader.Synchronized(newIn);
        lock (InternalSyncObject)
        {
            _in = newIn;
        }
    }

    //
    // Resumen:
    //     Establece la propiedad System.Console.Out en el objeto System.IO.TextWriter especificado.
    //
    //
    // Parámetros:
    //   newOut:
    //     Flujo que constituye la nueva salida estándar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de newOut es null.
    //
    //   T:System.Security.SecurityException:
    //     El llamador no dispone del permiso requerido.
    [SecuritySafeCritical]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void SetOut(TextWriter newOut)
    {
        if (newOut == null)
        {
            throw new ArgumentNullException("newOut");
        }

        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
        _isOutTextWriterRedirected = true;
        newOut = TextWriter.Synchronized(newOut);
        lock (InternalSyncObject)
        {
            _out = newOut;
        }
    }

    //
    // Resumen:
    //     Establece la propiedad System.Console.Error en el objeto System.IO.TextWriter
    //     especificado.
    //
    // Parámetros:
    //   newError:
    //     Flujo que constituye la nueva salida de error estándar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de newError es null.
    //
    //   T:System.Security.SecurityException:
    //     El llamador no dispone del permiso requerido.
    [SecuritySafeCritical]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void SetError(TextWriter newError)
    {
        if (newError == null)
        {
            throw new ArgumentNullException("newError");
        }

        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
        _isErrorTextWriterRedirected = true;
        newError = TextWriter.Synchronized(newError);
        lock (InternalSyncObject)
        {
            _error = newError;
        }
    }

    //
    // Resumen:
    //     Lee el siguiente carácter del flujo de entrada estándar.
    //
    // Devuelve:
    //     El carácter siguiente del flujo de entrada o menos uno (-1) si no hay actualmente
    //     más caracteres que leer.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static int Read()
    {
        return In.Read();
    }

    //
    // Resumen:
    //     Lee la siguiente línea de caracteres del flujo de entrada estándar.
    //
    // Devuelve:
    //     La siguiente línea de caracteres del flujo de entrada o null si no hay más líneas
    //     disponibles.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.OutOfMemoryException:
    //     No hay memoria suficiente para asignar un búfer para la cadena devuelta.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     El número de caracteres de la siguiente línea de caracteres es mayor que System.Int32.MaxValue.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static string ReadLine()
    {
        return In.ReadLine();
    }

    //
    // Resumen:
    //     Escribe el terminador de línea actual en el flujo de salida estándar.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine()
    {
        Out.WriteLine();
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor booleano especificado, seguida del
    //     terminador de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(bool value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe el carácter Unicode especificado, seguido del terminador de línea actual,
    //     en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(char value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la matriz de caracteres Unicode especificada, seguida del terminador
    //     de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   buffer:
    //     Matriz de caracteres Unicode.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(char[] buffer)
    {
        Out.WriteLine(buffer);
    }

    //
    // Resumen:
    //     Escribe la submatriz de caracteres Unicode especificada, seguida del terminador
    //     de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   buffer:
    //     Matriz de caracteres Unicode.
    //
    //   index:
    //     Posición inicial en buffer.
    //
    //   count:
    //     Número de caracteres que se van a escribir.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de buffer es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index o count es menor que cero.
    //
    //   T:System.ArgumentException:
    //     index más count especifica una posición que no está dentro de buffer.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(char[] buffer, int index, int count)
    {
        Out.WriteLine(buffer, index, count);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor System.Decimal especificado, seguido
    //     del terminador de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(decimal value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor de punto flotante de precisión doble
    //     especificado, seguido del terminador de línea actual, en el flujo de salida estándar.
    //
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(double value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor de punto flotante de precisión sencilla
    //     especificado, seguido del terminador de línea actual, en el flujo de salida estándar.
    //
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(float value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 32 bits con signo especificado,
    //     seguido del terminador de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(int value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 32 bits sin signo especificado,
    //     seguido del terminador de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [CLSCompliant(false)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(uint value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 64 bits con signo especificado,
    //     seguido del terminador de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(long value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 64 bits sin signo especificado,
    //     seguido del terminador de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [CLSCompliant(false)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(ulong value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del objeto especificado, seguida del terminador
    //     de línea actual, en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(object value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe el valor de cadena especificado, seguido del terminador de línea actual,
    //     en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(string value)
    {
        Out.WriteLine(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del objeto especificado, seguida del terminador
    //     de línea actual, en el flujo de salida estándar usando la información de formato
    //     especificada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg0:
    //     Objeto que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(string format, object arg0)
    {
        Out.WriteLine(format, arg0);
    }

    //
    // Resumen:
    //     Escribe la representación de texto de los objetos especificados, seguida del
    //     terminador de línea actual, en el flujo de salida estándar usando la información
    //     de formato especificada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg0:
    //     Primer objeto que se va a escribir con format.
    //
    //   arg1:
    //     Segundo objeto que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(string format, object arg0, object arg1)
    {
        Out.WriteLine(format, arg0, arg1);
    }

    //
    // Resumen:
    //     Escribe la representación de texto de los objetos especificados, seguida del
    //     terminador de línea actual, en el flujo de salida estándar usando la información
    //     de formato especificada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg0:
    //     Primer objeto que se va a escribir con format.
    //
    //   arg1:
    //     Segundo objeto que se va a escribir con format.
    //
    //   arg2:
    //     Tercer objeto que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(string format, object arg0, object arg1, object arg2)
    {
        Out.WriteLine(format, arg0, arg1, arg2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [CLSCompliant(false)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
    {
        ArgIterator argIterator = new ArgIterator(__arglist);
        int num = argIterator.GetRemainingCount() + 4;
        object[] array = new object[num];
        array[0] = arg0;
        array[1] = arg1;
        array[2] = arg2;
        array[3] = arg3;
        for (int i = 4; i < num; i++)
        {
            array[i] = TypedReference.ToObject(argIterator.GetNextArg());
        }

        Out.WriteLine(format, array);
    }

    //
    // Resumen:
    //     Escribe la representación de texto de la matriz de objetos especificada, seguida
    //     del terminador de línea actual, en el flujo de salida estándar usando la información
    //     de formato especificada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg:
    //     Matriz de objetos que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format o arg es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void WriteLine(string format, params object[] arg)
    {
        if (arg == null)
        {
            Out.WriteLine(format, null, null);
        }
        else
        {
            Out.WriteLine(format, arg);
        }
    }

    //
    // Resumen:
    //     Escribe la representación de texto del objeto especificado en el flujo de salida
    //     estándar usando la información de formato indicada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg0:
    //     Objeto que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(string format, object arg0)
    {
        Out.Write(format, arg0);
    }

    //
    // Resumen:
    //     Escribe la representación de texto de los objetos especificados en el flujo de
    //     salida estándar usando la información de formato indicada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg0:
    //     Primer objeto que se va a escribir con format.
    //
    //   arg1:
    //     Segundo objeto que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(string format, object arg0, object arg1)
    {
        Out.Write(format, arg0, arg1);
    }

    //
    // Resumen:
    //     Escribe la representación de texto de los objetos especificados en el flujo de
    //     salida estándar usando la información de formato indicada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg0:
    //     Primer objeto que se va a escribir con format.
    //
    //   arg1:
    //     Segundo objeto que se va a escribir con format.
    //
    //   arg2:
    //     Tercer objeto que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(string format, object arg0, object arg1, object arg2)
    {
        Out.Write(format, arg0, arg1, arg2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [CLSCompliant(false)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
    {
        ArgIterator argIterator = new ArgIterator(__arglist);
        int num = argIterator.GetRemainingCount() + 4;
        object[] array = new object[num];
        array[0] = arg0;
        array[1] = arg1;
        array[2] = arg2;
        array[3] = arg3;
        for (int i = 4; i < num; i++)
        {
            array[i] = TypedReference.ToObject(argIterator.GetNextArg());
        }

        Out.Write(format, array);
    }

    //
    // Resumen:
    //     Escribe la representación de texto de la matriz de objetos especificada en el
    //     flujo de salida estándar usando la información de formato especificada.
    //
    // Parámetros:
    //   format:
    //     Cadena de formato compuesto.
    //
    //   arg:
    //     Matriz de objetos que se va a escribir con format.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    //
    //   T:System.ArgumentNullException:
    //     El valor de format o arg es null.
    //
    //   T:System.FormatException:
    //     La especificación de formato de format no es válida.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(string format, params object[] arg)
    {
        if (arg == null)
        {
            Out.Write(format, null, null);
        }
        else
        {
            Out.Write(format, arg);
        }
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor booleano especificado en el flujo
    //     de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(bool value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe el valor del carácter Unicode especificado en el flujo de salida estándar.
    //
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(char value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la matriz especificada de caracteres Unicode en el flujo de salida estándar.
    //
    //
    // Parámetros:
    //   buffer:
    //     Matriz de caracteres Unicode.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(char[] buffer)
    {
        Out.Write(buffer);
    }

    //
    // Resumen:
    //     Escribe la submatriz de caracteres Unicode especificada en el flujo de salida
    //     estándar.
    //
    // Parámetros:
    //   buffer:
    //     Matriz de caracteres Unicode.
    //
    //   index:
    //     Posición inicial en buffer.
    //
    //   count:
    //     Número de caracteres que se van a escribir.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     El valor de buffer es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index o count es menor que cero.
    //
    //   T:System.ArgumentException:
    //     index más count especifica una posición que no está dentro de buffer.
    //
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(char[] buffer, int index, int count)
    {
        Out.Write(buffer, index, count);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor de punto flotante de precisión doble
    //     especificado en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(double value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor System.Decimal especificado en el
    //     flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(decimal value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor de punto flotante de precisión sencilla
    //     especificado en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(float value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 32 bits con signo especificado
    //     en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(int value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 32 bits sin signo especificado
    //     en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [CLSCompliant(false)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(uint value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 64 bits con signo especificado
    //     en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(long value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del valor entero de 64 bits sin signo especificado
    //     en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [CLSCompliant(false)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(ulong value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe la representación de texto del objeto especificado en el flujo de salida
    //     estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir o null.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(object value)
    {
        Out.Write(value);
    }

    //
    // Resumen:
    //     Escribe el valor de cadena especificado en el flujo de salida estándar.
    //
    // Parámetros:
    //   value:
    //     Valor que se va a escribir.
    //
    // Excepciones:
    //   T:System.IO.IOException:
    //     Error de E/S.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [HostProtection(SecurityAction.LinkDemand, UI = true)]
    public static void Write(string value)
    {
        Out.Write(value);
    }
}
