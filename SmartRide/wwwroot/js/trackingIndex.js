"use strict";

var map;

var driverMarker;
var destMarker;

var rideId = $("#rideId").val()
var finishedPickup = 0;
var vehicleType = $('#vehicleType').val();

var interval = null;
var polyline = null;

// this is hardcorded because driver might not be on the street right now
var latitude = 10.732812886889537;
var longitude = 106.69987052679063;

// setting up map
if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(locSuccess, locError);
} else {
    console.error("Geolocation is not supported by this browser");
}

function locSuccess(position) {
    //latitude = position.coords.latitude;
    //longitude = position.coords.longitude;

    map = L.map('map').setView([latitude, longitude], 13)

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);
}

function locError() {
    alert("Sorry, position not avaliable");
}

// setting up button functions
function completePickup(e) {
    finishedPickup = 1;
    $(e.target).prop("disabled", true);
}

function completeDropoff(e) {
    // todo : clear interval
    clearInterval(interval);
}

// connecting to signalR hub
var connection = new signalR.HubConnectionBuilder().withUrl("/trackHub").build();

connection.on("UpdateMap", function (response)
{
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

 
    var cancelled = res['cancelled'];
    if (cancelled) {
        alert("Ride has been cancelled by customer");
        clearInterval(interval);
        setTimeout(function () {
            window.location.replace("/Home/Index");
        }, 3000);
    }


    polyline = L.polyline(latlngs, { color: 'blue' }).addTo(map);
    driverMarker = L.marker(latlngs[0]).addTo(map);
    destMarker = L.marker(latlngs[latlngs.length - 1]).addTo(map);

    map.fitbounds(polyline.getBounds());

    
})

connection.on("pickupComplete", function () {
    finishedPickup = 2
})

//join a ride group after starting connection
connection.start().then(function () {
    connection.invoke("JoinGroup", "RideGroup" + rideId.toString()).catch(function (err) {
        return console.error(err.toString());
    })
})

if (interval == null) {
    interval = setInterval(function () {
        var driverMsg = {
            finishedPickup: finishedPickup,
            vehicleType: (vehicleType === "Car") ? "car" : "bike",
            rideId: rideId,
            longitude: longitude,
            latitude: latitude
        }
        connection.invoke("SendRoute", JSON.stringify(driverMsg)).catch(function (err) {
            return console.error(err.toString());
        });
    }, 10000);
    // timer is set to 10 seconds because both customer and driver should be in track view
}

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