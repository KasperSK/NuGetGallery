// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGetGallery.Security;

namespace NuGetGallery
{
    public sealed class ListPackageItemRequiredSignerViewModel : ListPackageItemViewModel
    {
        private static readonly SignerViewModel AnySigner = 
            new SignerViewModel(id: null, username: null, displayText: "Any");

        public SignerViewModel RequiredSigner { get; set; }
        public string RequiredSignerMessage { get; set; }
        public IEnumerable<SignerViewModel> AllSigners { get; set; }
        public bool ShowRequiredSigner { get; set; }
        public bool CanEditRequiredSigner { get; set; }

        public ListPackageItemRequiredSignerViewModel(
            Package package,
            User currentUser,
            ISecurityPolicyService securityPolicyService)
            : base(package, currentUser)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            if (securityPolicyService == null)
            {
                throw new ArgumentNullException(nameof(securityPolicyService));
            }

            var requiredSigner = package.PackageRegistration?.RequiredSigners.FirstOrDefault();

            RequiredSigner = Convert(requiredSigner);

            var owners = package.PackageRegistration?.Owners ?? Enumerable.Empty<User>();

            if (HasSingleOwner || !owners.Any())
            {
                if (requiredSigner == null || requiredSigner == currentUser)
                {
                    AllSigners = Enumerable.Empty<SignerViewModel>();
                    ShowRequiredSigner = false;
                    CanEditRequiredSigner = false;
                }
                else if (owners.Contains(currentUser))
                {
                    AllSigners = new[] { RequiredSigner, Convert(currentUser) };
                    ShowRequiredSigner = true;
                    CanEditRequiredSigner = true;
                }
                else
                {
                    AllSigners = new[] { RequiredSigner };
                    ShowRequiredSigner = false;
                    CanEditRequiredSigner = false;
                }
            }
            else
            {
                var currentUserIsOwner = owners.Contains(currentUser);
                var currentUserHasControl = securityPolicyService.IsSubscribed(
                    currentUser,
                    ControlRequiredSignerPolicy.PolicyName);
                var noOwnerHasControl = !owners.Any(
                    owner => securityPolicyService.IsSubscribed(owner, ControlRequiredSignerPolicy.PolicyName));

                CanEditRequiredSigner = currentUserIsOwner && (currentUserHasControl || noOwnerHasControl);

                ShowRequiredSigner = true;

                if (CanEditRequiredSigner)
                {
                    AllSigners = new[] { AnySigner }.Concat(owners.Select(owner => Convert(owner)));
                }
                else
                {
                    AllSigners = new[] { RequiredSigner ?? AnySigner };

                    if (currentUserIsOwner)
                    {
                        var ownersWithRequiredSignerControl = owners.Where(
                            owner => securityPolicyService.IsSubscribed(owner, ControlRequiredSignerPolicy.PolicyName));

                        RequiredSignerMessage = GetRequiredSignerMessage(ownersWithRequiredSignerControl);
                    }
                }
            }
        }

        private static SignerViewModel Convert(User user)
        {
            if (user == null)
            {
                return null;
            }

            var certificatesCount = user.UserCertificates.Count(uc => uc.IsActive);
            var displayText = $"{user.Username} ({certificatesCount} certificate{(certificatesCount == 1 ? string.Empty : "s")})";

            return new SignerViewModel(user.Key, user.Username, displayText);
        }

        private static string GetRequiredSignerMessage(IEnumerable<User> users)
        {
            var count = users.Count();

            if (count == 0)
            {
                return null;
            }

            var builder = new StringBuilder();

            builder.AppendFormat("The signing owner is managed by the ");

            if (count == 1)
            {
                builder.Append($"'{users.Single().Username}' account.");
            }
            else if (count == 2)
            {
                builder.Append($"'{users.First().Username}' and '{users.Last().Username}' accounts.");
            }
            else
            {
                foreach (var user in users.Take(count - 1))
                {
                    builder.Append($"'{user.Username}', ");
                }

                builder.Append($"and '{users.Last().Username}' accounts.");
            }

            return builder.ToString();
        }
    }
}