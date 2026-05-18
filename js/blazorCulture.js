window.blazorCulture = {
  get: function () {
    try {
      return localStorage.getItem('luma_settings') ? JSON.parse(localStorage.getItem('luma_settings')).Language : localStorage.getItem('BlazorCulture');
    } catch (e) { return null; }
  },
  set: function (value, reload) {
    try {
      // also keep a quick key for other consumers
      localStorage.setItem('BlazorCulture', value);
      // keep language inside persisted settings if present
      var raw = localStorage.getItem('luma_settings');
      if (raw) {
        try {
          var obj = JSON.parse(raw);
          obj.Language = value;
          localStorage.setItem('luma_settings', JSON.stringify(obj));
        } catch { }
      }
      if (reload) location.reload();
    } catch (e) { }
  }
};
