// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery
{
    public sealed class SignerViewModel
    {
        public int? Id { get; }
        public string Username { get; }
        public string DisplayText { get; }

        public SignerViewModel(int? id, string username, string displayText)
        {
            Id = id;
            Username = username;
            DisplayText = displayText;
        }
    }
}