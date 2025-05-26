var map;

var pickupMarker;
var dropoffMarker;
var pickupLat;
var pickupLong;
var dropoffLat;
var dropoffLong;

var test = document.getElementById('test');

if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(locSuccess, locError);
} else {
    test.innerText = "Geolocation is not supported by this browser";
}


function locSuccess(position) {
    var longitude = position.coords.longitude;
    var latitude = position.coords.latitude;
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


