function applyMargins() {
    var leftToggler = $(".mini-submenu-left");
    var rightToggler = $(".mini-submenu-right");
    if (rightToggler.is(":visible")) {
        $("#map .mapboxgl-ctrl-group")
            .css("margin-right", 10)
            .css("margin-top", 60)
    } else {
        $("#map .mapboxgl-ctrl-group")
            .css("margin-right", $(".sidebar-right").width() + 5)
            .css("margin-top", 10)
    }
}
function isConstrained() {
    return $("div.mid").width() == $(window).width();
}
function applyInitialUIState() {
    if (isConstrained()) {
        $(".sidebar-left .sidebar-body").fadeOut('slide');
        $(".sidebar-right .sidebar-body").fadeOut('slide');
        $('.mini-submenu-left').fadeIn();
        $('.mini-submenu-right').fadeIn();
    }
}

//here start related functions
$('.sidebar-left .slide-submenu').on('click', function () {
    var thisEl = $(this);
    thisEl.closest('.sidebar-body').fadeOut('slide', function () {
        $('.mini-submenu-left').fadeIn();
        applyMargins();
    });
});
$('.mini-submenu-left').on('click', function () {
    var thisEl = $(this);
    $('.sidebar-left .sidebar-body').toggle('slide');
    thisEl.hide();
    applyMargins();
});
$('.sidebar-right .slide-submenu').on('click', function () {
    var thisEl = $(this);
    thisEl.closest('.sidebar-body').fadeOut('slide', function () {
        $('.mini-submenu-right').fadeIn();
        applyMargins();
    });
});
$('.mini-submenu-right').on('click', function () {
    var thisEl = $(this);
    $('.sidebar-right .sidebar-body').toggle('slide');
    thisEl.hide();
    applyMargins();
});
$(window).on("resize", applyMargins);
