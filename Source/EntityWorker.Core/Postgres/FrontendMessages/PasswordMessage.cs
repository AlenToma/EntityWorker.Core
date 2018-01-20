﻿#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EntityWorker.Core.Postgres.BackendMessages;

namespace EntityWorker.Core.Postgres.FrontendMessages
{
    class PasswordMessage : FrontendMessage
    {
        internal byte[] Payload { get; private set; }
        internal int PayloadOffset { get; private set; }
        internal int PayloadLength { get; private set; }

        const byte Code = (byte)'p';

        internal static PasswordMessage CreateClearText(string password)
        {
            var encoded = new byte[Encoding.UTF8.GetByteCount(password) + 1];
            Encoding.UTF8.GetBytes(password, 0, password.Length, encoded, 0);
            encoded[encoded.Length - 1] = 0;
            return new PasswordMessage(encoded);
        }

        /// <summary>
        /// Creates an MD5 password message.
        /// This is the password, hashed with the username as salt, and hashed again with the backend-provided
        /// salt.
        /// </summary>
        internal static PasswordMessage CreateMD5(string password, string username, byte[] serverSalt)
        {
            var md5 = MD5.Create();

            // First phase
            var passwordBytes = PGUtil.UTF8Encoding.GetBytes(password);
            var usernameBytes = PGUtil.UTF8Encoding.GetBytes(username);
            var cryptBuf = new byte[passwordBytes.Length + usernameBytes.Length];
            passwordBytes.CopyTo(cryptBuf, 0);
            usernameBytes.CopyTo(cryptBuf, passwordBytes.Length);

            var sb = new StringBuilder();
            var hashResult = md5.ComputeHash(cryptBuf);
            foreach (var b in hashResult)
                sb.Append(b.ToString("x2"));

            var prehash = sb.ToString();

            var prehashbytes = PGUtil.UTF8Encoding.GetBytes(prehash);
            cryptBuf = new byte[prehashbytes.Length + 4];

            Array.Copy(serverSalt, 0, cryptBuf, prehashbytes.Length, 4);

            // 2.
            prehashbytes.CopyTo(cryptBuf, 0);

            sb = new StringBuilder("md5");
            hashResult = md5.ComputeHash(cryptBuf);
            foreach (var b in hashResult)
                sb.Append(b.ToString("x2"));

            var resultString = sb.ToString();
            var result = new byte[Encoding.UTF8.GetByteCount(resultString) + 1];
            Encoding.UTF8.GetBytes(resultString, 0, resultString.Length, result, 0);
            result[result.Length - 1] = 0;

            return new PasswordMessage(result);
        }

        internal PasswordMessage() {}

        PasswordMessage(byte[] payload)
        {
            Payload = payload;
            PayloadOffset = 0;
            PayloadLength = payload.Length;
        }

        internal PasswordMessage Populate(byte[] payload, int offset, int count)
        {
            Payload = payload;
            PayloadOffset = offset;
            PayloadLength = count;
            return this;
        }

        internal override async Task Write(NpgsqlWriteBuffer buf, bool async)
        {
            if (buf.WriteSpaceLeft < 1 + 5)
                await buf.Flush(async);
            buf.WriteByte(Code);
            buf.WriteInt32(4 + PayloadLength);

            if (PayloadLength <= buf.WriteSpaceLeft)
            {
                // The entire array fits in our buffer, copy it into the buffer as usual.
                buf.WriteBytes(Payload, PayloadOffset, Payload.Length);
                return;
            }

            await buf.Flush(async);
            buf.DirectWrite(Payload, PayloadOffset, PayloadLength);
        }

        public override string ToString() =>  "[Password]";
    }
}
