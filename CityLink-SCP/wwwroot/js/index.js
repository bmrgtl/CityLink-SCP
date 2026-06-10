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