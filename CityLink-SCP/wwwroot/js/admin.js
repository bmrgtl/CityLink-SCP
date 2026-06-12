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

    $(document).on("click", ".xml-overlay", function (e) {
        if ($(e.target).is(".xml-overlay")) closePanel($(this).attr("id"));
    });

    var _timers = {};
    function showStatus(elemId, msg, type) {
        clearTimeout(_timers[elemId]);
        $("#" + elemId)
            .removeClass("xml-status--success xml-status--error xml-status--info")
            .addClass("xml-status--" + (type || "success"))
            .text(msg).show();
        _timers[elemId] = setTimeout(function () { $("#" + elemId).fadeOut(300); }, 5000);
    }

    function csrf() { return $("input[name='__RequestVerificationToken']").first().val(); }

    function setPanelMode(prefix, mode) {
        var isView = (mode === "view");
        var $detail = $("#" + prefix + "-detail-pane");
        var $form   = $("#" + prefix + "-form-pane");
        if ($detail.length) {
            $detail.toggle(isView);
            $form.toggle(!isView);
        } else {
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

    $(document).on("click", ".panel-edit-btn", function () {
        setPanelMode($(this).data("prefix"), "edit");
    });

    function errMsg(xhr) {
        try { return JSON.parse(xhr.responseText).error || xhr.responseText; }
        catch (e) { return xhr.responseText || xhr.statusText || "Unknown error"; }
    }

    function resubmit(formId) {
        $("#" + formId).trigger("submit");
    }

    // ═══════════════════════════════════════════════════════
    //  Responsive column hiding
    // ═══════════════════════════════════════════════════════

    function applyTablePriorities(table) {
        var $table = $(table);
        var $headerRow = $table.find("thead tr");
        if (!$headerRow.length) return;

        var hideableCols = [];
        $headerRow.find("th[data-hide-priority]").each(function () {
            hideableCols.push({
                index: $(this).index(),
                priority: parseInt($(this).data("hide-priority"), 10)
            });
        });
        hideableCols.sort(function (a, b) { return a.priority - b.priority; });

        function setColVisible(colIndex, visible) {
            $table.find("tr > *:nth-child(" + (colIndex + 1) + ")").css("display", visible ? "" : "none");
        }

        function isOverflowing() {
            return table.scrollWidth > $table.parent().innerWidth() + 1;
        }

        $.each(hideableCols, function (_, col) { setColVisible(col.index, true); });
        $.each(hideableCols, function (_, col) {
            if (!isOverflowing()) return false;
            setColVisible(col.index, false);
        });
    }

    function applyAllTables() {
        $("table.admin-table").each(function () { applyTablePriorities(this); });
    }

    $(window).on("resize", applyAllTables);

    // ═══════════════════════════════════════════════════════
    //  Table header sorting
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".admin-table th[data-sort]", function () {
        var $th   = $(this);
        var field = $th.data("sort");
        var $form = $th.closest(".admin-section").find("form.table-search");
        if (!$form.length) return;

        var $sortBy    = $form.find("input[name='SortBy']");
        var $sortOrder = $form.find("input[name='SortOrder']");
        var $page      = $form.find("input[name='Page']");

        if ($sortBy.val() === field) {
            $sortOrder.val($sortOrder.val() === "asc" ? "desc" : "asc");
        } else {
            $sortBy.val(field);
            $sortOrder.val("asc");
        }
        $page.val("1");
        $form.trigger("submit");
    });

    // ═══════════════════════════════════════════════════════
    //  Pagination
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".pager-btn:not([disabled])", function () {
        var $form = $(this).closest(".admin-section").find("form.table-search");
        if (!$form.length) return;
        $form.find("input[name='Page']").val($(this).data("page"));
        $form.trigger("submit");
    });

    // ═══════════════════════════════════════════════════════
    //  Search forms — serialize → filter empties → AJAX GET
    // ═══════════════════════════════════════════════════════

    function wireSearch(formId, containerId, endpoint, clearBtnId) {
        $(document).on("submit", "#" + formId, function (e) {
            e.preventDefault();
            var params = $.param($(this).serializeArray().filter(function (f) { return f.value !== ""; }));
            var $c = $("#" + containerId).css("opacity", "0.45");
            $.ajax({
                url: "/Admin/" + endpoint + "?" + params,
                method: "GET",
                success: function (html) { $c.html(html).css("opacity", "1"); applyAllTables(); },
                error: function (xhr) { $c.css("opacity", "1"); alert("Search failed: " + errMsg(xhr)); }
            });
        });

        if (clearBtnId) {
            $(document).on("click", "#" + clearBtnId, function () {
                document.getElementById(formId).reset();
                $("#" + formId).trigger("submit");
            });
        }
    }

    wireSearch("events-search-form",   "events-table-container",       "SearchEvents",             "eventClearBtn");
    wireSearch("services-search-form", "services-table-container",     "SearchServices",           "serviceClearBtn");
    wireSearch("users-search-form",    "users-table-container",        "SearchUsers",              "userClearBtn");
    wireSearch("staff-search-form",    "staff-table-container",        "SearchStaff",              "staffClearBtn");
    wireSearch("feedback-search-form", "feedback-table-container",     "SearchFeedback",           "feedbackClearBtn");
    wireSearch("xml-search-form",      "xml-configs-table-container",  "SearchXmlConfigs",         "xmlClearBtn");
    wireSearch("bookings-search-form", "bookings-table-container",     "SearchServiceBookings",    "bookingClearBtn");
    wireSearch("regs-search-form",     "regs-table-container",         "SearchEventRegistrations", "regClearBtn");

    // ═══════════════════════════════════════════════════════
    //  Tabs — load section(s) on switch, reload on every visit
    // ═══════════════════════════════════════════════════════

    var tabLoaders = {
        "xml":             [{ url: "/Admin/GetXmlConfigsSection",   id: "xml-section-container" }],
        "users-staff":     [{ url: "/Admin/GetUsersSection",         id: "users-section-container" },
                            { url: "/Admin/GetStaffSection",         id: "staff-section-container" }],
        "feedback":        [{ url: "/Admin/GetFeedbackSection",      id: "feedback-section-container" }],
        "services-events": [{ url: "/Admin/GetServicesSection",      id: "services-section-container" },
                            { url: "/Admin/GetEventsSection",        id: "events-section-container" }],
        "bookings-regs":   [{ url: "/Admin/GetBookingsSection",      id: "bookings-section-container" },
                            { url: "/Admin/GetRegistrationsSection", id: "registrations-section-container" }]
    };

    function loadTab(tab) {
        $(".admin-tab-panel").hide();
        $("#tab-" + tab).show();
        $.each(tabLoaders[tab], function (_, cfg) {
            var $c = $("#" + cfg.id).empty().css("opacity", "0.45");
            $.get(cfg.url, function (html) { $c.html(html).css("opacity", "1"); applyAllTables(); })
             .fail(function (xhr) { $c.css("opacity", "1").html('<p style="color:#c0392b;padding:1rem">Failed to load: ' + errMsg(xhr) + "</p>"); });
        });
    }

    $(document).on("click", ".admin-tab-btn", function () {
        $(".admin-tab-btn").removeClass("active");
        $(this).addClass("active");
        loadTab($(this).data("tab"));
    });

    loadTab("xml");

    // ═══════════════════════════════════════════════════════
    //  XML Config
    // ═══════════════════════════════════════════════════════

    $(document).on("click", "#newConfigBtn", function () {
        $.get("/Admin/NewXmlPanel", function (html) {
            $("#xml-editor-overlay").html(html);
            openPanel("xml-editor-overlay");
        }).fail(function (xhr) { alert("Could not open config panel: " + errMsg(xhr)); });
    });

    $(document).on("click", ".edit-config-btn", function () {
        $.get("/Admin/GetXmlConfig", { id: $(this).data("id") }, function (html) {
            $("#xml-editor-overlay").html(html);
            openPanel("xml-editor-overlay");
        }).fail(function (xhr) { alert("Could not load config: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closeXmlPanel", function () { closePanel("xml-editor-overlay"); });

    $(document).on("click", "#loadTemplateBtn", function () {
        var type = $("#xmlTypeSelect").val();
        if (!type) { showStatus("xml-status", "Select a config type first.", "error"); return; }
        $.ajax({
            url: "/Admin/LoadTemplate", data: { typeName: type }, dataType: "text",
            success: function (x) { $("#xmlEditText").val(x); showStatus("xml-status", "Template loaded.", "info"); },
            error: function (xhr) { showStatus("xml-status", errMsg(xhr) || "Template unavailable.", "error"); }
        });
    });

    $(document).on("click", "#previewXmlBtn", function () {
        var id = $("#xml-editor-overlay input[name='ExistingId']").val();
        if (!id) { showStatus("xml-status", "Save the config first, or preview an existing row.", "info"); return; }
        $.get("/Admin/GetXmlPreview", { id: id }, function (html) {
            $("#xml-preview-content").empty().append(html);
            openPanel("xml-preview-overlay");
        }).fail(function (xhr) { alert("Preview error: " + errMsg(xhr)); });
    });

    $(document).on("click", ".preview-config-btn", function () {
        $.get("/Admin/GetXmlPreview", { id: $(this).data("id") }, function (html) {
            $("#xml-preview-content").empty().append(html);
            openPanel("xml-preview-overlay");
        }).fail(function (xhr) { alert("Preview error: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closePreviewPanel", function () { closePanel("xml-preview-overlay"); });

    $(document).on("submit", "#xml-form", function (e) {
        e.preventDefault();
        var $form = $(this);
        var $btn = $form.find("#saveXmlBtn").prop("disabled", true).text("Saving…");
        $.ajax({
            url: $form.attr("action"), type: "POST", data: $form.serialize(),
            success: function () {
                closePanel("xml-editor-overlay");
                resubmit("xml-search-form");
                showStatus("xml-status", "Saved.", "success");
                $btn.prop("disabled", false).text("✓ Save Version");
            },
            error: function (xhr) {
                showStatus("xml-status", "Save failed: " + errMsg(xhr), "error");
                $btn.prop("disabled", false).text("✓ Save Version");
            }
        });
    });

    $(document).on("click", ".activate-config-btn", function () {
        var id = $(this).data("id");
        if (!confirm("Set config #" + id + " as the active version?")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/ActivateVersion", type: "POST",
            data: { recordId: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("xml-search-form"); },
            error: function (xhr) { alert("Error: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    $(document).on("click", ".delete-config-btn", function () {
        var id = $(this).data("id"), title = $(this).data("title") || "this config";
        if (!confirm('Delete "' + title + '"? This cannot be undone.')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteXmlConfig", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("xml-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Events
    // ═══════════════════════════════════════════════════════

    $(document).on("click", "#newEventBtn", function () {
        $.get("/Admin/NewEventPanel", function (html) {
            $("#event-overlay").html(html);
            setPanelMode("event", "edit");
            openPanel("event-overlay");
        }).fail(function (xhr) { alert("Could not open event panel: " + errMsg(xhr)); });
    });

    $(document).on("click", ".view-event-btn", function () {
        $.get("/Admin/GetEvent", { id: $(this).data("id") }, function (html) {
            $("#event-overlay").html(html);
            setPanelMode("event", "view");
            openPanel("event-overlay");
        }).fail(function (xhr) { alert("Could not load event: " + errMsg(xhr)); });
    });

    $(document).on("click", ".edit-event-btn", function () {
        $.get("/Admin/GetEvent", { id: $(this).data("id") }, function (html) {
            $("#event-overlay").html(html);
            setPanelMode("event", "edit");
            openPanel("event-overlay");
        }).fail(function (xhr) { alert("Could not load event: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closeEventPanel, #cancelEventBtn", function () {
        closePanel("event-overlay");
    });

    $(document).on("submit", "#event-form", function (e) {
        e.preventDefault();
        var $form = $(this);
        var $btn = $form.find(".panel-save-btn").prop("disabled", true);
        $.ajax({
            url: $form.attr("action"), type: "POST", data: $form.serialize(),
            success: function () { closePanel("event-overlay"); resubmit("events-search-form"); },
            error: function (xhr) { showStatus("event-status", errMsg(xhr), "error"); $btn.prop("disabled", false); }
        });
    });

    $(document).on("click", ".delete-event-btn", function () {
        var id = $(this).data("id"), title = $(this).data("title") || "this event";
        if (!confirm('Delete "' + title + '"? This cannot be undone.')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteEvent", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("events-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Services
    // ═══════════════════════════════════════════════════════

    $(document).on("click", "#newServiceBtn", function () {
        $.get("/Admin/NewServicePanel", function (html) {
            $("#service-overlay").html(html);
            setPanelMode("service", "edit");
            openPanel("service-overlay");
        }).fail(function (xhr) { alert("Could not open service panel: " + errMsg(xhr)); });
    });

    $(document).on("click", ".view-service-btn", function () {
        $.get("/Admin/GetService", { id: $(this).data("id") }, function (html) {
            $("#service-overlay").html(html);
            setPanelMode("service", "view");
            openPanel("service-overlay");
        }).fail(function (xhr) { alert("Could not load service: " + errMsg(xhr)); });
    });

    $(document).on("click", ".edit-service-btn", function () {
        $.get("/Admin/GetService", { id: $(this).data("id") }, function (html) {
            $("#service-overlay").html(html);
            setPanelMode("service", "edit");
            openPanel("service-overlay");
        }).fail(function (xhr) { alert("Could not load service: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closeServicePanel, #cancelServiceBtn", function () {
        closePanel("service-overlay");
    });

    $(document).on("submit", "#service-form", function (e) {
        e.preventDefault();
        var $form = $(this);
        var $btn = $form.find(".panel-save-btn").prop("disabled", true);
        $.ajax({
            url: $form.attr("action"), type: "POST", data: $form.serialize(),
            success: function () { closePanel("service-overlay"); resubmit("services-search-form"); },
            error: function (xhr) { showStatus("service-status", errMsg(xhr), "error"); $btn.prop("disabled", false); }
        });
    });

    $(document).on("click", ".delete-service-btn", function () {
        var id = $(this).data("id"), title = $(this).data("title") || "this service";
        if (!confirm('Delete "' + title + '"? This cannot be undone.')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteService", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("services-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Feedback
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".view-feedback-btn", function () {
        $.get("/Admin/GetFeedback", { id: $(this).data("id") }, function (html) {
            $("#feedback-overlay").html(html);
            setPanelMode("feedback", "view");
            openPanel("feedback-overlay");
        }).fail(function (xhr) { alert("Could not load feedback: " + errMsg(xhr)); });
    });

    $(document).on("click", ".edit-feedback-btn", function () {
        $.get("/Admin/GetFeedback", { id: $(this).data("id") }, function (html) {
            $("#feedback-overlay").html(html);
            setPanelMode("feedback", "edit");
            openPanel("feedback-overlay");
        }).fail(function (xhr) { alert("Could not load feedback: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closeFeedbackPanel, #cancelFeedbackBtn", function () {
        closePanel("feedback-overlay");
    });

    $(document).on("submit", "#feedback-form", function (e) {
        e.preventDefault();
        var $form = $(this);
        var $btn = $form.find(".panel-save-btn").prop("disabled", true);
        $.ajax({
            url: $form.attr("action"), type: "POST", data: $form.serialize(),
            success: function () { closePanel("feedback-overlay"); resubmit("feedback-search-form"); },
            error: function (xhr) { showStatus("feedback-status", errMsg(xhr), "error"); $btn.prop("disabled", false); }
        });
    });

    $(document).on("click", ".delete-feedback-btn", function () {
        var id = $(this).data("id");
        if (!confirm("Delete this feedback entry? This cannot be undone.")) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteFeedback", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("feedback-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Users
    // ═══════════════════════════════════════════════════════

    $(document).on("click", "#newUserBtn", function () {
        $.get("/Admin/NewUserPanel", function (html) {
            $("#user-overlay").html(html);
            setPanelMode("user", "edit");
            openPanel("user-overlay");
        }).fail(function (xhr) { alert("Could not open user panel: " + errMsg(xhr)); });
    });

    $(document).on("click", ".view-user-btn", function () {
        $.get("/Admin/GetUser", { id: $(this).data("id") }, function (html) {
            $("#user-overlay").html(html);
            setPanelMode("user", "view");
            openPanel("user-overlay");
        }).fail(function (xhr) { alert("Could not load user: " + errMsg(xhr)); });
    });

    $(document).on("click", ".edit-user-btn", function () {
        $.get("/Admin/GetUser", { id: $(this).data("id") }, function (html) {
            $("#user-overlay").html(html);
            setPanelMode("user", "edit");
            openPanel("user-overlay");
        }).fail(function (xhr) { alert("Could not load user: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closeUserPanel, #cancelUserBtn", function () {
        closePanel("user-overlay");
    });

    $(document).on("submit", "#user-form", function (e) {
        e.preventDefault();
        var $form = $(this);
        var $btn = $form.find(".panel-save-btn").prop("disabled", true);
        $.ajax({
            url: $form.attr("action"), type: "POST", data: $form.serialize(),
            success: function () { closePanel("user-overlay"); resubmit("users-search-form"); },
            error: function (xhr) { showStatus("user-status", errMsg(xhr), "error"); $btn.prop("disabled", false); }
        });
    });

    $(document).on("click", ".delete-user-btn", function () {
        var id = $(this).data("id"), name = $(this).data("name") || "this user";
        if (!confirm('Delete "' + name + '"? This will also delete their bookings and registrations.')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteUser", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("users-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Staff
    // ═══════════════════════════════════════════════════════

    $(document).on("click", "#newStaffBtn", function () {
        $.get("/Admin/NewStaffPanel", function (html) {
            $("#staff-overlay").html(html);
            setPanelMode("staff", "edit");
            openPanel("staff-overlay");
        }).fail(function (xhr) { alert("Could not open staff panel: " + errMsg(xhr)); });
    });

    $(document).on("click", ".view-staff-btn", function () {
        $.get("/Admin/GetStaff", { id: $(this).data("id") }, function (html) {
            $("#staff-overlay").html(html);
            setPanelMode("staff", "view");
            openPanel("staff-overlay");
        }).fail(function (xhr) { alert("Could not load staff member: " + errMsg(xhr)); });
    });

    $(document).on("click", ".edit-staff-btn", function () {
        $.get("/Admin/GetStaff", { id: $(this).data("id") }, function (html) {
            $("#staff-overlay").html(html);
            setPanelMode("staff", "edit");
            openPanel("staff-overlay");
        }).fail(function (xhr) { alert("Could not load staff member: " + errMsg(xhr)); });
    });

    $(document).on("click", "#closeStaffPanel, #cancelStaffBtn", function () {
        closePanel("staff-overlay");
    });

    $(document).on("submit", "#staff-form", function (e) {
        e.preventDefault();
        var $form = $(this);
        var $btn = $form.find(".panel-save-btn").prop("disabled", true);
        $.ajax({
            url: $form.attr("action"), type: "POST", data: $form.serialize(),
            success: function () { closePanel("staff-overlay"); resubmit("staff-search-form"); },
            error: function (xhr) { showStatus("staff-status", errMsg(xhr), "error"); $btn.prop("disabled", false); }
        });
    });

    $(document).on("click", ".delete-staff-btn", function () {
        var id = $(this).data("id"), name = $(this).data("name") || "this staff member";
        if (!confirm('Delete "' + name + '"? This cannot be undone.')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteStaff", type: "POST",
            data: { id: id, __RequestVerificationToken: csrf() },
            success: function () { resubmit("staff-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Event Registrations
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".delete-reg-btn", function () {
        var userId = $(this).data("userid"),
            eventId = $(this).data("eventid"),
            label = $(this).data("label") || "this registration";
        if (!confirm('Delete registration for "' + label + '"?')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteEventRegistration", type: "POST",
            data: { userId: userId, eventId: eventId, __RequestVerificationToken: csrf() },
            success: function () { resubmit("regs-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

    // ═══════════════════════════════════════════════════════
    //  Service Bookings
    // ═══════════════════════════════════════════════════════

    $(document).on("click", ".delete-booking-btn", function () {
        var userId = $(this).data("userid"),
            serviceId = $(this).data("serviceid"),
            label = $(this).data("label") || "this booking";
        if (!confirm('Delete booking for "' + label + '"?')) return;
        var $btn = $(this).prop("disabled", true);
        $.ajax({
            url: "/Admin/DeleteServiceBooking", type: "POST",
            data: { userId: userId, serviceId: serviceId, __RequestVerificationToken: csrf() },
            success: function () { resubmit("bookings-search-form"); },
            error: function (xhr) { alert("Delete failed: " + errMsg(xhr)); $btn.prop("disabled", false); }
        });
    });

});
