﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Ninject;
using NuGetGallery.Configuration;

namespace NuGetGallery.Infrastructure
{
    /// <summary>
    /// Used by EF Migrations to load the Entity Context for migrations and such like.
    /// Don't use it for anything else because it doesn't respect read-only mode.
    /// </summary>
    public class EntitiesContextFactory : IDbContextFactory<EntitiesContext>
    {
        // Used by GalleryGateway
        internal static string OverrideConnectionString { get; set; }

        public EntitiesContext Create()
        {
            // readOnly: false - without read access, database migrations will fail and 
            // the whole site will be down (even when migrations are a no-op apparently).
            return new EntitiesContext(
                OverrideConnectionString ?? Container.Kernel.Get<IAppConfiguration>().SqlConnectionString,
                readOnly: false);
        }
    }
}