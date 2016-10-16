/**
 * Page.js 
 * Description : Page class used for saving a new page in RazorEngineCms
 * Methods: init, validate, and save
 * Dependencies: jQuery, default.js, an initialized code mirror object 
 *               called pageTemplateEditor
 * LastUpdated : 9/18/2016
 */

"use strict";

var Page = function () {
    this.init();
}

// initialize Page object properties 
Page.prototype.init = function () {
    this.Id = $("#pageId").text().trim(),
    this.name = $("#pageName").val() != undefined ? $("#pageName").val().trim() : "";
    this.$name = $("#pageName");
    this.section = $("#pageVar").val() != undefined ? $("#pageVar").val().trim() : "";
    this.$section = $("#pageVar");
    this.$model = $("#model");
    this.$template = $("#template");
    this.createTemplateFile = $("input[name=\"templateFile\"]:checked").val();
    this.hasParams = $("input[name=\"hasParams\"]:checked").val();
    if (typeof (pageTemplateEditor) != "undefined") {
        this.template = pageTemplateEditor.getValue().trim();
    }
    if (typeof (pageModelEditor) != "undefined") {
        this.model = pageModelEditor.getValue().trim();
    }
}

// validates new page form inputs and returns true if valid 
Page.prototype.validate = function () {
    var isValid = true;
    if (this.name === null || this.name.length === 0) {
        this.$name.addError("Name cannot be empty", "name");
        isValid = false;
    } else if (this.name.indexOf(" ") > -1) {
        this.$name.add("Name cannot contain a space");
        isValid = false;
    }
    if (this.section != null && this.section.indexOf(" ") > -1) {
        this.$section.add("Section cannot contain a space");
        isValid = false;
    }
    if (this.template === null || this.template.length === 0) {
        this.$template.addError("Template cannot be empty", "template");
        isValid = false;
    }
    if (this.model != null && (this.model.indexOf("); DROP TABLE") > -1 || this.model.indexOf("DROP TABLE") > -1 || this.model.indexOf("DROP DATABASE") > -1)) {
        this.$model.addError("SQL injetion detected", "model");
        isValid = false;
    }
    if (this.model != null && (this.model.indexOf("[sys].") > -1 || this.model.indexOf("sys.") > -1)) {
        this.$model.addError("Sys SQL Object not accessible", "model");
        isValid = false;
    }
    return isValid;
}
// calls page.validate() then saves page using /Page/New post request
Page.prototype.save = function () {
    this.init();
    _default.clearErrors();
    if (this.validate()) {
        var scopedObject = this;
        // create page object model for post request 
        var pageModel = {
            Name: scopedObject.name,
            Section: scopedObject.section,
            Model: scopedObject.model,
            Template: scopedObject.template,
            CreateTemplateFile: scopedObject.createTemplateFile,
            HasParams: scopedObject.hasParams
        };
        if (this.Id != "") {
            pageModel.Id = this.Id;
        }
        // callback for successful post request
        var successCallback = function (data) {
            $("#new-page-alert").remove();
            // create element to store alerts
            $("#newPage").prepend("<div id=\"new-page-alert\"></div>");
            if (data.Status == true) {
                _default.alertMsg("success", "Page has been saved. <a href='/" + scopedObject.section + "/" + scopedObject.name + "' target='_blank'>View</a>", "#new-page-alert")
            } else {
                _default.alertMsg("error", "Something went wrong. Try again?", "#new-page-alert")

                if (data.Errors.length > 0) {
                    var errorMsgBlock = [];
                    data.Errors.forEach(function (error) {
                        //logger.logError(error);
                        errorMsgBlock.push(error);
                        errorMsgBlock.push(("<br/>"));
                    });
                    $("#new-page-alert").append("<div class\"exception-msg\"><pre><h4>Exception:</h4>" + errorMsgBlock.join("") + "</pre></div>");

                } // end if data.Errors.length > 0 
            } // end else 
            $("html, body").animate({ scrollTop: 0 }, "slow");
        };
        // set params for ajax request 
        var settings = {
            type: "POST",
            contentType: "application/json",
            url: "/CMS/Page/Save",
            data: JSON.stringify(pageModel),
            success: successCallback
        };
        // submit post request 
        $.ajax(settings);
    } // end if valid request 
    return;
};

Page.prototype.delete = function (id, section, name, msgSel) {
    this.Id = id,
    this.section = section;
    this.name = name;
    return this.ajaxPost(id, name, section, msgSel, "Delete");
};

Page.prototype.copy = function (id, section, name, msgSel) {
    this.Id = id,
    this.section = section;
    this.name = name;
    return this.ajaxPost(id, name, section, msgSel, "Copy");
}

Page.prototype.ajaxPost = function (id, section, name,  msgSel, action) {
    if (id == undefined || id.length === 0) { // if empty get from Page object
        id = this.Id;
    }
    if (name == undefined || name.length === 0) { // if empty get from Page object
        name = this.name;
    }
    if (section == undefined || section.length === 0) { // if empty get from Page object
        section = this.section;
    }

    var pageModel = {
        Id: id,
        Section: section,
        Name: name
    };
    var scopedObj = this;
    var settings = {
        type: "POST",
        contentType: "application/json",
        url: "/CMS/Page/" + action,
        data: JSON.stringify(pageModel),
        async: false,
        success: function (data) {
            return scopedObj.successCallback(data, msgSel, action);
        }
    };
    if (settings != null && settings.data != null) {
        return $.ajax(settings);
    }

    return false;
};

// callback function for ajax request. If message success alert message displayed 
// else errors are logged 
Page.prototype.successCallback = function (data, msgSel, action) {
    var callbackReturnStatus = false;
    $(msgSel).empty(); // clear any previous messages
    if (data.Status === true) {
        var msgAction = "";
        if (action != null && action.toLowerCase() == "copy") {
            msgAction = "copied";
            callbackReturnStatus = data.newId; // for copy return new id of copied page
        } else if (action != null && action.toLowerCase() == "delete") {
            msgAction = "deleted";
            callbackReturnStatus = true;
        }
        var successMsg = "The page /" + this.section + "/" + this.name + " has been " + msgAction + ".";
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