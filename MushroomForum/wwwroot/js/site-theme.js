// Inicjalizacja Bootstrapowych dropdownów
var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
    return new bootstrap.Dropdown(dropdownToggleEl);
});
(function () {
    const savedTheme = localStorage.getItem('theme') || 'light';
    const stylesheetHref = savedTheme === 'dark' ? '/css/site2.css' : '/css/site.css';
    const bannerSrc = savedTheme === 'dark' ? '/images/baner-dark.png' : '/images/baner-light.png';

    document.write('<link id="themeStylesheet" rel="stylesheet" href="' + stylesheetHref + '" asp-append-version="true" />');
    window.__bannerSrc = bannerSrc;
})();
document.addEventListener('DOMContentLoaded', () => {
    const banner = document.getElementById('bannerImage');
    if (banner && window.__bannerSrc) {
        banner.src = window.__bannerSrc;
    }
});
// Funkcja zmieniająca motyw
function toggleTheme() {
    const link = document.getElementById('themeStylesheet');
    const bannerImg = document.querySelector('.banner img');
    const currentHref = link.getAttribute('href');

    if (currentHref.includes('site.css') && !currentHref.includes('site2.css')) {
        link.setAttribute('href', '/css/site2.css');
        localStorage.setItem('theme', 'dark');
        if (bannerImg) {
            bannerImg.src = '/images/baner-dark.png';
        }
    } else {
        link.setAttribute('href', '/css/site.css');
        localStorage.setItem('theme', 'light');
        if (bannerImg) {
            bannerImg.src = '/images/baner-light.png';
        }
    }
}
// Funkcja ustawiająca motyw przy starcie strony
function applySavedTheme() {
    const savedTheme = localStorage.getItem('theme');
    const link = document.getElementById('themeStylesheet');
    const bannerImg = document.querySelector('.banner img');

    if (savedTheme === 'dark') {
        link.setAttribute('href', '/css/site2.css');
        if (bannerImg) {
            bannerImg.src = '/images/baner-dark.png';
        }
    } else {
        link.setAttribute('href', '/css/site.css');
        if (bannerImg) {
            bannerImg.src = '/images/baner-light.png';
        }
    }
}
