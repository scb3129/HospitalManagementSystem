// auth.js
(function () {
  // redirect to login if no token
  function requireAuth() {
    const token = localStorage.getItem("hms_jwt");
    if (!token) {
      window.location.href = "login.html";
      return false;
    }
    return true;
  }

  // call this on protected pages
  window.hmsRequireAuth = requireAuth;

  // helper to attach Authorization header
  window.hmsFetch = async function (url, options = {}) {
    const token = localStorage.getItem("hms_jwt");
    options.headers = options.headers || {};
    if (token) options.headers["Authorization"] = "Bearer " + token;
    return fetch(url, options);
  };

  // logout
  window.hmsLogout = function () {
    localStorage.removeItem("hms_jwt");
    localStorage.removeItem("hms_email");
    window.location.href = "login.html";
  };

})();
