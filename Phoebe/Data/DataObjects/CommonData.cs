﻿using System;

namespace Toggl.Phoebe.Data.DataObjects
{
    public abstract class CommonData
    {
        protected CommonData ()
        {
            ModifiedAt = Time.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Toggl.Phoebe.Data.DataObjects.CommonData"/> class copying
        /// the data from the other object.
        /// </summary>
        /// <param name="other">Instance to copy data from.</param>
        protected CommonData (CommonData other)
        {
            Id = other.Id;
            ModifiedAt = other.ModifiedAt;
            DeletedAt = other.DeletedAt;
            IsDirty = other.IsDirty;
            RemoteId = other.RemoteId;
            RemoteRejected = other.RemoteRejected;
        }

        [DontDirty]
        [SQLite.PrimaryKey]
        public Guid Id { get; set; }

        public DateTime ModifiedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        [DontDirty]
        public bool IsDirty { get; set; }

        [DontDirty]
        [SQLite.Unique]
        public long? RemoteId { get; set; }

        [DontDirty]
        public bool RemoteRejected { get; set; }
    }
}
