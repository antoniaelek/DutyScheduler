var DayTheirShift = React.createClass({

	getInitialState: function () {
		return {
			myShifts: [],
			myReplacements: [],
			selectedShift: null
		};
	},

	componentDidMount: function () {
		this.load();
	},

	componentWillReceiveProps: function (newProps) {
		this.load();
	},

	load: function () {
		var day = moment(this.props.day.date);
		var month = day.month();
		var year = day.year();

		API.Ajax("GET", "api/Shift/user/username=" + CurrentUser.username + "&month=" + (month + 1) + "&year=" + year)
		.done(function (response) {
			this.setState({ myShifts: response, selectedShift: "none" });
		}.bind(this))
		.fail(reportError);

		API.Ajax("GET", "api/Replacement/user=" + CurrentUser.username + "&shift=" + this.props.day.shiftId)
		.done(function (response) {
			this.setState({ myReplacements: response });
		}.bind(this))
		.fail(reportError);
	},


	onMySuggestionClicked: function (e) {
		this.setState({ selectedShift: e.target.value });
	},

	removeSuggestion: function (replacementId) {
		API.Ajax("DELETE", "api/Replacement/" + replacementId)
			.done(this.props.onDayUpdate)
			.fail(reportError);
	},

	onAddRequest: function () {
		var date = null;
		if (this.state.selectedShift !== "none")
			date = this.state.selectedShift;

		var data = {
			date: date,
			shiftId: this.props.day.shiftId
		};

		API.Ajax("POST", "api/Replacement", data)
			.done(this.props.onDayUpdate)
			.fail(reportError);
	},

	render: function () {
		var day = this.props.day;

		var assignable = null;
		if (day.isReplaceable) {

			options = null;
			if (this.state.myShifts.length > 0) {
				options = this.state.myShifts.map(function (shift) {
					return <option key={shift.id} value={shift.date }>{moment(shift.date).format("DD. MM. YYYY.")}</option>;
				});
				}

				var myReplacements = this.state.myReplacements.map(function (replacement) {
					return (<li key={replacement.id }>
						<span>{replacement.date == null ? "I'll jump in!" : moment(replacement.date).format("DD. MM. YYYY.")}</span>
						<Icon icon="close" className="removeIcon" onClick={this.removeSuggestion.bind(this, replacement.id) } />
					</li>)
				}.bind(this));

				assignable = (
				<div className="replacementsSuggestions">
					<div>The shift is available for replacement.</div>
					<div>You can request replacements with your shifts:</div>
					<select onChange={this.onMySuggestionClicked} value={this.state.selectedShift}>
						<option key="none" value="none">(No replacement. I'll jump in.)</option>
						{options}
					</select>
						<div className="button" onClick={this.onAddRequest}>Request</div>
					<div>
						<div>These are the shifts you have already suggested as replacements:</div>
						<ul className="myReplacements">{myReplacements}</ul>
					</div>
				</div>
			);
		}

		return (
			<div className="dayMyShift">
				<div>User {this.props.day.scheduled.name} is assigned to this shift.</div>
				<div>{assignable}</div>
			</div>
		);
	}
});