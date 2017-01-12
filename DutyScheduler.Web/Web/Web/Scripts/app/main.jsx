
localStorage.currentUserId = 11; // Remove

var CurrentUser = null;

var URL = function (rest) {
	return "http://localhost:56536/" + rest;
}

var queryString = function (obj) {
	var str = [];
	for (var p in obj)
		if (obj.hasOwnProperty(p)) {
			var result = obj[p] === undefined || obj[p] === null ? "%00" : encodeURIComponent(obj[p]);
			str.push(encodeURIComponent(p) + "=" + result);
		}
	return str.join("&");
}

var API = {
	Ajax: function (method, url, data) {
		return $.ajax({
			url: URL(url),
			method: method,
			data: JSON.stringify(data),
			contentType: "application/json",
			xhrFields: {
				withCredentials: true
			}
		});
	}
};

function reportError(response) {
	console.log(response);
	var rObject = response.responseJSON;
	var errorsObject = rObject.errors;
	alert(Object.keys(errorsObject).map(function (e) { return errorsObject[e]; }).join("\n"));
}

ReactDOM.render(<App />, document.getElementById("app"));