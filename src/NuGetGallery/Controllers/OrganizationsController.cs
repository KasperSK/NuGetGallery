﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NuGetGallery.Authentication;
using NuGetGallery.Filters;
using NuGetGallery.Security;

namespace NuGetGallery
{
    public class OrganizationsController
        : AccountsController<Organization, OrganizationAccountViewModel>
    {
        public OrganizationsController(
            AuthenticationService authService,
            ICuratedFeedService curatedFeedService,
            IMessageService messageService,
            IUserService userService,
            ITelemetryService telemetryService,
            ISecurityPolicyService securityPolicyService,
            ICertificateService certificateService)
            : base(
                  authService,
                  curatedFeedService,
                  messageService,
                  userService,
                  telemetryService,
                  securityPolicyService,
                  certificateService)
        {
        }

        public override string AccountAction => nameof(ManageOrganization);

        protected internal override ViewMessages Messages => new ViewMessages
        {
            EmailConfirmed = Strings.OrganizationEmailConfirmed,
            EmailPreferencesUpdated = Strings.OrganizationEmailPreferencesUpdated,
            EmailUpdateCancelled = Strings.OrganizationEmailUpdateCancelled
        };

        protected override void SendNewAccountEmail(User account)
        {
            var confirmationUrl = Url.ConfirmOrganizationEmail(account.Username, account.EmailConfirmationToken, relativeUrl: false);

            MessageService.SendNewAccountEmail(account, confirmationUrl);
        }

        protected override void SendEmailChangedConfirmationNotice(User account)
        {
            var confirmationUrl = Url.ConfirmOrganizationEmail(account.Username, account.EmailConfirmationToken, relativeUrl: false);
            MessageService.SendEmailChangeConfirmationNotice(account, confirmationUrl);
        }

        [HttpGet]
        [UIAuthorize]
        public ActionResult Add()
        {
            return View(new AddOrganizationViewModel());
        }

        [HttpPost]
        [UIAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(AddOrganizationViewModel model)
        {
            var organizationName = model.OrganizationName;
            var organizationEmailAddress = model.OrganizationEmailAddress;
            var adminUser = GetCurrentUser();

            try
            {
                var organization = await UserService.AddOrganizationAsync(organizationName, organizationEmailAddress, adminUser);
                SendNewAccountEmail(organization);
                TelemetryService.TrackOrganizationAdded(organization);
                return RedirectToAction(nameof(ManageOrganization), new { accountName = organization.Username });
            }
            catch (EntityException e)
            {
                TempData["AddOrganizationErrorMessage"] = e.Message;
                return View(model);
            }
        }

        [HttpGet]
        [UIAuthorize]
        public virtual ActionResult ManageOrganization(string accountName)
        {
            var account = GetAccount(accountName);

            return AccountView(account);
        }

        [HttpPost]
        [UIAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddMember(string accountName, string memberName, bool isAdmin)
        {
            var account = GetAccount(accountName);

            if (account == null
                || ActionsRequiringPermissions.ManageMembership.CheckPermissions(GetCurrentUser(), account)
                    != PermissionsCheckResult.Allowed)
            {
                return Json(HttpStatusCode.Forbidden, Strings.Unauthorized);
            }

            if (!account.Confirmed)
            {
                return Json(HttpStatusCode.BadRequest, Strings.Member_OrganizationUnconfirmed);
            }

            try
            {
                var request = await UserService.AddMembershipRequestAsync(account, memberName, isAdmin);
                var currentUser = GetCurrentUser();

                var profileUrl = Url.User(account, relativeUrl: false);
                var confirmUrl = Url.AcceptOrganizationMembershipRequest(request, relativeUrl: false);
                var rejectUrl = Url.RejectOrganizationMembershipRequest(request, relativeUrl: false);
                var cancelUrl = Url.CancelOrganizationMembershipRequest(memberName, relativeUrl: false);

                MessageService.SendOrganizationMembershipRequest(account, request.NewMember, currentUser, request.IsAdmin, profileUrl, confirmUrl, rejectUrl);
                MessageService.SendOrganizationMembershipRequestInitiatedNotice(account, currentUser, request.NewMember, request.IsAdmin, cancelUrl);

                return Json(new OrganizationMemberViewModel(request));
            }
            catch (EntityException e)
            {
                return Json(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpGet]
        [UIAuthorize]
        public async Task<ActionResult> ConfirmMemberRequest(string accountName, string confirmationToken)
        {
            var account = GetAccount(accountName);

            if (account == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            try
            {
                var member = await UserService.AddMemberAsync(account, GetCurrentUser().Username, confirmationToken);
                MessageService.SendOrganizationMemberUpdatedNotice(account, member);

                TempData["Message"] = String.Format(CultureInfo.CurrentCulture,
                    Strings.AddMember_Success, account.Username);

                return Redirect(Url.ManageMyOrganization(account.Username));
            }
            catch (EntityException e)
            {
                var failureReason = e.AsUserSafeException().GetUserSafeMessage();
                return HandleOrganizationMembershipRequestView(new HandleOrganizationMembershipRequestModel(true, account, failureReason));
            }
        }

        [HttpGet]
        [UIAuthorize]
        public async Task<ActionResult> RejectMemberRequest(string accountName, string confirmationToken)
        {
            var account = GetAccount(accountName);

            if (account == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            try
            {
                var member = GetCurrentUser();
                await UserService.RejectMembershipRequestAsync(account, member.Username, confirmationToken);
                MessageService.SendOrganizationMembershipRequestRejectedNotice(account, member);

                return HandleOrganizationMembershipRequestView(new HandleOrganizationMembershipRequestModel(false, account));
            }
            catch (EntityException e)
            {
                var failureReason = e.AsUserSafeException().GetUserSafeMessage();
                return HandleOrganizationMembershipRequestView(new HandleOrganizationMembershipRequestModel(false, account, failureReason));
            }
        }

        private ActionResult HandleOrganizationMembershipRequestView(HandleOrganizationMembershipRequestModel model)
        {
            return View("HandleOrganizationMembershipRequest", model);
        }

        [HttpPost]
        [UIAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CancelMemberRequest(string accountName, string memberName)
        {
            var account = GetAccount(accountName);

            if (account == null
                || ActionsRequiringPermissions.ManageMembership.CheckPermissions(GetCurrentUser(), account)
                    != PermissionsCheckResult.Allowed)
            {
                return Json(HttpStatusCode.Forbidden, Strings.Unauthorized);
            }

            try
            {
                var removedUser = await UserService.CancelMembershipRequestAsync(account, memberName);
                MessageService.SendOrganizationMembershipRequestCancelledNotice(account, removedUser);
                return Json(Strings.CancelMemberRequest_Success);
            }
            catch (EntityException e)
            {
                return Json(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpPost]
        [UIAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateMember(string accountName, string memberName, bool isAdmin)
        {
            var account = GetAccount(accountName);

            if (account == null
                || ActionsRequiringPermissions.ManageMembership.CheckPermissions(GetCurrentUser(), account)
                    != PermissionsCheckResult.Allowed)
            {
                return Json(HttpStatusCode.Forbidden, Strings.Unauthorized);
            }

            if (!account.Confirmed)
            {
                return Json(HttpStatusCode.BadRequest, Strings.Member_OrganizationUnconfirmed);
            }

            try
            {
                var membership = await UserService.UpdateMemberAsync(account, memberName, isAdmin);
                MessageService.SendOrganizationMemberUpdatedNotice(account, membership);

                return Json(new OrganizationMemberViewModel(membership));
            }
            catch (EntityException e)
            {
                return Json(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpPost]
        [UIAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteMember(string accountName, string memberName)
        {
            var account = GetAccount(accountName);

            var currentUser = GetCurrentUser();

            if (account == null ||
                (currentUser.Username != memberName &&
                ActionsRequiringPermissions.ManageMembership.CheckPermissions(currentUser, account)
                    != PermissionsCheckResult.Allowed))
            {
                return Json(HttpStatusCode.Forbidden, Strings.Unauthorized);
            }

            if (!account.Confirmed)
            {
                return Json(HttpStatusCode.BadRequest, Strings.Member_OrganizationUnconfirmed);
            }

            try
            {
                var removedMember = await UserService.DeleteMemberAsync(account, memberName);
                MessageService.SendOrganizationMemberRemovedNotice(account, removedMember);
                return Json(Strings.DeleteMember_Success);
            }
            catch (EntityException e)
            {
                return Json(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [UIAuthorize]
        [ValidateAjaxAntiForgeryToken(HttpVerbs.Post)]
        [RequiresAccountConfirmation("add or get certificates")]
        public virtual async Task<JsonResult> AddOrGetCertificates(string accountName, HttpPostedFileBase uploadFile)
        {
            var currentUser = GetCurrentUser();
            var organization = GetAccount(accountName);

            if (currentUser == null || currentUser.IsDeleted ||
                organization == null || organization.IsDeleted)
            {
                return Json(HttpStatusCode.Unauthorized, obj: null);
            }

            if (Request.Is(HttpVerbs.Post))
            {
                if (organization == null ||
                    ActionsRequiringPermissions.ManageCertificate.CheckPermissions(currentUser, organization) != PermissionsCheckResult.Allowed)
                {
                    return Json(HttpStatusCode.Forbidden, Strings.Unauthorized);
                }

                return await AddCertificateAsync(uploadFile, organization);
            }

            var template = Url.OrganizationCertificateTemplate(accountName);

            return GetCertificates(organization, template);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Delete)]
        [UIAuthorize]
        [ValidateAjaxAntiForgeryToken(HttpVerbs.Delete)]
        [RequiresAccountConfirmation("get or remove a certificate")]
        public virtual async Task<JsonResult> GetOrRemoveCertificate(string accountName, string thumbprint)
        {
            var currentUser = GetCurrentUser();
            var organization = GetAccount(accountName);

            if (Request.Is(HttpVerbs.Delete))
            {
                if (organization == null ||
                    ActionsRequiringPermissions.ManageCertificate.CheckPermissions(currentUser, organization) != PermissionsCheckResult.Allowed)
                {
                    return Json(HttpStatusCode.Forbidden, Strings.Unauthorized);
                }

                await CertificateService.DeactivateCertificateAsync(thumbprint, organization);

                return Json(HttpStatusCode.OK, new { });
            }

            var template = Url.OrganizationCertificateTemplate(accountName);

            return GetCertificate(thumbprint, organization, template);
        }

        protected override void UpdateAccountViewModel(Organization account, OrganizationAccountViewModel model)
        {
            base.UpdateAccountViewModel(account, model);

            model.Members =
                account.Members.Select(m => new OrganizationMemberViewModel(m))
                .Concat(account.MemberRequests.Select(m => new OrganizationMemberViewModel(m)));

            model.RequiresTenant = account.IsRestrictedToOrganizationTenantPolicy();

            model.CanManageMemberships = 
                ActionsRequiringPermissions.ManageMembership.CheckPermissions(GetCurrentUser(), account) == 
                PermissionsCheckResult.Allowed;
        }
    }
}