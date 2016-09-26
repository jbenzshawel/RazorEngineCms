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
    this.name = $("#pageName").val() != undefined ? $("#pageName").val().trim() : "";
    this.$name = $("#pageName");
    this.variable = $("#pageVar").val() != undefined ? $("#pageVar").val().trim() : "";
    this.$variable = $("#pageVar");
    this.$model = $("#model");
    this.$template = $("#template");
    this.createTemplateFile = $("input[name=\"templateFile\"]:checked").val();
    if (typeof (pageTemplateEditor) != "undefined") {
        this.template = pageTemplateEditor.getValue().trim();
    }
    if (typeof (pageModelEditor) != "undefined") {
        this.model = pageModelEditor.getValue().trim();
    }
}

// validates new page form inputs and returns true if valid 
Page.prototype.validate = function() {
    var isValid = true;
    if (this.name === null || this.name.length === 0) {
        this.$name.addError("Name cannot be empty", "name");
        isValid = false;
    } else if (this.name.indexOf(" ") > -1) {
        this.$name.add("Name cannot contain a space");
        isValid = false;
    }
    if (this.variable != null && this.variable.indexOf(" ") > -1) {
        this.$variable.add("Variable cannot contain a space");
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
            Variable: scopedObject.variable,
            Model: scopedObject.model,
            Template: scopedObject.template,
            CreateTemplateFile: scopedObject.createTemplateFile
        };
        // callback for successful post request
        var successCallback = function (data) {
            $("#new-page-alert").remove();
            // create element to store alerts
            $("#newPage").prepend("<div id=\"new-page-alert\"></div>");
            if (data.Status == true) {
                _default.alertMsg("success", "New page created. <a href='/Page/Preview/" + scopedObject.name + "/" + scopedObject.variable + "' target='_blank'>Preview</a>", "#new-page-alert")
            } else {
                _default.alertMsg("error", "Something went wrong. Try again?", "#new-page-alert")
                
                if (data.Errors.length > 0) {
                    var errorMsgBlock = [];
                    for (var i = 0, error; error = data.Errors[i++];) {
                        //logger.logError(error);
                        errorMsgBlock.push(error);
                        errorMsgBlock.push(("<br/>"));
                    }
                    $("#new-page-alert").append("<div class\"exception-msg\"><pre><h4>Exception:</h4>" + errorMsgBlock.join("") + "</pre></div>");

                } // end if data.Errors.length > 0 
            } // end else 
            $("html, body").animate({ scrollTop: 0 }, "slow");
        };
        // set params for ajax request 
        var settings = {
            type: "POST",
            contentType: "application/json",
            url: "/Page/Save",
            data: JSON.stringify(pageModel),
            success: successCallback
        };
        // submit post request 
        $.ajax(settings);
    } // end if valid request 
    return;
};