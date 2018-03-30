﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGetGallery
{
    public class DeleteOrganizationViewModel
    {
        private Lazy<bool> _hasOrphanPackages;

        public DeleteOrganizationViewModel()
        {
            _hasOrphanPackages = new Lazy<bool>(() => Packages.Any(p => p.HasSingleOrganizationOwner));
        }

        public List<ListPackageItemViewModel> Packages { get; set; }

        public Organization Organization { get; set; }

        public string AccountName { get; set; }

        public bool HasOrphanPackages
        {
            get
            {
                return Packages == null ? false : _hasOrphanPackages.Value;
            }
        }
    }
}