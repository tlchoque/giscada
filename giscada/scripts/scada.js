﻿
var ticker = $.connection.zenonVariableValues;

var map = new mapboxgl.Map({
    container: 'map',
    //style: 'json/dark-matter.json',
    style: 'json/dark-matter.json',
    center: [
        -70.69376,
        -16.806349,
        20
    ],
    zoom: 7.2,
    maxZoom: 20,
    // pitch: 45,
    // bearing: -17.6,
});


//change style
var layerList = document.getElementById('menu');
var inputs = layerList.getElementsByTagName('input');

function switchLayer(layer) {
    var layerId = layer.target.id;
    map.setStyle('json/' + layerId + '.json');
}

for (var i = 0; i < inputs.length; i++) {
    inputs[i].onclick = switchLayer;
}

map.addControl(new mapboxgl.NavigationControl());

map.on('load', function () {

    map.loadImage('img/t100.png', function (error, image) {
        if (error) throw error;
        map.addImage('triangle', image);
    });

    map.loadImage('img/breaker.png', function (error, image) {
        if (error) throw error;
        map.addImage('breaker', image);
    });

    map.loadImage('img/van.png', function (error, image) {
        if (error) throw error;
        map.addImage('car', image);
    });

    map.loadImage('img/blue.png', function (error, image) {
        if (error) throw error;
        map.addImage('claim', image);
    });

    map.loadImage('img/red.png', function (error, image) {
        if (error) throw error;
        map.addImage('claimr', image);
    });

})

var breakers = [];
var substations = [];
function init() {
    alert("starting"); 

    //ticker.server.getInitialLayer().done(function (info) {
    //    //alert(JSON.stringify(info));
    //    map.addSource("vehicles", {
    //        "type": "geojson",
    //        "data": JSON.parse(info)
    //    });
    //    map.addLayer({
    //        "id": "vehicles",
    //        "type": "symbol",
    //        "source": "vehicles",
    //        "layout": {
    //            "text-field": "{plate}",
    //            "icon-image": "car",
    //            "icon-size": {
    //                "base": 1.75,
    //                "stops": [[14, 0.08], [20, 0.4]]
    //            },
    //            "icon-allow-overlap": true,
    //            "text-allow-overlap": true,
    //        },
    //        "paint": {
    //            "text-color": "#00FF00",
    //            "text-halo-color": "black",
    //            "text-halo-width": 1,

    //        }
    //    });
    //    //alert("ola k ase again"); 
    //});

    ticker.server.getInitialOpenedBreakers().done(function (info) {
        //alert(JSON.stringify(info));
        breakers = ["bazinga"];
        for (var i = 0; i < info.length; i++) {
            var obj = info[i];
            breakers.push(obj.Name);
        }
        map.setPaintProperty("mv_line", 'line-color', [
            "case",
            ["match", ['get', "name"], breakers, true, false], "#ff0000",
            '#00FF00'
        ]);
    });

    ticker.server.getInitialOpenedSubstations().done(function (info) {
        substations = ["bazinga"];
        for (var i = 0; i < info.length; i++) {
            var obj = info[i];
            substations.push(obj.Name);
        }

        map.setPaintProperty("lv_line", 'line-color', [
            "case",
            ["match", ['get', "sub"], substations, true, false], "#ff6600",
            '#ffff00'
        ]);
        map.setPaintProperty("lv_service_line", 'line-color', [
            "case",
            ["match", ['get', "sub"], substations, true, false], "#ff6600",
            '#ffff00'
        ]);
    });

}

ticker.client.updateColorLines = function (data) {
    breakers = ["bazinga"];
    for (var i = 0; i < data.length; i++) {
        var obj = data[i];
        breakers.push(obj.Name);
    }
    map.setPaintProperty("mv_line", 'line-color', [
        "case",
        ["match", ['get', "name"], breakers, true, false], "#ff0000",
        '#00FF00'
    ]);
};

ticker.client.updateLvLines = function (data) {
    substations = ["bazinga"];
    for (var i = 0; i < data.length; i++) {
        var obj = data[i];
        substations.push(obj.Name);
    }

    map.setPaintProperty("lv_line", 'line-color', [
        "case",
        ["match", ['get', "sub"], substations, true, false], "#ff6600",
        '#ffff00'
    ]);
    map.setPaintProperty("lv_service_line", 'line-color', [
        "case",
        ["match", ['get', "sub"], substations, true, false], "#ff6600",
        '#ffff00'
    ]);
};

ticker.client.updateVehicles = function (data) {
    map.getSource('vehicles').setData(JSON.parse(data));
}

$.connection.hub.start().done(init);