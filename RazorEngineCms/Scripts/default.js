/**
 * Created by addison on 7/31/16.
 */
"use strict";

// logger object to handle errors 
var logger = {};
logger.logError = function (error) {
    console.log("%cError: " + error, "color:red");
    return;
};
logger.logMessage = function (msg) {
    console.log(msg);
    return;
};
logger.logSuccess = function (msg) {
    console.log("%cSuccess: " + msg, "color:green");
    return;
}

// create default object to store default functions
var _default = {};

// @param email = email to validate
// @return bool
_default.validateEmail = function (email) {
    var regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return regex.test(email);
};

// @param password = string to validate id = id of input field
// @return bool (adds error message if false)
_default.validatePasswordLength = function (password, id) {
    if (password.trim() == "" || password.trim().length < 5) {
        $(id).addError(ErrorMessages.PasswordLength);
        return false;
    }
    return true;
};

// clears all error messages on a page. If a form id is
// passed the form fields will also be cleared
// @param formid = id of form (optional)
// @return void
_default.clearErrors = function (formId) {
    $(".input-error").removeClass("input-error");
    $(".error-message").remove();
    if (formId != undefined) {
        var form = document.getElementById(formId);
        form.reset();
    }
};

// clears input errors and values in a modal form
// @param modal = id of modal
// @return void
_default.clearModalErrors = function (modal) {
    $(modal).on('hidden.bs.modal', function () {
        var id = $(this).find('form').attr('id');
        _default.clearErrors(id);
    });
};

// Clears input errors on change
// @param field = id of input
// @return void
_default.updateInputField = function (field) {
    $(field).change(function () {
        if (this.value != "") {
            $(this).removeClass("input-error");
            $("." + field.substring(1) + ".error-message").remove();
        }
    });
};

// function for ajax post request
// @params settings = object for ajax, async = bool
// @return bool
_default.post = function (settings, async) {
    if (async == undefined) async = true;
    if (typeof (settings) === 'object') {
        settings.type = 'POST';
        settings.async = async;
        $.ajax(settings);
        return true;
    }
    return false;
};

_default.get = function (settings, async) {
    if (async == undefined) async = true;
    if (typeof (settings) === 'object') {
        settings.type = 'GET';
        settings.async = async;
        $.ajax(settings);
        return true;
    }
    return false;
}

// adds a dismissable alert to an element
// @params type = name of what is being updated, action = update or delete, msgId = id of element to update
_default.alertMsg = function (action, msg, msgId) {
    var errorMsg = '<div class="alert alert-danger alert-dismissible" role="alert">' +
        '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
        '<strong>Error!</strong> ' + msg
    '</div>';
    var successMsg = '<div class="alert alert-success alert-dismissible" role="alert">' +
        '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
        '<strong>Success!</strong> ' + msg
    '</div>';
    if (action == 'error') $(msgId).html(errorMsg);
    if (action == 'success') $(msgId).html(successMsg);
}

// jquery functions
// $(target).addError
// @params errorMsg = message to add, field = name of field (used for targeting with _default.updateInputField)
$.fn.addError = function (errorMsg, field, altId) {
    if (field == undefined) field = "";
    var targetId = this;
    if (altId != undefined) {
        targetId = altId;
    }
    $(targetId).find('.error-message').remove();
    $(targetId).after("<div class=\"" + field + " error-message text-danger\">" + errorMsg + "</div>");
    $(targetId).addClass("input-error");
};