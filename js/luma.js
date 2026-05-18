window.lumaJS = {
    getSunTimes: function (lat, lng) {
        const now = new Date();
        const times = SunCalc.getTimes(now, lat, lng);
        return {
            sunrise: times.sunrise.toISOString(),
            sunriseEnd: times.sunriseEnd.toISOString(),
            goldenHourEnd: times.goldenHourEnd.toISOString(),
            solarNoon: times.solarNoon.toISOString(),
            goldenHour: times.goldenHour.toISOString(),
            sunsetStart: times.sunsetStart.toISOString(),
            sunset: times.sunset.toISOString(),
            dusk: times.dusk.toISOString(),
            nauticalDusk: times.nauticalDusk.toISOString(),
            night: times.night.toISOString(),
            nadir: times.nadir.toISOString(),
            nightEnd: times.nightEnd.toISOString(),
            nauticalDawn: times.nauticalDawn.toISOString(),
            dawn: times.dawn.toISOString(),
            nowUtc: now.toISOString(),
        };
    },
    getCurrentPosition: function () {
        return new Promise((resolve, reject) => {
            navigator.geolocation.getCurrentPosition(
                pos => resolve({
                    lat: pos.coords.latitude,
                    lng: pos.coords.longitude,
                    altitude: pos.coords.altitude
                }),
                err => reject(err.message)
            );
        });
    },
    getLocationName: function (lat, lng) {
        return fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`)
            .then(r => r.json())
            .then(d => {
                const a = d.address;
                return a.city || a.town || a.village || a.county || "";
            })
            .catch(() => "");
    }
};