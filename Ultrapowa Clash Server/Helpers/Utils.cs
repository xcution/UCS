﻿/*
 * Program : Ultrapowa Clash Server
 * Description : A C# Writted 'Clash of Clans' Server Emulator !
 *
 * Authors:  Jean-Baptiste Martin <Ultrapowa at Ultrapowa.com>,
 *           And the Official Ultrapowa Developement Team
 *
 * Copyright (c) 2016  UltraPowa
 * All Rights Reserved.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UCS.Core;
using UCS.Files.Logic;
using UCS.Logic;

namespace UCS.Helpers
{
    internal static class Utils
    {
        #region Public Properties

        /// <summary>
        ///     Returns Proxy-Version in the following format: v1.2.3.4
        /// </summary>
        public static string AssemblyVersion
        {
            get { return "v" + Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        ///     Returns SERVER-Version in the following format: v1.2.3.4 - © 2016
        /// </summary>
        public static string VersionTitle
        {
            get { return "Ultrapowa Clash Server v" + Assembly.GetExecutingAssembly().GetName().Version + " - © 2016"; }
        }
    
        /// <summary>
        ///     Returns opened instances
        /// </summary>
        public static int OpenedInstances
        {
            get
            {
                return
                    Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location))
                           .Length;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static void AddDataSlots(this List<byte> list, List<DataSlot> data)
        {
            list.AddInt32(data.Count);
            foreach (var dataSlot in data)
            {
                list.AddRange(dataSlot.Encode());
            }
        }

        public static void AddInt32(this List<byte> list, int data)
        {
            list.AddRange(BitConverter.GetBytes(data).Reverse());
        }

        public static void AddInt64(this List<byte> list, long data)
        {
            list.AddRange(BitConverter.GetBytes(data).Reverse());
        }

        public static void AddString(this List<byte> list, string data)
        {
            if (data == null)
                list.AddRange(BitConverter.GetBytes(-1).Reverse());
            else
            {
                list.AddRange(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(data)).Reverse());
                list.AddRange(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string ByteArrayToHex(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ").ToUpper();
        }

        public static byte[] HexaToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static void Increment(this byte[] nonce, int timesToIncrease = 2)
        {
            /*
            int __cdecl sodium_increment(unsigned char nonce, unsigned int nonceLength)
            {
                __int64 v2 = 1i64; // rax@1
                do
                {
                     LODWORD(v2) = *(_BYTE *)(HIDWORD(v2) + a1) + (_DWORD)v2;
                     *(_BYTE *)(HIDWORD(v2)++ + a1) = v2;
                     LODWORD(v2) = (unsigned int)v2 >> 8;
                }
                while ( HIDWORD(v2) < a2 );
             return v2;
            }
            */
            for (int j = 0; j < timesToIncrease; j++)
            {
                ushort c = 1;
                for (UInt32 i = 0; i < nonce.Length; i++)
                {
                    c += (ushort)nonce[i];
                    nonce[i] = (byte)c;
                    c >>= 8;
                }
            }
        }
        public static int ParseConfigInt(string str)
        {
            return int.Parse(ConfigurationManager.AppSettings[str]);
        }

        public static string parseConfigString(string str)
        {
            return ConfigurationManager.AppSettings[str];
        }

        public static byte[] ReadAllBytes(this BinaryReader br)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[bufferSize];
                int count;
                while ((count = br.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        public static Data ReadDataReference(this BinaryReader br)
        {
            var id = br.ReadInt32WithEndian();
            return ObjectManager.DataTables.GetDataById(id);
        }

        public static int ReadInt32WithEndian(this BinaryReader br)
        {
            var a32 = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }

        public static long ReadInt64WithEndian(this BinaryReader br)
        {
            var a64 = br.ReadBytes(8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a64);
            return BitConverter.ToInt64(a64, 0);
        }

        public static string ReadScString(this BinaryReader br)
        {
            var stringLength = br.ReadInt32WithEndian();
            string result;

            if (stringLength > -1)
            {
                if (stringLength > 0)
                {
                    var astr = br.ReadBytes(stringLength);
                    result = Encoding.UTF8.GetString(astr);
                }
                else
                {
                    result = string.Empty;
                }
            }
            else
                result = null;
            return result;
        }

        public static ushort ReadUInt16WithEndian(this BinaryReader br)
        {
            var a16 = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a16);
            return BitConverter.ToUInt16(a16, 0);
        }

        public static uint ReadUInt32WithEndian(this BinaryReader br)
        {
            var a32 = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a32);
            return BitConverter.ToUInt32(a32, 0);
        }

        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            TValue ignored;
            return self.TryRemove(key, out ignored);
        }

        #endregion Public Methods
    }
}