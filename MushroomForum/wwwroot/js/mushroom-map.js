const map = L.map('map').setView([52.0, 19.0], 6);
const places = [];
let userMarker = null;
let routeLine = null;
let userLocation = null;
const orsApiKey = "";
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors',
    crossOrigin: true
}).addTo(map);
const markers = [];
places.forEach((place, index) => {
    const marker = L.marker([place.lat, place.lng]).addTo(map)
        .bindPopup(`<strong>${place.name}</strong><br>${place.description}<br>Ocena: ${place.rating}`);
    markers.push(marker);
});
function clearSpotsFromMap() {
    map.eachLayer(layer => {
        if ((layer instanceof L.Marker || layer instanceof L.CircleMarker || layer instanceof L.Polyline) &&
            layer !== userMarker && layer._url === undefined) {
            map.removeLayer(layer);
        }
    });

    if (routeLine) {
        map.removeLayer(routeLine);
        routeLine = null;
    }

    map.closePopup();
}
function loadPins() {
    const url = `/MushroomSpots/GetAll?ts=${Date.now()}`; // dodanie znacznika czasu

    fetch(url)
        .then(res => res.json())
        .then(spots => {
            spots.forEach(spot => {
                const marker = L.marker([spot.latitude, spot.longitude])
                    .bindTooltip(spot.name, { permanent: true, direction: 'top', offset: [0, -10] })
                    .addTo(map);

                let rating = parseInt(spot.rating) || 0;
                let stars = '★'.repeat(rating) + '☆'.repeat(5 - rating);

                marker.bindPopup(`
                            <div id="popup-${spot.id}">
                                <b>${spot.name}</b><br>
                                ${spot.description}<br><br>
                                <div>${stars}</div>
                                <form onsubmit="deleteSpot(event, ${spot.id})">
                                    <button type="submit" class="nav-link nav-button-green">Usuń</button>
                                </form>
                                <button type="button" class="nav-link nav-button-green"
                                    onclick="event.stopPropagation(); showEditForm(${spot.id}, '${spot.name}', '${spot.description.replace(/'/g, "\\'")}', ${spot.rating})">
                                    Edytuj</button>
                                       <button class="nav-link nav-button-green" onclick="drawRoute(${spot.longitude}, ${spot.latitude})">Wyznacz trasę</button>
                            </div>
                        `);
            });
            updatePlaceList(spots);
        });
}

function showEditForm(id, name, description, rating = 0) {
    const container = document.getElementById(`popup-${id}`);
    if (!container) return;

    const safeName = name.replace(/"/g, '&quot;');
    const safeDescription = description
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    container.innerHTML = `
                <form onsubmit="editSpot(event, ${id})" onclick="event.stopPropagation()">
                    <input type="text" name="name" value="${safeName}" required /><br>
                    <textarea name="description">${safeDescription}</textarea><br>

                    <label>Ocena:</label>
                    <div class="star-rating">
                        ${[5, 4, 3, 2, 1].map(star => `
                            <input type="radio" name="rating" value="${star}" id="edit-star-${star}-${id}" ${rating === star ? 'checked' : ''}>
                            <label for="edit-star-${star}-${id}">★</label>
                        `).join('')}
                    </div>

                    <button class="nav-link nav-button-green" type="submit">Zapisz</button>
                </form>
            `;
}

function editSpot(event, id) {
    event.preventDefault();
    const form = event.target;
    const name = form.name.value;
    const description = form.description.value;
    const rating = parseInt(form.rating.value);


    fetch('/MushroomSpots/Edit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id, name, description, rating })


    }).then(res => {
        if (res.ok) {
            clearSpotsFromMap();
            setTimeout(() => {
                loadPins();
                map.closePopup();
            }, 30);
        }
        else {
            alert("Nie udało się zaktualizować pinezki.");
        }
    });
}

function deleteSpot(event, id) {
    event.preventDefault();
    if (confirm("Czy na pewno chcesz usunąć to miejsce?")) {
        fetch('/MushroomSpots/Delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(id)
        }).then(res => {
            if (res.ok) {
                clearSpotsFromMap();
                setTimeout(() => {
                    loadPins();
                    map.closePopup();
                }, 30);
            }
            else {
                alert("Nie udało się usunąć pinezki.");
            }
        });
    }
}
function drawRoute(destLng, destLat) {
    if (!userLocation) {
        alert("Nieznana lokalizacja użytkownika.");
        return;
    }

    const url = "https://api.openrouteservice.org/v2/directions/driving-car";

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + orsApiKey
        },
        body: JSON.stringify({
            coordinates: [
                [userLocation[1], userLocation[0]],  // [lng, lat] użytkownika
                [destLng, destLat]                   // [lng, lat celu]
            ],
            instructions: false,
            geometry: true
        })
    })
        .then(res => {
            if (!res.ok) throw new Error("Błąd odpowiedzi z serwera: " + res.status);
            return res.json();
        })
        .then(data => {
            if (!data.routes || !data.routes[0] || !data.routes[0].geometry) {
                console.error("Błędna odpowiedź z ORS:", data);
                alert("Nie udało się wyznaczyć trasy – odpowiedź serwera była niepoprawna.");
                return;
            }

            const decodedCoords = decodePolyline(data.routes[0].geometry);

            if (routeLine) {
                map.removeLayer(routeLine);
            }

            routeLine = L.polyline(decodedCoords, { color: 'blue', weight: 4 }).addTo(map);
            //map.fitBounds(routeLine.getBounds());
        })
        .catch(err => {
            console.error("Błąd podczas wyznaczania trasy:", err);
            alert("Nie udało się wyznaczyć trasy.");
        });
}


