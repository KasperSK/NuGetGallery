var CertificatesManagement = (function () {
    'use strict';

    var _certificatesUrl;
    var _model;

    return new function () {
        this.init = function (certificatesUrl) {
            _certificatesUrl = certificatesUrl;

            $('#input-select-file').on('change', function () {
                clearErrors();

                var filePath = $('#input-select-file').val();

                if (filePath) {
                    var uploadForm = $('#uploadCertificateForm')[0];
                    var formData = new FormData(uploadForm);

                    uploadForm.reset();

                    addCertificateAsync(formData);
                }
            });

            listCertificatesAsync();
        }

        function listCertificatesAsync() {
            $.ajax({
                method: 'GET',
                url: _certificatesUrl,
                dataType: 'json',
                cache: false,
                contentType: false,
                processData: false,
                success: function (response) {
                    applyModel(response);
                },
                error: onError.bind(this)
            });
        }

        function deactivateCertificateAsync(model) {
            clearErrors();

            if (model.CanDeactivate) {
                $.ajax({
                    method: 'DELETE',
                    url: model.DeactivateUrl,
                    headers: getHeaders(),
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (response) {
                        listCertificatesAsync();
                    },
                    error: onError.bind(this)
                });
            }
        }

        function addCertificateAsync(data) {
            clearErrors();

            $.ajax({
                method: 'POST',
                url: _certificatesUrl,
                data: data,
                headers: getHeaders(),
                cache: false,
                contentType: false,
                processData: false,
                complete: function (xhr, textStatus) {
                    switch (xhr.status) {
                        case 201:
                        case 409:
                            listCertificatesAsync();
                            break;

                        default:
                            onError(xhr, textStatus);
                            break;
                    }
                }
            });
        }

        function applyModel(data) {
            if (_model) {
                _model.certificates(data);
            } else {
                _model = {
                    certificates: ko.observableArray(data),
                    deactivate: deactivateCertificateAsync,
                    hasCertificates: function () {
                        return this.certificates().length > 0;
                    }
                };

                ko.applyBindings(_model, document.getElementById('certificates-container'));
            }

            var certificatesHeader;

            if (data) {
                certificatesHeader = data.length + " certificate";

                if (data.length !== 1) {
                    certificatesHeader += "s";
                }
            } else {
                certificatesHeader = "";
            }

            $('#certificates-section-header').text(certificatesHeader);
        }

        function getHeaders() {
            var headers = {};

            window.nuget.addAjaxAntiForgeryToken(headers);

            return headers;
        }

        function onError(model, resultCodeString, fullResponse) {
            switch (resultCodeString) {
                case "timeout":
                    displayErrors(["The operation timed out. Please try again."]);
                    break;
                case "abort":
                    displayErrors(["The operation was aborted. Please try again."]);
                    break;
                default:
                    displayErrors(model.responseJSON);
                    break;
            }

            if (fullResponse.status >= 500) {
                displayErrors(["There was a server error."]);
            }
        }

        function displayErrors(errors) {
            if (errors == null || errors.length === 0) {
                return;
            }

            clearErrors();

            var failureContainer = $("#validation-failure-container");
            var failureListContainer = document.createElement("div");
            $(failureListContainer).attr("id", "validation-failure-list");
            $(failureListContainer).attr("data-bind", "template: { name: 'validation-errors', data: data }");
            failureContainer.append(failureListContainer);
            ko.applyBindings({ data: errors }, failureListContainer);
            failureContainer.removeClass("hidden");
        }

        function clearErrors() {
            $("#validation-failure-container").addClass("hidden");
            $("#validation-failure-list").remove();

            var warnings = $('#warning-container');
            warnings.addClass("hidden");
            warnings.children().remove();
        }
    };
}());