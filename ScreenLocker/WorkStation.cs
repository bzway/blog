﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Linq;
namespace Bzway
{
    public class Git
    {
        public Git(string root, string auth, string comments)
        {
            #region 1. 遍历工作目录中所有文件
            var files = this.GetFiles(root);
            #endregion

            #region 2.生成新版本快照
            var stageVersionId = this.GetStageVersionId(files, root);
            #endregion

            #region 3.与最近版本比较差异
            var listOfVersion = GetVersions(root);
            var lastVersion = listOfVersion.LastOrDefault();
            if (lastVersion == null || lastVersion.Id != stageVersionId)
            {
                listOfVersion.Add(new Version()
                {
                    Id = stageVersionId,
                    PId = lastVersion == null ? null : lastVersion.Id,
                    Time = DateTime.UtcNow,
                    Auth = auth,
                    Comments = comments,
                });
                #region 4.如果有差异，则提交新版本号（版本树）到本地库
                SaveVersion(root, listOfVersion);
                #endregion
            }
            #endregion

        }
        public List<FileInfo> GetFiles(string root)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (var item in Directory.GetFiles(root, "*.*", SearchOption.TopDirectoryOnly))
            {
                list.Add(new FileInfo(item));
            }
            foreach (var item in Directory.GetDirectories(root, "*.*", SearchOption.TopDirectoryOnly))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(item);
                if (directoryInfo.Name == ".git")
                {
                    continue;
                }
                foreach (var file in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    list.Add(file);
                }
            }
            return list;
        }
        public string GetStageVersionId(List<FileInfo> files, string root)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var path = file.FullName.Remove(0, root.Length + 1).Replace("\\", "/");
                var data = GetFileData(root, file);
                dictionary.Add(path, data);
            }
            var tempPath = Path.GetTempFileName();
            using (Stream tempStream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(tempStream, dictionary);
                tempStream.Flush();
            }

            var id = GetFileData(root, new FileInfo(tempPath));
            return id;
        }
        public void SaveVersion(string root, List<Version> list)
        {
            var versionFilePath = Path.Combine(root, ".git", "version");
            try
            {
                using (var versionFileStream = File.OpenWrite(versionFilePath))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(versionFileStream, list);
                }
            }
            catch { }
        }
        public List<Version> GetVersions(string root)
        {
            var versionFilePath = Path.Combine(root, ".git", "version");
            var o = new List<Version>();
            try
            {
                using (var versionFileStream = File.OpenRead(versionFilePath))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    o = (List<Version>)formatter.Deserialize(versionFileStream);
                }
            }
            catch { }
            return o;
        }
        public string GetFileData(string root, FileInfo file, bool zip = true)
        {
            byte[] buffer;
            using (var stream = file.OpenRead())
            {
                if (zip)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                        {
                            stream.CopyTo(gZipStream);
                            buffer = new byte[memoryStream.Length];
                            memoryStream.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
                else
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                }

            }
            var hashData = sha1(buffer);
            var dataFilePath = Path.Combine(root, ".git", "data", hashData);
            FileInfo dataFileInfo = new FileInfo(dataFilePath);
            if (!dataFileInfo.Exists)
            {
                if (!dataFileInfo.Directory.Exists)
                {
                    dataFileInfo.Directory.Create();
                }
                using (var stream = dataFileInfo.Create())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            return hashData;
        }
        public string sha1(byte[] data)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] hash = sha1.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        [Serializable]
        public class Version
        {
            public string Id { get; set; }
            public string PId { get; set; }
            public string Auth { get; set; }
            public DateTime Time { get; set; }
            public string Comments { get; set; }
        }
    }
    public class WorkStation
    {
        static WorkStation _me;
        private WorkStation()
        {
        }
        public static WorkStation Instance
        {
            get
            {
                if (_me == null)
                {
                    _me = new WorkStation();
                }
                return _me;
            }
        }

        public void LockWorkStation(bool Block)
        {
            try
            {
                BlockInput(Block);
                LockCtrlAltDelete(Block);
            }
            catch (Exception ex)
            {
                var msss = ex.Message;

            }
        }

        [DllImport("user32.dll")]
        static extern void BlockInput(bool Block);

        void LockCtrlAltDelete(bool Block)
        {

        }

    }
}