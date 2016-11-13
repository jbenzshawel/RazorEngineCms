/**
 * Page.js 
 * Description : Page class used for saving a new page in RazorEngineCms
 * Methods: init, validate, and save
 * Dependencies: jQuery, default.js, an initialized code mirror object 
 *               called pageTemplateEditor
 */

"use strict";

var Page = function () {
    this.init();
}

// initialize Page object properties 
Page.prototype.init = function () {
    this.Id = $("#pageId").text().trim();
    this.updated = $("#pageUpdated").text() != "" ? $("#pageUpdated").text() : new Date().toISOString();
    this.name = $("#pageName").val() != undefined ? $("#pageName").val().trim() : "";
    this.$name = $("#pageName");
    this.section = $("#pageVar").val() != undefined ? $("#pageVar").val().trim() : "";
    this.$section = $("#pageVar");
    this.$model = $("#model");
    this.$template = $("#template");
    this.createTemplateFile = $("input[name=\"templateFile\"]:checked").val();
    this.hasParams = $("input[name=\"hasParams\"]:checked").val();
    this.hasInclude = $("input[name=\"hasInclude\"]:checked").val();
    var hasModel = document.getElementById("hasModel-true");
    this.hasModel = hasModel != null ? hasModel.checked : false;
    if (typeof (pageTemplateEditor) != "undefined") {
        this.template = pageTemplateEditor.getValue().trim().replace("</script>", "<\/script>");
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
    if (this.hasModel != undefined && !this.hasModel) {
        this.model = null;
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
            HasParams: scopedObject.hasParams,
            HasInclude: scopedObject.hasInclude,
            Updated: scopedObject.updated
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
                if (data.Updated != null) {
                    $("#pageUpdated").text(data.Updated);
                }
            } else {
                _default.alertMsg("error", "Something went wrong. Try again?", "#new-page-alert")
                if (data.Errors.length > 0) {
                    var errorMsgBlock = [];
                    data.Errors.forEach(function (error) {
                        var lineNumRegex = /Line: \d?\d?\d?\d/g;
                        if (error.search(lineNumRegex) != -1) {
                            var lineNumText = error.match(lineNumRegex)[0];
                            var lineNum = parseInt(lineNumText.replace("Line:", ""), 10) - 1; // line numbers start at 0 in CodeMirror API
                            pageModelEditor.addLineClass(lineNum, "background", "line-error");
                            // add change event to remove line error
                            pageModelEditor.on('change', function (cMirror) {
                                pageModelEditor.removeLineClass(lineNum, "background", "line-error");
                            });
                        }
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
    return this.ajaxPost(id, section, name, msgSel, "Delete");
};

Page.prototype.copy = function (id, section, name, msgSel) {
    this.Id = id,
    this.section = section;
    this.name = name;
    return this.ajaxPost(id, section, name, msgSel, "Copy");
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
    var returnId = null;
    var scopedObj = this;
    var settings = {
        type: "POST",
        contentType: "application/json",
        url: "/CMS/Page/" + action,
        data: JSON.stringify(pageModel),
        async: false,
        success: function (data) {
            $(msgSel).empty(); // clear any previous messages
            if (data.Status === true) {
                var msgAction = "";
                if (action != null && action.toLowerCase() == "copy") {
                    msgAction = "copied";
                    returnId = data.newId; // for copy return new id of copied page
                } else if (action != null && action.toLowerCase() == "delete") {
                    msgAction = "deleted";
                }
                var successMsg = "The page /" + scopedObj.section + "/" + scopedObj.name + " has been " + msgAction + ".";
                if (msgSel != undefined && msgSel.length > 0) {
                    _default.alertMsg("success", successMsg, msgSel);
                }
                logger.logSuccess(successMsg);
            } else if (data.Errors.length > 0) {
                data.Errors.forEach(function (error) {
                    logger.logError(error);
                });
            }
        }
    };
    if (settings.data != null) {
        if (action.toLowerCase() === "copy") {
            $.ajax(settings);
        } else { // if delete don't need to return new id
           return $.ajax(settings);
        }
    }

    return returnId;
};

//// callback function for ajax request. If message success alert message displayed 
//// else errors are logged 
//Page.prototype.ajaxSuccessCallback = function (data, msgSel, action) {
   
//};

// hides save template as file option if page uses url params
Page.prototype.checkPageVariables = function () {
    var toggleTemplateFileOption = $("input[name='hasParams']:checked").val() === "true";
    if (toggleTemplateFileOption) {
        document.getElementById("templateFileTrue").checked = false;
        document.getElementById("templateFileFalse").checked = true;
        document.getElementsByName("templateFile").forEach(function (elem) {
            elem.setAttribute("disabled", "disabled");
        });
    } else {
        document.getElementsByName("templateFile").forEach(function (elem) {
            elem.removeAttribute("disabled");
        });
    }
};

Page.prototype.checkModelStatus = function () {
    this.init();
    if (this.hasModel) {
        // setup code mirror editor
        window.pageModelEditor = CodeMirror.fromTextArea(document.getElementById("model"), {
            lineNumbers: true,
            matchBrackets: true,
            mode: "text/x-csharp",
            theme: "material"
        });
        window.pageModelEditor.setSize("100%", "80%");
        window.setTimeout(("#pageModelSection").slideDown(), 250);
    } else {
        $("#pageModelSection").slideUp();
        // remove code mirror editor 
        var clearCodeMirror = function() {
            if (typeof(window.pageModelEditor) === "object")
                window.pageModelEditor.toTextArea();
        };
        // set timeout to prevent showing unstyled textbox before it is hidden
        window.setTimeout(clearCodeMirror, 500); 
    }
}