var DayMyShift = React.createClass({

	getInitialState: function () {
		var state = {
			requestId: null
		};

		if (this.props.day.replacementRequests.length > 0)
			state.requestId = this.props.day.replacementRequests[0].id;

		return state;
	},

	setReplaceable: function (replaceable) {
		API.Ajax("PUT", "api/Shift/" + this.props.day.shiftId, {
			setReplaceable: replaceable
		})
		.done(this.props.onDayUpdate)
		.fail(reportError);
	},

	onAcceptRequest: function () {
		console.log(this);
		API.Ajax("POST", "api/Replacement/accept", { requestId: this.state.requestId })
		.done(this.props.onDayUpdate)
		.fail(reportError);
	},

	onReplacementRequestChanged: function (e) {
		this.setState({ requestId: e.target.value }, function () { console.log(this.state); });
	},

	render: function () {
		var day = this.props.day;
		var replaceable = null;
		if (day.isReplaceable) {

			var availableReplacements = <div>There are no replacement requests.</div>;

			var acceptReplacementButton = null;
			if (day.replacementRequests.length > 0) {
				var options = day.replacementRequests.map(function (rr) {
					return <option key={rr.id} value={rr.id }>{rr.user.name} {rr.date == null ? "offers to jump in!" : "| " + moment(rr.date).format("DD. MM. YYYY.")}</option>;
				});

				availableReplacements = (<select value={this.state.requestId} onChange={this.onReplacementRequestChanged }>{options}</select>);
				acceptReplacementButton = <div className="button" onClick={this.onAcceptRequest }>Accept selected replacement request</div>;
			}

			replaceable = (
				<div>
					<div>The shift is available for replacement.</div>
					<div>{availableReplacements}</div>
					<div>{acceptReplacementButton}</div>
					<div className="button" onClick={this.setReplaceable.bind(this, false)}>Cancel</div>
				</div>
			);
		}
		else {
			replaceable = (
				<div>
					<div>Click the button if you want this shift to be assignable to another user.</div>
					<div className="button" onClick={this.setReplaceable.bind(this, true)}>Make assignable</div>
				</div>
			);
		}

		return (
			<div className="dayMyShift">
				<div><Icon icon="user-circle-o" />This shift is assigned to me.</div>
				<div className="makeAssignable">{replaceable}</div>
			</div>
		);
	}
});