var map;

var pickupMarker;
var dropoffMarker;
var pickupLat;
var pickupLong;
var dropoffLat;
var dropoffLong;

$('#veh').val("Car")

$('#vehicleType').on('change',function (e) {
    var selectedValue = $(e.target).val();
    $('#veh').val(selectedValue)
})

if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(locSuccess, locError);
} else {
    dlert("Geolocation is not supported by this browser");
}


function locSuccess(position) {
    var longitude = position.coords.longitude;
    var latitude = position.coords.latitude;

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

function setPickup() {
    if (pickupMarker != null) {
        map.removeLayer(pickupMarker);
    }

    var coords = map.getCenter();
    pickupLat = coords.lat;
    pickupLong = coords.lng;
    $('#pickupLong').val(pickupLong);
    $('#pickupLat').val(pickupLat);
    
    pickupMarker = L.marker([coords.lat, coords.lng]).addTo(map)
    pickupMarker.bindPopup("<b>Pickup Location</b>")
}

function setDropoff() {
    if (dropoffMarker != null) {
        map.removeLayer(dropoffMarker);
    }

    var coords = map.getCenter();
    dropoffLat = coords.lat;
    dropoffLong = coords.lng;
    $('#dropoffLong').val(dropoffLong);
    $('#dropoffLat').val(dropoffLat);
    dropoffMarker = L.marker([coords.lat, coords.lng]).addTo(map)
    dropoffMarker.bindPopup("<b>Dropoff Location</b>")
}

