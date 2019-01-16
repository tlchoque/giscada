function flyToClaim(currentFeature) {
    map.flyTo({
        center: currentFeature.geometry.coordinates,
        zoom: 20
    });
}

function createPopUp(currentFeature) {
    var popUps = document.getElementsByClassName('mapboxgl-popup');
    if (popUps[0]) popUps[0].remove();
    var popup = new mapboxgl.Popup({ closeOnClick: false })
        .setLngLat(currentFeature.geometry.coordinates)
        .setHTML('<h3>' + currentFeature.properties.cod +'</h3>' +
        '<h4>' + currentFeature.properties.des + ' &middot; ' + currentFeature.properties.dat  +'</h4>')
        .addTo(map);
}


function buildClaimList(data) {
    document.getElementById('listings').innerHTML = "";

    for (i = 0; i < data.features.length; i++) {
        var currentFeature = data.features[i];
        var prop = currentFeature.properties;
        var listings = document.getElementById('listings');
        var listing = listings.appendChild(document.createElement('div'));
        listing.className = 'item';
        listing.id = 'listing-' + i;
        var link = listing.appendChild(document.createElement('a'));
        link.href = '#';
        link.className = 'title';
        link.dataPosition = i;
        link.innerHTML = prop.cod + ' - ' + prop.sup;//this is supply
        var details = listing.appendChild(document.createElement('div'));
        details.innerHTML = prop.des;
        if (prop.dat) {
            details.innerHTML += ' &middot; ' + prop.dat;
        }

        link.addEventListener('click', function (e) {
            var clickedListing = data.features[this.dataPosition];
            flyToClaim(clickedListing);
            createPopUp(clickedListing);
            var activeItem = document.getElementsByClassName('active');
            if (activeItem[0]) {
                activeItem[0].classList.remove('active');
            }
            this.parentNode.classList.add('active');
        });
    }
}