
var days = [
	{
		date: moment("2017-02-01"),
		day: "Srijeda",
		isShort: false,
		isWorking: true,
		preference: null
	},
	{
		date: moment("2017-02-02"),
		day: "Četvrtak",
		isShort: false,
		isWorking: true,
		preference: null
	},
	{
		date: moment("2017-02-03"),
		day: "Petak",
		isShort: true,
		isWorking: true,
		preference: null
	},
	{
		date: moment("2017-02-04"),
		day: "Subota",
		isShort: false,
		isWorking: false,
		preference: null
	},
	{
		date: moment("2017-02-05"),
		day: "Nedjelja",
		isShort: false,
		isWorking: false,
		preference: null
	},
	{
		date: moment("2017-02-06"),
		day: "Ponedjeljak",
		isShort: false,
		isWorking: true,
		preference: null
	}
]

var PreferenceView = React.createClass({

	getInitialState: function () {
		return {
			days: []
		};
	},

	onPreferencesLoaded: function (response) {
		this.setState({ days: response });
	},

	componentDidMount: function () {
		$.get(Routes.GetPreferences + queryString({ userId: localStorage.currentUserId}))
		.done(this.onPreferencesLoaded)
		.fail(reportError);
	},

	render: function () {

		var days = this.state.days.map(function (d) {
			return <PreferenceDay key={d.date} day={d} />
		});

		return (
		<div className="preferenceContainer">
			{days}
		</div>);
	}
});