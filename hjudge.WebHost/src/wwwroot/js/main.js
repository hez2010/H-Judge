// remove preloader mask if page loaded
document.body.onload = function () {
    const preloader = document.querySelector('#preloader');
    preloader.setAttribute('class', 'fadeout-preloader');
    setTimeout(function () { preloader.remove(); }, 500);
};

