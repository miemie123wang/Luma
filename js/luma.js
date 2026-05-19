window.lumaJS = {
    getSunTimes: function (lat, lng) {
        if (typeof SunCalc === 'undefined') {
            throw new Error('LUMA_SUNCALC_UNAVAILABLE');
        }

        const now = new Date();
        const times = SunCalc.getTimes(now, lat, lng);

        if (!times || !times.sunrise || Number.isNaN(times.sunrise.getTime())) {
            throw new Error('LUMA_SUNCALC_INVALID_RESULT');
        }

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
            if (!navigator.geolocation) {
                reject(new Error('LUMA_GEO_UNSUPPORTED'));
                return;
            }

            navigator.geolocation.getCurrentPosition(
                pos => resolve({
                    lat: pos.coords.latitude,
                    lng: pos.coords.longitude,
                    altitude: pos.coords.altitude
                }),
                err => {
                    const code = err.code === err.PERMISSION_DENIED
                        ? 'LUMA_GEO_PERMISSION_DENIED'
                        : err.code === err.POSITION_UNAVAILABLE
                            ? 'LUMA_GEO_POSITION_UNAVAILABLE'
                            : err.code === err.TIMEOUT
                                ? 'LUMA_GEO_TIMEOUT'
                                : 'LUMA_GEO_FAILED';
                    reject(new Error(code));
                },
                { enableHighAccuracy: false, timeout: 10000, maximumAge: 300000 }
            );
        });
    },
    getLocationName: function (lat, lng) {
        if (!window.fetch) {
            return Promise.resolve('');
        }

        return fetch(`https://nominatim.openstreetmap.org/reverse?lat=${encodeURIComponent(lat)}&lon=${encodeURIComponent(lng)}&format=json&zoom=10`, {
            headers: { 'Accept': 'application/json' }
        })
            .then(r => r.ok ? r.json() : null)
            .then(d => {
                const a = d && d.address;
                if (!a) return '';
                return a.city || a.town || a.village || a.county || "";
            })
            .catch(() => "");
    }
};