function decodePolyline(encoded) {
    let points = [];
    let index = 0, len = encoded.length;
    let lat = 0, lng = 0;

    while (index < len) {
        let b, shift = 0, result = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        let dlat = ((result & 1) ? ~(result >> 1) : (result >> 1));
        lat += dlat;

        shift = 0;
        result = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        let dlng = ((result & 1) ? ~(result >> 1) : (result >> 1));
        lng += dlng;

        points.push([lat / 1e5, lng / 1e5]);
    }
    // Leaflet oczekuje [lat, lng], więc jest dobrze
    return points;
}
function updatePlaceList(spots) {
    const list = document.getElementById("placeList");
    const filter = document.getElementById("filter").value.toLowerCase();
    list.innerHTML = "";

    spots
        .filter(s =>
            s.name.toLowerCase().includes(filter) ||
            s.description.toLowerCase().includes(filter) ||
            (s.rating && s.rating.toString().includes(filter))
        )
        .forEach(s => {
            const li = document.createElement("li");
            li.style.marginBottom = "10px";
            li.innerHTML = `
                            <strong>${s.name}</strong><br>
                            ${s.description}<br>
                            Ocena: ${'★'.repeat(s.rating || 0)}${'☆'.repeat(5 - (s.rating || 0))}
                            <br><button onclick="map.setView([${s.latitude}, ${s.longitude}], 14)" class="nav-link nav-button-green">Pokaż na mapie</button>
                        `;
            list.appendChild(li);
        });
}


function renderList(filter = '') {
    const list = document.getElementById('placeList');
    list.innerHTML = '';

    places.forEach((place, i) => {
        const match = place.description.toLowerCase().includes(filter) ||
            place.rating.toString().includes(filter);
        if (!filter || match) {
            const li = document.createElement('li');
            li.innerHTML = `<strong>${place.name}</strong> (${place.rating})<br><small>${place.description}</small>`;
            li.style.cursor = 'pointer';
            li.style.marginBottom = '10px';
            li.onclick = () => {
                map.setView([place.lat, place.lng], 15);
                markers[i].openPopup();
                markers[i].setZIndexOffset(1000); // Podbij marker
            };
            list.appendChild(li);
        }
    });
}
document.getElementById('filter').addEventListener('input', e => {
    renderList(e.target.value.toLowerCase());
});

renderList();
function exportMapToPDF() {
    domtoimage.toPng(document.getElementById("map"))
        .then(function (dataUrl) {
            const pdf = new jspdf.jsPDF({
                orientation: "landscape",
                unit: "px",
                format: [document.getElementById("map").offsetWidth, document.getElementById("map").offsetHeight]
            });

            pdf.addImage(dataUrl, 'PNG', 0, 0);
            pdf.save("mapa.pdf");
        })
        .catch(function (error) {
            console.error("Błąd eksportu mapy:", error);
        });
}

document.getElementById("filter").addEventListener("input", () => {
    fetch(`/MushroomSpots/GetAll?ts=${Date.now()}`)
        .then(res => res.json())
        .then(spots => updatePlaceList(spots));
});

map.on('click', function (e) {
    const lat = e.latlng.lat;
    const lng = e.latlng.lng;

    const popup = L.popup()
        .setLatLng(e.latlng)
        .setContent(`
                <form method="post" action="/MushroomSpots/Create">
                    <input name="Name" placeholder="Nazwa" required />
                    <textarea name="Description" placeholder="Opis"></textarea>
                    <input type="hidden" name="Latitude" value="${lat}" />
                    <input type="hidden" name="Longitude" value="${lng}" />
                    <label>Ocena:</label>
                    <div class="star-rating">
                        <input type="radio" name="Rating" value="5" id="star5"><label for="star5">★</label>
                        <input type="radio" name="Rating" value="4" id="star4"><label for="star4">★</label>
                        <input type="radio" name="Rating" value="3" id="star3"><label for="star3">★</label>
                        <input type="radio" name="Rating" value="2" id="star2"><label for="star2">★</label>
                        <input type="radio" name="Rating" value="1" id="star1"><label for="star1">★</label>
                    </div>
                    <button type="submit" class="nav-link nav-button-green">Dodaj</button>
                </form>
            `)
        .openOn(map);
});
loadPins();
// Geolokalizacja użytkownika – niebieska kropka
if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(function (position) {
        const lat = position.coords.latitude;
        const lng = position.coords.longitude;
        userLocation = [lat, lng];
        if (userMarker) {
            map.removeLayer(userMarker);
        }
        userMarker = L.circleMarker([lat, lng], {
            radius: 6,
            fillColor: "#007bff",
            color: "#fff",
            weight: 1,
            opacity: 1,
            fillOpacity: 0.9
        }).addTo(map).bindPopup("Tu jesteś!");


        map.setView([lat, lng], 10);
    }, function (error) {
        console.warn("Nie udało się uzyskać lokalizacji użytkownika:", error.message);
    });
} else {
    alert("Twoja przeglądarka nie wspiera geolokalizacji.");
}
