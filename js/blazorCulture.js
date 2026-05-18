window.blazorCulture = {
  get: function () {
    try {
      return localStorage.getItem('luma_settings') ? JSON.parse(localStorage.getItem('luma_settings')).Language : localStorage.getItem('BlazorCulture');
    } catch (e) { return null; }
  },
  set: function (value, reload) {
    try {
      localStorage.setItem('BlazorCulture', value);
      var raw = localStorage.getItem('luma_settings');
      var obj = raw ? JSON.parse(raw) : {};
      obj.Language = value;
      localStorage.setItem('luma_settings', JSON.stringify(obj));
      if (reload) location.reload();
    } catch (e) { }
  }
};
