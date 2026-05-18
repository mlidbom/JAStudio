using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Compze.Threading;
using Python.Runtime;

namespace JAStudio.PythonInterop.Utilities;

public static class PythonEnvironment
{
   static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

   public static PythonObjectWrapper Import(string module) => Use(() => new PythonObjectWrapper(Py.Import(module)));

   public static TResult Use<TResult>(Func<TResult> func)
   {
      using(LockGil())
         return func();
   }

   public static void Use(Action action) => Use(() => { action(); return 0; });

   public static IDisposable LockGil()
   {
      EnsureInitialized();
      return Py.GIL();
   }

   static readonly IMonitor Monitor = IMonitor.New();

   /// <param name="venvPath">Optional path to venv. If not provided, uses JASTUDIO_VENV_PATH environment variable or auto-detects.</param>
   public static void EnsureInitialized(string? venvPath = null)
   {
      if(PythonEngine.IsInitialized) return;
      Monitor.Locked(() =>
      {
         if(PythonEngine.IsInitialized) return;

         venvPath ??= GetVenvPath();

         if(!Directory.Exists(venvPath))
         {
            throw new Exception($"""
                                 Python venv not found at: {venvPath}
                                 Please set JASTUDIO_VENV_PATH environment variable to your venv location.
                                 Current directory: {Directory.GetCurrentDirectory()}
                                 """);
         }

         // Read pyvenv.cfg to find base Python installation
         var pyvenvCfg = Path.Combine(venvPath, "pyvenv.cfg");
         if(!File.Exists(pyvenvCfg))
         {
            throw new Exception($"pyvenv.cfg not found at: {pyvenvCfg}");
         }

         var basePython = GetBasePythonFromConfig(pyvenvCfg);
         if(basePython == null)
         {
            throw new Exception($"Could not find base Python from {pyvenvCfg}");
         }

         // Find the Python shared library
         var pythonDll = FindPythonDll(venvPath, basePython);
         if(pythonDll == null)
         {
            throw new Exception($"Could not find Python shared library in {venvPath} or {basePython}");
         }

         Console.WriteLine($"[PythonEnvironment] Using venv: {venvPath}");
         Console.WriteLine($"[PythonEnvironment] Base Python: {basePython}");
         Console.WriteLine($"[PythonEnvironment] Python DLL: {pythonDll}");

         // Configure Python.NET
         Runtime.PythonDLL = pythonDll;
         PythonEngine.PythonHome = IsLinux ? Path.GetDirectoryName(basePython)! : basePython;

         if(IsLinux)
         {
            var pythonVersion = GetPythonVersionFromConfig(pyvenvCfg);
            PythonEngine.PythonPath = string.Join(
               Path.PathSeparator.ToString(),
               Path.Combine(basePython, "..", "lib", pythonVersion),
               Path.Combine(basePython, "..", "lib", pythonVersion, "lib-dynload"),
               Path.Combine(venvPath, "lib", pythonVersion, "site-packages")
            );
         } else
         {
            PythonEngine.PythonPath = string.Join(
               Path.PathSeparator.ToString(),
               Path.Combine(basePython, "Lib"),
               Path.Combine(venvPath, "Lib", "site-packages"),
               Path.Combine(basePython, "DLLs")
            );
         }

         PythonEngine.Initialize();
         PythonEngine.BeginAllowThreads();
      });
   }

   static string GetVenvPath()
   {
      var envPath = Environment.GetEnvironmentVariable("JASTUDIO_VENV_PATH");
      if(!string.IsNullOrEmpty(envPath))
      {
         return envPath;
      }

      var projectRoot = Path.GetFullPath(Path.Combine(
                                            Directory.GetCurrentDirectory(),
                                            "..",
                                            "..",
                                            "..",
                                            "..",
                                            "..",
                                            ".."
                                         ));
      return Path.Combine(projectRoot, "venv");
   }

   static string? GetBasePythonFromConfig(string pyvenvCfgPath)
   {
      return File.ReadAllLines(pyvenvCfgPath)
                 .Where(line => line.StartsWith("home = "))
                 .Select(line => line[7..].Trim())
                 .FirstOrDefault();
   }

   static string GetPythonVersionFromConfig(string pyvenvCfgPath)
   {
      var versionLine = File.ReadAllLines(pyvenvCfgPath)
                            .Where(line => line.StartsWith("version = "))
                            .Select(line => line[10..].Trim())
                            .FirstOrDefault();

      if(versionLine != null)
      {
         var parts = versionLine.Split('.');
         if(parts.Length >= 2) return $"python{parts[0]}.{parts[1]}";
      }

      return "python3";
   }

   static string? FindPythonDll(string venvPath, string basePython)
   {
      if(IsLinux)
      {
         return FindPythonSharedLibraryLinux(basePython);
      }

      return FindPythonDllWindows(venvPath, basePython);
   }

   static string? FindPythonDllWindows(string venvPath, string basePython)
   {
      // Try venv\Scripts first (some setups copy the DLL there)
      var venvScripts = Path.Combine(venvPath, "Scripts");
      if(Directory.Exists(venvScripts))
      {
         var venvDll = Directory.GetFiles(venvScripts, "python3??.dll")
                                .OrderByDescending(f => f)
                                .FirstOrDefault();
         if(venvDll != null)
            return venvDll;
      }

      // Fall back to base Python installation (standard Windows venv behavior)
      if(Directory.Exists(basePython))
      {
         var baseDll = Directory.GetFiles(basePython, "python3??.dll")
                                .OrderByDescending(f => f)
                                .FirstOrDefault();
         if(baseDll != null)
            return baseDll;
      }

      return null;
   }

   static string? FindPythonSharedLibraryLinux(string basePython)
   {
      // On Linux, basePython from pyvenv.cfg "home" is the bin directory (e.g. /usr/bin)
      // The shared library is typically in a lib directory like /usr/lib/x86_64-linux-gnu/ or /usr/lib/
      var prefixDir = Path.GetDirectoryName(basePython);
      if(prefixDir == null) return null;

      // Search common library paths
      var searchDirs = new[]
                       {
                          Path.Combine(prefixDir, "lib"),
                          Path.Combine(prefixDir, "lib", "x86_64-linux-gnu"),
                          "/usr/lib",
                          "/usr/lib/x86_64-linux-gnu",
                          "/usr/lib64"
                       };

      // Try to find exact version match first (e.g. libpython3.12.so)
      var pythonExe = Directory.GetFiles(basePython, "python3.*")
                               .Where(f => !f.Contains('-'))
                               .OrderByDescending(f => f)
                               .FirstOrDefault();

      string? versionSuffix = null;
      if(pythonExe != null)
      {
         var exeName = Path.GetFileName(pythonExe);
         // "python3.12" → "3.12"
         if(exeName.StartsWith("python"))
            versionSuffix = exeName["python".Length..];
      }

      foreach(var dir in searchDirs)
      {
         if(!Directory.Exists(dir)) continue;

         // Prefer exact version match (e.g. libpython3.12.so)
         if(versionSuffix != null)
         {
            var exactMatch = Path.Combine(dir, $"libpython{versionSuffix}.so");
            if(File.Exists(exactMatch))
               return exactMatch;
         }

         // Fallback: find any libpython3.X.so (not the unversioned libpython3.so)
         var so = Directory.GetFiles(dir, "libpython3.*.so")
                           .Where(f => !f.EndsWith(".a"))
                           .OrderByDescending(f => f)
                           .FirstOrDefault();

         if(so != null)
         {
            return so;
         }
      }

      return null;
   }
}
