var map;

var pickupMarker;
var dropoffMarker;
var pickupLat;
var pickupLong;
var dropoffLat;
var dropoffLong;


var test = document.getElementById('test');

$('#veh').val("Car")

$('#vehicleType').on('change',function (e) {
    var selectedValue = $(e.target).val();
    console.log(selectedValue)
    $('#veh').val(selectedValue)
})

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

function setPickup() {
    if (pickupMarker != null) {
        map.removeLayer(pickupMarker);
    }

    var coords = map.getCenter();
    pickupLat = coords.lat;
    pickupLong = coords.lng;
    $('#pickupLong').val(pickupLong);
    $('#pickupLat').val(pickupLat);
    console.log('Longitude : ' + coords.lng + ', Latitude : ' + coords.lat)
    test.append('<p>Pickup Longitude : ' + coords.lng + ', Pickup Latitude : ' + coords.lat + '</p>')
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
    console.log('Longitude : ' + coords.lng + ', Latitude : ' + coords.lat)
    test.append('<p>Dropoff Longitude : ' + coords.lng + ', Dropoff Latitude : ' + coords.lat + '</p>')
    dropoffMarker = L.marker([coords.lat, coords.lng]).addTo(map)
    dropoffMarker.bindPopup("<b>Dropoff Location</b>")
}

