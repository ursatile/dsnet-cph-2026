// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function connectToSignalR() {
	var conn = new signalR.HubConnectionBuilder().withUrl("/hub").build();
	conn.on("LookThisIsAMagicString", function (user, message) {
		console.log(user);
		console.log(message);
	});
	conn.start().then(function () {
		console.log("Connected to SignalR!");
	}).catch(function (err) {
		console.error("Error connecting to SignalR:", err);
	});
}

$(document).ready(connectToSignalR);
