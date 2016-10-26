﻿"use strict";

var Include = function () {
    this.init();
}

// initialize Include object properties 
Include.prototype.init = function () {
    this.Id = $("#includeId").text().trim() || "",
    this.name = $("#includeName").val() != undefined ? $("#includeName").val().trim() : "";
    this.$name = $("#includeName");
    this.type = $("#includeType").val() != undefined ? $("#includeType").val() : "";
    this.$type = $("#includeType");
    this.$content = $("#includeContent");
    if (typeof (includeModelEditor) != "undefined") {
        this.content = includeModelEditor.getValue().trim();
    }
}

// validates new Include form inputs and returns true if valid 
Include.prototype.validate = function () {
    var isValid = true;
    if (this.name === null || this.name.length === 0) {
        this.$name.addError("Name cannot be empty", "name");
        isValid = false;
    } else if (this.name.indexOf(" ") > -1) {
        this.$name.add("Name cannot contain a space");
        isValid = false;
    }
    if (this.content === null || this.content.length === 0) {
        this.$content.addError("Content cannot be empty", "content");
        isValid = false;
    }
    return isValid;
}
// calls Include.validate() then saves Include using /Include/New post request
Include.prototype.save = function (async) {
    this.init();
    if (typeof (async) == "undefined") {
        async = true;
    }
    _default.clearErrors();
    if (this.validate()) {
        var scopedObject = this;
        // create Include object model for post request 
        var IncludeModel = {
            Name: scopedObject.name,
            Type: scopedObject.type,
            content: scopedObject.content
        };
        if (this.Id != "") {
            IncludeModel.Id = this.Id;
        }
        // callback for successful post request
        var successCallback = function (result) {
            var idReturn = null;
            $("#new-include-alert").remove();
            // create element to store alerts
            $("#newInclude").prepend("<div id=\"new-include-alert\"></div>");
            if (result.Status == true) {
                _default.alertMsg("success", "Include has been saved.", "#new-include-alert");
                if (result.Data.IncludeId != null) {
                    idReturn = result.Data.IncludeId; 
                }
            } else {
                _default.alertMsg("error", "Something went wrong. Try again?", "#new-include-alert")
                if (result.Errors.length > 0) {
                    var errorMsgBlock = [];
                    result.Errors.forEach(function (error) {
                        errorMsgBlock.push(error);
                        errorMsgBlock.push(("<br/>"));
                    });
                    $("#new-include-alert").append("<div class\"exception-msg\"><pre><h4>Exception:</h4>" + errorMsgBlock.join("") + "</pre></div>");
                } // end if data.Errors.length > 0 
            } // end else 
            $("html, body").animate({ scrollTop: 0 }, "slow");

            return idReturn;
        };
        // set params for ajax request 
        var settings = {
            type: "POST",
            contentType: "application/json",
            url: "/CMS/Include/Save",
            data: JSON.stringify(IncludeModel),
            success: successCallback,
            async: async
        };
        // submit post request 
        return $.ajax(settings);
    } // end if valid request 
    return;
};

Include.prototype.delete = function (id, section, name, msgSel) {
    this.Id = id,
    this.section = section;
    this.name = name;
    return this.ajaxPost(id, name, section, msgSel, "Delete");
};

Include.prototype.copy = function (id, section, name, msgSel) {
    this.Id = id,
    this.section = section;
    this.name = name;
    return this.ajaxPost(id, name, section, msgSel, "Copy");
}

Include.prototype.ajaxPost = function (id, section, name, msgSel, action) {
    if (id == undefined || id.length === 0) { // if empty get from Include object
        id = this.Id;
    }
    if (name == undefined || name.length === 0) { // if empty get from Include object
        name = this.name;
    }
    if (section == undefined || section.length === 0) { // if empty get from Include object
        section = this.section;
    }
    var IncludeModel = {
        Id: id,
        Section: section,
        Name: name
    };
    var scopedObj = this;
    var settings = {
        type: "POST",
        contentType: "application/json",
        url: "/CMS/Include/" + action,
        data: JSON.stringify(IncludeModel),
        async: false,
        success: function (data) {
            return scopedObj.ajaxSuccessCallback(data, msgSel, action);
        }
    };
    if (settings != null && settings.data != null) {
        return $.ajax(settings);
    }

    return false;
};

// callback function for ajax request. If message success alert message displayed 
// else errors are logged 
Include.prototype.ajaxSuccessCallback = function (data, msgSel, action) {
    var callbackReturnStatus = false;
    $(msgSel).empty(); // clear any previous messages
    if (data.Status === true) {
        var msgAction = "";
        if (action != null && action.toLowerCase() == "copy") {
            msgAction = "copied";
            callbackReturnStatus = data.newId; // for copy return new id of copied Include
        } else if (action != null && action.toLowerCase() == "delete") {
            msgAction = "deleted";
            callbackReturnStatus = true;
        }
        var successMsg = "The Include " + this.name + " has been " + msgAction + ".";
        if (msgSel != undefined && msgSel.length > 0) {
            _default.alertMsg("success", successMsg, msgSel);
        }
        logger.logSuccess(successMsg);
    } else if (data.Errors.length > 0) {
        data.Errors.forEach(function (error) {
            logger.logError(error);
        });
        callbackReturnStatus = false;
    }
    return callbackReturnStatus;
};