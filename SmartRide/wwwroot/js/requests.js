var map;

var latitude = 10.732812886889537;
var longitude = 106.69987052679063;

var test = document.getElementById('test');

if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(locSuccess, locError);
} else {
    test.innerText = "Geolocation is not supported by this browser";
}


function locSuccess(position) {
    // longitude = position.coords.longitude;
    // latitude = position.coords.latitude;
    //console.log("Latitude:" + latitude + "\nLongtitude:" + longitude);
    //test.innerText = "Latitude:" + latitude + "\nLongtitude:" + longitude;

    map = L.map('map').setView([latitude, longitude], 13)

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    var marker = L.marker([latitude, longitude]).addTo(map)
    marker.bindPopup("<b>You are here</p>")
}

function locError() {
    alert("Sorry, position not avaliable");
}


