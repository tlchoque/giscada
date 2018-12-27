

var results = null;

function flyToPosition(target) {
    //map.flyTo({
    //    // These options control the ending camera position: centered at
    //    // the target, at zoom level 9, and north up.
    //    center: target,
    //    zoom: 20,
    //    bearing: 0,
    //    // These options control the flight curve, making it move
    //    // slowly and zoom out almost completely before starting
    //    // to pan.
    //    speed: 1.2, // make the flying slow
    //    curve: 1, // change the speed at which it zooms out
    //    // This can be any easing function: it takes a number between
    //    // 0 and 1 and returns another number between 0 and 1.
    //    easing: function (t) {
    //        return t;
    //    }
    //});
    map.flyTo({ center: target, zoom: 20 });
}

function autocomplete(inp, arr) {
    var currentFocus;
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;

        a = document.createElement("div");
        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items");

        this.parentNode.appendChild(a);
                        
        var search = new Object();
        search.label = val;
        $.ajax({
            data: JSON.stringify(search),
            type: "POST",
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            url: "Default.aspx/GetTopResults",
            async: false,
            cache: false,
            success: function (data) {
                results = data.d;
            }
        });

        $.each(results, function (i, item) {
            if (item.Name.substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                b = document.createElement("div");
                b.style.fontSize = "small";
                b.innerHTML = item.Category + "<strong> " + item.Name.substr(0, val.length) + "</strong>";
                b.innerHTML += item.Name.substr(val.length);
                b.innerHTML += " (" + item.Feeder + ")";

                b.innerHTML += "<input type='hidden' value='" + item.Name + "'>";
                b.innerHTML += "<input type='hidden' value='" + item.Position.Latitude + "'>";
                b.innerHTML += "<input type='hidden' value='" + item.Position.Longitude + "'>";
                b.addEventListener("click", function (e) {
                    inp.value = this.getElementsByTagName("input")[0].value;
                    closeAllLists();

                    var lat = this.getElementsByTagName("input")[1].value;
                    var lon = this.getElementsByTagName("input")[2].value;
                    var target = [lon,lat];
                    flyToPosition(target);
                    
                });
                a.appendChild(b);
            }
        });

    });

    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            currentFocus++;
            addActive(x);
        } else if (e.keyCode == 38) { 
            currentFocus--;
            addActive(x);
        } else if (e.keyCode == 13) {
            e.preventDefault();
            if (currentFocus > -1) {
                if (x) x[currentFocus].click();
            }
        }
    });
    function addActive(x) {
        if (!x) return false;
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });

}

autocomplete(document.getElementById("myInput"));
