function openBreaker(x) {
    //alert(x);
    ticker.server.openBreaker(x).done(function (info) {
        var popUps = document.getElementsByClassName('mapboxgl-popup');
        if (popUps[0]) popUps[0].remove();
    });
}

function closeBreaker(x) {
    //alert(x);
    ticker.server.closeBreaker(x).done(function (info) {
        var popUps = document.getElementsByClassName('mapboxgl-popup');
        if (popUps[0]) popUps[0].remove();
    });
}