
/* ── Mobile nav ── */
const menuBtn = document.getElementById('menuBtn');
const mobileMenu = document.getElementById('mobileMenu');

menuBtn.addEventListener('click', () => {
    const open = mobileMenu.classList.toggle('open');
    menuBtn.setAttribute('aria-expanded', open);
});

// Close when a link is tapped
mobileMenu.querySelectorAll('a').forEach(a =>
    a.addEventListener('click', () => mobileMenu.classList.remove('open'))
);