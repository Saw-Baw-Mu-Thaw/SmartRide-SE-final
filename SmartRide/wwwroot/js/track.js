"use strict";

var map;

var driverMarker;
var destMarker;
var latitude;
var longitude;
var polyline = null;

var rideId = $('#rideId').val();

// hide arrival btn
$('#arrivalBtn').hide();

// setting up map
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

// connecting to signalR hub
var connection = new signalR.HubConnectionBuilder().withUrl("/trackHub").build();

connection.on("UpdateMap", function (response) {

    var res = JSON.parse(response);


    var time = res['paths'][0]['time']
    var distance = res['paths'][0]['distance']
    time = formatTime(time)
    distance = formatDistance(distance)
    $('#distance').text(distance)
    $('#time').text(time)

    // draw polyline on map
    var latlngs = []
    var points = res['paths'][0]['points']['coordinates']; // this is in long, lat format

    // reverse points to get lat, long format
    for (var i = 0; i < points.length; i++) {
        latlngs.push([points[i][1], points[i][0]]);
    }
    if (polyline != null) {
        map.removeLayer(polyline);
        map.removeLayer(driverMarker);
        map.removeLayer(destMarker);
    }

    polyline = L.polyline(latlngs, { color: 'blue' }).addTo(map);
    driverMarker = L.marker(latlngs[0]).addTo(map);
    destMarker = L.marker(latlngs[latlngs.length - 1]).addTo(map);

    map.fitbounds(polyline.getBounds());
})

connection.on("pickupComplete", function () {
    $('#arrivalBtn').show();
    $('#cancelBtn').hide();
})

connection.start().then(function () {
    connection.invoke("JoinGroup", "RideGroup" + rideId.toString()).catch(function (err) {
        return console.error(err.toString());
    })
})

// time and distance formatting functions
function formatTime(time) {
    time = time / 1000; // convert milliseconds to seconds
    if (time > 3600) {
        return "Time : " + Math.round(time / 3600).toString() + " Hours";
    } else if (time > 60) {
        return "Time : " + Math.round(time / 60).toString() + " Minutes";
    } else {
        return "Time : " + time.toString() + " Seconds";
    }
}

function formatDistance(distance) {
    if (distance > 1000) {
        return "Distance : " + (distance / 1000).toFixed(2) + " km";
    } else {
        return "Distance : " + distance.toFixed(2) + " m";
    }
}