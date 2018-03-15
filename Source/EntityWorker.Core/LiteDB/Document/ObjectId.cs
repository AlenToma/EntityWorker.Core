﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;

namespace EntityWorker.Core.LiteDB
{
    /// <summary>
    /// Represent a 12-bytes BSON type used in document Id
    /// </summary>
    public class ObjectId : IComparable<ObjectId>, IEquatable<ObjectId>
    {
        /// <summary>
        /// A zero 12-bytes ObjectId
        /// </summary>
        public static readonly ObjectId Empty = new ObjectId();

        #region Properties

        /// <summary>
        /// Get timestamp
        /// </summary>
        public int Timestamp { get; private set; }

        /// <summary>
        /// Get machine number
        /// </summary>
        public int Machine { get; private set; }

        /// <summary>
        /// Get pid number
        /// </summary>
        public short Pid { get; private set; }

        /// <summary>
        /// Get increment
        /// </summary>
        public int Increment { get; private set; }

        /// <summary>
        /// Get creation time
        /// </summary>
        public DateTime CreationTime
        {
            get { return BsonValue.UnixEpoch.AddSeconds(this.Timestamp); }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new empty instance of the ObjectId class.
        /// </summary>
        public ObjectId()
        {
            this.Timestamp = 0;
            this.Machine = 0;
            this.Pid = 0;
            this.Increment = 0;
        }

        /// <summary>
        /// Initializes a new instance of the ObjectId class from ObjectId vars.
        /// </summary>
        public ObjectId(int timestamp, int machine, short pid, int increment)
        {
            this.Timestamp = timestamp;
            this.Machine = machine;
            this.Pid = pid;
            this.Increment = increment;
        }

        /// <summary>
        /// Initializes a new instance of ObjectId class from another ObjectId.
        /// </summary>
        public ObjectId(ObjectId from)
        {
            this.Timestamp = from.Timestamp;
            this.Machine = from.Machine;
            this.Pid = from.Pid;
            this.Increment = from.Increment;
        }

        /// <summary>
        /// Initializes a new instance of the ObjectId class from hex string.
        /// </summary>
        public ObjectId(string value)
            : this(FromHex(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObjectId class from byte array.
        /// </summary>
        public ObjectId(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length != 12) throw new ArgumentException(nameof(bytes), "Byte array must be 12 bytes long");

            this.Timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            this.Machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
            this.Pid = (short)((bytes[7] << 8) + bytes[8]);
            this.Increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
        }

        /// <summary>
        /// Convert hex value string in byte array
        /// </summary>
        private static byte[] FromHex(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            if (value.Length != 24) throw new ArgumentException(string.Format("ObjectId strings should be 24 hex characters, got {0} : \"{1}\"", value.Length, value));

            var bytes = new byte[12];

            for (var i = 0; i < 24; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            }

            return bytes;
        }

        #endregion

        #region Equals/CompareTo/ToString

        /// <summary>
        /// Checks if this ObjectId is equal to the given object. Returns true
        /// if the given object is equal to the value of this instance. 
        /// Returns false otherwise.
        /// </summary>
        public bool Equals(ObjectId other)
        {
            return other != null && 
                this.Timestamp == other.Timestamp &&
                this.Machine == other.Machine &&
                this.Pid == other.Pid &&
                this.Increment == other.Increment;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        public override bool Equals(object other)
        {
            return Equals(other as ObjectId);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + this.Timestamp.GetHashCode();
            hash = 37 * hash + this.Machine.GetHashCode();
            hash = 37 * hash + this.Pid.GetHashCode();
            hash = 37 * hash + this.Increment.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Compares two instances of ObjectId
        /// </summary>
        public int CompareTo(ObjectId other)
        {
            var r = this.Timestamp.CompareTo(other.Timestamp);
            if (r != 0) return r;

            r = this.Machine.CompareTo(other.Machine);
            if (r != 0) return r;

            r = this.Pid.CompareTo(other.Pid);
            if (r != 0) return r < 0 ? -1 : 1;

            return this.Increment.CompareTo(other.Increment);
        }

        /// <summary>
        /// Represent ObjectId as 12 bytes array
        /// </summary>
        public byte[] ToByteArray()
        {
            var bytes = new byte[12];

            bytes[0] = (byte)(this.Timestamp >> 24);
            bytes[1] = (byte)(this.Timestamp >> 16);
            bytes[2] = (byte)(this.Timestamp >> 8);
            bytes[3] = (byte)(this.Timestamp);
            bytes[4] = (byte)(this.Machine >> 16);
            bytes[5] = (byte)(this.Machine >> 8);
            bytes[6] = (byte)(this.Machine);
            bytes[7] = (byte)(this.Pid >> 8);
            bytes[8] = (byte)(this.Pid);
            bytes[9] = (byte)(this.Increment >> 16);
            bytes[10] = (byte)(this.Increment >> 8);
            bytes[11] = (byte)(this.Increment);

            return bytes;
        }

        public override string ToString()
        {
            return BitConverter.ToString(this.ToByteArray()).Replace("-", "").ToLower();
        }

        #endregion

        #region Operators

        public static bool operator ==(ObjectId lhs, ObjectId rhs)
        {
            if (object.ReferenceEquals(lhs, null)) return object.ReferenceEquals(rhs, null);
            if (object.ReferenceEquals(rhs, null)) return false; // don't check type because sometimes different types can be ==

            return lhs.Equals(rhs);
        }

        public static bool operator !=(ObjectId lhs, ObjectId rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >=(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        public static bool operator >(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator <(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator <=(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        #endregion

        #region Static methods

        private static int _machine;
        private static short _pid;
        private static int _increment;

        // static constructor
        static ObjectId()
        {
            _machine = (GetMachineHash() +
#if HAVE_APP_DOMAIN
                AppDomain.CurrentDomain.Id
#else
                10000 // Magic number
#endif   
                ) & 0x00ffffff;
            _increment = (new Random()).Next();

            try
            {
                _pid = (short)GetCurrentProcessId();
            }
            catch (SecurityException)
            {
                _pid = 0;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
#if HAVE_PROCESS
            return Process.GetCurrentProcess().Id;
#else
            return 1000; // Magic number
#endif
        }

        private static int GetMachineHash()
        {
            var hostName =
#if HAVE_ENVIRONMENT
                Environment.MachineName; // use instead of Dns.HostName so it will work offline
#else
                "SOMENAME";
#endif
            return 0x00ffffff & hostName.GetHashCode(); // use first 3 bytes of hash
        }

        /// <summary>
        /// Creates a new ObjectId.
        /// </summary>
        public static ObjectId NewObjectId()
        {
            var timestamp = (long)Math.Floor((DateTime.UtcNow - BsonValue.UnixEpoch).TotalSeconds);
            var inc = Interlocked.Increment(ref _increment) & 0x00ffffff;

            return new ObjectId((int)timestamp, _machine, _pid, inc);
        }

        #endregion
    }
}