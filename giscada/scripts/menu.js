
map.on('contextmenu', 'breaker', function (e) {

    var popUps = document.getElementsByClassName('mapboxgl-popup');
    // Check if there is already a popup on the map and if so, remove it
    if (popUps[0]) popUps[0].remove();


    var coordinates = e.features[0].geometry.coordinates.slice();
    //var description = "ola k ase";
    var description = e.features[0].properties.short;
    var variable = e.features[0].properties.name;

    // Ensure that if the map is zoomed out such that multiple
    // copies of the feature are visible, the popup appears
    // over the copy being pointed to.
    while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
        coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
    }

    new mapboxgl.Popup()
        .setLngLat(coordinates)
        //.setHTML('<h3>' + description + '</h3> <h4>Abrir</h4>' + '<h4>Cerrar</h4>')
        .setHTML('<h3>' + description + '</h3> <ul><li><a onclick="openBreaker(\'' + variable + '\');" >Abrir</a></li>' + '<li><a onclick="closeBreaker(\'' + variable +'\');" >Cerrar</a></li></ul>')
        .addTo(map);
});