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
        link.innerHTML = prop.sup;//this is supply
        var details = listing.appendChild(document.createElement('div'));
        details.innerHTML = prop.des;
        if (prop.dat) {
            details.innerHTML += ' &middot; ' + prop.dat;
        }

        //link.addEventListener('click', function (e) {
        //    var clickedListing = data.features[this.dataPosition];
        //    flyToStore(clickedListing);
        //    createPopUp(clickedListing);
        //    var activeItem = document.getElementsByClassName('active');
        //    if (activeItem[0]) {
        //        activeItem[0].classList.remove('active');
        //    }
        //    this.parentNode.classList.add('active');
        //});
    }
}