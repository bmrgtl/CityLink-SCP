function wirePublicSearch(formId, containerId, endpoint, clearBtnId, sortSelectId, sortDirBtnId, sortHiddenId, sortOrderHiddenId) {
    var $form = $("#" + formId);
    if (!$form.length) return;

    // Sync visible sort select to hidden value on load
    $("#" + sortSelectId).val($("#" + sortHiddenId).val());
    syncSortDirBtn(sortDirBtnId, sortOrderHiddenId);

    // Sort-by select → update hidden, reset to page 1, submit
    $(document).on("change", "#" + sortSelectId, function () {
        $("#" + sortHiddenId).val($(this).val());
        $form.find("[name=Page]").val("1");
        $form.trigger("submit");
    });

    // Asc/desc toggle
    $(document).on("click", "#" + sortDirBtnId, function () {
        var $o = $("#" + sortOrderHiddenId);
        $o.val($o.val() === "asc" ? "desc" : "asc");
        syncSortDirBtn(sortDirBtnId, sortOrderHiddenId);
        $form.find("[name=Page]").val("1");
        $form.trigger("submit");
    });

    // Pager buttons inside the results container
    $(document).on("click", "#" + containerId + " .pager-btn", function () {
        $form.find("[name=Page]").val($(this).data("page"));
        $form.trigger("submit");
    });

    // Clear — reset fields, restore defaults, reload
    $(document).on("click", "#" + clearBtnId, function () {
        $form[0].reset();
        var defaultSort = $("#" + sortSelectId + " option:first").val();
        $("#" + sortHiddenId).val(defaultSort);
        $("#" + sortSelectId).val(defaultSort);
        $("#" + sortOrderHiddenId).val("asc");
        syncSortDirBtn(sortDirBtnId, sortOrderHiddenId);
        $form.find("[name=Page]").val("1");
        $form.trigger("submit");
    });

    // AJAX submit
    $(document).on("submit", "#" + formId, function (e) {
        e.preventDefault();
        var params = $.param($(this).serializeArray().filter(function (f) { return f.value !== ""; }));
        var $c = $("#" + containerId).css("opacity", "0.45");
        $.ajax({
            url: "/Home/" + endpoint + "?" + params,
            method: "GET",
            success: function (html) {
                $c.html(html).css("opacity", "1");
                var fields = $form.serializeArray().filter(function (f) {
                    return ["Page", "Size", "SortBy", "SortOrder"].indexOf(f.name) === -1 && f.value !== "";
                });
                $("#" + clearBtnId).toggle(fields.length > 0);
            },
            error: function () { $c.css("opacity", "1"); }
        });
    });
}

function syncSortDirBtn(btnId, orderId) {
    $("#" + btnId).text($("#" + orderId).val() === "asc" ? "▲" : "▼");
}
