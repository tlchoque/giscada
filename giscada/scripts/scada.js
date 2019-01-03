
var ticker = $.connection.zenonVariableValues;
var breakers = [];
var substations = [];

var map = new mapboxgl.Map({
    container: 'map',
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

map.addControl(new mapboxgl.NavigationControl());
applyInitialUIState();
applyMargins();

function loadImages() {
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
}

function loadClaimLayer(data) {
    map.addSource("claims", {
        "type": "geojson",
        "data": JSON.parse(data)
    });
    map.addLayer({
        "id": "claims",
        "type": "symbol",
        "source": "claims",
        "layout": {
            //"text-field": "{sup}",
            "icon-image": "claim",
            "icon-size": {
                "base": 1.75,
                "stops": [[13, 0.03], [20, 0.1]]
            },
            "icon-allow-overlap": true,
            "text-allow-overlap": true,
        },
        "paint": {
            "text-color": "#00FF00",
            "text-halo-color": "black",
            "text-halo-width": 1,
        }
    });
}

function loadVehicleLayer(data) {
    map.addSource("vehicles", {
        "type": "geojson",
        "data": JSON.parse(data)
    });
    map.addLayer({
        "id": "vehicles",
        "type": "symbol",
        "source": "vehicles",
        "layout": {
            "text-field": "{nam}",
            "icon-image": "car",
            "icon-size": {
                "base": 1.75,
                "stops": [[13, 0.16], [20, 0.55]]
            },
            "icon-allow-overlap": true,
            "text-allow-overlap": true,
        },
        "paint": {
            "text-color": "#0000FF",
            "text-halo-color": "white",
            "text-halo-width": 1,
        }
    });
}

//change style, disable to try the menu

var layerList = document.getElementById('map-style');
var inputs = layerList.getElementsByTagName('a');

function switchLayer(layer) {   
    loadImages();  
    var layerId = layer.target.id;
    map.setStyle('json/' + layerId + '.json');
}

for (var i = 0; i < inputs.length; i++) {
    inputs[i].onclick = switchLayer;
}

map.on('load', function () {
    loadImages();  
})

map.on('style.load', function () {
    map.setPaintProperty("mv_line", 'line-color', [
        "case",
        ["match", ['get', "name"], breakers, true, false], "#ff0000",
        '#00FF00'
    ]);
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

    ticker.server.getInitialClaimLayer().done(function (info) {
        loadClaimLayer(info);
    });

    ticker.server.getInitialVehicleLayer().done(function (info) {
        loadVehicleLayer(info);
    });
})


map.on('click', function (e) {
    var features = map.queryRenderedFeatures(e.point, { layers: ['claims'] });
    if (features.length) {
        var clickedPoint = features[0];
        flyToClaim(clickedPoint);
        createPopUp(clickedPoint);
        var activeItem = document.getElementsByClassName('active');
        if (activeItem[0]) {
            activeItem[0].classList.remove('active');
        }
        var selectedFeature = clickedPoint.properties.des;

        //for (var i = 0; i < stores.features.length; i++) {
        //    if (stores.features[i].properties.des === selectedFeature) {
        //        selectedFeatureIndex = i;
        //    }
        //}
        //var listing = document.getElementById('listing-' + selectedFeatureIndex);
        //listing.classList.add('active');
    }
});

function init() {
    alert("starting"); 

    ticker.server.getInitialClaimLayer().done(function (info) {
        loadClaimLayer(info);
        buildClaimList(JSON.parse(info));
    });

    ticker.server.getInitialVehicleLayer().done(function (info) {
        loadVehicleLayer(info);
    });
    
    ticker.server.getInitialOpenedBreakers().done(function (info) {
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
    //alert(JSON.stringify(data));
    //alert("nerw update");
    map.getSource('vehicles').setData(JSON.parse(data));
}

ticker.client.updateClaims = function (data) {
    map.getSource('claims').setData(JSON.parse(data));
    buildClaimList(JSON.parse(data));
}

$.connection.hub.start().done(init);