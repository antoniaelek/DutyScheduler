
var data = [
	{
		date: moment("2017-01-03"),
		day: "Utorak",
		user: {
			id: 11,
			name: "Maja Antolić",
			phone: "0981234569",
			room: "C07-16"
		}
	},
	{
		date: moment("2017-01-04"),
		day: "Srijeda",
		user: {
			id: 12,
			name: "Toni Vuk",
			phone: "0913546833",
			room: "C07-21"
		}
	},
	{
		date: moment("2017-01-05"),
		day: "Četvrtak",
		user: {
			id: 22,
			name: "Marko Lonić",
			phone: "0913481684",
			room: "C07-18"
		}
	}
];

var ScheduleView = React.createClass({

	getInitialState: function () {
		return {
			shifts: data
		};
	},

	render: function () {

		var shifts = this.state.shifts.map(function (s) {
			return <Shift key={s.date} shift={s} />
		});

		return (
			<div>
				{shifts}
			</div>
		);
	}
});