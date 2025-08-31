using System;
using System.IO;
using UnityEngine;

namespace FantasyColony.Core.Services
{
    public interface ILogger
    {
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg, Exception ex = null);
    }

    public sealed class FileLogger : ILogger
    {
        private readonly string _path;

        public FileLogger()
        {
            var dir = Path.Combine(UnityEngine.Application.persistentDataPath, "logs");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            Info($"FantasyColony boot {UnityEngine.Application.version} Unity {UnityEngine.Application.unityVersion}");
        }

        public void Info(string msg)
        {
            Debug.Log(msg);
            Append("INFO", msg);
        }

        public void Warn(string msg)
        {
            Debug.LogWarning(msg);
            Append("WARN", msg);
        }

        public void Error(string msg, Exception ex = null)
        {
            Debug.LogError(msg);
            if (ex != null) msg += "\n" + ex;
            Append("ERROR", msg);
        }

        private void Append(string level, string msg)
        {
            try
            {
                File.AppendAllText(_path, $"[{DateTime.Now:HH:mm:ss}] {level} {msg}\n");
            }
            catch { /* ignore logging failures */ }
        }
    }
}
