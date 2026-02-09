// %%%%%%    @%%%%%@
//%%%%%%%%   %%%%%%%@
//@%%%%%%%@  %%%%%%%%%        @@      @@  @@@      @@@ @@@     @@@ @@@@@@@@@@   @@@@@@@@@
//%%%%%%%%@ @%%%%%%%%       @@@@@   @@@@ @@@@@   @@@@ @@@@   @@@@ @@@@@@@@@@@@@@@@@@@@@@@ @@@@
// @%%%%%%%%  %%%%%%%%%      @@@@@@  @@@@  @@@@  @@@@   @@@@@@@@@     @@@@    @@@@         @@@@
//  %%%%%%%%%  %%%%%%%%@     @@@@@@@ @@@@   @@@@@@@@     @@@@@@       @@@@    @@@@@@@@@@@  @@@@
//   %%%%%%%%@  %%%%%%%%%    @@@@@@@@@@@@     @@@@        @@@@@       @@@@    @@@@@@@@@@@  @@@@
//    %%%%%%%%@ @%%%%%%%%    @@@@ @@@@@@@     @@@@      @@@@@@@@      @@@@    @@@@         @@@@
//    @%%%%%%%%% @%%%%%%%%   @@@@   @@@@@     @@@@     @@@@@ @@@@@    @@@@    @@@@@@@@@@@@ @@@@@@@@@@
//     @%%%%%%%%  %%%%%%%%@  @@@@    @@@@     @@@@    @@@@     @@@@   @@@@    @@@@@@@@@@@@ @@@@@@@@@@@
//      %%%%%%%%@ @%%%%%%%%
//      @%%%%%%%%  @%%%%%%%%
//       %%%%%%%%   %%%%%%%@
//         %%%%%      %%%%
//
// Copyright (C) 2025-2026 NyxTel Wireless / Nyx Gallini
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ClosedSkyCPSWinForms
{
    internal class ConfigurationFile
    {
        private const int KeySize = 256;

        private const int Iterations = 100000;

        private static readonly byte[] Salt = "ClosedSkyCPS2025"u8.ToArray();

        public class ConfigData
        {
            public string ESN { get; set; } = string.Empty;

            public DateTime SavedDate { get; set; }

            public Dictionary<string, string> Settings { get; set; } = [];
        }

        public static void SaveEncrypted(string filePath, string esn, Dictionary<string, string> settings, string password)
        {
            ConfigData data = new()
            {
                ESN = esn,
                SavedDate = DateTime.Now,
                Settings = new Dictionary<string, string>(settings)
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using Aes aes = Aes.Create();
            aes.KeySize = KeySize;

            using var deriveBytes = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(KeySize / 8);
            aes.GenerateIV();

            using MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                csEncrypt.Write(jsonBytes, 0, jsonBytes.Length);
                csEncrypt.FlushFinalBlock();
            }

            byte[] encrypted = msEncrypt.ToArray();
            byte[] checksum = SHA256.HashData(encrypted);

            using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(fs);

            writer.Write(Encoding.ASCII.GetBytes("OPENSKYCPGV1"));
            writer.Write((byte)1);
            writer.Write(checksum.Length);
            writer.Write(checksum);
            writer.Write(aes.IV.Length);
            writer.Write(aes.IV);
            writer.Write(encrypted.Length);
            writer.Write(encrypted);
        }

        public static ConfigData LoadEncrypted(string filePath, string password)
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(fs);

            byte[] header = reader.ReadBytes(12);
            if (Encoding.ASCII.GetString(header) != "OPENSKYCPGV1")
            {
                throw new InvalidDataException("Invalid file format.");
            }

            byte version = reader.ReadByte();
            if (version != 1)
            {
                throw new InvalidDataException($"Unsupported file version: {version}");
            }

            int checksumLength = reader.ReadInt32();
            byte[] storedChecksum = reader.ReadBytes(checksumLength);

            int ivLength = reader.ReadInt32();
            byte[] iv = reader.ReadBytes(ivLength);

            int encryptedLength = reader.ReadInt32();
            byte[] encrypted = reader.ReadBytes(encryptedLength);

            byte[] calculatedChecksum = SHA256.HashData(encrypted);
            if (!CompareBytes(storedChecksum, calculatedChecksum))
            {
                throw new InvalidDataException("Checksum verification failed. File may be corrupted or tampered with.");
            }

            using Aes aes = Aes.Create();
            aes.KeySize = KeySize;
            using var deriveBytes = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(KeySize / 8);
            aes.IV = iv;

            using MemoryStream msDecrypt = new();
            using (CryptoStream csDecrypt = new(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                csDecrypt.Write(encrypted, 0, encrypted.Length);
                csDecrypt.FlushFinalBlock();
            }

            byte[] decryptedBytes = msDecrypt.ToArray();
            string json = Encoding.UTF8.GetString(decryptedBytes);

            return JsonSerializer.Deserialize<ConfigData>(json)
                ?? throw new InvalidDataException("Failed to deserialize configuration data.");
        }

        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}