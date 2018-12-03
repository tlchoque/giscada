
var ticker = $.connection.zenonVariableValues;

var filterBy = ["O-145", "O-284", "O-161", "O-782"];

var map = new mapboxgl.Map({
    container: 'map',
    style: 'http://TESTSERVER:9090/styles/dark-matter/style.json',
    //style: 'http://TESTSERVER:9090/styles/positron/style.json',
    //style: 'http://TESTSERVER:9090/styles/osm-bright/style.json',
    //style: 'http://TESTSERVER:9090/styles/klokantech-basic/style.json',
    center: [0, 0],
    zoom: 7.2,
    maxZoom: 20,
    // pitch: 45,
    // bearing: -17.6,
});

map.addControl(new mapboxgl.NavigationControl());

var setCenterFromLayer = true;

map.on('load', function () {

    map.addSource('giscada', {
        type: 'vector',
        tiles: ["http://TESTSERVER:8080/tileserver-php-master/tileserver.php?/index.json?/final_3/{z}/{x}/{y}.pbf"]
    });

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
    
    var layers = map.getStyle().layers;
    var firstSymbolId;
    for (var i = 0; i < layers.length; i++) {
        if (layers[i].type === 'symbol') {
            firstSymbolId = layers[i].id;
            break;
        }
    }
    firstSymbolId = "highway_name_other";

    map.addLayer({
        "id": "mv_line",
        "type": "line",
        "source": "giscada",
        "source-layer": "mv_line",
        "interactive": true,
        "paint": {
            "line-color": "#00FF00",
            //"line-color": [
            //    "case",
            //    //["match",['get', "feed"],["O-145","O-284"],true,false], "#ff0000", 
            //    ["match", ['get', "feed"], filterBy, true, false], "#ff0000",
            //    //["==", "$type", "LineString"], "#ff0000",
            //    '#00FF00'
            //],

            "line-width": {
                "base": 1.75,
                "stops": [[0, 3.5], [20, 6]]
            },

        }
    }, firstSymbolId);

    map.addLayer({
        "id": "lv_line",
        "type": "line",
        "source": "giscada",
        "source-layer": "lv_line",
        "interactive": true,
        "paint": {
            "line-color": "#ffff00",
            // "line-color":[
            //     "case", 
            //     ['==', ['get', "feed"], "O-145"], "#ff6600", 
            //     '#ffff00'
            //   ],

            //"line-color": [
            //    "case",
            //    ['==', ['get', "name"], "1122"], "#ff6600",
            //    '#ffff00'
            //],
            "line-width": {
                "base": 1.75,
                "stops": [[11, 1], [20, 2]]
            },
        }
    }, "mv_line");

    map.addLayer({
        "id": "lv_service_line",
        "type": "line",
        "source": "giscada",
        "source-layer": "lv_service_line",
        "interactive": true,
        "paint": {
            "line-color": "#ffff00",
            // "line-color":[
            //     "case", 
            //     ['==', ['get', "feed"], "O-145"], "#ff6600", 
            //     '#ffff00'
            //   ],
            //"line-color": [
            //    "case",
            //    ['==', ['get', "name"], "1122"], "#ff6600",
            //    '#ffff00'
            //],
            "line-width": {
                "base": 1.75,
                "stops": [[13, 1], [20, 4]]
            },
        }
    }, "mv_line");

    map.addLayer({
        "id": "breaker",
        "type": "symbol",
        "source": "giscada",
        "source-layer": "breaker",
        //"interactive": true,
        // "paint": {
        //   "circle-color": "#0000FF"
        //   }
        "layout": {
            "text-field": "{short}",
            "icon-image": "breaker",
            //"icon-size": 0.03,
            "icon-size": {
                "base": 1.75,
                "stops": [[15, 0.03], [20, 0.14]]
            },
            "icon-allow-overlap": true,
            //"text-size":9,
            "text-size": {
                "base": 1.75,
                "stops": [[13, 10], [20, 21]]
            },
            'icon-offset': [0, -300],
            "text-font": ["Noto Sans Regular"],
            "text-allow-overlap": true,
            //"text-offset": [0,-300],
            //"text-offset": [0, -3]
        },
        "paint": {
            "text-color": "#00FF00",
            "text-halo-color": "black",
            "text-halo-width": 1,

        }
    });

    map.addLayer({
        "id": "substation",
        "type": "symbol",
        "source": "giscada",
        "source-layer": "substation",
        //"interactive": true,
        // "paint": {
        //   "circle-color": "#0000FF"
        //   }
        "layout": {
            "icon-image": "triangle",
            //"icon-size": 0.11,
            "icon-size": {
                "base": 1.75,
                "stops": [[14, 0.08], [20, 0.4]]
            },
            "icon-allow-overlap": true
        }
    }, firstSymbolId);

    map.addLayer({
        "id": "substation_text",
        "type": "symbol",
        "source": "giscada",
        "source-layer": "substation_text",
        "layout": {
            "text-field": "{name}",
            "icon-allow-overlap": true,
            "text-font": ["Open Sans"],
            //"text-size":12,
            "text-size": {
                "base": 1.75,
                "stops": [[15, 10], [20, 18]]
            },
        },
        "paint": {
            // "text-color": "#0000FF",
            // "text-halo-color": "#fff",
            // "text-halo-width": 1
            //"fill-outline-color": "white"
            "text-color": "#00ccff",
        }
    });

    map.addLayer({
        "id": "mv_pole",
        "type": "circle",
        "source": "giscada",
        "source-layer": "mv_pole",
        "interactive": true,
        "paint": {
            "circle-color": "#84299e",
            //"circle-radius": 4,
            "circle-radius": {
                "base": 1.75,
                "stops": [[15, 2], [20, 9]]
            },
        },
        "minzoom": 15
    }, "substation");

    map.addLayer({
        "id": "lv_pole",
        "type": "circle",
        "source": "giscada",
        "source-layer": "lv_pole",
        "interactive": true,
        "paint": {
            "circle-color": "#ff00ff",
            "circle-radius": 3
        },
        "minzoom": 17,
    }, "mv_pole");

    map.addLayer({
        "id": "mv_pole_text",
        "type": "symbol",
        "source": "giscada",
        "source-layer": "mv_pole_text",
        "layout": {
            "text-field": "{name}",
            "icon-allow-overlap": true,
            "text-font": ["Open Sans"],
            "text-size": {
                "base": 1.75,
                "stops": [[16, 8], [20, 17]]
            },
        },
        "paint": {
            //"text-color": "#ffffff",
            "text-color": "#84299e",
            // "text-halo-color": "#fff",
            // "text-halo-width": 1
        }
    });

    map.addLayer({
        "id": "lv_pole_text",
        "type": "symbol",
        "source": "giscada",
        "source-layer": "lv_pole_text",
        "layout": {
            "text-field": "{name}",
            "icon-allow-overlap": true,
            "text-font": ["Open Sans"],
            "text-size": 9,
        },
        "paint": {
            "text-color": "#ff00ff",
            // "text-color":[
            //     "case", 
            //     ['==', obj['get', "feed"], 1], "#00ff00", 
            //     ['==', obj['get', "feed"], 2], "#0000ff",
            //     '#ff0000'
            //   ],

            // "text-halo-color": "#fff",
            // "text-halo-width": 1,
            //"text-max-width": 100 px 
        },
        "minzoom": 18,
    });

    map.addLayer({
        "id": "client",
        "type": "circle",
        "source": "giscada",
        "source-layer": "client",
        "interactive": true,
        "paint": {
            "circle-color": "#123c82",
            "circle-radius": 2.5
        }
    });

    map.addLayer({
        "id": "client_text",
        "type": "symbol",
        "source": "giscada",
        "source-layer": "client_text",
        "layout": {
            "text-field": "{name}",
            "icon-allow-overlap": true,
            "text-font": ["Open Sans"],
            "text-size": 9,
        },
        "paint": {
            // "text-color": "#123c82",
            // "text-halo-color": "#fff",
            // "text-halo-width": 1,
            "text-color": "white"
            //"text-max-width": 100 px 
        }
    });

    var layers2 = map.getStyle().layers;

    var labelLayerId2;
    for (var i = 0; i < layers2.length; i++) {
        if (layers2[i].type === 'symbol' && layers2[i].layout['text-field']) {
            labelLayerId2 = layers[i].id;
            break;
        }
    }

    map.addLayer({
        'id': '3d-buildings',
        'source': 'openmaptiles',
        'source-layer': 'building',
        'type': 'fill-extrusion',
        'minzoom': 15,
        'filter': ["!=", ['id'], -1],
        'paint': {
            'fill-extrusion-color': '#aaa',
            // use an 'interpolate' expression to add a smooth transition effect to the
            // buildings as the user zooms in
            'fill-extrusion-height': [
                "interpolate", ["linear"], ["zoom"],
                15, 0,
                15.05, ["get", "render_height"]
            ],
            'fill-extrusion-base': [
                "interpolate", ["linear"], ["zoom"],
                15, 0,
                15.05, ["get", "min_height"]
            ],
            'fill-extrusion-opacity': .6
        }
    }, labelLayerId2);

    if (setCenterFromLayer) {
        var center = tileJSON['center'];
        if (typeof center == 'string') {
            center = center.split(',');
        }
        map.setCenter([parseFloat(center[0]), parseFloat(center[1])]);
        //map.setZoom(parseInt(center[2], 10));
    }
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
        //alert(JSON.stringify(data));
        map.setPaintProperty("lv_line", 'line-color', [
            "case",
            ["match", ['get', "name"], substations, true, false], "#ff6600",
            '#ffff00'
        ]);
        map.setPaintProperty("lv_service_line", 'line-color', [
            "case",
            ["match", ['get', "name"], substations, true, false], "#ff6600",
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
    //alert(JSON.stringify(data));
    map.setPaintProperty("lv_line", 'line-color', [
        "case",
        ["match", ['get', "name"], substations, true, false], "#ff6600",
        '#ffff00'
    ]);
    map.setPaintProperty("lv_service_line", 'line-color', [
        "case",
        ["match", ['get', "name"], substations, true, false], "#ff6600",
        '#ffff00'
    ]);
};

ticker.client.updateVehicles = function (data) {
    //alert("vehicles");
    map.getSource('vehicles').setData(JSON.parse(data));
}

$.connection.hub.start().done(init);