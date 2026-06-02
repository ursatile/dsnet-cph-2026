// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function connectToSignalR() {
	var conn = new signalR.HubConnectionBuilder().withUrl("/hub").build();
	conn.on("LookThisIsAMagicString", showNotification);
	conn.start().then(function () {
		console.log("Connected to SignalR!");
	}).catch(function (err) {
		console.error("Error connecting to SignalR:", err);
	});
}

function showNotification(user, message) {
	var data = JSON.parse(message);
	var $target = $("#signalr-notifications");
	const $div = $(`
		<div>${data.Make} ${data.Model} (${data.Year}, ${data.Color})<br />
		PRICE: ${data.Price} ${data.CurrencyCode}<br />
		<a href="/vehicles/details/${data.Registration}">click for more!</a>
		</div>`);
	$target.prepend($div);
	$div.css("--background-color", data.Color);
	window.setTimeout(function () {
		$div.fadeOut(2000, function () {
			$div.remove();
		});
	}, 3000);
}

$(document).ready(connectToSignalR);
