$(function () {

    // ═══════════════════════════════════════════════════════
    //  Shared helpers
    // ═══════════════════════════════════════════════════════

    function openPanel(id) {
        $("#" + id).addClass("is-open").attr("aria-hidden", "false");
        $("body").css("overflow", "hidden");
    }
    function closePanel(id) {
        $("#" + id).removeClass("is-open").attr("aria-hidden", "true");
        $("body").css("overflow", "");
    }

    // Backdrop click closes any panel
    $(".xml-overlay").on("click", function (e) {
        if ($(e.target).is(".xml-overlay")) closePanel($(this).attr("id"));
    });

    // Status banners
    var _timers = {};
    function showStatus(elemId, msg, type) {
        clearTimeout(_timers[elemId]);
        $("#" + elemId)
            .removeClass("xml-status--success xml-status--error xml-status--info")
            .addClass("xml-status--" + (type || "success"))
            .text(msg).show();
        _timers[elemId] = setTimeout(function () { $("#" + elemId).fadeOut(300); }, 5000);
    }

    function csrf() { return $("input[name='__RequestVerificationToken']").val(); }

    function fmtDateTime(s) {
        if (!s) return "—";
        return new Date(s).toLocaleString("en-AU", {
            day: "2-digit", month: "short", year: "numeric",
            hour: "2-digit", minute: "2-digit"
        });
    }

    // Generic panel mode switcher (combined view/edit — no separate detail pane)
    function setPanelMode(prefix, mode) {
        var isView = (mode === "view");
        var $detail = $("#" + prefix + "-detail-pane");
        var $form   = $("#" + prefix + "-form-pane");
        if ($detail.length) {
            // Legacy two-pane mode kept for XML config panel
            $detail.toggle(isView);
            $form.toggle(!isView);
        } else {
            // Combined mode: always show form, disable inputs when viewing
            $form.show();
            $form.find("input:not([type=hidden]), select, textarea").prop("disabled", isView);
        }
        var $actions = $("#" + prefix + "-panel-actions");
        if ($actions.length) {
            $actions.show();
            $actions.find(".panel-save-btn").toggle(!isView);
            $actions.find(".panel-edit-btn").toggle(isView);
        }
        $("#" + prefix + "-status").hide();
    }

    // Switch from view to edit mode via the "Edit" button inside the panel
    $(document).on("click", ".panel-edit-btn", function () {
        setPanelMode($(this).data("prefix"), "edit");
    });

    // Staff dropdown cache
    var _staffCache = null;
    function loadStaffDropdowns(callback) {
        if (_staffCache) { callback(_staffCache); return; }
        $.getJSON("/Admin/GetAllStaff", function (data) {
            _staffCache = data;
            callback(data);
        }).fail(function () { callback([]); });
    }
    function populateStaffSelect(selectId, selectedId) {
        loadStaffDropdowns(function (staff) {
            var $sel = $("#" + selectId).empty().append('<option value="">— select staff —</option>');
            $.each(staff, function (_, s) {
                $sel.append($("<option>").val(s.id).text(s.firstName + " " + s.lastName));
            });
            if (selectedId) $sel.val(selectedId);
        });
    }

    // Server-side AJAX search.
    // Reads every named input/select inside the given form div,
    // sends them as query params to the endpoint,
    // swaps the container div's innerHTML with the returned partial HTML.
    //
    // cfg: { formId, containerId, endpoint, searchBtnId, clearBtnId, staffSelectId? }
    function wireAdminSearch(cfg) {
        var debounceTimer;

        function buildParams() {
            var params = { Size: 100, Page: 1 };
            $("#" + cfg.formId).find("input[name], select[name]").each(function () {
                var name = $(this).attr("name");
                var val = $(this).attr("type") === "checkbox"
                    ? ($(this).is(":checked") ? "true" : "")
                    : $.trim($(this).val());
                if (val !== "") params[name] = val;
            });
            return params;
        }

        function hasActiveFilter() {
            var active = false;
            // Consider a filter active when any field other than SortBy is filled
            $("#" + cfg.formId).find("input[name], select[name]").each(function () {
                if ($(this).attr("name") === "SortBy") return;
                var val = $(this).attr("type") === "checkbox"
                    ? $(this).is(":checked")
                    : !!$.trim($(this).val());
                if (val) { active = true; return false; }
            });
            return active;
        }

        function doSearch() {
            var $c = $("#" + cfg.containerId);
            $c.css("opacity", "0.45");
            $.get("/Admin/" + cfg.endpoint, buildParams())
                .done(function (html) {
                    $c.html(html).css("opacity", "1");
                    if (cfg.clearBtnId) $("#" + cfg.clearBtnId).toggle(hasActiveFilter());
                })
                .fail(function () { $c.css("opacity", "1"); });
        }

        // Debounce text/number/date inputs
        $("#" + cfg.formId).on("input", "input[name]", function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(doSearch, 350);
        });

        // Immediate response on select changes
        $("#" + cfg.formId).on("change", "select[name]", doSearch);

        // Search button
        if (cfg.searchBtnId) $("#" + cfg.searchBtnId).on("click", doSearch);

        // Clear: reset all named controls and re-fetch
        if (cfg.clearBtnId) {
            $("#" + cfg.clearBtnId).on("click", function () {
                $("#" + cfg.formId).find("input[name]").val("");
                $("#" + cfg.formId).find("select[name]").val("");
                $(this).hide();
                doSearch();
            });
        }

        // Populate staff filter dropdown (Events and Services only)
        if (cfg.staffSelectId) {
            loadStaffDropdowns(function (staff) {
                var $sel = $("#" + cfg.staffSelectId).empty()
                    .append('<option value="">All Staff</option>');
                $.each(staff, function (_, s) {
                    $sel.append($("<option>").val(s.id).text(s.firstName + " " + s.lastName));
                });
            });
        }
    }

    wireAdminSearch({ formId: "events-search-form",   containerId: "events-table-container",   endpoint: "SearchEvents",   searchBtnId: "eventSearchBtn",    clearBtnId: "eventClearBtn",    staffSelectId: "ev-staff-filter" });
    wireAdminSearch({ formId: "services-search-form", containerId: "services-table-container", endpoint: "SearchServices", searchBtnId: "serviceSearchBtn",  clearBtnId: "serviceClearBtn",  staffSelectId: "svc-staff-filter" });
    wireAdminSearch({ formId: "users-search-form",    containerId: "users-table-container",    endpoint: "SearchUsers",    searchBtnId: "userSearchBtn",     clearBtnId: "userClearBtn" });
    wireAdminSearch({ formId: "staff-search-form",    containerId: "staff-table-container",    endpoint: "SearchStaff",    searchBtnId: "staffSearchBtn",    clearBtnId: "staffClearBtn" });
    wireAdminSearch({ formId: "feedback-search-form", containerId: "feedback-table-container", endpoint: "SearchFeedback", searchBtnId: "feedbackSearchBtn", clearBtnId: "feedbackClearBtn" });


    // ═══════════════════════════════════════════════════════
    //  XML Editor Panel
    // ═══════════════════════════════════════════════════════

    $("#closeXmlPanel").on("click", function () { closePanel("xml-editor-overlay"); });

    $("#newConfigBtn").on("click", function () {
        $("#xmlConfigId").val(""); $("#xmlTypeSelect").val("");
        $("#xmlLabelInput").val(""); $("#xmlEditText").val("");
        $("#xml-panel-title").text("New Configuration");
        $("#xml-status").hide();
        openPanel("xml-editor-overlay");
    });

    $(document).on("click", ".edit-config-btn", function () {
        var $b = $(this), id = $b.data("id");
        $("#xmlConfigId").val(id);
        $("#xmlTypeSelect").val($b.data("type"));
        $("#xmlLabelInput").val($b.data("label") || "");
        $("#xml-panel-title").text("Edit — " + $b.data("type"));
        $("#xml-status").hide();
        $("#xmlEditText").val("Loading…");
        openPanel("xml-editor-overlay");
        $.ajax({
            url: "/Admin/LoadXml", data: { id: id }, dataType: "text",
            success: function (x) { $("#xmlEditText").val(x); },
            error: function () { $("#xmlEditText").val(""); showStatus("xml-status", "Could not load XML.", "error"); }
        });
    });

    $("#loadTemplateBtn").on("click", function () {
        var type = $("#xmlTypeSelect").val();
        if (!type) { showStatus("xml-status", "Select a config type first.", "error"); return; }
        $.ajax({
            url: "/Admin/LoadTemplate", data: { typeName: type }, dataType: "text",
            success: function (x) { $("#xmlEditText").val(x); showStatus("xml-status", "Template loaded.", "info"); },
            error: function (xhr) { showStatus("xml-status", xhr.responseText || "Template unavailable.", "error"); }
        });
    });

    $("#saveXmlBtn").on("click", function () {
        var type = $("#xmlTypeSelect").val(), xml = $.trim($("#xmlEditText").val());
        if (!type) { showStatus("xml-status", "Select a config type.", "error"); return; }
        if (!xml)  { showStatus("xml-status", "XML cannot be empty.", "error"); return; }
        var $b = $(this).prop("disabled", true).text("Saving…");
        $.ajax({
            url: "/Admin/UploadXmlConfig", type: "POST",
            data: { Type: type, Label: $.trim($("#xmlLabelInput").val()), XmlContent: xml, __RequestVerificationToken: csrf() },
            success: function () { showStatus("xml-status", "Saved. Reload to see it in the table.", "success"); },
            error: function (xhr) { showStatus("xml-status", "Save failed: " + (xhr.responseText || xhr.statusText), "error"); },
            complete: function () { $b.prop("disabled", false).text("✓ Save Version"); }
        });
    });

    $(document).on("click", ".activate-config-btn", function () {
        var id = $(this).data("id");
        if (!confirm("Set config #" + id + " as the active version?")) return;
        $.ajax({
            url: "/Admin/ActivateVersion", type: "POST",
            data: { recordId: id, __RequestVerificationToken: csrf() },
            success: function () { window.location.reload(); },
            error: function (xhr) { alert("Error: " + (xhr.responseText || xhr.statusText)); }
        });
    });

    // XML Preview
    $("#closePreviewPanel").on("click", function () { closePanel("xml-preview-overlay"); });
    $("#previewXmlBtn").on("click", function () {
        var id = $("#xmlConfigId").val();
        if (!id) { showStatus("xml-status", "Save the config first, or preview an existing row.", "info"); return; }
        loadXmlPreview(id);
    });
    $(document).on("click", ".preview-config-btn", function () { loadXmlPreview($(this).data("id")); });
    function loadXmlPreview(id) {
        $.ajax({
            url: "/Admin/GetXmlPreview", data: { id: id },
            success: function (html) { $("#xml-preview-content").empty().append(html); openPanel("xml-preview-overlay"); },
            error: function (xhr) { alert("Preview error: " + (xhr.responseText || xhr.statusText)); }
        });
    }


    // ═══════════════════════════════════════════════════════
    //  Events Panel
    // ═══════════════════════════════════════════════════════

    var _currentEventId = null;

    function openEventCreate() {
        _currentEventId = null;
        $("#event-panel-title").text("New Event");
        $("#eventId,#eventTitle,#eventLocation,#eventDesc").val("");
        $("#eventStart,#eventEnd").val("");
        $("#eventCost").val("0");
        $("#eventCapacity").val("");
        populateStaffSelect("eventStaffId", null);
        setPanelMode("event", "edit");
        openPanel("event-overlay");
    }

    function openEventEdit(id) {
        $.getJSON("/Admin/GetEvent", { id: id }, function (ev) {
            _currentEventId = ev.id;
            $("#event-panel-title").text("Edit Event");
            $("#eventId").val(ev.id);
            $("#eventTitle").val(ev.title);
            $("#eventLocation").val(ev.location);
            $("#eventDesc").val(ev.description);
            $("#eventCost").val(ev.cost);
            $("#eventCapacity").val(ev.max_Capcity);
            $("#eventStart").val(ev.start_Date_Time ? ev.start_Date_Time.substring(0, 16) : "");
            $("#eventEnd").val(ev.end_Date_Time ? ev.end_Date_Time.substring(0, 16) : "");
            populateStaffSelect("eventStaffId", ev.staffId);
            setPanelMode("event", "edit");
            openPanel("event-overlay");
        }).fail(function () { alert("Could not load event."); });
    }

    function openEventView(id) {
        $.getJSON("/Admin/GetEvent", { id: id }, function (ev) {
            _currentEventId = ev.id;
            $("#event-panel-title").text(ev.title);
            $("#eventId").val(ev.id);
            $("#eventTitle").val(ev.title);
            $("#eventLocation").val(ev.location);
            $("#eventDesc").val(ev.description);
            $("#eventCost").val(ev.cost);
            $("#eventCapacity").val(ev.max_Capcity);
            $("#eventStart").val(ev.start_Date_Time ? ev.start_Date_Time.substring(0, 16) : "");
            $("#eventEnd").val(ev.end_Date_Time ? ev.end_Date_Time.substring(0, 16) : "");
            populateStaffSelect("eventStaffId", ev.staffId);
            setPanelMode("event", "view");
            openPanel("event-overlay");
        }).fail(function () { alert("Could not load event."); });
    }

    $("#newEventBtn").on("click", openEventCreate);
    $("#closeEventPanel,#cancelEventBtn").on("click", function () { closePanel("event-overlay"); });

    $(document).on("click", ".edit-event-btn", function () { openEventEdit($(this).data("id")); });
    $(document).on("click", ".view-event-btn", function () { openEventView($(this).data("id")); });

    $(document).on("click", ".delete-event-btn", function () {
        var id = $(this).data("id"), title = $(this).data("title") || "this event";
        if (!confirm("Delete \"" + title + "\"? This cannot be undone.")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteEvent", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });

    $("#saveEventBtn").on("click", function () {
        var id = parseInt($("#eventId").val()) || 0;
        var data = {
            Id: id,
            Title: $("#eventTitle").val(),
            Location: $("#eventLocation").val(),
            Description: $("#eventDesc").val(),
            Cost: parseFloat($("#eventCost").val()) || 0,
            Max_Capcity: parseInt($("#eventCapacity").val()) || 0,
            Start_Date_Time: $("#eventStart").val(),
            End_Date_Time: $("#eventEnd").val(),
            StaffId: $("#eventStaffId").val(),
            __RequestVerificationToken: csrf()
        };
        if (!data.Title || !data.Location || !data.StaffId || !data.Start_Date_Time || !data.End_Date_Time) {
            showStatus("event-status", "Please fill in all required fields.", "error"); return;
        }
        var url = id > 0 ? "/Admin/UpdateEvent" : "/Admin/CreateEvent";
        var $b = $(this).prop("disabled", true).text("Saving…");
        $.ajax({
            url: url, type: "POST", data: data,
            success: function () {
                showStatus("event-status", "Event saved successfully.", "success");
                setTimeout(function () { closePanel("event-overlay"); window.location.reload(); }, 1200);
            },
            error: function (xhr) { showStatus("event-status", "Save failed: " + (xhr.responseText || xhr.statusText), "error"); },
            complete: function () { $b.prop("disabled", false).text("✓ Save Event"); }
        });
    });


    // ═══════════════════════════════════════════════════════
    //  Services Panel
    // ═══════════════════════════════════════════════════════

    var _currentServiceId = null;

    function openServiceCreate() {
        _currentServiceId = null;
        $("#service-panel-title").text("New Service");
        $("#serviceId,#serviceTitle,#serviceLocation,#serviceDesc").val("");
        $("#serviceCost").val("0");
        $("#serviceAvailStart,#serviceAvailEnd").val("");
        populateStaffSelect("serviceStaffId", null);
        setPanelMode("service", "edit");
        openPanel("service-overlay");
    }

    function openServiceEdit(id) {
        $.getJSON("/Admin/GetService", { id: id }, function (svc) {
            _currentServiceId = svc.id;
            $("#service-panel-title").text("Edit Service");
            $("#serviceId").val(svc.id);
            $("#serviceTitle").val(svc.title);
            $("#serviceLocation").val(svc.location);
            $("#serviceDesc").val(svc.description);
            $("#serviceCost").val(svc.cost);
            $("#serviceAvailStart").val(svc.available_Start_Time);
            $("#serviceAvailEnd").val(svc.available_End_Time);
            populateStaffSelect("serviceStaffId", svc.staffId);
            setPanelMode("service", "edit");
            openPanel("service-overlay");
        }).fail(function () { alert("Could not load service."); });
    }

    function openServiceView(id) {
        $.getJSON("/Admin/GetService", { id: id }, function (svc) {
            _currentServiceId = svc.id;
            $("#service-panel-title").text(svc.title);
            $("#serviceId").val(svc.id);
            $("#serviceTitle").val(svc.title);
            $("#serviceLocation").val(svc.location);
            $("#serviceDesc").val(svc.description);
            $("#serviceCost").val(svc.cost);
            $("#serviceAvailStart").val(svc.available_Start_Time);
            $("#serviceAvailEnd").val(svc.available_End_Time);
            populateStaffSelect("serviceStaffId", svc.staffId);
            setPanelMode("service", "view");
            openPanel("service-overlay");
        }).fail(function () { alert("Could not load service."); });
    }

    $("#newServiceBtn").on("click", openServiceCreate);
    $("#closeServicePanel,#cancelServiceBtn").on("click", function () { closePanel("service-overlay"); });

    $(document).on("click", ".edit-service-btn", function () { openServiceEdit($(this).data("id")); });
    $(document).on("click", ".view-service-btn", function () { openServiceView($(this).data("id")); });

    $(document).on("click", ".delete-service-btn", function () {
        var id = $(this).data("id"), title = $(this).data("title") || "this service";
        if (!confirm("Delete \"" + title + "\"? This cannot be undone.")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteService", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });

    $("#saveServiceBtn").on("click", function () {
        var id = parseInt($("#serviceId").val()) || 0;
        var data = {
            Id: id,
            Title: $("#serviceTitle").val(),
            Location: $("#serviceLocation").val(),
            Description: $("#serviceDesc").val(),
            Cost: parseFloat($("#serviceCost").val()) || 0,
            Available_Start_Time: $("#serviceAvailStart").val(),
            Available_End_Time: $("#serviceAvailEnd").val(),
            StaffId: $("#serviceStaffId").val(),
            __RequestVerificationToken: csrf()
        };
        if (!data.Title || !data.Location || !data.StaffId || !data.Available_Start_Time || !data.Available_End_Time) {
            showStatus("service-status", "Please fill in all required fields.", "error"); return;
        }
        var url = id > 0 ? "/Admin/UpdateService" : "/Admin/CreateService";
        var $b = $(this).prop("disabled", true).text("Saving…");
        $.ajax({
            url: url, type: "POST", data: data,
            success: function () {
                showStatus("service-status", "Service saved successfully.", "success");
                setTimeout(function () { closePanel("service-overlay"); window.location.reload(); }, 1200);
            },
            error: function (xhr) { showStatus("service-status", "Save failed: " + (xhr.responseText || xhr.statusText), "error"); },
            complete: function () { $b.prop("disabled", false).text("✓ Save Service"); }
        });
    });


    // ═══════════════════════════════════════════════════════
    //  Feedback Panel
    // ═══════════════════════════════════════════════════════

    function openFeedbackView(id) {
        $.getJSON("/Admin/GetFeedback", { id: id }, function (fb) {
            $("#feedback-panel-title").text("Feedback #" + fb.id);
            $("#feedbackId").val(fb.id);
            $("#fb-form-from").text(fb.from);
            $("#fb-form-date").text(fb.createdAt);
            $("#fb-form-message").text(fb.message);
            $("#feedbackStatus").val(fb.status);
            $("#feedbackResolution").val(fb.resolution_Message || "");
            populateStaffSelect("feedbackStaffId", fb.staffId);
            setPanelMode("feedback", "view");
            openPanel("feedback-overlay");
        }).fail(function () { alert("Could not load feedback."); });
    }

    function openFeedbackEdit(id) {
        $.getJSON("/Admin/GetFeedback", { id: id }, function (fb) {
            $("#feedback-panel-title").text("Resolve Feedback #" + fb.id);
            $("#feedbackId").val(fb.id);
            $("#fb-form-from").text(fb.from);
            $("#fb-form-date").text(fb.createdAt);
            $("#fb-form-message").text(fb.message);
            $("#feedbackStatus").val(fb.status);
            $("#feedbackResolution").val(fb.resolution_Message || "");
            populateStaffSelect("feedbackStaffId", fb.staffId);
            setPanelMode("feedback", "edit");
            openPanel("feedback-overlay");
        }).fail(function () { alert("Could not load feedback."); });
    }

    $("#closeFeedbackPanel,#cancelFeedbackBtn").on("click", function () { closePanel("feedback-overlay"); });

    $(document).on("click", ".view-feedback-btn", function () { openFeedbackView($(this).data("id")); });
    $(document).on("click", ".edit-feedback-btn", function () { openFeedbackEdit($(this).data("id")); });

    $(document).on("click", ".delete-feedback-btn", function () {
        var id = $(this).data("id");
        if (!confirm("Delete this feedback entry? This cannot be undone.")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteFeedback", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });

    $("#saveFeedbackBtn").on("click", function () {
        var data = {
            Id: $("#feedbackId").val(),
            Status: $("#feedbackStatus").val(),
            Resolution_Message: $("#feedbackResolution").val(),
            StaffId: $("#feedbackStaffId").val(),
            __RequestVerificationToken: csrf()
        };
        if (!data.StaffId) { showStatus("feedback-status", "Please select a staff member.", "error"); return; }
        var $b = $(this).prop("disabled", true).text("Saving…");
        $.ajax({
            url: "/Admin/UpdateFeedback", type: "POST", data: data,
            success: function () {
                showStatus("feedback-status", "Feedback updated.", "success");
                setTimeout(function () { closePanel("feedback-overlay"); window.location.reload(); }, 1200);
            },
            error: function (xhr) { showStatus("feedback-status", "Save failed: " + (xhr.responseText || xhr.statusText), "error"); },
            complete: function () { $b.prop("disabled", false).text("✓ Save"); }
        });
    });


    // ═══════════════════════════════════════════════════════
    //  Users Panel
    // ═══════════════════════════════════════════════════════

    function openUserCreate() {
        $("#user-panel-title").text("New User");
        $("#userId,#userFirstName,#userLastName,#userEmail,#userPhone,#userAddress,#userPassword").val("");
        setPanelMode("user", "edit");
        openPanel("user-overlay");
    }

    function openUserView(id) {
        $.getJSON("/Admin/GetUser", { id: id }, function (u) {
            $("#user-panel-title").text(u.firstName + " " + u.lastName);
            $("#userId").val(u.id);
            $("#userFirstName").val(u.firstName);
            $("#userLastName").val(u.lastName);
            $("#userEmail").val(u.email);
            $("#userPhone").val(u.phoneNumber || "");
            $("#userAddress").val(u.address || "");
            $("#userPassword").val("");
            setPanelMode("user", "view");
            openPanel("user-overlay");
        }).fail(function () { alert("Could not load user."); });
    }

    function openUserEdit(id) {
        $.getJSON("/Admin/GetUser", { id: id }, function (u) {
            $("#user-panel-title").text("Edit User");
            $("#userId").val(u.id);
            $("#userFirstName").val(u.firstName);
            $("#userLastName").val(u.lastName);
            $("#userEmail").val(u.email);
            $("#userPhone").val(u.phoneNumber);
            $("#userAddress").val(u.address);
            $("#userPassword").val("");
            setPanelMode("user", "edit");
            openPanel("user-overlay");
        }).fail(function () { alert("Could not load user."); });
    }

    $("#newUserBtn").on("click", openUserCreate);
    $("#closeUserPanel,#cancelUserBtn").on("click", function () { closePanel("user-overlay"); });

    $(document).on("click", ".view-user-btn", function () { openUserView($(this).data("id")); });
    $(document).on("click", ".edit-user-btn", function () { openUserEdit($(this).data("id")); });

    $(document).on("click", ".delete-user-btn", function () {
        var id = $(this).data("id"), name = $(this).data("name") || "this user";
        if (!confirm("Delete \"" + name + "\"? This will also delete their bookings and registrations.")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteUser", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });

    $("#saveUserBtn").on("click", function () {
        var id = $("#userId").val();
        var data = {
            Id: id,
            First_Name: $("#userFirstName").val(),
            Last_Name: $("#userLastName").val(),
            Email: $("#userEmail").val(),
            Phone_Number: $("#userPhone").val(),
            Address: $("#userAddress").val(),
            Password: $("#userPassword").val(),
            __RequestVerificationToken: csrf()
        };
        if (!data.First_Name || !data.Email) { showStatus("user-status", "First name and email are required.", "error"); return; }
        var url = id ? "/Admin/UpdateUser" : "/Admin/CreateUser";
        var $b = $(this).prop("disabled", true).text("Saving…");
        $.ajax({
            url: url, type: "POST", data: data,
            success: function () {
                showStatus("user-status", "User saved.", "success");
                setTimeout(function () { closePanel("user-overlay"); window.location.reload(); }, 1200);
            },
            error: function (xhr) { showStatus("user-status", "Save failed: " + (xhr.responseText || xhr.statusText), "error"); },
            complete: function () { $b.prop("disabled", false).text("✓ Save User"); }
        });
    });


    // ═══════════════════════════════════════════════════════
    //  Staff Panel
    // ═══════════════════════════════════════════════════════

    function openStaffCreate() {
        $("#staff-panel-title").text("New Staff Member");
        $("#staffId,#staffFirstName,#staffLastName,#staffEmail,#staffPhone,#staffAddress,#staffRole,#staffPassword").val("");
        setPanelMode("staff", "edit");
        openPanel("staff-overlay");
    }

    function openStaffView(id) {
        $.getJSON("/Admin/GetStaff", { id: id }, function (s) {
            $("#staff-panel-title").text(s.firstName + " " + s.lastName);
            $("#staffId").val(s.id);
            $("#staffFirstName").val(s.firstName);
            $("#staffLastName").val(s.lastName);
            $("#staffEmail").val(s.email);
            $("#staffPhone").val(s.phoneNumber || "");
            $("#staffAddress").val(s.address || "");
            $("#staffRole").val(s.jobTitle || "");
            $("#staffPassword").val("");
            setPanelMode("staff", "view");
            openPanel("staff-overlay");
        }).fail(function () { alert("Could not load staff member."); });
    }

    function openStaffEdit(id) {
        $.getJSON("/Admin/GetStaff", { id: id }, function (s) {
            $("#staff-panel-title").text("Edit Staff Member");
            $("#staffId").val(s.id);
            $("#staffFirstName").val(s.firstName);
            $("#staffLastName").val(s.lastName);
            $("#staffEmail").val(s.email);
            $("#staffPhone").val(s.phoneNumber);
            $("#staffAddress").val(s.address);
            $("#staffRole").val(s.jobTitle);
            $("#staffPassword").val("");
            setPanelMode("staff", "edit");
            openPanel("staff-overlay");
        }).fail(function () { alert("Could not load staff member."); });
    }

    $("#newStaffBtn").on("click", openStaffCreate);
    $("#closeStaffPanel,#cancelStaffBtn").on("click", function () { closePanel("staff-overlay"); });

    $(document).on("click", ".view-staff-btn", function () { openStaffView($(this).data("id")); });
    $(document).on("click", ".edit-staff-btn", function () { openStaffEdit($(this).data("id")); });

    $(document).on("click", ".delete-staff-btn", function () {
        var id = $(this).data("id"), name = $(this).data("name") || "this staff member";
        if (!confirm("Delete staff member \"" + name + "\"? This cannot be undone.")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteStaff", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });

    $("#saveStaffBtn").on("click", function () {
        var id = $("#staffId").val();
        var data = {
            Id: id,
            First_Name: $("#staffFirstName").val(),
            Last_Name: $("#staffLastName").val(),
            Email: $("#staffEmail").val(),
            Phone_Number: $("#staffPhone").val(),
            Address: $("#staffAddress").val(),
            JobTitle: $("#staffRole").val(),
            Password: $("#staffPassword").val(),
            __RequestVerificationToken: csrf()
        };
        if (!data.First_Name || !data.Email || !data.JobTitle) {
            showStatus("staff-status", "First name, email and role are required.", "error"); return;
        }
        var url = id ? "/Admin/UpdateStaff" : "/Admin/CreateStaff";
        var $b = $(this).prop("disabled", true).text("Saving…");
        $.ajax({
            url: url, type: "POST", data: data,
            success: function () {
                showStatus("staff-status", "Staff member saved.", "success");
                _staffCache = null; // Invalidate staff cache
                setTimeout(function () { closePanel("staff-overlay"); window.location.reload(); }, 1200);
            },
            error: function (xhr) { showStatus("staff-status", "Save failed: " + (xhr.responseText || xhr.statusText), "error"); },
            complete: function () { $b.prop("disabled", false).text("✓ Save Staff"); }
        });
    });


    // ═══════════════════════════════════════════════════════
    //  Event Registrations — Delete
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".delete-reg-btn", function () {
        var userId = $(this).data("userid"),
            eventId = $(this).data("eventid"),
            label = $(this).data("label") || "this registration";
        if (!confirm("Delete registration for \"" + label + "\"?")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteEventRegistration", type: "POST",
            data: { userId: userId, eventId: eventId, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });


    // ═══════════════════════════════════════════════════════
    //  Service Bookings — Delete
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".delete-booking-btn", function () {
        var userId = $(this).data("userid"),
            serviceId = $(this).data("serviceid"),
            label = $(this).data("label") || "this booking";
        if (!confirm("Delete booking for \"" + label + "\"?")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteServiceBooking", type: "POST",
            data: { userId: userId, serviceId: serviceId, __RequestVerificationToken: csrf() },
            success: function () { $btn.closest("tr").fadeOut(300, function () { $(this).remove(); }); },
            error: function (xhr) { alert("Delete failed: " + (xhr.responseText || "Unknown error")); $btn.prop("disabled", false); }
        });
    });

});
