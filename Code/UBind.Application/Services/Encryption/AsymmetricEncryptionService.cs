// <copyright file="AsymmetricEncryptionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Encryption
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;
    using NodaTime;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Encodings;
    using Org.BouncyCastle.Crypto.Engines;
    using Org.BouncyCastle.OpenSsl;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    public class AsymmetricEncryptionService : IAsymmetricEncryptionService
    {
        private readonly string cipherKey;
        private readonly string encryptionKey;
        private readonly IClock clock;

        public AsymmetricEncryptionService(
            IEncryptionConfiguration encryptionConfig,
            IClock clock)
        {
            this.encryptionKey = encryptionConfig.RsaPublicKey;
            this.cipherKey = encryptionConfig.RsaPrivateKey;
            this.clock = clock;
        }

        public string Encrypt(string clearText)
        {
            if (clearText.IsNullOrEmpty())
            {
                throw new ArgumentException("Input is missing.");
            }

            byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            using (var textReader = new StringReader(this.encryptionKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(textReader).ReadObject();
                encryptEngine.Init(true, keyParameter);
            }

            var cipher = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return cipher;
        }

        public string Decrypt(string cipherText)
        {
            if (cipherText.IsNullOrEmpty())
            {
                throw new ArgumentException("Input is missing.");
            }

            byte[] cipherTextAsByteArray = Convert.FromBase64String(cipherText);
            var csp = new RSACryptoServiceProvider();

            var cleanCipher = Regex.Unescape(this.cipherKey);
            var stringBuilder = new StringReader(cleanCipher);
            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            csp.ImportParameters((RSAParameters)xmlSerializer.Deserialize(stringBuilder));

            byte[] plainTextDataAsBytes = csp.Decrypt(cipherTextAsByteArray, false);
            string plainText = Encoding.UTF8.GetString(plainTextDataAsBytes);

            this.ThrowIfTimestampMissingOrExpired(plainText);
            return this.StripTimestamp(plainText);
        }

        public dynamic GenerateRSAKeys()
        {
            var csp = new RSACryptoServiceProvider(2048);
            string publicKey = this.ExportPublicKey(csp);

            string privateKey;
            {
                var sw = new StringWriter();
                var xs = new XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, csp.ExportParameters(true));
                privateKey = sw.ToString();
            }

            return new { PublicKey = publicKey, PrivateKey = privateKey };
        }

        private string ExportPublicKey(RSACryptoServiceProvider csp)
        {
            StringWriter outputStream = new StringWriter();
            var parameters = csp.ExportParameters(false);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    innerWriter.Write((byte)0x30); // SEQUENCE
                    this.EncodeLength(innerWriter, 13);
                    innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
                    var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
                    this.EncodeLength(innerWriter, rsaEncryptionOid.Length);
                    innerWriter.Write(rsaEncryptionOid);
                    innerWriter.Write((byte)0x05); // NULL
                    this.EncodeLength(innerWriter, 0);
                    innerWriter.Write((byte)0x03); // BIT STRING
                    using (var bitStringStream = new MemoryStream())
                    {
                        var bitStringWriter = new BinaryWriter(bitStringStream);
                        bitStringWriter.Write((byte)0x00); // # of unused bits
                        bitStringWriter.Write((byte)0x30); // SEQUENCE
                        using (var paramsStream = new MemoryStream())
                        {
                            var paramsWriter = new BinaryWriter(paramsStream);
                            this.EncodeIntegerBigEndian(paramsWriter, parameters.Modulus); // Modulus
                            this.EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
                            var paramsLength = (int)paramsStream.Length;
                            this.EncodeLength(bitStringWriter, paramsLength);
                            bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
                        }

                        var bitStringLength = (int)bitStringStream.Length;
                        this.EncodeLength(innerWriter, bitStringLength);
                        innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
                    }

                    var length = (int)innerStream.Length;
                    this.EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();

                // WriteLine terminates with \r\n, we want only \n
                outputStream.Write("-----BEGIN PUBLIC KEY-----\n");
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.Write(base64, i, Math.Min(64, base64.Length - i));
                    outputStream.Write("\n");
                }

                outputStream.Write("-----END PUBLIC KEY-----");
            }

            return outputStream.ToString();
        }

        private void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            }

            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }

                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0)
                {
                    break;
                }

                prefixZeros++;
            }

            if (value.Length - prefixZeros == 0)
            {
                this.EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    this.EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    this.EncodeLength(stream, value.Length - prefixZeros);
                }

                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }

        private void ThrowIfTimestampMissingOrExpired(string plainText)
        {
            if (plainText.IsNullOrEmpty())
            {
                return;
            }

            var verificationTag = plainText.Substring(plainText.LastIndexOf('|') + 1);
            if (verificationTag.IsNullOrEmpty() || verificationTag.IsNullOrWhitespace())
            {
                throw new ErrorException(
                    Errors.Payment.TokenisationFailure(new List<string> { "Validation failed for data token. Data not found." }));
            }

            var parseResult = DateTime.TryParseExact(
                verificationTag, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime verificationStamp);
            if (!parseResult)
            {
                throw new ErrorException(
                    Errors.Payment.TokenisationFailure(new List<string> { "Validation failed for data token. Invalid format." }));
            }
            else
            {
                var currentTimestamp = this.clock.Now().ToDateTimeUtc();
                var difference = currentTimestamp.Subtract(verificationStamp);
                if (difference.Minutes > 2)
                {
                    throw new ErrorException(
                        Errors.Payment.TokenisationFailure(new List<string> { "Validation failed for data token. Request expired." }));
                }
            }
        }

        private string StripTimestamp(string plainText)
        {
            if (plainText.IsNotNullOrEmpty())
            {
                return plainText.Substring(0, plainText.IndexOf('|'));
            }

            return plainText;
        }
    }
}
