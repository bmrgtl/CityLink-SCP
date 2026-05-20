document.addEventListener("DOMContentLoaded", () => {


    // CAROUSEL
    function initCarousel(trackId, prevId, nextId) {
        const track = document.getElementById(trackId);
        if (!track) return;

        const prev = document.getElementById(prevId);
        const next = document.getElementById(nextId);

        let scrollAmount = 0;

        next?.addEventListener("click", () => {
            scrollAmount += 300;
            track.scrollTo({ left: scrollAmount, behavior: "smooth" });
        });

        prev?.addEventListener("click", () => {
            scrollAmount -= 300;
            track.scrollTo({ left: scrollAmount, behavior: "smooth" });
        });
    }

    initCarousel("servicesTrack", "servicesPrev", "servicesNext");
    initCarousel("eventsTrack", "eventsPrev", "eventsNext");
    initFAQAccordian();
});

function initFAQAccordian() {
    const faqTriggers = document.querySelectorAll(".faq__trigger");

    faqTriggers.forEach(trigger => {
        trigger.addEventListener("click", () => {
            const panel = document.getElementById(
                trigger.getAttribute("aria-controls")
            );

            const isOpen = trigger.getAttribute("aria-expanded") === "true";

            trigger.setAttribute("aria-expanded", !isOpen);

            if (panel) {
                panel.style.maxHeight = isOpen ? null : panel.scrollHeight + "px";
            }
        });
    });
}