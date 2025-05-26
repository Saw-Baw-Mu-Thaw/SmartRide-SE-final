var map;

var pickupMarker;
var dropoffMarker;
var latitude;
var longitude;

if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(locSuccess, locError);
} else {
    console.error("Geolocation is not supported by this browser");
}

function locSuccess(position) {
    latitude = position.coords.latitude;
    longitude = position.coords.longitude;

    map = L.map('map').setView([latitude, longitude], 13)

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);
}

function locError() {
    alert("Sorry, position not avaliable");
}