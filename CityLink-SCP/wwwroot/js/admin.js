/*
    Replaced named function with 'EventListeners', apparently its better to keep the html and js separate
    Where before in the <button> it had 'onclick="LoadXML()"', this instead uses JQuery to do this:
        document.querySelector('button').addEventListener('click', doSomething); 
    Which is better for "Separatoin of Concerns", html doesnt have js function names in it, which may change
*/

$(function () {

    //  Overlay helpers 

    function openPanel(overlayId) {
        $("#" + overlayId).addClass("is-open").attr("aria-hidden", "false");
        $("body").css("overflow", "hidden");
    }

    function closePanel(overlayId) {
        $("#" + overlayId).removeClass("is-open").attr("aria-hidden", "true");
        $("body").css("overflow", "");
    }

    // Close on backdrop click (clicking the overlay itself, not the panel inside)
    $(".xml-overlay").on("click", function (e) {
        if ($(e.target).is(".xml-overlay")) {
            closePanel($(this).attr("id"));
        }
    });

    $("#closeXmlPanel").on("click", function () {
        closePanel("xml-editor-overlay");
    });

    $("#closePreviewPanel").on("click", function () {
        closePanel("xml-preview-overlay");
    });


    //  Status banner 

    var statusTimer;

    function showStatus(message, type) {
        type = type || "success";
        clearTimeout(statusTimer);
        $("#xml-status")
            .removeClass("xml-status--success xml-status--error xml-status--info")
            .addClass("xml-status--" + type)
            .text(message)
            .show();
        statusTimer = setTimeout(function () {
            $("#xml-status").fadeOut(300);
        }, 4000);
    }


    //  New Config button 

    $("#newConfigBtn").on("click", function () {
        $("#xmlConfigId").val("");
        $("#xmlTypeSelect").val("");
        $("#xmlLabelInput").val("");
        $("#xmlEditText").val("");
        $("#xml-panel-title").text("New Configuration");
        $("#xml-status").hide();
        openPanel("xml-editor-overlay");
    });


    //  Edit buttons (XML configs table) 

    $(document).on("click", ".edit-config-btn", function () {
        var $btn = $(this);
        var id = $btn.data("id");
        var type = $btn.data("type");
        var label = $btn.data("label") || "";

        $("#xmlConfigId").val(id);
        $("#xmlTypeSelect").val(type);
        $("#xmlLabelInput").val(label);
        $("#xml-panel-title").text("Edit — " + type);
        $("#xml-status").hide();
        $("#xmlEditText").val("Loading…");

        openPanel("xml-editor-overlay");

        $.ajax({
            url: "/Admin/LoadXml",
            type: "GET",
            data: { id: id },
            dataType: "text",
            success: function (xml) {
                $("#xmlEditText").val(xml);
            },
            error: function () {
                $("#xmlEditText").val("");
                showStatus("Could not load XML for this record.", "error");
            }
        });
    });


    //  Load Template 

    $("#loadTemplateBtn").on("click", function () {
        var type = $("#xmlTypeSelect").val();
        if (!type) {
            showStatus("Select a config type first.", "error");
            return;
        }

        $.ajax({
            url: "/Admin/LoadTemplate",
            type: "GET",
            data: { typeName: type },
            dataType: "text",
            success: function (xml) {
                $("#xmlEditText").val(xml);
                showStatus("Template loaded. Edit and save when ready.", "info");
            },
            error: function (xhr) {
                showStatus(xhr.responseText || "Template unavailable.", "error");
            }
        });
    });


    //  Preview (from inside the editor) 

    $("#previewXmlBtn").on("click", function () {
        var id = $("#xmlConfigId").val();
        if (!id) {
            showStatus("Save the config first to preview it, or click Preview on an existing row.", "info");
            return;
        }
        loadPreview(id);
    });

    // Preview buttons in the XML configs table
    $(document).on("click", ".preview-config-btn", function () {
        loadPreview($(this).data("id"));
    });

    function loadPreview(id) {
        $.ajax({
            url: "/Admin/GetXmlPreview",
            type: "GET",
            data: { id: id },
            success: function (html) {
                var parsed = $.parseHTML(html, document, true);
                $("#xml-preview-content").empty().append(html);
                openPanel("xml-preview-overlay");
            },
            error: function (xhr) {
                alert("Preview error: " + (xhr.responseText || xhr.statusText));
            }
        });
       
    }


    //  Save Config 

    $("#saveXmlBtn").on("click", function () {
        var type = $("#xmlTypeSelect").val();
        var label = $.trim($("#xmlLabelInput").val());
        var xmlContent = $.trim($("#xmlEditText").val());

        if (!type) {
            showStatus("Select a config type before saving.", "error");
            return;
        }
        if (!xmlContent) {
            showStatus("XML content cannot be empty.", "error");
            return;
        }

        var $btn = $(this).prop("disabled", true).text("Saving…");

        $.ajax({
            url: "/Admin/UploadXmlConfig",
            type: "POST",
            data: {
                Type: type,
                Label: label,
                XmlContent: xmlContent,
                // CSRF token (Razor adds a hidden input via @Html.AntiForgeryToken()
                // or the default _Layout form; grab it by name)
                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
            },
            success: function () {
                showStatus("Configuration saved. Reload the page to see it in the table.", "success");
            },
            error: function (xhr) {
                showStatus("Save failed: " + (xhr.responseText || xhr.statusText), "error");
            },
            complete: function () {
                $btn.prop("disabled", false).text("✓ Save Version");
            }
        });
    });


    //  Activate Version 

    $(document).on("click", ".activate-config-btn", function () {
        var id = $(this).data("id");
        if (!confirm("Set config #" + id + " as the active version?")) return;

        $.ajax({
            url: "/Admin/ActivateVersion",
            type: "POST",
            data: {
                recordId: id,
                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
            },
            success: function () {
                window.location.reload();
            },
            error: function (xhr) {
                alert("Error: " + (xhr.responseText || xhr.statusText));
            }
        });
    });

});