
var PreferenceDay = React.createClass({

	getInitialState: function() {
		return {
			preference: null
		};
	},

	onPreferenceClicked: function (pref) {
		console.log(this.props.day.date);
		var data = { userId: localStorage.currentUserId, date: this.props.day.date.format("YYYY-MM-DD"), preference: pref };

		$.post(Routes.SetPreference, data)
		.done(function (response) {
			if (response === true)
				this.setState({ preference: pref });
			else
				alert(response);
		}.bind(this))
		.fail(reportError);
	},

	render: function () {

		var day = this.props.day;
		var className = "dayContainer";
		if (day.isShort) {
			className += " short";
		}

		if (!day.isWorking) {
			className += " weekend";
		}
		
		var checkButtonClassName = "check button" + (this.state.preference === true ? " selected" : "");
		var defaultButtonClassName = "default button" + (this.state.preference === null ? " selected" : "");
		var uncheckButtonClassName = "uncheck button" + (this.state.preference === false ? " selected" : "");

		var buttons = <div></div>;

		if (day.isWorking) {
			buttons = (
			<div className="buttonsContainer">
				<Icon onClick={this.onPreferenceClicked.bind(this, true)} className={checkButtonClassName} icon="thumbs-up"/>
				<Icon onClick={this.onPreferenceClicked.bind(this, null)} className={defaultButtonClassName} icon="circle-o"/>
				<Icon onClick={this.onPreferenceClicked.bind(this, false)} className={uncheckButtonClassName} icon="thumbs-down"/>
			</div>
			);
		}

		return (
			<div className={className }>
				<div className="day">
					{day.date.format("DD-MM-YYYY")} {day.day}
				</div>
				{buttons}
			</div>
		);
	}
});