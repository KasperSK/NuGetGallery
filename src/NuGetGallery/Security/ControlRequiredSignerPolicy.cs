// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery.Security
{
    public sealed class ControlRequiredSignerPolicy : RequiredSignerPolicy
    {
        public const string PolicyName = nameof(ControlRequiredSignerPolicy);

        public ControlRequiredSignerPolicy()
            : base(PolicyName, SecurityPolicyAction.ControlRequiredSigner)
        {
        }
    }
}