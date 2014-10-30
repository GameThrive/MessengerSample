var express = require('express')
var app = express()
var https = require("https");

var options = {
	host: 'gamethrive.com',
	path: '/api/v1/players?app_id=abfa40c2-606a-11e4-a646-23007197f2dd&offset=0&limit=300',
	method: 'GET',
	headers: {"Authorization":"Basic YWJmYTQxNTgtNjA2YS0xMWU0LWE2NDctYjc2NjFmN2MzYjcz"}
};

app.get('/devices', function (req, res) {
	var req = https.request(options, function(gtres) {
		gtres.on('data', function (chunk) {
			res.set({"Content-Type":"application/json"});
			devices = [];
			players = JSON.parse(chunk)["players"]
			for(var i = 0; i < players.length; i++) {
				player = players[i]
				if(players["identifier"] != "") {
					devices.push({
						"created_at":player["created_at"],
						"device_model":player["device_model"],
						"device_type": (player["device_type"] == 1 ? "Android" : "iOS"),
						"identifier": player["identifier"]
					});
				}
			}

			res.json(devices)
		});
	});

	req.on('error', function(e) {
		console.log('Problem with request: ' + e.message);
		res.status(500).json({error: e.message})
	});

	req.end();
})

var server = app.listen(3100, function () {

	var host = server.address().address
	var port = server.address().port

	console.log('Listening at http://%s:%s', host, port)

})