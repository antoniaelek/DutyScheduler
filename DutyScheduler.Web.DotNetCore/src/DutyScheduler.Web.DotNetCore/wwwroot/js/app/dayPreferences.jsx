
var DayPreferences = React.createClass({

	propTypes: function(){
		onDayUpdate: React.PropTypes.func.isRequired
	},

	getInitialState: function () {
		return {
			day: this.props.day,
			prefered: this.props.day.isPrefered,
			isLoading: false
		};
	},

	componentWillReceiveProps: function (newProps) {
		this.setState({
			day: newProps.day,
			prefered: newProps.day.isPrefered,
			isLoading: false
		});
	},

	onClick: function (pref) {

		if (this.props.day.isPrefered === pref)
			return;

		API.Ajax("PUT", "api/Preference", { date: this.props.day.date.format("YYYY-MM-DD"), setPrefered: pref })
			.done(this.props.onDayUpdate)
			.fail(reportError);

	},

	render: function () {

		var upClassName = "iconContainer" + (this.state.prefered === true ? " prefered" : "");
		var downClassName = "iconContainer" + (this.state.prefered === false ? " notPrefered" : "");

		var buttons = [
			<div key="F" onClick={this.onClick.bind(this, true)} className={upClassName}><Icon icon="thumbs-up" />I prefer to work on this day</div>,
			<div key="D" onClick={this.onClick.bind(this, null)} className="iconContainer"><Icon icon="circle-thin" />I don't care if I work on this day</div>,
			<div key="T" onClick={this.onClick.bind(this, false)} className={downClassName}><Icon icon="thumbs-down" />I would rather not work this day</div>
		];

		if (this.state.isLoading)
			buttons = <div>Loading...</div>;

		return (
			<div className="dayPreferences">
				{buttons}
			</div>
		);
	}
});