/*
 * JQUERY & AJAX REFERENCE
 *
 * JQUERY BASICS
 * $()                    - Core jQuery function. Selects elements, e.g. $('#myButton') selects element with id "myButton".
 * .on() / .click()       - Attach click (or other) event listeners to elements.
 * .val()                 - Get or set the value of an input field.
 * .text() / .html()      - Get or set the text or HTML content of an element.
 * .show() / .hide()      - Show or hide elements.
 * .addClass() /
 *   .removeClass()       - Add or remove CSS classes.
 * .css()                 - Get or set inline styles, e.g. $('#box').css('color', 'red').
 * .append() / .prepend() - Add content inside an element at the end or beginning.
 * $(document).ready()    - Runs your code only after the page has fully loaded.
 *                          Without this, your JS may run before the HTML exists.
 *
 * AJAX (VIA JQUERY)
 * $.ajax()               - Main function for making HTTP requests without reloading the page.
 * $.get() / $.post()     - Shorthand versions of $.ajax() for simple GET or POST requests.
 * success / error        - Callbacks that run when the request succeeds or fails.
 * JSON                   - Data format you'll almost always send/receive. Looks like: { "name": "John" }.
 * Asynchronous           - AJAX requests don't block the rest of your code; response comes back
 *                          later, which is why you need callbacks.
 *
 * GENERAL JS TIPS
 * console.log()          - Print values to the browser dev tools console. Best debugging tool.
 * $(this)                - Inside a jQuery event handler, refers to the element that was clicked/triggered.
 * Semicolons             - Not always required but good habit to use them.
 * Quotes                 - Single and double quotes both work in JS, just be consistent.
 * 
 * 
 * ASP.NET MVC SPECIFIC
 * @Url.Action()          - Generate AJAX endpoint URLs instead of hardcoding them,
 *                          e.g. url: '@Url.Action("MethodName", "ControllerName")'.
 * JsonResult             - AJAX-targeted controller methods should return this,
 *                          using return Json(data) instead of a view.
 * [HttpPost] / [HttpGet] - Decorate controller actions to match your AJAX request type.
 * Antiforgery Token      - Required for POST requests. Add @Html.AntiForgeryToken() to
 *                          your form and pass it with your AJAX call or POST will be rejected.
 * JSON.stringify()       - When sending data via $.ajax(), stringify your JS object first
 *                          and set contentType: 'application/json'.
 *
 * ROUTING
 * URL structure          - AJAX URLs must match MVC routing conventions (/Controller/Action),
 *                          wrong URL will result in a 404.
 */