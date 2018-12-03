function openBreaker(x) {
    //alert(x);
    ticker.server.openBreaker(x).done(function (info) {
        //alert(info);
    });
}

function closeBreaker(x) {
    //alert(x);
    ticker.server.closeBreaker(x).done(function (info) {
        //alert(info);
    });
}