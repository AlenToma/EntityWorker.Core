﻿using System;

namespace EntityWorker.Core.LiteDB
{
    /// <summary>
    /// Set a name to this property in BsonDocument
    /// </summary>
    public class BsonFieldAttribute : Attribute
    {
        public string Name { get; set; }

        public BsonFieldAttribute(string name)
        {
            this.Name = name;
        }

        public BsonFieldAttribute()
        {
        }
    }
}