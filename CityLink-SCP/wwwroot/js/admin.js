/*
    These functions are commented out because they use Fetch (browser API) to do GET and POST requests
    I prefer Ajax, the syntax is cleaner and easier IMO, so I rewrote them below
    but using fetch is fine, 
    so those functions can be uncommented and changed slight to make them work again

    BUT,
    THe MakeCards function expects different return data,
    which wont work with action that returns partial view
*/



/*async function LoadXML() {
    const url = "load"
    try {
        const response = await fetch(url);
        if (response.ok) {
            const data = await response.json();
            makeCards(data.cards);

            const textarea = document.getElementById("xml");
            textarea.value = await data.xml;
        }
    } catch (error) {
        console.error("Error loading data:", error);
    }
}

async function UploadXML() {
    const url = "upload";
    try {
        const xml = document.getElementById("xml").value;
        const response = await fetch(url, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(xml),
        })
        if (response.ok) {
            const data = await response.json();
            makeCards(data.cards);
        }

    } catch (error) {
        console.error("Error loading data:", error);
    }
}
function makeCards(data) {

    const cardsContainer = document.getElementById("cards_test");
    cardsContainer.replaceChildren();

        data.forEach(item => {
        // <article class="card">
        const article = document.createElement("article");
        article.classList.add("card");

        // <div class="placeholder placeholder--card"></div>
        const placeholder = document.createElement("div");
        placeholder.classList.add("placeholder", "placeholder--card");

        // <div class="card__body">
        const body = document.createElement("div");
        body.classList.add("card__body");

        // <h3 class="card__title">
        const title = document.createElement("h3");
        title.classList.add("card__title");
        title.textContent = item.title;

        // <p class="card__text">
        const text = document.createElement("p");
        text.classList.add("card__text");
        text.textContent = item.description;

        // <a class="btn btn--outline">
        const link = document.createElement("a");
        link.classList.add("btn", "btn--outline");
        link.textContent = item.buttonLabel;

        // Build structure
        body.appendChild(title);
        body.appendChild(text);
        body.appendChild(link);

        article.appendChild(placeholder);
        article.appendChild(body);

        // Add to container
        cardsContainer.appendChild(article);
    });
}*/

/*
    Replaced named function with 'EventListeners', apparently its better to keep the html and js separate
    Where before in the <button> it had 'onclick="LoadXML()"', this instead uses JQuery to do this:
        document.querySelector('button').addEventListener('click', doSomething); 
    Which is better for "Separatoin of Concerns", html doesnt have js function names in it, which may change
*/
$("#loadXML").on("click", function() {
    $.ajax({
        url: "loadxml",
        type: "GET",
        dataType: "text",
        success: function (data) {
            $("#xmlEditText").val(data);
        }
    });
});

$("#loadCards").on("click", function() {
    $.ajax({
        url: "loadcards",
        type: "GET",
        success: function (html) {
            $("#cards_test").html(html);
        }
    });
});

$("#saveXML").on("click", function() {
    // Set variable to -> Get element with id -> Get its value
    var xml = $("#xmlEditText").val();

    $.ajax({
        url: "uploadcards",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(xml),
        // If the request returned a success code
        success: function (html) {
            // Get this element ->  set the html content of it -> to the html returned by the request
            $("#cards_test").html(html);
        }
    });
});

