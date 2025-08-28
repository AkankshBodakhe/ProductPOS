using System;
using System.IO;
using System.Runtime.InteropServices;

namespace POSApp.Helpers
{
    public static class RawPrinterHelper
    {
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter")]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter")]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter")]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter")]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true)]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential)]
        public struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        public static void SendPdfToPrinter(string printerName, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDF file not found.", filePath);

            byte[] bytes = File.ReadAllBytes(filePath);
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);

            IntPtr hPrinter;
            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                throw new Exception("Could not open printer: " + printerName);

            var di = new DOCINFOA
            {
                pDocName = Path.GetFileName(filePath),
                pDataType = "RAW"
            };

            if (StartDocPrinter(hPrinter, 1, ref di))
            {
                StartPagePrinter(hPrinter);
                WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out int written);
                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);
            }
            ClosePrinter(hPrinter);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
        }
    }
}
