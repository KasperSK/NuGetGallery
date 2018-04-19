// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery.Security
{
    public sealed class OverwriteRequiredSignerPolicy : RequiredSignerPolicy
    {
        public const string PolicyName = nameof(OverwriteRequiredSignerPolicy);

        public OverwriteRequiredSignerPolicy()
            : base(PolicyName, SecurityPolicyAction.OverwriteRequiredSigner)
        {
        }
    }
}