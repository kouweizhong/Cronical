﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Cronical.Logging;
using Cronical.Misc;

namespace Cronical
{
  internal class InjectCtrlC
  {
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AttachConsole(uint dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern bool FreeConsole();
    [DllImport("kernel32.dll")]
    private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    internal static void Break(int processId)
    {
      var cmd = Environment.GetCommandLineArgs().First().Replace("vshost.", "");
      var args = "ctrlc " + processId + " " + Checksum(processId);

      var start = new ProcessStartInfo(cmd, args)
      {
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden
      };

      var result = Process.Start(start);
      if (result == null)
        Logger.Warn("CtrlC: Unable to start kill process: " + cmd);
    }

    internal static void Handle(string[] args)
    {
      var pid = int.Parse(args.ElementAt(1));
      var checksum = int.Parse(args.ElementAt(2));

      if (Checksum(pid) != checksum)
        throw new Exception("CtrlC: Checksum fails.");

      Logger.Log("CtrlC: Sending BREAK to process " + pid);

      FreeConsole();
      if (!AttachConsole((uint)pid))
      {
        Logger.Debug("Unable to attach to console for process " + pid);
        return;
      }

      GenerateConsoleCtrlEvent(1 /* CTRL-BREAK */, 0);

      // Should not go past this point...
    }

    private static int Checksum(int pid)
    {
      return Crc32.ComputeChecksum(Encoding.ASCII.GetBytes("Lb34c2enjDmD9Zv4MS8xaCB" + pid));
    }
  }
}
